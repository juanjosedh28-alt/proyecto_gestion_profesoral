# Quickstart y verificación — Gestión Profesoral v2

> Documento 7 de 8 del spec kit de la v2. Validación de lo que la v2 añade (BD + API + front) y **regresión completa de la v1**.
> Los comandos `curl` se ejecutan en Git Bash, PowerShell (`curl.exe`), Linux o Mac. En `cmd.exe` las comillas del JSON fallan: usa Swagger (`/swagger`).
> Los ejemplos usan los docentes `1017245` y `1098332` y los estudios `1`, `2`, `3`.

---

## 1. Levantar

```bash
docker compose down -v           # arranque limpio: la v2 se aplica al crear la base
docker compose up -d --build
```

Direcciones: front http://localhost:8010 · Swagger http://localhost:8074/swagger · SQL Server `localhost,11443` (`sa` / `Paradigmas123!`, base `gestion_local`).

**Espera a que `sqlserver-init` esté en `Exited (0)`** antes de seguir. Si queda en `Exited (1)`, el script de la v2 falló: `docker compose logs sqlserver-init`.

Atajo para las consultas a la base (se usa en varios pasos):

```bash
sql() { docker compose exec sqlserver /opt/mssql-tools18/bin/sqlcmd -C -S localhost -U sa -P 'Paradigmas123!' -d gestion_local -Q "$1"; }
```

---

## 2. Verificación de la v2 en 11 pasos

### Paso 1 — Objetos de la v2 en la base (CA-V2-03, CA-V2-04)

```bash
docker compose ps -a      # sqlserver-init: Exited (0)
sql "SELECT COUNT(*) AS procedimientos FROM sys.procedures WHERE name LIKE 'sp[_]%'"
# Esperado: 61  (53 de los cinco verbos + 8 de lote)
sql "SELECT name FROM sys.triggers WHERE name IN ('trg_contar_estudios','trg_sello_docente')"
# Esperado: los 2 disparadores
sql "SELECT COL_LENGTH('docente','total_estudios') AS columna_total"
# Esperado: 4 (existe la columna)
```

### Paso 2 — Datos de apoyo de la v1 (para tener opciones en los desplegables)

```bash
curl -i -X POST http://localhost:8074/api/linea-investigacion -H "Content-Type: application/json" \
  -d '{"nombre":"Ingeniería de software","descripcion":"Procesos y calidad de software."}'
curl -i -X POST http://localhost:8074/api/programa -H "Content-Type: application/json" \
  -d '{"id":3,"nombre":"Ingeniería de Sistemas","tipo":"Pregrado","nivel":"Profesional","fecha_creacion":"2010-02-15","numero_cohortes":"20","cant_graduados":"350","ciudad":"Medellín","facultad":1}'
curl -i -X POST http://localhost:8074/api/red -H "Content-Type: application/json" \
  -d '{"idr":1,"nombre":"RENATA","url":"https://www.renata.edu.co","pais":"Colombia"}'
curl -i -X POST http://localhost:8074/api/termino-clave -H "Content-Type: application/json" \
  -d '{"termino":"Inteligencia artificial","termino_ingles":"Artificial intelligence"}'
# Esperado: 201 en las cuatro
```

### Paso 3 — Docente: CRUD y clave foránea opcional (CA-V2-02, CA-V2-06)

