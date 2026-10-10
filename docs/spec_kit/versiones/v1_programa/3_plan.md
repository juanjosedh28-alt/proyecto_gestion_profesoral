# Plan de Arquitectura — Gestión Profesoral v1 (`3_plan.md`)

> **Documento 3 de 8 del spec kit raíz.**
> Define **CÓMO** construir lo especificado en `2_spec.md`, bajo los principios de `1_constitution.md`.
> Orden de lectura: `1_constitution` → `2_spec` → `3_plan` → `4_research` → `5_data_model` → `6_contracts` → `7_quickstart` → `8_tasks`.

---

## 1. Estructura de archivos del proyecto

```text
gestion_profesoral/
├── docker-compose.yml
├── README.md
├── .gitignore                       # bin/ obj/ .vs/
├── db/
│   └── sqlserver/
│       ├── gestion_profesoral.sql   # esquema (19 tablas) + 218 áreas de conocimiento
│       └── init.sh                  # crea gestion_local y ejecuta el .sql solo la primera vez
├── docs/                            # spec kit (los 8 documentos)
├── api_gestion/                     # API ASP.NET Core (.NET 10)
│   ├── Dockerfile
│   ├── api_gestion.csproj
│   ├── appsettings.json
│   ├── Program.cs
│   ├── Controllers/                 # un controlador por entidad (5)
│   │   └── ProgramaController.cs ...
│   ├── Servicios/                   # interfaz + clase en el mismo archivo (5)
│   │   └── ServicioPrograma.cs ...
│   ├── Repositorios/                # interfaz + clase en el mismo archivo (5)
│   │   └── RepositorioProgramaSqlServer.cs ...
│   ├── Datos/
│   │   └── GestionDbContext.cs
│   ├── Modelos/                     # entidades (5): Programa.cs, Red.cs, TerminoClave.cs ...
│   ├── Peticiones/                  # DTOs de entrada: ProgramaCrear.cs, ProgramaActualizar.cs ...
│   ├── Respuestas/                  # DTOs de salida: ProgramaRespuesta.cs, RedRespuesta.cs ...
│   ├── Excepciones/                 # ExcepcionNegocio, ExcepcionNoEncontrada, ExcepcionConflicto, ExcepcionValidacion
│   └── Infraestructura/             # RespuestaError.cs, ManejadorExcepciones.cs, FabricaErrores.cs
├── front_razor/                     # Front Razor Pages (.NET 10)
│   ├── Dockerfile
│   ├── front_razor.csproj
│   ├── appsettings.json
│   ├── Program.cs
│   ├── Clientes/                    # ClienteApiBase + un cliente por entidad (5) con su interfaz
│   ├── Modelos/                     # vistas (respuestas) y formularios por entidad
│   ├── Infraestructura/             # ErrorApiException.cs, RespuestaError.cs
│   └── Pages/
│       ├── Index.cshtml             # inicio
│       ├── Shared/_Layout.cshtml    # barra de navegación + zona de mensajes
│       ├── AreasConocimiento/       # Index, Detalle
│       ├── TerminosClave/           # Index, Crear, Editar
│       ├── LineasInvestigacion/     # Index, Crear, Editar
│       ├── Programas/               # Index, Crear, Editar
│       └── Redes/                   # Index, Crear, Editar
└── pruebas/
    └── api_gestion.Pruebas/         # xUnit + EF Core InMemory (prueba de capas)
```

Las pruebas viven **fuera** de `api_gestion/` para que el `.csproj` de la API no las compile por accidente.

Paquetes NuGet:

| Proyecto | Paquetes |
|---|---|
| `api_gestion` | `Microsoft.EntityFrameworkCore.SqlServer`, `Swashbuckle.AspNetCore` |
| `front_razor` | ninguno adicional (Razor Pages ya trae todo; Bootstrap viene en `wwwroot/lib`) |
| `api_gestion.Pruebas` | `Microsoft.EntityFrameworkCore.InMemory`, `xunit`, `xunit.runner.visualstudio`, `Microsoft.NET.Test.Sdk`; referencia al proyecto `api_gestion` |

---

## 2. Capas de la API (C#)

El flujo es siempre `Controller → Servicio → Repositorio → DbContext → SQL Server`.

