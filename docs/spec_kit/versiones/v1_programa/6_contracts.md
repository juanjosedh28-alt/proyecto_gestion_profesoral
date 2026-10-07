# Contratos de integración — Gestión Profesoral v1

> Documento 6 de 8 del spec kit raíz: cómo se hablan las piezas entre sí (puertos, hosts, variables, healthchecks, contratos de la API y contrato front ↔ API).

---

## 1. Mapa de puertos

| Servicio | Host interno (red compose) | Puerto publicado al PC |
|---|---|---|
| front-razor | `front-razor:8010` | **8010** (`http://localhost:8010`) |
| api-gestion | `api-gestion:8074` | **8074** (`/swagger`) |
| sqlserver | `sqlserver:1433` | **11443** |

Regla: entre contenedores siempre el host interno con puerto estándar; desde el PC siempre `localhost` con el puerto publicado. El **navegador solo habla con el front**; el front habla con la API usando `http://api-gestion:8074`.

---

## 2. Variables de entorno (contrato de configuración)

| Servicio | Variable | Valor en compose |
|---|---|---|
| api-gestion | `ASPNETCORE_ENVIRONMENT` | `Development` |
| api-gestion | `ASPNETCORE_URLS` | `http://+:8074` |
| api-gestion | `ConnectionStrings__GestionLocal` | `Server=sqlserver,1433;Database=gestion_local;User Id=sa;Password=Paradigmas123!;TrustServerCertificate=True` |
| api-gestion / front-razor | `DOTNET_USE_POLLING_FILE_WATCHER` | `true` |
| front-razor | `ASPNETCORE_ENVIRONMENT` | `Development` |
| front-razor | `ASPNETCORE_URLS` | `http://+:8010` |
| front-razor | `ApiGestion__BaseUrl` | `http://api-gestion:8074` |
| sqlserver | `ACCEPT_EULA`, `MSSQL_SA_PASSWORD`, `MSSQL_PID` | `Y`, `Paradigmas123!`, `Developer` |

---

## 3. Contrato de la API v1

### 3.1 Convenciones

- Cuerpos JSON en `snake_case`. `Content-Type: application/json`.
- Parámetros de ruta numéricos con restricción `:int` (un id no numérico → `404`).
- `Location` del `201`: URL del recurso creado (`/api/programa/1`).
- Ninguna respuesta incluye `activo`.

### 3.2 Códigos de estado y formato de error

| Código | Cuándo |
|---|---|
| `200` | `GET`, `PUT`, `PATCH` exitosos (lista vacía = `[]`) |
| `201` | `POST` exitoso |
| `204` | `DELETE` exitoso (sin cuerpo) |
| `404` | No existe o está inactivo |
| `409` | Clave o campo único ya existente (también contra inactivos) |
| `422` | Validación fallida, `id` de ruta ≠ `id` del cuerpo, JSON mal formado, `PATCH` vacío |
| `500` | Error inesperado |

Formato único de error (todos los códigos de error):

```json
{ "mensaje": "Datos inválidos.", "detalle": ["nombre: El nombre es obligatorio.", "facultad: La facultad debe ser mayor que 0."] }
```

Mensajes tipo: `404` → `"No se encontró el programa con id 7."`; `409` → `"Ya existe un programa con id 1."`; `422` (id) → `"El id de la ruta (1) no coincide con el id del cuerpo (2)."`; `500` → `"Ocurrió un error interno. Intente de nuevo."`.

### 3.3 Endpoints

Todos con prefijo `/api`. (✔ = existe)

| Recurso | `GET` | `GET /{clave}` | `POST` | `PUT /{clave}` | `PATCH /{clave}` | `DELETE /{clave}` |
|---|:-:|:-:|:-:|:-:|:-:|:-:|
| `area-conocimiento` (`{id}` texto) | ✔ | ✔ | — | — | — | — |
| `termino-clave` (`{termino}` texto) | ✔ | ✔ | ✔ | ✔ | — | ✔ |
| `linea-investigacion` (`{id:int}`) | ✔ | ✔ | ✔ | ✔ | — | ✔ |
| `programa` (`{id:int}`) | ✔ | ✔ | ✔ | ✔ | ✔ | ✔ |
| `red` (`{idr:int}`) | ✔ | ✔ | ✔ | ✔ | — | ✔ |

Códigos por verbo: `GET` lista `200`; `GET` por id `200`/`404`; `POST` `201`/`409`/`422`; `PUT` `200`/`404`/`422`; `PATCH` `200`/`404`/`422`; `DELETE` `204`/`404`.

### 3.4 Cuerpos de petición y respuesta

**Área de conocimiento** — respuesta (solo lectura):

```json
{ "id": "1A01", "gran_area": "Ciencias Naturales", "area": "Matemáticas", "disciplina": "Matemáticas puras" }
```

