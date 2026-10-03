# Plan de Arquitectura — v1 Programa (`3_plan.md`)

> **Documento 3 de 8 del spec kit raíz.**
> Define **CÓMO** construir lo especificado en `2_spec.md` (RF4 — Programa), bajo los principios de `1_constitution.md`.
> Orden de lectura: `1_constitution` → `2_spec` → `3_plan` → `4_research` → `5_data_model` → `6_contracts` → `7_quickstart` → `8_tasks`.

---

## 1. Estructura de archivos del proyecto

```text
gestion_profesoral/
├── docker-compose.yml              # servicios: api, sqlserver, sqlserver-init + volumen
├── README.md
├── db/
│   └── sqlserver/
│       ├── gestion_profesoral.sql  # esquema + 218 áreas de conocimiento (T-SQL)
│       └── init.sh                 # crea gestion_local y ejecuta el .sql solo la primera vez
├── docs/                           # spec kit (los 8 documentos)
└── api_gestion/                    # API ASP.NET Core (.NET 10)
    ├── Program.cs                  # arranque, inyección de dependencias, Swagger
    ├── Controllers/
    │   └── ProgramaController.cs
    ├── Servicios/
    │   └── ServicioPrograma.cs     # incluye IServicioPrograma
    ├── Repositorios/
    │   └── RepositorioProgramaSqlServer.cs   # incluye IRepositorioPrograma
    ├── Datos/
    │   └── GestionDbContext.cs     # DbContext de EF Core
    ├── Modelos/
    │   └── Programa.cs
    └── Peticiones/
        ├── ProgramaCrear.cs
        └── ProgramaActualizar.cs
```

---

## 2. Estructura de capas (C#)

El flujo de una petición es siempre `Controller → Servicio → Repositorio → DbContext → SQL Server`. Ninguna capa salta a la siguiente.

| Archivo | Capa | Responsabilidad |
|---|---|---|
| `Controllers/ProgramaController.cs` | Presentación | Traduce HTTP a llamadas al servicio y devuelve respuestas JSON con el código de estado correcto. No contiene reglas de negocio ni acceso a datos. |
| `Servicios/ServicioPrograma.cs` | Negocio | Aplica las reglas de negocio (campos obligatorios, existencia del registro, borrado lógico). Expone la interfaz `IServicioPrograma`. |
| `Repositorios/RepositorioProgramaSqlServer.cs` | Persistencia | Ejecuta las consultas con el `DbContext` de forma parametrizada. Expone la interfaz `IRepositorioPrograma`. |
| `Modelos/Programa.cs` | Dominio | Entidad que representa una fila de la tabla `programa`. |
| `Peticiones/ProgramaCrear.cs` | DTO | Datos de entrada para `POST` (y `PUT`, reemplazo completo). |
| `Peticiones/ProgramaActualizar.cs` | DTO | Datos de entrada para `PATCH` (todos los campos opcionales). |

### 2.1 Inversión de dependencias

- El controlador depende de `IServicioPrograma`, nunca de la clase concreta.
- El servicio depende de `IRepositorioPrograma`, nunca de `RepositorioProgramaSqlServer`.
- Las dependencias se registran en `Program.cs`:

```csharp
builder.Services.AddDbContext<GestionDbContext>(opciones =>
    opciones.UseSqlServer(builder.Configuration.GetConnectionString("GestionLocal")));

builder.Services.AddScoped<IRepositorioPrograma, RepositorioProgramaSqlServer>();
builder.Services.AddScoped<IServicioPrograma, ServicioPrograma>();
```

### 2.2 Correspondencia endpoint → capas

| Endpoint | Servicio | Respuesta esperada |
|---|---|---|
| `GET /api/programa` | `ListarAsync()` | `200` con la lista, o `200` si no hay registros activos |
| `POST /api/programa` | `CrearAsync(ProgramaCrear)` | `200` con el programa creado; `400`/`400` si faltan campos |
| `PUT /api/programa/{id}` | `ReemplazarAsync(id, ProgramaCrear)` | `200` con el programa; `404` si no existe |
| `PATCH /api/programa/{id}` | `ActualizarParcialAsync(id, ProgramaActualizar)` | `200 OK` con el programa; `404` si no existe |
| `DELETE /api/programa/{id}` | `DesactivarAsync(id)` | `200`; `404` si no existe. Asigna `activo = 0`, no borra la fila |

---

## 3. Modelo de datos usado por esta versión

Solo se usa la tabla `programa`, que no tiene dependencias de claves foráneas y arranca **vacía** según el script de inicialización.

| Columna | Tipo SQL Server | ¿Obligatoria? | Nota |
|---|---|---|---|
| `id` | `INT` | Sí (PK) | **No es `IDENTITY`**: el cliente envía el `id` en el `POST`. |
| `nombre` | `VARCHAR(60)` | Sí | |
| `tipo` | `VARCHAR(45)` | Sí | |
| `nivel` | `VARCHAR(45)` | Sí | |
| `fecha_creacion` | `VARCHAR(45)` | Sí | Se guarda como texto. |
| `fecha_cierre` | `VARCHAR(45)` | No | |
| `numero_cohortes` | `VARCHAR(45)` | Sí | |
| `cant_graduados` | `VARCHAR(45)` | Sí | |
| `fecha_actualizacion` | `VARCHAR(45)` | Sí | |
| `ciudad` | `VARCHAR(45)` | Sí | |
| `facultad` | `INT` | Sí | |
| `activo` | `BIT` | Sí | Por defecto `1`. El borrado lógico lo pone en `0`. |

