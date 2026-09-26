using ApiGestion.Repositorios;
using ApiGestion.Servicios;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Inyección de dependencias por capas
builder.Services.AddScoped<IRepositorioPrograma, RepositorioProgramaSqlServer>();
builder.Services.AddScoped<IServicioPrograma, ServicioPrograma>();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseAuthorization();
app.MapControllers();

app.Run();