```bash
# Sin línea principal: la clave opcional manda null -> 201
curl -i -X POST http://localhost:8074/api/docente -H "Content-Type: application/json" -d '{
 "cedula":1017245,"nombres":"Ana","apellidos":"Torres Gómez","genero":"Femenino","cargo":"Profesora titular",
 "fecha_nacimiento":"1985-04-12","correo":"ana.torres@universidad.edu.co","telefono":"3001234567",
 "url_cvlac":"https://scienti.minciencias.gov.co/cvlac/1017245","escalafon":"Titular",
 "perfil":"Investigadora en software.","cat_minciencia":null,"conv_minciencia":"Convocatoria 894 de 2021",
 "nacionalidaad":"Colombiana","linea_investigacion_principal":null}'
# Esperado: 201, con fecha_actualizacion = hoy y total_estudios = 0 (los puso la base)

# Con línea principal (id 1) y otra cédula -> 201
curl -i -X POST http://localhost:8074/api/docente -H "Content-Type: application/json" -d '{
 "cedula":1098332,"nombres":"Carlos","apellidos":"Pérez Mejía","genero":"Masculino","cargo":"Profesor asociado",
 "fecha_nacimiento":"1979-09-01","correo":"carlos.perez@universidad.edu.co","telefono":"3109876543",
 "url_cvlac":"https://scienti.minciencias.gov.co/cvlac/1098332","escalafon":"Asociado",
 "perfil":"Docente de bases de datos.","cat_minciencia":"Junior","conv_minciencia":"Convocatoria 894 de 2021",
 "nacionalidaad":"Colombiana","linea_investigacion_principal":1}'

# Cédula repetida -> 409
curl -i -X POST http://localhost:8074/api/docente -H "Content-Type: application/json" -d '{"cedula":1017245,"nombres":"Otra","apellidos":"Persona","genero":"Femenino","cargo":"X","fecha_nacimiento":"1990-01-01","correo":"a@b.co","telefono":"3000000000","url_cvlac":"https://x.co","escalafon":"X","perfil":"X","conv_minciencia":"X","nacionalidaad":"X"}'

# Línea principal inexistente -> 409
# (mismo cuerpo del primer docente con "cedula":1,"linea_investigacion_principal":999)

# Campos faltantes -> 422 con detalle en español
curl -i -X POST http://localhost:8074/api/docente -H "Content-Type: application/json" -d '{"cedula":2,"nombres":"Incompleto"}'

curl -i http://localhost:8074/api/docente             # 200 con 2 docentes (lista plana)
curl -i http://localhost:8074/api/docente/1017245     # 200
curl -i http://localhost:8074/api/docente/99999       # 404
```

### Paso 4 — Hijo con padre inexistente: 409, nunca 500 (CA-V2-07)

```bash
curl -i -X POST http://localhost:8074/api/estudios-realizados -H "Content-Type: application/json" -d '{
 "id":50,"titulo":"Estudio huérfano","universidad":"UdeA","fecha":"2012-12-15","tipo":"Maestría","ciudad":"Medellín",
 "docente":99999,"ins_acreditada":1,"metodologia":"Presencial","perfil_egresado":"X","pais":"Colombia"}'
# Esperado: 409 con {"mensaje":"No se puede registrar: el docente 99999 no existe o está retirado.","detalle":[]}
# Un 500 aquí es criterio fallado.
```

### Paso 5 — Envío múltiple y disparador contador (CA-V2-04, CA-V2-13)

