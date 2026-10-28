# Tareas — Plan de fases y commits (v2)

> Documento 8 de 8 del spec kit de la v2: orden de construcción de lo que la v2 añade (BD + API + front + entrega).
> Requisitos: [2_spec.md](2_spec.md) · arquitectura: [3_plan.md](3_plan.md) · decisiones: [4_research.md](4_research.md) · datos: [5_data_model.md](5_data_model.md) · contratos: [6_contracts.md](6_contracts.md) · verificación: [7_quickstart.md](7_quickstart.md) · principios: [1_constitution.md](1_constitution.md).

**Cómo usar este documento**

- IDs `T-V2-xxx` (para no chocar con los `T-xxx` de la v1), checkbox y requisitos que cubre (`RF`/`RNF`, numeración continuada de la v1).
- Cada fase termina en **un commit** (mensaje sugerido) y en algo **verificable** con el paso indicado del quickstart.
- **Definición de terminado** (Art. 11 de la v1 + Art. 16 y 17 de la v2): compila sin errores; la verificación de la fase pasa; cada archivo nuevo (`.cs`, `.sql`) tiene su comentario de cabecera en español que explica qué hace **y por qué**; los documentos siguen coherentes con el código; **no se modificó ningún archivo de la v1** salvo líneas añadidas (`git diff` lo demuestra); commit hecho y checkbox marcado.
- **Spec kit ANTES que el código:** la Fase 0 va primero; un contrato que cambie se corrige en el documento antes que en el código.
- **Autoría:** cada integrante hace los commits de lo que construye y debe poder explicarlo en voz alta (Art. 16). Repartan por **vertical completa** (SQL → API → front de una tabla), no por capas sueltas.
- Patrón: `docente` se construye **completa primero** (API en la Fase 3, front en la Fase 7); las demás tablas replican ese patrón.

---

## Fase 0 — Spec kit de la v2 y línea base

- [x] **T-V2-001** Crear `docs/spec_kit/versiones/v2_gestion_profesoral/` con los 8 documentos del kit (solo el delta sobre la v1).
- [ ] **T-V2-002** Escribir `9_checklist.md` y la guía de IA de la v2.
- [ ] **T-V2-003** Confirmar con el profesor las decisiones abiertas: puentes puras sin `sp_actualizar_` (R6) y los dos disparadores (A y B).
- [ ] **T-V2-004** Resolver las marcas **[CONFIRMAR]** de `3_plan.md` contra el código real de la v1 (cómo está implementado el manejador de excepciones, dónde viven `Respuestas/` y `GestionDbContext`, convención de `Modelos/` del front, namespaces) y ajustar el plan **antes** de escribir código.
- [ ] **T-V2-005** Crear `prompts_v2.md` (plantilla: prompt usado · resultado · **qué hubo que corregirle a la IA** · cómo se corrigió) y registrar desde el primer prompt.
- [ ] **T-V2-006** Línea base: con la v1 sin cambios, correr los pasos 1 a 9 de su quickstart y anotar el resultado (y las observaciones conocidas: `red.url`, marcas de conflicto en su `7_quickstart.md`).

**Cubre:** RNF12, RNF13. **Verificar:** la carpeta existe, la línea base de la v1 pasa.
**Commit:** `fase 0 v2: spec kit de la version 2`

---

## Fase 1 — Base de datos: columna, procedimientos y disparadores

