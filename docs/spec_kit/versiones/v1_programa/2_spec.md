# Especificación Funcional — Gestión Profesoral v1 (`2_spec.md`)

> **Documento 2 de 8 del spec kit raíz.**
> Orden de lectura: `1_constitution` → `2_spec` → `3_plan` → `4_research` → `5_data_model` → `6_contracts` → `7_quickstart` → `8_tasks`.

Define **QUÉ** debe hacer el sistema. El **CÓMO** está en `3_plan.md`.

---

## 1. Propósito

Entregar un sistema completo (**SQL Server + API REST + front Razor Pages**) para administrar las **5 entidades independientes** (sin claves foráneas) de la base `gestion_local`, con arquitectura en capas estricta y enfoque didáctico en español, sobre **ASP.NET Core (.NET 10)** y **Entity Framework Core**.

El usuario final usa el **front** para listar, crear, editar y eliminar registros; el front consume la API; la API persiste en SQL Server.

---

## 2. Alcance de la v1

Entran: `area_conocimiento`, `termino_clave`, `linea_investigacion`, `programa`, `red`.

**Fuera de alcance** (versiones futuras): tablas con relaciones (las otras 14 del script), el módulo de login (`rol`, `usuario`, autenticación y autorización, que se agrega más adelante), paginación en servidor, reactivación de registros inactivos, modo administrativo para ver inactivos, CORS (el front llama a la API desde el servidor), migraciones de EF Core.

---

## 3. Matriz entidad × operación (API)

| Entidad | Ruta base | GET lista | GET por id | POST | PUT | PATCH | DELETE |
|---|---|:-:|:-:|:-:|:-:|:-:|:-:|
| `area_conocimiento` | `/api/area-conocimiento` | ✔ | ✔ (`{id}`) | — | — | — | — |
| `termino_clave` | `/api/termino-clave` | ✔ | ✔ (`{termino}`) | ✔ | ✔ | — | ✔ |
| `linea_investigacion` | `/api/linea-investigacion` | ✔ | ✔ (`{id}`) | ✔ | ✔ | — | ✔ |
| `programa` | `/api/programa` | ✔ | ✔ (`{id}`) | ✔ | ✔ | ✔ | ✔ |
| `red` | `/api/red` | ✔ | ✔ (`{idr}`) | ✔ | ✔ | — | ✔ |

Semántica común (detalle de cuerpos y códigos en `6_contracts.md`, reglas de campos en `5_data_model.md`):

- `GET` lista: solo `activo = 1`; vacío → `200` con `[]`.
- `GET` por id: `404` si no existe o está inactivo.
- `POST`: `201` + `Location`. Asigna `activo = true` en el servicio. `id`/clave repetida → `409` (se valida también contra inactivos).
- `PUT`: reemplazo completo. Si el cuerpo trae un `id` (o `termino`, `idr`) distinto del de la ruta → `422`. `404` si no existe.
- `PATCH` (solo `programa`): modifica solo los campos enviados; `200` con el programa completo; `422` si el cuerpo no trae ningún campo.
- `DELETE`: `UPDATE ... SET activo = 0`; `204`; `404` si no existe.

---

## 4. Requisitos funcionales — API

### RF1 — Área de conocimiento (solo lectura)

Catálogo oficial precargado con **218 registros** (ids alfanuméricos como `1A01`). La API expone únicamente `GET` lista y `GET` por id. No hay `POST`/`PUT`/`DELETE`: es una tabla de referencia.

### RF2 — Término clave

CRUD según la matriz. La clave primaria es el texto del término (`termino`, máx. 30). `termino_ingles` es opcional. Un término ya existente (activo o inactivo) responde `409`.

### RF3 — Línea de investigación

CRUD según la matriz. `id` autoincremental (`IDENTITY`): el cliente **no** lo envía. `nombre` y `descripcion` obligatorios.

### RF4 — Programa (vertical de referencia)

CRUD completo + `PATCH`. `id` **lo envía el cliente** (no es `IDENTITY`). Reglas:

- Obligatorios: `id`, `nombre`, `tipo`, `nivel`, `fecha_creacion`, `numero_cohortes`, `cant_graduados`, `ciudad`, `facultad`. Opcional: `fecha_cierre`.
- Fechas en formato `yyyy-MM-dd`; `fecha_cierre`, si viene, no puede ser anterior a `fecha_creacion` (`422`).
- `numero_cohortes` y `cant_graduados` son texto numérico (solo dígitos).
- `fecha_actualizacion` **no la envía el cliente**: el servicio la fija con la fecha actual en `POST`, `PUT` y `PATCH`.
- La tabla inicia **vacía** (0 filas).
- El `PATCH` no permite borrar un valor (enviar `null` equivale a "no enviado"); para borrar `fecha_cierre` se usa `PUT`.

