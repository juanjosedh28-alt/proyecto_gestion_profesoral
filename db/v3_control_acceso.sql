-- ============================================================
-- VERSIÓN 3 — Control de acceso
-- Base: gestion_local (SQL Server 2022)
--
-- QUÉ HACE ESTE ARCHIVO
--   1. Crea las dos tablas del PERMISO: ruta (qué se puede proteger) y
--      ruta_rol (qué rol entra a qué ruta). usuario, rol y rol_usuario ya
--      existían desde la v1 del esquema.
--   2. Siembra los 3 roles, las 11 rutas, los permisos de cada rol y 3
--      usuarios de prueba CON LA CONTRASEÑA CON HASH (bcrypt).
--   3. Crea los procedimientos del acceso:
--        sp_verificar_acceso_ruta  ¿este correo puede entrar a esta ruta?  (el 403)
--        sp_rutas_permitidas       las rutas de este correo                (el menú)
--        sp_credenciales_usuario   el hash guardado, para iniciar sesión
--        sp_roles_de_usuario       los nombres de sus roles, para el token
--   4. Crea el CRUD por procedimientos de usuario, rol, rol_usuario, ruta y
--      ruta_rol (las pantallas de administración; solo el Administrador).
--
-- LAS TRES IDEAS DE LA V3 (docs del curso: CONCEPTOS_CONTROL_DE_ACCESO.md)
--   - La contraseña NO se guarda: se guarda su hash bcrypt. La base nunca ve
--     la contraseña; la API calcula el hash y manda solo eso.
--   - El permiso NO va en el token: sp_verificar_acceso_ruta se llama en CADA
--     petición. Por eso quitar una fila de ruta_rol surte efecto de inmediato,
--     sin que nadie vuelva a iniciar sesión (criterio 7).
--   - La decisión la toma ESTA base: la API no escribe el JOIN de permisos.
--
-- Mismos códigos de error que la v2: 50001 padre inexistente (409),
-- 50002 repetido o no permitido (409), 50003 dato inválido (422),
-- 50004 no encontrado (404).
--
-- Es IDEMPOTENTE: init.sh lo corre en cada arranque. Las tablas y la semilla
-- solo se crean si faltan, así que los cambios que haga el administrador
-- (por ejemplo, quitar un permiso) NO se deshacen al reiniciar.
-- ============================================================

USE gestion_local;
GO

-- ============================================================
-- 1. TABLAS NUEVAS
-- ============================================================

-- ruta: las INTERFACES y ACCIONES que se pueden proteger. Llevan punto
-- (interfaz.docentes, datos.modificar) y no barra, para no confundirlas con
-- las direcciones de la API (/api/docente): son otra cosa.
IF OBJECT_ID('dbo.ruta', 'U') IS NULL
    CREATE TABLE ruta (
        id INT IDENTITY(1,1) PRIMARY KEY,
        nombre VARCHAR(60) NOT NULL UNIQUE,
        descripcion VARCHAR(200) NOT NULL
    );
GO

-- ruta_rol: QUÉ ROL ENTRA A QUÉ RUTA (la puente rol <-> permiso de RBAC).
-- Quitar una fila de aquí le quita el permiso a ese rol en la siguiente
-- petición. ON DELETE CASCADE: si desaparece una ruta o un rol, sus permisos
-- se van con él.
IF OBJECT_ID('dbo.ruta_rol', 'U') IS NULL
    CREATE TABLE ruta_rol (
        ruta_id INT NOT NULL,
        rol_id INT NOT NULL,
        PRIMARY KEY (ruta_id, rol_id),
        FOREIGN KEY (ruta_id) REFERENCES ruta(id) ON DELETE CASCADE,
        FOREIGN KEY (rol_id) REFERENCES rol(id) ON DELETE CASCADE
    );
GO

-- ============================================================
-- 2. SEMILLA
-- ============================================================

-- 2.1 Roles (por nombre: si ya existen, no se tocan)
IF NOT EXISTS (SELECT 1 FROM rol WHERE nombre = 'Administrador')
    INSERT INTO rol (nombre, descripcion, activo) VALUES ('Administrador', 'Todo el sistema, incluidos usuarios, roles y permisos', 1);
