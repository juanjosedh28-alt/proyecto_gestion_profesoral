# Registros de Decisión (ADR) — v1 Programa

> Documento 4 de 8 del spec kit raíz · Lectura opcional (contexto de por qué el plan es como es).

## D1 — Borrado lógico sobre borrado físico

Alternativas: (a) DELETE físico `DELETE FROM programa`, (b) borrado lógico `UPDATE programa SET activo = 0`. Decisión: opción (b) para preservar la integridad referencial e historial académico.

## D2 — Docker Compose para TODO, incluso la BD

Alternativas: SQL Server instalado en cada PC de estudiante (heterogéneo, imposible de soportar), una sola BD compartida en la nube (sin trabajo offline, un punto de falla para todo el curso). Decisión: todo en contenedores; el único requisito es Docker Desktop. La primera descarga es grande UNA vez; después arranca en segundos.

## D3 — Un solo motor: SQL Server

El script `gestion_profesoral.sql` declara que SQL Server es el único motor del proyecto. Mantener varios scripts equivalentes a mano multiplicaría el trabajo sin aportar a la v1. Costo aceptado: la API queda atada a SQL Server (Entity Framework Core permitiría cambiar de proveedor más adelante).

## D4 — Puerto de BD desplazado (11443)

Muchos estudiantes ya tienen un SQL Server local en el puerto estándar. Publicar 1433 chocaría. Dentro de la red de compose se usa el estándar (`sqlserver:1433`), porque los contenedores no chocan entre sí.

## D5 — `sqlserver-init` como contenedor efímero

La imagen de SQL Server NO tiene el mecanismo `/docker-entrypoint-initdb.d`. Decisión: un servicio auxiliar con la misma imagen (trae `sqlcmd`), `depends_on: service_healthy`, que verifica en `sys.databases` si `gestion_local` existe y solo entonces la crea y la puebla. `restart: "no"`: corre y muere. Alternativa rechazada: script dentro del mismo contenedor `sqlserver` con `&` (frágil, mezcla responsabilidades).

## D6 — Sin `depends_on` de la API hacia la BD

El `DbContext` abre la conexión en la primera petición, así que la API tolera que la BD tarde. Quitar la dependencia acelera el arranque y evita el problema clásico de "healthy pero aún cargando datos".

## D7 — Código montado como volumen + recarga en caliente

`./api_gestion:/app` + `dotnet watch run`: guardar un archivo recarga la app sin rebuild. Rebuild (`--build`) solo cuando cambian paquetes NuGet o el Dockerfile. Es la diferencia entre iterar en 2 segundos o en 2 minutos de clase.

## D8 — Cadena de conexión por variable de entorno

La cadena de `gestion_local` se entrega con `ConnectionStrings__GestionLocal` en `docker-compose.yml`, nunca escrita en el código. Cambiar de servidor o credenciales no exige recompilar.

## D9 — Volúmenes nombrados y `down -v` como reset oficial

Los datos sobreviven a `down` y reinicios (aprendizaje de persistencia). El "botón de pánico" documentado es `down -v` + `up -d`: el script solo corre si la BD no existe, así que también es la vía para aplicar cambios de esquema.

## D10 — Credenciales públicas a propósito

`sa/Paradigmas123!` está en el repo deliberadamente: es didáctica y el entorno jamás va a producción. Cumple la política de complejidad mínima de SQL Server.

## D11 — Las 19 tablas se crean completas aunque la v1 use pocas

El script crea las 19 tablas con sus claves foráneas, porque la base es infraestructura dada. La v1 solo expone las 7 entidades sin dependencias, pero el esquema ya queda listo para las siguientes versiones.

## D12 — `programa` arranca vacía

El Excel de origen tiene 191 filas, pero no trae seis columnas obligatorias (`nivel`, `fecha_creacion`, `numero_cohortes`, `cant_graduados`, `fecha_actualizacion` y `ciudad`). Rellenarlas sería inventar datos. Decisión: la tabla queda con 0 filas y la verificación de la v1 empieza por el `204` del listado vacío.

## D13 — `area_conocimiento.id` como `VARCHAR(6)`

Los datos oficiales son códigos alfanuméricos (`1A01`), no números. Por eso la clave pasó de `INT` a `VARCHAR(6)`, y esto afecta a `estudio_ac.area_conocimiento`, que la referencia.