### RF5 — Red

CRUD según la matriz. `idr` **lo envía el cliente**. `url` debe empezar por `http://` o `https://` (máx. 200).

---

## 5. Requisitos funcionales — Front (Razor Pages)

### RF6 — Front de las 5 entidades

El front (`front_razor`, puerto 8010) ofrece:

- **RF6.1 Inicio y navegación:** página de inicio con acceso a las 5 entidades y barra de navegación común (layout).
- **RF6.2 Listado:** por cada entidad una página `Index` con tabla de registros; si no hay datos muestra el mensaje "No hay registros para mostrar".
- **RF6.3 Crear:** formulario con validación en servidor (y de cliente con las anotaciones) por cada entidad editable.
- **RF6.4 Editar:** formulario precargado. La clave primaria (`id`, `idr`, `termino`) se muestra de solo lectura.
- **RF6.5 Eliminar:** botón en el listado con confirmación del navegador; ejecuta `DELETE` de la API.
- **RF6.6 Mensajes:** éxito (verde) y error (rojo) en español tras cada operación. Los `409` y `422` de la API se muestran en el formulario; un `404` redirige al listado con aviso; si la API no responde se muestra "No se pudo conectar con la API de gestión."
- **RF6.7 Área de conocimiento:** solo listado con **filtro de texto** (gran área, área, disciplina) y página de detalle. Sin crear/editar/eliminar.
- **RF6.8 Programas:** el formulario usa `<input type="date">` para las fechas; al crear se pide el `id`.

---

## 6. Requisitos no funcionales

### RNF1 — Arquitectura en capas
Controllers → Servicios → Repositorios → `DbContext` en la API; Página → Cliente de API en el front (Constitución, Art. 2).

### RNF2 — Contrato de respuestas y errores
Códigos y formato de error únicos definidos en la Constitución (Art. 9) y detallados en `6_contracts.md`. Mensajes en español.

### RNF3 — Filtrado por defecto
Todo `GET` retorna solo `activo = 1` mediante un filtro global de EF Core. No existe modo administrativo en la v1; los registros inactivos solo se ven en la base de datos.

### RNF4 — Documentación automática
La API expone Swagger UI en `/swagger` (entorno `Development`).

### RNF5 — Convenciones de nombres
Código C# en PascalCase en español; columnas y JSON en `snake_case` (Constitución, Art. 6).

### RNF6 — Seguridad
LINQ parametrizado, cadena de conexión por variable de entorno y DTOs de respuesta (Constitución, Art. 8).

### RNF7 — Resiliencia al arranque
La API arranca sin esperar a la BD; la conexión es perezosa y EF Core reintenta ante fallos transitorios (`EnableRetryOnFailure`), de modo que una petición temprana no rompe la aplicación. El front no necesita esperar a la API.

---

## 7. Criterios de aceptación

| ID | Criterio |
|---|---|
| CA-01 | `docker compose up -d --build` deja corriendo `sqlserver`, `api-gestion` y `front-razor`; `sqlserver-init` termina en `Exited (0)`. |
| CA-02 | `GET /api/area-conocimiento` retorna `200` con **218** registros. |
| CA-03 | `GET /api/programa` con la tabla vacía retorna `200` con `[]`. |
| CA-04 | `POST /api/programa` válido retorna `201`; repetido retorna `409`; con campos faltantes retorna `422` con el formato de error único. |
| CA-05 | Un `DELETE` en **cualquiera** de las 4 entidades editables retorna `204` y deja la fila en la BD con `activo = 0`; el `GET` posterior ya no la devuelve. |
| CA-06 | `PUT /api/programa/1` con `"id": 2` en el cuerpo retorna `422`. |
| CA-07 | `/swagger` muestra los endpoints de las 5 entidades. |
| CA-08 | En el front (`http://localhost:8010`) se puede listar, crear, editar y eliminar en las 4 entidades editables, y listar/filtrar/ver detalle en áreas de conocimiento. |
| CA-09 | Los errores `409`/`422` de la API se ven como mensajes en español dentro del formulario del front. |
| CA-10 | Los datos persisten tras `docker compose down` + `up -d`, y `down -v` los devuelve al estado original (218 áreas, resto vacío). |
| CA-11 | La suite de pruebas de capas (`dotnet test`) pasa. |