IF NOT EXISTS (SELECT 1 FROM rol WHERE nombre = 'Editor')
    INSERT INTO rol (nombre, descripcion, activo) VALUES ('Editor', 'Ve y modifica los datos del módulo; no administra usuarios', 1);
IF NOT EXISTS (SELECT 1 FROM rol WHERE nombre = 'Consulta')
    INSERT INTO rol (nombre, descripcion, activo) VALUES ('Consulta', 'Solo ve los datos del módulo; no puede crear, editar ni retirar', 1);
GO

-- 2.2 Rutas. Una ruta NUEVA se le da también al Administrador, para que
--     nunca quede algo que nadie pueda abrir.
DECLARE @rutas TABLE (nombre VARCHAR(60), descripcion VARCHAR(200));
INSERT INTO @rutas VALUES
    ('interfaz.inicio',    'Página de inicio'),
    ('interfaz.areas',     'Áreas de conocimiento (consulta)'),
    ('interfaz.terminos',  'Términos clave'),
    ('interfaz.lineas',    'Líneas de investigación'),
    ('interfaz.programas', 'Programas'),
    ('interfaz.redes',     'Redes'),
    ('interfaz.docentes',  'Docentes y todo su detalle'),
    ('interfaz.usuarios',  'Gestión de usuarios y de sus roles'),
    ('interfaz.roles',     'Gestión de roles'),
    ('interfaz.permisos',  'Permisos de cada rol'),
    ('datos.modificar',    'Crear, editar y retirar registros del módulo');

DECLARE @nuevas TABLE (id INT);
INSERT INTO ruta (nombre, descripcion)
OUTPUT INSERTED.id INTO @nuevas
SELECT n.nombre, n.descripcion FROM @rutas n
WHERE NOT EXISTS (SELECT 1 FROM ruta r WHERE r.nombre = n.nombre);

-- Solo si ya había permisos sembrados (si no, lo hace el punto 2.3).
IF EXISTS (SELECT 1 FROM ruta_rol)
    INSERT INTO ruta_rol (ruta_id, rol_id)
    SELECT n.id, r.id FROM @nuevas n CROSS JOIN rol r WHERE r.nombre = 'Administrador';
GO

-- 2.3 Permisos de cada rol: SOLO LA PRIMERA VEZ (tabla vacía). Si se
--     sembraran en cada arranque, un permiso que el administrador quitó
--     volvería a aparecer al reiniciar.
IF NOT EXISTS (SELECT 1 FROM ruta_rol)
BEGIN
    -- Administrador: todas las rutas
    INSERT INTO ruta_rol (ruta_id, rol_id)
    SELECT ru.id, ro.id FROM ruta ru CROSS JOIN rol ro WHERE ro.nombre = 'Administrador';

    -- Editor: el módulo completo y puede escribir
    INSERT INTO ruta_rol (ruta_id, rol_id)
    SELECT ru.id, ro.id FROM ruta ru CROSS JOIN rol ro
    WHERE ro.nombre = 'Editor'
      AND ru.nombre IN ('interfaz.inicio', 'interfaz.areas', 'interfaz.terminos', 'interfaz.lineas',
                        'interfaz.programas', 'interfaz.redes', 'interfaz.docentes', 'datos.modificar');

    -- Consulta: el módulo completo, SIN datos.modificar
    INSERT INTO ruta_rol (ruta_id, rol_id)
    SELECT ru.id, ro.id FROM ruta ru CROSS JOIN rol ro
    WHERE ro.nombre = 'Consulta'
      AND ru.nombre IN ('interfaz.inicio', 'interfaz.areas', 'interfaz.terminos', 'interfaz.lineas',
                        'interfaz.programas', 'interfaz.redes', 'interfaz.docentes');
END;
GO

