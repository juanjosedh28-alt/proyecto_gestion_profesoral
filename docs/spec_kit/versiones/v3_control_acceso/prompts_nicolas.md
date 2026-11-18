# Prompts de Nicolás — Versión 3

> Integrante: **Nicolás** · Rama: `rama-nicolas-v3` · Herramienta: Claude (chat con herramientas de código) ·
> Noviembre de 2026.
>
> Los prompts van **textuales**, como se escribieron. Debajo de cada uno: qué produjo, qué revisé y qué hubo que
> corregir. Solo están los mensajes que llevaron del spec kit al código; las consultas sobre Git, Docker o Postman
> no se incluyen.

---

## Cómo trabajé con la IA

1. **Las fuentes son las del profesor.** Cada prompt de la v3 va acompañado del mensaje del profesor y de sus dos
   documentos: `CONCEPTOS_CONTROL_DE_ACCESO.md` (la teoría) y `FLUJO_CONTROL_DE_ACCESO.md` (los 16 pasos en el código).
   La v3 sigue ese flujo método por método; donde nuestro proyecto es distinto, la diferencia está escrita en
   `4_research.md`.
2. **La v3 va encima de una v2 que funciona.** Cuando el profesor corrigió la v2, no seguí con la v3 vieja:
   primero se cerró la v2 corregida y después se rehízo la v3 sobre ella (D81), para que la regresión con token
   corra sobre la v2 que se va a entregar.
3. **Todo se prueba fuera del chat.** Cada versión la levanté con Docker, revisé los logs y corrí la colección de
   Postman. En la revisión final (prompt 3) la IA además compiló el front y lo probó contra una API simulada; la API
   y la base solo se prueban con Docker.
4. **Commits propios y pequeños.** Un commit por cambio, en mi rama, con mensajes que dicen qué y por qué.

---

## Prompt 1 — Construir la v3 con lo que pide el profesor

> Cordial saludo estimadas y estimados estudiantes · La sustentación de la tercera entrega control de acceso es el
> sábado 21 de noviembre · […] para la sustentación de la versión 3 debe tener en cuenta de manera muy detallada:
> CONCEPTOS_CONTROL_DE_ACCESO.md · FLUJO_CONTROL_DE_ACCESO.md · la cuarta y última entrega no se sustenta […]
>
> esta es la 3ra entrega, puedes editarlo y completarlo porfa

**Resultado:** se compararon los 10 criterios de la spec de la v3 del profesor con nuestro esquema. El módulo solo
tiene `usuario`, `rol` y `rol_usuario`, sin una tabla de permisos; sin ella el criterio 7 (quitar un permiso sin
volver a entrar) no se puede cumplir. Por eso se agregaron `ruta` y `ruta_rol` (D68), igual que `ruta` y `rutarol`
en el proyecto del profesor. Se definieron tres roles (D69) y la ruta `datos.modificar` para que Consulta no escriba
(D70), que es el criterio de cierre del documento del módulo.

**Construido, un commit por paso:**

1. Base: `db/v3_control_acceso.sql` con `ruta`, `ruta_rol`, roles, permisos, usuarios con hash bcrypt (en el
   script, no a mano) y los procedimientos `sp_verificar_acceso_ruta`, `sp_rutas_permitidas` y los de administración.
2. API: `POST /api/sesion` con bcrypt y JWT; `[ExigePermiso]` que llama a `sp_verificar_acceso_ruta` en cada
   petición; `[Authorize]` en los 16 controladores y `datos.modificar` en cada escritura; `GET /api/permisos/mios`.
3. API: administración de usuarios, roles y permisos (solo Administrador).
4. Front: token en la sesión del servidor, `ManejadorToken` (Bearer), `FiltroSesion`, 401 → iniciar sesión,
   403 → «No tiene permiso»; menú según el rol y modo consulta; pantallas de Usuarios, Roles y Permisos por rol.
5. Colección de Postman v3 y el spec kit de la v3.

## Prompt 2 — Revisión exhaustiva antes de la sustentación

> […mismo mensaje del profesor…] esto fue lo que nos actualizo el profesor quiero que lo revise detenidamente y
> exhaustivamente que no falte ningun detalle porfavor para poder hacer el trabajo correcto y lo que pide el profeso

**Resultado:** se volvió a leer cada documento (`CONCEPTOS`, `FLUJO`, la spec de la v3 del profesor, las guías de
verificación con Swagger y Postman, `0_METODOLOGIA.md` y el documento del módulo) y se encontró:

| # | Hallazgo | Qué se hizo |
|---|---|---|
| 1 | La v3 estaba construida sobre la v2 anterior a las correcciones del profesor | Se rehízo sobre `rama-nicolas-v2-correcciones`, llevando los commits uno por uno (D81) |
| 2 | El menú y la página de inicio de la v3 tenían 6 entradas; la v2 corregida tiene 16 tablas | Menú en grupos (Catálogos, Docentes, Información del docente, Relaciones, Administración) y 16 tarjetas, cada una según el rol |
| 3 | Crear, editar y retirar áreas de conocimiento (nuevo en la v2 corregida) no exigía `datos.modificar` | Se agregó el atributo a las tres acciones |
| 4 | La guía del profesor usa `POST /api/sesion`, pero `0_METODOLOGIA.md` §6 y el documento del módulo dicen `POST /api/login` | Las dos rutas llegan al mismo método (D80) |
| 5 | Los `curl` de la regresión de la v1 ya no servían sin token (criterio 10) | Quickstart §2.1: cómo sacar el token y repetirlos con `Authorization: Bearer` |
| 6 | La colección v3 venía de la colección v2 anterior | Se regeneró desde la v2 corregida: carpetas 0 a 8 de regresión con token y 9 a 11 de acceso, con pruebas nuevas (Consulta no crea áreas ni retira becas, `/api/login`) |
| 7 | Las decisiones de la v3 empezaban en D65, el mismo número que usó después la v2 corregida | Se renumeraron D68 a D81 |

