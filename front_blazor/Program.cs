builder.Services.AddScoped(sp => new HttpClient 
{ 
    BaseAddress = new Uri("http://localhost:8074/") // URL de la API de gestión
});
builder.Services.AddScoped<ProgramaService>();
