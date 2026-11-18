# Lista de chequeo de requisitos — v3 (compuerta 3)

> Cada requisito tiene que ser **medible, único y verificable**. **Las casillas las marca una persona del equipo**,
> revisando la parte que escribió otro: una IA puede ayudar a evaluar, pero no puede auto-aprobarse. Por eso esta
> lista se entrega **sin marcar**.

Revisó: ______________________  Fecha: ____/____/________

## Requisitos funcionales

| ✔ | Requisito | ¿Medible? | ¿Único? | ¿Cómo se verifica? |
|---|---|---|---|---|
| [ ] | RF15 Contraseña con bcrypt; vacía al editar = no cambia; nunca sale en JSON | Sí | Sí | Quickstart §3; colección carpetas 9 y 11 |
| [ ] | RF16 `POST /api/sesion`: token o el mismo 401 | Sí | Sí | Colección carpeta A |
| [ ] | RF17 Sin token o alterado → 401; sin permiso → 403 | Sí | Sí | Colección carpeta 9 |
| [ ] | RF18 El permiso lo decide `sp_verificar_acceso_ruta` en cada petición | Sí | Sí | Quickstart §5; colección carpeta 10 |
| [ ] | RF19 `GET /api/permisos/mios` arma el menú | Sí | Sí | Colección carpeta 9 |
| [ ] | RF20 Administración de usuarios, roles y permisos | Sí | Sí | Colección carpeta 11; quickstart §4 |
| [ ] | RF21 Iniciar y cerrar sesión en el front; sesión vencida → iniciar | Sí | Sí | Quickstart §4 pasos 1 y 7 |
| [ ] | RF22 Menú según el rol; modo consulta; 403 al escribir la dirección | Sí | Sí | Quickstart §4 pasos 2, 3 y 5 |
| [ ] | RF23 Pantallas de administración | Sí | Sí | Quickstart §4 paso 4 |

## Reglas de negocio

| ✔ | Regla | ¿Está en un solo sitio? | ¿Dónde se verifica? |
|---|---|---|---|
| [ ] | R11 Solo hash bcrypt | Sí (`ServicioUsuario`) | Quickstart §3 |
| [ ] | R12 Mismo mensaje en los dos fallos | Sí (`ServicioSesion`) | Colección carpeta A |
| [ ] | R13 Permiso en la base, en cada petición | Sí (`sp_verificar_acceso_ruta`) | Colección carpeta 10 |
| [ ] | R14 Escribir exige `datos.modificar` | Sí (atributo en cada escritura) | Colección carpeta 9 |
| [ ] | R15 Nunca sin administrador | Sí (procedimientos) | Colección carpeta 11 |
| [ ] | R16 No retirarse a sí mismo | Sí (`ServicioUsuario`) | Front: admin intenta retirarse |
| [ ] | R17 Retirado no entra | Sí (procedimientos del acceso) | Colección carpeta 11 |

## Criterios de aceptación del profesor

| ✔ | Criterio | ✔ | Criterio |
|---|---|---|---|
| [ ] | 1 Sin texto legible en la contraseña | [ ] | 6 Sin `JOIN` de permisos en C# |
| [ ] | 2 Token / mismo mensaje | [ ] | 7 Quitar permiso sin volver a entrar |
| [ ] | 3 Sin token → 401 | [ ] | 8 Menú según el rol |
| [ ] | 4 Token alterado → 401 | [ ] | 9 Dirección a mano → 403 |
| [ ] | 5 Sin permiso → 403, y se explica | [ ] | 10 Regresión v1 y v2 con token |

---

## La lista del profesor (A a G), aplicada a este proyecto

> Son las casillas del `9_checklist.md` de la v3 del profesor. Donde nuestro proyecto es distinto (Razor Pages en vez
> de Blazor, SQL Server, dos tablas nuevas), la casilla dice dónde está escrita la diferencia.

### A. La especificación está completa

- [ ] No queda ningún `[NECESITA ACLARACIÓN: …]` en `2_spec.md`.
- [ ] Los **diez** criterios son verificables: cada uno dice qué hacer y qué tiene que pasar (`2_spec.md` §7).
- [ ] Está escrito **cuál de los diez no se puede simular** (el 9) y por qué (`2_spec.md`, al final de §7).

### B. Los conceptos están entendidos, no solo leídos

