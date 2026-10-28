# Modelo de datos — gestion_local (v2)

> Documento 5 de 8 del spec kit de la v2.
> Las 11 tablas con clave foránea ya existen en `db/sqlserver/gestion_profesoral.sql` (v1, no se toca). La v2 solo agrega **una columna**, **procedimientos** y **disparadores** en `db/sqlserver/v2_procedimientos.sql`.
> Todas las columnas son `NOT NULL` salvo las marcadas `NULL`. Todas las tablas tienen `activo BIT NOT NULL DEFAULT 1`.

---

## 1. Las 11 tablas de la v2

### 1.1 Maestro: `docente`

| Columna | Tipo | Nota |
|---|---|---|
| `cedula` | INT **PK** | La envía el cliente |
| `nombres` · `apellidos` | VARCHAR(60) | |
| `genero` | VARCHAR(12) | |
| `cargo` | VARCHAR(30) | |
| `fecha_nacimiento` | DATE | |
| `correo` | VARCHAR(70) | |
| `telefono` | VARCHAR(20) | |
| `url_cvlac` | VARCHAR(128) | |
| `fecha_actualizacion` | DATE | La fija la base (alta y `trg_sello_docente`); **no** la envía el cliente |
| `escalafon` | VARCHAR(45) | |
| `perfil` | VARCHAR(MAX) | |
| `cat_minciencia` | VARCHAR(45) **NULL** | |
| `conv_minciencia` | VARCHAR(45) | |
| `nacionalidaad` | VARCHAR(45) | Nombre del esquema, con su error de digitación (R1) |
| `linea_investigacion_principal` | INT **NULL** · **FK** → `linea_investigacion(id)` | **Clave foránea opcional** («(ninguna)» → `null`) |
| `total_estudios` | INT NOT NULL DEFAULT 0 | **Nueva en la v2.** La mantiene `trg_contar_estudios`; **no** la envía el cliente |
| `activo` | BIT | |

### 1.2 Detalle directo de `docente`

| Tabla | Clave | Columnas (además de `docente` FK → `docente(cedula)` y `activo`) |
|---|---|---|
| `estudios_realizados` | `id` INT **PK** (la envía el cliente) | `titulo` VARCHAR(45) · `universidad` VARCHAR(50) · `fecha` DATE · `tipo` VARCHAR(45) · `ciudad` VARCHAR(45) · `ins_acreditada` TINYINT (0/1) · `metodologia` VARCHAR(45) · `perfil_egresado` VARCHAR(MAX) · `pais` VARCHAR(45) |
| `evaluacion_docente` | `id` INT **IDENTITY** | `calificacion` REAL (0 a 5) · `semestre` VARCHAR(45) |
| `reconocimiento` | `id` INT **IDENTITY** | `tipo` VARCHAR(45) · `fecha` DATE · `institucion` VARCHAR(45) · `nombre` VARCHAR(45) · `ambito` VARCHAR(45) |
| `experiecia` | `id` INT **IDENTITY** | `nombre_cargo` VARCHAR(45) · `institucion` VARCHAR(45) · `tipo` VARCHAR(45) · `fecha_inicio` DATE · `fecha_fin` DATE **NULL** |

### 1.3 Detalle 1:1 de `estudios_realizados`

La clave primaria **es** la clave foránea: un estudio tiene a lo sumo un apoyo y una beca.

| Tabla | Clave (PK = FK) | Columnas |
|---|---|---|
| `apoyo_profesoral` | `estudios` INT → `estudios_realizados(id)` | `con_apoyo` TINYINT (0/1) · `institucion` VARCHAR(45) · `tipo` VARCHAR(45) |
| `beca` | `estudios` INT → `estudios_realizados(id)` | `tipo` VARCHAR(45) · `institucion` VARCHAR(**80**) · `fecha_inicio` DATE · `fecha_fin` DATE **NULL** |

### 1.4 Tablas puente (PK compuesta, dos FK)

| Tabla | Clave primaria | Referencias | Atributos |
|---|---|---|---|
| `docente_departamento` | (`docente`, `departamento`) | `docente` → `docente(cedula)` · `departamento` → **`programa(id)`** | `dedicacion` VARCHAR(15) · `modalidad` VARCHAR(45) · `fecha_ingreso` DATE · `fecha_salida` DATE **NULL** |
| `red_docente` | (`red`, `docente`) | `red` → `red(idr)` · `docente` → `docente(cedula)` | `fecha_inicio` DATE · `fecha_fin` **VARCHAR(45) NULL** · `act_destacadas` VARCHAR(MAX) |
| `intereses_futuros` | (`docente`, `termino_clave`) | `docente` → `docente(cedula)` · `termino_clave` → `termino_clave(termino)` | — (puente puro) |
| `estudio_ac` | (`estudio`, `area_conocimiento`) | `estudio` → `estudios_realizados(id)` · `area_conocimiento` → `area_conocimiento(id)` (VARCHAR(6)) | — (puente puro) |