```bash
# Tres renglones en UNA petición -> 201
curl -i -X POST http://localhost:8074/api/docente/1017245/estudios-realizados -H "Content-Type: application/json" -d '[
 {"id":1,"titulo":"Pregrado en Sistemas","universidad":"UdeA","fecha":"2008-06-20","tipo":"Pregrado","ciudad":"Medellín","ins_acreditada":1,"metodologia":"Presencial","perfil_egresado":"Ingeniero.","pais":"Colombia"},
 {"id":2,"titulo":"Maestría en Ingeniería","universidad":"UdeA","fecha":"2012-12-15","tipo":"Maestría","ciudad":"Medellín","ins_acreditada":1,"metodologia":"Presencial","perfil_egresado":"Investigador.","pais":"Colombia"},
 {"id":3,"titulo":"Doctorado en Informática","universidad":"UPM","fecha":"2018-09-30","tipo":"Doctorado","ciudad":"Madrid","ins_acreditada":1,"metodologia":"Presencial","perfil_egresado":"Doctor.","pais":"España"}]'

# El contador lo puso el DISPARADOR; la API no lo envió -> total_estudios = 3
curl -s http://localhost:8074/api/docente/1017245 | grep -o '"total_estudios":[0-9]*'

# Todo o nada: el renglón 3 sin título -> 422 y NO se guarda ninguno de los 3
curl -i -X POST http://localhost:8074/api/docente/1017245/estudios-realizados -H "Content-Type: application/json" -d '[
 {"id":10,"titulo":"A","universidad":"U","fecha":"2020-01-01","tipo":"Curso","ciudad":"C","ins_acreditada":0,"metodologia":"M","perfil_egresado":"P","pais":"Colombia"},
 {"id":11,"titulo":"B","universidad":"U","fecha":"2020-01-01","tipo":"Curso","ciudad":"C","ins_acreditada":0,"metodologia":"M","perfil_egresado":"P","pais":"Colombia"},
 {"id":12,"universidad":"U","fecha":"2020-01-01","tipo":"Curso","ciudad":"C","ins_acreditada":0,"metodologia":"M","perfil_egresado":"P","pais":"Colombia"}]'
sql "SELECT COUNT(*) AS estudios FROM estudios_realizados WHERE id IN (10,11,12)"   # Esperado: 0

# Id repetido dentro del mismo envío -> 409 con el número de renglón
curl -i -X POST http://localhost:8074/api/docente/1017245/estudios-realizados -H "Content-Type: application/json" -d '[
 {"id":20,"titulo":"A","universidad":"U","fecha":"2020-01-01","tipo":"Curso","ciudad":"C","ins_acreditada":0,"metodologia":"M","perfil_egresado":"P","pais":"Colombia"},
 {"id":20,"titulo":"B","universidad":"U","fecha":"2020-01-01","tipo":"Curso","ciudad":"C","ins_acreditada":0,"metodologia":"M","perfil_egresado":"P","pais":"Colombia"}]'

# Retirar un estudio (es un UPDATE, no un DELETE): el contador baja solo -> 2
curl -i -X DELETE http://localhost:8074/api/estudios-realizados/3
curl -s http://localhost:8074/api/docente/1017245 | grep -o '"total_estudios":[0-9]*'
sql "SELECT id, activo FROM estudios_realizados"      # el 3 sigue ahí con activo = 0
```

### Paso 6 — Disparador sello de modificación

```bash
# Dejamos una fecha vieja saltándonos el disparador (solo para la prueba)
sql "ALTER TABLE docente DISABLE TRIGGER trg_sello_docente; UPDATE docente SET fecha_actualizacion='2000-01-01' WHERE cedula=1098332; ALTER TABLE docente ENABLE TRIGGER trg_sello_docente;"

# Un PUT normal (sin enviar fecha_actualizacion) -> la fecha vuelve a ser la de hoy
curl -i -X PUT http://localhost:8074/api/docente/1098332 -H "Content-Type: application/json" -d '{
 "cedula":1098332,"nombres":"Carlos","apellidos":"Pérez Mejía","genero":"Masculino","cargo":"Profesor titular",
 "fecha_nacimiento":"1979-09-01","correo":"carlos.perez@universidad.edu.co","telefono":"3109876543",
 "url_cvlac":"https://scienti.minciencias.gov.co/cvlac/1098332","escalafon":"Titular",
 "perfil":"Docente de bases de datos.","cat_minciencia":"Junior","conv_minciencia":"Convocatoria 894 de 2021",
 "nacionalidaad":"Colombiana","linea_investigacion_principal":1}'
# Esperado: 200 con "fecha_actualizacion" = hoy. El total_estudios de otro docente no altera su sello.

# PUT con cédula distinta en el cuerpo -> 422
curl -i -X PUT http://localhost:8074/api/docente/1098332 -H "Content-Type: application/json" -d '{"cedula":5,"nombres":"X"}'
```

### Paso 7 — Hijos directos y 1:1 (CA-V2-02, CA-V2-10)

