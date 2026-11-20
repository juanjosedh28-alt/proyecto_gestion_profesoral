using front_razor.Clientes;

var builder = WebApplication.CreateBuilder(args);

// Los mensajes que pone ASP.NET cuando un dato no se puede convertir (por ejemplo,
// un desplegable sin elegir en un campo numérico) vienen en inglés: «The value '' is
// invalid.». Aquí se cambian por mensajes en castellano, para que ninguna pantalla
// muestre texto técnico ni en otro idioma (criterio 11 de la v2).
builder.Services.AddRazorPages().AddMvcOptions(o =>
{
    var m = o.ModelBindingMessageProvider;
    m.SetValueMustNotBeNullAccessor(_ => "Este campo es obligatorio.");
    m.SetMissingBindRequiredValueAccessor(_ => "Este campo es obligatorio.");
    m.SetMissingKeyOrValueAccessor(() => "Este campo es obligatorio.");
    m.SetAttemptedValueIsInvalidAccessor((valor, _) => $"El valor «{valor}» no es válido.");
    m.SetNonPropertyAttemptedValueIsInvalidAccessor(valor => $"El valor «{valor}» no es válido.");
    m.SetUnknownValueIsInvalidAccessor(_ => "El valor escrito no es válido.");
    m.SetNonPropertyUnknownValueIsInvalidAccessor(() => "El valor escrito no es válido.");
    m.SetValueIsInvalidAccessor(valor => $"El valor «{valor}» no es válido.");
    m.SetValueMustBeANumberAccessor(_ => "Escriba un número.");
    m.SetNonPropertyValueMustBeANumberAccessor(() => "Escriba un número.");
});

// El front llama a la API desde el servidor. En Docker: ApiGestion__BaseUrl=http://api-gestion:8074
var urlApi = builder.Configuration["ApiGestion:BaseUrl"] ?? "http://localhost:8074";
builder.Services.AddHttpClient<ClienteAreaConocimiento>(c => c.BaseAddress = new Uri(urlApi));
builder.Services.AddHttpClient<ClienteTerminoClave>(c => c.BaseAddress = new Uri(urlApi));
builder.Services.AddHttpClient<ClienteLineaInvestigacion>(c => c.BaseAddress = new Uri(urlApi));
builder.Services.AddHttpClient<ClientePrograma>(c => c.BaseAddress = new Uri(urlApi));
builder.Services.AddHttpClient<ClienteRed>(c => c.BaseAddress = new Uri(urlApi));

// v2: las 11 tablas con clave foránea
builder.Services.AddHttpClient<ClienteDocente>(c => c.BaseAddress = new Uri(urlApi));
builder.Services.AddHttpClient<ClienteEstudio>(c => c.BaseAddress = new Uri(urlApi));
builder.Services.AddHttpClient<ClienteEvaluacion>(c => c.BaseAddress = new Uri(urlApi));
builder.Services.AddHttpClient<ClienteExperiencia>(c => c.BaseAddress = new Uri(urlApi));
builder.Services.AddHttpClient<ClienteReconocimiento>(c => c.BaseAddress = new Uri(urlApi));
builder.Services.AddHttpClient<ClienteDocenteDepartamento>(c => c.BaseAddress = new Uri(urlApi));
builder.Services.AddHttpClient<ClienteRedDocente>(c => c.BaseAddress = new Uri(urlApi));
builder.Services.AddHttpClient<ClienteInteresFuturo>(c => c.BaseAddress = new Uri(urlApi));
builder.Services.AddHttpClient<ClienteEstudioAc>(c => c.BaseAddress = new Uri(urlApi));
builder.Services.AddHttpClient<ClienteApoyoProfesoral>(c => c.BaseAddress = new Uri(urlApi));
builder.Services.AddHttpClient<ClienteBeca>(c => c.BaseAddress = new Uri(urlApi));

var app = builder.Build();

app.UseStaticFiles();
app.MapRazorPages();
app.Run();
