# Registros de Decisión (ADR) — Gestión Profesoral v1

> Documento 4 de 8 del spec kit raíz · Lectura opcional (contexto de por qué el plan es como es).
> Las decisiones D1–D13 vienen de la Fase 0 (algunas ajustadas); D14–D30 son nuevas de esta versión.

---

## Base de datos e infraestructura

### D1 — Borrado lógico sobre borrado físico
Alternativas: (a) `DELETE FROM` físico, (b) `UPDATE ... SET activo = 0`. **Decisión: (b)** para preservar el historial académico y la integridad referencial de las versiones futuras.

### D2 — Docker Compose para TODO, incluso la BD
Alternativas: SQL Server instalado en cada PC (heterogéneo, imposible de soportar) o una BD compartida en la nube (sin trabajo offline, punto único de falla). **Decisión:** todo en contenedores; solo se exige Docker Desktop. La primera descarga es grande una vez; después arranca en segundos.

### D3 — Un solo motor: SQL Server
El proyecto usa únicamente SQL Server. Mantener varios scripts equivalentes multiplicaría el trabajo sin aportar a la v1. Costo aceptado: la API queda atada a SQL Server (EF Core permitiría cambiar de proveedor más adelante). Por eso la constitución ya no menciona `DB_PROVIDER` ni otros motores.

### D4 — Puerto de BD desplazado (11443)
Muchos estudiantes ya tienen un SQL Server local en 1433. Dentro de la red de compose se usa el estándar (`sqlserver:1433`).

### D5 — `sqlserver-init` como contenedor efímero
La imagen de SQL Server no tiene `/docker-entrypoint-initdb.d`. Un servicio auxiliar con la misma imagen (trae `sqlcmd`), con `depends_on: service_healthy`, verifica en `sys.databases` si `gestion_local` existe y solo entonces la crea y la puebla. `restart: "no"`. Alternativa rechazada: script dentro del contenedor `sqlserver` con `&` (frágil).

### D6 — Sin `depends_on` hacia la BD + reintentos de EF Core
Con `DbContext` la conexión se abre en la primera petición, así que la API arranca aunque la BD tarde. Sin `depends_on` una petición muy temprana podría llegar antes de que `sqlserver-init` termine (la BD aún no existe). **Mitigación:** `EnableRetryOnFailure` (10 reintentos, 5 s) y la regla del quickstart de esperar `Exited (0)` antes de probar. El mismo criterio aplica al front respecto de la API.

### D7 — Código montado como volumen + recarga en caliente
`./api_gestion:/app` y `./front_razor:/app` con `dotnet watch`: guardar recarga sin rebuild. Se añade `DOTNET_USE_POLLING_FILE_WATCHER` porque Docker Desktop no propaga siempre los eventos del sistema de archivos.

### D8 — Cadena de conexión y URL de la API por variable de entorno
`ConnectionStrings__GestionLocal` (API) y `ApiGestion__BaseUrl` (front) se entregan por `docker-compose.yml`, nunca en el código.

### D9 — Volúmenes nombrados y `down -v` como reset oficial
Los datos sobreviven a `down`; `down -v` + `up -d` es el reinicio y la vía para aplicar cambios de esquema.

### D10 — Credenciales públicas a propósito
`sa/Paradigmas123!` está en el repo deliberadamente: es didáctica y el entorno jamás va a producción.

### D11 — Las 19 tablas se crean completas
La base es infraestructura dada; la v1 solo expone las 5 sin relaciones, pero el esquema (incluidas `rol` y `usuario` del módulo de login) ya queda listo para las siguientes versiones.

### D12 — `programa` arranca vacía
El Excel de origen tiene 191 filas pero no trae seis columnas obligatorias (`nivel`, `fecha_creacion`, `numero_cohortes`, `cant_graduados`, `fecha_actualizacion`, `ciudad`). Rellenarlas sería inventar datos. La tabla queda con 0 filas; los datos se crean desde el front.

### D13 — `area_conocimiento.id` como `VARCHAR(6)`
Los datos oficiales son alfanuméricos (`1A01`). Esto afecta a `estudio_ac.area_conocimiento`, que la referencia (versión futura).

---

## Tecnologías y arquitectura

### D14 — EF Core sin migraciones
El esquema es "infraestructura dada" y se define en el `.sql`. Mezclar migraciones con el script produciría dos fuentes de verdad. **Decisión:** `DbContext` con mapeo fluido manual (también más didáctico que un scaffold opaco). Costo: si cambia una columna hay que tocar dos sitios (script y `OnModelCreating`), de ahí la regla de oro del `5_data_model.md`.

### D15 — Repositorio sobre `DbContext`
EF Core ya implementa Unit of Work y repositorio, por lo que una capa extra es redundante en producción. Se mantiene porque el curso enseña capas e inversión de dependencias, y porque permite probar el servicio con un `DbContext` InMemory sin HTTP. Costo aceptado: algo de código repetitivo (Art. 1).

