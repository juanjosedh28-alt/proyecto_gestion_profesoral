# Plan de Arquitectura — Gestión Profesoral v2 (`3_plan.md`)

> **Documento 3 de 8 del spec kit de la v2.**
> Define **CÓMO** construir lo especificado en `2_spec.md`, bajo `1_constitution.md`. Solo el delta sobre `docs/3_plan.md` (v1).

> **Nota de alineación con el repositorio.** Este plan usa los nombres de carpetas del plan de la v1. El repositorio real tiene algunas diferencias (por ejemplo `Middleware/` en lugar de `Infraestructura/` en la API, y no se vieron `Respuestas/` ni `Datos/`). Los puntos marcados **[CONFIRMAR]** se ajustan al código real de la v1 antes de escribir cada pieza; **nunca** se mueve ni se renombra nada de la v1.

---

## 1. Archivos NUEVOS de la v2

```text
gestion_profesoral/
├── db/sqlserver/
│   ├── v2_procedimientos.sql          # NUEVO: ALTER docente, 53 SP + 8 SP de lote, 2 disparadores
│   ├── v2_verificacion.sql            # NUEVO: script de comprobación de SP y disparadores
│   └── init.sh                        # AJUSTE: ejecuta también v2_procedimientos.sql
├── docs/spec_kit/versiones/v2_gestion_profesoral/
│   ├── 1_constitution.md … 8_tasks.md # estos documentos (+ 9_checklist.md y la guía de IA)
│   └── prompts_v2.md                  # prompts reales + correcciones a la IA
├── api_gestion/
│   ├── Controllers/                   # +11: DocenteController, EstudiosRealizadosController, …
│   ├── Servicios/                     # +11: ServicioDocente, ServicioEstudiosRealizados, …
│   ├── Repositorios/                  # +11 RepositorioXxxSqlServer  +  EjecutorProcedimientos.cs
│   ├── Peticiones/                    # +DTOs de entrada: DocenteCrear, EstudiosRealizadosCrear, …
│   ├── Respuestas/                    # +DTOs de salida: DocenteRespuesta, … [CONFIRMAR carpeta]
│   ├── Excepciones/                   # sin cambios (se reutilizan las 4 de la v1)
│   └── Middleware/                    # +ManejadorExcepcionesSql.cs [CONFIRMAR: ver §4]
├── front_razor/
│   ├── Clientes/                      # +11 ClienteXxx con su interfaz
│   ├── Modelos/                       # +un archivo por tabla, como en la v1 [CONFIRMAR]
│   └── Pages/
│       ├── Docentes/                  # Index, Crear, Editar, Detalle + carpetas de secciones
│       └── EstudiosRealizados/        # Detalle (apoyo, beca, áreas)
└── pruebas/ (o api_gestion/pruebas)   # + pruebas de servicios con repositorios falsos
```

**No se crean entidades EF ni se toca `GestionDbContext`** para la v2. El `DbContext` solo aporta su conexión.

**Archivos de la v1 que reciben líneas añadidas** (nada más): `api_gestion/Program.cs`, `front_razor/Program.cs`, `front_razor/Pages/Shared/_Layout.cshtml` (un enlace) y `db/sqlserver/init.sh`.

Paquetes NuGet: **ninguno nuevo** (`Microsoft.Data.SqlClient` ya viene con `Microsoft.EntityFrameworkCore.SqlServer`).

---

## 2. Capas de la API (v2)

El flujo es `Controller → Servicio → Repositorio → conexión ADO.NET → procedimiento almacenado`.

| Capa | Responsabilidad en la v2 |
|---|---|
| `Controllers/` | Igual que en la v1: traduce HTTP a llamadas al servicio. Declara además las **rutas anidadas** del maestro (ruta absoluta, p. ej. `[HttpPost("/api/docente/{cedula:int}/estudios-realizados")]`). Sin `try/catch`. |
| `Servicios/` | Normaliza (`Trim`), valida lo que no cubren las anotaciones (fecha fin ≥ fecha inicio, clave de ruta = clave del cuerpo, lista de 1 a 100 renglones, `fecha_fin` de `red_docente` con formato `yyyy-MM-dd`), traduce «no hay fila» a `ExcepcionNoEncontrada`. **No** revisa si el padre existe ni si hay duplicados: eso lo hace el procedimiento (una sola fuente, sin carreras). |
| `Repositorios/` | Una clase por tabla con **una llamada por método**: nombre del procedimiento + `SqlParameter`s + mapeo de la fila a DTO. Cero SQL. |
| `Repositorios/EjecutorProcedimientos` | Único apoyo compartido: abre la conexión del `DbContext`, arma el comando con `CommandType.StoredProcedure`, ejecuta y cierra. Ver §3. |
| Base de datos | Todo el SQL, las reglas de integridad y los disparadores. |

