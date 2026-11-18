# Plan técnico — Gestión Profesoral v3 (`3_plan.md`)

> **Documento 3 de 8 del spec kit de la v3.** El **CÓMO** de lo que pide `2_spec.md`.

---

## 1. Archivos de la v3

| Capa | Archivo | Qué hace |
|---|---|---|
| Base | `db/v3_control_acceso.sql` | Tablas `ruta` y `ruta_rol`, roles, rutas, permisos y usuarios de prueba (con hash), y los procedimientos del acceso y de la administración |
| Base | `db/init.sh` | Paso 4: aplica `v3_control_acceso.sql` en cada arranque (es idempotente) |
| Docker | `docker-compose.yml`, `.env.example` | Pasa `JWT_KEY` a la API como `Jwt__Key` |
| API | `ApiGestion.csproj` | Paquetes `BCrypt.Net-Next` y `Microsoft.AspNetCore.Authentication.JwtBearer` |
| API | `appsettings.json` | Sección `Jwt` (emisor, audiencia, 60 minutos). **Sin la clave** |
| API | `Program.cs` | Autenticación JWT, política de respaldo, botón «Authorize» de Swagger, registros |
| API | `Autorizacion/ConfiguracionJwt.cs` | La configuración del token |
| API | `Autorizacion/ExigePermisoAttribute.cs` | El filtro que pregunta a la base en cada petición |
| API | `Controllers/SesionController.cs` | `POST /api/sesion` y su alias `POST /api/login` (D80) |
| API | `Controllers/PermisosController.cs` | `GET /api/permisos/mios` |
| API | `Controllers/UsuarioController.cs`, `RolController.cs` | Administración (usuario, rol-usuario, rol, ruta, ruta-rol) |
| API | `Servicios/ServicioSesion.cs` | Verifica la contraseña con bcrypt y arma el token |
| API | `Servicios/ServicioAdministracion.cs` | Hash al crear/editar, reglas de usuario y rol |
| API | `Repositorios/RepositorioAcceso.cs` | `TieneAccesoAsync`, `RutasPermitidasAsync`, `CredencialesAsync`, `RolesAsync` |
| API | `Repositorios/RepositorioAdministracion.cs` | Usuario, rol y permisos por procedimientos |
| API | Los 16 controladores de la v1 y la v2 | `[Authorize]` + `[ExigePermiso("interfaz.x")]` en la clase y `[ExigePermiso("datos.modificar")]` en cada escritura |
| Front | `Infraestructura/SesionUsuario.cs` | Lee y escribe la sesión del servidor (token, nombre, roles, rutas) |
| Front | `Infraestructura/ManejadorToken.cs` | Pone `Authorization: Bearer <token>` en cada petición a la API |
| Front | `Infraestructura/FiltroSesion.cs` | Sin sesión → `/Sesion/Iniciar`; en cada GET refresca las rutas del menú |
| Front | `Infraestructura/ManejadorAcceso.cs`, `ExcepcionesAcceso.cs` | `401` → borra la sesión y lleva a iniciar; `403` → página «No tiene permiso» |
| Front | `Clientes/ClienteApiBase.cs`, `ClientesAcceso.cs` | Traduce 401/403; clientes de sesión, usuario, rol y permiso |
| Front | `Pages/Sesion/Iniciar`, `Salir`, `Pages/Error` | Inicio y cierre de sesión; errores 403 (con el aviso de la API), 404 y 500 (API apagada) |
| Front | `Pages/Shared/_Layout.cshtml`, `Pages/Index.cshtml` | Las 16 tablas de la v2 corregida en el menú (Catálogos, Docentes, Información del docente, Relaciones) y en 16 tarjetas, más Administración; cada entrada según las rutas del rol; modo consulta |
| Front | `Pages/Usuarios`, `Pages/Roles`, `Pages/Permisos` | Administración |
| Pruebas | `postman/gestion_profesoral_v3.postman_collection.json` | Sesión, regresión v2 con token, 401/403, criterio 7, administración |

---

## 2. El flujo, en tres momentos

