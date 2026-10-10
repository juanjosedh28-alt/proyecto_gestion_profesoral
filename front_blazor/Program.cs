using FrontBlazor;
using FrontBlazor.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents().AddInteractiveServerComponents();

// URL de la API: dentro de Docker usa el nombre del servicio; en local, localhost.
// Se puede sobreescribir con la variable de entorno ApiUrl.
var enDocker = Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") == "true";
var apiUrl = builder.Configuration["ApiUrl"]
    ?? (enDocker ? "http://api-gestion:8074/" : "http://localhost:8074/");

builder.Services.AddHttpClient<ProgramaService>(c => c.BaseAddress = new Uri(apiUrl));

var app = builder.Build();

app.UseStaticFiles();
app.UseAntiforgery();
app.MapRazorComponents<App>().AddInteractiveServerRenderMode();

app.Run();
