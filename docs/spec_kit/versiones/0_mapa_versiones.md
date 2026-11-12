# Mapa de Versiones — Gestión Profesoral

| Versión | Alcance | Carpeta | Estado |
| :--- | :--- | :--- | :--- |
| **v1** | CRUD de las **5 tablas sin clave foránea**: `area_conocimiento` (solo lectura), `termino_clave`, `linea_investigacion`, `programa`, `red` + spec kit | `v1_programa/` | Cerrada (tag `v1`) |
| **v2** | CRUD de las **11 tablas con clave foránea**: `docente`, `estudios_realizados`, `evaluacion_docente`, `reconocimiento`, `experiecia`, `apoyo_profesoral`, `beca`, `docente_departamento`, `red_docente`, `intereses_futuros`, `estudio_ac`. Todo el CRUD (también el de la v1) por procedimientos almacenados, dos disparadores, desplegables y maestro-detalle | `v2_gestion_profesoral/` | En entrega (tag `v2` al pasar los criterios) |
| **v3** | Inicio de sesión con JWT, roles y CRUD de `usuario`, `rol`, `rol_usuario` | — | Pendiente |
| **v4** | 10 consultas multitabla, dashboard, imagen corporativa, publicación | — | Pendiente |

**Reglas:** la spec de una versión se escribe antes de su código; una versión cerrada no se reabre (sus ajustes van
en la siguiente); al cerrar una versión se corre la regresión de todas las anteriores.

Los prompts usados para generar cada versión están en la carpeta de esa versión (`GUIA_IA<N>.md` y `prompts_<integrante>.md`).
