# Especificación Funcional — Gestión Profesoral v2 (`2_spec.md`)

> **Documento 2 de 8 del spec kit de la v2.**
> Define **QUÉ** debe hacer el sistema en esta versión (solo el delta sobre la v1). El **CÓMO** está en `3_plan.md`.
> RF y RNF continúan la numeración de la v1 (que llegó a RF6 y RNF7).

---

## 1. Propósito

Operar desde el front **las 11 tablas con clave foránea** del módulo Gestión Profesoral, con:

- todo el CRUD por **procedimientos almacenados**,
- **disparadores** que mantienen datos derivados,
- **desplegables** para cada clave foránea (ninguna se digita),
- **maestro-detalle**: el detalle se ve y se agrega desde su maestro,
- errores previsibles de la base de datos traducidos a **409/422**, nunca `500`.

La v1 (5 tablas sin claves foráneas) queda intacta y **se vuelve a verificar completa**.

---

## 2. Alcance de la v2

**Entran (11):** `docente`, `estudios_realizados`, `evaluacion_docente`, `reconocimiento`, `experiecia`, `apoyo_profesoral`, `beca`, `docente_departamento`, `intereses_futuros`, `red_docente`, `estudio_ac`.

**Relaciones maestro → detalle:**

| Maestro | Detalle |
|---|---|
| `docente` | `estudios_realizados`, `evaluacion_docente`, `experiecia`, `reconocimiento` |
| `estudios_realizados` | `apoyo_profesoral`, `beca` |

**Tablas puente (clave primaria compuesta, dos claves foráneas):**

| Tabla | Une | Tiene atributos propios |
|---|---|---|
| `docente_departamento` | `docente` ↔ `programa` | sí (dedicación, modalidad, fechas) |
| `red_docente` | `red` ↔ `docente` | sí (fechas, actividades destacadas) |
| `intereses_futuros` | `docente` ↔ `termino_clave` | no (puente puro) |
| `estudio_ac` | `estudios_realizados` ↔ `area_conocimiento` | no (puente puro) |

**Fuera de alcance:** inicio de sesión, contraseñas, tokens, roles y `rol`/`usuario`/`rol_usuario` (v3); consultas multitabla, dashboard con gráficos, imagen corporativa y publicación (v4); paginación en servidor; modo administrativo para ver inactivos; `PATCH` (la v2 no lo usa); cambiar el código o el esquema de las 5 tablas de la v1.

---

## 3. Matriz entidad × operación (API)

Rutas completas en `6_contracts.md`. (✔ existe · — no existe)

| Tabla | Clave | GET lista | GET uno | POST | PUT | DELETE | Envío múltiple |
|---|---|:-:|:-:|:-:|:-:|:-:|:-:|
| `docente` | `cedula` | ✔ | ✔ | ✔ | ✔ | ✔ | — |
| `estudios_realizados` | `id` | ✔ | ✔ | ✔ | ✔ | ✔ | ✔ |
| `evaluacion_docente` | `id` | ✔ | ✔ | ✔ | ✔ | ✔ | ✔ |
| `reconocimiento` | `id` | ✔ | ✔ | ✔ | ✔ | ✔ | ✔ |
| `experiecia` | `id` | ✔ | ✔ | ✔ | ✔ | ✔ | ✔ |
| `apoyo_profesoral` | `estudios` | ✔ | ✔ | ✔ | ✔ | ✔ | — (1:1) |
| `beca` | `estudios` | ✔ | ✔ | ✔ | ✔ | ✔ | — (1:1) |
| `docente_departamento` | `docente` + `departamento` | ✔ | ✔ | ✔ | ✔ (solo atributos) | ✔ | ✔ |
| `red_docente` | `red` + `docente` | ✔ | ✔ | ✔ | ✔ (solo atributos) | ✔ | ✔ |
| `intereses_futuros` | `docente` + `termino_clave` | ✔ | ✔ | ✔ | — | ✔ | ✔ |
| `estudio_ac` | `estudio` + `area_conocimiento` | ✔ | ✔ | ✔ | — | ✔ | ✔ |

Semántica común (códigos y cuerpos en `6_contracts.md`):

- `GET` lista: solo `activo = 1`; vacío → `200` con `[]` (lista plana, como en la v1).
- `GET` uno: `404` si no existe o está inactivo.
- `POST`: `201` + `Location`. Hijo con padre inexistente o retirado → `409`. Clave repetida → `409`.
- `PUT`: reemplazo completo; clave de la ruta distinta de la del cuerpo → `422`; `404` si no existe.
- `DELETE`: `UPDATE … SET activo = 0`; `204`; `404` si no existe; `409` si es un maestro con detalle activo (§6, regla R2).
- Envío múltiple: una sola petición con una lista de renglones; todo o nada.

---

## 4. Requisitos funcionales — API

### RF7 — Docente (maestro)

CRUD según la matriz.

- `cedula` la **envía el cliente**. `linea_investigacion_principal` es **opcional** (acepta `null`).
- `fecha_actualizacion` y `total_estudios` **no los envía el cliente**: los gestiona la base de datos (`trg_sello_docente` y `trg_contar_estudios`; el alta fija la fecha de hoy).
- La respuesta incluye `total_estudios` y el nombre de la línea de investigación principal (`linea_investigacion_principal_nombre`), para mostrar sin consultas adicionales.

