-- ============================================================
-- VERSIÓN 2 — Procedimientos almacenados, vistas y disparadores
-- Base: gestion_local (SQL Server 2022)
--
-- QUÉ HACE ESTE ARCHIVO
--   1. Agrega a docente la columna total_estudios (la mantiene un disparador).
--   2. Crea una VISTA por tabla: define CÓMO SE VE una fila hacia la API
--      (fechas como texto 'aaaa-mm-dd' y el nombre del padre para mostrar).
--   3. Crea los procedimientos almacenados de las 16 tablas del módulo:
--      sp_listar_, sp_consultar_, sp_crear_, sp_actualizar_, sp_eliminar_<tabla>
--      y, para el detalle del docente, sp_crear_<tabla>_lote (varios renglones
--      en UN SOLO envío).
--   4. Crea los dos disparadores: trg_contar_estudios y trg_sello_docente.
--
-- POR QUÉ ASÍ
--   - Todo el SQL vive aquí, en un solo sitio y con nombre. La API solo LLAMA
--     a los procedimientos con parámetros (nunca arma SQL pegando texto, que
--     es por donde entra una inyección de SQL).
--   - El script es IDEMPOTENTE (CREATE OR ALTER, COL_LENGTH): se puede volver a
--     ejecutar sin romper nada. init.sh lo corre en cada arranque.
--
-- CÓDIGOS DE ERROR (la API los traduce; ver Middleware/ManejadorErroresSql.cs)
--   50001 -> 409  el padre no existe o está retirado
--   50002 -> 409  clave repetida, pareja repetida o retiro bloqueado
--   50003 -> 422  dato inválido que solo la base de datos puede ver
--   50004 -> 404  el registro no existe o está retirado
--   Los mensajes van con N'...' para que las tildes lleguen bien.
--
-- BORRADO LÓGICO: sp_eliminar_ hace UPDATE ... SET activo = 0 (nunca DELETE)
-- y todos los sp_listar_ filtran activo = 1.
-- ============================================================

USE gestion_local;
GO

-- ============================================================
-- 1. COLUMNA NUEVA: docente.total_estudios
--    Cuántos estudios ACTIVOS tiene el docente. No la envía la API:
--    la calcula el disparador trg_contar_estudios.
-- ============================================================
IF COL_LENGTH('docente', 'total_estudios') IS NULL
    ALTER TABLE docente ADD total_estudios INT NOT NULL
        CONSTRAINT df_docente_total_estudios DEFAULT 0;
GO

-- ############################################################
-- PARTE A — TABLAS DE LA V1 (sin claves foráneas)
-- En la v1 estas tablas usaban SQL escrito en los repositorios de C#.
-- La v2 exige que TODAS las tablas pasen por procedimientos, así que
-- aquí se agregan los suyos (el esquema de las tablas no cambia).
-- ############################################################

-- ------------------------------------------------------------
-- area_conocimiento (catálogo precargado de 218 filas: solo lectura)
-- ------------------------------------------------------------
CREATE OR ALTER PROCEDURE sp_listar_area_conocimiento
AS
BEGIN
    SET NOCOUNT ON;
    SELECT id, gran_area, area, disciplina
    FROM area_conocimiento WHERE activo = 1 ORDER BY id;
END;
GO

CREATE OR ALTER PROCEDURE sp_consultar_area_conocimiento
    @id VARCHAR(6)
AS
BEGIN
    SET NOCOUNT ON;
    SELECT id, gran_area, area, disciplina
    FROM area_conocimiento WHERE id = @id AND activo = 1;
END;
GO

-- ------------------------------------------------------------
-- termino_clave (la clave es el propio término, texto)
-- ------------------------------------------------------------
CREATE OR ALTER PROCEDURE sp_listar_termino_clave
AS
BEGIN
    SET NOCOUNT ON;
    SELECT termino, termino_ingles FROM termino_clave WHERE activo = 1 ORDER BY termino;
END;
GO

CREATE OR ALTER PROCEDURE sp_consultar_termino_clave
    @termino VARCHAR(30)
AS
BEGIN
    SET NOCOUNT ON;
    SELECT termino, termino_ingles FROM termino_clave WHERE termino = @termino AND activo = 1;
END;
GO

CREATE OR ALTER PROCEDURE sp_crear_termino_clave
    @termino VARCHAR(30),
    @termino_ingles VARCHAR(30) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @msg NVARCHAR(400);
    -- Se busca también entre los retirados: la clave no se reutiliza.
    IF EXISTS (SELECT 1 FROM termino_clave WHERE termino = @termino)
    BEGIN
        SET @msg = CONCAT(N'Ya existe un término clave «', @termino, N'».');
        THROW 50002, @msg, 1;
    END;
    INSERT INTO termino_clave (termino, termino_ingles) VALUES (@termino, @termino_ingles);
    EXEC sp_consultar_termino_clave @termino;   -- devuelve la fila como quedó guardada
END;
GO

CREATE OR ALTER PROCEDURE sp_actualizar_termino_clave
    @termino VARCHAR(30),
    @termino_ingles VARCHAR(30) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    IF NOT EXISTS (SELECT 1 FROM termino_clave WHERE termino = @termino AND activo = 1)
        THROW 50004, N'No se encontró el término clave.', 1;
    UPDATE termino_clave SET termino_ingles = @termino_ingles WHERE termino = @termino;
    EXEC sp_consultar_termino_clave @termino;
END;
GO

CREATE OR ALTER PROCEDURE sp_eliminar_termino_clave
    @termino VARCHAR(30)
AS
BEGIN
    SET NOCOUNT ON;
    IF NOT EXISTS (SELECT 1 FROM termino_clave WHERE termino = @termino AND activo = 1)
        THROW 50004, N'No se encontró el término clave.', 1;
    UPDATE termino_clave SET activo = 0 WHERE termino = @termino;
END;
GO

-- ------------------------------------------------------------
-- linea_investigacion (el id lo genera la base: IDENTITY)
-- ------------------------------------------------------------
CREATE OR ALTER PROCEDURE sp_listar_linea_investigacion
AS
BEGIN
    SET NOCOUNT ON;
    SELECT id, nombre, descripcion FROM linea_investigacion WHERE activo = 1 ORDER BY id;
END;
GO

CREATE OR ALTER PROCEDURE sp_consultar_linea_investigacion
    @id INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT id, nombre, descripcion FROM linea_investigacion WHERE id = @id AND activo = 1;
END;
GO

CREATE OR ALTER PROCEDURE sp_crear_linea_investigacion
    @nombre VARCHAR(45),
    @descripcion VARCHAR(256)
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO linea_investigacion (nombre, descripcion) VALUES (@nombre, @descripcion);
    DECLARE @id INT = SCOPE_IDENTITY();   -- el id que acaba de generar la base
    EXEC sp_consultar_linea_investigacion @id;
END;
GO

CREATE OR ALTER PROCEDURE sp_actualizar_linea_investigacion
    @id INT,
    @nombre VARCHAR(45),
    @descripcion VARCHAR(256)
AS
BEGIN
    SET NOCOUNT ON;
    IF NOT EXISTS (SELECT 1 FROM linea_investigacion WHERE id = @id AND activo = 1)
        THROW 50004, N'No se encontró la línea de investigación.', 1;
    UPDATE linea_investigacion SET nombre = @nombre, descripcion = @descripcion WHERE id = @id;
    EXEC sp_consultar_linea_investigacion @id;
END;
GO

CREATE OR ALTER PROCEDURE sp_eliminar_linea_investigacion
    @id INT
AS
BEGIN
    SET NOCOUNT ON;
    IF NOT EXISTS (SELECT 1 FROM linea_investigacion WHERE id = @id AND activo = 1)
        THROW 50004, N'No se encontró la línea de investigación.', 1;
    UPDATE linea_investigacion SET activo = 0 WHERE id = @id;
END;
GO

-- ------------------------------------------------------------
-- programa (el id lo envía el cliente; sus fechas son VARCHAR en el esquema)
-- ------------------------------------------------------------
CREATE OR ALTER PROCEDURE sp_listar_programa
AS
BEGIN
    SET NOCOUNT ON;
    SELECT id, nombre, tipo, nivel, fecha_creacion, fecha_cierre, numero_cohortes,
           cant_graduados, ciudad, facultad, fecha_actualizacion
    FROM programa WHERE activo = 1 ORDER BY id;
END;
GO

CREATE OR ALTER PROCEDURE sp_consultar_programa
    @id INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT id, nombre, tipo, nivel, fecha_creacion, fecha_cierre, numero_cohortes,
           cant_graduados, ciudad, facultad, fecha_actualizacion
    FROM programa WHERE id = @id AND activo = 1;
END;
GO

CREATE OR ALTER PROCEDURE sp_crear_programa
    @id INT,
    @nombre VARCHAR(60),
    @tipo VARCHAR(45),
    @nivel VARCHAR(45),
    @fecha_creacion VARCHAR(45),
    @fecha_cierre VARCHAR(45) = NULL,
    @numero_cohortes VARCHAR(45),
    @cant_graduados VARCHAR(45),
    @ciudad VARCHAR(45),
    @facultad INT
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @msg NVARCHAR(400);
    IF EXISTS (SELECT 1 FROM programa WHERE id = @id)
    BEGIN
        SET @msg = CONCAT(N'Ya existe un programa con id ', @id, N'.');
        THROW 50002, @msg, 1;
    END;
    -- fecha_actualizacion la pone la base con la fecha de hoy (formato 23 = aaaa-mm-dd)
    INSERT INTO programa (id, nombre, tipo, nivel, fecha_creacion, fecha_cierre, numero_cohortes,
                          cant_graduados, fecha_actualizacion, ciudad, facultad)
    VALUES (@id, @nombre, @tipo, @nivel, @fecha_creacion, @fecha_cierre, @numero_cohortes,
            @cant_graduados, CONVERT(CHAR(10), GETDATE(), 23), @ciudad, @facultad);
    EXEC sp_consultar_programa @id;
