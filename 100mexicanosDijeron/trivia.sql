-- 1. Limpieza y Creación de la Base de Datos
DROP DATABASE IF EXISTS juego_trivia;
CREATE DATABASE juego_trivia;
USE juego_trivia;

-- 2. Crear la tabla de categorías
CREATE TABLE categorias (
    id INT AUTO_INCREMENT PRIMARY KEY,
    nombre VARCHAR(100) NOT NULL
);

-- 3. Crear la tabla de preguntas
CREATE TABLE preguntas (
    id INT AUTO_INCREMENT PRIMARY KEY,
    categoria_id INT NOT NULL,
    tipo ENUM('opcion_multiple', 'imagen') NOT NULL,
    pregunta TEXT NOT NULL,
    url_pregunta_imagen VARCHAR(500) DEFAULT NULL,
    opcion_a VARCHAR(500) NOT NULL,
    opcion_b VARCHAR(500) NOT NULL,
    opcion_c VARCHAR(500) NOT NULL,
    opcion_d VARCHAR(500) NOT NULL,
    respuesta_correcta CHAR(1) NOT NULL,
    FOREIGN KEY (categoria_id) REFERENCES categorias(id) ON DELETE CASCADE
);

-- 4. Insertar Categorías
INSERT INTO categorias (id, nombre) VALUES 
(1, 'Música'),
(2, 'Deportes'),
(3, 'Geografía y Viajes'),
(4, 'Cine y Series'),
(5, 'Videojuegos y Tecnología');

-- 5. Insertar Preguntas de Opción Múltiple (Texto)
INSERT INTO preguntas (categoria_id, tipo, pregunta, opcion_a, opcion_b, opcion_c, opcion_d, respuesta_correcta) VALUES
(1, 'opcion_multiple', '¿Quién es conocido como el "Rey del Pop"?', 'Elvis Presley', 'Michael Jackson', 'Prince', 'Freddie Mercury', 'b'),
(1, 'opcion_multiple', '¿Cuál es el álbum más vendido de la historia?', 'Back in Black', 'Thriller', 'The Dark Side of the Moon', 'Greatest Hits (Eagles)', 'b'),
(1, 'opcion_multiple', '¿Quién canta "Bohemian Rhapsody"?', 'Led Zeppelin', 'Pink Floyd', 'Queen', 'The Who', 'c'),
(1, 'opcion_multiple', '¿Qué artista lanzó el disco "Midnights" en 2022?', 'Adele', 'Taylor Swift', 'Beyoncé', 'Billie Eilish', 'b'),
(1, 'opcion_multiple', '¿De dónde es la banda One Direction?', 'Inglaterra', 'Escocia', 'Irlanda', 'Gales', 'a'),

(2, 'opcion_multiple', '¿Qué país ha ganado más Copas del Mundo de la FIFA?', 'Alemania', 'Italia', 'Brasil', 'Argentina', 'c'),
(2, 'opcion_multiple', '¿En qué deporte se usa una piedra y una escoba?', 'Golf', 'Curling', 'Hockey', 'Lacrosse', 'b'),
(2, 'opcion_multiple', '¿Cuál es la distancia de un maratón?', '21 km', '42.195 km', '10 km', '50 km', 'b'),
(2, 'opcion_multiple', '¿A qué deporte pertenece el término "Home Run"?', 'Cricket', 'Béisbol', 'Tenis', 'Rugby', 'b'),
(2, 'opcion_multiple', '¿Qué equipo de la F1 es famoso por su color rojo?', 'Mercedes', 'Red Bull', 'Ferrari', 'McLaren', 'c'),

(3, 'opcion_multiple', '¿Qué ciudad es conocida como la "Gran Manzana"?', 'Los Ángeles', 'Chicago', 'Nueva York', 'Londres', 'c'),
(3, 'opcion_multiple', '¿Cuál es el monte más alto del planeta?', 'K2', 'Kilimanjaro', 'Everest', 'Mont Blanc', 'c'),
(3, 'opcion_multiple', '¿Cuál es la capital de Australia?', 'Sídney', 'Melbourne', 'Canberra', 'Perth', 'c'),
(3, 'opcion_multiple', '¿Cuál es la moneda oficial de Japón?', 'Won', 'Yuan', 'Yen', 'Ringgit', 'c'),
(3, 'opcion_multiple', '¿Cuál es el país más pequeño del mundo?', 'Mónaco', 'San Marino', 'Ciudad del Vaticano', 'Malta', 'c'),

