# Modelo de datos — gestion_local (v1)

> Documento 5 de 8 del spec kit raíz.
> La base existe en un solo motor: SQL Server (`db/sqlserver/gestion_profesoral.sql` + `db/sqlserver/init.sh`). Este documento define las **5 tablas independientes** (sin claves foráneas) que implementa la v1, sus reglas de validación y su mapeo a EF Core.

---

## 1. Las 5 tablas de la v1

Todas las columnas son `NOT NULL` salvo las marcadas `NULL`.

| Tabla | Clave | Columnas |
|---|---|---|
| `area_conocimiento` | `id` VARCHAR(6) | `gran_area` VARCHAR(60) · `area` VARCHAR(60) · `disciplina` VARCHAR(150) · `activo` BIT DEFAULT 1 |
| `termino_clave` | `termino` VARCHAR(30) | `termino_ingles` VARCHAR(30) **NULL** · `activo` BIT DEFAULT 1 |
| `linea_investigacion` | `id` INT **IDENTITY** | `nombre` VARCHAR(45) · `descripcion` VARCHAR(256) · `activo` BIT DEFAULT 1 |
| `programa` | `id` INT (**sin** IDENTITY) | `nombre` VARCHAR(60) · `tipo` VARCHAR(45) · `nivel` VARCHAR(45) · `fecha_creacion` VARCHAR(45) · `fecha_cierre` VARCHAR(45) **NULL** · `numero_cohortes` VARCHAR(45) · `cant_graduados` VARCHAR(45) · `fecha_actualizacion` VARCHAR(45) · `ciudad` VARCHAR(45) · `facultad` INT · `activo` BIT DEFAULT 1 |
| `red` | `idr` INT (**sin** IDENTITY) | `nombre` VARCHAR(45) · `url` VARCHAR(**200**) · `pais` VARCHAR(45) · `activo` BIT DEFAULT 1 |

### Aclaraciones

- `programa.facultad` es un **entero sin clave foránea** en la v1 (la tabla no tiene dependencias). La API solo exige que sea mayor que 0. Verificación en la tarea `T-005`: confirmar en el script que `programa` no declara ninguna FK; si la declarara, habría que quitarla para la v1 porque las tablas relacionadas están vacías.
- `programa.id` y `red.idr` los **envía el cliente**.

---

## 2. Ajustes al script de la Fase 0 (tarea `T-005`)

El script original solo necesita un ajuste para la v1 (todas las columnas `activo` de estas 5 tablas ya son `NOT NULL`, lo que permite mapearlas a `bool` sin ambigüedad en el filtro `activo = 1`):

```sql
-- red: url más larga
url VARCHAR(200) NOT NULL
```

Referencia de las 5 tablas (T-SQL) tal como deben quedar:

```sql
CREATE TABLE area_conocimiento (
    id         VARCHAR(6)   NOT NULL PRIMARY KEY,
    gran_area  VARCHAR(60)  NOT NULL,
    area       VARCHAR(60)  NOT NULL,
    disciplina VARCHAR(150) NOT NULL,
    activo     BIT          NOT NULL DEFAULT 1
);

CREATE TABLE termino_clave (
    termino        VARCHAR(30) NOT NULL PRIMARY KEY,
    termino_ingles VARCHAR(30) NULL,
    activo         BIT         NOT NULL DEFAULT 1
);

CREATE TABLE linea_investigacion (
    id          INT IDENTITY(1,1) PRIMARY KEY,
    nombre      VARCHAR(45)  NOT NULL,
    descripcion VARCHAR(256) NOT NULL,
    activo      BIT          NOT NULL DEFAULT 1
);

CREATE TABLE programa (
    id                  INT         NOT NULL PRIMARY KEY,
    nombre              VARCHAR(60) NOT NULL,
    tipo                VARCHAR(45) NOT NULL,
    nivel               VARCHAR(45) NOT NULL,
    fecha_creacion      VARCHAR(45) NOT NULL,
    fecha_cierre        VARCHAR(45) NULL,
    numero_cohortes     VARCHAR(45) NOT NULL,
    cant_graduados      VARCHAR(45) NOT NULL,
    fecha_actualizacion VARCHAR(45) NOT NULL,
    ciudad              VARCHAR(45) NOT NULL,
    facultad            INT         NOT NULL,
    activo              BIT         NOT NULL DEFAULT 1
);

CREATE TABLE red (
    idr    INT          NOT NULL PRIMARY KEY,
    nombre VARCHAR(45)  NOT NULL,
    url    VARCHAR(200) NOT NULL,
    pais   VARCHAR(45)  NOT NULL,
    activo BIT          NOT NULL DEFAULT 1
);
```

