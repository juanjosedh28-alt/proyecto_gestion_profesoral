-- ============================================================
-- datos_demo.sql — Datos de ejemplo para que el sistema no se vea vacío.
--
-- QUÉ HACE: llena las 15 tablas del módulo que no traían datos (el catálogo
--   de áreas de conocimiento ya trae 218 filas) con un grupo pequeño y
--   coherente: 8 docentes con sus estudios, evaluaciones, reconocimientos,
--   experiencias, programas, redes e intereses.
--
-- ES IDEMPOTENTE: cada fila entra solo si su clave todavía no existe. Se puede
--   correr en cada arranque (db/init.sh, paso 5) sin duplicar nada, y si
--   alguien retira un dato de ejemplo desde el front, no vuelve a aparecer
--   (la fila sigue existiendo, con activo = 0).
--
-- LAS CLAVES SE ESCOGIERON PARA NO CHOCAR CON LA COLECCIÓN DE POSTMAN:
--   Postman crea cédulas, programas y redes entre 100000 y 900000, estudios
--   por encima de 1000000, y prueba que NO existan el docente 1, el docente
--   99 y el programa 1. Aquí: cédulas de 10 dígitos, programas 101-105,
--   redes 11-15 y estudios 501-514.
--
-- Las personas son INVENTADAS; las instituciones y redes son reales.
-- Se escribe con INSERT directo porque es la base sembrándose a sí misma,
-- como el catálogo de áreas: la regla de «solo procedimientos» es para la API.
-- Los disparadores de la v2 siguen funcionando: trg_contar_estudios deja al
-- día docente.total_estudios al insertar los estudios.
-- ============================================================
USE gestion_local;
GO
SET NOCOUNT ON;
GO

-- 1. Términos clave -------------------------------------------------------
INSERT INTO termino_clave (termino, termino_ingles)
SELECT v.termino, v.ingles FROM (VALUES
    ('Inteligencia artificial', 'Artificial intelligence'),
    ('Aprendizaje automático', 'Machine learning'),
    ('Ciencia de datos', 'Data science'),
    ('Ciberseguridad', 'Cybersecurity'),
    ('Internet de las cosas', 'Internet of things'),
    ('Energías renovables', 'Renewable energy'),
    ('Educación virtual', 'E-learning'),
    ('Robótica', 'Robotics'),
    ('Economía circular', 'Circular economy'),
    ('Bioinformática', 'Bioinformatics')
) v (termino, ingles)
WHERE NOT EXISTS (SELECT 1 FROM termino_clave t WHERE t.termino = v.termino);
GO

-- 2. Líneas de investigación (el id lo pone la base: se reconocen por el nombre)
INSERT INTO linea_investigacion (nombre, descripcion)
SELECT v.nombre, v.descripcion FROM (VALUES
    ('Analítica de datos', 'Modelos estadísticos y de aprendizaje automático para la toma de decisiones en organizaciones.'),
    ('Sistemas inteligentes', 'Agentes, visión por computador y procesamiento de lenguaje natural.'),
    ('Seguridad informática', 'Protección de redes, criptografía aplicada y respuesta a incidentes.'),
    ('Energía y sostenibilidad', 'Generación con fuentes renovables y eficiencia energética en la industria.'),
    ('Pedagogía y TIC', 'Uso de tecnologías digitales en la enseñanza universitaria.'),
    ('Automatización industrial', 'Control de procesos, robótica y manufactura flexible.')
) v (nombre, descripcion)
WHERE NOT EXISTS (SELECT 1 FROM linea_investigacion l WHERE l.nombre = v.nombre);
GO

