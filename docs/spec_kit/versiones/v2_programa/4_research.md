# Registros de Decisión (ADR) — Gestión Profesoral v2

> Documento 4 de 8 del spec kit de la v2 · Lectura opcional (contexto de por qué el plan es como es).
> La v1 llegó a **D30**; las decisiones de esta versión continúan desde **D31**.

---

## Base de datos

### D31 — Todo el CRUD de la v2 por procedimientos almacenados
Alternativas: (a) LINQ/EF como en la v1, (b) SQL escrito en los repositorios, (c) procedimientos almacenados. **Decisión: (c).** El SQL queda en **un solo sitio**, con nombre, versionado en `v2_procedimientos.sql`, y los parámetros viajan como parámetros (sin inyección). Las 5 tablas de la v1 **no migran**: está cerrada y se verifica por regresión.

### D32 — Cinco verbos con nombre fijo
`sp_listar_`, `sp_consultar_`, `sp_crear_`, `sp_actualizar_`, `sp_eliminar_<tabla>`, más `sp_crear_<tabla>_lote`. Un nombre predecible por operación y tabla permite encontrar cualquier procedimiento sin buscar. `sp_crear_` y `sp_actualizar_` devuelven la fila **como quedó guardada**, para que la API responda con los valores por defecto y lo que calculó un disparador.

### D33 — Las puentes puras no tienen `sp_actualizar_`
En `intereses_futuros` y `estudio_ac` la fila **es** la pareja: no hay nada que modificar sin cambiar la identidad. Un `sp_actualizar_` ahí sería un procedimiento vacío o un cambio de clave disfrazado. Se documenta como excepción (R6) y se confirma con el profesor.

### D34 — Errores propios con `THROW` y números 50001–50004
La base **valida antes de escribir** y lanza errores con mensaje en castellano: 50001 padre inexistente (`409`), 50002 duplicado o retiro bloqueado (`409`), 50003 dato inválido (`422`), 50004 no encontrado (`404`). Así el mensaje se escribe una vez, junto a la regla que lo produce. Los números nativos 547 (clave foránea) y 2627/2601 (duplicado) quedan como **red de seguridad** para la carrera entre dos peticiones simultáneas.

### D35 — La transacción vive dentro del procedimiento
La v1 activa `EnableRetryOnFailure`, y con esa estrategia EF Core no permite abrir transacciones manuales desde C#. Además un envío múltiple debe ser todo o nada. **Decisión:** `SET XACT_ABORT ON` + `BEGIN TRANSACTION`/`COMMIT` dentro del procedimiento, con `TRY/CATCH` que hace `ROLLBACK` y re-lanza con `THROW;`.

### D36 — Envío múltiple con `OPENJSON`
Alternativas: parámetros con valores de tabla (exigen crear tipos y una API de ADO.NET más pesada), llamar N veces al procedimiento (N peticiones: «medio registro»). **Decisión:** una lista JSON en un parámetro `NVARCHAR(MAX)`, expandida con `OPENJSON` en un solo `INSERT … SELECT`. Tope de **100** renglones. Costo: las longitudes se validan en la API antes de enviar (el procedimiento no es la primera línea de validación).

### D37 — Disparador A: contador recalculado, no incrementado
`trg_contar_estudios` hace `COUNT(*)` de los activos del docente en lugar de sumar o restar 1: es **idempotente**, corrige cualquier desfase y soporta lotes y cambios de docente. Escucha `INSERT`, `UPDATE` y `DELETE` porque con borrado lógico retirar es un `UPDATE` (un disparador sordo al `UPDATE` deja el contador mal justo al retirar). Trabaja por conjuntos con `inserted` y `deleted`, sale temprano si no hay filas o si el `UPDATE` no toca `activo` ni `docente`.

### D38 — Disparador B: sello que ignora el contador
`trg_sello_docente` pone `fecha_actualizacion` solo cuando el `UPDATE` toca una columna editable. Sin esa guarda, cada vez que el disparador A recalculara `total_estudios` también cambiaría la fecha de actualización del docente, y la fecha dejaría de significar «la última vez que alguien editó al docente».

### D39 — `total_estudios` cuenta solo activos
Coherente con el borrado lógico: un estudio retirado no cuenta (R5). La columna se agrega con `ALTER TABLE` en el script de la v2 y **no** en el de la v1 (cerrado).

### D40 — Reactivación en tablas de clave compuesta o 1:1
En una puente (o en `apoyo_profesoral`/`beca`) la identidad de la fila **es** la pareja. Si alguien retira y vuelve a asignar, lo natural es que reaparezca; bloquearlo con `409` obligaría a editar la base a mano. En cambio en `docente` y `estudios_realizados` (clave enviada por el cliente) un id retirado sigue dando `409`, como en la v1. La diferencia es deliberada y está escrita (R3).

### D41 — Retirar un maestro con detalle activo da `409`
Con borrado lógico la base no lo impide (es un `UPDATE`), así que es regla de negocio nuestra (R2). Alternativas: retirar en cascada (silenciosamente borra datos) o permitirlo (deja hijos activos de un padre retirado, que luego los listados no sabrían mostrar). **Decisión:** bloquear y pedir que primero se retire el detalle.