(El script completo conserva las otras 14 tablas, incluidas `rol` y `usuario` del módulo de login, y los 218 `INSERT` de `area_conocimiento`; esas filas no se reproducen aquí.)

---

## 3. Reglas de validación por campo (API y front)

Se aplican con anotaciones en `Peticiones/` (API) y en `Modelos/*Formulario` (front). Los textos obligatorios rechazan vacío y solo espacios. Las claves `int` no anulables se validan con `[Range(1, int.MaxValue)]` (así un campo ausente, que vale 0, produce `422`).

| Entidad | Campo | Regla |
|---|---|---|
| `termino_clave` | `termino` | obligatorio, ≤ 30 |
| | `termino_ingles` | opcional, ≤ 30 |
| `linea_investigacion` | `nombre` | obligatorio, ≤ 45 |
| | `descripcion` | obligatorio, ≤ 256 |
| `programa` | `id` | obligatorio, ≥ 1 |
| | `nombre` | obligatorio, ≤ 60 |
| | `tipo`, `nivel`, `ciudad` | obligatorios, ≤ 45 |
| | `fecha_creacion` | obligatorio, regex `^\d{4}-\d{2}-\d{2}$` **y** fecha válida |
| | `fecha_cierre` | opcional, mismo formato; si viene no puede ser anterior a `fecha_creacion` |
| | `numero_cohortes`, `cant_graduados` | obligatorios, regex `^\d{1,45}$` |
| | `facultad` | obligatorio, ≥ 1 |
| | `fecha_actualizacion` | **no se envía**: la fija el servicio (`DateTime.Now.ToString("yyyy-MM-dd")`) |
| `programa` (PATCH) | cualquier campo de arriba salvo `id` | opcional; si viene, cumple la misma regla (un texto vacío da `422`); al menos un campo |
| `red` | `idr` | obligatorio, ≥ 1 |
| | `nombre`, `pais` | obligatorios, ≤ 45 |
| | `url` | obligatorio, ≤ 200, regex `^https?://\S+$` |

Validación de fecha: además de la regex, el servicio verifica con `DateTime.TryParseExact(valor, "yyyy-MM-dd", ...)` (descarta `2024-13-45`).

---

## 4. Mapeo a clases C# (`Modelos/`)

Cada columna `snake_case` es una propiedad `PascalCase` (`fecha_creacion` → `FechaCreacion`, `idr` → `Idr`, `cant_graduados` → `CantGraduados`). Tipos: `VARCHAR` → `string`, `INT` → `int`, `BIT` → `bool`, columnas `NULL` → `string?`.

| Tabla | Clase | Clave C# |
|---|---|---|
| `area_conocimiento` | `AreaConocimiento` | `Id` (string) |
| `termino_clave` | `TerminoClave` | `Termino` (string) |
| `linea_investigacion` | `LineaInvestigacion` | `Id` (int) |
| `programa` | `Programa` | `Id` (int) |
| `red` | `Red` | `Idr` (int) |

Las reglas de configuración de EF Core (claves no generadas, `IDENTITY`, `Activo`, filtro global, longitudes) están en `3_plan.md`, sección 3.

---

## 5. Borrado lógico (columna `activo`)

La base **no tiene triggers ni procedimientos almacenados**: toda la lógica vive en la API.

- `DELETE` en la API = `UPDATE ... SET activo = 0`; la fila no se elimina.
- Todo `GET` filtra `activo = 1` mediante `HasQueryFilter` (no hay modo administrativo).
- Los chequeos de duplicados (`id`, `termino`, `idr`) **incluyen inactivos** con `IgnoreQueryFilters()` y responden `409`. En la v1 no hay reactivación.

---

## 6. Datos iniciales

| Tabla | Filas |
|---|---|
| `area_conocimiento` | **218** (códigos como `1A01`, `1B01`, `6E03`) |
| `programa` | **0** (el Excel de origen tiene 191 filas pero no trae seis columnas obligatorias; rellenarlas sería inventar datos) |
| `termino_clave`, `linea_investigacion`, `red` | **0** |

---

## 7. Particularidades de SQL Server

| Aspecto | Detalle |
|---|---|
| Autoincremento | `IDENTITY(1,1)` solo en `linea_investigacion`. `programa` y `red` **no**: el cliente envía el id. |
| Trigger/SP | No hay. |
| Re-ejecutable | El script inicia con `DROP TABLE` en orden inverso a las claves foráneas; `init.sh` verifica `sys.databases` antes de crear la BD. |
| Ejecución | `sqlserver-init` con `sqlcmd -i`. |

**Regla de oro:** cualquier cambio de esquema se hace en `gestion_profesoral.sql` **y** en `GestionDbContext`, y se aplica con `docker compose down -v` + `up -d`.
