# Contratos HTTP — Gestión Profesoral v3 (`6_contracts.md`)

> **Documento 6 de 8 del spec kit de la v3.** Rutas nuevas y lo que cambia en las existentes.
> JSON en `snake_case` y errores `{ mensaje, detalle }`, como en la v1 y la v2.

## 1. Lo que cambia en TODAS las rutas existentes

Cada petición lleva la cabecera `Authorization: Bearer <token>`.

| Respuesta nueva | Cuándo |
|---|---|
| `401` `{ "mensaje": "Debe iniciar sesión: la petición no trae un token válido.", "detalle": [] }` | Sin token, token alterado o que no es un JWT |
| `401` `{ "mensaje": "La sesión venció. Inicie sesión de nuevo.", "detalle": [] }` | Token vencido |
| `403` `{ "mensaje": "Su rol no tiene permiso para esta operación.", "detalle": ["permiso requerido: interfaz.usuarios"] }` | Token válido, rol sin la ruta |

## 2. Sesión

### `POST /api/sesion` — sin token

```json
{ "email": "admin@gestion.edu.co", "contrasena": "admin123" }
```

| | Cuerpo |
|---|---|
| `200` | `{ "token": "eyJhbGciOi…", "email": "admin@gestion.edu.co", "nombre_completo": "Administrador del sistema", "roles": ["Administrador"], "expira": "2026-11-21T15:00:00Z" }` |
| `401` | `{ "mensaje": "El correo o la contraseña no son correctos.", "detalle": [] }` |
| `422` | faltó el correo o la contraseña |

El token decodificado (cualquiera lo puede leer en jwt.io: está **firmado, no cifrado**):

```json
{ "sub": "admin@gestion.edu.co", "name": "admin@gestion.edu.co", "role": "Administrador",
  "iss": "ApiGestionProfesoral", "aud": "FrontGestionProfesoral", "exp": 1795…, "iat": 1795… }
```

### `GET /api/permisos/mios` — solo token

`200` `{ "email": "consulta@gestion.edu.co", "roles": ["Consulta"], "rutas": ["interfaz.areas", "interfaz.docentes", …] }`

## 3. Administración

| Método y ruta | Permiso | Cuerpo | Respuesta |
|---|---|---|---|
| `GET /api/usuario` | `interfaz.usuarios` | — | `200` `[ { id, username, email, nombre_completo, fecha_creacion, fecha_actualizacion, roles } ]` (sin `password`) |
| `GET /api/usuario/{id}` | `interfaz.usuarios` | — | `200` / `404` |
| `GET /api/usuario/{id}/roles` | `interfaz.usuarios` | — | `200` `[ { usuario_id, rol_id, rol_nombre } ]` |
| `POST /api/usuario` | `interfaz.usuarios` | `{ username, email, nombre_completo, contrasena, rol_ids: [1] }` | `201` / `409` repetido / `422` |
| `PUT /api/usuario/{id}` | `interfaz.usuarios` | igual; `contrasena: ""` = no cambiarla | `200` / `404` / `409` / `422` |
| `DELETE /api/usuario/{id}` | `interfaz.usuarios` | — | `204` / `409` (último administrador o uno mismo) |
| `POST /api/rol-usuario` | `interfaz.usuarios` | `{ usuario_id, rol_id }` | `201` / `409` |
| `DELETE /api/rol-usuario/{usuarioId}/{rolId}` | `interfaz.usuarios` | — | `204` / `409` (único rol, último administrador) |
| `GET /api/rol` | `interfaz.roles` | — | `200` `[ { id, nombre, descripcion, total_usuarios } ]` |
| `GET /api/rol/{id}` | `interfaz.roles` | — | `200` / `404` |
| `POST /api/rol` | `interfaz.roles` | `{ nombre, descripcion }` | `201` / `409` |
| `PUT /api/rol/{id}` | `interfaz.roles` | `{ nombre, descripcion }` | `200` / `409` (renombrar Administrador) |
| `DELETE /api/rol/{id}` | `interfaz.roles` | — | `204` / `409` (Administrador o con usuarios activos) |
| `GET /api/ruta` | `interfaz.permisos` | — | `200` `[ { id, nombre, descripcion } ]` |
| `GET /api/ruta-rol/{rolId}` | `interfaz.permisos` | — | `200` `[ { ruta_id, ruta_nombre, descripcion, rol_id, asignada } ]` |
| `POST /api/ruta-rol` | `interfaz.permisos` | `{ ruta_id, rol_id }` | `201` / `409` |
| `DELETE /api/ruta-rol/{rutaId}/{rolId}` | `interfaz.permisos` | — | `204` / `409` (administración del Administrador) |

## 4. Permiso de cada ruta existente

| Rutas | Para leer (GET) | Para escribir (POST, PUT, DELETE) |
|---|---|---|
| `/api/area-conocimiento` | `interfaz.areas` | `datos.modificar` |
| `/api/termino-clave` | `interfaz.terminos` | `datos.modificar` |
| `/api/linea-investigacion` | `interfaz.lineas` | `datos.modificar` |
| `/api/programa` | `interfaz.programas` | `datos.modificar` |
| `/api/red` | `interfaz.redes` | `datos.modificar` |
| `/api/docente` y las 10 de su detalle | `interfaz.docentes` | `datos.modificar` |

## 5. Swagger

`http://localhost:8074` trae el botón **Authorize**: se pega **solo** el token (sin la palabra `Bearer`).
