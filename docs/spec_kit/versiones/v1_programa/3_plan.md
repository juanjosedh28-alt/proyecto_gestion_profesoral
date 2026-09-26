# Plan de Arquitectura v1_programa

## Estructura de Capas (C#)
- `Controllers/ProgramaController.cs`: Traducción HTTP y respuestas JSON.
- `Servicios/ServicioPrograma.cs`: Reglas de negocio e interfaz `IServicioPrograma`.
- `Repositorios/RepositorioProgramaSqlServer.cs`: Consultas Dapper parametrizadas e interfaz `IRepositorioPrograma`.
- `Modelos/Programa.cs`: Entidad del dominio.
- `Peticiones/`: DTOs por verbo (`ProgramaCrear` y `ProgramaActualizar`).