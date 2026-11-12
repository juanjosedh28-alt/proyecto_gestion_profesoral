# Guía de IA — Versión 2

> Cómo se pasó del spec kit al código de la v2 usando una IA, y cómo **volver a hacerlo**.
> Los prompts reales de cada integrante, con lo que hubo que corregir, están en `prompts_<integrante>.md` de esta carpeta.

---

## 1. Qué tiene que leer la IA (en este orden)

1. `docs/spec_kit/1_constitution.md` — **incluidas las Enmiendas de la versión 2** al final.
2. `docs/spec_kit/versiones/v2_gestion_profesoral/2_spec.md` — qué se construye (y §9, las clarificaciones).
3. `3_plan.md` — cómo (capas, archivos, patrón por tabla).
4. `4_research.md` — por qué, sobre todo **D57–D64** (mandan sobre las anteriores).
5. `5_data_model.md` y `6_contracts.md` — columnas, procedimientos, rutas, cuerpos y desplegables.
6. El código de la v1 que ya existe (`api_gestion/`, `front_razor/`, `db/gestion_profesoral.sql`), para seguir su estilo.

## 2. Prompt para reproducir la v2

Este es el prompt que se le da a otra IA, junto con los archivos de arriba, para regenerar la v2:

```text
Eres el desarrollador del proyecto "Gestión Profesoral". Adjunto la constitución (con sus enmiendas de la v2),
el spec kit completo de la versión 2 y el código de la v1. La v1 está cerrada: no cambies sus rutas, DTO ni
pantallas; solo pasa sus repositorios a procedimientos almacenados (regla R9).

Construye la v2 siguiendo 3_plan.md, por fases, en este orden y con un commit por fase:
1. db/v2_procedimientos.sql: columna docente.total_estudios (idempotente), los 22 procedimientos de la v1,
   una vista vw_<tabla> por cada tabla de la v2 (fechas con CONVERT(CHAR(10), x, 23) y el nombre del padre),
   los 53 procedimientos de la v2, los 4 de lote con OPENJSON y los disparadores trg_contar_estudios
   (INSERT, UPDATE y DELETE, por conjuntos, con COUNT(*)) y trg_sello_docente. Errores con THROW 50001..50004
   y mensajes N'...' en castellano, como dice 5_data_model.md §4. Borrado lógico: activo = 0.
2. db/init.sh: tablas solo la primera vez, el script de la v2 en cada arranque, sqlcmd con -b -I -f 65001.
   La contraseña de sa viene de .env; agrega .env.example.
3. API: Repositorios/EjecutorProcedimientos.cs con Dapper (CommandType.StoredProcedure) y
   Middleware/ManejadorErroresSql.cs registrado DESPUÉS del ManejadorErrores de la v1.
4. API de las 11 tablas: por tabla un Modelo, una Petición, un Repositorio (solo llama procedimientos,
   sin ninguna palabra SQL), un Servicio y un Controlador, con las rutas de 6_contracts.md §3.2.
5. Front Razor: modelos Vista/Formulario, un cliente por tabla, Docentes (Index, Crear, Editar, Detalle con
   las siete secciones), las pantallas de edición del detalle y EstudiosRealizados/Detalle.
   Ninguna clave foránea se escribe: desplegables cargados de la API.
6. postman/: colección con las peticiones que deben pasar y las que deben fallar (404, 409, 422).

Reglas:
- Cada archivo nuevo empieza con un comentario en español que dice QUÉ hace y POR QUÉ está hecho así.
- Si algo del spec es ambiguo, NO lo supongas: escribe [NECESITA ACLARACIÓN: ...] y pregúntame.
- Si el spec contradice al código real de la v1, dímelo antes de escribir código.
```

## 3. Reglas que se le exigieron a la IA

| Regla | Por qué |
|---|---|
| Comentar el **porqué**, no solo el qué | Es lo que queda cuando la conversación se cierra; ayuda en la sustentación |
| Ninguna palabra SQL en `Repositorios/` (ni en comentarios) | El criterio 3 se revisa con `grep` |
| Marcar la ambigüedad en vez de rellenarla | Una suposición de la IA es una decisión de la spec disfrazada |
| Un commit por cambio, con mensaje en español que diga qué y por qué | Los commits son la evidencia de la sustentación individual |
| No adelantar la v3 (login, roles) ni la v4 | «Adelantarlos no suma, resta» (`VERSION_2.md` §10) |

## 4. Correcciones importantes durante la construcción

El detalle está en cada `prompts_<integrante>.md`. Las que cambiaron el resultado:

| Qué salió mal o no cuadraba | Cómo se corrigió |
|---|---|
| El repositorio tenía marcas de conflicto de un merge sin resolver en 12 archivos y no compilaba | Se tomó la versión completa del CRUD (la que coincide con la tabla `programa`) y se borraron los archivos duplicados |
| El spec decía EF Core y `DbContext`, pero la v1 está hecha con Dapper | Se enmendó la constitución y se agregó D57 antes de escribir la API |
| Los procedimientos del primer intento de `sp_crear_docente` usaban una columna que no existía y no llenaban los campos obligatorios | Se reescribieron con todas las columnas y la columna `total_estudios` se crea primero |
| `init.sh` volvía a crear las tablas en cada arranque (se perdían los datos) | Ahora crea las tablas solo si `docente` no existe |
| La API de la v1 aceptaba URL de 200 caracteres en una columna de 45 → `500` | Validación ajustada a 45 (D64) |
| Los comentarios de los repositorios decían «UPDATE … SET activo = 0» y el `grep` del criterio 3 los encontraba | Se reescribieron sin palabras SQL |
| En las pantallas de edición, la validación pedía un id que no venía en el formulario | Las claves van en campos ocultos y se fijan desde la ruta |

## 5. Preguntas de sustentación (para practicar)

| Pregunta | Dónde está la respuesta |
|---|---|
| Cuénteme `sp_crear_estudios_realizados_lote` sin leerlo línea por línea | `db/v2_procedimientos.sql`: revisa el docente, pasa el JSON a una tabla en memoria con el número de renglón, busca repetidos, guarda todo en un solo INSERT dentro de una transacción y devuelve las filas |
| ¿Por qué el total lo calcula un disparador y no el servicio en C#? | Porque así es imposible olvidarlo: cualquier camino que cambie un estudio (la API, otra aplicación, una edición a mano) lo actualiza. Y cuenta con `COUNT(*)`, así que se corrige solo |
| Si la base rechaza la clave foránea, ¿qué recibe quien llamó y qué ve la persona? | El procedimiento hace `THROW 50001`; `ManejadorErroresSql` responde `409` con el mensaje; el front lo muestra en rojo dentro de la sección |
| Si mañana hay que agregarle un campo al detalle, ¿dónde se toca? | Columna en la tabla → parámetro en `sp_crear_` y `sp_actualizar_` (y el lote) → la vista → Modelo y Petición de la API → objeto de parámetros del repositorio → Vista y Formulario del front → la pantalla |
| ¿Por qué el disparador escucha también `UPDATE`? | Porque retirar es `UPDATE activo = 0`; si solo oyera `INSERT` y `DELETE`, el contador no bajaría al retirar |
| ¿Por qué el middleware nuevo va después del de la v1 en `Program.cs`? | La excepción sube desde el controlador: el registrado de último la ve primero |