-- 3. Programas (también son los «departamentos» de docente_departamento)
INSERT INTO programa (id, nombre, tipo, nivel, fecha_creacion, fecha_cierre, numero_cohortes, cant_graduados, fecha_actualizacion, ciudad, facultad)
SELECT v.id, v.nombre, v.tipo, v.nivel, v.creacion, NULL, v.cohortes, v.graduados, CONVERT(VARCHAR(10), GETDATE(), 23), v.ciudad, v.facultad
FROM (VALUES
    (101, 'Ingeniería de Software', 'Pregrado', 'Profesional', '2008-02-15', '32', '1450', 'Medellín', 1),
    (102, 'Tecnología en Sistemas de Información', 'Pregrado', 'Tecnológico', '2001-08-01', '45', '2310', 'Medellín', 1),
    (103, 'Ingeniería Electromecánica', 'Pregrado', 'Profesional', '2010-02-01', '28', '980', 'Medellín', 2),
    (104, 'Maestría en Ciencias de la Computación', 'Posgrado', 'Maestría', '2016-08-01', '14', '120', 'Medellín', 1),
    (105, 'Especialización en Seguridad Informática', 'Posgrado', 'Especialización', '2019-02-01', '10', '85', 'Bogotá', 1)
) v (id, nombre, tipo, nivel, creacion, cohortes, graduados, ciudad, facultad)
WHERE NOT EXISTS (SELECT 1 FROM programa p WHERE p.id = v.id);
GO

-- 4. Redes académicas
INSERT INTO red (idr, nombre, url, pais)
SELECT v.idr, v.nombre, v.url, v.pais FROM (VALUES
    (11, 'RedCOLSI', 'https://www.redcolsi.org', 'Colombia'),
    (12, 'RENATA', 'https://www.renata.edu.co', 'Colombia'),
    (13, 'Red CLARA', 'https://www.redclara.net', 'Uruguay'),
    (14, 'ACOFI', 'https://www.acofi.edu.co', 'Colombia'),
    (15, 'IEEE Sección Colombia', 'https://ieee.org.co', 'Colombia')
) v (idr, nombre, url, pais)
WHERE NOT EXISTS (SELECT 1 FROM red r WHERE r.idr = v.idr);
GO

-- 5. Docentes (el maestro). La línea principal se busca por su nombre.
INSERT INTO docente (cedula, nombres, apellidos, genero, cargo, fecha_nacimiento, correo, telefono, url_cvlac,
                     fecha_actualizacion, escalafon, perfil, cat_minciencia, conv_minciencia, nacionalidaad, linea_investigacion_principal)
SELECT v.cedula, v.nombres, v.apellidos, v.genero, v.cargo, v.nacimiento, v.correo, v.telefono,
       CONCAT('https://scienti.minciencias.gov.co/cvlac/', v.cedula), CAST(GETDATE() AS DATE), v.escalafon, v.perfil,
       v.categoria, v.convocatoria, 'Colombiana', (SELECT id FROM linea_investigacion l WHERE l.nombre = v.linea)
