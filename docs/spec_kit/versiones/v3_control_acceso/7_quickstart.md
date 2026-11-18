# Quickstart y verificación — Gestión Profesoral v3

> Documento 7 de 8 del spec kit de la v3. Cómo levantar el sistema y comprobar los criterios de `2_spec.md` §7.
> Los comandos se escriben en Git Bash dentro de la carpeta del proyecto.

---

## 1. Levantar

```bash
# 1. Agregar la clave de firma del token al .env (UNA vez). Sin ella la API no arranca (CA-V3-15).
#    Abra .env y agregue una línea con 32 caracteres o más, por ejemplo:
#    JWT_KEY=EstaEsLaClaveDeFirmaDelEquipoGestion2026XYZ
grep JWT_KEY .env || echo "FALTA JWT_KEY en .env"

# 2. Arranque limpio (la v3 trae paquetes nuevos y un script SQL nuevo)
docker compose down -v
docker compose up -d --build
docker compose ps -a        # sqlserver-init en «Exited (0)», api y front en «Up»
```

| Qué | Dirección |
|---|---|
| Front | http://localhost:8010 |
| Swagger de la API | http://localhost:8074 |
| SQL Server | `localhost,11472` · usuario `sa` · la contraseña de su `.env` · base `gestion_local` |

**Usuarios de prueba**

| Correo | Contraseña | Rol | Qué ve |
|---|---|---|---|
| `admin@gestion.edu.co` | `admin123` | Administrador | Todo, incluida la Administración |
| `editor@gestion.edu.co` | `editor123` | Editor | El módulo, y puede crear, editar y retirar |
| `consulta@gestion.edu.co` | `consulta123` | Consulta | El módulo, solo para mirar |

**CA-V3-15:** quite `JWT_KEY` del `.env`, `docker compose up -d api-gestion` y `docker compose logs api-gestion` → dice que falta la clave.
Vuelva a ponerla.

---

## 2. Colección de Postman (CA-V3-02 a 05, 07, 09 a 14)

Importe `postman/gestion_profesoral_v3.postman_collection.json` → clic derecho en la colección → **Run collection** →
**Run**. Todo en verde. La carpeta **A** inicia sesión con los tres roles; las carpetas **0 a 8** son la regresión de la
v1 y la v2 con el token de administrador (CA-V3-10); **9, 10 y 11** son el control de acceso.

---

### 2.1 La regresión de la v1 con token (CA-V3-10)

Los `curl` del quickstart de la v1 ahora necesitan el token. Se saca una vez y se agrega la cabecera:

```bash
TOKEN=$(curl -s -X POST http://localhost:8074/api/sesion -H "Content-Type: application/json" \
  -d '{"email":"admin@gestion.edu.co","contrasena":"admin123"}' | sed 's/.*"token":"\([^"]*\)".*/\1/')

curl -s -H "Authorization: Bearer $TOKEN" http://localhost:8074/api/area-conocimiento | grep -o '"id"' | wc -l   # 218
curl -i -H "Authorization: Bearer $TOKEN" http://localhost:8074/api/area-conocimiento/1A01                       # 200
curl -i http://localhost:8074/api/area-conocimiento/1A01                                                          # 401 (sin token)
```

El resto de los `curl` de `v1_programa/7_quickstart.md` se corre igual, con `-H "Authorization: Bearer $TOKEN"`.

---

## 3. La contraseña en la base (CA-V3-01)

```bash
CLAVE='la contraseña de sa de su .env'
docker compose exec sqlserver /opt/mssql-tools18/bin/sqlcmd -C -S localhost -U sa -P "$CLAVE" -d gestion_local \
  -Q "SELECT username, LEFT(password, 7) AS inicio, LEN(password) AS largo FROM usuario"
#   todas empiezan por $2a$12$ y miden 60: es un hash bcrypt, no la contraseña

docker compose exec sqlserver /opt/mssql-tools18/bin/sqlcmd -C -S localhost -U sa -P "$CLAVE" -d gestion_local \
  -Q "SELECT COUNT(*) AS en_claro FROM usuario WHERE password NOT LIKE '\$2_\$%' OR LEN(password) <> 60"
#   en_claro = 0
```

---

## 4. En el front (CA-V3-07, 08, 09, 11)

1. Abra http://localhost:8010 → lo lleva a **Iniciar sesión**.
2. Entre como **consulta**: el menú NO tiene «Administración»; las tablas se ven sin «Nuevo», «Editar» ni «Retirar» y
   arriba sale «modo consulta» (CA-V3-08 y 11).
