# Tareas — Plan de fases y commits (v1)

> Documento 8 de 8 del spec kit raíz: orden de construcción del sistema completo (BD + API + front).
> Requisitos: [2_spec.md](2_spec.md) · arquitectura: [3_plan.md](3_plan.md) · decisiones: [4_research.md](4_research.md) · datos: [5_data_model.md](5_data_model.md) · contratos: [6_contracts.md](6_contracts.md) · verificación: [7_quickstart.md](7_quickstart.md) · principios: [1_constitution.md](1_constitution.md).

**Cómo usar este documento**

- Cada tarea tiene ID (`T-xxx`), checkbox y los requisitos que cubre (`RF`/`RNF`).
- Cada fase termina en **un commit** (mensaje sugerido) y en algo **verificable**.
- **Definición de terminado** (Art. 11): compila sin errores, la verificación de la fase pasa, cada archivo `.cs` tiene su comentario XML en español, los documentos siguen coherentes con el código, commit hecho y checkbox marcado.
- Patrón: `programa` se construye completa primero (API en la Fase 4, front en la Fase 8); las otras entidades **replican** ese patrón.

---

## Fase 0 — Base de datos y esqueleto

- [x] **T-001** Crear el repositorio (`git init`, `.gitignore` con `bin/ obj/ .vs/`, `README.md`) y las carpetas `db/sqlserver`, `docs/`, `api_gestion/`.
- [x] **T-002** Escribir `db/sqlserver/gestion_profesoral.sql` y `db/sqlserver/init.sh`.
- [x] **T-003** Crear `docker-compose.yml` con `sqlserver`, `sqlserver-init` y el volumen `mssqldata`.
- [ ] **T-004** Copiar los 8 documentos del kit en `docs/` y crear las carpetas `front_razor/` y `pruebas/`.
- [ ] **T-005** Ajustar el script según `5_data_model.md` §2: `red.url` a `VARCHAR(200)`; confirmar que `programa` no declara ninguna clave foránea. Aplicar con `docker compose down -v` + `up -d`.

**Cubre:** RNF3 (base). **Verificar:** quickstart paso 2 (`areas = 218`, `programas = 0`); `sqlserver-init` en `Exited (0)`; en la BD `red.url` es `VARCHAR(200)`.
**Commit:** `fase 0: base de datos, compose inicial y ajustes de esquema`

---

## Fase 1 — Esqueleto de la API y Docker

- [ ] **T-101** Crear el proyecto `api_gestion` (`dotnet new webapi --use-controllers`, .NET 10), borrar el controlador de ejemplo y agregar los paquetes `Microsoft.EntityFrameworkCore.SqlServer`, `Swashbuckle.AspNetCore`.
- [ ] **T-102** Crear `api_gestion/Dockerfile` (plan §7.1) y `appsettings.json` **sin** cadena de conexión.
- [ ] **T-103** `Program.cs` mínimo: controladores con `SnakeCaseLower`, Swagger solo en `Development`, sin `UseHttpsRedirection` (plan §5, sin DbContext todavía).
- [ ] **T-104** Agregar el servicio `api-gestion` al `docker-compose.yml` (variables, puerto `8074`, volumen, `dotnet watch`).

**Cubre:** RNF4, RNF5. **Verificar:** `docker compose up -d --build` y `http://localhost:8074/swagger` abre (aunque aún sin endpoints).
**Commit:** `fase 1: esqueleto de la API con Swagger en Docker`

---

## Fase 2 — Modelos y DbContext

- [ ] **T-201** Crear las 5 entidades en `Modelos/` (`Programa.cs` primero; luego `AreaConocimiento`, `TerminoClave`, `LineaInvestigacion`, `Red`). Sin anotaciones; columnas `NULL` como `string?`.
- [ ] **T-202** Crear `Datos/GestionDbContext.cs` con el mapeo fluido de las 5 tablas siguiendo las 6 reglas de `3_plan.md` §3 (claves, `IDENTITY`, `Activo` sin default, `HasQueryFilter`, longitudes).
- [ ] **T-203** Registrar `AddDbContext` con `UseSqlServer` + `EnableRetryOnFailure` en `Program.cs` y agregar `ConnectionStrings__GestionLocal` al compose.

**Cubre:** RNF3, RNF5, RNF7. **Verificar:** el proyecto compila; la API sigue arrancando; revisar que las longitudes de `OnModelCreating` coinciden con el `.sql`.
**Commit:** `fase 2: modelos y GestionDbContext con filtro de borrado logico`

---

## Fase 3 — Peticiones, respuestas y manejo de errores

