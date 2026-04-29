using System;
using System.Collections.Generic;
using System.Drawing;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace _100mexicanosDijeron
{
    public partial class FormJuegoRed : Form
    {
        NetworkStream stream;
        string miNombre;

        private static readonly HttpClient clientWeb = new HttpClient();
        enum Estado { EsperandoPregunta, MostrandoPregunta, MostrandoResultado, FinJuego }
        Estado estado = Estado.EsperandoPregunta;

        int numeroPregunta = 0, totalPreguntas = 0;
        string textoPregunta = "";
        string[] opciones = new string[4];

        string respuestaCorrecta = "";
        string miRespuesta = "";
        bool miEsCorrecta = false;
        List<(string nombre, string inciso, bool correcto)> respuestasJugadores = new List<(string, string, bool)>();

        List<(string nombre, int aciertos, int errores)> estadisticas = new List<(string, int, int)>();
        string ganador = "";

        Dictionary<Rectangle, string> areasOpciones = new Dictionary<Rectangle, string>();
        bool yaRespondio = false;

        System.Windows.Forms.Timer timerResultado = new System.Windows.Forms.Timer { Interval = 4000 };

        public FormJuegoRed(NetworkStream stream, string nombre)
        {
            InitializeComponent();
            this.stream = stream;
            this.miNombre = nombre;
            this.DoubleBuffered = true;
            this.WindowState = FormWindowState.Maximized;

            timerResultado.Tick += (s, e) =>
            {
                timerResultado.Stop();
                estado = Estado.EsperandoPregunta;
                Invalidate();
            };

            new Thread(EscucharServidor) { IsBackground = true }.Start();
        }

        async void EscucharServidor()
        {
            var sb = new StringBuilder();
            byte[] buffer = new byte[1];
            try
            {
                while (true)
                {
                    // 1. Seguimos leyendo del socket letra por letra hasta encontrar el final (\n)
                    int n = stream.Read(buffer, 0, 1);
                    if (n == 0) break;

                    char c = (char)buffer[0];
                    if (c != '\n')
                    {
                        sb.Append(c);
                        continue; // Sigue leyendo hasta completar la línea
                    }

                    // 2. Cuando llegamos aquí, ya tenemos un mensaje completo en sb
                    string mensajeRecibido = sb.ToString();
                    sb.Clear();

                    if (string.IsNullOrWhiteSpace(mensajeRecibido)) continue;

                    try
                    {
                        // 3. Analizamos el JSON que mandó el Servidor TCP
                        var doc = JsonDocument.Parse(mensajeRecibido);
                        if (doc.RootElement.TryGetProperty("tipo", out JsonElement tipoElement))
                        {
                            string tipo = tipoElement.GetString();

                            if (tipo == "nueva_pregunta")
                            {
                                // Sacamos el ID que nos mandó el servidor
                                int idPregunta = doc.RootElement.GetProperty("id").GetInt32();

                                // 4. ¡VAMOS A LA API POR LOS DATOS!
                                await CargarPreguntaDesdeAPI(idPregunta);
                            }
                            else
                            {
                                // Si es otro tipo de mensaje (resultado, fin, etc.)
                                // puedes llamar a tu antiguo ProcesarMensaje
                                ProcesarMensaje(mensajeRecibido);
                            }
                        }
                    }
                    catch (JsonException) { /* Mensaje mal formado, ignorar */ }
                }
            }
            catch { /* Desconexión */ }
        }

        private async Task CargarPreguntaDesdeAPI(int id)
        {
            try
            {
                // IP de la PC donde corre la API (la que te dio ipconfig)
                string url = $"http://127.0.0.1:5000/preguntas/id/{id}";

                string response = await clientWeb.GetStringAsync(url);
                var p = JsonSerializer.Deserialize<PreguntaDTO>(response);

                // Actualizamos la pantalla (Invoke es necesario porque estamos en otro hilo)
                this.Invoke((Action)(() => {
                    this.textoPregunta = p.pregunta;
                    this.opciones = new string[] { p.opcion_a, p.opcion_b, p.opcion_c, p.opcion_d };
                    this.respuestaCorrecta = p.correcta;

                    this.estado = Estado.MostrandoPregunta;
                    this.yaRespondio = false;
                    this.Invalidate(); // Esto activa el Paint para dibujar la pregunta
                }));
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al contactar la API: " + ex.Message);
            }
        }

        void ProcesarMensaje(string linea)
        {
            if (string.IsNullOrEmpty(linea)) return;
            var msg = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(linea);
            string tipo = msg["tipo"].GetString();

            switch (tipo)
            {
                case "pregunta": CargarPregunta(msg); break;
                case "resultado_pregunta": CargarResultado(msg); break;
                case "fin_juego": CargarFinJuego(msg); break;
            }
        }

        void CargarPregunta(Dictionary<string, JsonElement> msg)
        {
            numeroPregunta = msg["numero"].GetInt32();
            totalPreguntas = msg["total"].GetInt32();
            textoPregunta = msg["textoPregunta"].GetString();
            yaRespondio = false;

            int i = 0;
            foreach (var op in msg["opciones"].EnumerateArray())
                opciones[i++] = op.GetString();

            estado = Estado.MostrandoPregunta;
            this.Invoke((Action)Invalidate);
        }

        void CargarResultado(Dictionary<string, JsonElement> msg)
        {
            respuestaCorrecta = msg["correcta"].GetString();
            miRespuesta = msg["miRespuesta"].GetString();
            miEsCorrecta = msg["esCorrecta"].GetBoolean();

            respuestasJugadores.Clear();
            foreach (var r in msg["respuestasJugadores"].EnumerateArray())
                respuestasJugadores.Add((
                    r.GetProperty("nombre").GetString(),
                    r.GetProperty("inciso").GetString(),
                    r.GetProperty("esCorrecta").GetBoolean()
                ));

            estado = Estado.MostrandoResultado;
            timerResultado.Start();
            this.Invoke((Action)Invalidate);
        }

        void CargarFinJuego(Dictionary<string, JsonElement> msg)
        {
            ganador = msg["ganador"].GetString();
            estadisticas.Clear();
            foreach (var st in msg["estadisticas"].EnumerateArray())
                estadisticas.Add((
                    st.GetProperty("nombre").GetString(),
                    st.GetProperty("aciertos").GetInt32(),
                    st.GetProperty("errores").GetInt32()
                ));
            estado = Estado.FinJuego;
            this.Invoke((Action)Invalidate);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            g.DrawImage(Properties.Resources.Aleatorio, 0, 0, ClientSize.Width, ClientSize.Height);
            using (SolidBrush filtro = new SolidBrush(Color.FromArgb(130, 0, 0, 0)))
                g.FillRectangle(filtro, 0, 0, ClientSize.Width, ClientSize.Height);

            switch (estado)
            {
                case Estado.EsperandoPregunta: DibujarEspera(g); break;
                case Estado.MostrandoPregunta: DibujarPregunta(g); break;
                case Estado.MostrandoResultado: DibujarPregunta(g); DibujarResultado(g); break;
                case Estado.FinJuego: DibujarFinJuego(g); break;
            }
        }

        private string ObtenerNombreArchivo(string rutaCompleta)
        {
            if (string.IsNullOrEmpty(rutaCompleta)) return "";
            // Si la ruta trae barras \, nos quedamos con lo último (ej: a.jpg)
            string[] partes = rutaCompleta.Split('\\');
            return partes[partes.Length - 1];
        }

        void DibujarEspera(Graphics g)
        {
            Font f = new Font("Showcard Gothic", 30, FontStyle.Bold);
            string txt = "Esperando pregunta...";
            SizeF tam = g.MeasureString(txt, f);
            g.DrawString(txt, f, Brushes.White, ClientSize.Width / 2 - tam.Width / 2, ClientSize.Height / 2 - tam.Height / 2);
        }

        void DibujarPregunta(Graphics g)
        {
            int cx = ClientSize.Width / 2;
            Font fInfo = new Font("Arial", 18, FontStyle.Bold);
            Font fOpciones = new Font("Arial", 17, FontStyle.Bold);

            g.DrawString($"{miNombre}  |  Pregunta {numeroPregunta} de {totalPreguntas}", fInfo, Brushes.LightGray, 50, 30);
            if (yaRespondio)
                g.DrawString("✔ Esperando a los demás...", fInfo, Brushes.LimeGreen, ClientSize.Width - 380, 30);

            Font fP = new Font("Showcard Gothic", 24, FontStyle.Bold);
            SizeF tamP = g.MeasureString(textoPregunta, fP, ClientSize.Width - 100);
            RectangleF rectP = new RectangleF(50, 90, ClientSize.Width - 100, tamP.Height + 20);
            g.DrawString(textoPregunta, fP, Brushes.White, rectP);

            areasOpciones.Clear();
            string[] incisos = { "a", "b", "c", "d" };
            int anchoBtn = 560, altoBtn = 60, sep = 28;
            int xBtn = cx - anchoBtn / 2;
            int yBtn = (int)(rectP.Bottom + 40);

            for (int i = 0; i < 4; i++)
            {
                Rectangle rect = new Rectangle(xBtn, yBtn + i * (altoBtn + sep), anchoBtn, altoBtn);
                areasOpciones.Add(rect, incisos[i]);

                // Dibujar el botón
                g.FillRectangle(new SolidBrush(Color.FromArgb(180, 0, 0, 0)), rect.X + 5, rect.Y + 5, rect.Width, rect.Height);
                g.FillRectangle(new SolidBrush(yaRespondio ? Color.Gray : Color.MediumSlateBlue), rect);
                g.DrawRectangle(new Pen(Color.White, 2), rect);

                string contenido = opciones[i];

                // ¿Es una imagen? (revisamos si termina en .jpg o .png)
                if (contenido.ToLower().Contains(".jpg") || contenido.ToLower().Contains(".png"))
                {
                    string nombreReal = ObtenerNombreArchivoLimpio(contenido);
                    string rutaFinal = System.IO.Path.Combine(Application.StartupPath, "Resources", "Imag", nombreReal);

                    if (System.IO.File.Exists(rutaFinal))
                    {
                        using (Image img = Image.FromFile(rutaFinal))
                        {
                            // Dibujamos la imagen centrada en el botón
                            g.DrawImage(img, rect.X + (rect.Width / 2) - 30, rect.Y + 5, 60, rect.Height - 10);
                        }
                    }
                    else
                    {
                        // Si no encuentra el archivo, nos dirá dónde lo está buscando
                        g.DrawString("Error: No existe " + nombreReal, new Font("Arial", 8), Brushes.Red, rect.X + 5, rect.Y + 5);
                    }
                }
                else
                {
                    // Si es texto normal, lo dibuja como siempre
                    g.DrawString(contenido, fOpciones, Brushes.White, rect.X + 20, rect.Y + 15);
                }
            }
        }

        private string ObtenerNombreArchivoLimpio(string rutaBaseDatos)
        {
            if (string.IsNullOrEmpty(rutaBaseDatos)) return "";
            // Esto toma "C:\carrera\interfaces\Imag\1\a.jpg" y devuelve solo "a.jpg"
            return System.IO.Path.GetFileName(rutaBaseDatos);
        }

        void DibujarResultado(Graphics g)
        {
            int cx = ClientSize.Width / 2;
            int cy = ClientSize.Height / 2;

            using (SolidBrush capa = new SolidBrush(Color.FromArgb(190, 0, 0, 0)))
                g.FillRectangle(capa, 0, 0, ClientSize.Width, ClientSize.Height);

            string msg = miEsCorrecta ? "¡CORRECTO! ✔" : "¡INCORRECTO! ✘";
            Color color = miEsCorrecta ? Color.LimeGreen : Color.Red;
            Font fMsg = new Font("Showcard Gothic", 55, FontStyle.Bold);
            SizeF tamMsg = g.MeasureString(msg, fMsg);
            g.DrawString(msg, fMsg, new SolidBrush(color), cx - tamMsg.Width / 2, cy - 180);

            if (!miEsCorrecta)
            {
                Font fCorre = new Font("Arial", 20, FontStyle.Bold);
                string corrTxt = "La respuesta era: " + respuestaCorrecta.ToUpper();
                SizeF tamC = g.MeasureString(corrTxt, fCorre);
                g.DrawString(corrTxt, fCorre, Brushes.White, cx - tamC.Width / 2, cy - 100);
            }

            Font fJug = new Font("Arial", 17, FontStyle.Bold);
            Font fLabel = new Font("Arial", 14);
            g.DrawString("Respuestas:", fLabel, Brushes.LightGray, cx - 200, cy - 50);

            for (int i = 0; i < respuestasJugadores.Count; i++)
            {
                var (nomb, inciso, correcto) = respuestasJugadores[i];
                string emoji = correcto ? "✔" : "✘";
                Color c = correcto ? Color.LimeGreen : Color.OrangeRed;
                g.DrawString($"{emoji}  {nomb}: {inciso.ToUpper()}", fJug, new SolidBrush(c), cx - 200, cy - 10 + i * 40);
            }
        }

        void DibujarFinJuego(Graphics g)
        {
            int cx = ClientSize.Width / 2;
            int cy = ClientSize.Height / 2;

            using (SolidBrush capa = new SolidBrush(Color.FromArgb(220, 0, 0, 0)))
                g.FillRectangle(capa, 0, 0, ClientSize.Width, ClientSize.Height);

            Font fTitulo = new Font("Showcard Gothic", 45, FontStyle.Bold);
            string titulo = "¡FIN DEL JUEGO!";
            SizeF tamT = g.MeasureString(titulo, fTitulo);
            g.DrawString(titulo, fTitulo, Brushes.Gold, cx - tamT.Width / 2, cy - 250);

            Font fGanador = new Font("Showcard Gothic", 28, FontStyle.Bold);
            string txtG = $"GANADOR: {ganador}";
            SizeF tamG = g.MeasureString(txtG, fGanador);
            g.DrawString(txtG, fGanador, Brushes.LimeGreen, cx - tamG.Width / 2, cy - 165);

            Font fHeader = new Font("Arial", 16, FontStyle.Bold | FontStyle.Underline);
            Font fFila = new Font("Arial", 16);

            Rectangle panel = new Rectangle(cx - 300, cy - 100, 600, 40 + estadisticas.Count * 45 + 20);
            using (SolidBrush bg = new SolidBrush(Color.FromArgb(180, 20, 20, 60)))
                g.FillRectangle(bg, panel);
            g.DrawRectangle(new Pen(Color.MediumSlateBlue, 2), panel);

            g.DrawString("Jugador", fHeader, Brushes.White, panel.X + 20, panel.Y + 10);
            g.DrawString("Aciertos", fHeader, Brushes.White, panel.X + 240, panel.Y + 10);
            g.DrawString("Errores", fHeader, Brushes.White, panel.X + 380, panel.Y + 10);
            g.DrawString("Puntos", fHeader, Brushes.White, panel.X + 490, panel.Y + 10);

            for (int i = 0; i < estadisticas.Count; i++)
            {
                var (nomb, aciertos, errores) = estadisticas[i];
                int y = panel.Y + 45 + i * 45;
                Brush bNombre = nomb == ganador ? Brushes.Gold : (nomb == miNombre ? Brushes.LightSkyBlue : Brushes.White);
                g.DrawString(nomb, fFila, bNombre, panel.X + 20, y);
                g.DrawString(aciertos.ToString(), fFila, Brushes.LimeGreen, panel.X + 250, y);
                g.DrawString(errores.ToString(), fFila, Brushes.OrangeRed, panel.X + 390, y);
                g.DrawString((aciertos * 100).ToString(), fFila, Brushes.Yellow, panel.X + 500, y);
            }

            Font fSalir = new Font("Arial", 16, FontStyle.Italic);
            g.DrawString("Haz clic para salir", fSalir, Brushes.LightGray, cx - 100, panel.Bottom + 30);
        }

        protected override void OnMouseClick(MouseEventArgs e)
        {
            base.OnMouseClick(e);
            if (estado == Estado.FinJuego) { Application.Exit(); return; }
            if (estado != Estado.MostrandoPregunta || yaRespondio) return;

            foreach (var opcion in areasOpciones)
            {
                if (opcion.Key.Contains(e.Location))
                {
                    yaRespondio = true;
                    string msg = JsonSerializer.Serialize(new { tipo = "respuesta", inciso = opcion.Value }) + "\n";
                    byte[] datos = Encoding.UTF8.GetBytes(msg);
                    stream.Write(datos, 0, datos.Length);
                    Invalidate();
                    break;
                }
            }
        }

        protected override void OnResize(EventArgs e) { base.OnResize(e); Invalidate(); }
    }



}



public class PreguntaDTO
{
    public string id { get; set; }
    public string tipo { get; set; }
    public string pregunta { get; set; }
    public string opcion_a { get; set; }
    public string opcion_b { get; set; }
    public string opcion_c { get; set; }
    public string opcion_d { get; set; }
    public string correcta { get; set; }
}