### RF8 — Detalle directo del docente

`estudios_realizados`, `evaluacion_docente`, `experiecia`, `reconocimiento`. Un renglón **no existe sin su `docente`**: crear uno con un `docente` inexistente o retirado → `409`.

- `estudios_realizados.id` lo envía el cliente (no es `IDENTITY`); en las otras tres, el `id` lo genera la base.
- Se pueden **listar y agregar desde el maestro**: `GET` y `POST` anidados bajo `/api/docente/{cedula}/…`.
- Cada respuesta incluye el nombre del docente (`docente_nombre`) para listados legibles.

### RF9 — Detalle del estudio (1:1)

`apoyo_profesoral` y `beca`: la clave primaria **es** la clave foránea `estudios`. Un estudio tiene a lo sumo un apoyo y una beca. Repetir con el mismo estudio activo → `409`; si está retirado, se **reactiva** (regla R3).

### RF10 — Tablas puente

- **Asignar** (`POST`) y **retirar** (`DELETE`) siempre con **las dos claves**.
- Repetir una pareja activa → `409`. Asignar una pareja retirada la **reactiva** (R3).
- Las puras (`intereses_futuros`, `estudio_ac`) **no se editan**. Las que tienen atributos (`docente_departamento`, `red_docente`) editan solo los atributos; las dos claves son de solo lectura.
- `estudio_ac` se maneja desde el estudio; `intereses_futuros`, `docente_departamento` y `red_docente`, desde el docente.

### RF11 — Envío múltiple (maestro-detalle)

`POST` anidado con una **lista** de renglones (máximo **100**): `POST /api/docente/{cedula}/estudios-realizados`, y análogos.

- Se guardan **todos o ninguno** (una transacción dentro del procedimiento `sp_crear_<tabla>_lote`).
- Lista vacía o con más de 100 renglones → `422`.
- Dos renglones iguales dentro del mismo envío → `409` con el número del renglón en el mensaje.
- El disparador se dispara **una vez por envío**, no una vez por renglón.

### RF12 — Disparadores

- **`trg_contar_estudios`** (tabla `estudios_realizados`): mantiene `docente.total_estudios` = número de estudios **activos** del docente. Escucha `INSERT`, `UPDATE` y `DELETE`; recalcula también al **retirar** (es un `UPDATE`) y al **cambiar de docente**.
- **`trg_sello_docente`** (tabla `docente`): pone `fecha_actualizacion` con la fecha de hoy en cada `UPDATE` hecho por el usuario. No se activa cuando el único cambio es `total_estudios`.
- Ambos operan por conjuntos y salen temprano si no hay filas que procesar.

### RF13 — Integridad referencial

| Situación | Respuesta |
|---|---|
| Crear hijo cuyo padre no existe (o está retirado) | `409` |
| Repetir una pareja activa en una tabla puente | `409` |
| Retirar un maestro con detalle activo | `409` |
| Clave primaria repetida (activa o retirada, en tablas con clave enviada por el cliente) | `409` |
| Dato con formato o rango inválido | `422` |
| Registro inexistente o retirado en `GET`/`PUT`/`DELETE` | `404` |
| Cualquier error previsible de la base | **Nunca `500`** |

---

## 5. Requisitos funcionales — Front (Razor Pages)

### RF14 — Front de las 11 tablas

El front (puerto 8010) añade **un solo enlace** a la barra: **Docentes**. Desde ahí se opera todo el módulo.

- **RF14.1 Docentes:** `Index` (lista con total de estudios, retirar con confirmación), `Crear`, `Editar` (la `cedula` aparece bloqueada).
- **RF14.2 Detalle del docente** (`/Docentes/Detalle/{cedula}`): muestra los datos del docente y, **en la misma pantalla**, el detalle: estudios, evaluaciones, experiencias, reconocimientos, departamentos, intereses futuros y redes. Cada sección lista, agrega (uno o **varios renglones de una vez**), edita y retira sin salir del docente.
- **RF14.3 Detalle del estudio** (`/EstudiosRealizados/Detalle/{id}`, se llega desde el docente): apoyo, beca y áreas de conocimiento del estudio.
- **RF14.4 Desplegables:** cada clave foránea se elige de un desplegable cargado desde la API (ver `6_contracts.md` §5). Muestra el nombre y manda la clave. Solo ofrece registros activos.
- **RF14.5 Opcional:** `linea_investigacion_principal` lleva «(ninguna)» y manda `null`.
- **RF14.6 Desplegable vacío:** si no hay opciones (por ejemplo, `programa` empieza vacía), la pantalla lo dice en castellano y enlaza a la pantalla donde crear la primera.
- **RF14.7 Mensajes:** éxito (verde) y error (rojo) en castellano. `409`/`422` se muestran en el formulario; `404` redirige con aviso; si la API no responde: «No se pudo conectar con la API de gestión.» **Sin jerga** (nada de «PUT», «422», «FK», números de error).
- **RF14.8 Puentes:** las puras no tienen botón de editar; las que tienen atributos editan solo los atributos.
- **RF14.9 Un solo envío:** el formulario de varios renglones envía una única petición a la API.

