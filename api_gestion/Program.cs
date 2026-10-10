using System.Text.Json;
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
                    : $"{JsonNamingPolicy.SnakeCaseLower.ConvertName(par.Key)}: {error.ErrorMessage}"))
            .Distinct()
            .ToArray();
        return new UnprocessableEntityObjectResult(new { mensaje = "Datos inválidos.", detalle });
    });

// 2. Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "API Gestión Profesoral", Version = "v1", Description = "CRUD de las 5 tablas independientes (v1)" });
});

// 3. Inyección de dependencias: cada interfaz con su implementación
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

var app = builder.Build();

app.UseMiddleware<ManejadorErrores>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "API Gestión Profesoral v1");
        c.RoutePrefix = string.Empty; // Swagger en la raíz: http://localhost:8074/
    });
}

app.UseAuthorization();
app.MapControllers();
app.Run();