- [ ] **T-V2-101** `db/sqlserver/v2_procedimientos.sql`: comentario de cabecera, `ALTER TABLE docente ADD total_estudios` idempotente (`COL_LENGTH`).
- [ ] **T-V2-102** Procedimientos de `docente` (5): `crear` fija `fecha_actualizacion`; `eliminar` aplica R2 con el conteo de detalle.
- [ ] **T-V2-103** Procedimientos de los hijos directos `estudios_realizados`, `evaluacion_docente`, `reconocimiento`, `experiecia` (4 × 5), con `listar(@docente = NULL)`, errores 50001–50004 y mensajes `N'…'`.
- [ ] **T-V2-104** Procedimientos de `apoyo_profesoral` y `beca` (2 × 5), con reactivación (R3).
- [ ] **T-V2-105** Procedimientos de las puentes: `docente_departamento` y `red_docente` (5 cada una, `actualizar` solo atributos) e `intereses_futuros` y `estudio_ac` (4 cada una, sin `actualizar`), con reactivación (R3).
- [ ] **T-V2-106** Los 8 procedimientos de lote `sp_crear_<tabla>_lote` (`OPENJSON`, repetidos dentro del envío, una transacción).
- [ ] **T-V2-107** Disparadores `trg_contar_estudios` (INSERT/UPDATE/DELETE, por conjuntos, salidas tempranas) y `trg_sello_docente` (con guarda de columnas).
- [ ] **T-V2-108** `db/sqlserver/v2_verificacion.sql`: casos felices **y de fallo** directamente en SQL (padre inexistente, pareja repetida, retiro bloqueado, lote de 100, cambio de docente de un estudio, retiro de un estudio).
- [ ] **T-V2-109** Ajustar `init.sh` para ejecutar `v2_procedimientos.sql` con `-I -b` después del script de la v1. Aplicar con `docker compose down -v` + `up -d`.

**Cubre:** RF12, RF13, RNF8, RNF10, RNF11. **Verificar:** quickstart paso 1 (61 procedimientos, 2 disparadores, columna creada) y `v2_verificacion.sql` sin fallos; `sqlserver-init` en `Exited (0)`.
**Commit:** `fase 1 v2: procedimientos almacenados y disparadores`

---

## Fase 2 — Infraestructura de la API

- [ ] **T-V2-201** `Repositorios/EjecutorProcedimientos.cs` (`LeerAsync`, `EjecutarAsync`) y su registro `AddScoped`. Sin lógica por tabla.
- [ ] **T-V2-202** `ManejadorExcepcionesSql` (50001/50002/2627/2601/547 → `409`, 50003 → `422`, 50004 → `404`; el resto, no lo maneja) y su registro **en el orden correcto** respecto del manejador de la v1.

**Cubre:** RNF8, RNF9. **Verificar:** compila; la API arranca; (el efecto real se comprueba en la Fase 3).
**Commit:** `fase 2 v2: ejecutor de procedimientos y manejador de errores SQL`

---

## Fase 3 — Vertical completa de `docente` (API)

- [ ] **T-V2-301** `Peticiones/DocenteCrear.cs` (anotaciones de `5_data_model.md` §6; sin `fecha_actualizacion` ni `total_estudios`; línea principal opcional) y `Respuestas/DocenteRespuesta.cs`.
- [ ] **T-V2-302** `Repositorios/RepositorioDocenteSqlServer.cs` con `IRepositorioDocente` (5 métodos, una llamada al procedimiento cada uno).
- [ ] **T-V2-303** `Servicios/ServicioDocente.cs`: `Trim`, `cedula` de ruta = `cedula` de cuerpo (`422`), «no hay fila» → `ExcepcionNoEncontrada`.
- [ ] **T-V2-304** `Controllers/DocenteController.cs` con los 5 endpoints (`201` + `CreatedAtAction`, `204`).
- [ ] **T-V2-305** Registrar repositorio y servicio en `Program.cs`.

**Cubre:** RF7, RF13. **Verificar:** quickstart paso 3 y el paso 9 (retiro bloqueado, borrado lógico), más el `409` de padre inexistente con `linea_investigacion_principal` = 999.
**Commit:** `fase 3 v2: API de docente de punta a punta`

---

## Fase 4 — Detalle directo del docente y envío múltiple (API)

Cada tarea repite el patrón de la Fase 3 y añade las rutas anidadas (`GET` y `POST` con lista) más `CrearLoteAsync`.