### Aclaraciones

- `docente_departamento.departamento` apunta a **`programa`**, no a una tabla `departamento`: en el desplegable se muestra el nombre del programa.
- `red_docente.fecha_fin` es `VARCHAR(45)` en el esquema dado (las demás fechas de la v2 son `DATE`). Se conserva y se valida con formato `yyyy-MM-dd` (D44).
- `nacionalidaad` y `experiecia` se conservan con su ortografía (R1/D43).

---

## 2. Cambios a la base de datos en la v2

Un solo archivo nuevo: `db/sqlserver/v2_procedimientos.sql` (idempotente). Contenido:

1. `ALTER TABLE docente ADD total_estudios INT NOT NULL DEFAULT 0` (protegido con `COL_LENGTH`).
2. **53 procedimientos** de los cinco verbos (11 tablas × 5, menos 2 `sp_actualizar_` de las puentes puras).
3. **8 procedimientos de lote** `sp_crear_<tabla>_lote`.
4. **2 disparadores**: `trg_contar_estudios`, `trg_sello_docente`.

El script de la v1 queda **intacto**.

---

## 3. Catálogo de procedimientos

Convención de parámetros: una columna de la tabla = un parámetro con el mismo nombre y tipo (`@cedula`, `@nombres`, …). Los opcionales se declaran `= NULL`. `sp_actualizar_` recibe la clave y los campos editables.

| Tabla | Procedimientos | Particularidades |
|---|---|---|
| `docente` | `sp_listar_docente`, `sp_consultar_docente (@cedula)`, `sp_crear_docente`, `sp_actualizar_docente`, `sp_eliminar_docente (@cedula)` | `crear`: cédula repetida (activa o retirada) → 50002; línea principal inexistente → 50001. `eliminar`: bloqueado si hay detalle activo (R2). `crear` fija `fecha_actualizacion` con la fecha de hoy |
| `estudios_realizados` | los 5 + `sp_crear_estudios_realizados_lote (@docente, @json)` | `listar` acepta `@docente = NULL` (filtra por docente). `crear`: docente inexistente → 50001; id repetido → 50002. `eliminar`: bloqueado si hay apoyo, beca o áreas activas (R2) |
| `evaluacion_docente` | los 5 + `…_lote` | `listar` con `@docente = NULL`. `calificacion` fuera de 0–5 → 50003 (red de seguridad; la API ya valida) |
| `reconocimiento` | los 5 + `…_lote` | `listar` con `@docente = NULL` |
| `experiecia` | los 5 + `…_lote` | `listar` con `@docente = NULL`; `fecha_fin < fecha_inicio` → 50003 |
| `apoyo_profesoral` | los 5 | PK = FK: `crear` reactiva si existe retirado (R3); activo → 50002 |
| `beca` | los 5 | Ídem |
| `docente_departamento` | los 5 + `…_lote` | `crear`: docente o programa inexistente → 50001; pareja activa → 50002; retirada → reactiva. `actualizar`: solo atributos |
| `red_docente` | los 5 + `…_lote` | Ídem con `red` y `docente` |
| `intereses_futuros` | `listar`, `consultar`, `crear`, `eliminar` + `…_lote` | Sin `actualizar` (R6). Pareja activa → 50002; retirada → reactiva |
| `estudio_ac` | `listar`, `consultar`, `crear`, `eliminar` + `…_lote` | Sin `actualizar` (R6). `listar` acepta `@estudio = NULL` |

**Columnas que devuelven** `sp_listar_`, `sp_consultar_`, `sp_crear_` y `sp_actualizar_` (siempre las mismas por tabla): todas las columnas **menos `activo`**, más los nombres del padre para mostrar:

| Tabla | Columnas extra |
|---|---|
| `docente` | `linea_investigacion_principal_nombre` |
| `estudios_realizados`, `evaluacion_docente`, `reconocimiento`, `experiecia` | `docente_nombre` (= `nombres + ' ' + apellidos`) |
| `apoyo_profesoral`, `beca` | `estudio_titulo` |
| `docente_departamento` | `docente_nombre`, `departamento_nombre` (nombre del programa) |
| `red_docente` | `red_nombre`, `docente_nombre` |
| `intereses_futuros` | `docente_nombre` |
| `estudio_ac` | `estudio_titulo`, `area_conocimiento_disciplina` |

Los `JOIN` **no filtran `activo` del padre** (R7); el filtro `activo = 1` se aplica a la tabla listada.

