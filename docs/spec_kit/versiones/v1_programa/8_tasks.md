```markdown
# Tareas — Plan de fases y commits

Documento 8 de 8 del spec kit raíz: orden de construcción de la API v1. Cada fase termina en un commit y en algo verificable. Requisitos: [2_spec.md](2_spec.md) · decisiones: [3_plan.md](3_plan.md) · principios: [1_constitution.md](1_constitution.md) · BD: [5_data_model.md](5_data_model.md) · integración: [6_contracts.md](6_contracts.md) · verificación: [7_quickstart.md](7_quickstart.md).

---

## Fase 0 — Base de datos y esqueleto

- [x] Completada

- Crear el repositorio (`git init`, `.gitignore`, `README.md`) y las carpetas `db/sqlserver`, `docs/` y `api_gestion/`.
- Escribir `db/sqlserver/gestion_profesoral.sql` y `db/sqlserver/init.sh`.
- Crear el `docker-compose.yml` con los servicios `sqlserver` y `sqlserver-init` y el volumen `mssqldata`.

**Verificar:** `docker compose up -d` → `sqlserver-init` termina `Exited (0)`; en `gestion_local` hay 218 filas en `area_conocimiento` y `programa` está vacía.

---

## Fase 1 — Los modelos

- [ ] Pendiente

- Crear las entidades de las 7 tablas independientes en `Modelos/`, empezando por `Programa.cs`.
- Crear `Datos/GestionDbContext.cs` con el filtro `activo = 1`.

**Verificar:** el proyecto compila sin errores.

---

## Fase 2 — Las peticiones por verbo y la excepción

- [ ] Pendiente

- Crear los DTOs en `Peticiones/`: `ProgramaCrear` (POST y PUT) y `ProgramaActualizar` (PATCH).
- Crear la excepción de negocio que el servicio lanza y el controlador traduce a `404` o `422`.

**Verificar:** el proyecto compila y los DTOs rechazan los campos obligatorios vacíos.

---

## Fase 3 — Interfaces y repositorios SQL Server

- [ ] Pendiente

- Definir `IRepositorioPrograma` y `RepositorioProgramaSqlServer` con consultas parametrizadas.
- Agregar el borrado lógico (`activo = 0`).

**Verificar:** una consulta de prueba contra `gestion_local` devuelve la tabla vacía.

---

## Fase 4 — Servicios y prueba de capas

- [ ] Pendiente

- Definir `IServicioPrograma` y `ServicioPrograma` con las reglas de negocio (campos obligatorios, existencia del registro, `id` repetido).
- Probar la cadena Servicio → Repositorio sin pasar por HTTP.

**Verificar:** crear, consultar, actualizar y desactivar un programa desde la prueba de capas.

---

## Fase 5 — Controladores y Program.cs

- [ ] Pendiente

- Crear `ProgramaController` con los 6 endpoints de los contratos de [6_contracts.md](6_contracts.md).
- Registrar el `DbContext`, el repositorio y el servicio en `Program.cs`, y activar Swagger.
- Agregar el servicio `api-gestion` al `docker-compose.yml`.

**Verificar:** `docker compose up -d --build` y los 8 pasos de [7_quickstart.md](7_quickstart.md).