---

## 6. Reglas de negocio de la v2 (decisiones del equipo)

Estas reglas no las impone la base de datos: las decide el equipo y viven en el servicio o en el procedimiento.

| ID | Regla |
|---|---|
| **R1** | Los nombres del esquema se conservan tal cual (`experiecia`, `nacionalidaad`). No se crea una segunda fuente de verdad corrigiéndolos. |
| **R2** | **Retirar un maestro con detalle activo da `409`**: «No se puede retirar: tiene N registros activos en su detalle.» Aplica a `docente` (estudios, evaluaciones, experiencias, reconocimientos, departamentos, intereses, redes) y a `estudios_realizados` (apoyo, beca, áreas). |
| **R3** | En tablas cuya clave primaria es una clave foránea (puentes y 1:1: `apoyo_profesoral`, `beca`, `docente_departamento`, `intereses_futuros`, `red_docente`, `estudio_ac`), volver a asignar una pareja **retirada la reactiva** con los datos nuevos; si está activa, `409`. Difiere de la v1, que no reactiva. |
| **R4** | `evaluacion_docente.calificacion` va **entre 0 y 5** (inclusive). |
| **R5** | `total_estudios` cuenta solo estudios **activos**. |
| **R6** | Los puentes puros no tienen `sp_actualizar_` (una pareja existe o no existe). |
| **R7** | Retirar en la v1 un registro que la v2 usa (un `programa`, `red`, `termino_clave`, `linea_investigacion` o `area_conocimiento`) **no se bloquea**, porque la v1 está cerrada. Las filas de la v2 siguen mostrando su nombre; los desplegables dejan de ofrecerlo. Limitación documentada. |
| **R8** | `docente.cedula` y `estudios_realizados.id` los envía el cliente (como `programa.id` en la v1). |

---

## 7. Requisitos no funcionales

- **RNF8 — SQL solo en la base.** El repositorio contiene únicamente la llamada al procedimiento con parámetros tipados (Constitución, Art. 8 y 12).
- **RNF9 — Errores previsibles ≠ 500.** Clave foránea, duplicado y retiro bloqueado → `409`; formato inválido → `422` (Art. 14).
- **RNF10 — Disparadores por conjuntos.** Un lote de 100 renglones produce una sola ejecución del disparador (Art. 13).
- **RNF11 — Atomicidad.** Un envío múltiple se guarda completo o no se guarda.
- **RNF12 — Regresión.** La v1 pasa completa sin haberse modificado (Art. 17).
- **RNF13 — Interpretabilidad.** Comentarios en cada archivo nuevo; prompts y correcciones entregados (Art. 16).
- **RNF14 — Resiliencia.** Igual que la v1: la API arranca sin esperar a la base; el front tolera que la API no responda.

---

## 8. Criterios de aceptación

| ID | Criterio |
|---|---|
| CA-V2-01 | Las tablas de la v1 tienen CRUD completo y su interfaz (regresión de CA-01 a CA-11 de la v1). |
| CA-V2-02 | Las 11 tablas de la v2 tienen CRUD completo e interfaz: crear, editar, retirar. |
| CA-V2-03 | Los cinco verbos de las 11 tablas pasan por procedimientos almacenados: **no hay SQL escrito a mano en el repositorio** (`grep` de `SELECT`/`INSERT`/`UPDATE`/`DELETE` sin resultados en los repositorios de la v2). |
| CA-V2-04 | Al menos un disparador funciona: se crean estudios por la interfaz y `total_estudios` del docente cambia **sin que la API lo envíe**; también al retirar. |
| CA-V2-05 | Cada clave foránea se elige de un desplegable **lleno** que muestra nombres, no códigos. |
| CA-V2-06 | La clave foránea opcional acepta «(ninguna)» y manda `null`: se crea un docente sin línea principal. |
| CA-V2-07 | Crear un hijo con un padre inexistente responde `409` en castellano, desde la colección de pruebas. Un `500` es criterio fallado. |
| CA-V2-08 | Las tablas puente se asignan y se retiran con sus dos claves; repetir la pareja da `409`. |
| CA-V2-09 | El detalle se ve y se agrega desde el maestro. |
| CA-V2-10 | Retirar un registro lo saca de los listados y los desplegables; **sigue en la base** con `activo = 0`. |
| CA-V2-11 | Ninguna pantalla usa jerga («PUT», «PATCH», «422», «FK»). |
| CA-V2-12 | La regresión de la v1 pasa **completa** (`7_quickstart.md` §3). |
| CA-V2-13 | Un envío de tres renglones es **una petición**; si el tercero es inválido, no se guarda ninguno. |
| CA-V2-14 | Retirar un docente con detalle activo responde `409` (regla R2). |
| CA-V2-15 | Los prompts y las correcciones a la IA están entregados (`prompts_v2.md`) y el tag `v2` apunta al commit que pasa los criterios anteriores. |
