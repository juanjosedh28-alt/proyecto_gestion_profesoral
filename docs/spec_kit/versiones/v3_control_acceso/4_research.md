# Investigación y decisiones — Gestión Profesoral v3 (`4_research.md`)

> **Documento 4 de 8 del spec kit de la v3.** Por qué se hizo así. Continúa la numeración de la v2 (que llegó a D67).

| | Decisión | Alternativas | Por qué |
|---|---|---|---|
| **D68** | Agregar las tablas `ruta` y `ruta_rol` | Guardar los permisos en el código (`[Authorize(Roles = "Administrador")]`) | Con roles escritos en el código, quitarle un permiso a un rol exige recompilar: el criterio 7 no se cumple. Con `ruta_rol` lo decide la base, como en el proyecto del profesor (`ruta`, `rutarol`) |
| **D69** | Tres roles: Administrador, Editor y Consulta | Solo Administrador y «usuario» | El módulo distingue quien administra, quien modifica y quien solo consulta. Además permite mostrar 403 en dos niveles (pantalla y escritura) |
| **D70** | La escritura se protege con una ruta aparte, `datos.modificar` | Una ruta por tabla y por verbo (`programa.crear`…) | 16 tablas × 3 verbos = 48 rutas que nadie administraría. Una sola ruta dice exactamente lo que el módulo pide: «consulta no escribe» |
| **D71** | bcrypt con costo 12 (`BCrypt.Net-Next`) | SHA-256; Argon2 | SHA-256 es rápido: con una GPU se prueban miles de millones por segundo. bcrypt es lento a propósito y trae el salt dentro del hash. Argon2 es mejor pero bcrypt es lo que enseña el curso (Provos y Mazières, 1999) |
| **D72** | El front guarda el token en la sesión del servidor | `localStorage`; cookie con el token | El navegador nunca ve el token: un script inyectado no lo puede robar. Razor Pages corre en el servidor, así que es lo natural |
| **D73** | Los permisos NO van en el token; se consultan en cada petición | Meterlos como claims | Si van en el token, quitarlos no surte efecto hasta que venza (CONCEPTOS §3). Una consulta indexada por petición es barata |
| **D74** | Política de respaldo `RequireAuthenticatedUser` además de `[Authorize]` | Solo `[Authorize]` en cada clase | Si alguien agrega un controlador y olvida `[Authorize]`, igual queda cerrado |
| **D75** | El mismo `401` para correo inexistente, contraseña equivocada y usuario retirado, y comparación contra un hash de relleno | Mensajes distintos | No confirmarle a un desconocido qué correos existen, ni por el texto ni por el tiempo de respuesta |
| **D76** | `POST /api/sesion` (no `/api/login`) | `/api/login`, `/api/auth` | Es el nombre de la guía del profesor: la sustentación sigue sus pasos |
| **D77** | El menú se refresca en cada GET del front | Solo al iniciar sesión | Así un permiso quitado también desaparece del menú sin volver a entrar (coherente con D73) |
| **D78** | Retirar un usuario o un rol es lógico (`activo = 0`); quitarle un rol a un usuario o un permiso a un rol es físico | Todo lógico | Usuarios y roles tienen historia y otras filas los nombran; las parejas de `rol_usuario` y `ruta_rol` no: si se vuelven a dar, se insertan de nuevo |
| **D79** | Protecciones contra quedarse sin administrador en la base | En el servicio de C# | Es una regla de datos: si alguien llama al procedimiento por otro camino, igual se cumple |
| **D80** | `POST /api/sesion` **y** `POST /api/login` llegan al mismo método (precisa D76) | Elegir uno solo | La guía del profesor (`FLUJO_CONTROL_DE_ACCESO.md`) usa `/api/sesion`; `0_METODOLOGIA.md` §6 y el documento del módulo dicen `/api/login`. Con los dos, cualquiera de las tres fuentes se cumple sin duplicar lógica |
| **D81** | La v3 se reconstruyó sobre la v2 corregida (17 de noviembre) | Mantenerla sobre la v2 anterior | El profesor corrigió la v2 (todas las tablas en la ventana principal, CRUD del área). La regresión de la v3 tiene que correr sobre esa v2. El menú agrupa las 16 tablas y cada entrada aparece según el rol; las escrituras del área exigen `datos.modificar` |
| **D82** | `ClockSkew = TimeSpan.Zero` | El valor por defecto (5 minutos) o un minuto | El token lo firma y lo valida el mismo servidor: no hay dos relojes que compensar. Con tolerancia, un token vencido sigue entrando y parece que el código está mal (tropiezo 1 del plan del profesor) |
| **D83** | Si la API no contesta, el front lo dice y no se cae: `FiltroSesion` conserva el menú y `UseExceptionHandler("/Error")` atiende lo que ninguna página atendió | Un `try/catch` más en cada una de las 65 páginas | La v2 ya mostraba «No se pudo conectar con la API»; la v3 lo había roto, porque el filtro pide las rutas **antes** que la página. Un solo sitio para lo imprevisto, en castellano y sin la página técnica |
| **D84** | La página 403 muestra el aviso de la API, el permiso que faltó y la dirección pedida, con código HTTP 403 y sin datos | Mostrar la pantalla pedida vacía con un aviso (como el front Blazor del profesor) | La pantalla sí se ejecuta y le pide los datos a la API; lo que se muestra es exactamente lo que la API respondió («Su rol no tiene permiso para esta operación» · «permiso requerido: interfaz.usuarios»). En Razor Pages la respuesta puede llevar el 403 de verdad, y se ve en las herramientas del navegador |
| **D85** | Toda pantalla de administración le pide algo a la API al abrirse («Nuevo rol» pide los roles que ya existen) | Formulario que se abre sin preguntar | Así cualquier dirección de administración escrita a mano recibe el 403 de la API, también `/Roles/Crear` (criterio 9) |
| **D86** | Al iniciar sesión: se descarta la sesión anterior y se vuelve solo a direcciones locales (`Url.IsLocalUrl`) | Comparar a mano que empiece por «/» | Con dos pestañas, el token viejo no viaja en el inicio de sesión (un 401 de credenciales no se confunde con una sesión vencida); `Url.IsLocalUrl` también rechaza `/\otro.com`, que el navegador lee como otro sitio |
| **D87** | `SesionController`: `[Authorize]` en la clase y `[AllowAnonymous]` solo en el método de entrar | `[AllowAnonymous]` en la clase (como estaba) | Con el atributo en la clase, cualquier método que se agregue después queda abierto aunque lleve `[Authorize]`, y solo avisa un *warning* (ASP0026). Cerrado por defecto, abierto por excepción |
| **D88** | En pantalla no sale ningún número de error (403, 404, 500) ni jerga («token», «hash»); el código va en la respuesta HTTP | Mostrar el código grande en la página de error | La regla del idioma del curso: la persona lee «No tiene permiso», no «403». El 403 sigue siendo verificable en las herramientas del navegador (F12 → Red) |

## Referencias

1. RFC 9110 — *HTTP Semantics*, §15.5.2 (401) y §15.5.4 (403).
2. RFC 7519 — *JSON Web Token (JWT)*.
3. Provos, N. y Mazières, D. — *A Future-Adaptable Password Scheme* (USENIX, 1999): bcrypt.
4. Sandhu, R. et al. — *Role-Based Access Control Models* (IEEE Computer, 1996).
5. NIST SP 800-63B — *Digital Identity Guidelines*, §5.1.1.
6. OWASP Top Ten 2021 — A01 *Broken Access Control*.
7. `docs/conceptos/CONCEPTOS_CONTROL_DE_ACCESO.md` y `FLUJO_CONTROL_DE_ACCESO.md` del proyecto del profesor.
8. `docs/spec_kit/versiones/v3_control_acceso/3_plan.md` del proyecto del profesor: los cinco tropiezos (§5), el
   `DelegatingHandler` (§4.2) y lo que queda fuera (§6). Aquí: `3_plan.md` §7 a §9.