---

## 4. Errores de los procedimientos

| Número | Significado | HTTP | Ejemplo de mensaje |
|---|---|---|---|
| 50001 | Padre inexistente o retirado | 409 | «No se puede registrar: el docente 99 no existe o está retirado.» |
| 50002 | Duplicado, pareja repetida, retiro bloqueado, repetido dentro del lote | 409 | «El docente 10 ya tiene asignado el programa 3.» · «No se puede retirar: tiene 5 registros activos en su detalle.» · «El renglón 3 repite el id 7.» |
| 50003 | Dato inválido que solo la base ve | 422 | «La calificación debe estar entre 0 y 5.» |
| 50004 | Registro no encontrado | 404 | «No se encontró el docente con cédula 99.» |

Los nativos 547, 2627 y 2601 son red de seguridad (ver `3_plan.md` §4).

---

## 5. Disparadores

| Disparador | Tabla | Eventos | Qué hace |
|---|---|---|---|
| `trg_contar_estudios` | `estudios_realizados` | `INSERT`, `UPDATE`, `DELETE` | Recalcula `docente.total_estudios` (solo estudios activos) para todos los docentes tocados, en una pasada por conjuntos. Cubre alta, retiro (que es `UPDATE`), reasignación de docente y borrado físico accidental |
| `trg_sello_docente` | `docente` | `UPDATE` | Fija `fecha_actualizacion` con la fecha de hoy si el `UPDATE` tocó una columna editable; ignora el cambio automático de `total_estudios` |

Código y razones en `3_plan.md` §6 y `4_research.md` D37–D39.

---

## 6. Reglas de validación por campo (API y front)

Se aplican con anotaciones en `Peticiones/` (API) y en `Modelos/` (front). Los textos obligatorios rechazan vacío y solo espacios. Las claves `int` no anulables usan `[Range(1, int.MaxValue)]`. Las fechas usan `DateOnly` (`yyyy-MM-dd`; una fecha inválida como `2024-13-45` da `422`). Los topes de `VARCHAR(MAX)` se fijan en **4000** caracteres.

| Tabla | Campo | Regla |
|---|---|---|
| `docente` | `cedula` | obligatorio, ≥ 1 |
| | `nombres`, `apellidos` | obligatorios, ≤ 60 |
| | `genero` | obligatorio, ≤ 12 |
| | `cargo` | obligatorio, ≤ 30 |
| | `fecha_nacimiento` | obligatoria, no futura |
| | `correo` | obligatorio, ≤ 70, formato de correo |
| | `telefono` | obligatorio, ≤ 20, dígitos, espacios, `+` y `-` |
| | `url_cvlac` | obligatoria, ≤ 128, regex `^https?://\S+$` |
| | `escalafon`, `conv_minciencia`, `nacionalidaad` | obligatorios, ≤ 45 |
| | `perfil` | obligatorio, ≤ 4000 |
| | `cat_minciencia` | opcional, ≤ 45 |
| | `linea_investigacion_principal` | **opcional** (`null`); si viene, ≥ 1 |
| | `fecha_actualizacion`, `total_estudios` | **no se envían** (anti *over-posting*, D19) |
| `estudios_realizados` | `id` | obligatorio, ≥ 1 |
| | `titulo`, `tipo`, `ciudad`, `metodologia`, `pais` | obligatorios, ≤ 45 |
| | `universidad` | obligatorio, ≤ 50 |
| | `fecha` | obligatoria |
| | `ins_acreditada` | obligatorio, 0 o 1 |
| | `perfil_egresado` | obligatorio, ≤ 4000 |
| | `docente` | obligatorio, ≥ 1 (en el envío múltiple viene de la ruta) |
| `evaluacion_docente` | `calificacion` | obligatoria, entre 0 y 5 (R4) |
| | `semestre` | obligatorio, ≤ 45 |
| | `docente` | obligatorio, ≥ 1 |
| `reconocimiento` | `tipo`, `institucion`, `nombre`, `ambito` | obligatorios, ≤ 45 |
| | `fecha` | obligatoria |
| | `docente` | obligatorio, ≥ 1 |
| `experiecia` | `nombre_cargo`, `institucion`, `tipo` | obligatorios, ≤ 45 |
| | `fecha_inicio` | obligatoria |
| | `fecha_fin` | opcional; si viene, ≥ `fecha_inicio` |
| | `docente` | obligatorio, ≥ 1 |
| `apoyo_profesoral` | `estudios` | obligatorio, ≥ 1 |
| | `con_apoyo` | obligatorio, 0 o 1 |
| | `institucion`, `tipo` | obligatorios, ≤ 45 |
| `beca` | `estudios` | obligatorio, ≥ 1 |
| | `tipo` | obligatorio, ≤ 45 |
| | `institucion` | obligatorio, ≤ 80 |
| | `fecha_inicio` | obligatoria |
| | `fecha_fin` | opcional; si viene, ≥ `fecha_inicio` |
| `docente_departamento` | `docente`, `departamento` | obligatorios, ≥ 1 |
| | `dedicacion` | obligatoria, ≤ 15 |
| | `modalidad` | obligatoria, ≤ 45 |
| | `fecha_ingreso` | obligatoria |
| | `fecha_salida` | opcional; si viene, ≥ `fecha_ingreso` |
| `red_docente` | `red`, `docente` | obligatorios, ≥ 1 |
| | `fecha_inicio` | obligatoria |
| | `fecha_fin` | opcional, **texto** `yyyy-MM-dd` válido y ≥ `fecha_inicio` (D44) |
| | `act_destacadas` | obligatoria, ≤ 4000 |
| `intereses_futuros` | `docente` | obligatorio, ≥ 1 |
| | `termino_clave` | obligatorio, ≤ 30 |
| `estudio_ac` | `estudio` | obligatorio, ≥ 1 |
| | `area_conocimiento` | obligatorio, ≤ 6 |
| Envío múltiple | lista de renglones | de 1 a **100** elementos; cada uno cumple las reglas de su tabla |

