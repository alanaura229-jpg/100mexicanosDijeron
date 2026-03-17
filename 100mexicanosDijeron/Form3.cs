using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace _100mexicanosDijeron
{
    
    public partial class FormJuego : Form
    {
        
        string cadenaConexion = "Server=127.0.0.1;Database=juego_trivia;Uid=root;Pwd=root;";
        private string categoriaSeleccionada;
        private Image imagenFondo;

        private List<PreguntaTrivia> mazoPreguntas = new List<PreguntaTrivia>();
        private int indicePreguntaActual = 0;
        private int aciertos = 0;
        private int errores = 0;

        private string textoPregunta = "Cargando...";
        private string[] textosOpciones = new string[4];
        private string respuestaCorrecta = "";
        private Dictionary<Rectangle, string> areasOpciones = new Dictionary<Rectangle, string>();

        public FormJuego(String categoria)
        {
            InitializeComponent();
            this.categoriaSeleccionada = categoria;
            this.DoubleBuffered = true;
            this.WindowState = FormWindowState.Maximized;

            switch (categoria)
            {
                case "Música": imagenFondo = Properties.Resources.musica; break;
                case "Deportes": imagenFondo = Properties.Resources.Deporte; break;
                case "Geografía": imagenFondo = Properties.Resources.Geografia; break;
                case "Cine": imagenFondo = Properties.Resources.Cine; break;
                case "Tecnología y video juegos": imagenFondo = Properties.Resources.VideoJuego; break;
                default: imagenFondo = Properties.Resources.categoria; break;
            }

            DescargarMazoDePreguntas();
        }

        private void DescargarMazoDePreguntas()
        {
            int idCategoria = 1;
            if (categoriaSeleccionada == "Deportes") idCategoria = 2;
            else if (categoriaSeleccionada == "Geografía") idCategoria = 3;
            else if (categoriaSeleccionada == "Cine") idCategoria = 4;
            else if (categoriaSeleccionada == "Tecnología y video juegos") idCategoria = 5;

            using (MySqlConnection conexion = new MySqlConnection(cadenaConexion))
            {
                try
                {
                    conexion.Open();
                    string query = $"SELECT pregunta, opcion_a, opcion_b, opcion_c, opcion_d, respuesta_correcta FROM preguntas WHERE categoria_id = {idCategoria} AND tipo = 'opcion_multiple' ORDER BY RAND()";

                    MySqlCommand comando = new MySqlCommand(query, conexion);
                    MySqlDataReader reader = comando.ExecuteReader();

                    while (reader.Read())
                    {
                        PreguntaTrivia nuevaPregunta = new PreguntaTrivia();
                        nuevaPregunta.Texto = reader["pregunta"].ToString();
                        nuevaPregunta.Opciones = new string[] {
                            "A) " + reader["opcion_a"].ToString(),
                            "B) " + reader["opcion_b"].ToString(),
                            "C) " + reader["opcion_c"].ToString(),
                            "D) " + reader["opcion_d"].ToString()
                        };
                        nuevaPregunta.Correcta = reader["respuesta_correcta"].ToString();

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
                PreguntaTrivia pActual = mazoPreguntas[indicePreguntaActual];
                textoPregunta = pActual.Texto;
                textosOpciones = pActual.Opciones;
                respuestaCorrecta = pActual.Correcta;

                this.Invalidate();
            }
            else
            {
                MessageBox.Show($"¡Fin de la categoría {categoriaSeleccionada}!\n\nAciertos: {aciertos}\nErrores: {errores}", "Resultados Finales");
                this.Close();
            }
        }

        private void FormJuego_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            if (imagenFondo != null)
                g.DrawImage(imagenFondo, 0, 0, this.ClientSize.Width, this.ClientSize.Height);

            using (SolidBrush filtro = new SolidBrush(Color.FromArgb(120, 0, 0, 0)))
                g.FillRectangle(filtro, 0, 0, this.ClientSize.Width, this.ClientSize.Height);

            Font fuenteTitulo = new Font("Arial", 20, FontStyle.Bold);
            g.DrawString($"Categoría: {categoriaSeleccionada}  |  Pregunta {indicePreguntaActual + 1} de {mazoPreguntas.Count}", fuenteTitulo, Brushes.LightGray, 50, 30);

            Font fuentePregunta = new Font("Showcard Gothic", 25, FontStyle.Bold);
            SizeF tamPregunta = g.MeasureString(textoPregunta, fuentePregunta, this.ClientSize.Width - 100);
            RectangleF rectPregunta = new RectangleF(50, 100, this.ClientSize.Width - 100, tamPregunta.Height);
            g.DrawString(textoPregunta, fuentePregunta, Brushes.White, rectPregunta);

            areasOpciones.Clear();
            int anchoBoton = 600;
            int altoBoton = 60;
            int separacion = 30;
            int xInicio = (this.ClientSize.Width / 2) - (anchoBoton / 2);
            int yInicio = (int)(rectPregunta.Bottom + 50);

            Font fuenteOpciones = new Font("Arial", 18, FontStyle.Bold);
            string[] letrasIncisos = { "a", "b", "c", "d" };

            for (int i = 0; i < 4; i++)
            {
                if (textosOpciones[i] == null) continue;

                Rectangle rect = new Rectangle(xInicio, yInicio + (i * (altoBoton + separacion)), anchoBoton, altoBoton);
                areasOpciones.Add(rect, letrasIncisos[i]);

                g.FillRectangle(new SolidBrush(Color.FromArgb(180, 0, 0, 0)), rect.X + 5, rect.Y + 5, rect.Width, rect.Height);
                g.FillRectangle(new SolidBrush(Color.MediumSlateBlue), rect);
                g.DrawRectangle(new Pen(Color.White, 3), rect);

                SizeF tamTexto = g.MeasureString(textosOpciones[i], fuenteOpciones);
                g.DrawString(textosOpciones[i], fuenteOpciones, Brushes.White, rect.X + 20, rect.Y + (rect.Height / 2) - (tamTexto.Height / 2));
            }
        }

        private void FormJuego_MouseClick(object sender, MouseEventArgs e)
        {
            if (indicePreguntaActual >= mazoPreguntas.Count) return;

            foreach (var opcion in areasOpciones)
            {
                if (opcion.Key.Contains(e.Location))
                {
                    string incisoElegido = opcion.Value;

                    if (incisoElegido == respuestaCorrecta)
                    {
                        MessageBox.Show("¡CORRECTO!");
                        aciertos++;
                    }
                    else
                    {
                        MessageBox.Show("Incorrecto. La respuesta era el inciso " + respuestaCorrecta.ToUpper());
                        errores++;
                    }

                    indicePreguntaActual++;
                    MostrarSiguientePregunta();
                    break;
                }
            }
        }

        private void FormJuego_Resize(object sender, EventArgs e)
        {
            this.Invalidate();
        }

        private void seleccionMusica_Load(object sender, EventArgs e) { }
    }

    public class PreguntaTrivia
    {
        public string Texto { get; set; }
        public string[] Opciones { get; set; }
        public string Correcta { get; set; }
    }
}