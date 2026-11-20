# Mapa de Versiones — Gestión Profesoral

| Versión | Alcance | Carpeta | Estado |
| :--- | :--- | :--- | :--- |
| **v1** | CRUD de las **5 tablas sin clave foránea**: `area_conocimiento` (solo lectura), `termino_clave`, `linea_investigacion`, `programa`, `red` + spec kit | `v1_programa/` | Cerrada (tag `v1`) |
| **v2** | CRUD de las **11 tablas con clave foránea**: `docente`, `estudios_realizados`, `evaluacion_docente`, `reconocimiento`, `experiecia`, `apoyo_profesoral`, `beca`, `docente_departamento`, `red_docente`, `intereses_futuros`, `estudio_ac`. Todo el CRUD (también el de la v1) por procedimientos almacenados, dos disparadores, desplegables y maestro-detalle | `v2_gestion_profesoral/` | Cerrada (PR #1 en `main`) |
| **v3** | Control de acceso: contraseña con bcrypt, inicio de sesión con JWT, permiso por rol en **cada** petición (`sp_verificar_acceso_ruta`), menú según el rol y CRUD de `usuario`, `rol`, `rol_usuario` y de los permisos (`ruta`, `ruta_rol`) | `v3_control_acceso/` | En entrega (sustentación: sábado 21 de noviembre de 2026) |
| **v4** | 10 consultas multitabla, dashboard, imagen corporativa, publicación (no se sustenta: se envía por correo la URL del front publicado, los usuarios y contraseñas por rol y las URL de los repositorios) | — | Pendiente |

**Reglas:** la spec de una versión se escribe antes de su código; una versión cerrada no se reabre (sus ajustes van
en la siguiente); al cerrar una versión se corre la regresión de todas las anteriores.

Los prompts usados para generar cada versión están en la carpeta de esa versión (`GUIA_IA<N>.md` y `prompts_<integrante>.md`).
