# Constitución del Proyecto — Gestión Profesoral

## Artículo 1 — Stack y Persistencia
El backend se construye en C# con ASP.NET Core sobre SQL Server (`gestion_local`). Todo borrado es LÓGICO mediante la columna `activo = 0`.

## Artículo 2 — Arquitectura en Capas Innegociable
El sistema se divide estrictamente en: Controller -> Servicio -> Repositorio -> BD. Las dependencias cruzan por interfaces y los controladores no conocen SQL.

## Artículo 3 — SQL Parametrizado
Toda consulta SQL utiliza parámetros (`@parametro`) con Dapper / SqlClient. Prohibida la concatenación de cadenas.

## Artículo 4 — Secretos por Variables de Entorno
Cero credenciales en código. Las cadenas de conexión se leen desde variables de entorno o `appsettings.json`.