- [ ] **T-301** Crear los DTOs de entrada en `Peticiones/` con las anotaciones de `5_data_model.md` §3: `ProgramaCrear` (POST y PUT), `ProgramaActualizar` (PATCH, todo opcional), `TerminoClaveCrear`, `LineaInvestigacionCrear`, `RedCrear`.
- [ ] **T-302** Crear los DTOs de salida en `Respuestas/` (`AreaConocimientoRespuesta`, `TerminoClaveRespuesta`, `LineaInvestigacionRespuesta`, `ProgramaRespuesta`, `RedRespuesta`).
- [ ] **T-303** Crear `Excepciones/` (`ExcepcionNegocio`, `ExcepcionNoEncontrada`, `ExcepcionConflicto`, `ExcepcionValidacion`).
- [ ] **T-304** Crear `Infraestructura/RespuestaError.cs`, `FabricaErrores.cs` (`422` en lugar de `400`) y `ManejadorExcepciones.cs` (`404`/`409`/`422`/`500`, `DbUpdateException` por único → `409`).
- [ ] **T-305** Registrar en `Program.cs`: `InvalidModelStateResponseFactory`, `AddProblemDetails`, `AddExceptionHandler`, `UseExceptionHandler`.

**Cubre:** RNF2, RNF6. **Verificar:** compila; los DTOs rechazan vacíos y longitudes excedidas (se comprueba de verdad en la Fase 4).
**Commit:** `fase 3: DTOs de entrada y respuesta, excepciones y formato de error unico`

---

## Fase 4 — Vertical completa de `programa` (API)

- [ ] **T-401** `Repositorios/RepositorioProgramaSqlServer.cs` con `IRepositorioPrograma` (`ListarAsync`, `ObtenerPorIdAsync`, `ExisteIdAsync` con `IgnoreQueryFilters`, `AgregarAsync`, `GuardarCambiosAsync`). Solo LINQ.
- [ ] **T-402** `Servicios/ServicioPrograma.cs` con `IServicioPrograma`: `Trim`, validación de fechas con `TryParseExact`, `fecha_cierre ≥ fecha_creacion`, `id` de ruta = `id` de cuerpo (`422`), duplicado (`409`), inexistente (`404`), `PATCH` vacío (`422`), `Activo = true` al crear, `FechaActualizacion` en POST/PUT/PATCH, borrado lógico.
- [ ] **T-403** `Controllers/ProgramaController.cs` con los 6 endpoints (`201` con `CreatedAtAction`, `204` en `DELETE`).
- [ ] **T-404** Registrar repositorio y servicio de `programa` en `Program.cs`.

**Cubre:** RF4, RNF1, RNF2, RNF3. **Verificar:** quickstart paso 4 completo (CA-03, CA-04, CA-05 parcial, CA-06).
**Commit:** `fase 4: API de programa de punta a punta (CRUD + PATCH)`

---

## Fase 5 — Replicar las otras 4 entidades (API)

Cada tarea repite el patrón de la Fase 4 (repositorio → servicio → controlador → registro en `Program.cs`).

- [ ] **T-501** `area_conocimiento` — RF1: solo `GET` lista y `GET {id}`.
- [ ] **T-502** `termino_clave` — RF2: CRUD con clave textual; `PUT` valida `termino` de ruta vs cuerpo.
- [ ] **T-503** `linea_investigacion` — RF3: CRUD, `id` generado por la BD (el DTO no trae `id`).
- [ ] **T-504** `red` — RF5: CRUD con `idr` enviado por el cliente.

**Cubre:** RF1, RF2, RF3, RF5. **Verificar:** quickstart pasos 3 y 5 (CA-02, CA-05) y paso 6 (CA-07).
**Commit (uno por tarea es recomendable):** `fase 5: API de <entidad>`

---

## Fase 6 — Pruebas de capas

- [ ] **T-601** Crear `pruebas/api_gestion.Pruebas` (xUnit) referenciando `api_gestion`, con `Microsoft.EntityFrameworkCore.InMemory`. Una clase auxiliar crea un `GestionDbContext` InMemory limpio por prueba.
- [ ] **T-602** Pruebas de `ServicioPrograma` (Servicio → Repositorio, sin HTTP): crear, consultar, `id` duplicado (`ExcepcionConflicto`), inexistente (`ExcepcionNoEncontrada`), `PUT` con `id` distinto (`ExcepcionValidacion`), `PATCH` solo cambia lo enviado, desactivar deja `Activo = false` y desaparece del listado.
- [ ] **T-603** Pruebas de duplicado contra un registro **inactivo** (→ `ExcepcionConflicto`) en `ServicioTerminoClave` y `ServicioRed`, y de `ServicioLineaInvestigacion` (el `id` lo genera la BD, el servicio no lo asigna).

**Cubre:** RNF1, RNF3, CA-11. **Verificar:** quickstart paso 9.
**Commit:** `fase 6: pruebas de capas con xUnit y EF InMemory`

---

## Fase 7 — Esqueleto del front (Razor Pages)

- [ ] **T-701** Crear el proyecto `front_razor` (`dotnet new webapp`, .NET 10) con su `Dockerfile` (puerto `8010`).
- [ ] **T-702** `Infraestructura/RespuestaError.cs` y `ErrorApiException.cs`; `Clientes/ClienteApiBase.cs` con `SnakeCaseLower`, manejo de `HttpRequestException` → `503` y lectura del formato de error.
- [ ] **T-703** `Program.cs` del front: `AddRazorPages`, lectura de `ApiGestion:BaseUrl` y (se irán agregando) los `AddHttpClient` tipados; sin `UseHttpsRedirection`.
- [ ] **T-704** `Pages/Shared/_Layout.cshtml` con barra de navegación de las 5 entidades y alertas de `TempData["Exito"]`/`["Error"]`; `Pages/Index.cshtml` de inicio.
- [ ] **T-705** Agregar el servicio `front-razor` al `docker-compose.yml` (puerto `8010`, `ApiGestion__BaseUrl`, volumen, `dotnet watch`).