END;
GO

CREATE OR ALTER PROCEDURE sp_actualizar_programa
    @id INT,
    @nombre VARCHAR(60),
    @tipo VARCHAR(45),
    @nivel VARCHAR(45),
    @fecha_creacion VARCHAR(45),
    @fecha_cierre VARCHAR(45) = NULL,
    @numero_cohortes VARCHAR(45),
    @cant_graduados VARCHAR(45),
    @ciudad VARCHAR(45),
    @facultad INT
AS
BEGIN
    SET NOCOUNT ON;
    IF NOT EXISTS (SELECT 1 FROM programa WHERE id = @id AND activo = 1)
        THROW 50004, N'No se encontró el programa.', 1;
    UPDATE programa
    SET nombre = @nombre, tipo = @tipo, nivel = @nivel, fecha_creacion = @fecha_creacion,
        fecha_cierre = @fecha_cierre, numero_cohortes = @numero_cohortes,
        cant_graduados = @cant_graduados, ciudad = @ciudad, facultad = @facultad,
        fecha_actualizacion = CONVERT(CHAR(10), GETDATE(), 23)
    WHERE id = @id;
    EXEC sp_consultar_programa @id;
END;
GO

CREATE OR ALTER PROCEDURE sp_eliminar_programa
    @id INT
AS
BEGIN
    SET NOCOUNT ON;
    IF NOT EXISTS (SELECT 1 FROM programa WHERE id = @id AND activo = 1)
        THROW 50004, N'No se encontró el programa.', 1;
    UPDATE programa SET activo = 0 WHERE id = @id;
END;
GO

-- ------------------------------------------------------------
-- red (la clave idr la envía el cliente)
-- ------------------------------------------------------------
CREATE OR ALTER PROCEDURE sp_listar_red
AS
BEGIN
    SET NOCOUNT ON;
    SELECT idr, nombre, url, pais FROM red WHERE activo = 1 ORDER BY idr;
END;
GO

CREATE OR ALTER PROCEDURE sp_consultar_red
    @idr INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT idr, nombre, url, pais FROM red WHERE idr = @idr AND activo = 1;
END;
GO

CREATE OR ALTER PROCEDURE sp_crear_red
    @idr INT,
    @nombre VARCHAR(45),
    @url VARCHAR(45),
    @pais VARCHAR(45)
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @msg NVARCHAR(400);
    IF EXISTS (SELECT 1 FROM red WHERE idr = @idr)
    BEGIN
        SET @msg = CONCAT(N'Ya existe una red con id ', @idr, N'.');
        THROW 50002, @msg, 1;
    END;
    INSERT INTO red (idr, nombre, url, pais) VALUES (@idr, @nombre, @url, @pais);
    EXEC sp_consultar_red @idr;
END;
GO

CREATE OR ALTER PROCEDURE sp_actualizar_red
    @idr INT,
    @nombre VARCHAR(45),
    @url VARCHAR(45),
    @pais VARCHAR(45)
AS
BEGIN
    SET NOCOUNT ON;
    IF NOT EXISTS (SELECT 1 FROM red WHERE idr = @idr AND activo = 1)
        THROW 50004, N'No se encontró la red.', 1;
    UPDATE red SET nombre = @nombre, url = @url, pais = @pais WHERE idr = @idr;
    EXEC sp_consultar_red @idr;
END;
GO

CREATE OR ALTER PROCEDURE sp_eliminar_red
    @idr INT
AS
BEGIN
    SET NOCOUNT ON;
    IF NOT EXISTS (SELECT 1 FROM red WHERE idr = @idr AND activo = 1)
        THROW 50004, N'No se encontró la red.', 1;
    UPDATE red SET activo = 0 WHERE idr = @idr;
END;
GO

-- ############################################################
-- PARTE B — VISTAS DE LA V2
-- Una vista por tabla con clave foránea. Las usan los sp_listar_ y
-- sp_consultar_ para que las CINCO operaciones devuelvan exactamente
-- las mismas columnas (así C# tiene un solo modelo por tabla).
--   - Las fechas salen como texto 'aaaa-mm-dd' (CONVERT estilo 23).
--   - Se agrega el NOMBRE del padre (docente_nombre, red_nombre, ...)
--     para mostrarlo en pantalla sin otra consulta.
--   - Los JOIN NO filtran el activo del padre (regla R7 del 2_spec.md):
--     una fila sigue mostrando el nombre aunque su padre se haya retirado.
--   - La columna activo viaja en la vista solo para poder filtrar;
--     la API no la devuelve (sus modelos no la tienen).
-- ############################################################

CREATE OR ALTER VIEW vw_docente AS
SELECT d.cedula, d.nombres, d.apellidos, d.genero, d.cargo,
       CONVERT(CHAR(10), d.fecha_nacimiento, 23) AS fecha_nacimiento,
       d.correo, d.telefono, d.url_cvlac,
       CONVERT(CHAR(10), d.fecha_actualizacion, 23) AS fecha_actualizacion,
       d.escalafon, d.perfil, d.cat_minciencia, d.conv_minciencia, d.nacionalidaad,
       d.linea_investigacion_principal,
       li.nombre AS linea_investigacion_principal_nombre,
       d.total_estudios, d.activo
FROM docente d
LEFT JOIN linea_investigacion li ON li.id = d.linea_investigacion_principal;  -- LEFT: la línea es opcional
GO

CREATE OR ALTER VIEW vw_estudios_realizados AS
SELECT e.id, e.titulo, e.universidad, CONVERT(CHAR(10), e.fecha, 23) AS fecha, e.tipo, e.ciudad,
       e.docente, CONCAT(d.nombres, ' ', d.apellidos) AS docente_nombre,
       CAST(e.ins_acreditada AS INT) AS ins_acreditada, e.metodologia, e.perfil_egresado, e.pais,
       e.activo
FROM estudios_realizados e
JOIN docente d ON d.cedula = e.docente;
GO

CREATE OR ALTER VIEW vw_evaluacion_docente AS
SELECT ev.id,
       CAST(ev.calificacion AS DECIMAL(4,2)) AS calificacion,   -- REAL -> DECIMAL: C# lo lee como decimal
       ev.semestre, ev.docente, CONCAT(d.nombres, ' ', d.apellidos) AS docente_nombre,
       ev.activo
FROM evaluacion_docente ev
JOIN docente d ON d.cedula = ev.docente;
GO

CREATE OR ALTER VIEW vw_reconocimiento AS
SELECT r.id, r.tipo, CONVERT(CHAR(10), r.fecha, 23) AS fecha, r.institucion, r.nombre, r.ambito,
       r.docente, CONCAT(d.nombres, ' ', d.apellidos) AS docente_nombre,
       r.activo
FROM reconocimiento r
JOIN docente d ON d.cedula = r.docente;
GO

CREATE OR ALTER VIEW vw_experiecia AS
SELECT x.id, x.nombre_cargo, x.institucion, x.tipo,
       CONVERT(CHAR(10), x.fecha_inicio, 23) AS fecha_inicio,
       CONVERT(CHAR(10), x.fecha_fin, 23) AS fecha_fin,
       x.docente, CONCAT(d.nombres, ' ', d.apellidos) AS docente_nombre,
       x.activo
FROM experiecia x
JOIN docente d ON d.cedula = x.docente;
GO

CREATE OR ALTER VIEW vw_apoyo_profesoral AS
SELECT a.estudios, e.titulo AS estudio_titulo, CAST(a.con_apoyo AS INT) AS con_apoyo,
       a.institucion, a.tipo, a.activo
FROM apoyo_profesoral a
JOIN estudios_realizados e ON e.id = a.estudios;
GO

CREATE OR ALTER VIEW vw_beca AS
SELECT b.estudios, e.titulo AS estudio_titulo, b.tipo, b.institucion,
       CONVERT(CHAR(10), b.fecha_inicio, 23) AS fecha_inicio,
       CONVERT(CHAR(10), b.fecha_fin, 23) AS fecha_fin,
       b.activo
FROM beca b
JOIN estudios_realizados e ON e.id = b.estudios;
GO

CREATE OR ALTER VIEW vw_docente_departamento AS
SELECT dd.docente, CONCAT(d.nombres, ' ', d.apellidos) AS docente_nombre,
       dd.departamento, p.nombre AS departamento_nombre,   -- "departamento" apunta a programa(id)
       dd.dedicacion, dd.modalidad,
       CONVERT(CHAR(10), dd.fecha_ingreso, 23) AS fecha_ingreso,
       CONVERT(CHAR(10), dd.fecha_salida, 23) AS fecha_salida,
       dd.activo
FROM docente_departamento dd
JOIN docente d ON d.cedula = dd.docente
JOIN programa p ON p.id = dd.departamento;
GO

CREATE OR ALTER VIEW vw_red_docente AS
SELECT rd.red, r.nombre AS red_nombre,
       rd.docente, CONCAT(d.nombres, ' ', d.apellidos) AS docente_nombre,
       CONVERT(CHAR(10), rd.fecha_inicio, 23) AS fecha_inicio,
       rd.fecha_fin,                                    -- en el esquema es VARCHAR(45)
       rd.act_destacadas, rd.activo
FROM red_docente rd
JOIN red r ON r.idr = rd.red
JOIN docente d ON d.cedula = rd.docente;
GO

CREATE OR ALTER VIEW vw_intereses_futuros AS
SELECT i.docente, CONCAT(d.nombres, ' ', d.apellidos) AS docente_nombre,
       i.termino_clave, t.termino_ingles, i.activo
FROM intereses_futuros i
JOIN docente d ON d.cedula = i.docente
JOIN termino_clave t ON t.termino = i.termino_clave;
GO

CREATE OR ALTER VIEW vw_estudio_ac AS
SELECT ea.estudio, e.titulo AS estudio_titulo,
       ea.area_conocimiento, a.disciplina AS area_conocimiento_disciplina,
       ea.activo
FROM estudio_ac ea
JOIN estudios_realizados e ON e.id = ea.estudio
JOIN area_conocimiento a ON a.id = ea.area_conocimiento;
GO

-- ############################################################
-- PARTE C — PROCEDIMIENTOS DE LA V2
-- ############################################################

-- ============================================================
-- docente (MAESTRO). Clave: cedula (la envía el cliente).
-- linea_investigacion_principal es OPCIONAL (NULL = «ninguna»).
-- ============================================================
CREATE OR ALTER PROCEDURE sp_listar_docente
AS
BEGIN
    SET NOCOUNT ON;
    SELECT * FROM vw_docente WHERE activo = 1 ORDER BY apellidos, nombres;
END;
GO

CREATE OR ALTER PROCEDURE sp_consultar_docente
    @cedula INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT * FROM vw_docente WHERE cedula = @cedula AND activo = 1;
END;
GO

CREATE OR ALTER PROCEDURE sp_crear_docente
    @cedula INT,
    @nombres VARCHAR(60),
    @apellidos VARCHAR(60),
    @genero VARCHAR(12),
    @cargo VARCHAR(30),
    @fecha_nacimiento DATE,
    @correo VARCHAR(70),
    @telefono VARCHAR(20),
    @url_cvlac VARCHAR(128),
    @escalafon VARCHAR(45),
    @perfil VARCHAR(MAX),
    @cat_minciencia VARCHAR(45) = NULL,
    @conv_minciencia VARCHAR(45),
    @nacionalidaad VARCHAR(45),
    @linea_investigacion_principal INT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @msg NVARCHAR(400);

    -- 1) La cédula no se repite, ni siquiera con un docente retirado.
    IF EXISTS (SELECT 1 FROM docente WHERE cedula = @cedula)
    BEGIN
        SET @msg = CONCAT(N'Ya existe un docente con la cédula ', @cedula, N'.');
        THROW 50002, @msg, 1;
    END;

    -- 2) Si mandaron línea de investigación, tiene que existir y estar activa.
    IF @linea_investigacion_principal IS NOT NULL
       AND NOT EXISTS (SELECT 1 FROM linea_investigacion WHERE id = @linea_investigacion_principal AND activo = 1)
        THROW 50001, N'No se puede registrar: la línea de investigación elegida no existe o está retirada.', 1;

    -- 3) Se guarda. fecha_actualizacion = hoy; total_estudios arranca en 0 (valor por defecto).
    INSERT INTO docente (cedula, nombres, apellidos, genero, cargo, fecha_nacimiento, correo, telefono,
                         url_cvlac, fecha_actualizacion, escalafon, perfil, cat_minciencia,
                         conv_minciencia, nacionalidaad, linea_investigacion_principal)
    VALUES (@cedula, @nombres, @apellidos, @genero, @cargo, @fecha_nacimiento, @correo, @telefono,
            @url_cvlac, CAST(GETDATE() AS DATE), @escalafon, @perfil, @cat_minciencia,
            @conv_minciencia, @nacionalidaad, @linea_investigacion_principal);

    -- 4) Devuelve la fila COMO QUEDÓ GUARDADA (con lo que puso la base).
    EXEC sp_consultar_docente @cedula;