```
INICIAR SESIÓN
  Front  Pages/Sesion/Iniciar.cshtml.cs  OnPostAsync
         → ClienteSesion.IniciarAsync ─ POST /api/sesion ─►
  API    SesionController.Iniciar → ServicioSesion.IniciarAsync
           → RepositorioAcceso.CredencialesAsync (sp_credenciales_usuario)  → hash
           → BCrypt.Verify(contraseña, hash)                                → ¿coincide?
           → RepositorioAcceso.RolesAsync (sp_roles_de_usuario)            → roles
           → ArmarToken (sub, name = correo; role por cada rol; HMAC-SHA256; 60 min)
         ◄─ 200 { token, email, nombre_completo, roles, expira }  |  401 mismo mensaje

ARMAR EL MENÚ
  Front  ClienteSesion.MisPermisosAsync ─ GET /api/permisos/mios (con token) ─►
  API    PermisosController.Mios → RepositorioAcceso.RutasPermitidasAsync (sp_rutas_permitidas)
  Front  SesionUsuario.GuardarRutas → _Layout pregunta SesionUsuario.Tiene("interfaz.x")

CADA PETICIÓN
  Front  ManejadorToken pone «Bearer <token>»
  API    UseAuthentication: valida firma, emisor, audiencia y vencimiento → si no, 401
         UseAuthorization: política de respaldo (hay usuario) → si no, 401
         [ExigePermiso("interfaz.x")] → RepositorioAcceso.TieneAccesoAsync
             → sp_verificar_acceso_ruta(@email, @ruta) → tiene_acceso 1/0 → si 0, 403
         [ExigePermiso("datos.modificar")] (solo en POST/PUT/DELETE) → igual
         el controlador corre → 200/201/204
```

---

## 3. Decisiones de implementación

- **Middleware de la API, en orden:** `ManejadorErrores` → `ManejadorErroresSql` → Swagger → `UseAuthentication` →
  `UseAuthorization` → controladores. Swagger queda abierto (no es un endpoint) y trae el botón **Authorize**.
- **Mensajes del 401:** `JwtBearerEvents.OnChallenge` escribe `{ mensaje, detalle }` en castellano; si el token venció lo dice.
- **`MapInboundClaims = false`** y `NameClaimType = "name"`: `User.Identity.Name` es el correo, tal como va en el token.
- **Comparación de tiempo constante:** si el correo no existe, se compara igual contra un hash de relleno, para que la
  respuesta no tarde distinto y no delate qué correos existen.
- **Front:** el token vive en `ISession` (memoria del servidor, cookie `gestion.sesion` `HttpOnly`, 60 minutos).
  Los 20 clientes HTTP se registran con `AddHttpMessageHandler<ManejadorToken>()`.
- **Página de error:** `UseStatusCodePagesWithReExecute("/Error/{0}")` antes de `UseRouting()`; `Error` acepta GET y POST.
  En el 403 muestra el aviso de la API, el permiso que faltó y la dirección pedida, sin ningún dato (D84).
- **API apagada:** `FiltroSesion` conserva el menú que ya había y la página muestra su aviso; lo que ninguna página
  atiende lo recoge `UseExceptionHandler("/Error")`, que lo dice en castellano con código 500 (D83).

---

## 4. Procedimientos nuevos

| Grupo | Procedimientos |
|---|---|
| Acceso | `sp_verificar_acceso_ruta`, `sp_rutas_permitidas`, `sp_credenciales_usuario`, `sp_roles_de_usuario` |
| Usuario | vista `vw_usuario` (sin contraseña), `sp_listar_usuario`, `sp_consultar_usuario`, `sp_crear_usuario`, `sp_actualizar_usuario`, `sp_eliminar_usuario`, `sp_revisar_ultimo_administrador` |
| Rol de usuario | `sp_listar_rol_usuario`, `sp_crear_rol_usuario`, `sp_eliminar_rol_usuario` |
| Rol | `sp_listar_rol`, `sp_consultar_rol`, `sp_crear_rol`, `sp_actualizar_rol`, `sp_eliminar_rol` |
| Permisos | `sp_listar_ruta`, `sp_listar_ruta_rol`, `sp_crear_ruta_rol`, `sp_eliminar_ruta_rol` |

Los errores usan los códigos de la v2 (`50001` padre inexistente → 409, `50002` conflicto → 409, `50003` dato inválido → 422,
`50004` no encontrado → 404), que ya traduce `ManejadorErroresSql`.

---

## 5. Riesgos y mitigaciones

| Riesgo | Mitigación |
|---|---|
| Olvidar `[Authorize]` en un controlador nuevo | Política de respaldo: todo exige sesión salvo lo marcado `[AllowAnonymous]` |
| Olvidar `[ExigePermiso]` en una escritura | Revisión con `grep -c "ExigePermiso(\"datos.modificar\")"` por controlador (quickstart §5) |
| Que el administrador se quede por fuera | Regla R15 en los procedimientos |
| Re-sembrar permisos al reiniciar y devolver uno que se quitó | `ruta_rol` se siembra solo si está vacía |
| `JWT_KEY` ausente o corta | La API no arranca y lo dice |
| El `.env` viejo no tiene `JWT_KEY` | Quickstart §1 lo pide antes de levantar |

---

## 6. Chequeo de constitución (compuerta 2)

