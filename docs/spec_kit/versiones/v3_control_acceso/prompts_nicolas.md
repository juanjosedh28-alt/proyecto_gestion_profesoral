# Prompts de Nicolás — Versión 3

> Integrante: **Nicolás** · Rama: `rama-nicolas-v3` · Herramienta: Claude (chat con herramientas de código), noviembre de 2026.
> Aquí están los prompts **tal como los escribí**, lo que hizo la IA con cada uno y lo que hubo que corregir.

---

## Prompt 1 — Lo que pide el profesor para la v3

> (mensaje del profesor: sustentación de la v3 «control de acceso» el sábado 21 de noviembre; seguir en detalle
> `CONCEPTOS_CONTROL_DE_ACCESO.md` y `FLUJO_CONTROL_DE_ACCESO.md`; la v4 no se sustenta)
> esta es la 3ra entrega, puedes editarlo y completarlo porfa

**Qué hizo la IA:** leyó los dos documentos del profesor, su spec de la v3 (los 10 criterios) y las guías de
verificación con Swagger y Postman. Comparó con nuestro esquema: solo teníamos `usuario`, `rol` y `rol_usuario`,
sin tabla de permisos, así que propuso agregar `ruta` y `ruta_rol` (D65) para que quitar un permiso funcione sin volver
a entrar (criterio 7). Definió tres roles (D66) y la ruta `datos.modificar` para que Consulta no escriba (D67).

Hizo la v3 en commits separados: base de datos, sesión con JWT, permisos en los 16 controladores, administración en la
API, el front (sesión, inicio de sesión, menú, administración), la colección de Postman y este spec kit.

**Qué hubo que corregir durante el trabajo:**

| Problema | Corrección |
|---|---|
| `sp_rutas_permitidas` ordenaba por una columna que no estaba en el `SELECT DISTINCT` (SQL Server lo rechaza) | Ordenar por el alias `ruta` |
| La página de error fallaba cuando el 403 venía de un formulario (POST) | Leer el código de la ruta y aceptar GET y POST |
| Las casillas de roles del formulario de usuario no llegaban al servidor por el nombre del campo | Nombrarlas `RolIds`, como los demás campos del formulario |
| El inicio de sesión estaba en `/api/login` y la guía del profesor usa `/api/sesion` | Se cambió a `POST /api/sesion` (D73) |
| Un comentario con la palabra JOIN hacía fallar el `grep` de «no hay SQL en C#» | Se reescribió el comentario |

**Lo que la IA NO pudo hacer:** compilar ni levantar el proyecto donde trabaja (no tiene acceso a NuGet), ni subir a
GitHub. Por eso **yo** lo levanté con Docker, corrí la colección y subí la rama. Si apareció algún error al compilar,
está anotado abajo.

## Prompt 2 — Verificación

> (anotar aquí los errores que salieron al correr `docker compose up -d --build` y la colección, y cómo se arreglaron)