| Capa | Responsabilidad |
|---|---|
| `Controllers/XxxController` | Traduce HTTP a llamadas al servicio y devuelve JSON con el código correcto (`201` + `CreatedAtAction`, `204`, etc.). Sin reglas de negocio ni acceso a datos. |
| `Servicios/ServicioXxx` | Reglas de negocio: normalizar texto (`Trim`), validaciones que no cubren las anotaciones (fecha de cierre ≥ fecha de creación, `id` de ruta = `id` de cuerpo), duplicados, existencia, borrado lógico (`Activo = false`). Convierte entidad ↔ DTO. Lanza excepciones de negocio. |
| `Repositorios/RepositorioXxxSqlServer` | Persistencia con el `DbContext` (solo LINQ). Sin reglas de negocio. |
| `Datos/GestionDbContext` | Mapeo fluido de las 5 tablas y filtro global `activo = 1`. |
| `Modelos/` | Entidades que representan una fila de cada tabla. Sin anotaciones. |
| `Peticiones/` | DTOs de entrada con anotaciones (`[Required]`, `[StringLength]`, `[RegularExpression]`, `[Range]`). |
| `Respuestas/` | DTOs de salida. Los controladores nunca devuelven la entidad de EF Core directamente. |
| `Excepciones/` + `Infraestructura/` | Traducción de excepciones a `404`/`409`/`422`/`500` con el formato de error único. |

### 2.1 Métodos de cada capa (patrón; ejemplo `Programa`)

`IRepositorioPrograma`:

| Método | Descripción |
|---|---|
| `Task<List<Programa>> ListarAsync()` | Solo activos (filtro global). `AsNoTracking`. |
| `Task<Programa?> ObtenerPorIdAsync(int id)` | Solo activos, con seguimiento (para modificar). |
| `Task<bool> ExisteIdAsync(int id)` | **Incluye inactivos** (`IgnoreQueryFilters`). Para detectar duplicados. |
| `Task AgregarAsync(Programa programa)` | `Add` + `SaveChangesAsync`. |
| `Task GuardarCambiosAsync()` | `SaveChangesAsync` (para `PUT`, `PATCH` y borrado lógico). |

`IServicioPrograma`: `ListarAsync()`, `ObtenerAsync(int id)`, `CrearAsync(ProgramaCrear)`, `ReemplazarAsync(int id, ProgramaCrear)`, `ActualizarParcialAsync(int id, ProgramaActualizar)`, `DesactivarAsync(int id)`.

Para las demás entidades se replica el patrón cambiando la clave: `termino`, `idr`, `id` (`ExisteXxxAsync` siempre incluye inactivos). `area_conocimiento` solo tiene `ListarAsync` y `ObtenerPorIdAsync`.

### 2.2 Inversión de dependencias

- El controlador depende de `IServicioXxx`; el servicio depende de `IRepositorioXxx`. Nunca de las clases concretas.
- Registro en `Program.cs` (10 líneas, 2 por entidad):

```csharp
builder.Services.AddScoped<IRepositorioPrograma, RepositorioProgramaSqlServer>();
builder.Services.AddScoped<IServicioPrograma, ServicioPrograma>();
// ... igual para AreaConocimiento, TerminoClave, LineaInvestigacion, Red
```

### 2.3 Correspondencia endpoint → servicio (`programa`)

| Endpoint | Servicio | Éxito | Errores |
|---|---|---|---|
| `GET /api/programa` | `ListarAsync()` | `200` (lista, posiblemente `[]`) | — |
| `GET /api/programa/{id:int}` | `ObtenerAsync(id)` | `200` | `404` |
| `POST /api/programa` | `CrearAsync(ProgramaCrear)` | `201` | `422`, `409` |
| `PUT /api/programa/{id:int}` | `ReemplazarAsync(id, ProgramaCrear)` | `200` | `404`, `422` (incluye `id` distinto) |
| `PATCH /api/programa/{id:int}` | `ActualizarParcialAsync(id, ProgramaActualizar)` | `200` | `404`, `422` |
| `DELETE /api/programa/{id:int}` | `DesactivarAsync(id)` | `204` | `404` |

---

## 3. Configuración de EF Core (`GestionDbContext`)

Reglas obligatorias (sin migraciones; el `.sql` manda):