### D16 — Razor Pages para el front, con llamadas servidor-a-servidor
Alternativas: Flask (otro lenguaje), MVC (más piezas), Blazor (más abstracción), JavaScript en el navegador llamando a la API (exigiría CORS y mezclaría el aprendizaje). **Decisión:** Razor Pages: mismo lenguaje, una página = un archivo con su `PageModel`, y el `HttpClient` vive en el servidor. Consecuencia: **no hay CORS** y la API nunca queda expuesta al navegador desde el front.

### D17 — JSON en `snake_case`
Coincide con las columnas SQL y evita conversiones mentales. Se configura con `JsonNamingPolicy.SnakeCaseLower` en API y front. El código C# conserva PascalCase.

### D18 — Códigos HTTP
- Lista vacía → `200` con `[]` (más simple para el front que un `204` sin cuerpo).
- `POST` → `201` + `Location`.
- `DELETE` → `204`.
- Validación → `422`; duplicado → `409`; `404` para inexistente.
- `400` no se usa: `[ApiController]` lo devolvería para `[Required]`, así que se reemplaza `InvalidModelStateResponseFactory` por una fábrica que responde `422`.

### D19 — DTOs de entrada y de respuesta separados
Devolver la entidad de EF Core acopla el contrato JSON al esquema de la BD y expone campos internos como `activo`. Con DTOs de entrada separados también se evita el *over-posting* (el cliente no puede enviar `activo` ni `fecha_actualizacion`).

### D20 — `Activo` sin `HasDefaultValue(true)`
Con `bool` no anulable, EF trata `false` como valor "sin asignar" cuando hay un default configurado y lo reemplaza por `true`. Se omite el default en EF y el servicio asigna `Activo = true`. El default `1` del `.sql` queda solo como red de seguridad.

### D21 — `fecha_actualizacion` gestionada por el servicio; fechas y contadores como texto validado
Las columnas `fecha_*`, `numero_cohortes` y `cant_graduados` siguen siendo `VARCHAR(45)` (compatibilidad con el script de Fase 0 y los datos de origen). Para no aceptar basura, la API valida el formato (`yyyy-MM-dd`, solo dígitos). `fecha_actualizacion` deja de pedirse al cliente: la fija el servicio. Mejora futura sugerida: migrar a `DATE`/`INT` en la v2.

### D22 — `area_conocimiento` es solo lectura
Es un catálogo oficial de 218 registros. Exponer `POST/PUT/DELETE` abriría la puerta a corromperlo. El front solo lista, filtra y muestra detalle.

### D23 — Sin modo administrativo
`GET` siempre filtra `activo = 1`. Un parámetro para ver inactivos aumenta el alcance y no está en el objetivo de la v1. `IgnoreQueryFilters()` se usa solo internamente para validar duplicados.

### D24 — Swashbuckle para Swagger
Desde .NET 9 la plantilla ya no incluye Swashbuckle, y `Microsoft.AspNetCore.OpenApi` solo genera el JSON (no la interfaz `/swagger`). **Decisión:** `Swashbuckle.AspNetCore`, solo en `Development`.

### D25 — `PATCH` solo en `programa`
Es el verbo que se quiere enseñar y tiene la entidad con más campos. Replicarlo en todas añadiría 4 DTOs sin valor didáctico nuevo. Limitación documentada: no permite borrar valores (usar `PUT`). El front no lo usa en la v1.

### D26 — `termino_clave` con clave textual
Se conserva `termino` (VARCHAR(30)) como PK porque así está el esquema dado. Riesgos: tildes/espacios en la URL (el front escapa con `Uri.EscapeDataString` y envía el término por query string en las páginas) y no se puede recrear un término borrado lógicamente (`409`).

### D27 — Pruebas de capas con xUnit + EF Core InMemory
Rápidas, sin contenedores, y ejercitan Servicio → Repositorio. InMemory respeta los filtros globales, pero no reproduce los `DEFAULT` ni el `IDENTITY` de SQL Server: por eso los duplicados se validan en el servicio (no se delega en la BD), que es lo que se prueba.

### D28 — Código explícito por entidad, sin genéricos
Un repositorio, servicio, controlador y conjunto de páginas por entidad. Es repetitivo a propósito (Art. 1): el estudiante lee `Programa` completo y reconoce el patrón en las demás.

### D29 — Manejo centralizado de excepciones
Los servicios lanzan excepciones de negocio y un único `IExceptionHandler` las traduce a HTTP. Así los controladores quedan sin `try/catch` y el formato de error es uno solo en toda la API.

### D30 — `rol` y `usuario` quedan fuera de la v1
Son las tablas del módulo de login (y `usuario` exige hash de contraseñas con BCrypt, además de autenticación y autorización). Se agregan en una versión posterior, junto con la seguridad de la API. La v1 se concentra en las 5 tablas de catálogo, por eso ni la API ni el front exigen credenciales y el proyecto no incluye `BCrypt.Net-Next`. El script SQL conserva las dos tablas sin cambios.
