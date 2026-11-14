# Bitácora de Prompts - Versión 2 (`v2_gestion_profesoral`)

- **Autor:** Juan José
- **Versión:** V2 - Gestión Profesoral (Procedimientos Almacenados, Arquitectura en Capas API REST y Frontend Razor)

---

## 1. Capa de Datos y Procedimientos Almacenados (`db/v2_procedimientos.sql`)

### Prompt 1: Diseño de Procedimientos Almacenados con Borrado Lógico
> **Intención:** Estandarizar las operaciones CRUD de las 16 entidades del modelo relacional en SQL Server aplicando borrado lógico e integridad referencial.
> 
> **Prompt utilizado:**
> *"Genera los procedimientos almacenados en T-SQL (`sp_listar_*`, `sp_consultar_*`, `sp_crear_*`, `sp_actualizar_*` y `sp_eliminar_*`) para las tablas del esquema `gestion_profesoral`, implementando borrado lógico mediante el campo `activo` (`UPDATE ... SET activo = 0` y filtros `WHERE activo = 1`) y validaciones de existencia previa."*
> 
> **Código / Artefacto generado:** Script `db/v2_procedimientos.sql` con los procedimientos almacenados de maestros, entidades hijas del docente y tablas puente (`docente_departamento`, `red_docente`).

---

## 2. Arquitectura Backend en .NET 8 y Dapper (`api_gestion/`)

### Prompt 2: Centralización de ejecución de Stored Procedures con Dapper
> **Intención:** Evitar código repetitivo de apertura de conexiones y manejo de parámetros en los 16 repositorios del sistema.
> 
> **Prompt utilizado:**
> *"Crea una clase utilitaria `EjecutorProcedimientos` en C# usando Dapper y `Microsoft.Data.SqlClient` que permita invocar procedimientos almacenados (`CommandType.StoredProcedure`) de forma asíncrona para consultas múltiples, registros únicos y comandos de escritura."*
> 
> **Código / Artefacto generado:** `api_gestion/Repositorios/EjecutorProcedimientos.cs` y los 16 repositorios tipados (`RepositorioDocente.cs`, `RepositorioEstudiosRealizados.cs`, `RepositorioRedDocente.cs`, etc.).

### Prompt 3: Middleware Global para Excepciones de Base de Datos y Claves Foráneas
> **Intención:** Traducir los errores nativos de SQL Server en respuestas HTTP semánticas (409 Conflict, 404 Not Found, 400 Bad Request) sin exponer trazas internas.
> 
> **Prompt utilizado:**
> *"Implementa un middleware en ASP.NET Core 8 (`ManejadorErrores` y `ManejadorErroresSql`) que intercepte `SqlException` y mapee los códigos de violación de clave foránea (`547`) y clave única duplicada (`2627`, `2601`) a una `ConflictoExcepcion` devolviendo HTTP 409 Conflict en formato JSON."*
> 
> **Código / Artefacto generado:** `api_gestion/Middleware/ManejadorErrores.cs`, `ManejadorErroresSql.cs` y excepciones personalizadas en `api_gestion/Excepciones/`.

### Prompt 4: Implementación de Controladores REST, Servicios y DTOs
> **Intención:** Exponer los endpoints CRUD de las tablas maestras y con claves foráneas respetando la separación de responsabilidades.
> 
> **Prompt utilizado:**
> *"Implementa los controladores REST (`DocenteController`, `EstudiosRealizadosController`, `EvaluacionDocenteController`, `DocenteDepartamentoController`, `RedDocenteController`, entre otros) junto con sus servicios y objetos de petición (`*Crear.cs`), conectando las reglas de negocio en `Validaciones.cs` e inyectando las dependencias en `Program.cs`."*
> 
> **Código / Artefacto generado:** Los 16 controladores en `api_gestion/Controllers/`, sus servicios en `api_gestion/Servicios/` y DTOs en `api_gestion/Peticiones/`.

---

## 3. Frontend Razor Pages y Consumo de API (`front_razor/`)

### Prompt 5: Clientes HTTP Tipados y Gestión de Entidades Relacionadas
> **Intención:** Conectar las vistas de Razor Pages con los endpoints de `api_gestion` para gestionar tanto los maestros como los registros hijos y tablas puente desde el perfil del docente.
> 
> **Prompt utilizado:**
> *"Desarrolla la clase base `ClienteApiBase` con `HttpClient` y los clientes específicos (`ClienteDocente`, `ClientesDetalleDocente`, `ClientesPuentes`) junto con las páginas Razor de detalle y edición (`Pages/Docentes/Detalle.cshtml`, `EditarEstudio.cshtml`, `EditarRed.cshtml`, `EditarPrograma.cshtml`) para administrar las relaciones del docente y capturar errores de validación de la API."*
> 
> **Código / Artefacto generado:** Clientes HTTP en `front_razor/Clientes/`, modelos de vista en `front_razor/Modelos/` y páginas Razor en `front_razor/Pages/`.
