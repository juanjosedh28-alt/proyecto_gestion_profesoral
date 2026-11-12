# Plan de Arquitectura — Gestión Profesoral v2 (`3_plan.md`)

> **Documento 3 de 8 del spec kit de la v2.**
> Define **CÓMO** construir lo especificado en `2_spec.md`, bajo `1_constitution.md` (con sus enmiendas de la v2).
> Solo el delta sobre la v1. Las razones de cada decisión están en `4_research.md` (D31–D64).

---

## 1. Archivos de la v2

```text
proyecto_gestion_profesoral/
├── .env.example                       # NUEVO: variables que hay que configurar (la contraseña de sa)
├── docker-compose.yml                 # AJUSTE: lee la contraseña de .env
├── db/
│   ├── gestion_profesoral.sql         # v1: las 19 tablas y las 218 áreas (sin cambios)
│   ├── v2_procedimientos.sql          # NUEVO: columna total_estudios, 11 vistas, 79 procedimientos, 2 disparadores
│   └── init.sh                        # AJUSTE: tablas solo la primera vez; la v2 en cada arranque
├── api_gestion/
│   ├── Repositorios/EjecutorProcedimientos.cs   # NUEVO: único punto que habla con SQL Server
│   ├── Middleware/ManejadorErroresSql.cs        # NUEVO: errores de la base → 404/409/422
│   ├── Servicios/Validaciones.cs                # NUEVO: fechas reales y tamaño del envío múltiple
│   ├── Controllers/  Servicios/  Repositorios/  Modelos/  Peticiones/   # +11 tablas, un archivo por capa y tabla
│   └── Program.cs                     # AJUSTE: registra lo nuevo
├── front_razor/
│   ├── Clientes/ClienteDocente.cs, ClientesDetalleDocente.cs, ClientesPuentes.cs
│   ├── Modelos/Docente.cs, DetalleDocente.cs, Puentes.cs
│   ├── Infraestructura/ErroresFormulario.cs
│   └── Pages/
│       ├── Docentes/        Index, Crear, Editar, Detalle (maestro-detalle), EditarEstudio, EditarEvaluacion,
│       │                    EditarExperiencia, EditarReconocimiento, EditarPrograma, EditarRed
│       └── EstudiosRealizados/Detalle   (apoyo, beca y áreas del estudio)
├── postman/gestion_profesoral_v2.postman_collection.json   # NUEVO: colección de pruebas
└── docs/spec_kit/versiones/v2_gestion_profesoral/          # este spec kit, GUIA_IA2.md y los prompts
```

---

## 2. Capas de la API

`Controller → Servicio → Repositorio → EjecutorProcedimientos → procedimiento almacenado`.

| Capa | Responsabilidad en la v2 |
|---|---|
| `Controllers/` | Traduce HTTP ↔ servicio. Declara las **rutas desde el maestro** con ruta absoluta (`[HttpGet("/api/docente/{cedula:int}/estudios-realizados")]`). Sin `try/catch`. |
| `Servicios/` | Revisa lo que se sabe **sin ir a la base**: fechas reales y en orden (`Validaciones`), clave de la ruta = clave del cuerpo (`422`), envío de 1 a 100 renglones (`422`), «no hay fila» → `NoEncontradoExcepcion` (`404`). **No** revisa si el padre existe ni si hay repetidos: eso lo hace el procedimiento. |
| `Repositorios/` | Una clase por tabla; **cada método es una llamada** a un procedimiento con sus parámetros. Cero SQL. |
| `Repositorios/EjecutorProcedimientos` | Abre la conexión, ejecuta el procedimiento por su nombre con `CommandType.StoredProcedure` y cierra. Tres métodos: `ListarAsync<T>`, `PrimeroAsync<T>`, `EjecutarAsync`. |
| Base de datos | Todo el SQL, las reglas de integridad, los mensajes de error y los disparadores. |

### 2.1 Patrón por tabla (ejemplo `estudios_realizados`)

| Capa | Archivo | Qué tiene |
|---|---|---|
| Modelo | `Modelos/EstudiosRealizados.cs` | Las columnas de `vw_estudios_realizados` menos `activo` (incluye `docente_nombre`) |
| Petición | `Peticiones/EstudiosRealizadosCrear.cs` | `EstudiosRealizadosRenglon` (sin docente, para el envío múltiple) y `EstudiosRealizadosCrear : …Renglon` (con docente) |
| Repositorio | `Repositorios/RepositorioEstudiosRealizados.cs` | `ListarAsync`, `ListarPorDocenteAsync`, `ObtenerAsync`, `CrearAsync`, `CrearLoteAsync`, `ActualizarAsync`, `EliminarAsync` |
| Servicio | `Servicios/ServicioEstudiosRealizados.cs` | Los mismos métodos + `Validar` |
| Controlador | `Controllers/EstudiosRealizadosController.cs` | `/api/estudios-realizados` + las dos rutas desde `/api/docente/{cedula}` |

Un método de repositorio queda así de corto:

