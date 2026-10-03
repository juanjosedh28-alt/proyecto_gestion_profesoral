# Modelo de datos — gestion_local (v1)

Documento 5 de 8 del spec kit raíz. La base de datos existe en un solo dialecto: SQL Server (`db/sqlserver/gestion_profesoral.sql` + `db/sqlserver/init.sh`). Este documento define el contenido común de las 7 tablas independientes que implementa la v1 y sus particularidades.

---

## 1. Las 7 tablas de la v1

Son las tablas sin claves foráneas (entidades raíz).

| Tabla | Columnas (todas NOT NULL salvo indicado) |
|---|---|
| `area_conocimiento` | `id` VARCHAR(6) PK · `gran_area` VARCHAR(60) · `area` VARCHAR(60) · `disciplina` VARCHAR(150) · `activo` BIT DEFAULT 1 |
| `termino_clave` | `termino` VARCHAR(30) PK · `termino_ingles` VARCHAR(30) (NULL) · `activo` BIT DEFAULT 1 |
| `linea_investigacion` | `id` INT IDENTITY PK · `nombre` VARCHAR(45) · `descripcion` VARCHAR(256) · `activo` BIT DEFAULT 1 |
| `programa` | `id` INT PK · `nombre` VARCHAR(60) · `tipo` VARCHAR(45) · `nivel` VARCHAR(45) · `fecha_creacion` VARCHAR(45) · `fecha_cierre` VARCHAR(45) (NULL) · `numero_cohortes` VARCHAR(45) · `cant_graduados` VARCHAR(45) · `fecha_actualizacion` VARCHAR(45) · `ciudad` VARCHAR(45) · `facultad` INT · `activo` BIT DEFAULT 1 |
| `red` | `idr` INT PK · `nombre` VARCHAR(45) · `url` VARCHAR(45) · `pais` VARCHAR(45) · `activo` BIT DEFAULT 1 |
| `rol` | `id` INT IDENTITY PK · `nombre` VARCHAR(100) UNIQUE · `descripcion` VARCHAR(MAX) (NULL) · `activo` BIT DEFAULT 1 (NULL permitido) · `fecha_creacion` DATETIME DEFAULT GETDATE() |
| `usuario` | `id` INT IDENTITY PK · `username` VARCHAR(100) UNIQUE · `password` VARCHAR(255) · `email` VARCHAR(150) UNIQUE · `nombre_completo` VARCHAR(200) (NULL) · `activo` BIT DEFAULT 1 (NULL permitido) · `fecha_creacion` DATETIME DEFAULT GETDATE() · `fecha_actualizacion` DATETIME DEFAULT GETDATE() |

---

## 2. Borrado lógico (columna `activo`)

La base no tiene triggers ni procedimientos almacenados: toda la lógica vive en la API.

- `area_conocimiento`, `termino_clave`, `linea_investigacion`, `programa` y `red` tienen `activo BIT NOT NULL DEFAULT 1`.
- `rol` y `usuario` tienen `activo BIT DEFAULT 1` (admite NULL).
- `DELETE` en la API ejecuta `UPDATE ... SET activo = 0`; la fila no se elimina.
- Todo `GET` filtra por `activo = 1`.

---

## 3. Datos de ejemplo

- `area_conocimiento`: **218 filas** (códigos alfanuméricos como `1A01`, `1B01`, `6E03`).
- `programa`: **0 filas**. El Excel de origen tiene 191 filas, pero no trae seis columnas obligatorias (`nivel`, `fecha_creacion`, `numero_cohortes`, `cant_graduados`, `fecha_actualizacion` y `ciudad`); rellenarlas sería inventar datos. La v1 arranca con la tabla vacía y su verificación empieza por el `204` del listado vacío.
- `termino_clave`, `linea_investigacion`, `red`, `rol` y `usuario`: **0 filas**.

---

## 4. Particularidades del dialecto

| Aspecto | SQL Server (`db/sqlserver/`) |
|---|---|
| Autoincremento | `IDENTITY(1,1)` en `linea_investigacion`, `rol` y `usuario`. `programa` y `red` NO son IDENTITY: el `id` lo envía el cliente. |
| Trigger/SP | No hay. |
| Re-ejecutable | El script inicia con `DROP TABLE` en orden inverso a las claves foráneas; `init.sh` verifica `sys.databases` antes de crear la BD. |
| Ejecución | `sqlserver-init` con `sqlcmd -i` (la imagen de SQL Server no tiene `/docker-entrypoint-initdb.d`). |

Regla de oro: cualquier cambio de esquema se hace en `gestion_profesoral.sql` y se aplica con `docker compose down -v` + `up -d`.