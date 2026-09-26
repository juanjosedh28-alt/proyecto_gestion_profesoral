# Especificación Funcional v1_programa

## Propósito
Exponer la API REST para administrar la entidad `programa` del módulo de Gestión Profesoral.

## Requisitos Funcionales
- GET `/api/programa`: Retorna la lista de programas (inicia vacía con 204).
- POST `/api/programa`: Crea un nuevo programa.
- PUT `/api/programa/{id}`: Reemplazo completo (422 si faltan campos).
- PATCH `/api/programa/{id}`: Actualización parcial (200 con campos opcionales).
- DELETE `/api/programa/{id}`: Borrado lógico (`activo = 0`).

## Clarificaciones
- La tabla `programa` arranca con 0 filas.
- Los campos de fecha se manejan como cadenas de texto según la estructura DDL dada.