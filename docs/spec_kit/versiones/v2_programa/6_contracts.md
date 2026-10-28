# Contratos de integración — Gestión Profesoral v2

> Documento 6 de 8 del spec kit de la v2: cómo se hablan las piezas entre sí en lo que la v2 añade (contratos de la API, desplegables, contrato front ↔ API y API ↔ BD).
> Lo que no se menciona aquí rige como en `docs/6_contracts.md` (v1): puertos, variables de entorno, healthchecks, volúmenes y credenciales **no cambian**.

---

## 1. Puertos, variables y credenciales

Sin cambios: front `8010`, API `8074` (`/swagger`), SQL Server `11443`; `ConnectionStrings__GestionLocal` y `ApiGestion__BaseUrl` igual que en la v1; credenciales `sa` / `Paradigmas123!`. El `docker-compose.yml` no se modifica.

---

## 2. Convenciones nuevas

- Cuerpos JSON en `snake_case` (incluidos `nacionalidaad` y las rutas de `experiecia`, por R1).
- Fechas `yyyy-MM-dd`. Claves opcionales: `null` (nunca cadena vacía).
- Rutas: `kebab-case` del nombre de la tabla (`/api/estudios-realizados`, `/api/docente-departamento`).
- Parámetros numéricos con restricción `:int`. La clave de texto (`termino_clave`, `area_conocimiento`) se escapa con `Uri.EscapeDataString` en el cliente.
- Las listas son **listas planas** (`[]` si no hay filas), como en la v1.
- Ninguna respuesta incluye `activo`.

---

## 3. Contrato de la API v2

### 3.1 Códigos de estado (los de la v1, con significado ampliado)

| Código | Cuándo en la v2 |
|---|---|
| `200` | `GET`, `PUT` exitosos (lista vacía = `[]`) |
| `201` | `POST` exitoso, también el envío múltiple (devuelve la lista de filas creadas) |
| `204` | `DELETE` exitoso |
| `404` | La clave no existe o está inactiva |
| `409` | Padre inexistente o retirado · pareja repetida · clave repetida · retiro de un maestro con detalle activo · renglón repetido dentro de un envío múltiple |
| `422` | Datos inválidos (formato, longitud, rango), clave de ruta ≠ clave del cuerpo, JSON mal formado, lista vacía o de más de 100 renglones |
| `500` | Solo errores realmente inesperados. **Un error de clave foránea, duplicado o retiro bloqueado nunca es `500`** |

Formato único de error (igual que la v1):

```json
{ "mensaje": "No se puede registrar: el docente 99 no existe o está retirado.", "detalle": [] }
```

Mensajes tipo: `409` padre → `"No se puede registrar: el docente 99 no existe o está retirado."` · `409` pareja → `"El docente 10 ya tiene asignado el programa 3."` · `409` retiro → `"No se puede retirar: tiene 5 registros activos en su detalle."` · `409` lote → `"El renglón 3 repite el id 7."` · `404` → `"No se encontró el docente con cédula 99."` · `422` lote → `"El envío debe tener entre 1 y 100 renglones."` · `422` → `"Datos inválidos."` con `detalle: ["calificacion: La calificación debe estar entre 0 y 5."]`.

### 3.2 Endpoints

Todos con prefijo `/api`. «Anidado» = ruta desde el maestro (lista y envío múltiple).