FROM (VALUES
    (1017245001, 'Laura Marcela', 'Restrepo Gil', 'Femenino', 'Docente de planta', '1984-03-12', 'laura.restrepo@gestion.edu.co', '604 440 5101', 'Titular',
     'Ingeniera de sistemas y doctora en ciencias de la computación. Dirige el semillero de analítica de datos.', 'Senior', 'Convocatoria 894 de 2021', 'Analítica de datos'),
    (1036612002, 'Andrés Felipe', 'Muñoz Ospina', 'Masculino', 'Docente de planta', '1979-11-02', 'andres.munoz@gestion.edu.co', '604 440 5102', 'Asociado',
     'Magíster en automatización. Trabaja en control de procesos y robótica educativa.', 'Asociado', 'Convocatoria 894 de 2021', 'Automatización industrial'),
    (1128399003, 'Valentina', 'Cardona Ríos', 'Femenino', 'Docente ocasional', '1991-06-25', 'valentina.cardona@gestion.edu.co', '604 440 5103', 'Asistente',
     'Ingeniera de software. Investiga procesamiento de lenguaje natural en español.', 'Junior', 'Convocatoria 957 de 2024', 'Sistemas inteligentes'),
    (1040755004, 'Juan Camilo', 'Zapata Henao', 'Masculino', 'Docente de cátedra', '1988-01-17', 'juan.zapata@gestion.edu.co', '604 440 5104', 'Auxiliar',
     'Especialista en seguridad informática y consultor en respuesta a incidentes.', NULL, 'No aplica', 'Seguridad informática'),
    (1017890005, 'Natalia', 'Gómez Villa', 'Femenino', 'Docente de planta', '1982-09-08', 'natalia.gomez@gestion.edu.co', '604 440 5105', 'Asociado',
     'Doctora en educación. Diseña cursos virtuales y evalúa su impacto en el aprendizaje.', 'Asociado', 'Convocatoria 894 de 2021', 'Pedagogía y TIC'),
    (1152044006, 'Santiago', 'Arango Mejía', 'Masculino', 'Docente ocasional', '1993-04-30', 'santiago.arango@gestion.edu.co', '604 440 5106', 'Asistente',
     'Ingeniero electromecánico. Estudia microrredes solares para zonas rurales.', 'Junior', 'Convocatoria 957 de 2024', 'Energía y sostenibilidad'),
    (1020337007, 'Carolina', 'Ortiz Salazar', 'Femenino', 'Docente de planta', '1977-12-19', 'carolina.ortiz@gestion.edu.co', '604 440 5107', 'Titular',
     'Estadística y doctora en ingeniería. Coordina la maestría en ciencias de la computación.', 'Senior', 'Convocatoria 894 de 2021', 'Analítica de datos'),
    (1035880008, 'Mateo', 'Londoño Correa', 'Masculino', 'Docente de cátedra', '1990-07-14', 'mateo.londono@gestion.edu.co', '604 440 5108', 'Auxiliar',
     'Ingeniero de sistemas. Enseña programación web y desarrolla aplicaciones móviles.', NULL, 'No aplica', NULL)
) v (cedula, nombres, apellidos, genero, cargo, nacimiento, correo, telefono, escalafon, perfil, categoria, convocatoria, linea)
WHERE NOT EXISTS (SELECT 1 FROM docente d WHERE d.cedula = v.cedula);
GO

-- 6. Estudios realizados (cada uno de un docente)
INSERT INTO estudios_realizados (id, titulo, universidad, fecha, tipo, ciudad, docente, ins_acreditada, metodologia, perfil_egresado, pais)
SELECT v.id, v.titulo, v.universidad, v.fecha, v.tipo, v.ciudad, v.docente, v.acreditada, v.metodologia, v.perfil, v.pais
FROM (VALUES
    (501, 'Ingeniera de Sistemas', 'Universidad de Antioquia', '2006-12-15', 'Pregrado', 'Medellín', 1017245001, 1, 'Presencial', 'Diseña y construye sistemas de información.', 'Colombia'),
    (502, 'Doctora en Ciencias de la Computación', 'Universidad Nacional de Colombia', '2016-06-20', 'Doctorado', 'Medellín', 1017245001, 1, 'Presencial', 'Investiga en aprendizaje automático.', 'Colombia'),
    (503, 'Ingeniero Electrónico', 'Universidad Pontificia Bolivariana', '2002-07-10', 'Pregrado', 'Medellín', 1036612002, 1, 'Presencial', 'Diseña sistemas electrónicos y de control.', 'Colombia'),
    (504, 'Magíster en Automatización', 'Universidad EAFIT', '2011-12-05', 'Maestría', 'Medellín', 1036612002, 1, 'Presencial', 'Automatiza procesos industriales.', 'Colombia'),
    (505, 'Ingeniera de Software', 'Instituto Tecnológico Metropolitano', '2014-06-28', 'Pregrado', 'Medellín', 1128399003, 1, 'Presencial', 'Desarrolla software de calidad.', 'Colombia'),
    (506, 'Magíster en Inteligencia Artificial', 'Universidad Politécnica de Madrid', '2018-07-15', 'Maestría', 'Madrid', 1128399003, 1, 'Presencial', 'Aplica técnicas de inteligencia artificial.', 'España'),
    (507, 'Especialista en Seguridad Informática', 'Universidad de los Andes', '2015-11-30', 'Especialización', 'Bogotá', 1040755004, 1, 'Virtual', 'Protege la información de las organizaciones.', 'Colombia'),
    (508, 'Licenciada en Matemáticas', 'Universidad de Antioquia', '2004-06-18', 'Pregrado', 'Medellín', 1017890005, 1, 'Presencial', 'Enseña matemáticas en educación media y superior.', 'Colombia'),
    (509, 'Doctora en Educación', 'Universidad de Barcelona', '2015-03-22', 'Doctorado', 'Barcelona', 1017890005, 1, 'Presencial', 'Investiga en didáctica y tecnologías educativas.', 'España'),
    (510, 'Ingeniero Electromecánico', 'Instituto Tecnológico Metropolitano', '2016-12-10', 'Pregrado', 'Medellín', 1152044006, 1, 'Presencial', 'Integra sistemas eléctricos y mecánicos.', 'Colombia'),
    (511, 'Estadística', 'Universidad Nacional de Colombia', '1999-12-15', 'Pregrado', 'Medellín', 1020337007, 1, 'Presencial', 'Analiza datos con métodos estadísticos.', 'Colombia'),
    (512, 'Doctora en Ingeniería', 'Universidad Nacional de Colombia', '2010-09-30', 'Doctorado', 'Medellín', 1020337007, 1, 'Presencial', 'Investiga en modelos de optimización.', 'Colombia'),
    (513, 'Ingeniero de Sistemas', 'Universidad de Medellín', '2013-06-15', 'Pregrado', 'Medellín', 1035880008, 1, 'Presencial', 'Desarrolla aplicaciones web y móviles.', 'Colombia'),
    (514, 'Magíster en Ingeniería de Software', 'Universidad de Medellín', '2020-12-12', 'Maestría', 'Medellín', 1035880008, 0, 'Virtual', 'Dirige proyectos de software.', 'Colombia')
) v (id, titulo, universidad, fecha, tipo, ciudad, docente, acreditada, metodologia, perfil, pais)
WHERE NOT EXISTS (SELECT 1 FROM estudios_realizados e WHERE e.id = v.id);
GO

