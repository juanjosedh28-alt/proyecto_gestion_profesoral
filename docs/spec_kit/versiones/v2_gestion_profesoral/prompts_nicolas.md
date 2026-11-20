# Prompts de Nicolás — Versión 2

> Integrante: **Nicolás** · Ramas: `rama-nicolas` y `rama-nicolas-v2-correcciones` · Herramienta: Claude (chat con
> herramientas de código) · Octubre y noviembre de 2026.
>
> Los prompts van **textuales**, como se escribieron. Debajo de cada uno: qué produjo, qué revisé y qué hubo que
> corregir. Solo están los mensajes que llevaron del spec kit al código; las preguntas de consulta sobre cómo
> usar Git, Docker o Postman no se incluyen.

---

## Cómo trabajé con la IA

1. **Primero el diagnóstico, después el código.** Antes de pedir nada, le pedí revisar el repositorio completo
   y compararlo con `0_METODOLOGIA.md` y `VERSION_2.md`, para saber qué faltaba y qué estaba roto.
2. **Decisiones mías, ejecución de la IA.** El front (Razor Pages y no Blazor), trabajar en mi rama y no en
   `main` y un commit por cambio fueron condiciones que puse desde el principio.
3. **Todo se prueba fuera del chat.** La IA no podía compilar ni levantar el proyecto, así que cada entrega la
   levanté yo con Docker, corrí la colección de Postman y le devolví los errores con el log.
4. **Lo que no cuadra con la spec se corrige en la spec.** Cuando el código y el spec kit decían cosas distintas,
   se actualizó el documento (D57 a D67), no solo el código.

---

## Prompt 1 — Diagnóstico del repositorio

> claude tengo este proyecto lo puedes analizar al completo ?
> https://github.com/juanjosedh28-alt/proyecto_gestion_profesoral

**Resultado:** el repositorio **no compilaba**. Había 12 archivos con marcas de conflicto de un merge sin resolver
(`<<<<<<< HEAD`), la interfaz `IRepositorioPrograma` estaba dos veces, el script de procedimientos (`db/db/`) nunca
se ejecutaba y `sp_crear_docente` usaba una columna que no existe. También estaban la carpeta `.vs` subida y la
contraseña de la base escrita en el `docker-compose.yml`.

## Prompt 2 — Requisitos de la v2 contra el estado del proyecto

> de acuerdo al documento metodología del curso y proyecto de aula: [0_METODOLOGIA.md] lo que se espera de la
> versión 2: [VERSION_2.md] y el proyecto ejemplo es: [proyecto_aplicacion_y_servicios_web4]

**Resultado:** una tabla con cada exigencia de `VERSION_2.md` (procedimientos, disparador, desplegables,
maestro-detalle, 409 en vez de 500, regresión) y su estado en el repositorio. Con esa tabla se armó el plan por fases.

## Prompt 3 — Construcción de la v2

> Adaptalo para usar razor

Con las condiciones que puse: rama propia (`rama-nicolas`), un commit por cambio y el front en Razor Pages.

**Resultado, por fases y con un commit por paso:**

1. Resolver los conflictos de la v1.
2. `db/v2_procedimientos.sql` (79 procedimientos, 11 vistas, 2 disparadores) e `init.sh`.
3. Sacar la contraseña a `.env` y crear `.env.example`.
4. El ejecutor de procedimientos (Dapper) y el traductor de errores SQL a 409/422.
5. Pasar la v1 a procedimientos.
6. La API de las 11 tablas.
7. Los modelos y clientes del front.
8. Las pantallas: docentes con maestro-detalle, edición del detalle y detalle del estudio.
9. La colección de Postman.
10. El spec kit alineado con el código.

---

## Lo que hubo que corregir

| # | Qué pasó | Cómo se corrigió | Quién lo detectó |
|---|---|---|---|
| 1 | Los conflictos del merge tenían dos versiones muy distintas de `programa` (una con `Codigo`, que no existe en la tabla) | Se tomó la versión que coincide con la tabla real y con el front Razor | Nicolás, al leer el código |
| 2 | El spec decía que la API usaba EF Core y `DbContext`; el código real usa Dapper | Se escribió D57 y se enmendó la constitución **antes** de escribir la API | La IA, al comparar spec y código |
| 3 | El spec pedía envío múltiple en 8 tablas | Se dejó en las 4 del detalle directo del docente (D61): en las puentes se asigna una pareja a la vez | Decisión al diseñar la pantalla |
| 4 | El spec dejaba la v1 sin procedimientos, pero el criterio 3 del profesor revisa **todas** las tablas | La v1 también pasó a procedimientos (R9, D58) | Nicolás, al leer `VERSION_2.md` |
| 5 | `init.sh` borraba y creaba las tablas en cada arranque | Ahora solo la primera vez | Juan José |
| 6 | `red.url` era de 45 caracteres en la base y la API dejaba 200 → error 500 | Validación ajustada a 45 (D64) | La IA |
| 7 | Los comentarios de los repositorios tenían la palabra «UPDATE» y el `grep` del criterio 3 los marcaba | Se reescribieron sin palabras SQL | La IA, corriendo el `grep` |
| 8 | Los mensajes generados decían «Los nombres es obligatorios» | Concordancia corregida | Juan José, al revisar lo generado |
| 9 | En las pantallas de edición, la validación pedía un id que el formulario no enviaba | Campos ocultos para las claves | Juan José, al revisar lo generado |
| 10 | La IA no podía subir la rama: el repositorio es de otro integrante | La IA entregó los commits en un paquete (`git bundle`) y yo los subí desde mi computador | Nicolás, al hacer el push |

