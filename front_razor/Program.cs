using front_razor.Clientes;
using front_razor.Infraestructura;

var builder = WebApplication.CreateBuilder(args);

// Los mensajes que pone ASP.NET cuando un dato no se puede convertir (por ejemplo,
// un desplegable sin elegir en un campo numérico) vienen en inglés: «The value '' is
// invalid.». Aquí se cambian por mensajes en castellano, para que ninguna pantalla
// muestre texto técnico ni en otro idioma (criterio 11 de la v2).
// (v3) Antes de CADA página corre FiltroSesion: sin sesión → a /Sesion/Iniciar.
builder.Services.AddRazorPages().AddMvcOptions(o =>
{
    o.Filters.Add<FiltroSesion>();

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

// (v3) La SESIÓN del servidor: aquí vive el token. El navegador solo guarda
// una cookie HttpOnly con el id de la sesión, que un script no puede leer.
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(o =>
{
    o.Cookie.Name = "gestion.sesion";
    o.Cookie.HttpOnly = true;
    o.Cookie.IsEssential = true;
    o.IdleTimeout = TimeSpan.FromMinutes(60);   // igual que la duración del token
});
builder.Services.AddHttpContextAccessor();

// (v3) ManejadorToken le pone «Authorization: Bearer <token>» a cada petición a la API.
builder.Services.AddTransient<ManejadorToken>();

// El front llama a la API desde el servidor. En Docker: ApiGestion__BaseUrl=http://api-gestion:8074
var urlApi = builder.Configuration["ApiGestion:BaseUrl"] ?? "http://localhost:8074";

// Registra un cliente HTTP con la dirección de la API y el manejador del token.
void Cliente<T>() where T : class =>
    builder.Services.AddHttpClient<T>(c => c.BaseAddress = new Uri(urlApi)).AddHttpMessageHandler<ManejadorToken>();

// v1: las 5 tablas sin clave foránea
Cliente<ClienteAreaConocimiento>();
Cliente<ClienteTerminoClave>();
Cliente<ClienteLineaInvestigacion>();
Cliente<ClientePrograma>();
Cliente<ClienteRed>();

// v2: las 11 tablas con clave foránea
Cliente<ClienteDocente>();
Cliente<ClienteEstudio>();
Cliente<ClienteEvaluacion>();
Cliente<ClienteExperiencia>();
Cliente<ClienteReconocimiento>();
Cliente<ClienteDocenteDepartamento>();
Cliente<ClienteRedDocente>();
Cliente<ClienteInteresFuturo>();
Cliente<ClienteEstudioAc>();
Cliente<ClienteApoyoProfesoral>();
Cliente<ClienteBeca>();

// v3: control de acceso
Cliente<ClienteSesion>();
Cliente<ClienteUsuario>();
Cliente<ClienteRol>();
Cliente<ClientePermiso>();

var app = builder.Build();

app.UseStaticFiles();

// (v3) Una respuesta 403/404 sin cuerpo se vuelve a ejecutar como /Error/403,
// /Error/404…: la persona ve una página con el menú, y el código sigue siendo 403.
// Va ANTES de UseRouting para que la nueva ejecución encuentre la página /Error.
app.UseStatusCodePagesWithReExecute("/Error/{0}");
app.UseRouting();

app.UseSession();
app.UseMiddleware<ManejadorAcceso>();   // 401 → iniciar sesión otra vez · 403 → «No tiene permiso»

app.MapRazorPages();
app.Run();
