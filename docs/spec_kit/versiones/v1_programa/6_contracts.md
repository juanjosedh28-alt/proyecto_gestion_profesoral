# Contratos de integración — Gestión Profesoral

Documento 6 de 8 del spec kit raíz: cómo se hablan las piezas entre sí (puertos, hosts, variables, healthchecks y contratos de la API v1).

---

## 1. Mapa de puertos

| Servicio | Host interno (red compose) | Puerto publicado al PC |
|---|---|---|
| api-gestion | `api-gestion:8011` | 8011 (`/swagger`) |
| sqlserver | `sqlserver:1433` | 11443 |

Regla: entre contenedores siempre el host interno con puerto estándar; desde el PC siempre `localhost` con el puerto publicado.

---

## 2. Variables de entorno (el contrato de configuración)

### api-gestion

| Variable | Valor en compose |
|---|---|
| `ASPNETCORE_URLS` | `http://+:8011` |
| `ConnectionStrings__GestionLocal` | `Server=sqlserver,1433;Database=gestion_local;User Id=sa;Password=Paradigmas123!;TrustServerCertificate=True` |

### Motor

- sqlserver: `ACCEPT_EULA=Y`, `MSSQL_SA_PASSWORD`, `MSSQL_PID=Developer`.

---

## 3. Contratos de la API v1

Contratos de la entidad `programa` (base `gestion_local`):

- `GET /api/programa` -> 200 OK / 204 No Content
- `GET /api/programa/{id}` -> 200 OK / 404 Not Found
- `POST /api/programa` -> 200 OK / 422 Unprocessable Entity
- `PUT /api/programa/{id}` -> 200 OK / 422 Unprocessable Entity
- `PATCH /api/programa/{id}` -> 200 OK
- `DELETE /api/programa/{id}` -> 200 OK / 404 Not Found

---

## 4. Contrato API ↔ BD

- La base es `gestion_local` en SQL Server y la cadena de conexión viene de `ConnectionStrings__GestionLocal`.
- La conexión se abre de forma perezosa (primera petición): la API arranca aunque la BD aún esté inicializando; no hay `depends_on` api → BD.
- La base no tiene triggers ni procedimientos almacenados: las reglas de negocio viven en la API.
- El borrado es lógico (`activo = 0`) y todo `GET` filtra por `activo = 1`.

---

## 5. Healthchecks y arranque

| Servicio | Check | Parámetros |
|---|---|---|
| sqlserver | `sqlcmd -C -Q 'SELECT 1'` | cada 10 s, 20 reintentos, `start_period` 30 s |
| sqlserver-init | `depends_on: sqlserver: condition: service_healthy`; termina `Exited (0)` | `restart: "no"` |

---

## 6. Contratos de volúmenes y montajes

| Montaje | Para qué |
|---|---|
| `mssqldata` (nombrado) | Persistencia de datos; `down -v` = reset |
| `./db/sqlserver` → `/scripts` (ro, en sqlserver-init) | `init.sh` + `gestion_profesoral.sql` |
| `./api_gestion` → `/app` | Recarga en caliente |

---

## 7. Credenciales (fijas en todo el proyecto)

| Motor | BD | Usuario | Clave |
|---|---|---|---|
| SQL Server | `gestion_local` | `sa` | `Paradigmas123!` |