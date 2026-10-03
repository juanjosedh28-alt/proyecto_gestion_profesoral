using ApiGestion.Repositorios;
using ApiGestion.Servicios;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// 1. Registrar los controladores de la API
builder.Services.AddControllers();

// 2. Configurar Swagger para la documentación interactiva
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo 
    { 
        Title = "API Gestión Profesoral", 
        Version = "v1",
        Description = "Ejemplo de referencia - Módulo Gestión Profesoral v1" 
    });
});

// 3. Registrar Inyección de Dependencias (Conectamos las interfaces con sus clases reales)
builder.Services.AddScoped<IRepositorioPrograma, RepositorioProgramaSqlServer>();
builder.Services.AddScoped<IServicioPrograma, ServicioPrograma>();

var app = builder.Build();

// 4. Configurar el entorno de desarrollo y Swagger
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "API Gestión Profesoral v1");
        c.RoutePrefix = string.Empty; // Hace que Swagger aparezca directamente en la raíz (http://localhost:puerto/)
    });
}

app.UseAuthorization();

app.MapControllers();

app.Run();