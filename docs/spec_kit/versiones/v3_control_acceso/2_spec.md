# Especificación Funcional — Gestión Profesoral v3: control de acceso (`2_spec.md`)

> **Documento 2 de 8 del spec kit de la v3.**
> Define **QUÉ** debe hacer el sistema en esta versión (solo el delta sobre la v2). El **CÓMO** está en `3_plan.md`.
> RF, RNF y reglas continúan la numeración de la v2 (que llegó a RF14, RNF14 y R10).
>
> | De dónde sale | Qué aporta |
> |---|---|
> | `docs/conceptos/CONCEPTOS_CONTROL_DE_ACCESO.md` (proyecto del profesor) | autenticar vs. autorizar, bcrypt, JWT, RBAC, «el menú no protege» |
> | `docs/conceptos/FLUJO_CONTROL_DE_ACCESO.md` (proyecto del profesor) | los 16 pasos del flujo, método por método |
> | `docs/spec_kit/versiones/v3_control_acceso/2_spec.md` (proyecto del profesor) | los **10 criterios** de aceptación de la v3 |
> | `0_mapa_versiones.md` de este proyecto | la v3 del módulo incluye además el CRUD de `usuario`, `rol` y `rol_usuario` |

---

## 1. Propósito

**Ponerle la puerta a lo que ya existe.** Con la v2 cerrada, las 16 tablas del módulo se operan desde el front, pero
cualquiera que llegue a la dirección puede hacer cualquier cosa. La v3 agrega, en este orden:

| | Qué | Por qué en ese orden |
|---|---|---|
| **1** | **La contraseña deja de guardarse en claro**: hash bcrypt al crear y al cambiar; se verifica contra el hash | Sin esto, lo demás es decoración |
| **2** | **La sesión**: quien se identifica recibe un token JWT que acompaña cada petición | Es lo que permite que la segunda petición sepa quién hizo la primera |
| **3** | **El permiso**: cada operación pregunta a la base si el rol de ese usuario puede | Es `sp_verificar_acceso_ruta` |

Y, porque el módulo lo pide en su mapa de versiones, **el CRUD de `usuario`, `rol` y `rol_usuario`** (y la pantalla de
permisos de cada rol), solo para el rol Administrador.

---

## 2. Alcance

**Incluye:**

- Hash bcrypt de la contraseña al crear y al cambiar un usuario; verificación al identificarse.
- `POST /api/sesion`: devuelve el token si las credenciales son correctas; **el mismo** `401` si falla el correo o la contraseña.
- El token en cada petición, y el rechazo (`401`) de las que no lo traigan o lo traigan vencido o alterado.
- La comprobación del permiso con `sp_verificar_acceso_ruta`, que cruza `usuario → rol_usuario → ruta_rol`.
- Dos tablas nuevas, `ruta` y `ruta_rol` (decisión D68): el esquema del módulo solo trae `usuario`, `rol` y
  `rol_usuario`, y sin una tabla de permisos no hay forma de que quitar un permiso surta efecto (criterio 7).
- Tres roles sembrados: **Administrador**, **Editor** y **Consulta** (decisión D69).
- Front: inicio y cierre de sesión, menú según el rol, modo consulta (sin botones de escritura), páginas de error 403 y 404,
  y la administración de usuarios, roles y permisos.

**No incluye (deliberado):** consultas multitabla, tablero, imagen corporativa y publicación (son la **v4**);
recuperación de contraseña, correo de confirmación y segundo factor (no los pide el curso).

---

## 3. Requisitos funcionales — API

### RF15 — La contraseña no se guarda en claro

| | |
|---|---|
| Al crear un usuario | La contraseña (mínimo 8 caracteres) se guarda **con hash bcrypt, costo 12**. La columna nunca tiene texto legible |
| Al identificarse | Se compara contra el hash (`BCrypt.Verify`), no la cadena |
| Al editar | Si el campo llega **vacío**, la contraseña **no se cambia**. Vacío = «déjela como está», no «bórrela» |
| Qué nunca sale | Ni la contraseña ni el hash en ningún JSON. `GET /api/usuario` no trae esa columna |

### RF16 — Identificarse devuelve un token

`POST /api/sesion` con `{ email, contrasena }`. La misma acción responde en `POST /api/login` (D80).

| Caso | Respuesta |
|---|---|
| Credenciales correctas | `200` con `{ token, email, nombre_completo, roles, expira }` |
| Correo inexistente, contraseña equivocada o usuario retirado | `401` con **el mismo** mensaje: «El correo o la contraseña no son correctos.» |
| Falta el correo o la contraseña | `422` |

El token (JWT firmado con HMAC-SHA256, 60 minutos) lleva `sub` y `name` = correo y un `role` por cada rol. **No lleva permisos.**

### RF17 — Sin token no se entra

Toda ruta de la API exige el token, **menos** `POST /api/sesion` (y su alias `POST /api/login`).

| Caso | Respuesta |
|---|---|
| Sin token | **401** «Debe iniciar sesión: la petición no trae un token válido.» |
| Token vencido | **401** «La sesión venció. Inicie sesión de nuevo.» |
| Token alterado o que no es un JWT | **401** |
| Token válido pero el rol no puede | **403** «Su rol no tiene permiso para esta operación.» con `detalle: ["permiso requerido: <ruta>"]` |

