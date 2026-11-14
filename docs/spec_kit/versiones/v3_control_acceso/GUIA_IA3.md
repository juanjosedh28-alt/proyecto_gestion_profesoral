# Guía de IA — Versión 3

> Cómo se pasó del spec kit al código de la v3 usando una IA, y cómo **volver a hacerlo**.
> Los prompts reales de cada integrante, con lo que hubo que corregir, están en `prompts_<integrante>.md` de esta carpeta.

---

## 1. Qué tiene que leer la IA (en este orden)

1. `docs/spec_kit/1_constitution.md` — **incluidas las Enmiendas de la v2 y de la v3** al final (artículos 18 a 21).
2. `CONCEPTOS_CONTROL_DE_ACCESO.md` y `FLUJO_CONTROL_DE_ACCESO.md` del proyecto del profesor: la teoría y los 16 pasos.
3. `2_spec.md` de esta carpeta — qué se construye y los 15 criterios.
4. `3_plan.md` y `4_research.md` — cómo y por qué (D65 a D76).
5. `5_data_model.md` y `6_contracts.md` — tablas, rutas sembradas, procedimientos y contratos HTTP.
6. El código de la v2 (`api_gestion/`, `front_razor/`, `db/`), para seguir su estilo.

## 2. Prompt para reproducir la v3

```text
Eres el desarrollador del proyecto "Gestión Profesoral" (ASP.NET Core 10 + Dapper + SQL Server en Docker,
front en Razor Pages). Adjunto la constitución con sus enmiendas, el spec kit de la v3 y el código de la v2.
La v2 está cerrada: no cambies sus rutas, DTO ni pantallas.

Construye el control de acceso en este orden, un commit por paso:
1. db/v3_control_acceso.sql (idempotente): tablas ruta y ruta_rol, roles Administrador/Editor/Consulta, las 11
   rutas de 5_data_model.md §2, permisos sembrados solo si ruta_rol está vacía, usuarios de prueba con HASH bcrypt
   (nunca la contraseña en claro) y los procedimientos de 3_plan.md §4. Agrégalo a db/init.sh.
2. API: POST /api/sesion con bcrypt (mismo 401 para correo y contraseña) y token JWT con sub, name y role.
   La clave sale de Jwt__Key (.env); si falta o mide menos de 32, la API no arranca.
3. API: [ExigePermiso("ruta")] (IAsyncAuthorizationFilter) que llama a sp_verificar_acceso_ruta en cada
   petición. Ponlo en los 16 controladores (interfaz.x en la clase, datos.modificar en cada POST/PUT/DELETE).
   Política de respaldo RequireAuthenticatedUser. NO metas permisos en el token. NO escribas SQL en C#.
4. API: GET /api/permisos/mios y el CRUD de usuario, rol-usuario, rol, ruta y ruta-rol (6_contracts.md).
5. Front: token en la sesión del servidor, DelegatingHandler que pone Bearer, filtro que exige sesión y
   refresca las rutas en cada GET, 401 → iniciar sesión, 403 → página "No tiene permiso". Menú según las
   rutas; sin datos.modificar, ocultar botones de escritura. NINGUNA página revisa permisos por su cuenta.
6. Front: páginas de Usuarios, Roles y Permisos por rol.
7. Colección de Postman v3: carpeta de inicio de sesión con los tres roles, la regresión de la v2 con token,
   y las pruebas de 401, 403 y del criterio 7.
Comenta cada archivo nuevo en español: qué hace y por qué.
```

## 3. Qué revisar en lo que entregue la IA

| Revisar | Por qué |
|---|---|
| ¿Hay contraseñas en claro en el SQL o en el código? | Criterio 1 |
| ¿Los permisos van en el token? | Rompe el criterio 7 |
| ¿Alguna página del front decide permisos? | Esconde el problema del criterio 9 |
| ¿El `grep` de SQL sobre `Repositorios` imprime algo? | Constitución, enmienda al artículo 8 |
| ¿`ruta_rol` se vuelve a sembrar en cada arranque? | Devolvería permisos quitados |
| ¿Puede el administrador dejar el sistema sin administrador? | Regla R15 |
| ¿Compila y levanta con `docker compose up -d --build`? | La IA no siempre puede compilar donde trabaja |
