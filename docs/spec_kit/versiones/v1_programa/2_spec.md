# Especificación Funcional — Gestión Profesoral (`2_spec.md`)

> **Documento 2 de 8 del spec kit raíz.**
> Orden de lectura: `1_constitution` → `2_spec` → `3_plan` → `4_research` → `5_data_model` → `6_contracts` → `7_quickstart` → `8_tasks`.

Este documento define **QUÉ** servicios y requisitos funcionales componen la primera entrega del proyecto, adaptando la arquitectura de referencia a los componentes específicos de la base de datos `gestion_local` en **ASP.NET Core (.NET 10)** con **Entity Framework Core** (`DbContext`).

---

## Propósito

Orquestar una arquitectura de backend en capas estricta (**Controllers → Services → Repositories**) para exponer la API REST orientada a administrar las entidades independientes sin relaciones de la base de datos `gestion_local`, garantizando simplicidad, tipado estricto y un enfoque 100% didáctico en español.

---

## Alcance de la Versión 1 (Entidades Independientes)

Para esta primera entrega, el sistema implementa los endpoints de API exclusivamente para las tablas de la base de datos que no poseen dependencias de claves foráneas (entidades raíz):

- `area_conocimiento`: catálogo base precargado con 218 registros.
- `termino_clave`: términos clave y sus traducciones.
- `linea_investigacion`: líneas de investigación institucionales.
- `programa`: programas académicos, iniciando vacíos según el script DDL.
- `red`: redes académicas e investigativas.
- `rol`: roles del sistema de seguridad.
- `usuario`: usuarios del sistema.

---

## Requisitos Funcionales por Entidad

Cada servicio expone operaciones CRUD estándar bajo las reglas de la constitución: borrado lógico mediante `activo = 0`, uso de DTOs en controladores y manejo de consultas mediante el `DbContext`.

### RF1 — Área de Conocimiento (`/api/area-conocimiento`)

- `GET /api/area-conocimiento`: retorna la lista completa de áreas activas (`activo = 1`).
- `GET /api/area-conocimiento/{id}`: retorna el detalle de un área específica por su código alfanumérico (ej. `1A01`).

> **Restricción:** al ser una tabla de referencia precargada con datos oficiales, las operaciones de escritura (`POST`/`PUT`/`DELETE`) se limitan exclusivamente a la gestión lógica interna del sistema.

### RF2 — Término Clave (`/api/termino-clave`)

- `GET /api/termino-clave`: lista todos los términos clave activos.
- `POST /api/termino-clave`: crea un nuevo término clave.
- `PUT /api/termino-clave/{termino}`: actualiza la información del término.
- `DELETE /api/termino-clave/{termino}`: realiza borrado lógico (`activo = 0`).

### RF3 — Línea de Investigación (`/api/linea-investigacion`)

- `GET /api/linea-investigacion`: retorna el listado de líneas de investigación.
- `POST /api/linea-investigacion`: registra una nueva línea (con ID auto-incremental).
- `PUT /api/linea-investigacion/{id}`: actualiza completamente la línea.
- `DELETE /api/linea-investigacion/{id}`: desactivación lógica (`activo = 0`).

### RF4 — Programa (`/api/programa`)

- `GET /api/programa`: retorna la lista de programas (inicia vacía, retornando estado HTTP `204 No Content` si no hay registros).
- `POST /api/programa`: crea un nuevo programa académico validando la obligatoriedad de campos.
- `PUT /api/programa/{id}`: reemplazo completo de los datos del programa.
- `PATCH /api/programa/{id}`: actualización parcial de campos opcionales, retornando estado `200 OK`.
- `DELETE /api/programa/{id}`: borrado lógico asignando `activo = 0`.

### RF5 — Red (`/api/red`)

- `GET /api/red`: lista todas las redes docentes y científicas registradas.
- `POST /api/red`: crea un nuevo registro de red.
- `PUT /api/red/{idr}`: actualiza los datos de la red.
- `DELETE /api/red/{idr}`: ejecuta el borrado lógico (`activo = 0`).

### RF6 — Roles y Usuarios (`/api/rol`, `/api/usuario`)

- `GET /api/rol` y `GET /api/usuario`: listados generales de roles y usuarios del sistema.
- `POST /api/rol` y `POST /api/usuario`: creación de entidades aplicando hashing seguro de contraseñas para los usuarios (según el Artículo 8 de la constitución).
- `DELETE /api/rol/{id}` y `DELETE /api/usuario/{id}`: borrado lógico institucional.

---

## Requisitos No Funcionales

### RNF1 — Arquitectura en Capas

Los controladores (`Controllers`) interactúan únicamente con la capa de servicios (`Services`), la cual maneja las reglas de negocio y delega la persistencia al `DbContext` en los repositorios (`Repositories`). Queda prohibido mezclar lógica HTTP con acceso a datos.

### RNF2 — Manejo de Errores y Respuestas

Todas las respuestas de la API devuelven contratos estructurados en formato JSON y utilizan códigos de estado HTTP estándar (`200`, `201`, `204`, `400`, `404`, `422`, `500`). Los mensajes de error se expresan claramente en español.

### RNF3 — Filtrado por Defecto

Cualquier consulta general (`GET`) ejecutada a través del `DbContext` debe filtrar implícitamente de manera que solo se retornen registros con `activo = 1`, salvo solicitudes administrativas explícitas.

### RNF4 — Documentación Automática

La API expone la interfaz interactiva de **Swagger / OpenAPI** en la ruta `/swagger` para facilitar la verificación y pruebas locales en el entorno de desarrollo.

---

## Criterios de Aceptación

- Al ejecutar el entorno mediante Docker Compose, la API levanta correctamente conectándose a la base de datos `gestion_local` en SQL Server.
- El endpoint `GET /api/area-conocimiento` retorna exitosamente los 218 registros oficiales precargados en el script de inicialización.
- El endpoint `GET /api/programa` responde con código `204 No Content` al encontrarse la tabla vacía en la versión inicial.
- Las operaciones de borrado (`DELETE`) sobre cualquiera de las entidades actualizan la columna `activo` a `0` en la base de datos sin eliminar físicamente las filas.