| Recurso | Clave en la ruta | Verbos de la colección y por clave | Anidado (desde el maestro) |
|---|---|---|---|
| `docente` | `{cedula:int}` | `GET` · `GET /{cedula}` · `POST` · `PUT /{cedula}` · `DELETE /{cedula}` | — |
| `estudios-realizados` | `{id:int}` | ídem | `GET`, `POST` (lista) en `/docente/{cedula:int}/estudios-realizados` |
| `evaluacion-docente` | `{id:int}` | ídem | `GET`, `POST` (lista) en `/docente/{cedula:int}/evaluacion-docente` |
| `reconocimiento` | `{id:int}` | ídem | `GET`, `POST` (lista) en `/docente/{cedula:int}/reconocimiento` |
| `experiecia` | `{id:int}` | ídem | `GET`, `POST` (lista) en `/docente/{cedula:int}/experiecia` |
| `apoyo-profesoral` | `{estudios:int}` | ídem | — (1:1: se consulta por la clave del estudio) |
| `beca` | `{estudios:int}` | ídem | — (1:1) |
| `docente-departamento` | `{docente:int}/{departamento:int}` | `GET` · `GET` por clave · `POST` · `PUT` (solo atributos) · `DELETE` | `GET`, `POST` (lista) en `/docente/{cedula:int}/docente-departamento` |
| `red-docente` | `{red:int}/{docente:int}` | ídem | `GET`, `POST` (lista) en `/docente/{cedula:int}/red-docente` |
| `intereses-futuros` | `{docente:int}/{termino}` | `GET` · `GET` por clave · `POST` · `DELETE` (**sin `PUT`**) | `GET`, `POST` (lista) en `/docente/{cedula:int}/intereses-futuros` |
| `estudio-ac` | `{estudio:int}/{area}` | ídem | `GET`, `POST` (lista) en `/estudios-realizados/{id:int}/estudio-ac` |

Códigos por verbo: `GET` lista `200` · `GET` por clave `200`/`404` · `POST` `201`/`409`/`422` · `PUT` `200`/`404`/`422` · `DELETE` `204`/`404`/`409` (`409` solo en `docente` y `estudios-realizados`).

### 3.3 Cuerpos de petición y respuesta (ejemplos)

**Docente** — `POST`/`PUT` (`DocenteCrear`; sin `fecha_actualizacion` ni `total_estudios`; `linea_investigacion_principal` opcional):

```json
{
  "cedula": 1017245, "nombres": "Ana", "apellidos": "Torres Gómez",
  "genero": "Femenino", "cargo": "Profesora titular", "fecha_nacimiento": "1985-04-12",
  "correo": "ana.torres@universidad.edu.co", "telefono": "3001234567",
  "url_cvlac": "https://scienti.minciencias.gov.co/cvlac/1017245",
  "escalafon": "Titular", "perfil": "Investigadora en ingeniería de software.",
  "cat_minciencia": null, "conv_minciencia": "Convocatoria 894 de 2021",
  "nacionalidaad": "Colombiana", "linea_investigacion_principal": null
}
```

Respuesta (`DocenteRespuesta`): los mismos campos más lo que calcula la base:

```json
{ "...": "...", "linea_investigacion_principal": null, "linea_investigacion_principal_nombre": null,
  "fecha_actualizacion": "2026-10-28", "total_estudios": 0 }
```

**Estudio realizado** — `POST` (`EstudiosRealizadosCrear`):

```json
{ "id": 1, "titulo": "Maestría en Ingeniería", "universidad": "Universidad de Antioquia",
  "fecha": "2012-12-15", "tipo": "Maestría", "ciudad": "Medellín", "docente": 1017245,
  "ins_acreditada": 1, "metodologia": "Presencial", "perfil_egresado": "Investigador en software.", "pais": "Colombia" }
```

Respuesta: los mismos campos más `"docente_nombre": "Ana Torres Gómez"`.

**Evaluación docente** — `POST`: `{ "calificacion": 4.5, "semestre": "2026-1", "docente": 1017245 }` (el `id` lo genera la base y viaja en la respuesta).

**Apoyo profesoral / beca** — `POST` y `PUT`: `{ "estudios": 1, "con_apoyo": 1, "institucion": "Colciencias", "tipo": "Parcial" }` · `{ "estudios": 1, "tipo": "Posgrado", "institucion": "Fulbright", "fecha_inicio": "2010-08-01", "fecha_fin": null }`.

**Tablas puente** — `POST`:

```json
{ "docente": 1017245, "departamento": 3, "dedicacion": "Tiempo completo", "modalidad": "Presencial", "fecha_ingreso": "2015-02-01", "fecha_salida": null }
{ "red": 1, "docente": 1017245, "fecha_inicio": "2018-03-10", "fecha_fin": null, "act_destacadas": "Coordinación de eventos." }
{ "docente": 1017245, "termino_clave": "Inteligencia artificial" }
{ "estudio": 1, "area_conocimiento": "1B01" }
```