END;
GO

CREATE OR ALTER PROCEDURE sp_actualizar_docente
    @cedula INT,
    @nombres VARCHAR(60),
    @apellidos VARCHAR(60),
    @genero VARCHAR(12),
    @cargo VARCHAR(30),
    @fecha_nacimiento DATE,
    @correo VARCHAR(70),
    @telefono VARCHAR(20),
    @url_cvlac VARCHAR(128),
    @escalafon VARCHAR(45),
    @perfil VARCHAR(MAX),
    @cat_minciencia VARCHAR(45) = NULL,
    @conv_minciencia VARCHAR(45),
    @nacionalidaad VARCHAR(45),
    @linea_investigacion_principal INT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    IF NOT EXISTS (SELECT 1 FROM docente WHERE cedula = @cedula AND activo = 1)
        THROW 50004, N'No se encontró el docente.', 1;

    IF @linea_investigacion_principal IS NOT NULL
       AND NOT EXISTS (SELECT 1 FROM linea_investigacion WHERE id = @linea_investigacion_principal AND activo = 1)
        THROW 50001, N'No se puede guardar: la línea de investigación elegida no existe o está retirada.', 1;

    -- fecha_actualizacion NO se toca aquí: la pone el disparador trg_sello_docente.
    UPDATE docente
    SET nombres = @nombres, apellidos = @apellidos, genero = @genero, cargo = @cargo,
        fecha_nacimiento = @fecha_nacimiento, correo = @correo, telefono = @telefono,
        url_cvlac = @url_cvlac, escalafon = @escalafon, perfil = @perfil,
        cat_minciencia = @cat_minciencia, conv_minciencia = @conv_minciencia,
        nacionalidaad = @nacionalidaad, linea_investigacion_principal = @linea_investigacion_principal
    WHERE cedula = @cedula;

    EXEC sp_consultar_docente @cedula;
END;
GO

CREATE OR ALTER PROCEDURE sp_eliminar_docente
    @cedula INT
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @msg NVARCHAR(400);
    IF NOT EXISTS (SELECT 1 FROM docente WHERE cedula = @cedula AND activo = 1)
        THROW 50004, N'No se encontró el docente.', 1;

    -- Regla R2: no se retira un docente que todavía tiene detalle activo.
    DECLARE @detalle INT =
          (SELECT COUNT(*) FROM estudios_realizados  WHERE docente = @cedula AND activo = 1)
        + (SELECT COUNT(*) FROM evaluacion_docente   WHERE docente = @cedula AND activo = 1)
        + (SELECT COUNT(*) FROM experiecia           WHERE docente = @cedula AND activo = 1)
        + (SELECT COUNT(*) FROM reconocimiento       WHERE docente = @cedula AND activo = 1)
        + (SELECT COUNT(*) FROM docente_departamento WHERE docente = @cedula AND activo = 1)
        + (SELECT COUNT(*) FROM intereses_futuros    WHERE docente = @cedula AND activo = 1)
        + (SELECT COUNT(*) FROM red_docente          WHERE docente = @cedula AND activo = 1);
    IF @detalle > 0
    BEGIN
        SET @msg = CONCAT(N'No se puede retirar: tiene ', @detalle, N' registros activos en su detalle. Retírelos primero.');
        THROW 50002, @msg, 1;
    END;

    UPDATE docente SET activo = 0 WHERE cedula = @cedula;   -- borrado LÓGICO
END;
GO

-- ============================================================
-- estudios_realizados (DETALLE de docente; MAESTRO de apoyo, beca y estudio_ac)
-- Clave: id (la envía el cliente).
-- ============================================================
CREATE OR ALTER PROCEDURE sp_listar_estudios_realizados
    @docente INT = NULL          -- NULL = todos; con valor = solo los de ese docente
AS
BEGIN
    SET NOCOUNT ON;
    SELECT * FROM vw_estudios_realizados
    WHERE activo = 1 AND (@docente IS NULL OR docente = @docente)
    ORDER BY id;
END;
GO

CREATE OR ALTER PROCEDURE sp_consultar_estudios_realizados
    @id INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT * FROM vw_estudios_realizados WHERE id = @id AND activo = 1;
END;
GO

CREATE OR ALTER PROCEDURE sp_crear_estudios_realizados
    @id INT,
    @titulo VARCHAR(45),
    @universidad VARCHAR(50),
    @fecha DATE,
    @tipo VARCHAR(45),
    @ciudad VARCHAR(45),
    @docente INT,
    @ins_acreditada TINYINT,
    @metodologia VARCHAR(45),
    @perfil_egresado VARCHAR(MAX),
    @pais VARCHAR(45)
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @msg NVARCHAR(400);
    IF NOT EXISTS (SELECT 1 FROM docente WHERE cedula = @docente AND activo = 1)
    BEGIN
        SET @msg = CONCAT(N'No se puede registrar: el docente ', @docente, N' no existe o está retirado.');
        THROW 50001, @msg, 1;
    END;
    IF EXISTS (SELECT 1 FROM estudios_realizados WHERE id = @id)
    BEGIN
        SET @msg = CONCAT(N'Ya existe un estudio con id ', @id, N'.');
        THROW 50002, @msg, 1;
    END;

    -- Al insertar, el disparador trg_contar_estudios recalcula docente.total_estudios.
    INSERT INTO estudios_realizados (id, titulo, universidad, fecha, tipo, ciudad, docente,
                                     ins_acreditada, metodologia, perfil_egresado, pais)
    VALUES (@id, @titulo, @universidad, @fecha, @tipo, @ciudad, @docente,
            @ins_acreditada, @metodologia, @perfil_egresado, @pais);

    EXEC sp_consultar_estudios_realizados @id;