### 2.1 Métodos por capa (patrón; ejemplo `Docente`)

`IRepositorioDocente`:

| Método | Procedimiento |
|---|---|
| `ListarAsync()` | `sp_listar_docente` |
| `ObtenerAsync(int cedula)` | `sp_consultar_docente` (devuelve `null` si no hay fila) |
| `CrearAsync(DocenteCrear)` | `sp_crear_docente` (devuelve la fila guardada) |
| `ActualizarAsync(int cedula, DocenteCrear)` | `sp_actualizar_docente` (devuelve la fila guardada) |
| `EliminarAsync(int cedula)` | `sp_eliminar_docente` |

`IServicioDocente`: `ListarAsync`, `ObtenerAsync`, `CrearAsync`, `ReemplazarAsync`, `DesactivarAsync`. Los repositorios devuelven **DTOs de respuesta** directamente (no hay entidades que convertir).

Tablas con envío múltiple añaden `CrearLoteAsync(int cedula, List<XxxCrear>)` → `sp_crear_<tabla>_lote`. Las tablas anidadas añaden `ListarPorDocenteAsync(int cedula)` (o `ListarPorEstudioAsync`) → `sp_listar_<tabla>` con parámetro opcional.

### 2.2 Inversión de dependencias

Igual que en la v1: el controlador depende de `IServicioXxx`; el servicio, de `IRepositorioXxx`. Esto permite probar los servicios con repositorios falsos.

---

## 3. Acceso a procedimientos (ADO.NET sobre el `DbContext`)

Forma de la llamada (referencia; el código final lleva su comentario XML):

```csharp
// EjecutorProcedimientos: abre la conexión del DbContext, ejecuta el SP y cierra.
public async Task<List<T>> LeerAsync<T>(string procedimiento, Func<DbDataReader, T> mapear,
                                         params SqlParameter[] parametros)
{
    var conexion = _contexto.Database.GetDbConnection();
    var estabaAbierta = conexion.State == ConnectionState.Open;
    if (!estabaAbierta) await conexion.OpenAsync();
    try
    {
        await using var comando = conexion.CreateCommand();
        comando.CommandText = procedimiento;                 // solo el nombre
        comando.CommandType = CommandType.StoredProcedure;   // nunca texto SQL
        comando.Parameters.AddRange(parametros);
        await using var lector = await comando.ExecuteReaderAsync();
        var resultado = new List<T>();
        while (await lector.ReadAsync()) resultado.Add(mapear(lector));
        return resultado;
    }
    finally { if (!estabaAbierta) await conexion.CloseAsync(); }
}
// + EjecutarAsync(procedimiento, parametros) para los que no devuelven filas (eliminar)
```

Y un método de repositorio queda así de corto:

```csharp
public async Task<DocenteRespuesta?> ObtenerAsync(int cedula) =>
    (await _ejecutor.LeerAsync("sp_consultar_docente", MapearDocente,
        new SqlParameter("@cedula", cedula))).FirstOrDefault();
```

Reglas del repositorio:

- Parámetros **siempre** como `SqlParameter` tipado. Los opcionales se envían como `DBNull.Value`, nunca como cadena vacía.
- Las fechas `DATE` viajan como `DateTime` con `SqlDbType.Date` y se leen con `DateOnly.FromDateTime(lector.GetDateTime(i))`.
- El envío múltiple serializa la lista a JSON (`snake_case`) y la pasa en **un** parámetro `@json NVARCHAR(MAX)`.
- Las longitudes y rangos ya los validó la API; el procedimiento recibe datos válidos.
- El `EnableRetryOnFailure` de la v1 **no afecta** estas llamadas ni exige transacciones desde C#: la transacción vive dentro del procedimiento.

---

## 4. Manejo de errores

Los servicios siguen lanzando las excepciones de la v1 (`ExcepcionNoEncontrada`, `ExcepcionConflicto`, `ExcepcionValidacion`). Lo nuevo es la traducción de lo que **sube desde la base de datos**:

**`ManejadorExcepcionesSql`** (nuevo; no se modifica el manejador de la v1). Recibe `SqlException` y escribe `{mensaje, detalle}`:

