# Tareas — Gestión Profesoral v3 (`8_tasks.md`)

> **Documento 8 de 8 del spec kit de la v3.** Cada tarea es un commit en `rama-nicolas-v3`.

## Fase 0 — Spec kit
- [x] T0.1 Leer `CONCEPTOS_CONTROL_DE_ACCESO.md`, `FLUJO_CONTROL_DE_ACCESO.md` y la spec v3 del profesor
- [x] T0.2 Escribir este spec kit y las enmiendas de la constitución

## Fase 1 — Base de datos
- [x] T1.1 `ruta`, `ruta_rol`, roles, rutas, permisos y usuarios con hash (`db/v3_control_acceso.sql`)
- [x] T1.2 Procedimientos del acceso: `sp_verificar_acceso_ruta`, `sp_rutas_permitidas`, `sp_credenciales_usuario`, `sp_roles_de_usuario`
- [x] T1.3 Procedimientos de la administración, con las protecciones de R15
- [x] T1.4 `db/init.sh` aplica el script en cada arranque

## Fase 2 — API: la sesión
- [x] T2.1 Paquetes BCrypt y JwtBearer; `Jwt__Key` desde `.env`
- [x] T2.2 `ServicioSesion` (bcrypt + token) y `POST /api/sesion`
- [x] T2.3 Autenticación JWT, mensajes 401 en castellano, Swagger con «Authorize»

## Fase 3 — API: el permiso
- [x] T3.1 `ExigePermisoAttribute` y `RepositorioAcceso`
- [x] T3.2 `[Authorize]` y `[ExigePermiso]` en los 16 controladores; `datos.modificar` en cada escritura
- [x] T3.3 `GET /api/permisos/mios`

## Fase 4 — API: administración
- [x] T4.1 Usuario (con hash), rol de usuario, rol, rutas y permisos de cada rol

## Fase 5 — Front
- [x] T5.1 Sesión en el servidor, `ManejadorToken`, `FiltroSesion`, `ManejadorAcceso`
- [x] T5.2 Iniciar sesión, salir y página de error 403/404
- [x] T5.3 Menú e inicio según las rutas; modo consulta
- [x] T5.4 Usuarios, roles y permisos por rol
- [x] T5.5 `POST /api/sesion` como en la guía del profesor

## Fase 6 — Pruebas
- [x] T6.1 Colección `postman/gestion_profesoral_v3.postman_collection.json`
- [ ] T6.2 Correr todo el quickstart con Docker y la colección en verde (Nicolás)

## Fase 7 — Entrega
- [ ] T7.1 `9_checklist.md` revisado por otro integrante
- [ ] T7.2 Subir `rama-nicolas-v3`, PR a `main`, revisión del equipo
- [ ] T7.3 Tag `v3` en `main`
- [ ] T7.4 Sustentación: sábado 21 de noviembre de 2026