## Pruebas con Docker (6 y 7 de noviembre)

| # | Qué falló | Diagnóstico | Arreglo |
|---|---|---|---|
| 1 | `docker compose up` no arrancaba | «The container name "/gestion-sqlserver" is already in use»: quedaba un contenedor de una prueba anterior | `docker rm -f` de los contenedores viejos y `docker compose down -v` |
| 2 | `sqlserver-init` terminaba con «exit 2» | El log decía `$'\r': command not found`: Git en Windows guardó `init.sh` con finales de línea CRLF y el Linux del contenedor no los entiende | Inmediato: `sed -i 's/\r$//' db/init.sh`. Definitivo: commit propio con `.gitattributes` (`*.sh text eol=lf`) para que no le pase a nadie del equipo |

**Resultado:** `sqlserver-init` en **Exited (0)** (79 procedimientos, 11 vistas y 2 disparadores sin errores) y la
colección de Postman con **91 de 91 pruebas en verde**, incluidas las que deben fallar (404, 409, 422) y ninguna
con 500. El disparador se comprobó: `total_estudios` subió a 3 al agregar tres estudios en un envío y bajó a 2 al
retirar uno, sin que la API enviara el número.

---

## Correcciones del profesor (17 de noviembre)

El profesor revisó la v2 y pidió ver **todas las tablas en la ventana principal**, además de cumplir todo
`VERSION_2.md`. Juan José subió a `main` una pantalla por cada tabla de detalle.

### Prompt 4 — Revisión de lo que subió Juan José

> ya mi compañero hizo una actualizacion en el codigo puedes revisar ?

| # | Problema | Cómo se arregló |
|---|---|---|
| 1 | 18 páginas llamaban a `ErroresFormulario.Aplicar(...)`, que no existía: el front no compilaba | Se agregó el método en `Infraestructura/ErroresFormulario.cs` |
| 2 | `EvaluacionesDocente/Index.cshtml` pedía un `IndexModel` que no estaba | Se creó `EvaluacionesDocente/Index.cshtml.cs` |
| 3 | Se habían borrado los comentarios de `ClientesDetalleDocente.cs` y `ClientesPuentes.cs` (la interpretabilidad se califica) | Se restauraron, conservando los métodos nuevos `ListarAsync` |
| 4 | Un error al eliminar salía en verde, como si fuera éxito | Se manda por `TempData["Error"]`, que el layout muestra en rojo |
| 5 | Abrir «Editar» de un registro que no existe daba error 500 | Vuelve al listado con el mensaje |
| 6 | La página de inicio decía «FK», «PK» y «CRUD» (criterio 11: la interfaz no habla en jerga) | Se cambiaron por palabras comunes |
| 7 | El menú solo tenía 6 entradas | Ahora tiene las 16 tablas en tres grupos: catálogos, información del docente y relaciones |
| 8 | Los desplegables decían «Sí (1)» y «No (0)»; la fecha fin de Red-Docente no tenía calendario | «Sí»/«No» y campo de fecha |

**Verificación mía:** con Docker, el log del front mostró «Build succeeded» y «Application started».

### Prompt 5 — Auditoría completa contra los documentos del profesor

> necesito que leas muy bien nuevamente lo que esta pidiendo el profesor en la v2 con todo al detalle pofavor para que todo este muy bien

| # | Problema | Cómo se arregló |
|---|---|---|
| 9 | `area_conocimiento` era de solo lectura (D22 de la v1), pero el criterio 1 pide crear, editar y retirar en las tablas de la v1 | CRUD completo: 3 procedimientos, API, pantallas y pruebas (D65) |
| 10 | Un desplegable sin elegir mostraba un mensaje en inglés | Mensajes de validación en castellano (D67) |
| 11 | Al editar en las pantallas nuevas se veía la cédula o el número del estudio, no el nombre (criterio 5) | Se muestra el nombre |
| 12 | Si un desplegable estaba vacío no se decía nada (RF14.6) | Aviso en castellano con enlace a donde crear el primero |
| 13 | Las pantallas nuevas no tenían comentarios | Comentario de qué hacen y por qué en cada una |
| 14 | La colección no probaba los listados de cada tabla | Carpetas 7 (área de conocimiento) y 8 (listados) |
| 15 | El spec kit decía «un solo enlace: Docentes»: ya no coincidía con el código | Se actualizaron `2_spec`, `3_plan`, `4_research`, `6_contracts`, `7_quickstart`, `8_tasks` y `9_checklist` |

---

## Lo que aprendí y haría distinto

- **Un merge sin revisar rompe a todo el equipo.** El repositorio llegó dos veces a `main` sin compilar (conflictos
  en octubre, métodos inexistentes en noviembre). Antes de aprobar un PR hay que levantarlo con Docker.
- **La IA comenta lo que cree que hizo.** Los comentarios con la palabra «UPDATE» y los mensajes con errores de
  concordancia salieron de la IA y los encontramos al revisar.
- **La spec tiene que seguir al código.** Cuando cambió el alcance (pantallas por tabla, CRUD del área), lo primero
  fue actualizar la spec, porque el profesor compara las dos.
