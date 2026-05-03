# 100 Mexicanos Dijeron - Trivia

Juego de trivia multijugador para escritorio. Tiene un modo de un jugador y un modo en red donde varios jugadores se conectan a un servidor y responden las mismas preguntas al mismo tiempo.

## Configuración de la base de datos

1. Abre MySQL y ejecuta el archivo `trivia.sql` que está en la raíz del proyecto. Esto crea la base de datos `juego_trivia` con todas las tablas y preguntas incluidas.

2. El usuario y contraseña que usa el proyecto por defecto son:

   - Usuario: `root`
   - Contraseña

## Modo un jugador (local)

1. Abre el proyecto en Visual Studio y ejecuta el proyecto.
2. Desde el menú principal elige una categoría o pulsa "Aleatorio".
3. Responde 10 preguntas de opción múltiple. Al final ves tu puntuación.

## Modo multijugador en red

El modo red requiere levantar el servidor antes de que los clientes se conecten.

### 1. Levantar el servidor

Abre `ServidorTrivia` en Visual Studio y ejecútalo. La consola muestra las IPs disponibles de la máquina, por ejemplo:
El servidor espera que se conecte 1 jugador y luego inicia la partida automáticamente.

### 2. Conectar el cliente

En la aplicación, elige "Modo Red" desde el menú. Ingresa tu nombre y la IP del servidor. Al conectarte, el juego comienza.

### 3. API de imágenes (opcional)

Las preguntas tipo imagen necesitan que la API esté corriendo para cargar las imágenes desde el servidor. Si no la levantas, esas preguntas aparecerán sin imagen.
La API corre en el puerto 8000. Las imágenes deben estar en la carpeta `C:\Imag` de la máquina que corre el servidor, organizadas por número de pregunta (por ejemplo `C:\Imag\1\a.png`).

## Base de datos

Cinco tablas:

| Tabla | Descripción |
|---|---|
| `categorias` | Música, Deportes, Geografía y Viajes, Cine y Series, Videojuegos y Tecnología |
| `preguntas` | 50 preguntas de texto + 25 de imagen |
| `resultados_usuarios` | Historial de partidas jugadas |
| `detalle_respuestas` | Registro de qué respondió cada jugador en cada pregunta |
| `pregunta_estadisticas` | Cuántas veces fue respondida correcta o incorrectamente cada pregunta |

## Equipo

| Integrante | GitHub |
|---|---|
| Jimena Anais Sánchez Espino | Anais / AnaisSanchez37 |
| Diego Emilio Ibarra Villela | Rendo / diegonavajas1-coder |
| Ana Laura Martínez Nava | alanaura229-jpg |


Interfaces Gráficas con Aplicaciones 
Facultad de Ingeniería, UASLP.