END;
GO

CREATE OR ALTER PROCEDURE sp_actualizar_estudios_realizados
    @id INT,
    @titulo VARCHAR(45),
    @universidad VARCHAR(50),
    @fecha DATE,
    @tipo VARCHAR(45),
    @ciudad VARCHAR(45),
    @docente INT,
    @ins_acreditada TINYINT,
    @metodologia VARCHAR(45),
    @perfil_egresado VARCHAR(MAX),
    @pais VARCHAR(45)
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @msg NVARCHAR(400);
    IF NOT EXISTS (SELECT 1 FROM estudios_realizados WHERE id = @id AND activo = 1)
        THROW 50004, N'No se encontró el estudio.', 1;
    IF NOT EXISTS (SELECT 1 FROM docente WHERE cedula = @docente AND activo = 1)
    BEGIN
        SET @msg = CONCAT(N'No se puede guardar: el docente ', @docente, N' no existe o está retirado.');
        THROW 50001, @msg, 1;
    END;

    UPDATE estudios_realizados
    SET titulo = @titulo, universidad = @universidad, fecha = @fecha, tipo = @tipo, ciudad = @ciudad,
        docente = @docente, ins_acreditada = @ins_acreditada, metodologia = @metodologia,
        perfil_egresado = @perfil_egresado, pais = @pais
    WHERE id = @id;

    EXEC sp_consultar_estudios_realizados @id;
END;
GO

CREATE OR ALTER PROCEDURE sp_eliminar_estudios_realizados
    @id INT
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @msg NVARCHAR(400);
    IF NOT EXISTS (SELECT 1 FROM estudios_realizados WHERE id = @id AND activo = 1)
        THROW 50004, N'No se encontró el estudio.', 1;

    -- Regla R2: el estudio es maestro de apoyo, beca y áreas de conocimiento.
    DECLARE @detalle INT =
          (SELECT COUNT(*) FROM apoyo_profesoral WHERE estudios = @id AND activo = 1)
        + (SELECT COUNT(*) FROM beca             WHERE estudios = @id AND activo = 1)
        + (SELECT COUNT(*) FROM estudio_ac       WHERE estudio  = @id AND activo = 1);
    IF @detalle > 0
    BEGIN
        SET @msg = CONCAT(N'No se puede retirar: el estudio tiene ', @detalle, N' registros activos (apoyo, beca o áreas). Retírelos primero.');
        THROW 50002, @msg, 1;
    END;

    -- Retirar es un UPDATE: el disparador también lo escucha y baja el contador.
    UPDATE estudios_realizados SET activo = 0 WHERE id = @id;
END;
GO

-- Varios estudios de un docente en UN SOLO ENVÍO: se guardan todos o ninguno.
-- @json es la lista que manda la API: [{"id":1,"titulo":"...",...}, {...}]
CREATE OR ALTER PROCEDURE sp_crear_estudios_realizados_lote
    @docente INT,
    @json NVARCHAR(MAX)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;    -- ante cualquier error, se deshace TODO lo del envío
    DECLARE @msg NVARCHAR(400), @renglon INT, @repetido INT;

    IF NOT EXISTS (SELECT 1 FROM docente WHERE cedula = @docente AND activo = 1)
    BEGIN
        SET @msg = CONCAT(N'No se puede registrar: el docente ', @docente, N' no existe o está retirado.');
        THROW 50001, @msg, 1;
    END;

    -- 1) Se pasa el JSON a una tabla en memoria. [key] es la posición (0, 1, 2...) -> renglón 1, 2, 3...
    DECLARE @nuevos TABLE (renglon INT, id INT, titulo VARCHAR(45), universidad VARCHAR(50), fecha DATE,
                           tipo VARCHAR(45), ciudad VARCHAR(45), ins_acreditada TINYINT,
                           metodologia VARCHAR(45), perfil_egresado VARCHAR(MAX), pais VARCHAR(45));
    INSERT INTO @nuevos
    SELECT CAST(j.[key] AS INT) + 1, r.id, r.titulo, r.universidad, r.fecha, r.tipo, r.ciudad,
           r.ins_acreditada, r.metodologia, r.perfil_egresado, r.pais
    FROM OPENJSON(@json) j
    CROSS APPLY OPENJSON(j.[value]) WITH (
        id INT '$.id', titulo VARCHAR(45) '$.titulo', universidad VARCHAR(50) '$.universidad',
        fecha DATE '$.fecha', tipo VARCHAR(45) '$.tipo', ciudad VARCHAR(45) '$.ciudad',
        ins_acreditada TINYINT '$.ins_acreditada', metodologia VARCHAR(45) '$.metodologia',
        perfil_egresado VARCHAR(MAX) '$.perfil_egresado', pais VARCHAR(45) '$.pais') r;

    IF NOT EXISTS (SELECT 1 FROM @nuevos)
        THROW 50003, N'El envío no trae renglones.', 1;

    -- 2) Un id repetido DENTRO del mismo envío.
    SELECT TOP 1 @renglon = n.renglon, @repetido = n.id
    FROM @nuevos n
    WHERE EXISTS (SELECT 1 FROM @nuevos o WHERE o.id = n.id AND o.renglon < n.renglon)
    ORDER BY n.renglon;
    IF @renglon IS NOT NULL
    BEGIN
        SET @msg = CONCAT(N'El renglón ', @renglon, N' repite el id ', @repetido, N'.');
        THROW 50002, @msg, 1;
    END;

    -- 3) Un id que ya existe en la tabla (activo o retirado).
    SELECT TOP 1 @renglon = n.renglon, @repetido = n.id
    FROM @nuevos n JOIN estudios_realizados e ON e.id = n.id
    ORDER BY n.renglon;
    IF @renglon IS NOT NULL
    BEGIN
        SET @msg = CONCAT(N'El renglón ', @renglon, N': ya existe un estudio con id ', @repetido, N'.');
        THROW 50002, @msg, 1;
    END;

    -- 4) Un solo INSERT para todos: el disparador corre UNA vez con todas las filas.
    BEGIN TRANSACTION;
        INSERT INTO estudios_realizados (id, titulo, universidad, fecha, tipo, ciudad, docente,
                                         ins_acreditada, metodologia, perfil_egresado, pais)
        SELECT id, titulo, universidad, fecha, tipo, ciudad, @docente,
               ins_acreditada, metodologia, perfil_egresado, pais
        FROM @nuevos;
    COMMIT TRANSACTION;

    SELECT * FROM vw_estudios_realizados WHERE id IN (SELECT id FROM @nuevos) ORDER BY id;
END;
GO

-- ============================================================
-- evaluacion_docente (DETALLE de docente). Clave: id IDENTITY (la pone la base).
-- ============================================================
CREATE OR ALTER PROCEDURE sp_listar_evaluacion_docente
    @docente INT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SELECT * FROM vw_evaluacion_docente
    WHERE activo = 1 AND (@docente IS NULL OR docente = @docente)
    ORDER BY semestre, id;
END;
GO

CREATE OR ALTER PROCEDURE sp_consultar_evaluacion_docente
    @id INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT * FROM vw_evaluacion_docente WHERE id = @id AND activo = 1;
END;
GO

CREATE OR ALTER PROCEDURE sp_crear_evaluacion_docente
    @calificacion DECIMAL(4,2),
    @semestre VARCHAR(45),
    @docente INT
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @msg NVARCHAR(400);
    IF NOT EXISTS (SELECT 1 FROM docente WHERE cedula = @docente AND activo = 1)
    BEGIN
        SET @msg = CONCAT(N'No se puede registrar: el docente ', @docente, N' no existe o está retirado.');
        THROW 50001, @msg, 1;
    END;
    -- Red de seguridad de la regla R4 (la API ya lo valida antes).
    IF @calificacion < 0 OR @calificacion > 5
        THROW 50003, N'La calificación debe estar entre 0 y 5.', 1;

    INSERT INTO evaluacion_docente (calificacion, semestre, docente) VALUES (@calificacion, @semestre, @docente);
    DECLARE @id INT = SCOPE_IDENTITY();
    EXEC sp_consultar_evaluacion_docente @id;
END;
GO

CREATE OR ALTER PROCEDURE sp_actualizar_evaluacion_docente
    @id INT,
    @calificacion DECIMAL(4,2),
    @semestre VARCHAR(45),
    @docente INT
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @msg NVARCHAR(400);
    IF NOT EXISTS (SELECT 1 FROM evaluacion_docente WHERE id = @id AND activo = 1)
        THROW 50004, N'No se encontró la evaluación.', 1;
    IF NOT EXISTS (SELECT 1 FROM docente WHERE cedula = @docente AND activo = 1)
    BEGIN
        SET @msg = CONCAT(N'No se puede guardar: el docente ', @docente, N' no existe o está retirado.');
        THROW 50001, @msg, 1;
    END;
    IF @calificacion < 0 OR @calificacion > 5
        THROW 50003, N'La calificación debe estar entre 0 y 5.', 1;

    UPDATE evaluacion_docente SET calificacion = @calificacion, semestre = @semestre, docente = @docente
    WHERE id = @id;
    EXEC sp_consultar_evaluacion_docente @id;
END;
GO

CREATE OR ALTER PROCEDURE sp_eliminar_evaluacion_docente
    @id INT
