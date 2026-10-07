# Quickstart y verificación — Gestión Profesoral v1

> Documento 7 de 8 del spec kit raíz. Validación del sistema completo (BD + API + front).
> Los comandos `curl` se ejecutan en Git Bash, PowerShell (`curl.exe`), Linux o Mac. En `cmd.exe` las comillas del JSON fallan: usa Swagger (`/swagger`) en ese caso.

---

## 1. Levantar

```bash
git clone <url-del-repositorio>
cd gestion_profesoral
docker compose up -d --build     # primera vez: varios minutos
<<<<<<< HEAD
curl http://localhost:8074/api/programa
=======
>>>>>>> 2eca3e1 (Modificacion y mejoras en el spec kit)
```

Direcciones:

| Qué | URL |
|---|---|
| Front | http://localhost:8010 |
| Swagger de la API | http://localhost:8074/swagger |
| SQL Server (SSMS / Azure Data Studio) | `localhost,11443` · usuario `sa` · clave `Paradigmas123!` · base `gestion_local` |

---

## 2. Verificación en 9 pasos

### Paso 1 — Contenedores (CA-01)

```bash
docker compose ps -a
# Esperado: sqlserver, api-gestion y front-razor corriendo; sqlserver-init con Exited (0)
```

**Espera a que `sqlserver-init` esté en `Exited (0)` antes de seguir** (si pruebas antes, la API reintenta pero puede tardar).

### Paso 2 — Base de datos (Fase 0)

```bash
<<<<<<< HEAD
curl -i http://localhost:8074/api/programa
=======
docker compose exec sqlserver /opt/mssql-tools18/bin/sqlcmd -C -S localhost -U sa -P 'Paradigmas123!' -d gestion_local \
  -Q "SELECT (SELECT COUNT(*) FROM area_conocimiento) AS areas, (SELECT COUNT(*) FROM programa) AS programas"
# Esperado: areas = 218, programas = 0
>>>>>>> 2eca3e1 (Modificacion y mejoras en el spec kit)
```

### Paso 3 — Áreas de conocimiento (CA-02)

```bash
curl -s http://localhost:8074/api/area-conocimiento | grep -o '"id"' | wc -l      # Esperado: 218
curl -i http://localhost:8074/api/area-conocimiento/1A01                           # Esperado: 200
curl -i http://localhost:8074/api/area-conocimiento/ZZZZ                           # Esperado: 404 con {"mensaje":...,"detalle":[]}
```

### Paso 4 — Programa, la vertical de referencia (CA-03, CA-04, CA-05, CA-06)

```bash
# Lista vacía -> 200 con []
curl -i http://localhost:8074/api/programa

# Crear -> 201 (con cabecera Location)
curl -i -X POST http://localhost:8074/api/programa \
  -H "Content-Type: application/json" \
  -d '{"id":1,"nombre":"Ingeniería de Sistemas","tipo":"Pregrado","nivel":"Profesional","fecha_creacion":"2010-02-15","numero_cohortes":"20","cant_graduados":"350","ciudad":"Medellín","facultad":1}'

# Mismo id otra vez -> 409
curl -i -X POST http://localhost:8074/api/programa \
  -H "Content-Type: application/json" \
  -d '{"id":1,"nombre":"Otro","tipo":"Pregrado","nivel":"Profesional","fecha_creacion":"2010-02-15","numero_cohortes":"20","cant_graduados":"350","ciudad":"Medellín","facultad":1}'

# Campos faltantes -> 422 con detalle en español
curl -i -X POST http://localhost:8074/api/programa \
  -H "Content-Type: application/json" -d '{"id":2,"nombre":"Incompleto"}'

# Consultar -> 200 ; inexistente -> 404
curl -i http://localhost:8074/api/programa/1
curl -i http://localhost:8074/api/programa/99

# PUT con id distinto en el cuerpo -> 422
curl -i -X PUT http://localhost:8074/api/programa/1 \
  -H "Content-Type: application/json" \
  -d '{"id":2,"nombre":"Ing. de Sistemas","tipo":"Pregrado","nivel":"Profesional","fecha_creacion":"2010-02-15","numero_cohortes":"21","cant_graduados":"360","ciudad":"Medellín","facultad":1}'

# PUT correcto -> 200
curl -i -X PUT http://localhost:8074/api/programa/1 \
  -H "Content-Type: application/json" \
  -d '{"id":1,"nombre":"Ing. de Sistemas","tipo":"Pregrado","nivel":"Profesional","fecha_creacion":"2010-02-15","numero_cohortes":"21","cant_graduados":"360","ciudad":"Medellín","facultad":1}'

# PATCH parcial -> 200 con fecha_cierre modificada
curl -i -X PATCH http://localhost:8074/api/programa/1 \
  -H "Content-Type: application/json" -d '{"fecha_cierre":"2030-12-31"}'

# PATCH vacío -> 422
curl -i -X PATCH http://localhost:8074/api/programa/1 -H "Content-Type: application/json" -d '{}'

# Borrado lógico -> 204 ; luego el listado vuelve a []
curl -i -X DELETE http://localhost:8074/api/programa/1
curl -i http://localhost:8074/api/programa
```