1. `ToTable("nombre_tabla")` y `HasColumnName("snake_case")` en **cada** propiedad. Sin `[Column]` en los modelos.
2. Claves **no autogeneradas** (`programa.id`, `red.idr`, `termino_clave.termino`, `area_conocimiento.id`): `ValueGeneratedNever()`.
3. Claves `IDENTITY` (`linea_investigacion.id`): `ValueGeneratedOnAdd()`.
4. `Activo` es `bool` **sin** `HasDefaultValue(true)` (con `bool`, EF trata `false` como "sin valor" y aplicaría el default `1`, rompiendo el borrado lógico). El servicio asigna `Activo = true` al crear.
5. Filtro global en las 5 entidades: `HasQueryFilter(e => e.Activo)`. Los duplicados se validan con `IgnoreQueryFilters()` en el repositorio.
6. Longitudes con `HasMaxLength(n)` iguales a las del `.sql` (ver `5_data_model.md`).

Fragmento de referencia:

```csharp
modelBuilder.Entity<Programa>(e =>
{
    e.ToTable("programa");
    e.HasKey(p => p.Id);
    e.Property(p => p.Id).HasColumnName("id").ValueGeneratedNever();
    e.Property(p => p.Nombre).HasColumnName("nombre").HasMaxLength(60).IsRequired();
    e.Property(p => p.FechaCierre).HasColumnName("fecha_cierre").HasMaxLength(45); // opcional
    // ... resto de columnas ...
    e.Property(p => p.Activo).HasColumnName("activo");
    e.HasQueryFilter(p => p.Activo);
});

modelBuilder.Entity<LineaInvestigacion>(e =>
{
    e.ToTable("linea_investigacion");
    e.HasKey(l => l.Id);
    e.Property(l => l.Id).HasColumnName("id").ValueGeneratedOnAdd();   // IDENTITY
    e.Property(l => l.Nombre).HasColumnName("nombre").HasMaxLength(45).IsRequired();
    e.Property(l => l.Descripcion).HasColumnName("descripcion").HasMaxLength(256).IsRequired();
    e.Property(l => l.Activo).HasColumnName("activo");
    e.HasQueryFilter(l => l.Activo);
});
```

---

## 4. Manejo de errores en la API

Piezas (todas en español):

- `ExcepcionNegocio` (base, con `Mensaje` y `Detalle`) y sus hijas `ExcepcionNoEncontrada` → 404, `ExcepcionConflicto` → 409, `ExcepcionValidacion` → 422.
- `FabricaErrores.CrearRespuesta422(ActionContext)`: se asigna a `ApiBehaviorOptions.InvalidModelStateResponseFactory`. Convierte los errores de `[Required]` etc. (que `[ApiController]` devolvería como `400`) en `422` con `{ "mensaje": "Datos inválidos.", "detalle": [ "campo: mensaje", ... ] }`. También cubre JSON mal formado o cuerpo ausente.
- `ManejadorExcepciones : IExceptionHandler`: captura las excepciones lanzadas por los servicios y escribe el JSON de error con el código correcto. `DbUpdateException` por clave primaria duplicada (condición de carrera) → `409`. Cualquier otra → `500` con mensaje genérico "Ocurrió un error interno. Intente de nuevo." y registro en el log.
- Los controladores **no** tienen `try/catch`: dejan subir la excepción.

---

## 5. `Program.cs` de la API (esqueleto)

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(o => o.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower)
    .ConfigureApiBehaviorOptions(o => o.InvalidModelStateResponseFactory = FabricaErrores.CrearRespuesta422);

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<ManejadorExcepciones>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<GestionDbContext>(opciones =>
    opciones.UseSqlServer(
        builder.Configuration.GetConnectionString("GestionLocal"),
        sql => sql.EnableRetryOnFailure(maxRetryCount: 10, maxRetryDelay: TimeSpan.FromSeconds(5), errorNumbersToAdd: null)));

// 10 registros AddScoped (repositorio + servicio por entidad)

var app = builder.Build();