### D42 — Limitación: la v1 no bloquea retirar padres usados por la v2
`programa`, `red`, `termino_clave`, `linea_investigacion` y `area_conocimiento` se retiran por el código de la v1, que está cerrado. Las filas de la v2 que los usan siguen mostrando su nombre (los `JOIN` no filtran `activo` del padre) y los desplegables simplemente dejan de ofrecerlos. Se documenta como limitación (R7) en vez de tocar la v1.

### D43 — Nombres del esquema tal cual
`experiecia` y `nacionalidaad` tienen errores de digitación en el esquema dado. Corregirlos crearía una segunda fuente de verdad entre el script del curso y el nuestro. Se conservan (R1); las rutas y el JSON los reflejan (`/api/experiecia`, `nacionalidaad`).

### D44 — Fechas `DATE` reales, con una excepción heredada
A diferencia de la v1 (fechas en `VARCHAR(45)`), las tablas de la v2 usan `DATE`: la API usa `DateOnly` y el JSON `yyyy-MM-dd`. Excepción: `red_docente.fecha_fin` es `VARCHAR(45)` en el esquema dado; se valida con el mismo formato y se trata como texto (como hacía la v1 con todas sus fechas).

### D45 — `calificacion` entre 0 y 5
La columna es `REAL` sin restricción. Se limita a [0, 5] (R4) en la API; es una decisión de negocio del equipo, no del esquema.

### D46 — Claves enviadas por el cliente
`docente.cedula` y `estudios_realizados.id` no son `IDENTITY` en el esquema; las envía el cliente, igual que `programa.id` y `red.idr` en la v1 (R8). Las otras tres tablas hijas sí usan `IDENTITY`.

---

## Tecnologías y arquitectura

### D47 — ADO.NET sobre la conexión del `DbContext`, sin entidades EF
Alternativas: `FromSqlRaw`/`ExecuteSqlRaw` (el Art. 8 de la v1 los miraba con recelo y exigirían mapear entidades nuevas) o `DbContext` con entidades nuevas (un segundo lugar donde describir cada columna, la «doble edición» que D14 ya criticaba). **Decisión:** `Database.GetDbConnection()` con `CommandType.StoredProcedure` y `SqlParameter`; cada repositorio mapea el resultado directo a un DTO. El procedimiento es la única descripción de las columnas.

### D48 — `EjecutorProcedimientos` como excepción a «sin genéricos»
D28 prefiere código explícito por entidad. Aquí un único apoyo compartido abre la conexión, arma el comando y cierra; **no contiene lógica por tabla**. Repetir ese bloque en 53 métodos multiplicaría el riesgo (una conexión sin cerrar, un parámetro sin tipar). Cada repositorio sigue siendo explícito: nombre del procedimiento, parámetros y mapeo a la vista.

### D49 — `ManejadorExcepcionesSql` aparte, registrado antes que el de la v1
El manejador de la v1 es un «atrapa-todo» que responde `500`. Se escribe uno nuevo **sin modificar** el de la v1 y se registra antes para que gane en lo que es suyo (`SqlException`) y deje pasar el resto. Un `500` por una clave foránea sería decir «me caí» cuando la verdad es «ese padre no existe».

### D50 — Validar formato en la API, integridad en la base
La API (DTOs y servicio) valida longitudes, formatos y rangos y devuelve `422`; la base valida lo que solo ella puede ver (existencia del padre, duplicados, retiro bloqueado). Evita duplicar reglas y evita que una carrera entre dos peticiones pase la validación de C#.

### D51 — Listas planas, no sobre
La guía de la v2 menciona un sobre `{tabla, limite, total, datos}`; la v1 (cerrada y de número menor en la jerarquía) devuelve listas planas `[]`. **Decisión:** se mantiene la lista plana en todo el proyecto, para que el front tenga una sola forma de leer respuestas.

### D52 — Maestro-detalle en la misma pantalla, sin menú aparte
Un `estudios_realizados` no existe sin su `docente`: ofrecer un menú «Estudios» que pregunte de nuevo de qué docente es obliga a elegir lo que la pantalla ya sabe. El detalle vive en `Docentes/Detalle`; el único enlace nuevo de la barra es «Docentes». Las rutas de edición de hijos llevan al docente en la URL.

### D53 — Un envío, una petición
El formulario agrega filas con JavaScript simple y el `PageModel` llama una sola vez a `CrearLoteAsync`. Tres renglones en tres peticiones dejan «medio registro» si la segunda falla. El mismo principio explica la transacción única del procedimiento (D35).

### D54 — Desplegables desde la API, solo activos
Las opciones salen de los `GET` de la API (que ya filtran `activo = 1`), no de listas fijas en el front. Ofrecer un padre retirado es ofrecer una opción que la base va a rechazar. Una clave opcional manda `null` (no cadena vacía: una cadena vacía donde va un número produce un error de conversión técnico y en otro idioma).

### D55 — Pruebas: repositorios falsos y script SQL de verificación
`InMemory` no ejecuta procedimientos ni disparadores (D27 ya lo decía de los `DEFAULT`). **Decisión:** los servicios se prueban con repositorios falsos (por eso existen las interfaces); los procedimientos y disparadores se comprueban con `v2_verificacion.sql` y con la colección de pruebas, **incluidas las peticiones que deben fallar** (`409`, `422`, `404`).

### D56 — La v1 se verifica, no se edita
Regresión completa de la v1 el día de la entrega. Las únicas líneas añadidas a archivos de la v1 son registros en `Program.cs`, el enlace del `_Layout` y la llamada al script de la v2 en `init.sh`.