| Artículo | ¿Se cumple? |
|---|---|
| 2 y enmienda (capas) | Sí: `Controller → Servicio → Repositorio → EjecutorProcedimientos → procedimiento` |
| 8 y enmiendas (seguridad, SQL solo en la base) | Sí: hash bcrypt; claves en `.env`; ningún SQL en C# |
| 9 (contrato HTTP) | Sí: 401 y 403 con `{ mensaje, detalle }` |
| 14 (nunca 500 previsible) | Sí: reglas de usuario y rol con 409/422/404 |
| 17 (regresión) | Sí: carpetas 0 a 8 de la colección v3 |
| 18 a 21 (nuevos) | Sí: ver `2_spec.md` RF17 a RF22 |

---

## 7. Los cinco tropiezos del plan del profesor, y cómo quedó cada uno aquí

El plan de la v3 del profesor (§5) nombra cinco errores que **compilan y arrancan**. Así quedaron en este proyecto:

| | Tropiezo | Cómo se ve si pasa | Aquí |
|---|---|---|---|
| **1** | `ClockSkew` por defecto | ASP.NET acepta **5 minutos** un token ya vencido | `ClockSkew = TimeSpan.Zero` en `api_gestion/Program.cs`: vencido es vencido (D82) |
| **2** | El 401 sin cuerpo | ASP.NET responde el 401 vacío y el front no tiene qué mostrar | `OnChallenge` escribe `{ mensaje, detalle }` en castellano, y dice si el token venció |
| **3** | `UseAuthentication` después de `UseAuthorization` | La autorización no sabe quién pregunta | `Program.cs` los llama en ese orden: primero autenticar, después autorizar |
| **4** | Una ruta que no está en la tabla | Si fallara abierto, todos entrarían | `sp_verificar_acceso_ruta` empieza en `0` y busca la ruta por su nombre: si no existe, `0` → **403** (falla cerrado) |
| **5** | Una sola sesión para todos | El último que entra le cambia la sesión a los demás | El token vive en `ISession`: una sesión por navegador (cookie `gestion.sesion`), nunca en un `static` ni en un singleton |

**Los que dejan el sistema menos seguro de lo que parece son el 1, el 3 y el 5:** funcionan, y por eso nadie los mira.

## 8. Dos decisiones del front del profesor que aquí son distintas, y por qué

El front del profesor es **Blazor Server**; el nuestro es **Razor Pages**. Dos de sus decisiones dependen de eso:

| En el plan del profesor | Por qué allá | Aquí |
|---|---|---|
| **El token se pone a mano, sin `DelegatingHandler`** | En Blazor Server la cadena de handlers de un `HttpClient` se arma una vez y se reutiliza: un handler que dependa de un servicio *scoped* (`EstadoSesion`) puede recibir el scope de **otro circuito**, y con él el token de otra persona | **Sí hay `DelegatingHandler`** (`ManejadorToken`), y es seguro porque **no recibe ningún servicio scoped**: lee la sesión de la petición **actual** con `IHttpContextAccessor` en el momento de enviar (el `HttpContext` viaja con la petición). Nunca ve la sesión de otro navegador. Con un solo sitio que pone el token, ninguna página se puede olvidar de hacerlo |
| **`@rendermode` una vez en `App.razor`, sin dibujo previo** | Si el layout quedara estático, el menú no vería la sesión del circuito | **No aplica:** Razor Pages arma cada página en el servidor, en cada petición, y el layout lee la sesión de esa misma petición. El menú siempre la ve, y recargar con F5 **no** cierra la sesión. Lo que la cierra es Salir, una hora sin actividad, o que el token venza (60 minutos después de entrar): entonces el front pide iniciar sesión otra vez |

## 9. Lo que este plan deja FUERA, a propósito

| | Por qué |
|---|---|
| **Refrescar el token** | Con una hora de duración, volver a iniciar sesión alcanza. Un *refresh token* trae su propio problema: cómo se revoca |
| **Recuperar la contraseña por correo** | Hace falta un servidor de correo; no lo pide el curso |
| **Segundo factor** | Ídem |
| **Revocar un token** | **No se puede**, y es una propiedad del diseño: un JWT está firmado y ya salió; solo lo apaga el vencimiento, por eso dura **60 minutos**. Lo que sí surte efecto en la siguiente petición: quitarle un permiso al rol (D73) o retirar al usuario (`sp_verificar_acceso_ruta` exige `activo = 1`, y su token deja de abrir cualquier pantalla) |
| **Permisos por operación** (leer sí, borrar no, tabla por tabla) | La v3 protege **por interfaz**, que es lo que piden los diez criterios, más una sola ruta para escribir, `datos.modificar` (D70) |