-- 7. Evaluaciones docente (id lo pone la base: se reconocen por docente y semestre)
INSERT INTO evaluacion_docente (calificacion, semestre, docente)
SELECT v.calificacion, v.semestre, v.docente FROM (VALUES
    (4.7, '2025-1', 1017245001), (4.8, '2025-2', 1017245001),
    (4.2, '2025-1', 1036612002), (4.4, '2025-2', 1036612002),
    (4.5, '2025-2', 1128399003), (3.9, '2025-1', 1040755004),
    (4.1, '2025-2', 1040755004), (4.6, '2025-1', 1017890005),
    (4.9, '2025-2', 1017890005), (4.0, '2025-2', 1152044006),
    (4.6, '2025-1', 1020337007), (4.7, '2025-2', 1020337007),
    (4.3, '2025-2', 1035880008)
) v (calificacion, semestre, docente)
WHERE NOT EXISTS (SELECT 1 FROM evaluacion_docente e WHERE e.docente = v.docente AND e.semestre = v.semestre);
GO

-- 8. Reconocimientos
INSERT INTO reconocimiento (tipo, fecha, institucion, nombre, ambito, docente)
SELECT v.tipo, v.fecha, v.institucion, v.nombre, v.ambito, v.docente FROM (VALUES
    ('Premio', '2023-11-20', 'Alcaldía de Medellín', 'Premio a la investigación aplicada', 'Local', 1017245001),
    ('Distinción', '2022-05-15', 'Universidad Nacional de Colombia', 'Tesis doctoral laureada', 'Nacional', 1017245001),
    ('Mención', '2024-10-03', 'ACOFI', 'Mejor ponencia en educación en ingeniería', 'Nacional', 1036612002),
    ('Beca', '2017-01-10', 'Fundación Carolina', 'Beca de maestría', 'Internacional', 1128399003),
    ('Premio', '2024-04-22', 'Ministerio de Educación Nacional', 'Docente innovador', 'Nacional', 1017890005),
    ('Distinción', '2019-09-12', 'Minciencias', 'Investigadora senior', 'Nacional', 1020337007),
    ('Mención', '2025-06-06', 'RedCOLSI', 'Semillero destacado', 'Regional', 1152044006)
) v (tipo, fecha, institucion, nombre, ambito, docente)
WHERE NOT EXISTS (SELECT 1 FROM reconocimiento r WHERE r.docente = v.docente AND r.nombre = v.nombre);
GO