app.UseExceptionHandler();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();      // queda en /swagger
}
app.MapControllers();
app.Run();
```

No se usa `UseHttpsRedirection` (el entorno es solo HTTP). `appsettings.json` no contiene la cadena de conexión: llega por variable de entorno.

---

## 6. Arquitectura del front (Razor Pages)

Flujo: `Página (.cshtml + PageModel) → IClienteXxx → HttpClient → API`.

### 6.1 Clientes de API

- `ClienteApiBase` (clase abstracta): recibe un `HttpClient`, define las `JsonSerializerOptions` con `SnakeCaseLower`, y ofrece métodos auxiliares `GetAsync<T>`, `PostAsync<TEntrada, TSalida>`, `PutAsync<...>`, `DeleteAsync`. Si la respuesta no es exitosa, lee el cuerpo como `RespuestaError` y lanza `ErrorApiException(codigoHttp, mensaje, detalle)`. Si hay `HttpRequestException` (API caída) lanza `ErrorApiException(503, "No se pudo conectar con la API de gestión.")`.
- Un cliente por entidad (`IClientePrograma`/`ClientePrograma`, …) con métodos explícitos: `ListarAsync`, `ObtenerAsync`, `CrearAsync`, `ReemplazarAsync`, `EliminarAsync` (áreas: solo `ListarAsync` y `ObtenerAsync`). El front **no** implementa `PATCH` en la v1 (se prueba desde Swagger/curl).
- La clave de `termino_clave` se escapa con `Uri.EscapeDataString(termino)` al armar la URL.

### 6.2 Registro en `Program.cs` del front

```csharp
var urlApi = builder.Configuration["ApiGestion:BaseUrl"]
    ?? throw new InvalidOperationException("Falta la configuración ApiGestion:BaseUrl.");

builder.Services.AddRazorPages();
builder.Services.AddHttpClient<IClientePrograma, ClientePrograma>(c => c.BaseAddress = new Uri(urlApi));
// ... igual para los otros 4 clientes
// sin UseHttpsRedirection
```

`ApiGestion__BaseUrl=http://api-gestion:8074` llega por variable de entorno del compose.

### 6.3 Páginas y modelos

- Cada entidad editable tiene `Index` (lista + handler `OnPostEliminarAsync`), `Crear` y `Editar`. Áreas: `Index` (con filtro `?buscar=`) y `Detalle`.
- Modelos del front en `Modelos/`: `XxxVista` (respuesta de la API) y `XxxFormulario` (entrada, con las **mismas anotaciones** que el DTO de la API, para validación inmediata en cliente y servidor). La duplicación es deliberada (Art. 1).
- Los `PageModel` capturan `ErrorApiException`: `409`/`422` → `ModelState.AddModelError(string.Empty, ...)` por cada ítem de `detalle` (y se vuelve a mostrar el formulario); `404` → `TempData["Error"]` + redirección al `Index`; `503`/otros → `TempData["Error"]`.
- `_Layout.cshtml` muestra `TempData["Exito"]` (alerta verde) y `TempData["Error"]` (alerta roja) y la barra con los 5 enlaces.
- `TerminosClave/Editar` recibe el término como parámetro de **query string** (`asp-route-termino`), no como segmento de ruta, para evitar problemas con tildes y espacios.

---

## 7. Docker

### 7.1 Dockerfile de la API (`api_gestion/Dockerfile`) — el del front es igual con `front_razor.csproj` y puerto `8010`

```dockerfile
FROM mcr.microsoft.com/dotnet/sdk:10.0
WORKDIR /app
COPY *.csproj ./
RUN dotnet restore
COPY . .
EXPOSE 8074
```

### 7.2 `docker-compose.yml` completo de referencia (`sqlserver` y `sqlserver-init` ya existen desde la Fase 0)

```yaml
services:
  sqlserver:
    image: mcr.microsoft.com/mssql/server:2022-latest
    container_name: sqlserver
    restart: unless-stopped
    environment:
      ACCEPT_EULA: "Y"
      MSSQL_SA_PASSWORD: "Paradigmas123!"
      MSSQL_PID: "Developer"
    ports:
      - "11443:1433"
    volumes:
      - mssqldata:/var/opt/mssql
    healthcheck:
      test: ["CMD-SHELL", "/opt/mssql-tools18/bin/sqlcmd -C -S localhost -U sa -P \"$$MSSQL_SA_PASSWORD\" -Q 'SELECT 1' || exit 1"]
      interval: 10s
      timeout: 5s
      retries: 20
      start_period: 30s

  sqlserver-init:
    image: mcr.microsoft.com/mssql/server:2022-latest
    depends_on:
      sqlserver:
        condition: service_healthy
    environment:
      MSSQL_SA_PASSWORD: "Paradigmas123!"
    volumes:
      - ./db/sqlserver:/scripts:ro
    entrypoint: ["/bin/bash", "/scripts/init.sh"]
    restart: "no"

  api-gestion:
    build: ./api_gestion
    container_name: api-gestion
    restart: unless-stopped
    environment:
      ASPNETCORE_ENVIRONMENT: "Development"
      ASPNETCORE_URLS: "http://+:8074"
      DOTNET_USE_POLLING_FILE_WATCHER: "true"
      ConnectionStrings__GestionLocal: "Server=sqlserver,1433;Database=gestion_local;User Id=sa;Password=Paradigmas123!;TrustServerCertificate=True"
    ports:
      - "8074:8074"
    volumes:
      - ./api_gestion:/app
    command: ["dotnet", "watch", "--non-interactive", "run", "--no-launch-profile"]

  front-razor:
    build: ./front_razor
    container_name: front-razor
    restart: unless-stopped
    environment:
      ASPNETCORE_ENVIRONMENT: "Development"
      ASPNETCORE_URLS: "http://+:8010"
      DOTNET_USE_POLLING_FILE_WATCHER: "true"
      ApiGestion__BaseUrl: "http://api-gestion:8074"
    ports:
      - "8010:8010"
    volumes:
      - ./front_razor:/app
    command: ["dotnet", "watch", "--non-interactive", "run", "--no-launch-profile"]

volumes:
  mssqldata:
```