## Prompt 3 — Revisión final antes de la sustentación

> estas seguro de que todo esta completo de la parte 3 ? no puede haber errores de igual forma puedes darle una ultima
> revisada que si este cumpliendo con todo correctamente

**Resultado:** se revisó otra vez todo el código de la v3 (base, API y front) y, además de `CONCEPTOS` y `FLUJO`, se
comparó contra el **plan**, los **contratos**, el **quickstart** y la **lista de chequeo** de la v3 del profesor. Esta
vez el front se compiló con .NET 10 en el entorno de la IA (0 advertencias, 0 errores) y se probó contra una API
simulada con las mismas reglas de acceso; la API y la base se siguen probando con Docker.

| # | Hallazgo | Qué se hizo |
|---|---|---|
| 1 | `ClockSkew` estaba en un minuto: un token vencido seguía entrando ese minuto. El plan del profesor lo pide en cero (tropiezo 1) | `ClockSkew = TimeSpan.Zero` (D82) |
| 2 | Con la API apagada, **todas** las páginas del front daban error 500 con la página técnica: el filtro pide las rutas antes que la página, y su error no lo atendía nadie. En la v2 salía el aviso «No se pudo conectar con la API» | El filtro conserva el menú y deja que la página avise; lo imprevisto lo recoge `UseExceptionHandler` con una página en castellano (D83). Probado: 20 listados con aviso, 12 formularios con la página de error, ninguna página técnica |
| 3 | La página 403 decía un texto propio, no lo que respondió la API | Muestra el aviso de la API, el permiso que faltó y la dirección pedida, sin datos (D84) |
| 4 | `/Roles/Crear` escrito a mano por un Editor mostraba el formulario (la página no le pedía nada a la API) | Ahora pide los roles que ya existen: la API responde 403 (D85) |
| 5 | Volver después de entrar aceptaba `/\otro.com`, que el navegador lee como otro sitio; y con dos pestañas el token viejo viajaba en el inicio de sesión | `Url.IsLocalUrl` y sesión anterior descartada (D86) |
| 6 | La ruta `interfaz.areas` decía «(consulta)», pero el área tiene CRUD desde la v2 corregida | Semilla corregida, con un `UPDATE` para una base ya sembrada |
| 7 | Faltaba escribir lo que la lista del profesor pide: los cinco tropiezos, por qué aquí sí hay `DelegatingHandler`, lo que queda fuera, las direcciones abiertas, la matriz de los tres roles | `3_plan.md` §7 a §9, `6_contracts.md`, `9_checklist.md` (secciones A a G), `7_quickstart.md` §7 |
| 8 | El quickstart decía `docker compose logs api`, pero el servicio se llama `api-gestion` | Corregido |
| 9 | `SesionController` tenía `[AllowAnonymous]` en la clase: es el error que la guía de IA del profesor anuncia para esta versión (anula cualquier `[Authorize]` de un método nuevo) | `[Authorize]` en la clase y `[AllowAnonymous]` solo en el método de entrar (D87) |
| 10 | La página de error mostraba el número (403, 404, 500) en grande, y algunos textos decían «token» o «hash bcrypt»: la regla del idioma dice que en pantalla no hay jerga | Solo palabras en pantalla; el 403 sigue en la respuesta (D88) |

**Lo que se comprobó y estaba bien:** los 16 controladores del módulo tienen `[Authorize]` y `[ExigePermiso]` en la
clase, y las 50 acciones que escriben exigen `datos.modificar`; ningún repositorio tiene SQL (criterio 6, el `grep` no
imprime nada); los nombres de las rutas coinciden en la API, el front y la semilla; los tres hashes de la semilla
verifican con sus contraseñas; `sp_verificar_acceso_ruta` falla cerrado; el token lleva el correo y los nombres de los
roles (como en el paso 9 de `FLUJO`) y ningún permiso, y la API nunca autoriza con esos roles.

---

## Lo que hubo que corregir durante la construcción

| # | Qué pasó | Cómo se corrigió | Quién lo detectó |
|---|---|---|---|
| 1 | `sp_rutas_permitidas` ordenaba por una columna que no estaba en el `SELECT DISTINCT` (SQL Server lo rechaza) | Ordenar por el alias `ruta` | La IA, al revisar el script |
| 2 | La página de error fallaba cuando el 403 venía de un formulario (POST) | Leer el código de la ruta y aceptar GET y POST | La IA, al revisar el flujo del 403 |
| 3 | Las casillas de roles del formulario de usuario no llegaban al servidor por el nombre del campo | Nombrarlas `RolIds`, como los demás campos | La IA, al revisar el enlace de datos |
| 4 | Un comentario con la palabra JOIN hacía fallar el `grep` del criterio 6 | Se reescribió el comentario | La IA, corriendo el `grep` |
| 5 | Al levantar con Docker la API se apagaba sola: el `.env` no tenía `JWT_KEY` | Se agregó la clave (32 caracteres o más). La API está hecha para no arrancar sin ella | Nicolás, con `docker compose ps` y el aviso «The JWT_KEY variable is not set» |
| 6 | El front no compilaba: `Index.cshtml` usaba `Context.Session`, que no existe en una página Razor | `HttpContext.Session` | Nicolás, en `docker compose logs front-razor` (`error CS0103`) |

## Verificación

- `docker compose ps -a`: `sqlserver-init` en **Exited (0)**, API y front en **Up**.
- Log del front: «Build succeeded» y «Application started».
- Pendiente de anotar: el resultado de la colección v3 completa y del quickstart §4 en el front.