-- 9. Experiencias
INSERT INTO experiecia (nombre_cargo, institucion, tipo, fecha_inicio, fecha_fin, docente)
SELECT v.cargo, v.institucion, v.tipo, v.inicio, v.fin, v.docente FROM (VALUES
    ('Analista de datos', 'Bancolombia', 'Profesional', '2007-02-01', '2010-12-31', 1017245001),
    ('Coordinadora de semillero', 'Instituto Tecnológico Metropolitano', 'Docencia', '2017-01-15', NULL, 1017245001),
    ('Ingeniero de control', 'Argos', 'Profesional', '2003-03-01', '2009-06-30', 1036612002),
    ('Desarrolladora de software', 'Globant', 'Profesional', '2014-08-01', '2016-12-31', 1128399003),
    ('Consultor de seguridad', 'Deloitte', 'Profesional', '2012-01-10', NULL, 1040755004),
    ('Docente de matemáticas', 'Colegio San José', 'Docencia', '2005-01-20', '2009-11-30', 1017890005),
    ('Practicante de energía', 'EPM', 'Práctica', '2016-01-15', '2016-07-15', 1152044006),
    ('Directora de maestría', 'Instituto Tecnológico Metropolitano', 'Administrativa', '2018-02-01', NULL, 1020337007),
    ('Desarrollador web', 'Rappi', 'Profesional', '2014-02-01', '2019-05-31', 1035880008)
) v (cargo, institucion, tipo, inicio, fin, docente)
WHERE NOT EXISTS (SELECT 1 FROM experiecia x WHERE x.docente = v.docente AND x.nombre_cargo = v.cargo);
GO

-- 10. Apoyo profesoral (uno por estudio)
INSERT INTO apoyo_profesoral (estudios, con_apoyo, institucion, tipo)
SELECT v.estudios, v.con_apoyo, v.institucion, v.tipo FROM (VALUES
    (502, 1, 'Instituto Tecnológico Metropolitano', 'Comisión de estudios'),
    (504, 1, 'Instituto Tecnológico Metropolitano', 'Descarga académica'),
    (509, 1, 'Instituto Tecnológico Metropolitano', 'Comisión de estudios'),
    (512, 0, 'Ninguna', 'Sin apoyo'),
    (514, 1, 'Universidad de Medellín', 'Descuento en matrícula')
) v (estudios, con_apoyo, institucion, tipo)
WHERE NOT EXISTS (SELECT 1 FROM apoyo_profesoral a WHERE a.estudios = v.estudios);
GO

-- 11. Becas (una por estudio)
INSERT INTO beca (estudios, tipo, institucion, fecha_inicio, fecha_fin)
SELECT v.estudios, v.tipo, v.institucion, v.inicio, v.fin FROM (VALUES
    (502, 'Doctorado nacional', 'Minciencias', '2012-02-01', '2016-06-20'),
    (506, 'Movilidad internacional', 'Fundación Carolina', '2017-09-01', '2018-07-15'),
    (509, 'Doctorado en el exterior', 'Colfuturo', '2011-09-01', '2015-03-22'),
    (512, 'Doctorado nacional', 'Minciencias', '2006-02-01', '2010-09-30')
) v (estudios, tipo, institucion, inicio, fin)
WHERE NOT EXISTS (SELECT 1 FROM beca b WHERE b.estudios = v.estudios);
GO

