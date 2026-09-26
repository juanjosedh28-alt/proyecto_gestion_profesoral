# Registros de Decisión (ADR)

## D1: Borrado Lógico sobre Borrado Físico
- **Alternativas:** (a) DELETE físico `DELETE FROM programa`, (b) Borrado lógico `UPDATE programa SET activo = 0`.
- **Decisión:** Opción (b) para preservar la integridad referencial e historial académico.