> **401 = «no sé quién es usted»; 403 = «sé quién es, y no puede».**

### RF18 — El permiso lo decide la base de datos, en cada petición

Cada controlador lleva `[ExigePermiso("<ruta>")]`. El filtro saca el correo del token y llama a
`sp_verificar_acceso_ruta(@email, @ruta)` → `tiene_acceso` (1/0). Si es 0 → `403` y el controlador **no corre**.

| Controladores | Ruta para entrar | Además, para POST, PUT y DELETE |
|---|---|---|
| `area-conocimiento`, `termino-clave`, `linea-investigacion`, `programa`, `red` | `interfaz.areas`, `interfaz.terminos`, `interfaz.lineas`, `interfaz.programas`, `interfaz.redes` | `datos.modificar` |
| Los 11 de la v2 (docente y todo su detalle) | `interfaz.docentes` | `datos.modificar` |
| `usuario`, `rol-usuario` | `interfaz.usuarios` | — |
| `rol` | `interfaz.roles` | — |
| `ruta`, `ruta-rol` | `interfaz.permisos` | — |

**No se hace:** armar el `JOIN` de permisos en C# (la regla quedaría en dos sitios) ni guardar los permisos en el
token (quitarlos no surtiría efecto hasta que venza).

### RF19 — Las rutas del usuario para armar el menú

`GET /api/permisos/mios` (solo exige token) → `{ email, roles, rutas }`. El correo sale del token, no de la petición.
Usa `sp_rutas_permitidas`. **No protege nada**: es para el menú.

### RF20 — Administración de usuarios, roles y permisos

| Recurso | Operaciones | Reglas |
|---|---|---|
| `/api/usuario` | listar, consultar, crear (con roles), editar (con roles), retirar (lógico) | ≥ 1 rol; correo y usuario únicos (`409`); contraseña obligatoria al crear |
| `/api/usuario/{id}/roles`, `/api/rol-usuario` | ver, asignar, quitar un rol | no se quita el único rol; no se deja sin administrador |
| `/api/rol` | listar (con total de usuarios), consultar, crear, editar, retirar | Administrador no se renombra ni se retira; rol con usuarios activos no se retira |
| `/api/ruta`, `/api/ruta-rol` | ver rutas, ver/dar/quitar permisos de un rol | al Administrador no se le quitan `interfaz.usuarios/roles/permisos` |

---

## 4. Requisitos funcionales — Front (Razor Pages)

### RF21 — Iniciar y cerrar sesión

- `/Sesion/Iniciar`: correo y contraseña. Si fallan, el mismo mensaje de la API y la caja de contraseña vacía.
- El token se guarda en la **sesión del servidor** (cookie `HttpOnly`): el navegador nunca lo ve.
- Sin sesión, cualquier página lleva a `/Sesion/Iniciar` y, al entrar, vuelve a la que se pidió.
- Si la API responde `401` (sesión vencida), se borra la sesión y se lleva al inicio de sesión con un aviso.
- «Salir» borra la sesión.
- Si la API no contesta, el front lo dice en castellano («No se pudo conectar con la API de gestión») y no muestra una
  página técnica (D83).

### RF22 — El menú según el rol

- El menú y las tarjetas de inicio se arman con `GET /api/permisos/mios`, que se vuelve a pedir **en cada página** (así un
  permiso quitado desaparece del menú sin volver a entrar).
- Sin `datos.modificar`: no se muestran «Nuevo», «Editar», «Retirar» ni formularios de escritura, y aparece el aviso «modo consulta».
- **Ninguna página revisa permisos por su cuenta.** Si alguien escribe una dirección a mano, la página le pide los datos
  a la API, la API responde `403` y el front muestra «No tiene permiso» con código `403`: el aviso de la API, el permiso
  que faltó («permiso requerido: interfaz.usuarios»), la dirección pedida y ningún dato (D84). En pantalla no sale ningún número de error (D88).

### RF23 — Pantallas de administración

Usuarios (roles con casillas, contraseña obligatoria al crear y opcional al editar), Roles y Permisos por rol
(elegir un rol y dar o quitar cada ruta).

---

## 5. Reglas de negocio de la v3

| | Regla | Dónde vive |
|---|---|---|
| R11 | La contraseña se guarda solo como hash bcrypt (costo 12) | `ServicioUsuario` (API) |
| R12 | El mismo mensaje para correo inexistente y contraseña equivocada | `ServicioSesion` + `SesionController` |
| R13 | El permiso se resuelve en la base, en cada petición | `sp_verificar_acceso_ruta` |
| R14 | Escribir (POST/PUT/DELETE) en el módulo exige `datos.modificar` | `[ExigePermiso("datos.modificar")]` en cada acción de escritura |
| R15 | No se deja el sistema sin administrador | `sp_revisar_ultimo_administrador`, `sp_eliminar_rol`, `sp_actualizar_rol`, `sp_eliminar_ruta_rol` |
| R16 | Un usuario no se retira a sí mismo | `ServicioUsuario.EliminarAsync` |
| R17 | Un usuario retirado (`activo = 0`) no inicia sesión y su token deja de servir | `sp_credenciales_usuario`, `sp_verificar_acceso_ruta` |

