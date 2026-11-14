using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using ApiGestion.Autorizacion;
using ApiGestion.Middleware;
using ApiGestion.Repositorios;
using ApiGestion.Servicios;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Dapper: mapea columnas snake_case (fecha_creacion) a propiedades PascalCase (FechaCreacion)
Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;

// 1. Controladores: JSON en snake_case y errores de validación (422) con el formato { mensaje, detalle }
builder.Services.AddControllers()
    .AddJsonOptions(o => o.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower)
    .ConfigureApiBehaviorOptions(o => o.InvalidModelStateResponseFactory = contexto =>
    {
        var detalle = contexto.ModelState
            .Where(par => par.Value?.Errors.Count > 0)
            .SelectMany(par => par.Value!.Errors.Select(error =>
                string.IsNullOrEmpty(error.ErrorMessage) || par.Key.StartsWith("$") || par.Key == string.Empty || par.Key == "peticion"
                    ? "El cuerpo de la petición no es un JSON válido."
                    : $"{NombreDelCampo(par.Key)}: {error.ErrorMessage}"))
            .Distinct()
            .ToArray();
        return new UnprocessableEntityObjectResult(new { mensaje = "Datos inválidos.", detalle });
    });

// 2. Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "API Gestión Profesoral", Version = "v3", Description = "CRUD de las 16 tablas del módulo por procedimientos almacenados, con inicio de sesión (JWT) y permisos por rol" });

    // v3 — EL BOTÓN «Authorize» DE SWAGGER. Sin esto, desde Swagger todo
    // respondería 401. Cómo se usa: POST /api/login → copiar el "token" →
    // botón Authorize → pegar SOLO el token (la palabra Bearer la pone Swagger).
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Pegue solo el token que devuelve POST /api/login."
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" } },
            Array.Empty<string>()
        }
    });
});

// 2b. v3 — AUTENTICACIÓN con JWT.
//     La clave que firma los tokens es un SECRETO: llega por la variable de
//     entorno Jwt__Key (docker-compose la toma de JWT_KEY en el archivo .env).
//     Si falta o es corta, la API NO arranca: mejor un error claro al iniciar
//     que una API que acepta tokens firmados con una clave débil.
var configuracionJwt = new ConfiguracionJwt();
builder.Configuration.GetSection("Jwt").Bind(configuracionJwt);
if (string.IsNullOrWhiteSpace(configuracionJwt.Key) || configuracionJwt.Key.Length < 32)
    throw new InvalidOperationException(
        "Falta la clave Jwt:Key o tiene menos de 32 caracteres. Póngala en el archivo .env (JWT_KEY) y reinicie con docker compose up -d.");
builder.Services.AddSingleton(configuracionJwt);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opciones =>
    {
        // Los nombres de los claims se leen tal cual vienen en el token ("name", "role").
        opciones.MapInboundClaims = false;
        opciones.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = configuracionJwt.Issuer,
            ValidateAudience = true,
            ValidAudience = configuracionJwt.Audience,
            ValidateIssuerSigningKey = true,   // la FIRMA: un token alterado no pasa
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuracionJwt.Key)),
            ValidateLifetime = true,           // un token vencido no pasa
            ClockSkew = TimeSpan.FromMinutes(1),
            NameClaimType = "name",            // User.Identity.Name = el correo
            RoleClaimType = "role"
        };

        // El 401 con el mismo formato { mensaje, detalle } de toda la API, en castellano.
        opciones.Events = new JwtBearerEvents
        {
            OnChallenge = async contexto =>
            {
                contexto.HandleResponse();
                var mensaje = contexto.AuthenticateFailure is SecurityTokenExpiredException
                    ? "La sesión venció. Inicie sesión de nuevo."
                    : "Debe iniciar sesión: la petición no trae un token válido.";
                contexto.Response.StatusCode = 401;
                await contexto.Response.WriteAsJsonAsync(new { mensaje, detalle = Array.Empty<string>() });
            }
        };
    });

// 2c. v3 — AUTORIZACIÓN. La política de reserva (FallbackPolicy) exige sesión
//     en TODO endpoint que no diga lo contrario: si alguien escribe un
//     controlador nuevo y olvida [Authorize], igual queda protegido.
//     El único que dice lo contrario es POST /api/login ([AllowAnonymous]).
builder.Services.AddAuthorization(o =>
    o.FallbackPolicy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build());

// 3. Inyección de dependencias: cada interfaz con su implementación

// v2: el único que habla con SQL Server. Todos los repositorios lo usan para llamar procedimientos.
builder.Services.AddScoped<IEjecutorProcedimientos, EjecutorProcedimientos>();

// v3: control de acceso
builder.Services.AddScoped<IRepositorioAcceso, RepositorioAcceso>();
builder.Services.AddScoped<IServicioSesion, ServicioSesion>();
builder.Services.AddScoped<IRepositorioUsuario, RepositorioUsuario>();
builder.Services.AddScoped<IServicioUsuario, ServicioUsuario>();
builder.Services.AddScoped<IRepositorioRol, RepositorioRol>();
builder.Services.AddScoped<IServicioRol, ServicioRol>();
builder.Services.AddScoped<IRepositorioPermiso, RepositorioPermiso>();