(4, 'opcion_multiple', '¿Quién dirigió la película "Pulp Fiction"?', 'Steven Spielberg', 'Quentin Tarantino', 'Christopher Nolan', 'Martin Scorsese', 'b'),
(4, 'opcion_multiple', '¿Cuál es la película más taquillera de la historia?', 'Avengers: Endgame', 'Titanic', 'Avatar', 'Star Wars VII', 'c'),
(4, 'opcion_multiple', '¿Quién interpretó a Iron Man en el UCM?', 'Chris Evans', 'Robert Downey Jr.', 'Mark Ruffalo', 'Chris Hemsworth', 'b'),
(4, 'opcion_multiple', '¿Cuál es la primera película de Star Wars lanzada (1977)?', 'El Imperio Contraataca', 'Una Nueva Esperanza', 'La Amenaza Fantasma', 'El Retorno del Jedi', 'b'),
(4, 'opcion_multiple', '¿Cuál es el nombre del villano en "The Lion King"?', 'Mufasa', 'Scar', 'Jafar', 'Gastón', 'b'),

(5, 'opcion_multiple', '¿Quién es el creador de Facebook?', 'Steve Jobs', 'Mark Zuckerberg', 'Bill Gates', 'Jeff Bezos', 'b'),
(5, 'opcion_multiple', '¿Cuál es el videojuego más vendido de la historia?', 'Tetris', 'GTA V', 'Minecraft', 'Super Mario Bros', 'c'),
(5, 'opcion_multiple', '¿Qué significa la sigla "CPU"?', 'Control Process Unit', 'Central Processing Unit', 'Core Print Unit', 'Central Power Unit', 'b'),
(5, 'opcion_multiple', '¿Qué consola popularizó el control por movimiento en 2006?', 'PS3', 'Xbox 360', 'Nintendo Wii', 'GameCube', 'c'),
(5, 'opcion_multiple', '¿Cómo se llama el asistente virtual de Amazon?', 'Siri', 'Cortana', 'Alexa', 'Bixby', 'c');


-- ==========================================
-- AQUÍ EMPIEZAN LAS IMÁGENES CON RUTAS LIMPIAS
-- ==========================================

-- 1. MÚSICA (Categoría 1)
INSERT INTO preguntas (categoria_id, tipo, pregunta, opcion_a, opcion_b, opcion_c, opcion_d, respuesta_correcta) VALUES
(1, 'imagen', '¿Cuál es el logotipo de la banda The Rolling Stones?', '1/a.png', '1/b.jpg', '1/c.jpg', '1/d.jpg', 'a'),
(1, 'imagen', 'Muestra la portada del álbum "Abbey Road" de los Beatles.', '2/a.jpg', '2/b.jpg', '2/c.jpg', '2/d.png', 'c'),
(1, 'imagen', 'Muestra una foto del vocalista de Queen', '3/a.jpg', '3/b.jpg', '3/c.jpg', '3/d.jpg', 'b'),
(1, 'imagen', '¿Qué objeto representa el logo de la aplicación Spotify?', '4/a.png', '4/b.png', '4/c.png', '4/d.png', 'd'),
(1, 'imagen', '¿Cuál es la máscara característica del DJ Marshmello?', '5/a.jpg', '5/b.jpg', '5/c.jpg', '5/d.pjp', 'a');

-- 2. DEPORTES (Categoría 2)
INSERT INTO preguntas (categoria_id, tipo, pregunta, opcion_a, opcion_b, opcion_c, opcion_d, respuesta_correcta) VALUES
(2, 'imagen', '¿Cuál es el símbolo oficial de los Juegos Olímpicos?', '6/a.jpg', '6/b.png', '6/c.png', '6/d.jpg', 'd'),
(2, 'imagen', '¿Cómo es un balón de fútbol americano?', '7/a.jpg', '7/b.png', '7/c.jpg', '7/d.jpg', 'c'),
(2, 'imagen', '¿Cómo es el trofeo de la Copa del Mundo de la FIFA?', '8/a.jpg', '8/b.jpg', '8/c.jpg', '8/d.jpg', 'a'),
(2, 'imagen', 'Muestra el logotipo del equipo de baloncesto Chicago Bulls.', '9/a.jpg', '9/b.png', '9/c.jpg', '9/d.jpg', 'c'),
(2, 'imagen', '¿Qué se lanza en el deporte del Curling?', '10/a.jpg', '10/b.png', '10/c.jpg', '10/d.jpg', 'b');

-- 3. GEOGRAFÍA Y VIAJES (Categoría 3)
INSERT INTO preguntas (categoria_id, tipo, pregunta, opcion_a, opcion_b, opcion_c, opcion_d, respuesta_correcta) VALUES
(3, 'imagen', '¿Cual es la bandera de Zimbabue?', '11/a.png', '11/b.jpg', '11/c.png', '11/d.png', 'c'),
(3, 'imagen', '¿Cuál es la estatua de la libertad?', '12/a.jpg', '12/b.jpeg', '12/c.jpg', '12/d.jpg', 'a'),
(3, 'imagen', '¿Cuál es el animal que aparece en el centro de la bandera de México?', '13/a.jpg', '13/b.jpg', '13/c.jpg', '13/d.jpg', 'c'),
(3, 'imagen', '¿Qué silueta geográfica represemta el continente africano?', '14/a.png', '14/b.png', '14/c.jpg', '14/d.jpg', 'd'),
(3, 'imagen', '¿Cuál es la bandera de la unión europea?', '15/a.jpg', '15/b.png', '15/c.png', '15/d.png', 'a');

