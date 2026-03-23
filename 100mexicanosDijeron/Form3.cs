using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace _100mexicanosDijeron
{
    public partial class FormJuego : Form
    {
        string cadenaConexion = "Server=127.0.0.1;Database=juego_trivia;Uid=root;Pwd=alex12wolf;";

        private string categoriaSeleccionada;
        private Image imagenFondo;

        private List<PreguntaTrivia> mazoPreguntas = new List<PreguntaTrivia>();
        private int indicePreguntaActual = 0;
        private int aciertos = 0;
        private int errores = 0;

        private PreguntaTrivia pActual;
        private Dictionary<Rectangle, string> areasOpciones = new Dictionary<Rectangle, string>();
        private Timer timerResultado = new Timer();
        private bool mostrandoResultado = false;
        private bool ultimaRespuestaCorrecta = false;
        private bool juegoTerminado = false;

        public FormJuego(String categoria)
        {
            InitializeComponent();
            this.categoriaSeleccionada = categoria;
            this.DoubleBuffered = true;
            this.WindowState = FormWindowState.Maximized;
            timerResultado.Interval = 2000;
            timerResultado.Tick += TimerResultado_Tick;

            switch (categoria)
            {
                case "Música": imagenFondo = Properties.Resources.musica; break;
                case "Deportes": imagenFondo = Properties.Resources.Deporte; break;
                case "Geografía": imagenFondo = Properties.Resources.Geografia; break;
                case "Cine": imagenFondo = Properties.Resources.Cine; break;
                case "Tecnología y video juegos": imagenFondo = Properties.Resources.VideoJuego; break;
                case "Aleatorio": imagenFondo = Properties.Resources.categoria; break;
                default: imagenFondo = Properties.Resources.categoria; break;
            }

            DescargarMazoDePreguntas();
        }

        private void DescargarMazoDePreguntas()
        {
            string query = "";
            if (categoriaSeleccionada == "Aleatorio")
            {
                query = "SELECT tipo, pregunta, opcion_a, opcion_b, opcion_c, opcion_d, respuesta_correcta FROM preguntas ORDER BY RAND() LIMIT 10";
            }
            else
            {
                int idCategoria = 1;
                if (categoriaSeleccionada == "Deportes") idCategoria = 2;
                else if (categoriaSeleccionada == "Geografía") idCategoria = 3;
                else if (categoriaSeleccionada == "Cine") idCategoria = 4;
                else if (categoriaSeleccionada == "Tecnología y video juegos") idCategoria = 5;
                query = $"SELECT tipo, pregunta, opcion_a, opcion_b, opcion_c, opcion_d, respuesta_correcta FROM preguntas WHERE categoria_id = {idCategoria} ORDER BY RAND()";
            }

            using (MySqlConnection conexion = new MySqlConnection(cadenaConexion))
            {
                try
                {
                    conexion.Open();
                    MySqlCommand comando = new MySqlCommand(query, conexion);
                    MySqlDataReader reader = comando.ExecuteReader();

                    while (reader.Read())
                    {
                        PreguntaTrivia nuevaPregunta = new PreguntaTrivia();
                        nuevaPregunta.Tipo = reader["tipo"].ToString();
                        nuevaPregunta.Texto = reader["pregunta"].ToString();
                        nuevaPregunta.Correcta = reader["respuesta_correcta"].ToString();

                        string opA = reader["opcion_a"].ToString();
                        string opB = reader["opcion_b"].ToString();
                        string opC = reader["opcion_c"].ToString();
                        string opD = reader["opcion_d"].ToString();

                        if (nuevaPregunta.Tipo == "opcion_multiple")
                        {
                            nuevaPregunta.Opciones = new string[] { "A) " + opA, "B) " + opB, "C) " + opC, "D) " + opD };
                        }
                        else
                        {
                            nuevaPregunta.Opciones = new string[] { opA, opB, opC, opD };
                        }
                        mazoPreguntas.Add(nuevaPregunta);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error de conexión: " + ex.Message);
                }
            }
            MostrarSiguientePregunta();
        }

        private void MostrarSiguientePregunta()
        {
            if (indicePreguntaActual < mazoPreguntas.Count)
            {
                pActual = mazoPreguntas[indicePreguntaActual];
                this.Invalidate();
            }
            else
            {
                juegoTerminado = true;
                this.Invalidate();
            }
        }

        private void FormJuego_Paint(object sender, PaintEventArgs e)
        {
            if (pActual == null) return;

            Graphics g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            if (imagenFondo != null)
                g.DrawImage(imagenFondo, 0, 0, this.ClientSize.Width, this.ClientSize.Height);

            using (SolidBrush filtro = new SolidBrush(Color.FromArgb(120, 0, 0, 0)))
                g.FillRectangle(filtro, 0, 0, this.ClientSize.Width, this.ClientSize.Height);

            Font fuenteTitulo = new Font("Arial", 20, FontStyle.Bold);
            g.DrawString($"Categoría: {categoriaSeleccionada}  |  Pregunta {indicePreguntaActual + 1} de {mazoPreguntas.Count}", fuenteTitulo, Brushes.LightGray, 50, 30);

            string marcador = $"Aciertos: {aciertos}    Errores: {errores}";
            SizeF tamMarcador = g.MeasureString(marcador, fuenteTitulo);
            g.DrawString(marcador, fuenteTitulo, Brushes.Yellow, this.ClientSize.Width - tamMarcador.Width - 50, 30);

            Font fuentePregunta = new Font("Showcard Gothic", 25, FontStyle.Bold);
            SizeF tamPregunta = g.MeasureString(pActual.Texto, fuentePregunta, this.ClientSize.Width - 100);
            RectangleF rectPregunta = new RectangleF(50, 100, this.ClientSize.Width - 100, tamPregunta.Height);
            g.DrawString(pActual.Texto, fuentePregunta, Brushes.White, rectPregunta);

            areasOpciones.Clear();
            Font fuenteOpciones = new Font("Arial", 18, FontStyle.Bold);
            string[] letrasIncisos = { "a", "b", "c", "d" };

            if (pActual.Tipo == "imagen")
            {
                int anchoBoton = 320;
                int altoBoton = 220;
                int separacionX = 80;
                int separacionY = 40;

                int anchoTotal = (anchoBoton * 2) + separacionX;
                int xInicio = (this.ClientSize.Width / 2) - (anchoTotal / 2);
                int yInicio = (int)(rectPregunta.Bottom + 30);

                for (int i = 0; i < 4; i++)
                {
                    if (pActual.Opciones[i] == null) continue;

                    int columna = i % 2;
                    int fila = i / 2;

                    int rectX = xInicio + (columna * (anchoBoton + separacionX));
                    int rectY = yInicio + (fila * (altoBoton + separacionY));

                    Rectangle rect = new Rectangle(rectX, rectY, anchoBoton, altoBoton);
                    areasOpciones.Add(rect, letrasIncisos[i]);

                    g.FillRectangle(new SolidBrush(Color.FromArgb(180, 0, 0, 0)), rect.X + 5, rect.Y + 5, rect.Width, rect.Height);
                    g.FillRectangle(new SolidBrush(Color.MediumSlateBlue), rect);
                    g.DrawRectangle(new Pen(Color.White, 3), rect);

                    g.DrawString(letrasIncisos[i].ToUpper() + ")", fuenteOpciones, Brushes.White, rect.X, rect.Y - 25);

                    try
                    {
                        string rutaOriginal = pActual.Opciones[i];
                        int indexImag = rutaOriginal.IndexOf("Imag", StringComparison.OrdinalIgnoreCase);

                        if (indexImag != -1)
                        {
                            string rutaCorta = rutaOriginal.Substring(indexImag);
                            string rutaFinal = Path.Combine(Application.StartupPath, @"..\..\Resources", rutaCorta);

                            using (Image imgOpcion = Image.FromFile(rutaFinal))
                            {
                                g.DrawImage(imgOpcion, rect.X + 10, rect.Y + 10, rect.Width - 20, rect.Height - 20);
                            }
                        }
                    }
                    catch
                    {
                        g.DrawString("Error cargar imagen", fuenteOpciones, Brushes.Red, rect.X + 20, rect.Y + 40);
                    }
                }
            }
            else
            {
                int anchoBoton = 600;
                int altoBoton = 60;
                int separacion = 30;
                int xInicio = (this.ClientSize.Width / 2) - (anchoBoton / 2);
                int yInicio = (int)(rectPregunta.Bottom + 50);

                for (int i = 0; i < 4; i++)
                {
                    if (pActual.Opciones[i] == null) continue;

                    Rectangle rect = new Rectangle(xInicio, yInicio + (i * (altoBoton + separacion)), anchoBoton, altoBoton);
                    areasOpciones.Add(rect, letrasIncisos[i]);

                    g.FillRectangle(new SolidBrush(Color.FromArgb(180, 0, 0, 0)), rect.X + 5, rect.Y + 5, rect.Width, rect.Height);
                    g.FillRectangle(new SolidBrush(Color.MediumSlateBlue), rect);
                    g.DrawRectangle(new Pen(Color.White, 3), rect);

                    SizeF tamTexto = g.MeasureString(pActual.Opciones[i], fuenteOpciones);
                    g.DrawString(pActual.Opciones[i], fuenteOpciones, Brushes.White, rect.X + 20, rect.Y + (rect.Height / 2) - (tamTexto.Height / 2));
                }
            }

            if (mostrandoResultado)
            {
                using (SolidBrush capaOscura = new SolidBrush(Color.FromArgb(180, 0, 0, 0)))
                    g.FillRectangle(capaOscura, 0, 0, this.ClientSize.Width, this.ClientSize.Height);

                string mensaje = ultimaRespuestaCorrecta ? "¡CORRECTO! ✔" : "¡INCORRECTO! ❌";
                Color colorMensaje = ultimaRespuestaCorrecta ? Color.LimeGreen : Color.Red;
                Font fuenteMensaje = new Font("Showcard Gothic", 60, FontStyle.Bold);

                SizeF tamMsj = g.MeasureString(mensaje, fuenteMensaje);
                float xCentrado = (this.ClientSize.Width / 2) - (tamMsj.Width / 2);
                float yCentrado = (this.ClientSize.Height / 2) - (tamMsj.Height / 2);

                g.DrawString(mensaje, fuenteMensaje, new SolidBrush(colorMensaje), xCentrado, yCentrado);

                if (!ultimaRespuestaCorrecta)
                {
                    string textoAyuda = "La respuesta era el inciso: " + pActual.Correcta.ToUpper();
                    Font fuenteAyuda = new Font("Arial", 20, FontStyle.Bold);
                    SizeF tamAyuda = g.MeasureString(textoAyuda, fuenteAyuda);

                    g.DrawString(textoAyuda, fuenteAyuda, Brushes.White,
                        (this.ClientSize.Width / 2) - (tamAyuda.Width / 2),
                        yCentrado + tamMsj.Height + 20);
                }
            }

            if (juegoTerminado)
            {
                using (SolidBrush capaOscura = new SolidBrush(Color.FromArgb(230, 0, 0, 0)))
                    g.FillRectangle(capaOscura, 0, 0, this.ClientSize.Width, this.ClientSize.Height);

                string tituloFin = "¡FIN DE LA CATEGORÍA!";
                Font fuenteFin = new Font("Showcard Gothic", 50, FontStyle.Bold);
                SizeF tamTitulo = g.MeasureString(tituloFin, fuenteFin);
                g.DrawString(tituloFin, fuenteFin, Brushes.Gold, (this.ClientSize.Width / 2) - (tamTitulo.Width / 2), (this.ClientSize.Height / 2) - 150);

                string textoPuntos = $"ACIERTOS: {aciertos}      ERRORES: {errores}";
                Font fuentePuntos = new Font("Arial", 40, FontStyle.Bold);
                SizeF tamPuntos = g.MeasureString(textoPuntos, fuentePuntos);
                g.DrawString(textoPuntos, fuentePuntos, Brushes.White, (this.ClientSize.Width / 2) - (tamPuntos.Width / 2), (this.ClientSize.Height / 2));

                string textoSalir = "Haz clic en cualquier parte para regresar al menú...";
                Font fuenteSalir = new Font("Arial", 20, FontStyle.Italic);
                SizeF tamSalir = g.MeasureString(textoSalir, fuenteSalir);
                g.DrawString(textoSalir, fuenteSalir, Brushes.LightGray, (this.ClientSize.Width / 2) - (tamSalir.Width / 2), (this.ClientSize.Height / 2) + 120);
            }
        }

        private void FormJuego_MouseClick(object sender, MouseEventArgs e)
        {
            if (juegoTerminado)
            {
                foreach (Form pantalla in Application.OpenForms)
                {
                    if (pantalla is SeleccionCategoria)
                    {
                        pantalla.Show();
                        break;
                    }
                }

                this.Close();
                return;
            }

            if (indicePreguntaActual >= mazoPreguntas.Count || mostrandoResultado) return;

            foreach (var opcion in areasOpciones)
            {
                if (opcion.Key.Contains(e.Location))
                {
                    string incisoElegido = opcion.Value;

                    if (incisoElegido == pActual.Correcta)
                    {
                        ultimaRespuestaCorrecta = true;
                        aciertos++;
                    }
                    else
                    {
                        ultimaRespuestaCorrecta = false;
                        errores++;
                    }

                    mostrandoResultado = true;
                    this.Invalidate();
                    timerResultado.Start();
                    break;
                }
            }
        }

        private void TimerResultado_Tick(object sender, EventArgs e)
        {
            timerResultado.Stop();
            mostrandoResultado = false;

            indicePreguntaActual++;
            MostrarSiguientePregunta();
        }

        private void FormJuego_Resize(object sender, EventArgs e)
        {
            this.Invalidate();
        }

        private void seleccionMusica_Load(object sender, EventArgs e) { }
    }

    public class PreguntaTrivia
    {
        public string Tipo { get; set; }
        public string Texto { get; set; }
        public string[] Opciones { get; set; }
        public string Correcta { get; set; }
    }
}