AS
BEGIN
    SET NOCOUNT ON;
    IF NOT EXISTS (SELECT 1 FROM evaluacion_docente WHERE id = @id AND activo = 1)
        THROW 50004, N'No se encontró la evaluación.', 1;
    UPDATE evaluacion_docente SET activo = 0 WHERE id = @id;
END;
GO

CREATE OR ALTER PROCEDURE sp_crear_evaluacion_docente_lote
    @docente INT,
    @json NVARCHAR(MAX)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    DECLARE @msg NVARCHAR(400), @renglon INT;

    IF NOT EXISTS (SELECT 1 FROM docente WHERE cedula = @docente AND activo = 1)
    BEGIN
        SET @msg = CONCAT(N'No se puede registrar: el docente ', @docente, N' no existe o está retirado.');
        THROW 50001, @msg, 1;
    END;

    DECLARE @nuevos TABLE (renglon INT, calificacion DECIMAL(4,2), semestre VARCHAR(45));
    INSERT INTO @nuevos
    SELECT CAST(j.[key] AS INT) + 1, r.calificacion, r.semestre
    FROM OPENJSON(@json) j
    CROSS APPLY OPENJSON(j.[value]) WITH (calificacion DECIMAL(4,2) '$.calificacion', semestre VARCHAR(45) '$.semestre') r;

    IF NOT EXISTS (SELECT 1 FROM @nuevos)
        THROW 50003, N'El envío no trae renglones.', 1;

    SELECT TOP 1 @renglon = renglon FROM @nuevos WHERE calificacion < 0 OR calificacion > 5 ORDER BY renglon;
    IF @renglon IS NOT NULL
    BEGIN
        SET @msg = CONCAT(N'El renglón ', @renglon, N': la calificación debe estar entre 0 y 5.');
        THROW 50003, @msg, 1;
    END;

    -- El id es IDENTITY: OUTPUT recoge los ids que generó la base para devolver esas filas.
    DECLARE @ids TABLE (id INT);
    BEGIN TRANSACTION;
        INSERT INTO evaluacion_docente (calificacion, semestre, docente)
        OUTPUT INSERTED.id INTO @ids
        SELECT calificacion, semestre, @docente FROM @nuevos ORDER BY renglon;
    COMMIT TRANSACTION;

    SELECT * FROM vw_evaluacion_docente WHERE id IN (SELECT id FROM @ids) ORDER BY id;
END;
GO

-- ============================================================
-- reconocimiento (DETALLE de docente). Clave: id IDENTITY.
-- ============================================================
CREATE OR ALTER PROCEDURE sp_listar_reconocimiento
    @docente INT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SELECT * FROM vw_reconocimiento
    WHERE activo = 1 AND (@docente IS NULL OR docente = @docente)
    ORDER BY fecha, id;
END;
GO

CREATE OR ALTER PROCEDURE sp_consultar_reconocimiento
    @id INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT * FROM vw_reconocimiento WHERE id = @id AND activo = 1;
END;
GO

CREATE OR ALTER PROCEDURE sp_crear_reconocimiento
    @tipo VARCHAR(45),
    @fecha DATE,
    @institucion VARCHAR(45),
    @nombre VARCHAR(45),
    @ambito VARCHAR(45),
    @docente INT
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @msg NVARCHAR(400);
    IF NOT EXISTS (SELECT 1 FROM docente WHERE cedula = @docente AND activo = 1)
    BEGIN
        SET @msg = CONCAT(N'No se puede registrar: el docente ', @docente, N' no existe o está retirado.');
        THROW 50001, @msg, 1;
    END;
    INSERT INTO reconocimiento (tipo, fecha, institucion, nombre, ambito, docente)
    VALUES (@tipo, @fecha, @institucion, @nombre, @ambito, @docente);
    DECLARE @id INT = SCOPE_IDENTITY();
    EXEC sp_consultar_reconocimiento @id;
END;
GO

CREATE OR ALTER PROCEDURE sp_actualizar_reconocimiento
    @id INT,
    @tipo VARCHAR(45),
    @fecha DATE,
    @institucion VARCHAR(45),
    @nombre VARCHAR(45),
    @ambito VARCHAR(45),
    @docente INT
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @msg NVARCHAR(400);
    IF NOT EXISTS (SELECT 1 FROM reconocimiento WHERE id = @id AND activo = 1)
        THROW 50004, N'No se encontró el reconocimiento.', 1;
    IF NOT EXISTS (SELECT 1 FROM docente WHERE cedula = @docente AND activo = 1)
    BEGIN
        SET @msg = CONCAT(N'No se puede guardar: el docente ', @docente, N' no existe o está retirado.');
        THROW 50001, @msg, 1;
    END;
    UPDATE reconocimiento
    SET tipo = @tipo, fecha = @fecha, institucion = @institucion, nombre = @nombre,
        ambito = @ambito, docente = @docente
    WHERE id = @id;
    EXEC sp_consultar_reconocimiento @id;
END;
GO

CREATE OR ALTER PROCEDURE sp_eliminar_reconocimiento
    @id INT
AS
BEGIN
    SET NOCOUNT ON;
    IF NOT EXISTS (SELECT 1 FROM reconocimiento WHERE id = @id AND activo = 1)
        THROW 50004, N'No se encontró el reconocimiento.', 1;
    UPDATE reconocimiento SET activo = 0 WHERE id = @id;
END;
GO

CREATE OR ALTER PROCEDURE sp_crear_reconocimiento_lote
    @docente INT,
    @json NVARCHAR(MAX)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    DECLARE @msg NVARCHAR(400);

    IF NOT EXISTS (SELECT 1 FROM docente WHERE cedula = @docente AND activo = 1)
    BEGIN
        SET @msg = CONCAT(N'No se puede registrar: el docente ', @docente, N' no existe o está retirado.');
        THROW 50001, @msg, 1;
    END;

    DECLARE @nuevos TABLE (renglon INT, tipo VARCHAR(45), fecha DATE, institucion VARCHAR(45),
                           nombre VARCHAR(45), ambito VARCHAR(45));
    INSERT INTO @nuevos
    SELECT CAST(j.[key] AS INT) + 1, r.tipo, r.fecha, r.institucion, r.nombre, r.ambito
    FROM OPENJSON(@json) j
    CROSS APPLY OPENJSON(j.[value]) WITH (
        tipo VARCHAR(45) '$.tipo', fecha DATE '$.fecha', institucion VARCHAR(45) '$.institucion',
        nombre VARCHAR(45) '$.nombre', ambito VARCHAR(45) '$.ambito') r;

    IF NOT EXISTS (SELECT 1 FROM @nuevos)
        THROW 50003, N'El envío no trae renglones.', 1;

    DECLARE @ids TABLE (id INT);
    BEGIN TRANSACTION;
        INSERT INTO reconocimiento (tipo, fecha, institucion, nombre, ambito, docente)
        OUTPUT INSERTED.id INTO @ids
        SELECT tipo, fecha, institucion, nombre, ambito, @docente FROM @nuevos ORDER BY renglon;
    COMMIT TRANSACTION;

    SELECT * FROM vw_reconocimiento WHERE id IN (SELECT id FROM @ids) ORDER BY id;
END;
GO

-- ============================================================
-- experiecia (DETALLE de docente; el nombre conserva la errata del esquema, R1).
-- Clave: id IDENTITY. fecha_fin es opcional (NULL = sigue en el cargo).
-- ============================================================
CREATE OR ALTER PROCEDURE sp_listar_experiecia
    @docente INT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SELECT * FROM vw_experiecia
    WHERE activo = 1 AND (@docente IS NULL OR docente = @docente)
    ORDER BY fecha_inicio, id;
END;
GO

CREATE OR ALTER PROCEDURE sp_consultar_experiecia
    @id INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT * FROM vw_experiecia WHERE id = @id AND activo = 1;
END;
GO

CREATE OR ALTER PROCEDURE sp_crear_experiecia
    @nombre_cargo VARCHAR(45),
    @institucion VARCHAR(45),
    @tipo VARCHAR(45),
    @fecha_inicio DATE,
    @fecha_fin DATE = NULL,
    @docente INT
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @msg NVARCHAR(400);
    IF NOT EXISTS (SELECT 1 FROM docente WHERE cedula = @docente AND activo = 1)
    BEGIN
        SET @msg = CONCAT(N'No se puede registrar: el docente ', @docente, N' no existe o está retirado.');
        THROW 50001, @msg, 1;
    END;
    IF @fecha_fin IS NOT NULL AND @fecha_fin < @fecha_inicio
        THROW 50003, N'La fecha de fin no puede ser anterior a la fecha de inicio.', 1;

    INSERT INTO experiecia (nombre_cargo, institucion, tipo, fecha_inicio, fecha_fin, docente)
    VALUES (@nombre_cargo, @institucion, @tipo, @fecha_inicio, @fecha_fin, @docente);
    DECLARE @id INT = SCOPE_IDENTITY();
    EXEC sp_consultar_experiecia @id;
END;
GO

