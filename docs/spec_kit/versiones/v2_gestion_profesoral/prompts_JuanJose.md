## 1. Despliegue y Depuración de Infraestructura (Docker y YAML)

### Prompt 1: Diagnóstico de error de sintaxis en `docker-compose.yml`
> **Intención:** Corregir el fallo de lectura del archivo de orquestación (`go-yaml load error in scanner`) al intentar levantar los servicios de SQL Server, API y Frontend.
> 
> **Prompt utilizado:**
> *"Tengo el siguiente error al ejecutar `docker compose up -d --build`: `go-yaml load error in scanner (while scanning a simple key) at L39.C1-L40.C1: could not find expected ':'`. Ayúdame a corregir la estructura YAML de `docker-compose.yml` para conectar `sqlserver`, `db-init`, `api-gestion` y `front-razor`."*
> 
> **Resultado:** Se estandarizó la indentación y variables de entorno (`ConnectionStrings__DefaultConnection` y `ApiGestionUrl`) logrando iniciar la orquestación de los contenedores.

---

## 2. Resolución de Conflictos de Fusión y Compilación (.NET 8)

### Prompt 2: Limpieza de marcadores de conflicto en `Dockerfile` y `.csproj`
> **Intención:** Resolver los errores de construcción de la imagen Docker de `api-gestion` provocados por marcas de fusión de Git (`<<<<<<< HEAD` y `error MSB4025`).
> 
> **Prompt utilizado:**
> *"Al construir la imagen de Docker para `api_gestion`, falla con `error MSB4025: The project file could not be loaded. Name cannot begin with the '<' character` y marcas `<<<<<<< HEAD` en el `Dockerfile`. ¿Cómo limpio `ApiGestion.csproj` y el `Dockerfile` para compilar en .NET 8?"*
> 
> **Resultado:** Se restauró el archivo de proyecto con las dependencias de Dapper, `Microsoft.Data.SqlClient` y Swashbuckle, y se configuró el `Dockerfile` multietapa.

### Prompt 3: Sincronización de Repositorios, Servicios y Controladores
> **Intención:** Eliminar definiciones duplicadas (`CS0101`) y errores de conflicto (`CS8300`) en los archivos C# de la capa de datos y controladores.
> 
> **Prompt utilizado:**
> *"Ejecuté `dotnet build` y me aparecen errores `CS8300` por marcadores de conflicto de Git en `Program.cs`, `ProgramaController.cs` y los repositorios, además de duplicidad en `IRepositorioPrograma`. Ayúdame a limpiar las interfaces y asegurar que el borrado lógico (`activo = 0`) y los procedimientos almacenados queden alineados."*
> 
> **Resultado:** Se eliminó el archivo duplicado y se logró compilar el backend con 0 errores.

---

## 3. Verificación de Frontend (`front_razor`) y Arquitectura para Sustentación

### Prompt 4: Despliegue del contenedor `front-razor`
> **Intención:** Solucionar el error `ERR_EMPTY_RESPONSE` en `http://localhost:8075/` asegurando que el cliente Razor Pages compile y se conecte a la API.
> 
> **Prompt utilizado:**
> *"El contenedor `front-razor` no responde en el puerto 8075 (`ERR_EMPTY_RESPONSE`). Ayúdame a corregir el `Dockerfile` de `front_razor` para que publique correctamente con .NET 8 y consuma la API en `http://api-gestion:8080/`."*
> 
> **Resultado:** Se levantó el frontend en el puerto `8075` conectado a la API REST.

### Prompt 5: Trazabilidad de Controladores y Claves Foráneas
> **Intención:** Validar el flujo de arquitectura en capas y el manejo de excepciones SQL (violación de llaves foráneas) para la revisión del proyecto.
> 
> **Prompt utilizado:**
> *"Muéstrame la ruta exacta de los controladores que manejan tablas con clave foránea y cómo el middleware captura los errores de SQL Server (código 547) para responder un HTTP 409 Conflict en lugar de un error 500."*
> 
> **Resultado:** Se verificó la arquitectura `Controllers -> Servicios -> Repositorios -> EjecutorProcedi