-- 12. Docente y programa (departamento)
INSERT INTO docente_departamento (docente, departamento, dedicacion, modalidad, fecha_ingreso, fecha_salida)
SELECT v.docente, v.programa, v.dedicacion, v.modalidad, v.ingreso, v.salida FROM (VALUES
    (1017245001, 101, 'Tiempo completo', 'Presencial', '2017-01-15', NULL),
    (1017245001, 104, 'Medio tiempo', 'Presencial', '2019-02-01', NULL),
    (1036612002, 103, 'Tiempo completo', 'Presencial', '2012-07-01', NULL),
    (1128399003, 101, 'Tiempo completo', 'Presencial', '2021-01-20', NULL),
    (1040755004, 105, 'Cátedra', 'Virtual', '2020-02-01', NULL),
    (1017890005, 102, 'Tiempo completo', 'Presencial', '2015-08-01', NULL),
    (1152044006, 103, 'Tiempo completo', 'Presencial', '2022-02-01', NULL),
    (1020337007, 104, 'Tiempo completo', 'Presencial', '2016-08-01', NULL),
    (1035880008, 102, 'Cátedra', 'Presencial', '2020-08-01', '2024-12-15')
) v (docente, programa, dedicacion, modalidad, ingreso, salida)
WHERE NOT EXISTS (SELECT 1 FROM docente_departamento d WHERE d.docente = v.docente AND d.departamento = v.programa);
GO

-- 13. Intereses futuros (docente y término clave)
INSERT INTO intereses_futuros (docente, termino_clave)
SELECT v.docente, v.termino FROM (VALUES
    (1017245001, 'Ciencia de datos'), (1017245001, 'Aprendizaje automático'),
    (1036612002, 'Robótica'), (1036612002, 'Internet de las cosas'),
    (1128399003, 'Inteligencia artificial'), (1040755004, 'Ciberseguridad'),
    (1017890005, 'Educación virtual'), (1152044006, 'Energías renovables'),
    (1152044006, 'Economía circular'), (1020337007, 'Bioinformática'),
    (1020337007, 'Ciencia de datos'), (1035880008, 'Inteligencia artificial')
) v (docente, termino)
WHERE NOT EXISTS (SELECT 1 FROM intereses_futuros i WHERE i.docente = v.docente AND i.termino_clave = v.termino);
GO

-- 14. Red y docente
INSERT INTO red_docente (red, docente, fecha_inicio, fecha_fin, act_destacadas)
SELECT v.red, v.docente, v.inicio, v.fin, v.actividades FROM (VALUES
    (11, 1017245001, '2018-03-01', NULL, 'Evaluadora de proyectos en el encuentro regional de semilleros.'),
    (14, 1036612002, '2015-05-10', NULL, 'Ponente en la reunión nacional de facultades de ingeniería.'),
    (15, 1036612002, '2012-01-01', '2020-12-31', 'Coordinó el capítulo de robótica y automatización.'),
    (13, 1128399003, '2022-06-01', NULL, 'Participa en el grupo de trabajo de inteligencia artificial.'),
    (12, 1040755004, '2019-02-15', NULL, 'Asesor del comité de seguridad de la red.'),
    (11, 1152044006, '2023-02-01', NULL, 'Tutor de un semillero de energías renovables.'),
    (12, 1020337007, '2017-08-01', NULL, 'Lidera proyectos de datos abiertos entre universidades.')
) v (red, docente, inicio, fin, actividades)
WHERE NOT EXISTS (SELECT 1 FROM red_docente r WHERE r.red = v.red AND r.docente = v.docente);
GO

-- 15. Estudio y área de conocimiento (códigos del catálogo de 218 áreas)
INSERT INTO estudio_ac (estudio, area_conocimiento)
SELECT v.estudio, v.area FROM (VALUES
    (501, '1B01'), (502, '1B01'), (502, '1A03'), (503, '2B01'),
    (504, '2B02'), (505, '1B01'), (506, '1B02'), (507, '1B02'),
    (508, '1A01'), (509, '5C01'), (510, '2A01'), (511, '1A03'),
    (512, '1A02'), (513, '1B01'), (514, '2B03')
) v (estudio, area)
WHERE NOT EXISTS (SELECT 1 FROM estudio_ac e WHERE e.estudio = v.estudio AND e.area_conocimiento = v.area)
  AND EXISTS (SELECT 1 FROM area_conocimiento a WHERE a.id = v.area);
GO

PRINT N'datos_demo.sql aplicado: datos de ejemplo listos.';
GO