CREATE OR ALTER PROCEDURE sp_actualizar_experiecia
    @id INT,
    @nombre_cargo VARCHAR(45),
    @institucion VARCHAR(45),
    @tipo VARCHAR(45),
    @fecha_inicio DATE,
    @fecha_fin DATE = NULL,
    @docente INT
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @msg NVARCHAR(400);
    IF NOT EXISTS (SELECT 1 FROM experiecia WHERE id = @id AND activo = 1)
        THROW 50004, N'No se encontró la experiencia.', 1;
    IF NOT EXISTS (SELECT 1 FROM docente WHERE cedula = @docente AND activo = 1)
    BEGIN
        SET @msg = CONCAT(N'No se puede guardar: el docente ', @docente, N' no existe o está retirado.');
        THROW 50001, @msg, 1;
    END;
    IF @fecha_fin IS NOT NULL AND @fecha_fin < @fecha_inicio
        THROW 50003, N'La fecha de fin no puede ser anterior a la fecha de inicio.', 1;

    UPDATE experiecia
    SET nombre_cargo = @nombre_cargo, institucion = @institucion, tipo = @tipo,
        fecha_inicio = @fecha_inicio, fecha_fin = @fecha_fin, docente = @docente
    WHERE id = @id;
    EXEC sp_consultar_experiecia @id;
END;
GO

CREATE OR ALTER PROCEDURE sp_eliminar_experiecia
    @id INT
AS
BEGIN
    SET NOCOUNT ON;
    IF NOT EXISTS (SELECT 1 FROM experiecia WHERE id = @id AND activo = 1)
        THROW 50004, N'No se encontró la experiencia.', 1;
    UPDATE experiecia SET activo = 0 WHERE id = @id;
END;
GO

CREATE OR ALTER PROCEDURE sp_crear_experiecia_lote
    @docente INT,
    @json NVARCHAR(MAX)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    DECLARE @msg NVARCHAR(400), @renglon INT;

    IF NOT EXISTS (SELECT 1 FROM docente WHERE cedula = @docente AND activo = 1)
    BEGIN
        SET @msg = CONCAT(N'No se puede registrar: el docente ', @docente, N' no existe o está retirado.');
        THROW 50001, @msg, 1;
    END;

    DECLARE @nuevos TABLE (renglon INT, nombre_cargo VARCHAR(45), institucion VARCHAR(45), tipo VARCHAR(45),
                           fecha_inicio DATE, fecha_fin DATE);
    INSERT INTO @nuevos
    SELECT CAST(j.[key] AS INT) + 1, r.nombre_cargo, r.institucion, r.tipo, r.fecha_inicio, r.fecha_fin
    FROM OPENJSON(@json) j
    CROSS APPLY OPENJSON(j.[value]) WITH (
        nombre_cargo VARCHAR(45) '$.nombre_cargo', institucion VARCHAR(45) '$.institucion',
        tipo VARCHAR(45) '$.tipo', fecha_inicio DATE '$.fecha_inicio', fecha_fin DATE '$.fecha_fin') r;

    IF NOT EXISTS (SELECT 1 FROM @nuevos)
        THROW 50003, N'El envío no trae renglones.', 1;

    SELECT TOP 1 @renglon = renglon FROM @nuevos
    WHERE fecha_fin IS NOT NULL AND fecha_fin < fecha_inicio ORDER BY renglon;
    IF @renglon IS NOT NULL
    BEGIN
        SET @msg = CONCAT(N'El renglón ', @renglon, N': la fecha de fin no puede ser anterior a la de inicio.');
        THROW 50003, @msg, 1;
    END;

    DECLARE @ids TABLE (id INT);
    BEGIN TRANSACTION;
        INSERT INTO experiecia (nombre_cargo, institucion, tipo, fecha_inicio, fecha_fin, docente)
        OUTPUT INSERTED.id INTO @ids
        SELECT nombre_cargo, institucion, tipo, fecha_inicio, fecha_fin, @docente FROM @nuevos ORDER BY renglon;
    COMMIT TRANSACTION;

    SELECT * FROM vw_experiecia WHERE id IN (SELECT id FROM @ids) ORDER BY id;
END;
GO

-- ============================================================
-- apoyo_profesoral (DETALLE 1:1 de estudios_realizados).
-- La clave primaria ES la clave foránea "estudios": un estudio tiene a lo sumo un apoyo.
-- Regla R3: si existe RETIRADO, crear lo REACTIVA con los datos nuevos.
-- ============================================================
CREATE OR ALTER PROCEDURE sp_listar_apoyo_profesoral
    @estudios INT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SELECT * FROM vw_apoyo_profesoral
    WHERE activo = 1 AND (@estudios IS NULL OR estudios = @estudios)
    ORDER BY estudios;
END;
GO

CREATE OR ALTER PROCEDURE sp_consultar_apoyo_profesoral
    @estudios INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT * FROM vw_apoyo_profesoral WHERE estudios = @estudios AND activo = 1;
END;
GO

CREATE OR ALTER PROCEDURE sp_crear_apoyo_profesoral
    @estudios INT,
    @con_apoyo TINYINT,
    @institucion VARCHAR(45),
    @tipo VARCHAR(45)
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @msg NVARCHAR(400);
    IF NOT EXISTS (SELECT 1 FROM estudios_realizados WHERE id = @estudios AND activo = 1)
    BEGIN
        SET @msg = CONCAT(N'No se puede registrar: el estudio ', @estudios, N' no existe o está retirado.');
        THROW 50001, @msg, 1;
    END;
    IF EXISTS (SELECT 1 FROM apoyo_profesoral WHERE estudios = @estudios AND activo = 1)
        THROW 50002, N'Ese estudio ya tiene un apoyo profesoral registrado.', 1;

    IF EXISTS (SELECT 1 FROM apoyo_profesoral WHERE estudios = @estudios)   -- existe pero retirado
        UPDATE apoyo_profesoral
        SET con_apoyo = @con_apoyo, institucion = @institucion, tipo = @tipo, activo = 1
        WHERE estudios = @estudios;
    ELSE
        INSERT INTO apoyo_profesoral (estudios, con_apoyo, institucion, tipo)
        VALUES (@estudios, @con_apoyo, @institucion, @tipo);

    EXEC sp_consultar_apoyo_profesoral @estudios;
END;
GO

CREATE OR ALTER PROCEDURE sp_actualizar_apoyo_profesoral
    @estudios INT,
    @con_apoyo TINYINT,
    @institucion VARCHAR(45),
    @tipo VARCHAR(45)
AS
BEGIN
    SET NOCOUNT ON;
    IF NOT EXISTS (SELECT 1 FROM apoyo_profesoral WHERE estudios = @estudios AND activo = 1)
        THROW 50004, N'No se encontró el apoyo profesoral.', 1;
    UPDATE apoyo_profesoral SET con_apoyo = @con_apoyo, institucion = @institucion, tipo = @tipo
    WHERE estudios = @estudios;
    EXEC sp_consultar_apoyo_profesoral @estudios;
END;
GO

CREATE OR ALTER PROCEDURE sp_eliminar_apoyo_profesoral
    @estudios INT
AS
BEGIN
    SET NOCOUNT ON;
    IF NOT EXISTS (SELECT 1 FROM apoyo_profesoral WHERE estudios = @estudios AND activo = 1)
        THROW 50004, N'No se encontró el apoyo profesoral.', 1;
    UPDATE apoyo_profesoral SET activo = 0 WHERE estudios = @estudios;
END;
GO

-- ============================================================
-- beca (DETALLE 1:1 de estudios_realizados). Mismo patrón que apoyo_profesoral.
-- ============================================================
CREATE OR ALTER PROCEDURE sp_listar_beca
    @estudios INT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SELECT * FROM vw_beca
    WHERE activo = 1 AND (@estudios IS NULL OR estudios = @estudios)
    ORDER BY estudios;
END;
GO

CREATE OR ALTER PROCEDURE sp_consultar_beca
    @estudios INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT * FROM vw_beca WHERE estudios = @estudios AND activo = 1;
END;
GO

CREATE OR ALTER PROCEDURE sp_crear_beca
    @estudios INT,
    @tipo VARCHAR(45),
    @institucion VARCHAR(80),
    @fecha_inicio DATE,
    @fecha_fin DATE = NULL
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @msg NVARCHAR(400);
    IF NOT EXISTS (SELECT 1 FROM estudios_realizados WHERE id = @estudios AND activo = 1)
    BEGIN
        SET @msg = CONCAT(N'No se puede registrar: el estudio ', @estudios, N' no existe o está retirado.');
        THROW 50001, @msg, 1;
    END;
    IF EXISTS (SELECT 1 FROM beca WHERE estudios = @estudios AND activo = 1)
        THROW 50002, N'Ese estudio ya tiene una beca registrada.', 1;
    IF @fecha_fin IS NOT NULL AND @fecha_fin < @fecha_inicio
        THROW 50003, N'La fecha de fin no puede ser anterior a la fecha de inicio.', 1;

    IF EXISTS (SELECT 1 FROM beca WHERE estudios = @estudios)   -- existe pero retirada: se reactiva (R3)
        UPDATE beca
        SET tipo = @tipo, institucion = @institucion, fecha_inicio = @fecha_inicio, fecha_fin = @fecha_fin, activo = 1
        WHERE estudios = @estudios;
    ELSE
        INSERT INTO beca (estudios, tipo, institucion, fecha_inicio, fecha_fin)
        VALUES (@estudios, @tipo, @institucion, @fecha_inicio, @fecha_fin);

    EXEC sp_consultar_beca @estudios;
END;
GO

CREATE OR ALTER PROCEDURE sp_actualizar_beca
    @estudios INT,
    @tipo VARCHAR(45),
    @institucion VARCHAR(80),
    @fecha_inicio DATE,
    @fecha_fin DATE = NULL
