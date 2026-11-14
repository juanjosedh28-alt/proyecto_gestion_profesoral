# Investigación y decisiones — Gestión Profesoral v3 (`4_research.md`)

> **Documento 4 de 8 del spec kit de la v3.** Por qué se hizo así. Continúa la numeración de la v2 (que llegó a D64).

| | Decisión | Alternativas | Por qué |
|---|---|---|---|
| **D65** | Agregar las tablas `ruta` y `ruta_rol` | Guardar los permisos en el código (`[Authorize(Roles = "Administrador")]`) | Con roles escritos en el código, quitarle un permiso a un rol exige recompilar: el criterio 7 no se cumple. Con `ruta_rol` lo decide la base, como en el proyecto del profesor (`ruta`, `rutarol`) |
| **D66** | Tres roles: Administrador, Editor y Consulta | Solo Administrador y «usuario» | El módulo distingue quien administra, quien modifica y quien solo consulta. Además permite mostrar 403 en dos niveles (pantalla y escritura) |
| **D67** | La escritura se protege con una ruta aparte, `datos.modificar` | Una ruta por tabla y por verbo (`programa.crear`…) | 16 tablas × 3 verbos = 48 rutas que nadie administraría. Una sola ruta dice exactamente lo que el módulo pide: «consulta no escribe» |
| **D68** | bcrypt con costo 12 (`BCrypt.Net-Next`) | SHA-256; Argon2 | SHA-256 es rápido: con una GPU se prueban miles de millones por segundo. bcrypt es lento a propósito y trae el salt dentro del hash. Argon2 es mejor pero bcrypt es lo que enseña el curso (Provos y Mazières, 1999) |
| **D69** | El front guarda el token en la sesión del servidor | `localStorage`; cookie con el token | El navegador nunca ve el token: un script inyectado no lo puede robar. Razor Pages corre en el servidor, así que es lo natural |
| **D70** | Los permisos NO van en el token; se consultan en cada petición | Meterlos como claims | Si van en el token, quitarlos no surte efecto hasta que venza (CONCEPTOS §3). Una consulta indexada por petición es barata |
| **D71** | Política de respaldo `RequireAuthenticatedUser` además de `[Authorize]` | Solo `[Authorize]` en cada clase | Si alguien agrega un controlador y olvida `[Authorize]`, igual queda cerrado |
| **D72** | El mismo `401` para correo inexistente, contraseña equivocada y usuario retirado, y comparación contra un hash de relleno | Mensajes distintos | No confirmarle a un desconocido qué correos existen, ni por el texto ni por el tiempo de respuesta |
| **D73** | `POST /api/sesion` (no `/api/login`) | `/api/login`, `/api/auth` | Es el nombre de la guía del profesor: la sustentación sigue sus pasos |
| **D74** | El menú se refresca en cada GET del front | Solo al iniciar sesión | Así un permiso quitado también desaparece del menú sin volver a entrar (coherente con D70) |
| **D75** | Retirar un usuario o un rol es lógico (`activo = 0`); quitarle un rol a un usuario o un permiso a un rol es físico | Todo lógico | Usuarios y roles tienen historia y otras filas los nombran; las parejas de `rol_usuario` y `ruta_rol` no: si se vuelven a dar, se insertan de nuevo |
| **D76** | Protecciones contra quedarse sin administrador en la base | En el servicio de C# | Es una regla de datos: si alguien llama al procedimiento por otro camino, igual se cumple |

## Referencias

1. RFC 9110 — *HTTP Semantics*, §15.5.2 (401) y §15.5.4 (403).
2. RFC 7519 — *JSON Web Token (JWT)*.
3. Provos, N. y Mazières, D. — *A Future-Adaptable Password Scheme* (USENIX, 1999): bcrypt.
4. Sandhu, R. et al. — *Role-Based Access Control Models* (IEEE Computer, 1996).
5. NIST SP 800-63B — *Digital Identity Guidelines*, §5.1.1.
6. OWASP Top Ten 2021 — A01 *Broken Access Control*.
7. `docs/conceptos/CONCEPTOS_CONTROL_DE_ACCESO.md` y `FLUJO_CONTROL_DE_ACCESO.md` del proyecto del profesor.