- [ ] Se puede explicar la diferencia entre **401 y 403** sin mirar.
- [ ] Se puede explicar por qué **SHA-256 no sirve** para contraseñas: es **demasiado rápido** (D71).
- [ ] Se puede explicar qué es el **salt** y qué problema resuelve.
- [ ] Se puede explicar que un JWT está **firmado y NO cifrado**, y qué significa para lo que lleva adentro.
- [ ] Se puede explicar por qué **los permisos no van en el token** (D73).
- [ ] Se puede explicar por qué **esconder una entrada del menú no protege nada**.

### C. El alcance está cerrado

- [ ] Está escrito que la v3 **agrega dos tablas**, `ruta` y `ruta_rol`, y por qué: el módulo no traía tabla de
      permisos (D68). En el proyecto del profesor ya existían; aquí no.
- [ ] Está escrito que el CRUD de `usuario`, `rol` y `rol_usuario` llega en esta versión porque el mapa de versiones
      del módulo lo pone aquí (`2_spec.md` §1).
- [ ] Está escrito que **un token no se puede revocar**, y que por eso dura 60 minutos (`3_plan.md` §9).
- [ ] Está escrito qué queda fuera: refrescar el token, recuperar la contraseña, segundo factor, permisos por
      operación (`3_plan.md` §9).

### D. El contrato es exacto

- [ ] `6_contracts.md` nombra `POST /api/sesion` con su cuerpo (y el alias `/api/login`, D80).
- [ ] Está dicho que el **401 responde lo mismo** para el correo inexistente y la contraseña equivocada, y por qué (D75).
- [ ] Están nombradas **las dos** direcciones abiertas, y solo dos: `GET /` (Swagger) y `POST /api/sesion`
      (`6_contracts.md` §1).
- [ ] Está la **matriz de los tres roles** contra los recursos (`6_contracts.md` §4.1).
- [ ] Están el **cuerpo** del 401 y el del 403 (`6_contracts.md` §1).

### E. El plan no anticipa, y nombra los tropiezos

- [ ] Los paquetes nuevos son `Microsoft.AspNetCore.Authentication.JwtBearer` **10.0.0** (la que corresponde a
      `net10.0`) y `BCrypt.Net-Next` **4.0.3** (aquí no existía antes; en el proyecto del profesor sí). Los dos
      compilan en Docker.
- [ ] Están escritos los **cinco tropiezos** y cuáles dejan el sistema menos seguro de lo que parece: 1, 3 y 5
      (`3_plan.md` §7).
- [ ] Está escrito por qué aquí **sí** se usa un `DelegatingHandler`, que el plan del profesor evita en Blazor
      (`3_plan.md` §8).
- [ ] Está escrito por qué el `@rendermode` **no aplica**: el front es Razor Pages (`3_plan.md` §8).
- [ ] Ninguna clase lleva `[AllowAnonymous]`: solo el método de entrar (D87; `grep` del quickstart §5 en 0).

### F. Las tareas son verificables

- [ ] Cada fase de `8_tasks.md` tiene su verificación (quickstart o colección).
- [ ] El orden **contraseña → sesión → permiso → interfaz** está justificado (`2_spec.md` §1).
- [ ] Cada tarea es **un commit** en `rama-nicolas-v3`.
- [ ] La prueba del **criterio 9 en el navegador** está en la lista (`7_quickstart.md` §4, paso 3).

### G. El entorno está listo

- [ ] La v2 corregida **arranca y su colección pasa**.
- [ ] Se sabe que la primera vez conviene `docker compose down -v`: borra la base y la vuelve a crear con los usuarios
      sembrados con hash. (Aquí `db/init.sh` aplica el script de la v3 en cada arranque, pero la semilla de usuarios y
      de permisos solo entra si no existen.)
- [ ] `sp_verificar_acceso_ruta` **existe** en la base: se comprobó con
      `SELECT name FROM sys.procedures WHERE name = 'sp_verificar_acceso_ruta'`, no se supuso.
- [ ] Las **11 rutas** y los **3 roles** están sembrados: se comprobó con `SELECT nombre FROM ruta` y
      `SELECT nombre FROM rol`.

---

## Firma

| | |
|---|---|
| **Quién revisó** | |
| **Fecha** | |
| **Casillas sin marcar, y por qué** | |

> Si queda una casilla sin marcar, se escribe aquí por qué. Una lista con huecos sin explicar es peor que no tenerla.
