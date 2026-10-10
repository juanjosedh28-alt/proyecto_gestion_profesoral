using System.Text.Json;
using ApiGestion.Middleware;
using ApiGestion.Repositorios;
using ApiGestion.Servicios;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.UseUrls("http://0.0.0.0:8074");

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;

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

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "API Gestión Profesoral", Version = "v1" });
});

// Inyección de dependencias corregida
builder.Services.AddScoped<IRepositorioAreaConocimiento, RepositorioAreaConocimiento>();
builder.Services.AddScoped<IServicioAreaConocimiento, ServicioAreaConocimiento>();
builder.Services.AddScoped<IRepositorioTerminoClave, RepositorioTerminoClave>();
builder.Services.AddScoped<IServicioTerminoClave, ServicioTerminoClave>();
builder.Services.AddScoped<IRepositorioLineaInvestigacion, RepositorioLineaInvestigacion>();
builder.Services.AddScoped<IServicioLineaInvestigacion, ServicioLineaInvestigacion>();
builder.Services.AddScoped<IRepositorioPrograma, RepositorioProgramaSqlServer>();
builder.Services.AddScoped<IServicioPrograma, ServicioPrograma>();
builder.Services.AddScoped<IRepositorioRed, RepositorioRed>();
builder.Services.AddScoped<IServicioRed, ServicioRed>();

var app = builder.Build();

app.UseCors("AllowAll");

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "API Gestión Profesoral v1");
    c.RoutePrefix = "swagger";
});

app.UseAuthorization();
app.MapControllers();

app.Run();
