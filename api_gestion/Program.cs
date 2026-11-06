using System.Text.Json;
using System.Text.RegularExpressions;
using ApiGestion.Middleware;
using ApiGestion.Repositorios;
using ApiGestion.Servicios;
using Microsoft.AspNetCore.Mvc;
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
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "API Gestión Profesoral", Version = "v2", Description = "CRUD de las 16 tablas del módulo, todo por procedimientos almacenados (v1 + v2)" });
});

// 3. Inyección de dependencias: cada interfaz con su implementación

// v2: el único que habla con SQL Server. Todos los repositorios lo usan para llamar procedimientos.
builder.Services.AddScoped<IEjecutorProcedimientos, EjecutorProcedimientos>();

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
