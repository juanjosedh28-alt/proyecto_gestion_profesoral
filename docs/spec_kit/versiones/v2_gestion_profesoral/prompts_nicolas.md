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
> eso es lo que nos pide el profesor como lo puedo aplicar a esto, no se nada de programacion explicame como si
> fura la primera vez programando o como puedo dejarlo para que lo hagas tu ayudame de todas las formas que puedas porfavor ayuda

**Qué hizo la IA:** leyó los dos documentos y el proyecto de ejemplo. Me explicó la v2 con la comparación del
restaurante (base = bodega, API = cocina, front = mesero), qué es una clave foránea, un procedimiento almacenado,
un disparador, un desplegable y el maestro-detalle. Hizo una tabla de lo que pide el profesor contra lo que tenía el
proyecto, y me advirtió que **la interpretabilidad (4 de 20 puntos) no la puede hacer la IA por mí**.

## Prompt 3 — Arrancar

> Si soy nicolas, si usemos razor, si tengo permiso manda la solicitud, recuerda que debo hacer commits con cada cambio que se realice

**Qué hizo la IA:** creó la rama `rama-nicolas` y construyó la v2 por fases, con **un commit por cambio**:

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
| 1 | Los conflictos del merge tenían dos versiones muy distintas de `programa` (una con `Codigo`, que no existe en la tabla) | Se tomó la versión que coincide con la tabla real y con el front Razor | La IA, al leer el código |
| 2 | El spec decía que la API usaba EF Core y `DbContext`; el código real usa Dapper | Se escribió D57 y se enmendó la constitución **antes** de escribir la API | La IA, al comparar spec y código |
| 3 | El spec pedía envío múltiple en 8 tablas | Se dejó en las 4 del detalle directo del docente (D61): en las puentes se asigna una pareja a la vez | Decisión tomada al diseñar la pantalla |
| 4 | El spec dejaba la v1 sin procedimientos, pero el criterio 3 del profesor revisa **todas** las tablas | La v1 también pasó a procedimientos (R9, D58) | La IA, al leer `VERSION_2.md` |
| 5 | `init.sh` borraba y creaba las tablas en cada arranque | Ahora solo la primera vez | La IA |
| 6 | `red.url` era de 45 caracteres en la base y la API dejaba 200 → error 500 | Validación ajustada a 45 | La IA |
| 7 | Los comentarios de los repositorios tenían la palabra «UPDATE» y el `grep` del criterio 3 los marcaba | Se reescribieron sin palabras SQL | La IA, corriendo el `grep` |
| 8 | Los mensajes generados decían «Los nombres es obligatorios» | Concordancia corregida («Los nombres son obligatorios») | La IA, al revisar lo generado |
| 9 | En las pantallas de edición, la validación pedía un id que el formulario no enviaba | Campos ocultos para las claves | La IA, al revisar lo generado |
| 10 | GitHub no dejó subir la rama: el repositorio es de otro integrante | Hay que instalar la app de Claude en el repositorio, o subir la rama desde mi computador | Al intentar el push |

**Pendiente de mi parte:** probar todo con Docker (`7_quickstart.md`) y anotar aquí lo que falle y cómo se arregló.

| # | Qué falló al probar | Cómo se arregló |
|---|---|---|
| | | |