AS
BEGIN
    SET NOCOUNT ON;
    IF NOT EXISTS (SELECT 1 FROM beca WHERE estudios = @estudios AND activo = 1)
        THROW 50004, N'No se encontró la beca.', 1;
    IF @fecha_fin IS NOT NULL AND @fecha_fin < @fecha_inicio
        THROW 50003, N'La fecha de fin no puede ser anterior a la fecha de inicio.', 1;
    UPDATE beca SET tipo = @tipo, institucion = @institucion, fecha_inicio = @fecha_inicio, fecha_fin = @fecha_fin
    WHERE estudios = @estudios;
    EXEC sp_consultar_beca @estudios;
END;
GO

CREATE OR ALTER PROCEDURE sp_eliminar_beca
    @estudios INT
AS
BEGIN
    SET NOCOUNT ON;
    IF NOT EXISTS (SELECT 1 FROM beca WHERE estudios = @estudios AND activo = 1)
        THROW 50004, N'No se encontró la beca.', 1;
    UPDATE beca SET activo = 0 WHERE estudios = @estudios;
END;
GO

-- ============================================================
-- docente_departamento (PUENTE docente <-> programa, con atributos).
-- Clave compuesta (docente, departamento). "departamento" apunta a programa(id).
-- Las dos claves NO se editan: solo los atributos.
-- ============================================================
CREATE OR ALTER PROCEDURE sp_listar_docente_departamento
    @docente INT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SELECT * FROM vw_docente_departamento
    WHERE activo = 1 AND (@docente IS NULL OR docente = @docente)
    ORDER BY docente, departamento;
END;
GO

CREATE OR ALTER PROCEDURE sp_consultar_docente_departamento
    @docente INT,
    @departamento INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT * FROM vw_docente_departamento
    WHERE docente = @docente AND departamento = @departamento AND activo = 1;
END;
GO

CREATE OR ALTER PROCEDURE sp_crear_docente_departamento
    @docente INT,
    @departamento INT,
    @dedicacion VARCHAR(15),
    @modalidad VARCHAR(45),
    @fecha_ingreso DATE,
    @fecha_salida DATE = NULL
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @msg NVARCHAR(400);
    IF NOT EXISTS (SELECT 1 FROM docente WHERE cedula = @docente AND activo = 1)
    BEGIN
        SET @msg = CONCAT(N'No se puede registrar: el docente ', @docente, N' no existe o está retirado.');
        THROW 50001, @msg, 1;
    END;
    IF NOT EXISTS (SELECT 1 FROM programa WHERE id = @departamento AND activo = 1)
    BEGIN
        SET @msg = CONCAT(N'No se puede registrar: el programa ', @departamento, N' no existe o está retirado.');
        THROW 50001, @msg, 1;
    END;
    IF EXISTS (SELECT 1 FROM docente_departamento WHERE docente = @docente AND departamento = @departamento AND activo = 1)
    BEGIN
        SET @msg = CONCAT(N'El docente ', @docente, N' ya tiene asignado el programa ', @departamento, N'.');
        THROW 50002, @msg, 1;
    END;
    IF @fecha_salida IS NOT NULL AND @fecha_salida < @fecha_ingreso
        THROW 50003, N'La fecha de salida no puede ser anterior a la fecha de ingreso.', 1;

    IF EXISTS (SELECT 1 FROM docente_departamento WHERE docente = @docente AND departamento = @departamento)
        UPDATE docente_departamento        -- la pareja existía retirada: se reactiva (R3)
        SET dedicacion = @dedicacion, modalidad = @modalidad, fecha_ingreso = @fecha_ingreso,
            fecha_salida = @fecha_salida, activo = 1
        WHERE docente = @docente AND departamento = @departamento;
    ELSE
        INSERT INTO docente_departamento (docente, departamento, dedicacion, modalidad, fecha_ingreso, fecha_salida)
        VALUES (@docente, @departamento, @dedicacion, @modalidad, @fecha_ingreso, @fecha_salida);

    EXEC sp_consultar_docente_departamento @docente, @departamento;
END;
GO

CREATE OR ALTER PROCEDURE sp_actualizar_docente_departamento
    @docente INT,
    @departamento INT,
    @dedicacion VARCHAR(15),
    @modalidad VARCHAR(45),
    @fecha_ingreso DATE,
    @fecha_salida DATE = NULL
AS
BEGIN
    SET NOCOUNT ON;
    IF NOT EXISTS (SELECT 1 FROM docente_departamento WHERE docente = @docente AND departamento = @departamento AND activo = 1)
        THROW 50004, N'No se encontró esa asignación de programa.', 1;
    IF @fecha_salida IS NOT NULL AND @fecha_salida < @fecha_ingreso
        THROW 50003, N'La fecha de salida no puede ser anterior a la fecha de ingreso.', 1;
    UPDATE docente_departamento
    SET dedicacion = @dedicacion, modalidad = @modalidad, fecha_ingreso = @fecha_ingreso, fecha_salida = @fecha_salida
    WHERE docente = @docente AND departamento = @departamento;
    EXEC sp_consultar_docente_departamento @docente, @departamento;
END;
GO

CREATE OR ALTER PROCEDURE sp_eliminar_docente_departamento
    @docente INT,
    @departamento INT
AS
BEGIN
    SET NOCOUNT ON;
    IF NOT EXISTS (SELECT 1 FROM docente_departamento WHERE docente = @docente AND departamento = @departamento AND activo = 1)
        THROW 50004, N'No se encontró esa asignación de programa.', 1;
    UPDATE docente_departamento SET activo = 0 WHERE docente = @docente AND departamento = @departamento;
END;
GO

-- ============================================================
-- red_docente (PUENTE red <-> docente, con atributos). Clave (red, docente).
-- fecha_fin es VARCHAR(45) en el esquema: se conserva como texto.
-- ============================================================
CREATE OR ALTER PROCEDURE sp_listar_red_docente
    @docente INT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SELECT * FROM vw_red_docente
    WHERE activo = 1 AND (@docente IS NULL OR docente = @docente)
    ORDER BY red, docente;
END;
GO

CREATE OR ALTER PROCEDURE sp_consultar_red_docente
    @red INT,
    @docente INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT * FROM vw_red_docente WHERE red = @red AND docente = @docente AND activo = 1;
END;
GO

CREATE OR ALTER PROCEDURE sp_crear_red_docente
    @red INT,
    @docente INT,
    @fecha_inicio DATE,
    @fecha_fin VARCHAR(45) = NULL,
    @act_destacadas VARCHAR(MAX)
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @msg NVARCHAR(400);
    IF NOT EXISTS (SELECT 1 FROM red WHERE idr = @red AND activo = 1)
    BEGIN
        SET @msg = CONCAT(N'No se puede registrar: la red ', @red, N' no existe o está retirada.');
        THROW 50001, @msg, 1;
    END;
    IF NOT EXISTS (SELECT 1 FROM docente WHERE cedula = @docente AND activo = 1)
    BEGIN
        SET @msg = CONCAT(N'No se puede registrar: el docente ', @docente, N' no existe o está retirado.');
        THROW 50001, @msg, 1;
    END;
    IF EXISTS (SELECT 1 FROM red_docente WHERE red = @red AND docente = @docente AND activo = 1)
    BEGIN
        SET @msg = CONCAT(N'El docente ', @docente, N' ya pertenece a la red ', @red, N'.');
        THROW 50002, @msg, 1;
    END;

    IF EXISTS (SELECT 1 FROM red_docente WHERE red = @red AND docente = @docente)
        UPDATE red_docente                 -- reactivación (R3)
        SET fecha_inicio = @fecha_inicio, fecha_fin = @fecha_fin, act_destacadas = @act_destacadas, activo = 1
        WHERE red = @red AND docente = @docente;
    ELSE
        INSERT INTO red_docente (red, docente, fecha_inicio, fecha_fin, act_destacadas)
        VALUES (@red, @docente, @fecha_inicio, @fecha_fin, @act_destacadas);

    EXEC sp_consultar_red_docente @red, @docente;
END;
GO

CREATE OR ALTER PROCEDURE sp_actualizar_red_docente
    @red INT,
    @docente INT,
    @fecha_inicio DATE,
    @fecha_fin VARCHAR(45) = NULL,
    @act_destacadas VARCHAR(MAX)
AS
BEGIN
    SET NOCOUNT ON;
    IF NOT EXISTS (SELECT 1 FROM red_docente WHERE red = @red AND docente = @docente AND activo = 1)
        THROW 50004, N'No se encontró esa participación en la red.', 1;
    UPDATE red_docente SET fecha_inicio = @fecha_inicio, fecha_fin = @fecha_fin, act_destacadas = @act_destacadas
    WHERE red = @red AND docente = @docente;
    EXEC sp_consultar_red_docente @red, @docente;
END;
GO

CREATE OR ALTER PROCEDURE sp_eliminar_red_docente
    @red INT,
    @docente INT
AS
BEGIN
    SET NOCOUNT ON;
    IF NOT EXISTS (SELECT 1 FROM red_docente WHERE red = @red AND docente = @docente AND activo = 1)
        THROW 50004, N'No se encontró esa participación en la red.', 1;
    UPDATE red_docente SET activo = 0 WHERE red = @red AND docente = @docente;
END;
GO

-- ============================================================
-- intereses_futuros (PUENTE PURO docente <-> termino_clave). Clave (docente, termino_clave).
-- Regla R6: una pareja existe o no existe -> NO hay sp_actualizar_.
-- ============================================================
CREATE OR ALTER PROCEDURE sp_listar_intereses_futuros
    @docente INT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SELECT * FROM vw_intereses_futuros
    WHERE activo = 1 AND (@docente IS NULL OR docente = @docente)
    ORDER BY docente, termino_clave;
END;
GO