3. **CA-V3-09:** escriba a mano `http://localhost:8010/Usuarios` → «No tiene permiso», con el menú, el aviso de la API
   «Su rol no tiene permiso para esta operación», la línea «permiso requerido: interfaz.usuarios» y **cero datos**. La
   respuesta es un **403** de verdad: se ve con F12 → Red; en la pantalla no sale ningún número (D88). La página no revisa nada: le pidió la lista a la API y fue la API la que respondió 403. Lo mismo con
   `/Roles`, `/Roles/Crear` y `/Permisos`.
4. Abra otra ventana **de incógnito**, entre como **admin** → Administración → **Permisos por rol** → elija **Consulta**
   → **Quitar** en `interfaz.redes`.
5. **CA-V3-07:** vuelva a la ventana de consulta **sin cerrar sesión** y recargue: «Redes» ya no está en el menú, y si
   escribe `/Redes` a mano → «No tiene permiso». Devuélvale el permiso desde la ventana del admin.
6. Entre como **editor**: ve el módulo con los botones de escritura, pero no «Administración».
7. Clic en **Salir** → vuelve a «Iniciar sesión». Escriba `http://localhost:8010/Docentes` → lo lleva a iniciar sesión.
8. **La API apagada:** con sesión iniciada, `docker compose stop api-gestion` y abra **Programas** → la página sale con
   el aviso «No se pudo conectar con la API de gestión.». Abra **Estudios realizados → Nuevo** (un formulario que
   carga la lista de docentes) → página «Sin conexión con la API», en castellano, nunca la página técnica.
   `docker compose start api-gestion` y todo vuelve.

---

## 5. En el código (CA-V3-06 y RNF17)

```bash
# No hay SQL escrito en C# (tampoco el JOIN de permisos): no debe imprimir nada
grep -riwn "select\|insert\|update\|delete\|join" api_gestion/Repositorios

# El permiso lo resuelve el procedimiento
grep -n "sp_verificar_acceso_ruta" api_gestion/Repositorios/RepositorioAcceso.cs

# Cada controlador del módulo exige permiso para entrar y para escribir
grep -c "ExigePermiso" api_gestion/Controllers/*.cs

# Ninguna clase con [AllowAnonymous]: todos deben dar 0 (solo el método de entrar lo lleva, D87)
grep -c "^\[AllowAnonymous\]" api_gestion/Controllers/*.cs
```

---

## 6. En Swagger (para la sustentación)

1. http://localhost:8074 → `POST /api/sesion` → **Try it out** → `{ "email": "admin@gestion.edu.co", "contrasena": "admin123" }` → copie el `token`.
2. **Authorize** (arriba a la derecha) → pegue **solo** el token → Authorize → Close.
3. `GET /api/usuario` → `200` con la lista, sin contraseñas.
4. Repita 1 y 2 con **consulta** → `GET /api/usuario` → `403` «Su rol no tiene permiso…».
5. **Authorize** → **Logout** → `GET /api/usuario` → `401` «Debe iniciar sesión…».
6. Pegue el token en https://jwt.io: se lee el correo y los roles (está **firmado, no cifrado**), y **no** hay permisos.

---

## 7. Si algo falla

| Síntoma | Qué pasa |
|---|---|
| Iniciar sesión responde 401 con las contraseñas de la tabla | La base no tiene los usuarios con hash: `docker compose down -v` y `docker compose up -d --build` |
| La API arranca y se apaga sola | Falta `JWT_KEY` en el `.env` o tiene menos de 32 caracteres; `docker compose logs api-gestion` lo dice |
| Todo responde 401, incluso con un token bueno | En Postman/`curl` falta la palabra `Bearer` y el espacio antes del token (en Swagger, al revés: se pega **solo** el token). O en `api_gestion/Program.cs` quedó `UseAuthorization()` antes de `UseAuthentication()`: la autorización no sabe quién pregunta |
| Todo responde 200, incluso sin token | Faltan `[Authorize]` y la política de respaldo (`FallbackPolicy`) de `api_gestion/Program.cs` (D74) |
| Un token vencido sigue entrando unos minutos | Falta `ClockSkew = TimeSpan.Zero` (D82) |
| El 401 llega con el cuerpo vacío | Falta el `OnChallenge` |
| Un rol sin permiso recibe 200 | Falta `[ExigePermiso]` en ese controlador: `grep -c "ExigePermiso" api_gestion/Controllers/*.cs` |
| Todo responde 403, incluso el administrador | El nombre de la ruta del atributo no está en la tabla `ruta`: `SELECT nombre FROM ruta` |
| El front muestra «No se pudo conectar con la API de gestión» | La API está apagada o reiniciando: `docker compose ps -a` y `docker compose logs api-gestion` |