```csharp
public Task<EstudiosRealizados?> ObtenerAsync(int id) =>
    _bd.PrimeroAsync<EstudiosRealizados>("sp_consultar_estudios_realizados", new { id });
```

El envío múltiple serializa la lista a JSON en `snake_case` y la manda en **un** parámetro:

```csharp
_bd.ListarAsync<EstudiosRealizados>("sp_crear_estudios_realizados_lote",
    new { docente = cedula, json = JsonSerializer.Serialize(renglones, JsonSnakeCase) });
```

Las tablas de la v1 siguen el mismo patrón: sus repositorios ahora llaman `sp_listar_red`, `sp_crear_red`, etc.

---

## 3. Manejo de errores

| Quién | Qué detecta | Respuesta |
|---|---|---|
| Anotaciones del DTO | Obligatorio, longitud, formato, rango | `422` (`InvalidModelStateResponseFactory`; en listas el campo se nombra «renglón N, campo») |
| Servicio | Fecha que no existe, fin antes del inicio, clave de ruta ≠ cuerpo, envío vacío o > 100 | `422` (`ValidacionExcepcion`) / `404` |
| Procedimiento | `THROW 50001` padre inexistente o retirado · `50002` repetido o retiro bloqueado · `50003` dato inválido · `50004` no encontrado | `409` / `409` / `422` / `404` (lo traduce `ManejadorErroresSql`) |
| SQL Server (red de seguridad) | 547 clave foránea · 2627/2601 duplicado · 2628/8152 texto demasiado largo | `409` / `409` / `422` |
| Cualquier otra cosa | — | `500` genérico (manejador de la v1) |

**Orden en `Program.cs`:** `app.UseMiddleware<ManejadorErrores>()` y **después** `app.UseMiddleware<ManejadorErroresSql>()`.
La excepción sube desde el controlador hacia afuera: pasa primero por el registrado de último. Si este no la
reconoce, la vuelve a lanzar con `throw;` y la atiende el de la v1.

---

## 4. Procedimientos almacenados

Todos en `db/v2_procedimientos.sql`, escritos con `CREATE OR ALTER` (se puede volver a ejecutar).

| Grupo | Procedimientos |
|---|---|
| v1 (22) | `area_conocimiento`: listar, consultar · `termino_clave`, `linea_investigacion`, `programa`, `red`: los cinco verbos |
| v2 (53) | Los cinco verbos de las 11 tablas, menos `sp_actualizar_` de las puentes puras (`intereses_futuros`, `estudio_ac`) |
| Lote (4) | `sp_crear_estudios_realizados_lote`, `sp_crear_evaluacion_docente_lote`, `sp_crear_reconocimiento_lote`, `sp_crear_experiecia_lote` |

Patrón de un procedimiento de escritura:

```sql
CREATE OR ALTER PROCEDURE sp_crear_estudios_realizados  @id INT, @titulo VARCHAR(45), … , @docente INT
AS
BEGIN
    SET NOCOUNT ON;
    -- 1) reglas que miran otras tablas -> THROW con número y mensaje en castellano
    IF NOT EXISTS (SELECT 1 FROM docente WHERE cedula = @docente AND activo = 1) … THROW 50001, @msg, 1;
    IF EXISTS (SELECT 1 FROM estudios_realizados WHERE id = @id)                 … THROW 50002, @msg, 1;
    -- 2) se guarda
    INSERT INTO estudios_realizados (…) VALUES (…);
    -- 3) se devuelve la fila COMO QUEDÓ GUARDADA
    EXEC sp_consultar_estudios_realizados @id;
END;
```

Patrón del envío múltiple: `OPENJSON(@json)` da la posición de cada renglón (`[key]`), se pasa a una tabla en
memoria, se revisan repetidos dentro del envío y contra la tabla (mensaje con el número de renglón), y se guarda
con **un solo** `INSERT … SELECT` dentro de `BEGIN TRANSACTION … COMMIT` con `SET XACT_ABORT ON` (todo o nada).

Reglas comunes:

- Cada tabla de la v2 tiene una vista `vw_<tabla>` con las fechas como `CHAR(10)` estilo 23 (`aaaa-mm-dd`) y el
  nombre del padre. `sp_listar_`, `sp_consultar_`, `sp_crear_` y `sp_actualizar_` devuelven sus columnas.
- `sp_listar_` filtra `activo = 1`; los del detalle aceptan un parámetro opcional (`@docente = NULL`, `@estudio = NULL`).
- `sp_eliminar_` hace `UPDATE … SET activo = 0`. En `docente` y `estudios_realizados` se bloquea con `50002` si hay detalle activo (R2).
- En puentes y 1:1, `sp_crear_` reactiva la pareja retirada (R3).

---

## 5. Disparadores

| Disparador | Tabla y eventos | Qué hace |
|---|---|---|
| `trg_contar_estudios` | `estudios_realizados` · `INSERT, UPDATE, DELETE` | `docente.total_estudios = COUNT(*)` de los estudios activos, para todos los docentes que aparecen en `inserted` o `deleted`, en una sola pasada |
| `trg_sello_docente` | `docente` · `UPDATE` | `fecha_actualizacion = hoy` si el `UPDATE` tocó una columna editable (no cuando el único cambio es `total_estudios`) |