- [ ] **T-V2-401** `estudios_realizados` — RF8, RF11: `id` enviado por el cliente; lote de 1 a 100.
- [ ] **T-V2-402** `evaluacion_docente` — `calificacion` entre 0 y 5; `id` generado por la base.
- [ ] **T-V2-403** `reconocimiento`.
- [ ] **T-V2-404** `experiecia` — `fecha_fin` ≥ `fecha_inicio`.

**Cubre:** RF8, RF11, RF12. **Verificar:** quickstart pasos 4, 5, 6 y 7 (el disparador cambia `total_estudios` sin que la API lo envíe; todo o nada del lote).
**Commit (uno por tarea):** `fase 4 v2: API de <tabla>`

---

## Fase 5 — Detalle 1:1 del estudio (API)

- [ ] **T-V2-501** `apoyo_profesoral` — RF9: clave = `estudios`; reactivación (R3).
- [ ] **T-V2-502** `beca` — RF9.

**Cubre:** RF9. **Verificar:** quickstart paso 7 (crear, repetir → `409`, retirar, volver a crear → `201`).
**Commit (uno por tarea):** `fase 5 v2: API de <tabla>`

---

## Fase 6 — Tablas puente (API)

- [ ] **T-V2-601** `docente_departamento` — `PUT` solo atributos; clave compuesta en la ruta.
- [ ] **T-V2-602** `red_docente` — `fecha_fin` como texto validado (D44).
- [ ] **T-V2-603** `intereses_futuros` — sin `PUT`; término escapado en la ruta.
- [ ] **T-V2-604** `estudio_ac` — sin `PUT`; ruta anidada desde el estudio.

**Cubre:** RF10, RF13. **Verificar:** quickstart paso 8 (asignar, repetir → `409`, retirar, reasignar, padre inexistente → `409`, `PUT` en puras → `405`).
**Commit (uno por tarea):** `fase 6 v2: API de <tabla>`

---

## Fase 7 — Front (Razor Pages)

- [ ] **T-V2-701** Modelos y clientes de `docente`: `ClienteDocente` con su interfaz y su `AddHttpClient`; modelo con `int?` en la línea principal.
- [ ] **T-V2-702** `Pages/Docentes/Index` (tabla con total de estudios, retirar con confirmación) y `Crear` (desplegable de línea principal con «(ninguna)» → `null`; aviso con enlace si no hay opciones; errores `409`/`422` en el formulario).
- [ ] **T-V2-703** `Pages/Docentes/Editar/{cedula}` (cédula bloqueada).
- [ ] **T-V2-704** `Pages/Docentes/Detalle/{cedula}`: datos del docente + sección de **estudios** con formulario de varios renglones (JavaScript simple, **una sola petición**) y retiro.
- [ ] **T-V2-705** Replicar la sección y su `Editar` para **evaluaciones**, **experiencias** y **reconocimientos** (clientes + vistas parciales).
- [ ] **T-V2-706** Secciones de **departamentos** (desplegable de programas), **intereses** (desplegable de términos; sin editar) y **redes** (desplegable de redes), con los clientes de las puentes.
- [ ] **T-V2-707** `Pages/EstudiosRealizados/Detalle/{id}`: apoyo y beca (un formulario que crea o actualiza según exista) y áreas de conocimiento (desplegable; sin editar).
- [ ] **T-V2-708** Enlace «Docentes» en `_Layout.cshtml`; revisar todos los textos (sin «PUT», «PATCH», «422», «FK», números de error).

**Cubre:** RF14, CA-V2-05, CA-V2-06, CA-V2-09, CA-V2-11. **Verificar:** quickstart paso 10 y la prueba de la API caída.
**Commit (uno por tarea o grupo):** `fase 7 v2: front de <pantalla>`

---

## Fase 8 — Pruebas, regresión y entrega