Reglas que se derivan de la tabla:

- Todo `GET` filtra por `activo = 1` (RNF3 del `2_spec.md`).
- El `PATCH` solo modifica los campos enviados.
- Un `id` repetido en el `POST` debe responder con un error controlado en español, no con una excepción de SQL Server.

Mandar el id de la ruta; si el cuerpo trae otro, responder 422
---

## 4. `docker-compose.yml` — decisiones por servicio

### 4.1 API (`api`)

- `build: ./api_gestion` con su propio `Dockerfile` (imagen base del SDK de .NET 10).
- Volumen de código y `command: dotnet watch run` para recargar en caliente sin reconstruir la imagen.
- `restart: unless-stopped`.
- Puerto público propuesto: `8074:8074`.
- Variables de entorno:

```text
ASPNETCORE_URLS=http://+:8074
ConnectionStrings__GestionLocal=Server=sqlserver,1433;Database=gestion_local;User Id=sa;Password=Paradigmas123!;TrustServerCertificate=True
```

- No se usa `depends_on` hacia la base de datos: `DbContext` abre la conexión en la primera petición, por lo que la API tolera que SQL Server tarde en arrancar.

### 4.2 Motor de base de datos (`sqlserver`)

- Imagen `mcr.microsoft.com/mssql/server:2022-latest`.
- Variables: `ACCEPT_EULA=Y`, `MSSQL_SA_PASSWORD`, `MSSQL_PID=Developer`.
- Healthcheck con `sqlcmd -C -Q 'SELECT 1'`, `retries: 20` y `start_period: 30s`, porque tarda en estar listo.
- Puerto hacia el host desplazado para no chocar con un SQL Server local: `11443:1433`.
- Volumen nombrado `mssqldata` para que los datos sobrevivan a `docker compose down`.

### 4.3 Inicializador (`sqlserver-init`)

- Misma imagen de SQL Server (ya trae `sqlcmd`).
- `depends_on: sqlserver: condition: service_healthy`.
- Monta `./db/sqlserver:/scripts:ro` y ejecuta `entrypoint: ["/bin/bash", "/scripts/init.sh"]`.
- `restart: "no"`: corre una vez y termina.
- Motivo: la imagen de SQL Server no tiene `/docker-entrypoint-initdb.d`, así que la inicialización se hace desde un contenedor aparte.

### 4.4 Volúmenes

```yaml
volumes:
  mssqldata:
```

---

## 5. Script de base de datos (`db/sqlserver/`)

- `gestion_profesoral.sql` crea las 19 tablas completas (aunque la v1 solo use `programa`) e inserta las **218** filas de `area_conocimiento`. `programa` queda con **0** filas.
- `init.sh` consulta `sys.databases` con `sqlcmd`. Si `gestion_local` ya existe, termina con código `0`; si no existe, ejecuta `CREATE DATABASE` y luego el `.sql` con `-d gestion_local -i`.
- Todas las tablas incluyen `activo BIT NOT NULL DEFAULT 1` para el borrado lógico.

---

## 6. Orden de arranque real (`docker compose up -d --build`)

1. Docker construye la imagen de la API (restaura paquetes NuGet y compila).
2. `sqlserver` arranca y empieza su healthcheck.
3. `sqlserver-init` espera a que el healthcheck pase, crea `gestion_local` y carga el script si la base no existe.
4. La API arranca de inmediato; la primera petición que necesite datos abre la conexión.
5. Swagger queda disponible en `http://localhost:8074/swagger`.

---

## 7. Verificación rápida de la v1

| Prueba | Resultado esperado |
|---|---|
| `GET /api/programa` con la tabla vacía | `200 No Content` |
| `POST /api/programa` con todos los campos | `200 Created` |
| `GET /api/programa` después del `POST` | `200 OK` con 1 elemento |
| `PATCH /api/programa/{id}` con un campo opcional | `200 OK` con el dato modificado |
| `DELETE /api/programa/{id}` | `200`; en la BD la fila sigue existiendo con `activo = 0` |
| `GET /api/programa` después del `DELETE` | `200 No Content` |

---

## 8. Riesgos y mitigaciones

| Riesgo | Mitigación |
|---|---|
| SQL Server necesita ~2 GB de RAM | Cerrar otras aplicaciones pesadas y documentarlo en la guía del estudiante. |
| El puerto `8074` u `11443` está ocupado en el host | Cambiar el mapeo en `docker-compose.yml`. |
| El script solo se ejecuta si la BD no existe | Usar `docker compose down -v` como procedimiento oficial de reinicio. |
| Los cambios de esquema en `db/sqlserver/*.sql` no se aplican a volúmenes existentes | Mismo procedimiento: `down -v` y luego `up -d`. |
| `id` repetido en el `POST` de `programa` (no es autoincremental) | Validar en el servicio y responder `400` con mensaje en español. |