La columna se agrega al comienzo del script: `IF COL_LENGTH('docente','total_estudios') IS NULL ALTER TABLE …`.

---

## 6. Front (Razor Pages)

Flujo: `Página (PageModel) → ClienteXxx → HttpClient → API`, igual que la v1.

- **Un cliente por tabla** sobre `ClienteApiBase`, con `ListarPorDocenteAsync` y `CrearVariosAsync` donde aplica.
  `ClienteApiBase` gana `GetOpcionalAsync` (404 → `null`) para apoyo y beca.
- **`Docentes/Detalle/{cedula}`**: datos del docente y siete secciones. En cada sección: tabla con Editar/Retirar
  y un formulario para agregar. Estudios, evaluaciones, experiencias y reconocimientos tienen **varios renglones**:
  JavaScript simple copia el renglón y renumera los nombres (`estudios[0].Titulo`, `estudios[1].Titulo`…) y el
  `PageModel` llama **una vez** a `CrearVariosAsync`.
- Los formularios de la página son **parámetros del handler**, no `[BindProperty]`, para que solo se valide el que se envió.
- **Desplegables**: se llenan con los `GET` de la API (ya filtran activos). Muestran el nombre y mandan la clave.
  La línea de investigación tiene «(ninguna)» y el modelo usa `int?` para que llegue `null`. Si un desplegable no
  tiene opciones, se muestra «Aún no hay … Cree uno primero en …» con el enlace.
- **Errores**: `409`/`422` se muestran dentro de la sección y se conservan los renglones escritos; lo demás va a
  `TempData["Error"]`. El mensaje genérico no muestra números HTTP.
- **Barra**: un enlace nuevo, «Docentes».

---

## 7. Base de datos y Docker

- `init.sh`: crea la base si no existe; si la tabla `docente` no existe ejecuta `gestion_profesoral.sql`; siempre
  ejecuta `v2_procedimientos.sql`. `sqlcmd` con `-b` (falla visible), `-I` y `-f 65001` (tildes).
- La contraseña de `sa` viene de `.env` → `MSSQL_SA_PASSWORD`; la API la recibe dentro de `ConnectionStrings__SqlServer`.
- Puertos: front `8010`, API `8074` (Swagger en la raíz), SQL Server `11472`.

---

## 8. Riesgos y mitigaciones

| Riesgo | Mitigación |
|---|---|
| Un error de la base sale como `500` | `ManejadorErroresSql` registrado después del de la v1; peticiones que deben fallar en la colección de Postman |
| Tildes dañadas en los mensajes | Literales `N'…'` y `sqlcmd -f 65001` |
| El script de la v2 falla a la mitad | `-b`: el contenedor queda en `Exited (1)` y se ve en `docker compose logs sqlserver-init` |
| `programa` empieza vacía y el desplegable de programas no tiene opciones | Aviso con enlace a `/Programas/Crear` |
| El contador se desfasa si alguien edita la base a mano | El disparador recalcula con `COUNT(*)`; el script lo deja al día al aplicarse |
| Se cambia la contraseña en `.env` con un volumen viejo | `docker compose down -v` (la contraseña de `sa` se fija al crear el volumen) |

---

## 9. Chequeo de constitución (compuerta 2)

| Artículo | ¿El plan lo cumple? |
|---|---|
| 1 Propósito didáctico | ✔ Cada archivo nuevo explica qué hace y por qué |
| 2 Tres capas (enmendado) | ✔ El front no toca la base; la API sigue `Controller → Servicio → Repositorio → Ejecutor → procedimiento` |
| 3 Tecnologías (enmendado) | ✔ .NET 10, controladores, Dapper + SQL Server 2022, Razor Pages, Swagger. Sin paquetes nuevos |
| 4 Un solo comando | ✔ `docker compose up -d --build` (con `.env` creado) |
| 5 Persistencia (enmendado) | ✔ Las tablas se crean una vez; el script v2 es idempotente |
| 6 Convenciones (enmendado) | ✔ Rutas `kebab-case`, JSON `snake_case`, puerto 11472, contraseña en `.env` |
| 7 Recarga en caliente | ✔ Sin cambios (`dotnet watch` y volúmenes) |
| 8 Seguridad (enmendado) | ✔ Sin SQL en C#; todo por parámetros; sin secretos en el código |
| 9 Contrato HTTP | ✔ Mismos códigos y formato `{ mensaje, detalle }` |
| 10 Borrado lógico | ✔ `sp_eliminar_` = `activo = 0`; todo `sp_listar_` filtra |
| 11 Calidad | ✔ Commits por fase, tareas con ID en `8_tasks.md` |
| 12–17 (v2) | ✔ SQL en la base, disparadores por conjuntos, nunca 500 previsible, desplegables y maestro-detalle, comentarios y prompts, regresión |

Resultado: **en verde**. Las diferencias con el texto original se resolvieron enmendando la constitución (Artículos 2, 3, 5, 6 y 8).