---

## 6. Requisitos no funcionales

| | |
|---|---|
| RNF15 | La clave de firma `JWT_KEY` (≥ 32 caracteres) viene de `.env`; si falta, la API **no arranca** con un mensaje claro |
| RNF16 | Ni contraseña ni hash en el código, en la colección de Postman ni en un JSON de respuesta (salvo las contraseñas de prueba documentadas) |
| RNF17 | En C# sigue sin haber SQL: el `grep` de la v2 sobre `api_gestion/Repositorios` no encuentra `SELECT/INSERT/UPDATE/DELETE` |
| RNF18 | Los mensajes de 401 y 403 están en castellano y con el formato `{ mensaje, detalle }` |
| RNF19 | El script `db/v3_control_acceso.sql` es idempotente: correrlo dos veces no duplica nada ni devuelve permisos quitados |

---

## 7. Criterios de aceptación

Los 10 del profesor (`CA-V3-01` a `CA-V3-10`), y los del módulo (`CA-V3-11` a `CA-V3-15`).

| | Criterio | Cómo se verifica |
|---|---|---|
| CA-V3-01 | La columna `password` **no tiene texto legible** en ninguna fila | Quickstart §3: consulta SQL → 0 filas |
| CA-V3-02 | Credenciales correctas → token; incorrectas → **el mismo** mensaje en los dos casos | Postman carpeta A |
| CA-V3-03 | Una operación **sin token** → **401** | Postman carpeta 9 |
| CA-V3-04 | Un token **alterado** → **401** | Postman carpeta 9 (token de consulta con el correo cambiado al del administrador) |
| CA-V3-05 | Un rol sin permiso → **403**, y se explica la diferencia con 401 | Postman carpeta 9 |
| CA-V3-06 | El permiso lo resuelve `sp_verificar_acceso_ruta`: no hay `JOIN` de permisos en C# | `grep` del quickstart §5 |
| CA-V3-07 | **Quitarle un permiso a un rol surte efecto sin volver a identificarse** | Postman carpeta 10 y quickstart §4 en el front |
| CA-V3-08 | El menú del front cambia según el rol | Quickstart §4 (admin, editor, consulta) |
| CA-V3-09 | **Escribir la dirección a mano, sin permiso, responde 403** | Quickstart §4: consulta escribe `/Usuarios` → «No tiene permiso»; Postman carpeta 9 |
| CA-V3-10 | **Regresión:** los criterios de la v1 y la v2 siguen pasando, **con token** | Postman carpetas 0 a 8 con el token de administrador |
| CA-V3-11 | El rol Consulta no puede crear, editar ni retirar (API `403`, front sin botones) | Postman carpeta 9; quickstart §4 |
| CA-V3-12 | Editar un usuario con la contraseña vacía no la cambia | Postman carpeta 11 |
| CA-V3-13 | Un usuario retirado ya no inicia sesión | Postman carpeta 11 |
| CA-V3-14 | No se puede dejar el sistema sin administrador | Postman carpeta 11 («eliminar el rol Administrador → 409») |
| CA-V3-15 | Sin `JWT_KEY` la API no arranca y lo dice | Quickstart §1 |

> **El criterio 9 es el que no se puede simular.** Los demás se pueden cumplir con un front que esconde botones. Ese no.

---

## 8. Definición de TERMINADA

1. Los criterios en verde, verificados con `7_quickstart.md` y la colección `postman/gestion_profesoral_v3.postman_collection.json`.
2. La regresión de la v1 y la v2 pasa (carpetas 0 a 8 de la colección v3).
3. `9_checklist.md` revisado por **otro** integrante.
4. PR de la rama a `main`, y tag `v3`.

---

## 9. Clarificaciones (compuerta 1)

| Pregunta | Respuesta |
|---|---|
| ¿El esquema del módulo tiene tabla de permisos? | No: solo `usuario`, `rol`, `rol_usuario`. Se agregan `ruta` y `ruta_rol` (D68), como en el proyecto del profesor |
| ¿Qué roles hay? | Administrador (todo), Editor (el módulo y escribir), Consulta (el módulo, solo leer) (D69) |
| ¿Cómo se distingue leer de escribir? | Con la ruta `datos.modificar` (D70) |
| ¿Dónde se guarda el token en el front? | En la sesión del servidor, no en el navegador (D72) |
| ¿El login se llama `/api/login` o `/api/sesion`? | Los dos: la guía del profesor usa `/api/sesion` y la metodología y el documento del módulo dicen `/api/login`. Las dos rutas llevan al mismo método (D80) |
| ¿Las pantallas por tabla de la v2 corregida también quedan protegidas? | Sí: usan las mismas rutas de la API, que exigen `interfaz.docentes` (o la del catálogo) y `datos.modificar` para escribir. En el menú y en la página de inicio aparecen según el rol |