En un `PUT`, la clave de la ruta debe coincidir con la del cuerpo si el cuerpo la trae (`422` si no).

---

## 7. Mapeo a la API y al front

Sin entidades EF: el repositorio mapea cada fila directamente a un DTO de respuesta.

| Tabla | DTO de entrada | DTO de respuesta |
|---|---|---|
| `docente` | `DocenteCrear` | `DocenteRespuesta` |
| `estudios_realizados` | `EstudiosRealizadosCrear` | `EstudiosRealizadosRespuesta` |
| `evaluacion_docente` | `EvaluacionDocenteCrear` | `EvaluacionDocenteRespuesta` |
| `reconocimiento` | `ReconocimientoCrear` | `ReconocimientoRespuesta` |
| `experiecia` | `ExperieciaCrear` | `ExperieciaRespuesta` |
| `apoyo_profesoral` | `ApoyoProfesoralCrear` | `ApoyoProfesoralRespuesta` |
| `beca` | `BecaCrear` | `BecaRespuesta` |
| `docente_departamento` | `DocenteDepartamentoCrear` | `DocenteDepartamentoRespuesta` |
| `red_docente` | `RedDocenteCrear` | `RedDocenteRespuesta` |
| `intereses_futuros` | `InteresesFuturosCrear` | `InteresesFuturosRespuesta` |
| `estudio_ac` | `EstudioAcCrear` | `EstudioAcRespuesta` |

Propiedades en `PascalCase` (`LineaInvestigacionPrincipal`, `TotalEstudios`, `ActDestacadas`); JSON en `snake_case`. Tipos: `VARCHAR` → `string`, `INT` → `int`, `TINYINT` → `byte`, `REAL` → `float`, `DATE` → `DateOnly`, columnas `NULL` → tipo anulable (`int?`, `DateOnly?`, `string?`).

---

## 8. Borrado lógico (columna `activo`)

- `DELETE` en la API = `sp_eliminar_<tabla>` = `UPDATE … SET activo = 0`. La fila no se elimina.
- Todo `sp_listar_` filtra `activo = 1`; por eso también lo hacen los desplegables.
- Retirar un maestro con detalle activo → `409` (R2).
- Puentes y 1:1: reasignar una pareja retirada la **reactiva** (R3). Tablas con clave enviada por el cliente (`docente`, `estudios_realizados`): el id retirado sigue dando `409`.

---

## 9. Datos iniciales

Las 11 tablas de la v2 arrancan **vacías** (0 filas). Dependencias de arranque:

| Para crear… | Hay que tener antes… |
|---|---|
| un `docente` | nada (la línea principal es opcional) |
| un `estudios_realizados` y los demás hijos | un `docente` |
| `apoyo_profesoral`, `beca`, `estudio_ac` | un `estudios_realizados` (y, para `estudio_ac`, las 218 áreas ya cargadas) |
| `docente_departamento` | un `docente` y un `programa` (la tabla `programa` de la v1 **empieza vacía**: se crea desde `/Programas`) |
| `red_docente` | un `docente` y una `red` (de la v1) |
| `intereses_futuros` | un `docente` y un `termino_clave` (de la v1) |

**Regla de oro (se mantiene):** un cambio de esquema se hace en el script correspondiente y se aplica con `docker compose down -v` + `up -d`.