-- 2.4 Usuarios de prueba, CON HASH.
--
-- Del hash no se puede volver a la contraseña: por eso las contraseñas en
-- claro están escritas aquí y en 7_quickstart.md. Sin saberlas no se puede
-- iniciar sesión, y sin iniciar sesión no se prueba nada de la v3.
--
--   admin@gestion.edu.co      admin123      Administrador
--   editor@gestion.edu.co     editor123     Editor
--   consulta@gestion.edu.co   consulta123   Consulta
--
-- El hash es bcrypt con costo 12: '$2a$12$' lo dice ('2a' es la variante,
-- '12' el costo). Los 22 caracteres que siguen son el SALT, distinto en cada
-- hash: dos personas con la misma contraseña tendrían hashes distintos.
--
-- Cada usuario se crea (con su rol) solo si su correo no existe.
DECLARE @semilla TABLE (username VARCHAR(100), email VARCHAR(150), nombre VARCHAR(200), hash VARCHAR(255), rol VARCHAR(100));
INSERT INTO @semilla VALUES
    ('admin',    'admin@gestion.edu.co',    'Administrador del sistema', '$2a$12$P93xWWYZySE9L/XEum6DO.qO5eEvaObQe0DA9kQbQmu.6PQqg7qz.', 'Administrador'),
    ('editor',   'editor@gestion.edu.co',   'Editor de prueba',          '$2a$12$0vaHHcWcPuAhNwGLjorp.OPF4BP9B4hPUeqHND5RcAKlA9PqUyX86', 'Editor'),
    ('consulta', 'consulta@gestion.edu.co', 'Consulta de prueba',        '$2a$12$IKcX64VSvea5lEHpvuEiXO7NG722R6Tv0NH.u8195dNgHYgtvt8fm', 'Consulta');

DECLARE @creados TABLE (id INT, email VARCHAR(150));
INSERT INTO usuario (username, password, email, nombre_completo, activo)
OUTPUT INSERTED.id, INSERTED.email INTO @creados
SELECT s.username, s.hash, s.email, s.nombre, 1 FROM @semilla s
WHERE NOT EXISTS (SELECT 1 FROM usuario u WHERE u.email = s.email OR u.username = s.username);

INSERT INTO rol_usuario (usuario_id, rol_id)
SELECT c.id, r.id
FROM @creados c
JOIN @semilla s ON s.email = c.email
JOIN rol r ON r.nombre = s.rol;
GO

-- ============================================================
-- 3. PROCEDIMIENTOS DEL ACCESO
-- ============================================================

-- ------------------------------------------------------------
-- sp_verificar_acceso_ruta — EL CORAZÓN DE LA V3
--
-- QUÉ HACE: responde 1 si ese correo tiene al menos un rol ACTIVO al que
-- ruta_rol le da esa ruta; si no, 0. Recorre RBAC:
--     usuario -> rol_usuario -> rol -> ruta_rol -> ruta
-- QUIÉN LO LLAMA: el filtro ExigePermiso de la API, en CADA petición.
-- POR QUÉ AQUÍ Y NO EN C#: la regla queda en un solo sitio. Si el JOIN
-- estuviera también en la API, el día que cambie, cambiaría en uno.
-- Empieza en 0: sin acceso hasta que se demuestre lo contrario. Una ruta que
-- no existe da 0: nadie entra.
-- ------------------------------------------------------------
CREATE OR ALTER PROCEDURE sp_verificar_acceso_ruta
    @email VARCHAR(150),
    @ruta VARCHAR(60)
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @tiene_acceso BIT = 0;

    IF EXISTS (
        SELECT 1
        FROM usuario u
        JOIN rol_usuario ru ON ru.usuario_id = u.id
        JOIN rol r          ON r.id = ru.rol_id AND r.activo = 1
        JOIN ruta_rol rr    ON rr.rol_id = r.id
        JOIN ruta t         ON t.id = rr.ruta_id
        WHERE u.email = @email AND u.activo = 1 AND t.nombre = @ruta
    )
        SET @tiene_acceso = 1;

    SELECT @tiene_acceso AS tiene_acceso;
END;
GO

-- sp_rutas_permitidas — las rutas de un correo, para ARMAR EL MENÚ.
-- No protege nada: el menú es comodidad. La protección es sp_verificar_acceso_ruta.
-- DISTINCT: si dos roles dan la misma ruta, sale una vez.
CREATE OR ALTER PROCEDURE sp_rutas_permitidas
    @email VARCHAR(150)
