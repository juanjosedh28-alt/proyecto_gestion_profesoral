# Guia de Prompts - Version 2

Este documento registra los prompts y correcciones para la Version 2.

## 1. Decisiones de Arquitectura V2
- Procedimientos Almacenados: CRUD 100% por SP.
- Borrado Logico: UPDATE con activo = 0.
- Manejo de Errores: Middleware retorna HTTP 409 Conflict.
- Disparadores: Trigger trg_contar_estudios activo.

## 2. Prompts Utilizados
> Actua como Arquitecto Senior .NET 8. Implementa CRUD por SP, borrado logico, trigger de conteo y errores 409.

## 3. Bitacora de Correcciones
- Trigger: Ajustado para oir UPDATE en borrado logico.
- Middleware: Captura de excepcion SQL 547 para responder 409.
- Docker: Eliminacion de volumen local sobre /app.

## 4. Preguntas de Sustentacion
- Q1: sp_crear_docente inserta y retorna la fila guardada.
- Q2: El trigger garantiza consistencia en la base de datos.
- Q3: Si falla la FK, se responde HTTP 409 en castellano.
- Q4: Se modifica la BD, los SP, el DTO y la interfaz.