Decisiones por servicio:

- **API y front sin `depends_on`**: la conexión de EF Core es perezosa y con reintentos; el front tolera que la API aún no responda (muestra un mensaje).
- `ASPNETCORE_ENVIRONMENT=Development` es obligatorio para que Swagger y la recarga de Razor funcionen.
- `DOTNET_USE_POLLING_FILE_WATCHER` hace que `dotnet watch` detecte cambios en volúmenes de Docker Desktop (Windows/Mac).
- El puerto de SQL Server hacia el host es `11443` para no chocar con un SQL Server local; entre contenedores se usa `sqlserver:1433`.
- Healthcheck de SQL Server con `sqlcmd -C` (la imagen 2022 trae `mssql-tools18`).
- `sqlserver-init`: misma imagen (trae `sqlcmd`), corre una vez y termina; la imagen de SQL Server no tiene `/docker-entrypoint-initdb.d`.

---

## 8. Script de base de datos (`db/sqlserver/`)

- `gestion_profesoral.sql` crea las 19 tablas (la v1 solo usa 5) e inserta las **218** filas de `area_conocimiento`. `programa` queda con **0** filas.
- `init.sh` consulta `sys.databases`; si `gestion_local` existe, termina con código `0`; si no, ejecuta `CREATE DATABASE` y luego el `.sql` con `-d gestion_local -i`.
- **Ajustes a la Fase 0** (tarea `T-005`): `red.url` pasa de `VARCHAR(45)` a `VARCHAR(200)` y se confirma que `programa` no declara claves foráneas. Detalle en `5_data_model.md`.

---

## 9. Orden de arranque real (`docker compose up -d --build`)

1. Docker construye las imágenes de la API y del front (restaura paquetes NuGet).
2. `sqlserver` arranca y empieza su healthcheck.
3. `sqlserver-init` espera `service_healthy`, crea `gestion_local` y carga el script si la base no existe.
4. `api-gestion` y `front-razor` arrancan de inmediato con `dotnet watch run` (la primera vez, `dotnet watch` compila: puede tardar un minuto).
5. Si llega una petición antes de que `sqlserver-init` termine, EF Core reintenta hasta 10 veces (5 s de espera máxima).
6. Swagger: `http://localhost:8074/swagger`. Front: `http://localhost:8010`.

---

## 10. Riesgos y mitigaciones

| Riesgo | Mitigación |
|---|---|
| SQL Server necesita ~2 GB de RAM | Cerrar aplicaciones pesadas; documentado en `7_quickstart.md`. |
| Puertos `8010`, `8074` u `11443` ocupados | Cambiar el mapeo en `docker-compose.yml` (y la URL base si se cambia el de la API interna: no aplica, la interna no cambia). |
| La primera petición llega antes de que la BD exista | `EnableRetryOnFailure`; esperar a que `sqlserver-init` esté en `Exited (0)` antes de probar (quickstart paso 2). |
| El script solo corre si la BD no existe | `docker compose down -v` + `up -d` como procedimiento oficial. |
| Los cambios de `*.sql` no se aplican a volúmenes existentes | Mismo procedimiento. |
| `dotnet watch` no detecta cambios en Windows/Mac | `DOTNET_USE_POLLING_FILE_WATCHER=true` (ya incluido). |
| `bin/` y `obj/` generados en el host chocan con los del contenedor | `.gitignore` y, si falla la restauración, borrar `api_gestion/obj` y `front_razor/obj`. |
| El mismo `id` existe como inactivo y el cliente intenta recrearlo | Se responde `409` (limitación conocida de la v1: no hay reactivación). |
