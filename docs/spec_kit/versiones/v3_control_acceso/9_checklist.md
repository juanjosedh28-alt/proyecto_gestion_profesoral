# Lista de chequeo de requisitos — v3 (compuerta 3)

> Cada requisito tiene que ser **medible, único y verificable**. **Las casillas las marca una persona del equipo**,
> revisando la parte que escribió otro: una IA puede ayudar a evaluar, pero no puede auto-aprobarse. Por eso esta
> lista se entrega **sin marcar**.

Revisó: ______________________  Fecha: ____/____/________

## Requisitos funcionales

| ✔ | Requisito | ¿Medible? | ¿Único? | ¿Cómo se verifica? |
|---|---|---|---|---|
| [ ] | RF15 Contraseña con bcrypt; vacía al editar = no cambia; nunca sale en JSON | Sí | Sí | Quickstart §3; colección carpetas 7 y 9 |
| [ ] | RF16 `POST /api/sesion`: token o el mismo 401 | Sí | Sí | Colección carpeta A |
| [ ] | RF17 Sin token o alterado → 401; sin permiso → 403 | Sí | Sí | Colección carpeta 7 |
| [ ] | RF18 El permiso lo decide `sp_verificar_acceso_ruta` en cada petición | Sí | Sí | Quickstart §5; colección carpeta 8 |
| [ ] | RF19 `GET /api/permisos/mios` arma el menú | Sí | Sí | Colección carpeta 7 |
| [ ] | RF20 Administración de usuarios, roles y permisos | Sí | Sí | Colección carpeta 9; quickstart §4 |
| [ ] | RF21 Iniciar y cerrar sesión en el front; sesión vencida → iniciar | Sí | Sí | Quickstart §4 pasos 1 y 7 |
| [ ] | RF22 Menú según el rol; modo consulta; 403 al escribir la dirección | Sí | Sí | Quickstart §4 pasos 2, 3 y 5 |
| [ ] | RF23 Pantallas de administración | Sí | Sí | Quickstart §4 paso 4 |

## Reglas de negocio

| ✔ | Regla | ¿Está en un solo sitio? | ¿Dónde se verifica? |
|---|---|---|---|
| [ ] | R11 Solo hash bcrypt | Sí (`ServicioUsuario`) | Quickstart §3 |
| [ ] | R12 Mismo mensaje en los dos fallos | Sí (`ServicioSesion`) | Colección carpeta A |
| [ ] | R13 Permiso en la base, en cada petición | Sí (`sp_verificar_acceso_ruta`) | Colección carpeta 8 |
| [ ] | R14 Escribir exige `datos.modificar` | Sí (atributo en cada escritura) | Colección carpeta 7 |
| [ ] | R15 Nunca sin administrador | Sí (procedimientos) | Colección carpeta 9 |
| [ ] | R16 No retirarse a sí mismo | Sí (`ServicioUsuario`) | Front: admin intenta retirarse |
| [ ] | R17 Retirado no entra | Sí (procedimientos del acceso) | Colección carpeta 9 |

## Criterios de aceptación del profesor

| ✔ | Criterio | ✔ | Criterio |
|---|---|---|---|
| [ ] | 1 Sin texto legible en la contraseña | [ ] | 6 Sin `JOIN` de permisos en C# |
| [ ] | 2 Token / mismo mensaje | [ ] | 7 Quitar permiso sin volver a entrar |
| [ ] | 3 Sin token → 401 | [ ] | 8 Menú según el rol |
| [ ] | 4 Token alterado → 401 | [ ] | 9 Dirección a mano → 403 |
| [ ] | 5 Sin permiso → 403, y se explica | [ ] | 10 Regresión v1 y v2 con token |
