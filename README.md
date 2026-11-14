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
cp .env.example .env          # ponga una contraseña segura y una JWT_KEY de 32 caracteres o más
docker compose up -d --build
```

Desde la v3 hay que **iniciar sesión**. Usuarios de prueba:

| Correo | Contraseña | Rol |
|---|---|---|
| `admin@gestion.edu.co` | `admin123` | Administrador (todo, incluida la administración de usuarios, roles y permisos) |
| `editor@gestion.edu.co` | `editor123` | Editor (ve y modifica el módulo) |
| `consulta@gestion.edu.co` | `consulta123` | Consulta (solo ve) |

La guía completa de verificación está en `docs/spec_kit/versiones/v3_control_acceso/7_quickstart.md`.

## Versiones

| Versión | Qué tiene | Spec kit |
|---|---|---|
| v1 | CRUD de las 5 tablas sin clave foránea | `docs/spec_kit/versiones/v1_programa/` |
| v2 | CRUD de las 11 tablas con clave foránea, todo por procedimientos almacenados, disparadores, desplegables y maestro-detalle | `docs/spec_kit/versiones/v2_gestion_profesoral/` |
| v3 | Control de acceso: contraseña con bcrypt, inicio de sesión con JWT, permiso por rol en cada petición, menú según el rol y administración de usuarios, roles y permisos | `docs/spec_kit/versiones/v3_control_acceso/` |

La constitución del proyecto (las reglas que valen para todas las versiones) está en `docs/spec_kit/1_constitution.md`.
Las pruebas de la API están en `postman/` (desde la v3 se usa `gestion_profesoral_v3.postman_collection.json`, que incluye la regresión de la v2 con token).