// v1: las 5 tablas sin clave foránea
builder.Services.AddScoped<IRepositorioAreaConocimiento, RepositorioAreaConocimiento>();
builder.Services.AddScoped<IServicioAreaConocimiento, ServicioAreaConocimiento>();
builder.Services.AddScoped<IRepositorioTerminoClave, RepositorioTerminoClave>();
builder.Services.AddScoped<IServicioTerminoClave, ServicioTerminoClave>();
builder.Services.AddScoped<IRepositorioLineaInvestigacion, RepositorioLineaInvestigacion>();
builder.Services.AddScoped<IServicioLineaInvestigacion, ServicioLineaInvestigacion>();
builder.Services.AddScoped<IRepositorioPrograma, RepositorioPrograma>();
builder.Services.AddScoped<IServicioPrograma, ServicioPrograma>();
builder.Services.AddScoped<IRepositorioRed, RepositorioRed>();
builder.Services.AddScoped<IServicioRed, ServicioRed>();

// v2: las 11 tablas con clave foránea
builder.Services.AddScoped<IRepositorioDocente, RepositorioDocente>();
builder.Services.AddScoped<IServicioDocente, ServicioDocente>();
builder.Services.AddScoped<IRepositorioEstudiosRealizados, RepositorioEstudiosRealizados>();
builder.Services.AddScoped<IServicioEstudiosRealizados, ServicioEstudiosRealizados>();
builder.Services.AddScoped<IRepositorioEvaluacionDocente, RepositorioEvaluacionDocente>();
builder.Services.AddScoped<IServicioEvaluacionDocente, ServicioEvaluacionDocente>();
builder.Services.AddScoped<IRepositorioReconocimiento, RepositorioReconocimiento>();
builder.Services.AddScoped<IServicioReconocimiento, ServicioReconocimiento>();
builder.Services.AddScoped<IRepositorioExperiecia, RepositorioExperiecia>();
builder.Services.AddScoped<IServicioExperiecia, ServicioExperiecia>();
builder.Services.AddScoped<IRepositorioApoyoProfesoral, RepositorioApoyoProfesoral>();
builder.Services.AddScoped<IServicioApoyoProfesoral, ServicioApoyoProfesoral>();
builder.Services.AddScoped<IRepositorioBeca, RepositorioBeca>();
builder.Services.AddScoped<IServicioBeca, ServicioBeca>();
builder.Services.AddScoped<IRepositorioDocenteDepartamento, RepositorioDocenteDepartamento>();
builder.Services.AddScoped<IServicioDocenteDepartamento, ServicioDocenteDepartamento>();
builder.Services.AddScoped<IRepositorioRedDocente, RepositorioRedDocente>();
builder.Services.AddScoped<IServicioRedDocente, ServicioRedDocente>();
builder.Services.AddScoped<IRepositorioInteresesFuturos, RepositorioInteresesFuturos>();
builder.Services.AddScoped<IServicioInteresesFuturos, ServicioInteresesFuturos>();
builder.Services.AddScoped<IRepositorioEstudioAc, RepositorioEstudioAc>();
builder.Services.AddScoped<IServicioEstudioAc, ServicioEstudioAc>();

var app = builder.Build();

// 4. Manejo de errores. EL ORDEN IMPORTA: la excepción sube desde el controlador
//    hacia afuera, así que pasa primero por el middleware registrado DE ÚLTIMO.
//    - ManejadorErroresSql (v2): traduce los errores de la base (50001..50004, 547, 2627...) a 404/409/422.
//    - ManejadorErrores (v1): traduce las excepciones de los servicios y, si nada más la atendió, responde 500.
app.UseMiddleware<ManejadorErrores>();
app.UseMiddleware<ManejadorErroresSql>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "API Gestión Profesoral");
        c.RoutePrefix = string.Empty; // Swagger en la raíz: http://localhost:8074/
    });
}

// v3: primero AUTENTICAR (¿quién es? lee y valida el token) y después
// AUTORIZAR (¿puede?). En el orden contrario, la autorización no sabría
// quién pregunta y respondería 401 a todos.
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();

// Convierte la clave de un error de validación en un nombre legible.
//   "FechaNacimiento"  -> "fecha_nacimiento"
//   "[2].Titulo"       -> "renglón 3, titulo"   (envío de varios renglones: se cuenta desde 1)
static string NombreDelCampo(string clave)
{
    var renglon = Regex.Match(clave, @"\[(\d+)\]\.?(.*)$");
    if (!renglon.Success) return JsonNamingPolicy.SnakeCaseLower.ConvertName(clave);

    var numero = int.Parse(renglon.Groups[1].Value) + 1;
    var campo = renglon.Groups[2].Value;
    return campo.Length == 0 ? $"renglón {numero}" : $"renglón {numero}, {JsonNamingPolicy.SnakeCaseLower.ConvertName(campo)}";
}
