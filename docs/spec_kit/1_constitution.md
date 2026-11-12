# Constitución del Proyecto — Gestión Profesoral (v1)

> **Documento 1 de 8 del spec kit raíz.**
> Orden de lectura: `1_constitution` → `2_spec` → `3_plan` → `4_research` → `5_data_model` → `6_contracts` → `7_quickstart` → `8_tasks`.

Principios innegociables de la infraestructura, la arquitectura y la integración del proyecto. El kit es autocontenido: si dos documentos se contradicen, **gana el de número menor** (la constitución manda sobre la spec, la spec sobre el plan, y así sucesivamente).

**Objetivo de la v1:** un sistema completo y funcionando (**base de datos + API + front**) que ofrece el CRUD de las **5 tablas sin relaciones** de la base `gestion_local`, construido con **C# (.NET 10), Entity Framework Core, SQL Server y Razor Pages**.

---

## Artículo 1 — Propósito didáctico ante todo

Este proyecto existe para enseñar construcción de software (especificación, implementación por capas, calidad y despliegue) a estudiantes universitarios que ya cursaron paradigmas de programación y diseño de software. Ante cualquier disyuntiva entre "lo más profesional" y "lo más claro para aprender", gana la claridad:

- Todo el código, los comentarios, los mensajes al usuario y la documentación se escriben en **español**.
- Cada archivo `.cs` abre con un comentario XML (`/// <summary>`) que explica su papel en la arquitectura.
- Se prefiere código **explícito y repetitivo-pero-legible** sobre genéricos, reflexión o metaprogramación compacta. Cada entidad tiene sus propios archivos (modelo, repositorio, servicio, controlador, páginas).
- Se respetan los nombres que exige el framework aunque estén en inglés (`Program.cs`, sufijo `Controller`, carpeta `Pages`, `PageModel`, `DbContext`).

---

## Artículo 2 — Arquitectura de 3 capas estricta

```text
CAPA 1: FRONT  (Razor Pages, :8010)   — solo pinta HTML y llama a la API; NUNCA toca la BD
CAPA 2: API    (ASP.NET Core, :8074)  — Controllers → Servicios → Repositorios → DbContext; solo JSON
CAPA 3: DATOS  (SQL Server 2022, base gestion_local)
```

- El front **no** referencia EF Core ni drivers de base de datos; solo habla HTTP con la API.
- La API **no** genera HTML; solo JSON.
- El front llama a la API **desde el servidor** (`HttpClient` dentro de los `PageModel`), nunca desde el navegador. Por eso la v1 **no necesita CORS**.
- Dentro de la API el flujo es siempre `Controller → Servicio → Repositorio → DbContext → SQL Server`. Ninguna capa se salta a otra. Está prohibido mezclar lógica HTTP con acceso a datos.
- Dentro del front el flujo es `Página (PageModel) → Cliente de API → API`.

---

## Artículo 3 — Tecnologías fijas

| Pieza | Tecnología |
|---|---|
| Lenguaje y plataforma | C# sobre **.NET 10** |
| API | ASP.NET Core Web API con **controladores** (`[ApiController]`) |
| Acceso a datos | **Entity Framework Core** (`Microsoft.EntityFrameworkCore.SqlServer`), un único `GestionDbContext` |
| Base de datos | **SQL Server 2022** (único motor; sin `DB_PROVIDER`) |
| Front | **Razor Pages** (.NET 10) con Bootstrap incluido en la plantilla |
| Documentación de la API | `Swashbuckle.AspNetCore` → `/swagger` |
| Pruebas | xUnit + `Microsoft.EntityFrameworkCore.InMemory` |

Reglas de EF Core:

- **No se usan migraciones.** El esquema lo define `db/sqlserver/gestion_profesoral.sql`; el `DbContext` se mapea a mano con la API fluida (`OnModelCreating`).
- Se usan versiones estables de los paquetes compatibles con .NET 10.

---

## Artículo 4 — Un solo comando para arrancar

```bash
docker compose up -d --build
```

Este comando debe dejar **todo** funcionando: SQL Server con datos, API y front. Sin pasos manuales y sin instalar nada local más allá de Docker Desktop. Los estudiantes tienen máquinas heterogéneas, por eso el entorno vive completo en contenedores.

---

## Artículo 5 — Persistencia y reproducibilidad

- Los datos viven en el volumen Docker `mssqldata`: sobreviven a `docker compose down` y a reinicios del PC.
- `docker compose down -v` devuelve la BD a su estado original. Ese es el "botón de pánico" oficial.
- El script de inicialización corre **una sola vez**: `sqlserver-init` verifica que `gestion_local` no exista antes de crearla y poblarla.
- Cualquier cambio de esquema se hace en el `.sql` y se aplica con `down -v` + `up -d`.