AS
BEGIN
    SET NOCOUNT ON;
    SELECT DISTINCT t.nombre AS ruta
    FROM usuario u
    JOIN rol_usuario ru ON ru.usuario_id = u.id
    JOIN rol r          ON r.id = ru.rol_id AND r.activo = 1
    JOIN ruta_rol rr    ON rr.rol_id = r.id
    JOIN ruta t         ON t.id = rr.ruta_id
    WHERE u.email = @email AND u.activo = 1
    ORDER BY ruta;
END;
GO

-- sp_credenciales_usuario — el HASH guardado de un correo ACTIVO, para que la
-- API lo compare con bcrypt. Es el ÚNICO procedimiento que devuelve el hash, y
-- solo lo usa el inicio de sesión: ninguna respuesta de la API lo lleva.
-- Sin fila = el correo no existe o está retirado: la API responde lo mismo
-- que con una contraseña mala.
CREATE OR ALTER PROCEDURE sp_credenciales_usuario
    @email VARCHAR(150)
AS
BEGIN
    SET NOCOUNT ON;
    SELECT id, email, nombre_completo, password AS hash
    FROM usuario
    WHERE email = @email AND activo = 1;
END;
GO

-- sp_roles_de_usuario — los NOMBRES de los roles activos de un correo. Van en
-- el token solo para mostrarlos; los permisos NO se deciden con ellos.
CREATE OR ALTER PROCEDURE sp_roles_de_usuario
    @email VARCHAR(150)
AS
BEGIN
    SET NOCOUNT ON;
    SELECT r.nombre AS rol
    FROM usuario u
    JOIN rol_usuario ru ON ru.usuario_id = u.id
    JOIN rol r          ON r.id = ru.rol_id AND r.activo = 1
    WHERE u.email = @email
    ORDER BY r.nombre;
END;
GO

-- ============================================================
-- 4. CRUD DE LA ADMINISTRACIÓN
-- ============================================================

-- ------------------------------------------------------------
-- usuario
-- La vista NO tiene la columna password: así ningún listado puede devolver
-- el hash por descuido. roles = sus roles activos, separados por coma.
-- ------------------------------------------------------------
CREATE OR ALTER VIEW vw_usuario AS
SELECT u.id, u.username, u.email, u.nombre_completo,
       CONVERT(CHAR(10), u.fecha_creacion, 23) AS fecha_creacion,
       CONVERT(CHAR(10), u.fecha_actualizacion, 23) AS fecha_actualizacion,
       ISNULL((SELECT STRING_AGG(r.nombre, ', ') WITHIN GROUP (ORDER BY r.nombre)
               FROM rol_usuario ru JOIN rol r ON r.id = ru.rol_id AND r.activo = 1
               WHERE ru.usuario_id = u.id), '') AS roles,
       u.activo
FROM usuario u;
GO

CREATE OR ALTER PROCEDURE sp_listar_usuario
AS
BEGIN
    SET NOCOUNT ON;
    SELECT id, username, email, nombre_completo, fecha_creacion, fecha_actualizacion, roles
    FROM vw_usuario WHERE activo = 1 ORDER BY username;
END;
GO

CREATE OR ALTER PROCEDURE sp_consultar_usuario
    @id INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT id, username, email, nombre_completo, fecha_creacion, fecha_actualizacion, roles
    FROM vw_usuario WHERE id = @id AND activo = 1;
END;
GO

-- ¿Quedaría el sistema sin ningún administrador activo si @sin_usuario deja
-- de serlo? Lo usan actualizar, eliminar y quitar rol: si nadie fuera
-- Administrador, nadie podría volver a administrar usuarios ni permisos.
CREATE OR ALTER PROCEDURE sp_revisar_ultimo_administrador
    @sin_usuario INT
AS
BEGIN
    SET NOCOUNT ON;
    IF NOT EXISTS (
        SELECT 1 FROM usuario u
        JOIN rol_usuario ru ON ru.usuario_id = u.id
        JOIN rol r ON r.id = ru.rol_id AND r.nombre = 'Administrador'
        WHERE u.activo = 1 AND u.id <> @sin_usuario
    )
        THROW 50002, N'No se puede: es el último administrador activo y el sistema quedaría sin quien lo administre.', 1;
