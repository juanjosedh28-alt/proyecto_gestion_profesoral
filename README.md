# Gestión Profesoral

Proyecto de aula del curso de Aplicaciones y Servicios Web: módulo **Gestión Profesoral** (docentes, sus estudios,
evaluaciones, experiencia, reconocimientos, programas, redes e intereses de investigación).

| Pieza | Tecnología | Carpeta | Dirección |
|---|---|---|---|
| Base de datos | SQL Server 2022 | `db/` | `localhost,11472` |
| API REST | C# / ASP.NET Core 10 + Dapper | `api_gestion/` | http://localhost:8074 (Swagger) |
| Front | Razor Pages | `front_razor/` | http://localhost:8010 |

## Arrancar

```bash
cp .env.example .env          # y ponga una contraseña segura en .env
docker compose up -d --build
```

La guía completa de verificación está en `docs/spec_kit/versiones/v2_gestion_profesoral/7_quickstart.md`.

## Versiones

| Versión | Qué tiene | Spec kit |
|---|---|---|
| v1 | CRUD de las 5 tablas sin clave foránea | `docs/spec_kit/versiones/v1_programa/` |
| v2 | CRUD de las 11 tablas con clave foránea, todo por procedimientos almacenados, disparadores, desplegables y maestro-detalle | `docs/spec_kit/versiones/v2_gestion_profesoral/` |

La constitución del proyecto (las reglas que valen para todas las versiones) está en `docs/spec_kit/1_constitution.md`.
Las pruebas de la API están en `postman/`.