| Origen | Número | HTTP | Mensaje |
|---|---|---|---|
| Procedimiento: padre inexistente o retirado | 50001 | `409` | el propio mensaje del procedimiento (castellano) |
| Procedimiento: pareja repetida, clave repetida, retiro bloqueado, repetido en el lote | 50002 | `409` | ídem |
| Procedimiento: dato inválido que solo la base puede ver | 50003 | `422` | ídem |
| Procedimiento: registro no encontrado | 50004 | `404` | ídem |
| Red de seguridad: clave foránea (nativo SQL Server) | 547 | `409` | «No se pudo guardar: un dato relacionado no existe o el registro está en uso.» |
| Red de seguridad: duplicado (nativo) | 2627 / 2601 | `409` | «Ya existe un registro con esa clave.» |
| Cualquier otro número | — | no lo maneja → cae al manejador de la v1 (`500` genérico) | — |

Nunca se devuelve el texto técnico de SQL Server al cliente.

**Registro en `Program.cs` — el orden importa.** El manejador de la v1 captura *toda* excepción y responde `500`; si el nuevo se registra después, nunca se ejecuta. **[CONFIRMAR con el `Program.cs` de la v1 cómo está implementado el de la v1]**:

```csharp
// Si la v1 usa IExceptionHandler: el nuevo va ANTES de la línea existente.
builder.Services.AddExceptionHandler<ManejadorExcepcionesSql>();   // línea nueva (v2)
builder.Services.AddExceptionHandler<ManejadorExcepciones>();      // línea existente (v1), sin tocar
// Si la v1 usa un middleware en Middleware/: el nuevo se registra DESPUÉS (más cerca del endpoint),
// porque la excepción sube desde el endpoint hacia afuera.
```

Los números 50001–50004 y los textos viven **solo** en los procedimientos; el manejador no los inventa.

---

## 5. Procedimientos almacenados (patrón)

Plantilla de escritura (ejemplo: retirar un maestro con regla R2):

```sql
CREATE OR ALTER PROCEDURE sp_eliminar_docente
    @cedula INT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF NOT EXISTS (SELECT 1 FROM docente WHERE cedula = @cedula AND activo = 1)
            THROW 50004, N'No se encontró el docente.', 1;

        DECLARE @detalle INT =
              (SELECT COUNT(*) FROM estudios_realizados   WHERE docente = @cedula AND activo = 1)
            + (SELECT COUNT(*) FROM evaluacion_docente    WHERE docente = @cedula AND activo = 1)
            + (SELECT COUNT(*) FROM experiecia            WHERE docente = @cedula AND activo = 1)
            + (SELECT COUNT(*) FROM reconocimiento        WHERE docente = @cedula AND activo = 1)
            + (SELECT COUNT(*) FROM docente_departamento  WHERE docente = @cedula AND activo = 1)
            + (SELECT COUNT(*) FROM intereses_futuros     WHERE docente = @cedula AND activo = 1)
            + (SELECT COUNT(*) FROM red_docente           WHERE docente = @cedula AND activo = 1);

        IF @detalle > 0
        BEGIN
            DECLARE @mensaje NVARCHAR(200) =
                CONCAT(N'No se puede retirar: tiene ', @detalle, N' registros activos en su detalle.');
            THROW 50002, @mensaje, 1;
        END;

        UPDATE docente SET activo = 0 WHERE cedula = @cedula;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        THROW;   -- re-lanza el error original con su número
    END CATCH
END;
```

Plantilla de envío múltiple (ejemplo recortado; la lista completa de columnas va en el script):

```sql
CREATE OR ALTER PROCEDURE sp_crear_estudios_realizados_lote
    @docente INT,
    @json    NVARCHAR(MAX)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF NOT EXISTS (SELECT 1 FROM docente WHERE cedula = @docente AND activo = 1)
            THROW 50001, N'No se puede registrar: el docente no existe o está retirado.', 1;

        DECLARE @nuevos TABLE (renglon INT IDENTITY(1,1), id INT, titulo VARCHAR(45) /* … */);
        INSERT INTO @nuevos (id, titulo /* … */)
        SELECT id, titulo /* … */
        FROM OPENJSON(@json) WITH (id INT '$.id', titulo VARCHAR(45) '$.titulo' /* … */);

        -- repetidos dentro del mismo envío / ya existentes en la tabla (activos o retirados)
        -- → THROW 50002 con el número de renglón en el mensaje

        BEGIN TRANSACTION;
            INSERT INTO estudios_realizados (id, titulo, /* … */ docente)
            SELECT id, titulo, /* … */ @docente FROM @nuevos;   -- el disparador corre UNA vez
        COMMIT TRANSACTION;

        SELECT /* mismas columnas que sp_listar_estudios_realizados */
        FROM estudios_realizados e JOIN docente d ON d.cedula = e.docente
        WHERE e.id IN (SELECT id FROM @nuevos) ORDER BY e.id;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END;
```

