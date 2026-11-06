# Tareas — Plan de fases y commits (v2)

> Documento 8 de 8 del spec kit de la v2: orden de construcción de lo que la v2 añade.
> Requisitos: [2_spec.md](2_spec.md) · arquitectura: [3_plan.md](3_plan.md) · decisiones: [4_research.md](4_research.md) ·
> datos: [5_data_model.md](5_data_model.md) · contratos: [6_contracts.md](6_contracts.md) · verificación: [7_quickstart.md](7_quickstart.md).

**Cómo usar este documento**

- IDs `T-V2-xxx`, checkbox y requisitos que cubre.
- Cada fase termina en **un commit** y en algo **verificable** (paso del quickstart).
- **Definición de terminado:** compila; la verificación de la fase pasa; cada archivo nuevo tiene su comentario en
  español que dice qué hace **y por qué**; los documentos siguen coherentes con el código; commit hecho y casilla marcada.
- **Autoría:** cada integrante hace los commits de lo que construye desde **su rama** y debe poder explicarlo en
  voz alta. La columna «Rama» dice quién lo entregó.
- Una casilla `[x]` quiere decir que **el código está escrito y con commit**. La verificación con Docker se marca
  aparte en la Fase 8.

---

## Fase 0 — Spec kit y línea base

- [x] **T-V2-001** Carpeta `v2_gestion_profesoral/` con los documentos del kit (solo el delta).
- [x] **T-V2-002** `9_checklist.md` y `GUIA_IA2.md`.
- [x] **T-V2-003** Decisiones abiertas resueltas en `2_spec.md` §9 (Clarificaciones).
- [x] **T-V2-004** Plan alineado con el código real de la v1 (Dapper, middleware, carpetas): `4_research.md` D57–D64.
- [x] **T-V2-005** Prompts de cada integrante en la carpeta de la versión (`prompts_<integrante>.md`).
- [x] **T-V2-006** Línea base: la v1 no compilaba por un merge sin resolver; se resolvió (rama-nicolas, commit «Resuelve los conflictos de merge de la v1…»).

## Fase 1 — Base de datos (rama-nicolas)

- [x] **T-V2-101** `db/v2_procedimientos.sql`: cabecera, columna `total_estudios` idempotente. — RF12
- [x] **T-V2-102** 22 procedimientos de las tablas de la v1. — R9, CA-V2-03
- [x] **T-V2-103** 11 vistas `vw_<tabla>`. — D59
- [x] **T-V2-104** Procedimientos de `docente` (5), con R2 al retirar. — RF7, R2
- [x] **T-V2-105** Procedimientos del detalle directo (4 × 5) y sus 4 de lote. — RF8, RF11
- [x] **T-V2-106** Procedimientos de `apoyo_profesoral` y `beca` (2 × 5), con reactivación. — RF9, R3
- [x] **T-V2-107** Procedimientos de las puentes (5 + 5 + 4 + 4). — RF10, R3, R6
- [x] **T-V2-108** Disparadores `trg_contar_estudios` y `trg_sello_docente`. — RF12
- [x] **T-V2-109** `init.sh`: tablas solo la primera vez, script de la v2 siempre, con `-b -I -f 65001`.
- [x] **T-V2-110** Contraseña fuera del código: `.env`, `.env.example`, `.gitignore`. — Metodología §5

**Verificar:** quickstart §2 (79 procedimientos, 11 vistas, 2 disparadores) y `sqlserver-init` en `Exited (0)`.

## Fase 2 — Infraestructura de la API (rama-nicolas)

- [x] **T-V2-201** `Repositorios/EjecutorProcedimientos.cs` y su registro. — RNF8
- [x] **T-V2-202** `Middleware/ManejadorErroresSql.cs` registrado después del de la v1. — RNF9
- [x] **T-V2-203** `Servicios/Validaciones.cs` (fechas y tamaño del envío).

## Fase 3 — La v1 por procedimientos (rama-nicolas)

- [x] **T-V2-301** Repositorios de las 5 tablas de la v1 llaman procedimientos; los servicios dejan de preguntar si la clave existe. — R9
- [x] **T-V2-302** `red.url` limitada a 45 caracteres en API y front. — R10

## Fase 4 — API de las 11 tablas (rama-nicolas)

- [x] **T-V2-401** Modelo, petición, repositorio, servicio y controlador de `docente`. — RF7
- [x] **T-V2-402** Ídem del detalle directo, con rutas desde el maestro y envío múltiple. — RF8, RF11
- [x] **T-V2-403** Ídem de `apoyo_profesoral` y `beca`. — RF9
- [x] **T-V2-404** Ídem de las 4 puentes (sin `PUT` en las puras). — RF10
- [x] **T-V2-405** `Program.cs`: registros y nombre «renglón N» en los errores de listas.

**Verificar:** quickstart §3 (colección de Postman en verde).

## Fase 5 — Front (rama-nicolas)

- [x] **T-V2-501** Modelos y clientes de las 11 tablas; `GetOpcionalAsync` en `ClienteApiBase`. — RF14
- [x] **T-V2-502** `Docentes/Index`, `Crear`, `Editar` con el desplegable de línea y «(ninguna)». — RF14.1, RF14.5
- [x] **T-V2-503** `Docentes/Detalle`: siete secciones, varios renglones en un envío, desplegables con aviso cuando están vacíos. — RF14.2, RF14.4, RF14.6, RF14.9
- [x] **T-V2-504** Pantallas de edición del detalle (6). — RF14.8
- [x] **T-V2-505** `EstudiosRealizados/Detalle`: apoyo, beca y áreas. — RF14.3
- [x] **T-V2-506** Enlace «Docentes» en la barra y textos sin jerga. — RF14.7

## Fase 6 — Pruebas (rama-nicolas)

- [x] **T-V2-601** `postman/gestion_profesoral_v2.postman_collection.json` con las peticiones que deben fallar. — CA-V2-07, CA-V2-08

## Fase 7 — Entrega

- [x] **T-V2-701** Spec kit coherente con el código; enmiendas de la constitución.
- [ ] **T-V2-702** Cada integrante agrega su `prompts_<nombre>.md` y sus commits desde su rama.
- [ ] **T-V2-703** Pull Request de cada rama a `main`; el encargado revisa y hace el merge.

## Fase 8 — Verificación y cierre

- [ ] **T-V2-801** `docker compose up -d --build` en un equipo con Docker: los tres contenedores arriba y `sqlserver-init` en `Exited (0)`.
- [ ] **T-V2-802** Colección de Postman completa en verde.
- [ ] **T-V2-803** Los 12 criterios por la interfaz (quickstart §4), incluida la regresión de la v1.
- [ ] **T-V2-804** Tag `v2` sobre el commit de `main` que pasa todo.
