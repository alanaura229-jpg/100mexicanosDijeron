// ============================================================
// ARCHIVO: Program.cs  (Proyecto nuevo: TriviaAPI)
// TIPO: Aplicación consola ASP.NET (.NET 6 o superior)
// CÓMO CREAR:
//   1. En Visual Studio: Archivo → Nuevo Proyecto →
//      "Aplicación web de ASP.NET Core (vacía)"
//   2. Nómbralo "TriviaAPI"
//   3. Reemplaza el contenido de Program.cs con este código
//   4. Instala el paquete NuGet: MySql.Data
// ============================================================

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;
using MySql.Data.MySqlClient;
using System.Text.Json;
using System.Collections.Generic;
using System.IO;
using System;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

// ─────────────────────────────────────────
// CADENA DE CONEXIÓN A TU BASE DE DATOS
// ─────────────────────────────────────────
string cadenaConexion = "Server=127.0.0.1;Database=juego_trivia;Uid=root;Pwd=orion6363Vv!;";

// ─────────────────────────────────────────
// CARPETA DONDE ESTÁN LAS IMÁGENES
// Ajusta esta ruta a donde estén tus imágenes en el servidor
// ─────────────────────────────────────────
string carpetaImagenes = Path.Combine(AppContext.BaseDirectory, "Resources", "Imag");

// ============================================================
// ENDPOINT 1: GET /preguntas/{categoria}
// Devuelve lista de preguntas de una categoría
// Ejemplo: http://192.168.1.5:5000/preguntas/Música
// ============================================================
app.MapGet("/preguntas/{categoria}", (string categoria) =>
{
    var preguntas = new List<object>();

    string query;
    if (categoria == "Aleatorio")
    {
        query = "SELECT id, tipo, pregunta, opcion_a, opcion_b, opcion_c, opcion_d, respuesta_correcta " +
                "FROM preguntas ORDER BY RAND() LIMIT 10";
    }
    else
    {
        int idCategoria = categoria switch
        {
            "Música" => 1,
            "Deportes" => 2,
            "Geografía" => 3,
            "Cine" => 4,
            "Tecnología y video juegos" => 5,
            _ => 1
        };
        query = $"SELECT id, tipo, pregunta, opcion_a, opcion_b, opcion_c, opcion_d, respuesta_correcta " +
                $"FROM preguntas WHERE categoria_id = {idCategoria} ORDER BY RAND() LIMIT 10";
    }

    using var con = new MySqlConnection(cadenaConexion);
    try
    {
        con.Open();
        using var cmd = new MySqlCommand(query, con);
        using var reader = cmd.ExecuteReader();

        while (reader.Read())
        {
            string tipo = reader["tipo"].ToString();

            // Si es pregunta de IMAGEN, convertimos cada opción a Base64
            string opA, opB, opC, opD;
            if (tipo == "imagen")
            {
                opA = ConvertirImagenABase64(reader["opcion_a"].ToString(), carpetaImagenes);
                opB = ConvertirImagenABase64(reader["opcion_b"].ToString(), carpetaImagenes);
                opC = ConvertirImagenABase64(reader["opcion_c"].ToString(), carpetaImagenes);
                opD = ConvertirImagenABase64(reader["opcion_d"].ToString(), carpetaImagenes);
            }
            else
            {
                // Si es texto, simplemente devolvemos el texto
                opA = reader["opcion_a"].ToString();
                opB = reader["opcion_b"].ToString();
                opC = reader["opcion_c"].ToString();
                opD = reader["opcion_d"].ToString();
            }

            preguntas.Add(new
            {
                id = reader["id"].ToString(),
                tipo = tipo,
                pregunta = reader["pregunta"].ToString(),
                opcion_a = opA,
                opcion_b = opB,
                opcion_c = opC,
                opcion_d = opD,
                correcta = reader["respuesta_correcta"].ToString()
            });
        }
    }
    catch (Exception ex)
    {
        return Results.Problem("Error de base de datos: " + ex.Message);
    }

    return Results.Ok(preguntas);
});

// ============================================================
// ENDPOINT 2: GET /preguntas/id/{id}
// Devuelve UNA pregunta por su ID (con imagen en Base64 si aplica)
// Ejemplo: http://192.168.1.5:5000/preguntas/id/5
// ============================================================
app.MapGet("/preguntas/id/{id}", (int id) =>
{
    using var con = new MySqlConnection(cadenaConexion);
    try
    {
        con.Open();
        string query = "SELECT id, tipo, pregunta, opcion_a, opcion_b, opcion_c, opcion_d, respuesta_correcta " +
                       "FROM preguntas WHERE id = @id";
        using var cmd = new MySqlCommand(query, con);
        cmd.Parameters.AddWithValue("@id", id);
        using var reader = cmd.ExecuteReader();

        if (reader.Read())
        {
            string tipo = reader["tipo"].ToString();
            string opA, opB, opC, opD;

            if (tipo == "imagen")
            {
                opA = ConvertirImagenABase64(reader["opcion_a"].ToString(), carpetaImagenes);
                opB = ConvertirImagenABase64(reader["opcion_b"].ToString(), carpetaImagenes);
                opC = ConvertirImagenABase64(reader["opcion_c"].ToString(), carpetaImagenes);
                opD = ConvertirImagenABase64(reader["opcion_d"].ToString(), carpetaImagenes);
            }
            else
            {
                opA = reader["opcion_a"].ToString();
                opB = reader["opcion_b"].ToString();
                opC = reader["opcion_c"].ToString();
                opD = reader["opcion_d"].ToString();
            }

            return Results.Ok(new
            {
                id = reader["id"].ToString(),
                tipo = tipo,
                pregunta = reader["pregunta"].ToString(),
                opcion_a = opA,
                opcion_b = opB,
                opcion_c = opC,
                opcion_d = opD,
                correcta = reader["respuesta_correcta"].ToString()
            });
        }
        return Results.NotFound("Pregunta no encontrada");
    }
    catch (Exception ex)
    {
        return Results.Problem("Error: " + ex.Message);
    }
});

// ============================================================
// ENDPOINT 3: GET /ping
// Para verificar que la API está corriendo
// Ejemplo: http://192.168.1.5:5000/ping → responde "pong"
// ============================================================
app.MapGet("/ping", () => "pong");

// Correr en todas las interfaces de red (para que otros PCs puedan conectarse)
app.Run("http://0.0.0.0:5000");

// ============================================================
// FUNCIÓN AUXILIAR: Convierte una imagen a texto Base64
// Base64 es una forma de convertir cualquier archivo a texto,
// así podemos mandarlo por la red sin archivos físicos.
// ============================================================
static string ConvertirImagenABase64(string rutaRelativa, string carpetaBase)
{
    try
    {
        // La ruta en la BD viene como "Imag/1/a.png" o similar
        // Extraemos solo la parte desde "Imag"
        int idx = rutaRelativa.IndexOf("Imag", StringComparison.OrdinalIgnoreCase);
        string rutaCorta = idx >= 0 ? rutaRelativa.Substring(idx) : rutaRelativa;

        // Construimos la ruta completa en el servidor
        string rutaFinal = Path.Combine(carpetaBase, "..", rutaCorta);
        rutaFinal = Path.GetFullPath(rutaFinal);

        if (!File.Exists(rutaFinal))
            return ""; // No se encontró la imagen

        // Leemos los bytes y los convertimos a texto Base64
        byte[] bytes = File.ReadAllBytes(rutaFinal);
        return Convert.ToBase64String(bytes);
    }
    catch
    {
        return ""; // Si falla, devolvemos vacío
    }
}