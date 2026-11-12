# Prompts de Nicolás — Versión 2

> Integrante: **Nicolás** · Rama: `rama-nicolas` · Herramienta: Claude (chat con herramientas de código), noviembre de 2026.
> Aquí están los prompts **tal como los escribí**, lo que hizo la IA con cada uno y lo que hubo que corregir.

---

## Prompt 1 — Análisis del proyecto

> claude tengo este proyecto lo puedes analizar al completo ?
> https://github.com/juanjosedh28-alt/proyecto_gestion_profesoral

**Qué hizo la IA:** clonó el repositorio y lo revisó completo. Encontró que **no compilaba**: 12 archivos tenían
marcas de conflicto de un merge sin resolver (`<<<<<<< HEAD`), la interfaz `IRepositorioPrograma` estaba dos
veces, el script SQL con los procedimientos (`db/db/`) nunca se ejecutaba y `sp_crear_docente` usaba una columna
que no existía. También señaló archivos vacíos (README), la carpeta `.vs` subida y la contraseña escrita en el compose.

## Prompt 2 — Lo que pide el profesor

> de acuerdo al documento metodología del curso y proyecto de aula: [0_METODOLOGIA.md] lo que se espera de la
> versión 2: [VERSION_2.md] y el proyecto ejemplo es: [proyecto_aplicacion_y_servicios_web4]


**Qué hizo la IA:** leyó los dos documentos y el proyecto de ejemplo. Me explicó la v2 con la comparación del
restaurante (base = bodega, API = cocina, front = mesero), qué es una clave foránea, un procedimiento almacenado,
un disparador, un desplegable y el maestro-detalle. Hizo una tabla de lo que pide el profesor contra lo que tenía el
proyecto

## Prompt 3 — Arrancar

> Adaptalo para usar razor

**Qué hizo la IA:** creó la rama `rama-nicolas` y construyó la v2 por fases.

1. Resolver los conflictos de la v1.
2. `db/v2_procedimientos.sql` (79 procedimientos, 11 vistas, 2 disparadores) e `init.sh`.
3. Sacar la contraseña a `.env`.
4. El ejecutor de procedimientos y el traductor de errores SQL.
5. Pasar la v1 a procedimientos.
6. La API de las 11 tablas.
7. Los modelos y clientes del front.
8. Las pantallas (docentes con maestro-detalle, edición, detalle del estudio).
9. La colección de Postman.
10. El spec kit alineado con el código.

---

## Lo que hubo que corregir

| # | Qué pasó | Cómo se corrigió | Quién lo detectó |
|---|---|---|---|
| 1 | Los conflictos del merge tenían dos versiones muy distintas de `programa` (una con `Codigo`, que no existe en la tabla) | Se tomó la versión que coincide con la tabla real y con el front Razor | Nicolas al leer el código |
| 2 | El spec decía que la API usaba EF Core y `DbContext`; el código real usa Dapper | Se escribió D57 y se enmendó la constitución **antes** de escribir la API | La IA, al comparar spec y código |
| 3 | El spec pedía envío múltiple en 8 tablas | Se dejó en las 4 del detalle directo del docente (D61): en las puentes se asigna una pareja a la vez | Decisión tomada al diseñar la pantalla |
| 4 | El spec dejaba la v1 sin procedimientos, pero el criterio 3 del profesor revisa **todas** las tablas | La v1 también pasó a procedimientos (R9, D58) | Nicolas, al leer `VERSION_2.md` |
| 5 | `init.sh` borraba y creaba las tablas en cada arranque | Ahora solo la primera vez | Juan Jose |
| 6 | `red.url` era de 45 caracteres en la base y la API dejaba 200 → error 500 | Validación ajustada a 45 | La IA |
| 7 | Los comentarios de los repositorios tenían la palabra «UPDATE» y el `grep` del criterio 3 los marcaba | Se reescribieron sin palabras SQL | La IA, corriendo el `grep` |
| 8 | Los mensajes generados decían «Los nombres es obligatorios» | Concordancia corregida («Los nombres son obligatorios») | Juan Jose, al revisar lo generado |
| 9 | En las pantallas de edición, la validación pedía un id que el formulario no enviaba | Campos ocultos para las claves | Juan Jose, al revisar lo generado |
| 10 | GitHub no dejó subir la rama: el repositorio es de otro integrante | Hay que instalar la app de Claude en el repositorio, o subir la rama desde mi computador | Nicolas intentar el push |

Probar todo con Docker (`7_quickstart.md`) y anotar aquí lo que falle y cómo se arregló.

| # | Qué falló al probar | Cómo se arregló |
## Lo que pasó al probar con Docker (6 y 7 de noviembre)

| # | Qué falló al probar | Cómo se arregló |
|---|---|---|
| 1 | `docker compose up` falló: «The container name "/gestion-sqlserver" is already in use». Había un contenedor viejo de otra prueba con el mismo nombre | Se borraron los contenedores viejos con `docker rm -f gestion-sqlserver gestion-sqlserver-init gestion-api front-razor` y `docker compose down -v`, y se volvió a arrancar |
| 2 | `sqlserver-init` terminó con «exit 2». El log decía `$'\r': command not found`: Git en Windows cambió los finales de línea de `init.sh` a CRLF y Linux, dentro de Docker, no los entiende | Arreglo inmediato: `sed -i 's/\r$//' db/init.sh`. Arreglo definitivo: archivo `.gitattributes` con `*.sh text eol=lf` (commit «Fuerza finales de linea LF…»), para que no le pase a nadie del equipo |
| 3 | Postman no dejaba importar la colección: estaba en el modo liviano (Lightweight API Client) y yo había escrito la ruta en el cuadro de cURL | Se creó una cuenta gratis y se importó el archivo con «Select files» |
| 4 | La primera corrida de la colección dio «No response»: Docker estaba apagado | Se encendió Docker Desktop y `docker compose up -d` |

## Resultado de las pruebas

- `sqlserver-init` en **Exited (0)**: se crearon los 79 procedimientos, las 11 vistas y los 2 disparadores sin errores.
- Colección de Postman: **91 de 91 pruebas en verde**, 0 fallidas. Incluye las peticiones que deben fallar (404, 409, 422), y ninguna respondió 500.
- El disparador se comprobó: `total_estudios` subió a 3 al agregar tres estudios en un envío y bajó a 2 al retirar uno, sin que la API enviara el número.