END;
GO

-- Crear un usuario con sus roles en UN solo envío (todo o nada).
-- @password_hash: el hash bcrypt que calculó la API. La contraseña en claro
--                 NUNCA llega a la base.
-- @roles_json:    los ids de sus roles, por ejemplo '[1,3]'.
CREATE OR ALTER PROCEDURE sp_crear_usuario
    @username VARCHAR(100),
    @password_hash VARCHAR(255),
    @email VARCHAR(150),
    @nombre_completo VARCHAR(200) = NULL,
    @roles_json NVARCHAR(MAX)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    DECLARE @msg NVARCHAR(400);

    IF EXISTS (SELECT 1 FROM usuario WHERE email = @email)
    BEGIN
        SET @msg = CONCAT(N'Ya existe un usuario con el correo ', @email, N'.');
        THROW 50002, @msg, 1;
    END;
    IF EXISTS (SELECT 1 FROM usuario WHERE username = @username)
    BEGIN
        SET @msg = CONCAT(N'Ya existe un usuario con el nombre de usuario ', @username, N'.');
        THROW 50002, @msg, 1;
    END;

    DECLARE @roles TABLE (rol_id INT PRIMARY KEY);
    INSERT INTO @roles SELECT DISTINCT CAST([value] AS INT) FROM OPENJSON(@roles_json);
    IF NOT EXISTS (SELECT 1 FROM @roles)
        THROW 50003, N'El usuario debe tener al menos un rol.', 1;
    IF EXISTS (SELECT 1 FROM @roles x WHERE NOT EXISTS (SELECT 1 FROM rol r WHERE r.id = x.rol_id AND r.activo = 1))
        THROW 50001, N'Uno de los roles elegidos no existe o está retirado.', 1;

    DECLARE @id INT;
    BEGIN TRANSACTION;
        INSERT INTO usuario (username, password, email, nombre_completo, activo, fecha_creacion, fecha_actualizacion)
        VALUES (@username, @password_hash, @email, @nombre_completo, 1, GETDATE(), GETDATE());
        SET @id = SCOPE_IDENTITY();
        INSERT INTO rol_usuario (usuario_id, rol_id) SELECT @id, rol_id FROM @roles;
    COMMIT TRANSACTION;

    EXEC sp_consultar_usuario @id;
END;
GO

-- Actualizar. @password_hash NULL = "déjela como está" (el campo llegó vacío).
-- @roles_json reemplaza el conjunto de roles del usuario.
CREATE OR ALTER PROCEDURE sp_actualizar_usuario
    @id INT,
    @username VARCHAR(100),
    @password_hash VARCHAR(255) = NULL,
    @email VARCHAR(150),
    @nombre_completo VARCHAR(200) = NULL,
    @roles_json NVARCHAR(MAX)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    DECLARE @msg NVARCHAR(400);

    IF NOT EXISTS (SELECT 1 FROM usuario WHERE id = @id AND activo = 1)
        THROW 50004, N'No se encontró el usuario.', 1;
    IF EXISTS (SELECT 1 FROM usuario WHERE email = @email AND id <> @id)
    BEGIN
        SET @msg = CONCAT(N'Ya existe otro usuario con el correo ', @email, N'.');
        THROW 50002, @msg, 1;
    END;
    IF EXISTS (SELECT 1 FROM usuario WHERE username = @username AND id <> @id)
    BEGIN
        SET @msg = CONCAT(N'Ya existe otro usuario con el nombre de usuario ', @username, N'.');
        THROW 50002, @msg, 1;
    END;

    DECLARE @roles TABLE (rol_id INT PRIMARY KEY);
    INSERT INTO @roles SELECT DISTINCT CAST([value] AS INT) FROM OPENJSON(@roles_json);
    IF NOT EXISTS (SELECT 1 FROM @roles)
        THROW 50003, N'El usuario debe tener al menos un rol.', 1;
    IF EXISTS (SELECT 1 FROM @roles x WHERE NOT EXISTS (SELECT 1 FROM rol r WHERE r.id = x.rol_id AND r.activo = 1))
        THROW 50001, N'Uno de los roles elegidos no existe o está retirado.', 1;

    -- Si le quitan el rol Administrador, que no sea el último.
    IF NOT EXISTS (SELECT 1 FROM @roles x JOIN rol r ON r.id = x.rol_id WHERE r.nombre = 'Administrador')
        EXEC sp_revisar_ultimo_administrador @id;

    BEGIN TRANSACTION;
        UPDATE usuario
        SET username = @username,
            password = ISNULL(@password_hash, password),
            email = @email,
            nombre_completo = @nombre_completo,
            fecha_actualizacion = GETDATE()
        WHERE id = @id;

        DELETE FROM rol_usuario WHERE usuario_id = @id AND rol_id NOT IN (SELECT rol_id FROM @roles);
        INSERT INTO rol_usuario (usuario_id, rol_id)
        SELECT @id, x.rol_id FROM @roles x
        WHERE NOT EXISTS (SELECT 1 FROM rol_usuario ru WHERE ru.usuario_id = @id AND ru.rol_id = x.rol_id);
    COMMIT TRANSACTION;

    EXEC sp_consultar_usuario @id;