---

## Artículo 6 — Convenciones fijas

| Cosa | Convención |
|---|---|
| Puertos públicos | front `8010` · API `8074` (`/swagger`) |
| Puerto de BD hacia el host | SQL Server `11443` (desplazado para no chocar con un SQL Server local) |
| Hosts internos (entre contenedores) | `sqlserver:1433` · `api-gestion:8074` · `front-razor:8010` |
| Servicios de compose | `sqlserver` · `sqlserver-init` · `api-gestion` · `front-razor` |
| Credenciales BD | `sa` / `Paradigmas123!` (públicas y didácticas a propósito) |
| Base de datos | `gestion_local` |
| Código C# | Clases, métodos, propiedades y carpetas en **PascalCase en español**; interfaces con prefijo `I`; parámetros y variables en `camelCase` en español |
| Columnas SQL y JSON | **`snake_case`** (`fecha_creacion`). El JSON se configura con `JsonNamingPolicy.SnakeCaseLower` en API **y** front |
| Rutas de la API | `kebab-case` en singular: `/api/area-conocimiento`, `/api/programa` |
| Rutas del front | `PascalCase` en plural: `/Programas`, `/AreasConocimiento` |

---

## Artículo 7 — Desarrollo con recarga en caliente

El código fuente se monta como volumen (`./api_gestion:/app`, `./front_razor:/app`) y ambos contenedores corren con `dotnet watch`: guardar un archivo recarga la aplicación sin reconstruir imágenes. Reconstruir (`--build`) solo es necesario cuando cambian paquetes NuGet o Dockerfiles.

---

## Artículo 8 — Seguridad en su justa medida académica

- Todo acceso a datos usa LINQ de EF Core (parametrizado). **Prohibido** `FromSqlRaw`/`ExecuteSqlRaw` con cadenas concatenadas.
- La API solo devuelve DTOs de respuesta (nunca la entidad de EF Core directamente), para controlar siempre qué campos salen.
- La cadena de conexión viaja por variable de entorno, nunca escrita en el código.
- Las credenciales de infraestructura son públicas a propósito: este entorno jamás se despliega a producción.

---

## Artículo 9 — Contrato HTTP único

Todas las respuestas son JSON (salvo `204`) y usan solo estos códigos:

| Código | Uso |
|---|---|
| `200` | Lectura, `PUT` y `PATCH` exitosos. Un listado vacío es `200` con `[]` |
| `201` | `POST` exitoso (con cabecera `Location`) |
| `204` | `DELETE` exitoso (borrado lógico) |
| `404` | El registro no existe o está inactivo |
| `409` | Conflicto: clave primaria o campo único ya existente (incluso si el registro duplicado está inactivo) |
| `422` | Datos inválidos: campos obligatorios, longitudes, formatos, `id` de la ruta distinto del `id` del cuerpo, JSON mal formado |
| `500` | Error inesperado (mensaje genérico en español, sin detalles técnicos) |

Los errores siempre tienen **un solo formato**:

```json
{ "mensaje": "Datos inválidos.", "detalle": ["nombre: El nombre es obligatorio."] }
```

`detalle` es una lista (puede ir vacía). Los mensajes están en español.

---

## Artículo 10 — Borrado lógico y alcance de la v1

- Todas las tablas de la v1 tienen `activo BIT NOT NULL DEFAULT 1`. El `DELETE` de la API ejecuta `activo = 0`; la fila **nunca** se elimina físicamente.
- Todo `GET` devuelve solo `activo = 1` (filtro global de EF Core). No existe modo administrativo en la v1.
- La v1 implementa **solo las 5 tablas independientes** (sin claves foráneas): `area_conocimiento`, `termino_clave`, `linea_investigacion`, `programa`, `red`. Las 14 tablas restantes existen en el script SQL pero no se exponen hasta versiones posteriores; entre ellas `rol` y `usuario`, que pertenecen al módulo de login y se agregarán más adelante.
- `programa` es la **vertical de referencia**: se construye primero completa (API + front) y las demás entidades la replican.

---

## Artículo 11 — Calidad y seguimiento

- Las tareas (`8_tasks.md`) tienen **ID** (`T-101`…), checkbox individual y trazabilidad a los requisitos (`RF`).
- Cada fase termina en **un commit** con mensaje propio y en algo **verificable**.
- **Definición de terminado** de una tarea/fase: (1) compila sin errores ni advertencias nuevas, (2) la verificación de la fase pasa, (3) comentarios XML en español en cada archivo nuevo, (4) los documentos del kit siguen coherentes con el código, (5) commit hecho y checkbox marcado.
- La **"prueba de capas"** es un proyecto xUnit que ejercita Servicio → Repositorio sobre EF Core InMemory, sin HTTP.
- Si durante la construcción se cambia un contrato, se actualiza primero el documento correspondiente y luego el código.

