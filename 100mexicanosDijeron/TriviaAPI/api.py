import os
import mysql.connector
from flask import Flask, jsonify, send_from_directory, request

app = Flask(__name__)

CARPETA_IMAGENES = r"C:\Imag"

def conectar_db():
    return mysql.connector.connect(
        host="127.0.0.1",
        user="root",
        password="alex12wolf", # Cambia esto si tu contraseña es otra
        database="juego_trivia"
    )

# RUTA 1: PREGUNTAS (SOPORTA CATEGORÍAS)
@app.route('/api/preguntas', methods=['GET'])
def obtener_preguntas():
    try:
        categoria_id = request.args.get('categoria') # Busca si le piden una categoría específica
        conexion = conectar_db()
        cursor = conexion.cursor(dictionary=True)
        
        # Si mandaron categoría, filtramos. Si no, traemos al azar de todas.
        if categoria_id:
            consulta = """
                SELECT id, tipo, pregunta, opcion_a, opcion_b, opcion_c, opcion_d, respuesta_correcta 
                FROM preguntas 
                WHERE categoria_id = %s
                ORDER BY RAND() LIMIT 10
            """
            cursor.execute(consulta, (categoria_id,))
        else:
            consulta = """
                SELECT id, tipo, pregunta, opcion_a, opcion_b, opcion_c, opcion_d, respuesta_correcta 
                FROM preguntas 
                ORDER BY RAND() LIMIT 10
            """
            cursor.execute(consulta)
            
        preguntas = cursor.fetchall()
        cursor.close()
        conexion.close()
        return jsonify(preguntas)
    
    except Exception as e:
        return jsonify({"error": str(e)}), 500

# RUTA 2: IMÁGENES
@app.route('/imagenes/<path:ruta_archivo>', methods=['GET'])
def servir_imagen(ruta_archivo):
    try:
        return send_from_directory(CARPETA_IMAGENES, ruta_archivo)
    except Exception as e:
        return f"Error al cargar imagen: {str(e)}", 404

if __name__ == '__main__':
    print("API de Trivia iniciada en el puerto 8000...")
    app.run(host='0.0.0.0', port=8000)