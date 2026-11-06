# Quickstart y verificación — Gestión Profesoral v2

> Documento 7 de 8 del spec kit de la v2. Cómo levantar el sistema y comprobar los 12 criterios de `VERSION_2.md`
> del curso, más la **regresión de la v1**.
> Los comandos se escriben en una terminal (PowerShell, Git Bash, Linux o Mac) dentro de la carpeta del proyecto.

---

## 1. Levantar

```bash
# 1. La primera vez: crear el archivo de secretos a partir del ejemplo
cp .env.example .env              # en PowerShell: copy .env.example .env
#    (abra .env y ponga una contraseña con mayúsculas, minúsculas, números y un símbolo)

# 2. Arranque limpio (borra la base anterior: la contraseña de sa se fija al crear el volumen)
docker compose down -v
docker compose up -d --build      # la primera vez tarda varios minutos
```

**Espere a que `sqlserver-init` esté en `Exited (0)`** (`docker compose ps -a`). Si queda en `Exited (1)`, un
script SQL falló: `docker compose logs sqlserver-init` dice cuál y en qué línea.

| Qué | Dirección |
|---|---|
| Front | http://localhost:8010 |
| Swagger de la API | http://localhost:8074 |
| SQL Server (SSMS / Azure Data Studio) | `localhost,11472` · usuario `sa` · la contraseña de su `.env` · base `gestion_local` |

Atajo para consultar la base (PowerShell: reemplace `$CLAVE` por su contraseña):

```bash
sql() { docker compose exec sqlserver /opt/mssql-tools18/bin/sqlcmd -C -S localhost -U sa -P "$CLAVE" -d gestion_local -Q "$1"; }
```

---

## 2. Objetos de la base

```bash
sql "SELECT COUNT(*) AS procedimientos FROM sys.procedures WHERE name LIKE 'sp[_]%'"   # Esperado: 79
sql "SELECT COUNT(*) AS vistas FROM sys.views WHERE name LIKE 'vw[_]%'"                 # Esperado: 11
sql "SELECT name FROM sys.triggers"                    # Esperado: trg_contar_estudios y trg_sello_docente
sql "SELECT COL_LENGTH('docente','total_estudios')"    # Esperado: 4 (la columna existe)
```

---

## 3. La colección de pruebas (criterios 3, 4, 6, 7, 8, 10 y 14 desde la API)

1. En Postman: **Import** → `postman/gestion_profesoral_v2.postman_collection.json`.
2. Clic derecho en la colección → **Run collection** → **Run**. Las carpetas van en orden (0 a 6).
3. **Esperado: todas las pruebas en verde.** Las peticiones que empiezan por «FALLA:» tienen que responder
   404, 409 o 422 con `mensaje` en castellano — **ninguna 500**.

Las claves son aleatorias, así que la colección se puede correr varias veces sin borrar la base.

---

## 4. Los 12 criterios de `VERSION_2.md`, por la interfaz

| # | Criterio | Cómo se comprueba | Esperado |
|---|---|---|---|
| 1 | Tablas de la v1 con CRUD e interfaz | Abrir Términos clave, Líneas de investigación, Programas y Redes: crear, editar, eliminar. Áreas de conocimiento: ver las 218 | Todo funciona como en la v1 |
| 2 | Tablas de la v2 con CRUD e interfaz | **Docentes** → Nuevo docente → Ver detalle; en cada sección agregar, editar y retirar. En un estudio, «Apoyo, beca y áreas» | Las 11 tablas se operan desde pantallas |
| 3 | Todo por procedimientos | `grep -riw "select\|insert\|update\|delete" api_gestion/Repositorios` | Sin resultados |
| 4 | Un disparador funciona | En el detalle de un docente, agregar 3 estudios en un envío; mirar «Estudios activos». Retirar uno | Sube a 3 y baja a 2 **sin que la API envíe el número** |
| 5 | Desplegables cargados de la API | Formularios de docente (línea), programas, redes, intereses y áreas | Muestran nombres, no códigos; solo activos |
| 6 | Clave opcional «(ninguna)» → `null` | Crear un docente con línea «(ninguna)» | Se crea; la lista muestra «(ninguna)» |
| 7 | Padre inexistente → 409 | Colección, carpeta 2: «FALLA: estudio para un docente inexistente» | `409` en castellano |
| 8 | Puentes con dos claves; repetir → 409 | Asignar el mismo programa dos veces al docente | La segunda vez: «El docente … ya tiene asignado el programa …» |
| 9 | Detalle desde el maestro | Docentes → Ver detalle | Todo el detalle en la misma pantalla |
| 10 | Borrado lógico filtra | Retirar un estudio; luego `sql "SELECT id, activo FROM estudios_realizados"` | Desaparece de la pantalla y sigue en la base con `activo = 0` |
| 11 | Sin jerga | Recorrer las pantallas y provocar errores | Nunca aparece «PUT», «PATCH», «422» ni «FK» |
| 12 | Regresión de la v1 | `docs/spec_kit/versiones/v1_programa/7_quickstart.md` (con el puerto 11472 y la contraseña del `.env`) | Pasa completa |

Pruebas adicionales de esta spec:

| Criterio | Cómo | Esperado |
|---|---|---|
| CA-V2-13 Todo o nada | En el detalle, tres renglones de estudios con la fecha del tercero vacía, o dos con el mismo id | No se guarda **ninguno** y el error dice el renglón |
| CA-V2-14 Retiro bloqueado | Retirar un docente que tiene estudios | «No se puede retirar: tiene N registros activos en su detalle…» |
| R3 Reactivación | Retirar una vinculación a programa y volver a asignarla | Vuelve a aparecer con los datos nuevos |

---

## 5. Problemas comunes

| Síntoma | Causa y solución |
|---|---|
| `sqlserver` no arranca | La contraseña del `.env` es débil. Cámbiela y `docker compose down -v` + `up -d` |
| La API dice «Login failed for user 'sa'» | El volumen se creó con otra contraseña: `docker compose down -v` |
| Cambié un procedimiento y no se nota | `docker compose restart sqlserver-init` (el script de la v2 se aplica en cada arranque) |
| Un desplegable dice «Aún no hay…» | Cree primero el padre (programa, red, término) con el enlace que muestra |