---

# Enmiendas de la versión 2

> La constitución es una sola para todas las versiones. Lo que sigue **no la reemplaza**: la corrige donde el
> código real de la v1 y las exigencias de la v2 (ver `ProyectosDeAula/docs/VERSION_2.md` del curso) la
> contradicen. Es la salida que da la **compuerta 2**: "o se corrige el plan, o se enmienda la constitución".
> Fecha: noviembre de 2026.

## Enmienda al Artículo 3 — Acceso a datos con Dapper y procedimientos almacenados

- El código de la v1 se construyó con **Dapper** (`Dapper` + `Microsoft.Data.SqlClient`), no con EF Core. La
  constitución se ajusta a lo que existe: no hay `GestionDbContext` ni pruebas InMemory.
- Desde la v2, **todo** el acceso a datos de las 16 tablas pasa por **procedimientos almacenados** de
  `db/v2_procedimientos.sql`. El único punto de la API que abre conexiones es `Repositorios/EjecutorProcedimientos.cs`,
  que ejecuta un procedimiento por su nombre (`CommandType.StoredProcedure`) con parámetros.
- Las pruebas de la v2 son la colección de Postman (`postman/`), con las peticiones que deben pasar y las que deben fallar.

## Enmienda al Artículo 2 — Flujo dentro de la API

`Controller → Servicio → Repositorio → EjecutorProcedimientos → procedimiento almacenado → SQL Server`.

## Enmienda al Artículo 6 — Puerto y credenciales

- El puerto de SQL Server hacia el equipo es **`11472`** (es el que usa `docker-compose.yml`).
- La contraseña de `sa` **ya no es pública**: se lee del archivo `.env` (que está en `.gitignore`); el repositorio
  trae `.env.example` con un valor de ejemplo. Lo exige la metodología del curso (§5): el proyecto se publica en la v4.
- La variable de la cadena de conexión es `ConnectionStrings__SqlServer`.

## Enmienda al Artículo 5 — El script inicial corre una sola vez

`db/init.sh` crea las tablas **solo si `docente` no existe** (antes el script de la v1 las borraba en cada
arranque). El script de la v2 es idempotente y se aplica en **cada** arranque.

## Enmienda al Artículo 8 — SQL solo en la base

- En C# no se escribe SQL: ni `SELECT`, ni `INSERT`, ni `UPDATE`, ni `DELETE`. Un `grep` sobre `api_gestion/Repositorios`
  no debe encontrar ninguna de esas palabras.
- Los valores viajan **siempre** como parámetros del procedimiento, nunca pegados en un texto.

## Artículo 12 — El SQL vive en la base

Los cinco verbos de cada tabla (`sp_listar_`, `sp_consultar_`, `sp_crear_`, `sp_actualizar_`, `sp_eliminar_<tabla>`)
están en `db/v2_procedimientos.sql`. `sp_crear_` y `sp_actualizar_` devuelven la fila **como quedó guardada**.

## Artículo 13 — Disparadores por conjuntos

Un disparador trabaja con todas las filas de `inserted` y `deleted` a la vez (nunca fila por fila) y escucha
`INSERT`, `UPDATE` **y** `DELETE` cuando el borrado es lógico.

## Artículo 14 — Un error previsible nunca es 500

Padre inexistente, pareja repetida, clave repetida y retiro bloqueado → `409`; dato inválido → `422`;
no encontrado → `404`. Siempre con mensaje en castellano y el formato `{ mensaje, detalle }` del Artículo 9.

## Artículo 15 — Las claves foráneas no se digitan

Cada clave foránea se elige de un desplegable **cargado desde la API** que muestra el nombre y manda la clave.
Una clave opcional ofrece «(ninguna)», que manda `null`. El detalle se ve y se agrega **desde su maestro**.

## Artículo 16 — Interpretabilidad y autoría

Cada archivo nuevo lleva un comentario en español que dice **qué hace y por qué**. Cada integrante hace los
commits de lo que entrega y registra sus prompts, con lo que tuvo que corregirle a la IA, en la carpeta de la versión.

## Artículo 17 — Regresión

Al cerrar la v2 se vuelven a correr los criterios de la v1. Los ajustes a código de la v1 que la v2 necesite
(por ejemplo, pasar sus repositorios a procedimientos) se hacen en la v2 y quedan escritos en su spec.