-- 4. CINE Y SERIES (Categoría 4)
INSERT INTO preguntas (categoria_id, tipo, pregunta, opcion_a, opcion_b, opcion_c, opcion_d, respuesta_correcta) VALUES
(4, 'imagen', '¿Qué objeto usan los Men in Black para borrar la memoria?', '16/a.jpg', '16/b.jpg', '16/c.jpg', '16/d.jpg', 'c'),
(4, 'imagen', 'Muestra el logotipo de la película Ghostbusters', '17/a.jpg', '17/b.jpg', '17/c.jpg', '17/d.png', 'a'),
(4, 'imagen', 'Muestra la imagen del robot R2-D2.', '18/a.jpg', '18/b.jpg', '18/c.jpg', '18/d.png', 'a'),
(4, 'imagen', '¿Cómo es el logo de la productora Walt Disney Pictures?', '19/a.png', '19/b.png', '19/c.jpg', '19/d.jpg', 'd'),
(4, 'imagen', '¿Cómo es el coche DeLorean de "Volver al Futuro"?', '20/a.png', '20/b.jpg', '20/c.jpg', '20/d.jpg', 'a');

-- 5. VIDEOJUEGOS Y TECNOLOGÍA (Categoría 5)
INSERT INTO preguntas (categoria_id, tipo, pregunta, opcion_a, opcion_b, opcion_c, opcion_d, respuesta_correcta) VALUES
(5, 'imagen', 'Muestra el logotipo de la empresa Apple.', '21/a.png', '21/b.png', '21/c.png', '21/d.png', 'c'),
(5, 'imagen', '¿Cómo se ve un bloque de TNT en el juego Minecraft?', '22/a.png', '22/b.jpg', '22/c.jpg', '22/d.jpg', 'a'),
(5, 'imagen', 'Muestra una imagen del fantasma de Pac-Man.', '23/a.png', '23/b.jpg', '23/c.jpg', '23/d.png', 'd'),
(5, 'imagen', '¿Cómo es el símbolo de Bluetooth?', '24/a.jpg', '24/b.png', '24/c.jpg', '24/d.jpg', 'b'),
(5, 'imagen', 'Muestra la imagen de un USB tipo C.', '25/a.jpg', '25/b.jpg', '25/c.jpg', '25/d.jpg', 'a');

-- 6. Crear la tabla de resultados de usuarios
CREATE TABLE resultados_usuarios (
    id                  INT AUTO_INCREMENT PRIMARY KEY,
    nombre              VARCHAR(100)  NOT NULL,
    categoria_id        INT           DEFAULT NULL,
    preguntas_correctas INT           NOT NULL DEFAULT 0,
    preguntas_incorrectas INT         NOT NULL DEFAULT 0,
    puntuacion          DECIMAL(5,2)  NOT NULL DEFAULT 0.00,
    fecha_partida       DATETIME      NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (categoria_id) REFERENCES categorias(id) ON DELETE CASCADE
);
 
-- 7. Crear la tabla de detalle por pregunta respondida
CREATE TABLE detalle_respuestas (
    id                  INT AUTO_INCREMENT PRIMARY KEY,
    resultado_id        INT           NOT NULL,
    pregunta_id         INT           NOT NULL,
    categoria_id        INT           NOT NULL,
    respuesta_dada      CHAR(1)       NOT NULL,
    es_correcta         TINYINT(1)    NOT NULL DEFAULT 0,
    FOREIGN KEY (resultado_id)  REFERENCES resultados_usuarios(id) ON DELETE CASCADE,
    FOREIGN KEY (pregunta_id)   REFERENCES preguntas(id)           ON DELETE CASCADE,
    FOREIGN KEY (categoria_id)  REFERENCES categorias(id)          ON DELETE CASCADE
); 

-- 8. Crear tabla de estadísticas de preguntas
CREATE TABLE pregunta_estadisticas (
    id               INT AUTO_INCREMENT PRIMARY KEY,
    pregunta_id      INT NOT NULL,
    veces_correcta   INT NOT NULL DEFAULT 0,
    veces_incorrecta INT NOT NULL DEFAULT 0,
    fecha            DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (pregunta_id) REFERENCES preguntas(id) ON DELETE CASCADE
);