Convenciones de los procedimientos:

- **Mensajes con prefijo `N'…'`** (Unicode) para que las tildes viajen bien.
- `sp_listar_`, `sp_consultar_`, `sp_crear_` y `sp_actualizar_` devuelven **el mismo conjunto de columnas** (la fila + los nombres del padre para mostrar), así el mapeo del repositorio es uno solo por tabla.
- Los `JOIN` con el padre **no filtran por `activo` del padre** (regla R7): un registro de la v2 sigue mostrando su nombre aunque la v1 haya retirado al padre.
- `sp_crear_` en tablas de clave compuesta o 1:1 implementa R3: si existe retirada → la reactiva (`UPDATE … SET activo = 1, <atributos>`); si existe activa → `THROW 50002`.
- `sp_crear_` en tablas con clave enviada por el cliente (`docente`, `estudios_realizados`): si existe (activa o retirada) → `THROW 50002`.

---

## 6. Disparadores

Ambos en `v2_procedimientos.sql`, con comentario de cabecera que explica el porqué.

**Columna nueva (idempotente):**

```sql
IF COL_LENGTH('docente', 'total_estudios') IS NULL
    ALTER TABLE docente ADD total_estudios INT NOT NULL
        CONSTRAINT df_docente_total_estudios DEFAULT 0;
GO
```

**`trg_contar_estudios`** (en `estudios_realizados`, `AFTER INSERT, UPDATE, DELETE`):

```sql
CREATE OR ALTER TRIGGER trg_contar_estudios
ON estudios_realizados
AFTER INSERT, UPDATE, DELETE
AS
BEGIN
    SET NOCOUNT ON;
    -- 1) nada que hacer si el lote no afectó filas
    IF NOT EXISTS (SELECT 1 FROM inserted) AND NOT EXISTS (SELECT 1 FROM deleted) RETURN;
    -- 2) un UPDATE que no toca ni 'activo' ni 'docente' no cambia ningún contador
    IF EXISTS (SELECT 1 FROM inserted) AND EXISTS (SELECT 1 FROM deleted)
       AND NOT UPDATE(activo) AND NOT UPDATE(docente) RETURN;

    -- 3) una sola pasada por conjuntos: todos los docentes tocados (los viejos y los nuevos)
    UPDATE d SET d.total_estudios = (
        SELECT COUNT(*) FROM estudios_realizados e
        WHERE e.docente = d.cedula AND e.activo = 1)
    FROM docente d
    WHERE d.cedula IN (SELECT docente FROM inserted UNION SELECT docente FROM deleted);
END;
```

**`trg_sello_docente`** (en `docente`, `AFTER UPDATE`): fija `fecha_actualizacion = CAST(GETDATE() AS DATE)` para las filas de `inserted`, **solo si** el `UPDATE` tocó alguna columna editable (`UPDATE(nombres) OR UPDATE(apellidos) OR … OR UPDATE(activo)`), de modo que el cambio automático de `total_estudios` no altera el sello. Sale temprano si `inserted` está vacío. No se auto-invoca porque `RECURSIVE_TRIGGERS` está apagado por defecto y la guarda de columnas lo evitaría de todos modos.

**Por qué por conjuntos:** un envío de 100 renglones es un solo `INSERT`, así que `trg_contar_estudios` corre **una vez** con 100 filas en `inserted`. Un disparador escrito fila por fila (con cursor) correría 100 veces.

---

## 7. Arquitectura del front (v2)

Flujo igual al de la v1: `Página (PageModel) → IClienteXxx → HttpClient → API`.

