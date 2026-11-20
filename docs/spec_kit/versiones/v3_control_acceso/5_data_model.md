# Modelo de datos — Gestión Profesoral v3 (`5_data_model.md`)

> **Documento 5 de 8 del spec kit de la v3.** Lo que cambia en la base. Script: `db/v3_control_acceso.sql`.

## 1. Las cinco tablas del acceso

```
usuario ──< rol_usuario >── rol ──< ruta_rol >── ruta
 (ya existía)  (ya existía)  (ya existía)  (NUEVA)    (NUEVA)

PERSONA  ──►  ROL  ──►  PERMISO      (RBAC: el permiso se le da al rol, no a la persona)
```

| Tabla | Columnas | Notas |
|---|---|---|
| `usuario` | `id`, `username` (único), `password`, `email` (único), `nombre_completo`, `activo`, `fecha_creacion`, `fecha_actualizacion` | Ya existía. **`password` guarda solo el hash bcrypt** (`$2a$12$…`, 60 caracteres) |
| `rol` | `id`, `nombre` (único), `descripcion`, `activo` | Ya existía |
| `rol_usuario` | `usuario_id`, `rol_id` (PK compuesta, FK con `ON DELETE CASCADE`) | Ya existía |
| `ruta` | `id` IDENTITY, `nombre` VARCHAR(60) único, `descripcion` VARCHAR(200) | **Nueva.** Catálogo de lo protegible |
| `ruta_rol` | `ruta_id`, `rol_id` (PK compuesta, FK con `ON DELETE CASCADE`) | **Nueva.** Qué rol entra a qué ruta |

## 2. Rutas sembradas

| Ruta | Qué permite | Administrador | Editor | Consulta |
|---|---|:-:|:-:|:-:|
| `interfaz.inicio` | Página de inicio | ✔ | ✔ | ✔ |
| `interfaz.areas` | Áreas de conocimiento | ✔ | ✔ | ✔ |
| `interfaz.terminos` | Términos clave | ✔ | ✔ | ✔ |
| `interfaz.lineas` | Líneas de investigación | ✔ | ✔ | ✔ |
| `interfaz.programas` | Programas | ✔ | ✔ | ✔ |
| `interfaz.redes` | Redes | ✔ | ✔ | ✔ |
| `interfaz.docentes` | Docentes y todo su detalle | ✔ | ✔ | ✔ |
| `interfaz.usuarios` | Gestión de usuarios y de sus roles | ✔ | | |
| `interfaz.roles` | Gestión de roles | ✔ | | |
| `interfaz.permisos` | Permisos de cada rol | ✔ | | |
| `datos.modificar` | Crear, editar y retirar registros del módulo | ✔ | ✔ | |

`ruta_rol` se siembra **solo si está vacía**: si se sembrara en cada arranque, un permiso que el administrador quitó
volvería a aparecer al reiniciar. Una ruta nueva se le da al Administrador.

## 3. Usuarios de prueba

| Usuario | Correo | Contraseña | Rol |
|---|---|---|---|
| `admin` | `admin@gestion.edu.co` | `admin123` | Administrador |
| `editor` | `editor@gestion.edu.co` | `editor123` | Editor |
| `consulta` | `consulta@gestion.edu.co` | `consulta123` | Consulta |

Lo que el script inserta en la columna `password` son los **hashes** bcrypt (costo 12), generados una vez; las contraseñas solo aparecen en un comentario, para la sustentación. Se siembran
solo si el correo o el usuario no existen. Estas contraseñas son de prueba y se documentan para la sustentación y
para el correo de la v4; en un sistema real se cambian al primer uso.

## 4. Procedimientos del acceso

| Procedimiento | Recibe | Devuelve | Lo usa |
|---|---|---|---|
| `sp_verificar_acceso_ruta` | `@email`, `@ruta` | `tiene_acceso` (BIT) | `[ExigePermiso]` en **cada** petición |
| `sp_rutas_permitidas` | `@email` | `ruta` (una fila por ruta, sin repetir) | `GET /api/permisos/mios` (menú) |
| `sp_credenciales_usuario` | `@email` | `id`, `email`, `nombre_completo`, `hash` | Solo el inicio de sesión. **El único que devuelve el hash** |
| `sp_roles_de_usuario` | `@email` | `rol` | El token (para mostrar los roles) y `permisos/mios` |

Los tres primeros solo cuentan usuarios **activos** y roles **activos**: retirar un usuario o un rol le quita el acceso
en la siguiente petición.

`vw_usuario` muestra los usuarios **sin** la columna `password` y con sus roles en un texto (`STRING_AGG`).