END;
GO

-- Retirar (borrado lógico). Un usuario retirado no puede iniciar sesión, y
-- su token deja de servir en la siguiente petición: sp_verificar_acceso_ruta
-- exige u.activo = 1.
CREATE OR ALTER PROCEDURE sp_eliminar_usuario
    @id INT
AS
BEGIN
    SET NOCOUNT ON;
    IF NOT EXISTS (SELECT 1 FROM usuario WHERE id = @id AND activo = 1)
        THROW 50004, N'No se encontró el usuario.', 1;
    EXEC sp_revisar_ultimo_administrador @id;
    UPDATE usuario SET activo = 0, fecha_actualizacion = GETDATE() WHERE id = @id;
END;
GO

-- ------------------------------------------------------------
-- rol_usuario (puente usuario <-> rol). No tiene columna activo en el
-- esquema: quitar un rol BORRA la fila (y surte efecto de inmediato).
-- ------------------------------------------------------------
CREATE OR ALTER PROCEDURE sp_listar_rol_usuario
    @usuario_id INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT ru.usuario_id, ru.rol_id, r.nombre AS rol_nombre
    FROM rol_usuario ru JOIN rol r ON r.id = ru.rol_id AND r.activo = 1
    WHERE ru.usuario_id = @usuario_id
    ORDER BY r.nombre;
END;
GO

CREATE OR ALTER PROCEDURE sp_crear_rol_usuario
    @usuario_id INT,
    @rol_id INT
AS
BEGIN
    SET NOCOUNT ON;
    IF NOT EXISTS (SELECT 1 FROM usuario WHERE id = @usuario_id AND activo = 1)
        THROW 50001, N'No se puede asignar: el usuario no existe o está retirado.', 1;
    IF NOT EXISTS (SELECT 1 FROM rol WHERE id = @rol_id AND activo = 1)
        THROW 50001, N'No se puede asignar: el rol no existe o está retirado.', 1;
    IF EXISTS (SELECT 1 FROM rol_usuario WHERE usuario_id = @usuario_id AND rol_id = @rol_id)
        THROW 50002, N'El usuario ya tiene ese rol.', 1;

    INSERT INTO rol_usuario (usuario_id, rol_id) VALUES (@usuario_id, @rol_id);
    SELECT ru.usuario_id, ru.rol_id, r.nombre AS rol_nombre
    FROM rol_usuario ru JOIN rol r ON r.id = ru.rol_id
    WHERE ru.usuario_id = @usuario_id AND ru.rol_id = @rol_id;
END;
GO

CREATE OR ALTER PROCEDURE sp_eliminar_rol_usuario
    @usuario_id INT,
    @rol_id INT
