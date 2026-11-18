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

## Fase 8 — Rehacer la v3 sobre la v2 corregida (17 de noviembre)
- [x] T8.1 Rama nueva `rama-nicolas-v3` desde `rama-nicolas-v2-correcciones`, con los commits de la v3 llevados uno por uno
- [x] T8.2 Menú y página de inicio: las 16 tablas, cada entrada según el rol (D81)
- [x] T8.3 Las escrituras de `area_conocimiento` exigen `datos.modificar`
- [x] T8.4 `POST /api/login` como alias de `POST /api/sesion` (D80)
- [x] T8.5 Colección v3 regenerada desde la colección v2 corregida (carpetas 0 a 8 de regresión, 9 a 11 de acceso)
- [ ] T8.6 Correr la colección y el quickstart con Docker (Nicolás)


## Fase 9 — Revisión final contra la lista del profesor (17 de noviembre)
- [x] T9.1 `ClockSkew = TimeSpan.Zero`: un token vencido ya no entra (tropiezo 1, D82)
- [x] T9.2 Con la API apagada el front avisa y no se cae: `FiltroSesion` y `UseExceptionHandler` (D83)
- [x] T9.3 La página 403 muestra el aviso de la API, el permiso que faltó y la dirección, sin datos (D84)
- [x] T9.4 «Nuevo rol» le pregunta a la API al abrirse: `/Roles/Crear` a mano también da 403 (D85)
- [x] T9.5 Inicio de sesión: `Url.IsLocalUrl` y sesión anterior descartada (D86)
- [x] T9.6 La ruta `interfaz.areas` ya no dice «(consulta)» (también en una base ya sembrada)
- [x] T9.7 Front compilado con .NET 10 (0 advertencias, 0 errores) y probado contra una API simulada: 401, 403 al
      escribir la dirección, criterio 7 desde «Permisos por rol», modo consulta, sesión vencida y API apagada
- [x] T9.8 Spec kit: los cinco tropiezos, el `DelegatingHandler`, lo que queda fuera (`3_plan.md` §7 a §9); direcciones
      abiertas y matriz de roles (`6_contracts.md`); la lista A a G del profesor (`9_checklist.md`); «Si algo falla»
      (`7_quickstart.md` §7)
- [ ] T9.9 Docker + colección v3 completa + quickstart §4 en el navegador (Nicolás)