**Cubre:** RF6.1, RF6.6. **Verificar:** `docker compose up -d --build` y `http://localhost:8010` muestra el inicio con la barra de navegación.
**Commit:** `fase 7: esqueleto del front Razor Pages con layout y cliente base`

---

## Fase 8 — Vertical completa de `programa` (front)

- [ ] **T-801** `Modelos/ProgramaVista.cs` y `Modelos/ProgramaFormulario.cs` (mismas anotaciones que `ProgramaCrear`).
- [ ] **T-802** `Clientes/ClientePrograma.cs` con `IClientePrograma` (`ListarAsync`, `ObtenerAsync`, `CrearAsync`, `ReemplazarAsync`, `EliminarAsync`) y su `AddHttpClient`.
- [ ] **T-803** `Pages/Programas/Index` (tabla, mensaje de vacío, handler `OnPostEliminarAsync` con confirmación).
- [ ] **T-804** `Pages/Programas/Crear` (campos con `type="date"`, errores `409`/`422` mostrados en el formulario).
- [ ] **T-805** `Pages/Programas/Editar/{id}` (`id` de solo lectura, redirección con aviso si da `404`).

**Cubre:** RF6.2–RF6.6, RF6.8, CA-09. **Verificar:** quickstart paso 7, filas de Programas y la prueba de API caída.
**Commit:** `fase 8: CRUD de programas en el front`

---

## Fase 9 — Replicar las otras 4 entidades (front)

Cada tarea repite el patrón de la Fase 8 (modelos → cliente → `Index`/`Crear`/`Editar`) y agrega el `AddHttpClient`.

- [ ] **T-901** `AreasConocimiento` — RF6.7: `Index` con filtro `?buscar=` (en memoria) y `Detalle`; sin escritura.
- [ ] **T-902** `TerminosClave` — términos con tildes/espacios (`Uri.EscapeDataString`; término por query string).
- [ ] **T-903** `LineasInvestigacion`.
- [ ] **T-904** `Redes` — `idr` lo escribe el usuario; validación de URL.

**Cubre:** RF6. **Verificar:** quickstart paso 7 completo (CA-08, CA-09).
**Commit (uno por tarea es recomendable):** `fase 9: front de <entidad>`

---

## Fase 10 — Verificación final y entrega

- [ ] **T-1001** Ejecutar `docker compose down -v` y `docker compose up -d --build` desde cero y completar los 9 pasos de [7_quickstart.md](7_quickstart.md).
- [ ] **T-1002** Recorrer los criterios CA-01 a CA-11 de `2_spec.md` y marcarlos.
- [ ] **T-1003** Completar `README.md` (qué es, requisitos = Docker Desktop, comandos, URLs, credenciales públicas, cómo resetear).
- [ ] **T-1004** Revisar que ningún archivo `.cs` quede sin comentario XML, que no haya `FromSqlRaw` con concatenación y que los controladores solo devuelvan DTOs de `Respuestas/`.
- [ ] **T-1005** Confirmar que los 8 documentos de `docs/` coinciden con lo construido (puertos, rutas, códigos, nombres de carpetas).

**Cubre:** todos los RF/RNF. **Commit:** `fase 10: verificacion final y README` y etiqueta `v1.0`.

---

## Matriz de trazabilidad

| Requisito | Tareas | Verificación (quickstart) | Criterio |
|---|---|---|---|
| RF1 Áreas | T-501, T-901 | Paso 3, 7 | CA-02, CA-08 |
| RF2 Términos clave | T-502, T-902 | Paso 5, 7 | CA-05, CA-08 |
| RF3 Líneas | T-503, T-903 | Paso 5, 7 | CA-05, CA-08 |
| RF4 Programa | T-401–T-404, T-801–T-805 | Paso 4, 7 | CA-03, CA-04, CA-05, CA-06, CA-08 |
| RF5 Red | T-504, T-904 | Paso 5, 7 | CA-05, CA-08 |
| RF6 Front | T-701–T-705, T-801–T-904 | Paso 7 | CA-08, CA-09 |
| RNF1 Capas | T-401–T-403, T-601–T-603 | Paso 9 | CA-11 |
| RNF2 Errores/códigos | T-301–T-305 | Pasos 3–5 | CA-04, CA-06 |
| RNF3 Filtro `activo` | T-202, T-401 | Paso 4 | CA-03, CA-05 |
| RNF4 Swagger | T-103 | Paso 6 | CA-07 |
| RNF5 Nombres/JSON | T-103, T-201, T-202 | Pasos 3–5 | — |
| RNF6 Seguridad | T-302 | Pasos 3–5 | — |
| RNF7 Resiliencia | T-203, T-702 | Pasos 1, 7 | CA-01 |
| Persistencia | T-003 | Paso 8 | CA-10 |