```bash
# Evaluación: el id lo genera la base; calificación fuera de 0-5 -> 422
curl -i -X POST http://localhost:8074/api/evaluacion-docente -H "Content-Type: application/json" -d '{"calificacion":4.5,"semestre":"2026-1","docente":1017245}'   # 201
curl -i -X POST http://localhost:8074/api/evaluacion-docente -H "Content-Type: application/json" -d '{"calificacion":7,"semestre":"2026-1","docente":1017245}'     # 422

# Reconocimiento y experiencia (nombre del esquema: experiecia)
curl -i -X POST http://localhost:8074/api/reconocimiento -H "Content-Type: application/json" -d '{"tipo":"Premio","fecha":"2019-05-10","institucion":"UdeA","nombre":"Mejor docente","ambito":"Nacional","docente":1017245}'
curl -i -X POST http://localhost:8074/api/experiecia -H "Content-Type: application/json" -d '{"nombre_cargo":"Jefe de área","institucion":"UdeA","tipo":"Administrativa","fecha_inicio":"2016-01-10","fecha_fin":null,"docente":1017245}'
# fecha_fin anterior a fecha_inicio -> 422
curl -i -X POST http://localhost:8074/api/experiecia -H "Content-Type: application/json" -d '{"nombre_cargo":"X","institucion":"U","tipo":"T","fecha_inicio":"2016-01-10","fecha_fin":"2015-01-01","docente":1017245}'

# Apoyo y beca (1:1 con el estudio 1)
curl -i -X POST http://localhost:8074/api/apoyo-profesoral -H "Content-Type: application/json" -d '{"estudios":1,"con_apoyo":1,"institucion":"Colciencias","tipo":"Parcial"}'   # 201
curl -i -X POST http://localhost:8074/api/apoyo-profesoral -H "Content-Type: application/json" -d '{"estudios":1,"con_apoyo":0,"institucion":"X","tipo":"Y"}'                # 409 (ya existe)
curl -i -X DELETE http://localhost:8074/api/apoyo-profesoral/1                                                                                                              # 204
curl -i -X POST http://localhost:8074/api/apoyo-profesoral -H "Content-Type: application/json" -d '{"estudios":1,"con_apoyo":1,"institucion":"Colciencias","tipo":"Total"}'     # 201: se REACTIVA (R3)
curl -i -X POST http://localhost:8074/api/beca -H "Content-Type: application/json" -d '{"estudios":1,"tipo":"Posgrado","institucion":"Fulbright","fecha_inicio":"2010-08-01","fecha_fin":null}'
```

### Paso 8 — Tablas puente (CA-V2-08)

