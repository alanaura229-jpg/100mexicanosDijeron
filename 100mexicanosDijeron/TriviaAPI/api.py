import os
import mysql.connector
from flask import Flask, jsonify, send_from_directory, request

app = Flask(__name__)

CARPETA_IMAGENES = r"C:\Imag"

def conectar_db():
    return mysql.connector.connect(
        host="127.0.0.1",
        user="root",
        password="alex12wolf",
        database="juego_trivia"
    )

@app.route('/api/preguntas', methods=['GET'])
def obtener_preguntas():
    try:
        categoria_id = request.args.get('categoria')
        conexion = conectar_db()
        cursor = conexion.cursor(dictionary=True)
        
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

@app.route('/imagenes/<path:ruta_archivo>', methods=['GET'])
def servir_imagen(ruta_archivo):
    try:
        return send_from_directory(CARPETA_IMAGENES, ruta_archivo)
    except Exception as e:
        return f"Error al cargar imagen: {str(e)}", 404

@app.route('/api/estadisticas', methods=['POST'])
def guardar_estadisticas():
    datos = request.json
    try:
        conexion = conectar_db()
        cursor = conexion.cursor()
        
        sql = """INSERT INTO pregunta_estadisticas 
                 (pregunta_id, veces_correcta, veces_incorrecta, fecha) 
                 VALUES (%s, %s, %s, NOW())"""
                 
        cursor.execute(sql, (datos['pregunta_id'], datos['correctas'], datos['incorrectas']))
        conexion.commit()
        
        cursor.close()
        conexion.close()
        return jsonify({"status": "ok"})
    except Exception as e:
        return jsonify({"error": str(e)}), 500

@app.route('/api/resultados', methods=['POST'])
def guardar_resultados():
    estadisticas = request.json
    try:
        conexion = conectar_db()
        cursor = conexion.cursor()
        
        sql = """INSERT INTO resultados_usuarios 
                 (nombre, categoria_id, preguntas_correctas, preguntas_incorrectas, puntuacion, fecha_partida) 
                 VALUES (%s, %s, %s, %s, %s, NOW())"""
                 
        for st in estadisticas:
            puntos = st['aciertos'] * 100
            cat_id = st.get('categoria_id')
            if cat_id == 0: 
                cat_id = None
                
            cursor.execute(sql, (st['nombre'], cat_id, st['aciertos'], st['errores'], puntos))
            
        conexion.commit()
        cursor.close()
        conexion.close()
        return jsonify({"status": "ok"})
    except Exception as e:
        return jsonify({"error": str(e)}), 500

if __name__ == '__main__':
    app.run(host='0.0.0.0', port=8000)