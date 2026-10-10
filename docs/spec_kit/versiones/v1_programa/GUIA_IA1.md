## 1. Prompt Inicial / De Arquitectura y Contexto (Prompt Base Extendido)

```text
Actúa como un Arquitecto de Software Senior y Desarrollador Lead en el ecosistema .NET / Cloud-Native. 
Necesito diseñar e implementar desde cero la solución empresarial denominada "proyecto_gestion_profesoral" siguiendo una metodología orientada a especificaciones (Spec Kit / Specification-Driven Development - SDD) y un enfoque de Arquitectura Limpia en capas (Clean Architecture).

---

### 1. OBJETIVO DEL SISTEMA
Construir la plataforma base para la gestión de información docente y académica institucionales, garantizando alta disponibilidad, escalabilidad, mantenibilidad y una estricta separación de responsabilidades entre la capa de persistencia, lógica de negocio, API RESTful y las interfaces de usuario.

---

### 2. ARQUITECTURA TÉCNICA Y STACK TECNOLÓGICO

#### A. Backend (API RESTful)
- **Framework:** ASP.NET Core Web API sobre .NET 8 LTS (`net8.0`).
- **Patrón de Persistencia:** Repository Pattern de forma asíncrona (`async`/`await`).
- **Acceso a Datos / ORM Híbrido:** Dapper como micro-ORM liviano combinado con `Microsoft.Data.SqlClient` para consultas SQL nativas de alto rendimiento.
- **Documentación y Contratos:** Swagger / OpenAPI 3.0 mediante `Swashbuckle.AspNetCore`.
- **Manejo Global de Excepciones:** Middleware centralizado para interceptar errores de dominio, validación (`ValidacionExcepcion`), recursos no encontrados (`NoEncontradoExcepcion`) y conflictos de concurrencia/claves duplicadas (`ConflictoExcepcion`), retornando respuestas estructuradas bajo el estándar RFC 7807 (Problem Details).

#### B. Frontend / Clientes
- **Interfaz Blazor:** Aplicación Blazor (.NET 8) interactiva basada en componentes modulares.
- **Interfaz Razor Pages:** Cliente web Razor Pages secundario configurado con consumidores HTTP asíncronos (`HttpClientFactory`).

#### C. Base de Datos
- **Motor:** Microsoft SQL Server 2022.
- **Estrategia de Inicialización:** Scripts SQL de creación e inserción de semillas ejecutados automáticamente en el ciclo de vida del contenedor mediante un script en bash (`init.sh`).
- **Estrategia de Borrado:** Borrado lógico obligatorio (`IsDeleted` / `Estado`) para preservar la integridad referencial y auditoría básica de los datos.

#### D. Infraestructura y Containerización
- **Orquestación:** Docker Compose (`docker-compose.yml`) integrando la API, la Base de Datos SQL Server, el script de inicialización y las interfaces web.
- **Aislamiento de Entorno:** Multi-stage builds en `Dockerfile` apuntando estrictamente a SDK `.NET 8.0` (`[mcr.microsoft.com/dotnet/sdk:8.0](https://mcr.microsoft.com/dotnet/sdk:8.0)` y `aspnet:8.0`).
- **Configuración de Salida:** Publicación explícita mediante parámetros de compilación `/p:AssemblyName=ApiGestion` para garantizar la generación determinista de la DLL ejecutable (`ApiGestion.dll`) y evitar incompatibilidades por *case-sensitivity* en entornos Linux.
- **Reglas de Volúmenes:** Prohibido el montaje de volúmenes locales en caliente sobre la ruta `/app` de los contenedores para evitar la pérdida o sobrescritura de los binarios compilados en tiempo de ejecución.

---

### 3. ALCANCE FUNCIONAL (FASE V1 - MÓDULOS INDEPENDIENTES)

Implementación completa de las operaciones CRUD (Create, Read, Update, Delete lógico) para las entidades base independientes (sin dependencias complejas de claves foráneas):
1. **Programas Académicos:** Gestión de identificadores, nombres y estados.
2. **Áreas de Conocimiento:** Clasificación disciplinar.
3. **Líneas de Investigación:** Categorización de investigación grupal y docente.
4. **Redes Académicas:** Agrupaciones y convenios.
5. **Términos Clave:** Taxonomía y descriptores de búsqueda.

---

### 4. DELIVERABLES / ENTREGABLES REQUERIDOS

Ayúdame a generar de manera paso a paso y con código de grado de producción:

1. **Estructura de Directorios:** Diseño del proyecto en carpetas modulares (`api_gestion`, `front_blazor`, `front_razor`, `db`, `docs/spec_kit`).
2. **Capa de Dominio e Infraestructura:**
   - Modelos DTOs de petición/respuesta (ej: `ProgramaCrear`, `ProgramaActualizar`).
   - Interfaces de repositorios (`IRepositorioPrograma`) e implementación de repositorios con Dapper utilizando consultas parametrizeadas.
   - Middlewares de manejo de errores personalizados.
3. **Controladores Web API:** Controladores decorados con atributos HTTP, respuestas de estado adecuadas (200, 201, 400, 404, 409, 500) y Swagger Annotations.
4. **Configuración de Infraestructura como Código:**
   - `Dockerfile` optimizado en 2 etapas (Build y Runtime final) para `api_gestion`.
   - `docker-compose.yml` completo con dependencias de salud (`healthcheck` en SQL Server) y redes internas configuradas.
