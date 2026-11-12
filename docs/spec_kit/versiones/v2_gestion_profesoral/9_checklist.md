# Lista de chequeo de requisitos — v2 (compuerta 3)

> Cada requisito tiene que ser **medible, único y verificable**. No se escribe código con la lista en rojo.
> **Las casillas las marca una persona del equipo**, revisando la parte de la spec que escribió otro: una IA
> puede ayudar a evaluar, pero no puede auto-aprobarse. Por eso esta lista se entrega **sin marcar**.

Revisó: ______________________  Fecha: ____/____/________

## Requisitos funcionales

| ✔ | Requisito | ¿Medible? | ¿Único? | ¿Cómo se verifica? |
|---|---|---|---|---|
| [ ] | RF7 Docente: CRUD, cédula enviada por el cliente, línea opcional | Sí | Sí | Colección carpeta 1 |
| [ ] | RF8 Detalle directo: no existe sin docente (409) | Sí | Sí | Colección carpeta 2, «FALLA: estudio para un docente inexistente» |
| [ ] | RF9 Apoyo y beca 1:1, repetir da 409, retirado se reactiva | Sí | Sí | Colección carpeta 5 |
| [ ] | RF10 Puentes: asignar y retirar con dos claves, repetir 409, puras sin PUT | Sí | Sí | Colección carpeta 4 |
| [ ] | RF11 Envío múltiple: 1 a 100 renglones, todo o nada | Sí | Sí | Colección carpeta 2 (id repetido, fecha mala, lista vacía) |
| [ ] | RF12 Disparadores: total_estudios cambia al crear y al retirar | Sí | Sí | Colección carpeta 2, pruebas «total_estudios = 3» y «bajó a 2» |
| [ ] | RF13 Integridad: ningún error previsible es 500 | Sí | Sí | Todas las peticiones «FALLA:» |
| [ ] | RF14 Front: las 11 tablas desde Docentes, desplegables, sin jerga | Sí | Sí | Quickstart §4, criterios 2, 5, 9 y 11 |

## Reglas de negocio

| ✔ | Regla | ¿Está en un solo sitio? | ¿Dónde se verifica? |
|---|---|---|---|
| [ ] | R1 Nombres del esquema tal cual | Sí (esquema) | Rutas `/api/experiecia`, campo `nacionalidaad` |
| [ ] | R2 Retirar maestro con detalle → 409 | Sí (`sp_eliminar_docente`, `sp_eliminar_estudios_realizados`) | Colección carpetas 2 y 5 |
| [ ] | R3 Reactivación en puentes y 1:1 | Sí (`sp_crear_` de esas tablas) | Colección carpeta 4, «Volver a asignarla la reactiva» |
| [ ] | R4 Calificación entre 0 y 5 | Sí (DTO; el procedimiento es red de seguridad) | Colección carpeta 3 |
| [ ] | R5 total_estudios cuenta solo activos | Sí (`trg_contar_estudios`) | Colección carpeta 2 |
| [ ] | R6 Puentes puras sin actualizar | Sí (no existe `sp_actualizar_` ni `PUT`) | Swagger |
| [ ] | R7 La v1 no bloquea retirar padres usados por la v2 | Sí (documentado) | — (limitación aceptada) |
| [ ] | R8 Cédula e id de estudio los envía el cliente | Sí | Colección carpeta 1 y 2 |
| [ ] | R9 La v1 también por procedimientos | Sí | `grep` del criterio 3 |
| [ ] | R10 URL de red ≤ 45 | Sí | Crear una red con URL larga → 422 |

## Requisitos no funcionales

| ✔ | Requisito | ¿Cómo se verifica? |
|---|---|---|
| [ ] | RNF8 SQL solo en la base | `grep -riw "select\|insert\|update\|delete" api_gestion/Repositorios` vacío |
| [ ] | RNF9 Errores previsibles ≠ 500 | Peticiones «FALLA:» de la colección |
| [ ] | RNF10 Disparadores por conjuntos | Lectura de `trg_contar_estudios`: usa `inserted`/`deleted`, sin cursores |
| [ ] | RNF11 Atomicidad del envío múltiple | Colección carpeta 2: «Ninguno de esos renglones se guardó» |
| [ ] | RNF12 Regresión de la v1 | Quickstart de la v1 completo |
| [ ] | RNF13 Interpretabilidad | Cada archivo nuevo tiene comentario con qué hace y por qué; cada integrante puede contarlo |
| [ ] | Secretos fuera del código | `grep -rn "Aplicacionweb123" --include=*.yml --include=*.cs --include=*.json .` vacío; existe `.env.example` |

## Compuertas

- [ ] Compuerta 1: `2_spec.md` §9 sin `[NECESITA ACLARACIÓN]` pendientes.
- [ ] Compuerta 2: `3_plan.md` §9 (chequeo de constitución) en verde.
- [ ] Compuerta 3: esta lista completa, revisada por alguien distinto de quien escribió cada parte.