```bash
# Asignar -> 201 ; repetir la pareja -> 409 ; retirar -> 204 ; reasignar -> 201 (reactiva)
curl -i -X POST http://localhost:8074/api/docente-departamento -H "Content-Type: application/json" -d '{"docente":1017245,"departamento":3,"dedicacion":"Tiempo completo","modalidad":"Presencial","fecha_ingreso":"2015-02-01","fecha_salida":null}'
curl -i -X POST http://localhost:8074/api/docente-departamento -H "Content-Type: application/json" -d '{"docente":1017245,"departamento":3,"dedicacion":"Medio tiempo","modalidad":"Virtual","fecha_ingreso":"2016-01-01","fecha_salida":null}'   # 409
curl -i -X DELETE http://localhost:8074/api/docente-departamento/1017245/3                                                                                                                                                             # 204
curl -i -X POST http://localhost:8074/api/docente-departamento -H "Content-Type: application/json" -d '{"docente":1017245,"departamento":3,"dedicacion":"Medio tiempo","modalidad":"Virtual","fecha_ingreso":"2016-01-01","fecha_salida":null}'   # 201

# Programa inexistente -> 409
curl -i -X POST http://localhost:8074/api/docente-departamento -H "Content-Type: application/json" -d '{"docente":1017245,"departamento":999,"dedicacion":"X","modalidad":"Y","fecha_ingreso":"2016-01-01"}'

# Red-docente (atributos editables) y puentes puras
curl -i -X POST http://localhost:8074/api/red-docente -H "Content-Type: application/json" -d '{"red":1,"docente":1017245,"fecha_inicio":"2018-03-10","fecha_fin":null,"act_destacadas":"Coordinación de eventos."}'
curl -i -X PUT  http://localhost:8074/api/red-docente/1/1017245 -H "Content-Type: application/json" -d '{"red":1,"docente":1017245,"fecha_inicio":"2018-03-10","fecha_fin":"2024-12-31","act_destacadas":"Coordinación y ponencias."}'
curl -i -X POST http://localhost:8074/api/intereses-futuros -H "Content-Type: application/json" -d '{"docente":1017245,"termino_clave":"Inteligencia artificial"}'
curl -i -X POST http://localhost:8074/api/estudio-ac -H "Content-Type: application/json" -d '{"estudio":1,"area_conocimiento":"1B01"}'
curl -i -X POST http://localhost:8074/api/estudio-ac -H "Content-Type: application/json" -d '{"estudio":1,"area_conocimiento":"1B01"}'     # 409
curl -i -X PUT  http://localhost:8074/api/estudio-ac/1/1B01 -H "Content-Type: application/json" -d '{}'                                 # sin PUT: 405
curl -i -X DELETE "http://localhost:8074/api/intereses-futuros/1017245/Inteligencia%20artificial"                                       # 204
```

### Paso 9 — Retiro bloqueado y borrado lógico (CA-V2-10, CA-V2-14)

```bash
# El docente 1017245 tiene detalle activo -> 409 (regla R2)
curl -i -X DELETE http://localhost:8074/api/docente/1017245
# Esperado: 409 "No se puede retirar: tiene N registros activos en su detalle."

# El docente 1098332 no tiene detalle: se puede retirar -> 204, desaparece del listado y sigue en la base
curl -i -X DELETE http://localhost:8074/api/docente/1098332
curl -s http://localhost:8074/api/docente | grep -o '"cedula":[0-9]*'     # solo 1017245
sql "SELECT cedula, activo FROM docente"                                  # 1098332 con activo = 0
```

### Paso 10 — Front (CA-V2-05, CA-V2-06, CA-V2-09, CA-V2-11)

Abre http://localhost:8010. Usa datos nuevos (los de los pasos anteriores quedaron retirados o con detalle). Recorre:

| Pantalla | Comprobar |
|---|---|
| Barra de navegación | Aparece «Docentes» (y siguen los enlaces de la v1) |
| `/Docentes/Crear` | El desplegable de línea principal está **lleno** con nombres y trae «(ninguna)»; crear con «(ninguna)» funciona |
| `/Docentes` | La lista muestra la columna de total de estudios; retirar pide confirmación |
| `/Docentes/Editar/{cedula}` | La cédula aparece bloqueada |
| `/Docentes/Detalle/{cedula}` | En la **misma pantalla** se ven y se agregan estudios, evaluaciones, experiencias, reconocimientos, departamentos, intereses y redes |
| Agregar 3 estudios de una vez | Un solo envío; mensaje «Se agregaron 3 estudios.»; el total del docente sube a 3 sin recargar a mano |
| Departamentos | El desplegable muestra **nombres de programas**, no códigos; si no hay programas, aparece el aviso con enlace a `/Programas/Crear` |
| Intereses y áreas | Sin botón de editar; repetir una pareja muestra el error en castellano dentro del formulario |
| `/EstudiosRealizados/Detalle/{id}` | Apoyo, beca y áreas se ven y se gestionan ahí |
| Jerga | Ninguna pantalla dice «PUT», «PATCH», «422», «FK» ni números de error |