Respuestas con el nombre del padre para mostrar (`docente_nombre`, `departamento_nombre`, `red_nombre`, `estudio_titulo`, `area_conocimiento_disciplina`).

---

## 4. Contrato del envío múltiple (maestro-detalle)

`POST /api/docente/{cedula}/estudios-realizados` con **una lista**. El `docente` sale de la ruta (en los renglones no se envía):

```json
[
  { "id": 1, "titulo": "Pregrado en Sistemas", "universidad": "UdeA", "fecha": "2008-06-20", "tipo": "Pregrado", "ciudad": "Medellín", "ins_acreditada": 1, "metodologia": "Presencial", "perfil_egresado": "Ingeniero.", "pais": "Colombia" },
  { "id": 2, "titulo": "Maestría en Ingeniería", "universidad": "UdeA", "fecha": "2012-12-15", "tipo": "Maestría", "ciudad": "Medellín", "ins_acreditada": 1, "metodologia": "Presencial", "perfil_egresado": "Investigador.", "pais": "Colombia" },
  { "id": 3, "titulo": "Doctorado en Informática", "universidad": "UPM", "fecha": "2018-09-30", "tipo": "Doctorado", "ciudad": "Madrid", "ins_acreditada": 1, "metodologia": "Presencial", "perfil_egresado": "Doctor.", "pais": "España" }
]
```

| Resultado | Código | Efecto |
|---|---|---|
| Los 3 válidos | `201` + lista de las 3 filas guardadas | Se guardan las 3 y el disparador corre **una vez** (`total_estudios` pasa a 3) |
| El renglón 3 inválido (formato) | `422` con `detalle` por renglón | **No se guarda ninguno** |
| El docente no existe o está retirado | `409` | No se guarda ninguno |
| Un id repetido dentro del envío o ya existente | `409` con el número del renglón | No se guarda ninguno |
| Lista vacía o > 100 | `422` | — |

---

## 5. Desplegables: de dónde salen las opciones

Cada clave foránea se elige de un `<select>` **cargado desde la API**: el texto es lo que la persona lee, el `value` es lo que viaja.

| Campo (clave foránea) | Fuente | La persona **ve** | Se **manda** | Opcional |
|---|---|---|---|---|
| `docente` (en hijos y puentes, cuando no viene de la ruta) | `GET /api/docente` | `nombres` + `apellidos` | `cedula` | no |
| `linea_investigacion_principal` | `GET /api/linea-investigacion` | `nombre` | `id` | **sí** → «(ninguna)» manda `null` |
| `departamento` (en `docente_departamento`) | `GET /api/programa` | `nombre` | `id` | no |
| `red` (en `red_docente`) | `GET /api/red` | `nombre` | `idr` | no |
| `termino_clave` (en `intereses_futuros`) | `GET /api/termino-clave` | `termino` (e inglés, si existe) | `termino` | no |
| `area_conocimiento` (en `estudio_ac`) | `GET /api/area-conocimiento` | `id` — `disciplina` | `id` | no |
| `estudios` / `estudio` (en `apoyo`, `beca`, `estudio_ac`) | viene de la ruta de la pantalla del estudio | — | `id` | no |

Reglas:

- Todas las fuentes **ya filtran `activo = 1`**: nunca se ofrece un padre retirado.
- Se cargan en `OnGetAsync` **y** al repintar el formulario tras un `409` o `422`.
- Si una fuente devuelve `[]`, el desplegable no se pinta vacío: se muestra el aviso en castellano con enlace a la pantalla donde crear el primero. Ejemplos: «Aún no hay programas. Cree uno primero en **Programas**.» (enlace a `/Programas/Crear`), «Aún no hay redes…» (`/Redes/Crear`), «Aún no hay términos clave…» (`/TerminosClave/Crear`).
- «(ninguna)» envía `null`; el modelo del front usa `int?` para que una opción vacía no se convierta en cadena vacía.