CREATE OR ALTER PROCEDURE sp_consultar_intereses_futuros
    @docente INT,
    @termino_clave VARCHAR(30)
AS
BEGIN
    SET NOCOUNT ON;
    SELECT * FROM vw_intereses_futuros WHERE docente = @docente AND termino_clave = @termino_clave AND activo = 1;
END;
GO

CREATE OR ALTER PROCEDURE sp_crear_intereses_futuros
    @docente INT,
    @termino_clave VARCHAR(30)
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @msg NVARCHAR(400);
    IF NOT EXISTS (SELECT 1 FROM docente WHERE cedula = @docente AND activo = 1)
    BEGIN
        SET @msg = CONCAT(N'No se puede registrar: el docente ', @docente, N' no existe o está retirado.');
        THROW 50001, @msg, 1;
    END;
    IF NOT EXISTS (SELECT 1 FROM termino_clave WHERE termino = @termino_clave AND activo = 1)
    BEGIN
        SET @msg = CONCAT(N'No se puede registrar: el término clave «', @termino_clave, N'» no existe o está retirado.');
        THROW 50001, @msg, 1;
    END;
    IF EXISTS (SELECT 1 FROM intereses_futuros WHERE docente = @docente AND termino_clave = @termino_clave AND activo = 1)
    BEGIN
        SET @msg = CONCAT(N'El docente ', @docente, N' ya tiene el interés «', @termino_clave, N'».');
        THROW 50002, @msg, 1;
    END;

    IF EXISTS (SELECT 1 FROM intereses_futuros WHERE docente = @docente AND termino_clave = @termino_clave)
        UPDATE intereses_futuros SET activo = 1 WHERE docente = @docente AND termino_clave = @termino_clave;  -- R3
    ELSE
        INSERT INTO intereses_futuros (docente, termino_clave) VALUES (@docente, @termino_clave);

    EXEC sp_consultar_intereses_futuros @docente, @termino_clave;
END;
GO

CREATE OR ALTER PROCEDURE sp_eliminar_intereses_futuros
    @docente INT,
    @termino_clave VARCHAR(30)
AS
BEGIN
    SET NOCOUNT ON;
    IF NOT EXISTS (SELECT 1 FROM intereses_futuros WHERE docente = @docente AND termino_clave = @termino_clave AND activo = 1)
        THROW 50004, N'No se encontró ese interés del docente.', 1;
    UPDATE intereses_futuros SET activo = 0 WHERE docente = @docente AND termino_clave = @termino_clave;
END;
GO

-- ============================================================
-- estudio_ac (PUENTE PURO estudios_realizados <-> area_conocimiento). Clave (estudio, area_conocimiento).
-- Sin sp_actualizar_ (R6).
-- ============================================================
CREATE OR ALTER PROCEDURE sp_listar_estudio_ac
    @estudio INT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SELECT * FROM vw_estudio_ac
    WHERE activo = 1 AND (@estudio IS NULL OR estudio = @estudio)
    ORDER BY estudio, area_conocimiento;
END;
GO

CREATE OR ALTER PROCEDURE sp_consultar_estudio_ac
    @estudio INT,
    @area_conocimiento VARCHAR(6)
AS
BEGIN
    SET NOCOUNT ON;
    SELECT * FROM vw_estudio_ac WHERE estudio = @estudio AND area_conocimiento = @area_conocimiento AND activo = 1;
END;
GO

CREATE OR ALTER PROCEDURE sp_crear_estudio_ac
    @estudio INT,
    @area_conocimiento VARCHAR(6)
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @msg NVARCHAR(400);
    IF NOT EXISTS (SELECT 1 FROM estudios_realizados WHERE id = @estudio AND activo = 1)
    BEGIN
        SET @msg = CONCAT(N'No se puede registrar: el estudio ', @estudio, N' no existe o está retirado.');
        THROW 50001, @msg, 1;
    END;
    IF NOT EXISTS (SELECT 1 FROM area_conocimiento WHERE id = @area_conocimiento AND activo = 1)
    BEGIN
        SET @msg = CONCAT(N'No se puede registrar: el área de conocimiento ', @area_conocimiento, N' no existe.');
        THROW 50001, @msg, 1;
    END;
    IF EXISTS (SELECT 1 FROM estudio_ac WHERE estudio = @estudio AND area_conocimiento = @area_conocimiento AND activo = 1)
    BEGIN
        SET @msg = CONCAT(N'El estudio ', @estudio, N' ya tiene asignada el área ', @area_conocimiento, N'.');
        THROW 50002, @msg, 1;
    END;

    IF EXISTS (SELECT 1 FROM estudio_ac WHERE estudio = @estudio AND area_conocimiento = @area_conocimiento)
        UPDATE estudio_ac SET activo = 1 WHERE estudio = @estudio AND area_conocimiento = @area_conocimiento;  -- R3
    ELSE
        INSERT INTO estudio_ac (estudio, area_conocimiento) VALUES (@estudio, @area_conocimiento);

    EXEC sp_consultar_estudio_ac @estudio, @area_conocimiento;
END;
GO

CREATE OR ALTER PROCEDURE sp_eliminar_estudio_ac
    @estudio INT,
    @area_conocimiento VARCHAR(6)
AS
BEGIN
    SET NOCOUNT ON;
    IF NOT EXISTS (SELECT 1 FROM estudio_ac WHERE estudio = @estudio AND area_conocimiento = @area_conocimiento AND activo = 1)
        THROW 50004, N'No se encontró esa área en el estudio.', 1;
    UPDATE estudio_ac SET activo = 0 WHERE estudio = @estudio AND area_conocimiento = @area_conocimiento;
END;
GO

-- ############################################################
-- PARTE D — DISPARADORES
-- ############################################################

-- ============================================================
-- trg_contar_estudios  (Propuesta A del profesor: contador en el maestro)
--
-- QUÉ HACE: cada vez que cambia estudios_realizados, recalcula
--   docente.total_estudios = cuántos estudios ACTIVOS tiene el docente.
--
-- POR QUÉ ESCUCHA INSERT, UPDATE **Y** DELETE: el borrado es lógico, así que
--   retirar un estudio es un UPDATE (activo = 0). Si solo oyera INSERT y
--   DELETE, el contador quedaría mal justo cuando alguien retira algo.
--
-- POR QUÉ CUENTA CON COUNT(*) Y NO SUMA +1 / -1: si algún día el número se
--   desfasa (alguien edita la base a mano), la siguiente operación lo corrige.
--
-- POR QUÉ "POR CONJUNTOS": inserted y deleted pueden traer MUCHAS filas
--   (un envío de varios renglones es un solo INSERT). Se actualizan todos
--   los docentes tocados en una sola pasada: el viejo y el nuevo, por si un
--   estudio cambió de docente.
-- ============================================================
CREATE OR ALTER TRIGGER trg_contar_estudios
ON estudios_realizados
AFTER INSERT, UPDATE, DELETE
AS
BEGIN
    SET NOCOUNT ON;

    -- Salida temprana: la operación no afectó ninguna fila.
    IF NOT EXISTS (SELECT 1 FROM inserted) AND NOT EXISTS (SELECT 1 FROM deleted)
        RETURN;

    UPDATE d
    SET d.total_estudios = (SELECT COUNT(*)
                            FROM estudios_realizados e
                            WHERE e.docente = d.cedula AND e.activo = 1)
    FROM docente d
    WHERE d.cedula IN (SELECT docente FROM inserted
                       UNION
                       SELECT docente FROM deleted);
END;
GO

-- ============================================================
-- trg_sello_docente  (Propuesta B del profesor: sello de modificación)
--
-- QUÉ HACE: cada vez que alguien MODIFICA un docente, pone
--   fecha_actualizacion = hoy. Así nadie se puede olvidar de actualizarla.
--
-- POR QUÉ LA CONDICIÓN UPDATE(...): cuando trg_contar_estudios cambia
--   total_estudios, eso también es un UPDATE sobre docente; pero no es un
--   cambio que haya hecho una persona, así que no debe mover la fecha.
--   UPDATE(columna) dice si la sentencia tocó esa columna.
--
-- No se llama a sí mismo: el UPDATE de abajo solo toca fecha_actualizacion,
--   que no está en la lista, y SQL Server trae la recursión directa apagada.
-- ============================================================
CREATE OR ALTER TRIGGER trg_sello_docente
ON docente
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    IF NOT EXISTS (SELECT 1 FROM inserted) RETURN;

    IF UPDATE(nombres) OR UPDATE(apellidos) OR UPDATE(genero) OR UPDATE(cargo)
       OR UPDATE(fecha_nacimiento) OR UPDATE(correo) OR UPDATE(telefono) OR UPDATE(url_cvlac)
       OR UPDATE(escalafon) OR UPDATE(perfil) OR UPDATE(cat_minciencia) OR UPDATE(conv_minciencia)
       OR UPDATE(nacionalidaad) OR UPDATE(linea_investigacion_principal) OR UPDATE(activo)
    BEGIN
        UPDATE d SET d.fecha_actualizacion = CAST(GETDATE() AS DATE)
        FROM docente d
        JOIN inserted i ON i.cedula = d.cedula;
    END;
END;
GO

-- Por si ya había estudios cargados antes de crear el disparador: deja el contador al día.
UPDATE d SET d.total_estudios = (SELECT COUNT(*) FROM estudios_realizados e WHERE e.docente = d.cedula AND e.activo = 1)
FROM docente d;
GO

PRINT N'v2_procedimientos.sql aplicado: vistas, procedimientos y disparadores listos.';
GO