- **Un cliente por tabla** (11), sobre `ClienteApiBase`, con los métodos explícitos de la v1 más `CrearLoteAsync` donde aplica y `ListarPorDocenteAsync` / `ListarPorEstudioAsync` para las secciones del detalle.
- **Registro en `Program.cs` del front:** 11 líneas `AddHttpClient<IClienteXxx, ClienteXxx>` añadidas junto a las de la v1.
- **Páginas:**
  - `Docentes/Index`, `Crear`, `Editar` (cedula de solo lectura).
  - `Docentes/Detalle`: datos del docente + siete secciones (estudios, evaluaciones, experiencias, reconocimientos, departamentos, intereses, redes). Cada sección es una **vista parcial** con su tabla, su botón «Agregar renglones» y los botones Editar/Retirar. Los handlers (`OnPostAgregarEstudiosAsync`, `OnPostRetirarEstudioAsync`, …) viven en el `PageModel` del Detalle.
  - `Docentes/<Seccion>/Editar` (estudios, evaluaciones, experiencias, reconocimientos, departamentos, redes): el docente ya viene en la ruta; no se vuelve a elegir.
  - `EstudiosRealizados/Detalle`: apoyo (un formulario que crea o actualiza según exista), beca (ídem) y áreas de conocimiento (asignar/retirar con desplegable).
- **Formulario de varios renglones:** JavaScript sencillo (sin librerías) añade filas; el envío enlaza una lista (`Renglones[0].Titulo`, `Renglones[1].Titulo`, …) y el `PageModel` llama **una sola vez** a `CrearLoteAsync`.
- **Desplegables:** el `PageModel` los carga en `OnGetAsync` **y** al repintar tras un `409`/`422`. Cada opción: texto = nombre, `value` = clave. Si un desplegable queda vacío, la vista muestra el aviso con enlace a la pantalla donde crear el primero (ver `6_contracts.md` §5).
- **Errores:** igual que en la v1 (`ErrorApiException` → `409`/`422` al formulario, `404` redirige, `503` aviso). El texto del mensaje viene en castellano desde la API; el front no traduce números ni usa jerga.
- **Barra de navegación:** un enlace nuevo «Docentes».

---

## 8. Base de datos y Docker

- `gestion_profesoral.sql` (v1) **no cambia**.
- `init.sh` ejecuta, **después** del script de la v1 y solo en la primera creación:

```bash
sqlcmd -C -S localhost -U sa -P "$MSSQL_SA_PASSWORD" -d gestion_local -I -b -i /scripts/v2_procedimientos.sql
```

  `-b` hace que `sqlcmd` termine con error si el script falla (el contenedor quedaría en `Exited (1)` en lugar de ocultar el problema) e `-I` activa `QUOTED_IDENTIFIER`, requerido por los disparadores.
- `docker-compose.yml`: **sin cambios** (ya monta `./db/sqlserver` en `/scripts`).
- Como siempre: un cambio de SQL se aplica con `docker compose down -v` + `up -d`. Para iterar sin borrar datos, `v2_procedimientos.sql` se escribe **idempotente** (`CREATE OR ALTER`, `COL_LENGTH`) y se puede reejecutar a mano con `sqlcmd`.

---

## 9. Orden de arranque

Igual que en la v1, con un paso más dentro de `sqlserver-init`: crear la base → script de la v1 (19 tablas + 218 áreas) → **script de la v2** (columna, procedimientos, disparadores). La API y el front arrancan sin esperar.

---

## 10. Riesgos y mitigaciones

| Riesgo | Mitigación |
|---|---|
| El manejador de la v1 captura todo y la v2 responde `500` en vez de `409` | Registrar `ManejadorExcepcionesSql` en el orden correcto (§4) y probar el criterio CA-V2-07 desde la colección de pruebas |
| Tildes corrompidas en los mensajes de los procedimientos | Literales `N'…'` y archivo guardado en UTF-8 |
| `v2_procedimientos.sql` falla a la mitad y la base queda a medias | `-b` en `sqlcmd`; script idempotente; `down -v` + `up -d` |
| `programa` empieza vacía y el desplegable de departamentos no tiene opciones | Aviso con enlace a `/Programas/Crear` (RF14.6) |
| Contador `total_estudios` desfasado si alguien edita la base a mano | El disparador recalcula con `COUNT(*)`, no con `+1/-1`: cualquier cambio posterior lo corrige |
| Retirar un padre de la v1 usado por la v2 | Limitación documentada (R7) |
| `InMemory` no ejecuta procedimientos ni disparadores | Servicios con repositorios falsos + `v2_verificacion.sql` + colección de pruebas |
| Un lote muy grande agota la memoria o el tiempo | Tope de 100 renglones (`422`) |
| Carpetas de la v1 distintas a las del plan | Marcas **[CONFIRMAR]**: se ajusta a lo real, sin mover nada de la v1 |