Resiliencia: `docker compose stop api-gestion` y recarga `/Docentes` → «No se pudo conectar con la API de gestión.» sin pantalla de error. Luego `docker compose start api-gestion`.

### Paso 11 — Pruebas y revisión de código (CA-V2-03)

```bash
# Pruebas de capas (la v1 + los servicios de la v2 con repositorios falsos)
docker run --rm -v "$(pwd):/src" -w /src mcr.microsoft.com/dotnet/sdk:10.0 dotnet test pruebas/api_gestion.Pruebas
# Esperado: todas aprobadas  (en PowerShell usa ${PWD})

# No hay SQL escrito a mano en los repositorios de la v2 (solo nombres de procedimiento)
grep -rnE "SELECT |INSERT |UPDATE |DELETE |FromSqlRaw|ExecuteSqlRaw" api_gestion/Repositorios/ --include="*.cs" \
  | grep -v "Repositorio\(Programa\|Red\|TerminoClave\|LineaInvestigacion\|AreaConocimiento\)"
# Esperado: sin resultados  (excluye los 5 repositorios de la v1)
```

---

## 3. Regresión de la v1 (CA-V2-01, CA-V2-12) — obligatoria

La v2 **incluye** la v1: se corre completa el día de la entrega, no «debería seguir funcionando».

1. Arranque limpio para que los conteos de la v1 sean los originales:

   ```bash
   docker compose down -v
   docker compose up -d --build        # esperar sqlserver-init en Exited (0)
   ```

2. Ejecutar los **pasos 1 a 9** de `docs/7_quickstart.md` (v1) tal cual: contenedores, base (`areas = 218`, `programas = 0`), áreas de conocimiento, programa de punta a punta, las otras 3 entidades, Swagger, front, persistencia/reset y pruebas de capas.
3. Marcar los criterios **CA-01 a CA-11** de la v1.

> Si en el repositorio el `7_quickstart.md` de la v1 todavía conserva marcas de conflicto de git (`<<<<<<<`, `=======`, `>>>>>>>`), el comando correcto de cada bloque es el que está **después** de `=======` (el de `sqlcmd` en el paso 2). No se edita el archivo de la v1: se anota la observación en la entrega.

Aceptación: **cero criterios de la v1 fallados**. Una falla aquí invalida el tag `v2`.

---

## 4. Problemas frecuentes

| Síntoma | Diagnóstico |
|---|---|
| `sqlserver-init` en `Exited (1)` | Falló `v2_procedimientos.sql` (`sqlcmd -b`): `docker compose logs sqlserver-init` muestra la línea. Corregir y `down -v` + `up -d` |
| Cambié un procedimiento y no pasa nada | El script solo corre al crear la base. Reejecutarlo a mano (es idempotente): `docker compose exec sqlserver /opt/mssql-tools18/bin/sqlcmd -C -S localhost -U sa -P 'Paradigmas123!' -d gestion_local -I -b -i /scripts/v2_procedimientos.sql`, o `down -v` + `up -d` |
| Un error de clave foránea llega como `500` | `ManejadorExcepcionesSql` quedó registrado **después** del de la v1 (o no está registrado). Ver `3_plan.md` §4 |
| Las tildes salen como `?` en los mensajes | Faltan los literales `N'…'` en el `THROW` o el archivo no está en UTF-8 |
| `total_estudios` queda en 0 al crear estudios | El disparador no está creado (paso 1) o no escucha `UPDATE`/`DELETE`; recrear con `CREATE OR ALTER` |
| El desplegable sale vacío sin error | La API devolvió `[]` (no hay filas activas) o el front leyó mal la respuesta; probar el `GET` directo con `curl` |
| El desplegable de departamentos está vacío | `programa` arranca **vacía** en la v1: crear un programa desde `/Programas/Crear` |
| `POST` de hijo con cadena vacía en una clave numérica | El campo opcional debe enviar `null` (modelo `int?`), no `""` |
| Todo roto | `docker compose down -v` + `docker compose up -d --build` |