- [ ] **T-V2-801** Pruebas de servicios con **repositorios falsos** (xUnit): `ServicioDocente` (cédula de ruta ≠ cuerpo, no encontrado), validación de la lista de lote (vacía, > 100), `fecha_fin` < `fecha_inicio`, calificación fuera de rango.
- [ ] **T-V2-802** **Colección de pruebas** de la v2 con las peticiones nuevas, **incluidas las que deben fallar** (`409` padre inexistente, `409` pareja repetida, `409` retiro bloqueado, `422`, `404`).
- [ ] **T-V2-803** **Regresión de la v1 completa** (`7_quickstart.md` §3) desde `down -v`.
- [ ] **T-V2-804** Recorrer los criterios **CA-V2-01 a CA-V2-15** y marcarlos.
- [ ] **T-V2-805** Revisión de código: comentario de cabecera en cada archivo nuevo; `grep` sin SQL en los repositorios de la v2 (quickstart paso 11); `git diff` de la v1 solo con líneas añadidas.
- [ ] **T-V2-806** Completar `prompts_v2.md` con los prompts **realmente usados** (chat e IDE agéntico) y **lo que hubo que corregirle a la IA**.
- [ ] **T-V2-807** **Ensayo de interpretabilidad:** cada integrante abre un archivo que entregó, elegido por otro, y responde en voz alta las cuatro preguntas: qué hace, por qué así, qué pasa si falla, qué cambiaría. Si un comentario de la IA resulta incorrecto, se corrige.
- [ ] **T-V2-808** Actualizar `README.md` (alcance v2, cómo correr los procedimientos a mano), confirmar que los 8 documentos coinciden con lo construido y crear el tag `v2` sobre el commit que pasa los criterios.

**Cubre:** RNF12, RNF13, CA-V2-01, CA-V2-03, CA-V2-07, CA-V2-12, CA-V2-15. **Verificar:** quickstart pasos 1 a 11 y §3 completos.
**Commit:** `fase 8 v2: pruebas, regresion y entrega` y etiqueta `v2`.

---

## Matriz de trazabilidad

| Requisito | Tareas | Verificación (quickstart) | Criterio |
|---|---|---|---|
| RF7 Docente | T-V2-102, T-V2-301–305, T-V2-701–703 | Pasos 3, 9, 10 | CA-V2-02, CA-V2-06, CA-V2-14 |
| RF8 Detalle del docente | T-V2-103, T-V2-401–404, T-V2-704–705 | Pasos 4, 5, 7, 10 | CA-V2-02, CA-V2-07, CA-V2-09 |
| RF9 Apoyo y beca | T-V2-104, T-V2-501–502, T-V2-707 | Paso 7, 10 | CA-V2-02 |
| RF10 Tablas puente | T-V2-105, T-V2-601–604, T-V2-706 | Paso 8, 10 | CA-V2-08 |
| RF11 Envío múltiple | T-V2-106, T-V2-401–404, T-V2-704 | Paso 5, 10 | CA-V2-13 |
| RF12 Disparadores | T-V2-107, T-V2-401 | Pasos 5, 6 | CA-V2-04 |
| RF13 Integridad (409) | T-V2-103–106, T-V2-202 | Pasos 3, 4, 8, 9 | CA-V2-07, CA-V2-08, CA-V2-10 |
| RF14 Front | T-V2-701–708 | Paso 10 | CA-V2-05, CA-V2-06, CA-V2-09, CA-V2-11 |
| RNF8 SQL solo en la base | T-V2-101–109, T-V2-201, T-V2-805 | Pasos 1, 11 | CA-V2-03 |
| RNF9 Errores ≠ 500 | T-V2-202, T-V2-802 | Pasos 4, 8 | CA-V2-07 |
| RNF10 Disparadores por conjuntos | T-V2-107, T-V2-108 | Paso 5 | CA-V2-04 |
| RNF11 Atomicidad | T-V2-106, T-V2-108 | Paso 5 | CA-V2-13 |
| RNF12 Regresión de la v1 | T-V2-006, T-V2-803 | §3 | CA-V2-01, CA-V2-12 |
| RNF13 Interpretabilidad | T-V2-005, T-V2-805–807 | — (se comprueba hablando) | CA-V2-15 |
| RNF14 Resiliencia | T-V2-201, T-V2-701 | Paso 10 | — |