AS
BEGIN
    SET NOCOUNT ON;
    IF NOT EXISTS (SELECT 1 FROM rol_usuario WHERE usuario_id = @usuario_id AND rol_id = @rol_id)
        THROW 50004, N'El usuario no tiene ese rol.', 1;
    IF EXISTS (SELECT 1 FROM rol WHERE id = @rol_id AND nombre = 'Administrador')
        EXEC sp_revisar_ultimo_administrador @usuario_id;
    IF (SELECT COUNT(*) FROM rol_usuario WHERE usuario_id = @usuario_id) = 1
        THROW 50002, N'No se puede: es el único rol del usuario. Asígnele otro primero.', 1;
    DELETE FROM rol_usuario WHERE usuario_id = @usuario_id AND rol_id = @rol_id;
END;
GO

-- ------------------------------------------------------------
-- rol
-- ------------------------------------------------------------
CREATE OR ALTER PROCEDURE sp_listar_rol
AS
BEGIN
    SET NOCOUNT ON;
    SELECT r.id, r.nombre, r.descripcion,
           (SELECT COUNT(*) FROM rol_usuario ru JOIN usuario u ON u.id = ru.usuario_id AND u.activo = 1
            WHERE ru.rol_id = r.id) AS total_usuarios
    FROM rol r WHERE r.activo = 1 ORDER BY r.nombre;
END;
GO

CREATE OR ALTER PROCEDURE sp_consultar_rol
    @id INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT r.id, r.nombre, r.descripcion,
           (SELECT COUNT(*) FROM rol_usuario ru JOIN usuario u ON u.id = ru.usuario_id AND u.activo = 1
            WHERE ru.rol_id = r.id) AS total_usuarios
    FROM rol r WHERE r.id = @id AND r.activo = 1;
END;
GO

CREATE OR ALTER PROCEDURE sp_crear_rol
    @nombre VARCHAR(100),
    @descripcion VARCHAR(MAX) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @msg NVARCHAR(400);
    IF EXISTS (SELECT 1 FROM rol WHERE nombre = @nombre)
    BEGIN
        SET @msg = CONCAT(N'Ya existe un rol llamado ', @nombre, N'.');
        THROW 50002, @msg, 1;
    END;
    INSERT INTO rol (nombre, descripcion, activo, fecha_creacion) VALUES (@nombre, @descripcion, 1, GETDATE());
    DECLARE @id INT = SCOPE_IDENTITY();
    EXEC sp_consultar_rol @id;
END;
GO

CREATE OR ALTER PROCEDURE sp_actualizar_rol
    @id INT,
    @nombre VARCHAR(100),
    @descripcion VARCHAR(MAX) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @msg NVARCHAR(400);
    IF NOT EXISTS (SELECT 1 FROM rol WHERE id = @id AND activo = 1)
        THROW 50004, N'No se encontró el rol.', 1;
    IF EXISTS (SELECT 1 FROM rol WHERE nombre = @nombre AND id <> @id)
    BEGIN
        SET @msg = CONCAT(N'Ya existe otro rol llamado ', @nombre, N'.');
        THROW 50002, @msg, 1;
    END;
    -- El rol Administrador no se renombra: los procedimientos lo buscan por su nombre.
    IF EXISTS (SELECT 1 FROM rol WHERE id = @id AND nombre = 'Administrador') AND @nombre <> 'Administrador'
        THROW 50002, N'El rol Administrador no se puede renombrar.', 1;
    UPDATE rol SET nombre = @nombre, descripcion = @descripcion WHERE id = @id;
    EXEC sp_consultar_rol @id;
END;
GO

-- Retirar un rol (borrado lógico). Se bloquea si es Administrador o si
-- todavía hay usuarios activos con él (igual que la regla R2 de la v2).
CREATE OR ALTER PROCEDURE sp_eliminar_rol
    @id INT
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @msg NVARCHAR(400);
    IF NOT EXISTS (SELECT 1 FROM rol WHERE id = @id AND activo = 1)
        THROW 50004, N'No se encontró el rol.', 1;
    IF EXISTS (SELECT 1 FROM rol WHERE id = @id AND nombre = 'Administrador')
        THROW 50002, N'El rol Administrador no se puede retirar.', 1;
    DECLARE @usuarios INT = (SELECT COUNT(*) FROM rol_usuario ru JOIN usuario u ON u.id = ru.usuario_id AND u.activo = 1
                             WHERE ru.rol_id = @id);
    IF @usuarios > 0
    BEGIN
        SET @msg = CONCAT(N'No se puede retirar: ', @usuarios, N' usuario(s) activos tienen este rol. Quíteselo primero.');
        THROW 50002, @msg, 1;
    END;
    UPDATE rol SET activo = 0 WHERE id = @id;
