using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using MySql.Data.MySqlClient;

namespace ServidorTrivia
{
    class Jugador
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string IP { get; set; }
        public TcpClient TcpCliente { get; set; }
        public NetworkStream Stream { get; set; }
        public int Aciertos { get; set; } = 0;
        public int Errores { get; set; } = 0;
        public string RespuestaActual { get; set; } = null;
    }

    class PreguntaDB
    {
        public int Id { get; set; }
        public string Tipo { get; set; }
        public string Texto { get; set; }
        public string[] Opciones { get; set; }
        public string Correcta { get; set; }
    }

    class Program
    {
        const int PUERTO = 54321;
        const int MAX_JUGADORES = 3;
        const string CADENA_DB = "Server=127.0.0.1;Database=juego_trivia;Uid=root;Pwd=orion6363Vv!;";

        static List<Jugador> jugadores = new List<Jugador>();
        static object lockJugadores = new object();
        static List<PreguntaDB> preguntas = new List<PreguntaDB>();
        static int indicePregunta = 0;
        static bool juegoIniciado = false;

        static void Main(string[] args)
        {
            Console.Title = "Servidor – 100 Mexicanos Dijeron";
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("╔══════════════════════════════════════╗");
            Console.WriteLine("║  SERVIDOR DE TRIVIA – 100 Mexicanos  ║");
            Console.WriteLine($"║  Puerto: {PUERTO,-27}║");
            Console.WriteLine($"║  Jugadores necesarios: {MAX_JUGADORES,-14}║");
            Console.WriteLine("╚══════════════════════════════════════╝");
            Console.ResetColor();

            MostrarIPsLocales();
            CargarPreguntas();

            TcpListener servidor = new TcpListener(IPAddress.Any, PUERTO);
            servidor.Start();
            Console.WriteLine($"\n[SERVIDOR] Escuchando en puerto {PUERTO}...\n");

            while (true)
            {
                TcpClient cliente = servidor.AcceptTcpClient();
                string ipCliente = ((IPEndPoint)cliente.Client.RemoteEndPoint).Address.ToString();

                lock (lockJugadores)
                {
                    if (juegoIniciado || jugadores.Count >= MAX_JUGADORES)
                    {
                        EnviarMensaje(cliente.GetStream(), new { tipo = "error", mensaje = "El juego ya comenzó o está lleno." });
                        cliente.Close();
                        Console.WriteLine($"[RECHAZADO] {ipCliente}");
                        continue;
                    }
                }

                Console.WriteLine($"[CONEXIÓN] {ipCliente}");
                Thread hilo = new Thread(() => ManejarCliente(cliente, ipCliente));
                hilo.IsBackground = true;
                hilo.Start();
            }
        }

        static void MostrarIPsLocales()
        {
            Console.WriteLine("\n[IPs del servidor] Comparte una de estas con los clientes:");
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
                if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                    Console.WriteLine($"  → {ip}:{PUERTO}");
            Console.WriteLine();
        }

        static void CargarPreguntas()
        {
            Console.Write("[DB] Cargando preguntas... ");
            try
            {
                using var con = new MySqlConnection(CADENA_DB);
                con.Open();
                string sql = "SELECT id, tipo, pregunta, opcion_a, opcion_b, opcion_c, opcion_d, respuesta_correcta FROM preguntas ORDER BY RAND() LIMIT 10";
                var cmd = new MySqlCommand(sql, con);
                var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    preguntas.Add(new PreguntaDB
                    {
                        Id = Convert.ToInt32(reader["id"]),
                        Tipo = reader["tipo"].ToString(),
                        Texto = reader["pregunta"].ToString(),
                        Correcta = reader["respuesta_correcta"].ToString(),
                        Opciones = new string[]
                        {
                            "A) " + reader["opcion_a"],
                            "B) " + reader["opcion_b"],
                            "C) " + reader["opcion_c"],
                            "D) " + reader["opcion_d"]
                        }
                    });
                }
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"OK ({preguntas.Count} preguntas)");
                Console.ResetColor();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"ERROR: {ex.Message}");
                Console.ResetColor();
            }
        }

        static void ManejarCliente(TcpClient tcpCliente, string ip)
        {
            NetworkStream stream = tcpCliente.GetStream();
            Jugador jugador = null;

            try
            {
                string lineaRaw = LeerLinea(stream);
                if (lineaRaw == null) return;

                var msg = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(lineaRaw);
                if (msg["tipo"].GetString() != "unirse") return;

                string nombre = msg["nombre"].GetString()?.Trim() ?? "Jugador";

                jugador = new Jugador { Nombre = nombre, IP = ip, TcpCliente = tcpCliente, Stream = stream };

                lock (lockJugadores)
                {
                    jugador.Id = jugadores.Count + 1;
                    jugadores.Add(jugador);

                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($"[LOBBY] {nombre} ({ip}) [{jugadores.Count}/{MAX_JUGADORES}]");
                    Console.ResetColor();

                    EnviarMensaje(stream, new
                    {
                        tipo = "bienvenida",
                        id = jugador.Id,
                        nombre = jugador.Nombre,
                        jugadores = ObtenerListaJugadores(),
                        esperando = MAX_JUGADORES - jugadores.Count
                    });

                    BroadcastExcepto(jugador, new
                    {
                        tipo = "jugador_unido",
                        jugadores = ObtenerListaJugadores(),
                        esperando = MAX_JUGADORES - jugadores.Count
                    });

                    if (jugadores.Count == MAX_JUGADORES)
                    {
                        juegoIniciado = true;
                        new Thread(EjecutarJuego) { IsBackground = true }.Start();
                    }
                }

                while (tcpCliente.Connected)
                {
                    string linea = LeerLinea(stream);
                    if (linea == null) break;

                    var msgResp = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(linea);
                    if (msgResp["tipo"].GetString() == "respuesta")
                    {
                        string inciso = msgResp["inciso"].GetString().ToLower();
                        lock (lockJugadores)
                        {
                            if (jugador.RespuestaActual == null)
                            {
                                jugador.RespuestaActual = inciso;
                                Console.WriteLine($"  [{jugador.Nombre}] → {inciso.ToUpper()}");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR {ip}] {ex.Message}");
            }
            finally
            {
                if (jugador != null)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"[OFF] {jugador.Nombre} ({ip})");
                    Console.ResetColor();
                }
                tcpCliente.Close();
            }
        }

        static void EjecutarJuego()
        {
            Thread.Sleep(500);
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("\n[JUEGO] Iniciando...\n");
            Console.ResetColor();

            Broadcast(new { tipo = "inicio_juego" });
            Thread.Sleep(2000);

            for (int i = 0; i < preguntas.Count; i++)
            {
                indicePregunta = i;
                var pregunta = preguntas[i];

                lock (lockJugadores)
                    foreach (var j in jugadores)
                        j.RespuestaActual = null;

                Console.WriteLine($"\n[Q{i + 1}] {pregunta.Texto}");

                Broadcast(new
                {
                    tipo = "pregunta",
                    numero = i + 1,
                    total = preguntas.Count,
                    textoPregunta = pregunta.Texto,
                    opciones = pregunta.Opciones,
                    tipoPregunta = pregunta.Tipo
                });

                // Esperar que todos respondan (máx 30 seg)
                int espera = 0;
                while (espera < 30000)
                {
                    Thread.Sleep(200);
                    espera += 200;
                    lock (lockJugadores)
                    {
                        bool todos = true;
                        foreach (var j in jugadores)
                            if (j.RespuestaActual == null) { todos = false; break; }
                        if (todos) break;
                    }
                }

                // Calcular resultados
                var respuestasJugadores = new List<object>();
                lock (lockJugadores)
                {
                    foreach (var j in jugadores)
                    {
                        string resp = j.RespuestaActual ?? "sin_respuesta";
                        bool ok = resp == pregunta.Correcta;
                        if (ok) j.Aciertos++; else j.Errores++;
                        respuestasJugadores.Add(new { nombre = j.Nombre, inciso = resp, esCorrecta = ok });
                    }
                }

                GuardarEstadisticaPregunta(pregunta.Id, respuestasJugadores);

                // Enviar resultado personalizado a cada jugador
                lock (lockJugadores)
                {
                    foreach (var j in jugadores)
                    {
                        string miResp = j.RespuestaActual ?? "sin_respuesta";
                        EnviarMensaje(j.Stream, new
                        {
                            tipo = "resultado_pregunta",
                            correcta = pregunta.Correcta,
                            miRespuesta = miResp,
                            esCorrecta = miResp == pregunta.Correcta,
                            respuestasJugadores
                        });
                    }
                }

                Console.WriteLine($"  Correcta: {pregunta.Correcta.ToUpper()}");
                Thread.Sleep(3000);
            }

            // Fin del juego
            var estadisticas = new List<object>();
            Jugador ganador = null;
            lock (lockJugadores)
            {
                int max = -1;
                foreach (var j in jugadores)
                {
                    estadisticas.Add(new { nombre = j.Nombre, aciertos = j.Aciertos, errores = j.Errores });
                    if (j.Aciertos > max) { max = j.Aciertos; ganador = j; }
                }
            }

            GuardarPartidaFinal(estadisticas, ganador?.Nombre);

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"\n[FIN] Ganador: {ganador?.Nombre ?? "Empate"}");
            Console.ResetColor();

            Broadcast(new { tipo = "fin_juego", estadisticas, ganador = ganador?.Nombre ?? "Empate" });
        }

        static void GuardarEstadisticaPregunta(int preguntaId, List<object> respuestas)
        {
            int correctas = 0, incorrectas = 0;
            foreach (dynamic r in respuestas)
            {
                if (r.esCorrecta) correctas++; else incorrectas++;
            }
            try
            {
                using var con = new MySqlConnection(CADENA_DB);
                con.Open();
                string sql = @"INSERT INTO pregunta_estadisticas 
                               (pregunta_id, veces_correcta, veces_incorrecta, fecha)
                               VALUES (@pid, @c, @i, NOW())";
                var cmd = new MySqlCommand(sql, con);
                cmd.Parameters.AddWithValue("@pid", preguntaId);
                cmd.Parameters.AddWithValue("@c", correctas);
                cmd.Parameters.AddWithValue("@i", incorrectas);
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex) { Console.WriteLine($"[BD] {ex.Message}"); }
        }

        static void GuardarPartidaFinal(List<object> estadisticas, string ganador)
        {
            try
            {
                using var con = new MySqlConnection(CADENA_DB);
                con.Open();
                foreach (dynamic st in estadisticas)
                {
                    string sql = @"INSERT INTO resultados_usuarios 
                                   (nombre, categoria_id, preguntas_correctas, preguntas_incorrectas, puntuacion, fecha_partida)
                                   VALUES (@nombre, NULL, @corr, @incorr, @pts, NOW())";
                    var cmd = new MySqlCommand(sql, con);
                    cmd.Parameters.AddWithValue("@nombre", st.nombre);
                    cmd.Parameters.AddWithValue("@corr", st.aciertos);
                    cmd.Parameters.AddWithValue("@incorr", st.errores);
                    cmd.Parameters.AddWithValue("@pts", st.aciertos * 100);
                    cmd.ExecuteNonQuery();
                }
                Console.WriteLine("[BD] Partida guardada.");
            }
            catch (Exception ex) { Console.WriteLine($"[BD] {ex.Message}"); }
        }

        static void Broadcast(object msg)
        {
            lock (lockJugadores)
                foreach (var j in jugadores)
                    EnviarMensaje(j.Stream, msg);
        }

        static void BroadcastExcepto(Jugador excluido, object msg)
        {
            lock (lockJugadores)
                foreach (var j in jugadores)
                    if (j.Id != excluido.Id)
                        EnviarMensaje(j.Stream, msg);
        }

        static void EnviarMensaje(NetworkStream stream, object obj)
        {
            try
            {
                string texto = JsonSerializer.Serialize(obj) + "\n";
                byte[] datos = Encoding.UTF8.GetBytes(texto);
                stream.Write(datos, 0, datos.Length);
            }
            catch { }
        }

        static string LeerLinea(NetworkStream stream)
        {
            var sb = new StringBuilder();
            byte[] buffer = new byte[1];
            try
            {
                while (true)
                {
                    int n = stream.Read(buffer, 0, 1);
                    if (n == 0) return null;
                    char c = (char)buffer[0];
                    if (c == '\n') return sb.ToString();
                    sb.Append(c);
                }
            }
            catch { return null; }
        }

        static List<object> ObtenerListaJugadores()
        {
            var lista = new List<object>();
            foreach (var j in jugadores)
                lista.Add(new { id = j.Id, nombre = j.Nombre, ip = j.IP });
            return lista;
        }
    }
}