**Término clave** — `POST`/`PUT` (el `termino` del cuerpo, si viene, debe coincidir con la ruta en `PUT`) y respuesta:

```json
{ "termino": "Inteligencia artificial", "termino_ingles": "Artificial intelligence" }
```

**Línea de investigación** — `POST`/`PUT` (sin `id`):

```json
{ "nombre": "Ingeniería de software", "descripcion": "Procesos, calidad y arquitectura de software." }
```
Respuesta: lo mismo más `"id": 1`.

**Programa** — `POST`/`PUT` (`ProgramaCrear`; `fecha_actualizacion` **no** se envía):

```json
{
  "id": 1, "nombre": "Ingeniería de Sistemas", "tipo": "Pregrado", "nivel": "Profesional",
  "fecha_creacion": "2010-02-15", "fecha_cierre": null,
  "numero_cohortes": "20", "cant_graduados": "350", "ciudad": "Medellín", "facultad": 1
}
```
Respuesta (`ProgramaRespuesta`): los mismos campos más `"fecha_actualizacion": "2026-10-06"`.

`PATCH` (`ProgramaActualizar`; solo los campos a cambiar; `id` no se acepta):

```json
{ "fecha_cierre": "2030-12-31" }
```
Responde `200` con el programa completo.

**Red** — `POST`/`PUT` y respuesta:

```json
{ "idr": 1, "nombre": "RENATA", "url": "https://www.renata.edu.co", "pais": "Colombia" }
```

---

## 4. Contrato front ↔ API

- El front consume la API **desde el servidor** (`HttpClient` tipado, `BaseAddress = ApiGestion:BaseUrl`). No hay CORS.
- Serialización: `JsonNamingPolicy.SnakeCaseLower` en ambos lados.
- Traducción de errores: toda respuesta no exitosa se convierte en `ErrorApiException(codigoHttp, mensaje, detalle)`; la API caída es `ErrorApiException(503, "No se pudo conectar con la API de gestión.")`.

| Resultado de la API | Qué hace el front |
|---|---|
| `200` / `201` / `204` | `TempData["Exito"]` con mensaje en español y redirección a `Index` (o muestra los datos) |
| `422` / `409` | Vuelve a pintar el formulario con cada ítem de `detalle` (o el `mensaje`) como error de validación |
| `404` | `TempData["Error"]` y redirección a `Index` |
| `503` / `500` | `TempData["Error"]` con el mensaje |

### Rutas del front

| Entidad | Rutas |
|---|---|
| Inicio | `/` |
| Áreas de conocimiento | `/AreasConocimiento` (`?buscar=texto`), `/AreasConocimiento/Detalle?id=1A01` |
| Términos clave | `/TerminosClave`, `/TerminosClave/Crear`, `/TerminosClave/Editar?termino=...` |
| Líneas de investigación | `/LineasInvestigacion`, `/Crear`, `/Editar/{id}` |
| Programas | `/Programas`, `/Programas/Crear`, `/Programas/Editar/{id}` |
| Redes | `/Redes`, `/Redes/Crear`, `/Redes/Editar/{idr}` |

Eliminar: botón en cada `Index` con `onsubmit="return confirm('¿Eliminar este registro?')"` que envía un `POST` al handler `?handler=Eliminar`.

---

## 5. Contrato API ↔ BD

- Base `gestion_local` en SQL Server; cadena desde `ConnectionStrings__GestionLocal`.
- Conexión perezosa (primera petición) con `EnableRetryOnFailure`; no hay `depends_on` api → BD.
- Sin triggers ni procedimientos almacenados: las reglas viven en la API.
- Borrado lógico (`activo = 0`); todo `GET` filtra `activo = 1`.

---

## 6. Healthchecks y arranque

| Servicio | Check | Parámetros |
|---|---|---|
| sqlserver | `sqlcmd -C -Q 'SELECT 1'` | cada 10 s, 20 reintentos, `start_period` 30 s |
| sqlserver-init | `depends_on: sqlserver: condition: service_healthy`; termina `Exited (0)` | `restart: "no"` |
| api-gestion / front-razor | sin healthcheck; se verifica abriendo `/swagger` y `/` | — |

---

## 7. Volúmenes y montajes

| Montaje | Para qué |
|---|---|
| `mssqldata` (nombrado) | Persistencia de datos; `down -v` = reset |
| `./db/sqlserver` → `/scripts` (ro, en sqlserver-init) | `init.sh` + `gestion_profesoral.sql` |
| `./api_gestion` → `/app` | Recarga en caliente de la API |
| `./front_razor` → `/app` | Recarga en caliente del front |

---

## 8. Credenciales (fijas en todo el proyecto)

| Motor | BD | Usuario | Clave |
|---|---|---|---|
| SQL Server | `gestion_local` | `sa` | `Paradigmas123!` |