---

## 6. Contrato front ↔ API

Igual que la v1: el front consume la API **desde el servidor** (`HttpClient` tipado, `BaseAddress = ApiGestion:BaseUrl`), `SnakeCaseLower` en ambos lados, y toda respuesta no exitosa se convierte en `ErrorApiException(codigoHttp, mensaje, detalle)`.

| Resultado de la API | Qué hace el front |
|---|---|
| `200` / `201` / `204` | `TempData["Exito"]` con mensaje en español y redirección (o muestra los datos) |
| `409` / `422` | Vuelve a pintar el formulario con cada ítem de `detalle` (o el `mensaje`) como error; recarga los desplegables |
| `404` | `TempData["Error"]` y redirección al `Index` o al `Detalle` del maestro |
| `503` / `500` | `TempData["Error"]` con el mensaje. Nunca se muestra jerga («PUT», «422», «FK») |

### Rutas del front (nuevas)

| Pantalla | Ruta |
|---|---|
| Lista de docentes | `/Docentes` |
| Crear docente | `/Docentes/Crear` |
| Editar docente | `/Docentes/Editar/{cedula}` |
| **Detalle del docente** (estudios, evaluaciones, experiencias, reconocimientos, departamentos, intereses, redes) | `/Docentes/Detalle/{cedula}` |
| Editar un estudio | `/Docentes/{cedula}/Estudios/Editar/{id}` |
| Editar una evaluación | `/Docentes/{cedula}/Evaluaciones/Editar/{id}` |
| Editar una experiencia | `/Docentes/{cedula}/Experiencias/Editar/{id}` |
| Editar un reconocimiento | `/Docentes/{cedula}/Reconocimientos/Editar/{id}` |
| Editar un departamento | `/Docentes/{cedula}/Departamentos/Editar/{departamento}` |
| Editar una red | `/Docentes/{cedula}/Redes/Editar/{red}` |
| **Detalle del estudio** (apoyo, beca, áreas) | `/EstudiosRealizados/Detalle/{id}` |

Retirar: botón en cada fila con `onsubmit="return confirm('¿Retirar este registro?')"` que envía un `POST` al handler correspondiente. Las puentes puras (`intereses_futuros`, `estudio_ac`) **no tienen** botón de editar.

### Textos de la interfaz (sin jerga)

| Situación | Texto mostrado |
|---|---|
| `409` por padre | «No se puede registrar: el docente no existe o está retirado.» |
| `409` por pareja | «Ese docente ya tiene asignado ese programa.» |
| `409` por retiro | «No se puede retirar: tiene registros activos en su detalle. Retírelos primero.» |
| Éxito al agregar varios | «Se agregaron 3 estudios.» |
| API caída | «No se pudo conectar con la API de gestión.» |

---

## 7. Contrato API ↔ BD

- Base `gestion_local` en SQL Server; cadena desde `ConnectionStrings__GestionLocal`.
- La API llama **únicamente a procedimientos almacenados** (`CommandType.StoredProcedure`) con parámetros tipados; **no hay SQL en los repositorios**.
- Convención de nombres: `sp_listar_`, `sp_consultar_`, `sp_crear_`, `sp_actualizar_`, `sp_eliminar_<tabla>` y `sp_crear_<tabla>_lote`. Catálogo completo en `5_data_model.md` §3.
- Los procedimientos devuelven la fila **como quedó guardada**, sin `activo`, con el nombre del padre.
- Errores de la base: 50001/50002 → `409`, 50003 → `422`, 50004 → `404`; nativos 547/2627/2601 → `409` (red de seguridad). Ver `5_data_model.md` §4.
- Los disparadores se disparan solos: la API **nunca** envía `total_estudios` ni `fecha_actualizacion`.
- Borrado lógico (`activo = 0`); todo `sp_listar_` filtra `activo = 1`.
- Conexión perezosa con `EnableRetryOnFailure` (v1); sin `depends_on` API → BD.
