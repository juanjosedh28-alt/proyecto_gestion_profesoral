# Quickstart y verificación — Gestión Profesoral

Documento 7 de 8 del spec kit raíz. Validación del sistema ya construido (versión condensada de verificación).

---

## 1. Levantar

```bash
git clone <url-del-repositorio>
cd gestion_profesoral
docker compose up -d --build     # primera vez: varios minutos
curl http://localhost:8074/api/programa
```

---

## 2. Verificación en 8 pasos

1. Estado de los contenedores:

```bash
docker compose ps
# Esperado: api-gestion y sqlserver corriendo; sqlserver-init con Exited (0)
```

2. Listado vacío (la tabla `programa` arranca sin filas). Esperado: `204 No Content`.

```bash
curl -i http://localhost:8074/api/programa
```

3. Crear un programa. Esperado: `200 OK`. Con campos faltantes: `422 Unprocessable Entity`.

```bash
curl -i -X POST http://localhost:8074/api/programa \
  -H "Content-Type: application/json" \
  -d '{"id":1,"nombre":"Ingeniería de Sistemas","tipo":"Pregrado","nivel":"Profesional","fecha_creacion":"2000-01-15","numero_cohortes":"40","cant_graduados":"1200","fecha_actualizacion":"2025-01-01","ciudad":"Medellín","facultad":1}'
```

4. Consultar por id. Esperado: `200 OK`. Con un id inexistente: `404 Not Found`.

```bash
curl -i http://localhost:8074/api/programa/1
```

5. Actualizar. Esperado: `200 OK`.

```bash
curl -i -X PATCH http://localhost:8074/api/programa/1 \
  -H "Content-Type: application/json" \
  -d '{"fecha_cierre":"2030-12-31"}'
```

6. Borrado lógico. Esperado: `200 OK`, y el listado vuelve a responder `204`. La fila sigue en la BD con `activo = 0`.

```bash
curl -i -X DELETE http://localhost:8074/api/programa/1
curl -i http://localhost:8074/api/programa
```

7. Swagger abre en `http://localhost:8074/swagger`.

8. Persistencia y reset:
   - Persistencia: crear un registro → `docker compose down` → `docker compose up -d` → sigue ahí.
   - Reset: `docker compose down -v` → `docker compose up -d` → datos originales de vuelta (218 áreas de conocimiento, `programa` vacía).

Herramienta externa: SSMS o Azure Data Studio a `localhost,11443` con `sa` / `Paradigmas123!` para ver la base `gestion_local`.

---

## 3. Problemas frecuentes

| Síntoma | Diagnóstico |
|---|---|
| `curl` no responde en el puerto 8074 | `docker compose ps` + `docker compose logs api-gestion` |
| SQL Server nunca queda sano | Necesita ~2 GB de RAM; cerrar otras aplicaciones pesadas |
| Puerto ocupado (8074/11443) | Cerrar el otro programa o cambiar el mapeo en compose |
| Cambié el script SQL y no pasa nada | Solo corre si la BD no existe: `down -v` + `up -d` |
| Todo roto | `docker compose down -v` + `docker compose up -d --build` |