END;
GO

-- ------------------------------------------------------------
-- ruta (catálogo de solo lectura: lo siembra este script)
-- ------------------------------------------------------------
CREATE OR ALTER PROCEDURE sp_listar_ruta
AS
BEGIN
    SET NOCOUNT ON;
    SELECT id, nombre, descripcion FROM ruta ORDER BY nombre;
END;
GO

-- ------------------------------------------------------------
-- ruta_rol (los permisos de cada rol)
-- ------------------------------------------------------------

-- Todas las rutas, marcando cuáles tiene el rol: con esto se arma la
-- pantalla de permisos (una casilla por ruta).
CREATE OR ALTER PROCEDURE sp_listar_ruta_rol
    @rol_id INT
AS
BEGIN
    SET NOCOUNT ON;
    IF NOT EXISTS (SELECT 1 FROM rol WHERE id = @rol_id AND activo = 1)
        THROW 50004, N'No se encontró el rol.', 1;
    SELECT t.id AS ruta_id, t.nombre AS ruta_nombre, t.descripcion,
           @rol_id AS rol_id,
           CAST(CASE WHEN rr.ruta_id IS NULL THEN 0 ELSE 1 END AS BIT) AS asignada
    FROM ruta t
    LEFT JOIN ruta_rol rr ON rr.ruta_id = t.id AND rr.rol_id = @rol_id
    ORDER BY t.nombre;
END;
GO

CREATE OR ALTER PROCEDURE sp_crear_ruta_rol
    @ruta_id INT,
    @rol_id INT
AS
BEGIN
    SET NOCOUNT ON;
    IF NOT EXISTS (SELECT 1 FROM ruta WHERE id = @ruta_id)
        THROW 50001, N'No se puede dar el permiso: la ruta no existe.', 1;
    IF NOT EXISTS (SELECT 1 FROM rol WHERE id = @rol_id AND activo = 1)
        THROW 50001, N'No se puede dar el permiso: el rol no existe o está retirado.', 1;
    IF EXISTS (SELECT 1 FROM ruta_rol WHERE ruta_id = @ruta_id AND rol_id = @rol_id)
        THROW 50002, N'El rol ya tiene ese permiso.', 1;
    INSERT INTO ruta_rol (ruta_id, rol_id) VALUES (@ruta_id, @rol_id);
END;
GO

-- Quitar un permiso. Surte efecto en la SIGUIENTE petición de cualquier
-- usuario con ese rol, sin que vuelva a iniciar sesión (criterio 7).
-- Al Administrador no se le quitan las rutas de administración: si no,
-- nadie podría devolver el permiso.
CREATE OR ALTER PROCEDURE sp_eliminar_ruta_rol
    @ruta_id INT,
    @rol_id INT
AS
BEGIN
    SET NOCOUNT ON;
    IF NOT EXISTS (SELECT 1 FROM ruta_rol WHERE ruta_id = @ruta_id AND rol_id = @rol_id)
        THROW 50004, N'El rol no tiene ese permiso.', 1;
    IF EXISTS (SELECT 1 FROM rol r, ruta t
               WHERE r.id = @rol_id AND r.nombre = 'Administrador'
                 AND t.id = @ruta_id AND t.nombre IN ('interfaz.usuarios', 'interfaz.roles', 'interfaz.permisos'))
        THROW 50002, N'Al Administrador no se le quitan los permisos de administración: nadie podría devolverlos.', 1;
    DELETE FROM ruta_rol WHERE ruta_id = @ruta_id AND rol_id = @rol_id;
END;
GO

PRINT N'v3_control_acceso.sql aplicado: rutas, permisos, usuarios de prueba y procedimientos del acceso listos.';
GO