La fila sigue en la BD con `activo = 0`:

```bash
docker compose exec sqlserver /opt/mssql-tools18/bin/sqlcmd -C -S localhost -U sa -P 'Paradigmas123!' -d gestion_local \
  -Q "SELECT id, nombre, activo FROM programa"
# Esperado: 1 | Ing. de Sistemas | 0
```

Recrear el id `1` ahora debe dar `409` (limitación conocida: no hay reactivación).

### Paso 5 — Las otras 3 entidades editables (CA-05)

```bash
# Término clave
curl -i -X POST http://localhost:8074/api/termino-clave -H "Content-Type: application/json" \
  -d '{"termino":"Inteligencia artificial","termino_ingles":"Artificial intelligence"}'
curl -i "http://localhost:8074/api/termino-clave/Inteligencia%20artificial"
# El mismo término otra vez -> 409
curl -i -X POST http://localhost:8074/api/termino-clave -H "Content-Type: application/json" \
  -d '{"termino":"Inteligencia artificial"}'

# Línea de investigación (el id lo genera la BD)
curl -i -X POST http://localhost:8074/api/linea-investigacion -H "Content-Type: application/json" \
  -d '{"nombre":"Ingeniería de software","descripcion":"Procesos, calidad y arquitectura."}'

# Red (el idr lo envía el cliente)
curl -i -X POST http://localhost:8074/api/red -H "Content-Type: application/json" \
  -d '{"idr":1,"nombre":"RENATA","url":"https://www.renata.edu.co","pais":"Colombia"}'
```

Prueba un `DELETE` por entidad (`/api/termino-clave/{termino}`, `/api/linea-investigacion/1`, `/api/red/1`): todos `204`, y el `GET` siguiente ya no devuelve el registro.

### Paso 6 — Swagger (CA-07)

Abre http://localhost:8074/swagger: deben verse los endpoints de las 5 entidades.

### Paso 7 — Front (CA-08, CA-09)

Abre http://localhost:8010 y recorre esta lista (usa datos nuevos, porque los de los pasos anteriores quedaron inactivos):

| Entidad | Comprobar |
|---|---|
| Áreas de conocimiento | Se listan 218; el filtro (ej. "matem") reduce la tabla; el detalle de una fila abre; no hay botones de crear/editar/eliminar |
| Programas | Lista vacía con "No hay registros para mostrar"; crear con fechas del selector; ver la fila; editar (el `id` aparece bloqueado); eliminar con confirmación; repetir un `id` inactivo muestra el error `409` en español dentro del formulario |
| Términos clave | Crear, editar y eliminar un término con tilde y espacio (ej. "Aprendizaje automático") |
| Líneas de investigación | Crear, editar, eliminar |
| Redes | Crear con una URL sin `http` → error de validación; con URL correcta → éxito |

Resiliencia del front: `docker compose stop api-gestion` y recarga `/Programas` → debe verse "No se pudo conectar con la API de gestión." sin pantalla de error. Luego `docker compose start api-gestion`.

### Paso 8 — Persistencia y reset (CA-10)

- **Persistencia:** crear un registro → `docker compose down` → `docker compose up -d` → sigue ahí.
- **Reset:** `docker compose down -v` → `docker compose up -d` → datos originales (218 áreas, resto vacío).

### Paso 9 — Pruebas de capas (CA-11)

Sin necesidad de instalar .NET en el PC:

```bash
docker run --rm -v "$(pwd):/src" -w /src mcr.microsoft.com/dotnet/sdk:10.0 dotnet test pruebas/api_gestion.Pruebas
# Esperado: todas las pruebas aprobadas
```

(En PowerShell usa `${PWD}` en lugar de `$(pwd)`.)

---

## 3. Problemas frecuentes

| Síntoma | Diagnóstico |
|---|---|
| `curl` no responde en 8074 o 8010 | `docker compose ps -a` + `docker compose logs api-gestion` / `front-razor` (la primera compilación de `dotnet watch` tarda ~1 min) |
| La API responde `500` justo al arrancar | La BD aún se está creando: esperar `sqlserver-init` en `Exited (0)` y reintentar |
| SQL Server nunca queda sano | Necesita ~2 GB de RAM; cerrar otras aplicaciones pesadas |
| Puerto ocupado (8010/8074/11443) | Cerrar el otro programa o cambiar el mapeo en compose |
| Cambié el script SQL y no pasa nada | Solo corre si la BD no existe: `down -v` + `up -d` |
| Guardé un archivo y no se recarga | Confirmar `DOTNET_USE_POLLING_FILE_WATCHER=true` en compose; revisar `docker compose logs -f api-gestion` |
| Error de restauración tras correr las pruebas | Borrar `api_gestion/obj` y `front_razor/obj` y reiniciar el contenedor |
| `/swagger` da 404 | Falta `ASPNETCORE_ENVIRONMENT=Development` en el servicio `api-gestion` |
| El front muestra "No se pudo conectar con la API" | Revisar que `ApiGestion__BaseUrl` sea `http://api-gestion:8074` y que `api-gestion` esté corriendo |
| Todo roto | `docker compose down -v` + `docker compose up -d --build` |
