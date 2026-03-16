using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace _100mexicanosDijeron
{
    public partial class SeleccionCategoria : Form
    {

        Dictionary<Rectangle, string> botonesCategorias = new Dictionary<Rectangle, string>();
        string[] nombresCategorias = { "Geografía", "Deportes", "Música", "Cine", "Tecnología y video juegos" };
        public SeleccionCategoria()
        {
            InitializeComponent();
            this.DoubleBuffered = true; 
            this.WindowState = FormWindowState.Maximized;
        }

        private void SeleccionCategoria_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            // Dibuja fondo
            g.DrawImage(Properties.Resources.categoria, 0, 0, this.ClientSize.Width, this.ClientSize.Height);


            // Dibuja botones
            botonesCategorias.Clear();
            int anchoBoton = 400;
            int altoBoton = 50;
            int separacion = 20;

            // Calcula el inicio
            int xInicio = (this.ClientSize.Width / 2) - (anchoBoton / 2);
            int yInicio = 270;

            Font fuenteBoton = new Font("Showcard Gothic", 18, FontStyle.Bold);

            for (int i = 0; i < nombresCategorias.Length; i++)
            {
                Rectangle rect = new Rectangle(xInicio, yInicio + (i * (altoBoton + separacion)), anchoBoton, altoBoton);
                botonesCategorias.Add(rect, nombresCategorias[i]);

                // Estética del botón
                g.FillRectangle(new SolidBrush(Color.FromArgb(180, 0, 0, 0)), rect.X + 5, rect.Y + 5, rect.Width, rect.Height); // Sombra
                g.FillRectangle(new SolidBrush(Color.MediumSlateBlue), rect); // Fondo morado
                g.DrawRectangle(new Pen(Color.White, 3), rect); // Borde blanco

                // Texto de la categoría
                SizeF tamTexto = g.MeasureString(nombresCategorias[i], fuenteBoton);
                g.DrawString(nombresCategorias[i], fuenteBoton, Brushes.White,
                    rect.X + (rect.Width / 2) - (tamTexto.Width / 2),
                    rect.Y + (rect.Height / 2) - (tamTexto.Height / 2));
            }
        }

        private void SeleccionCategoria_MouseClick(object sender, MouseEventArgs e)
        {
            // Cambiamos areasBotones por botonesCategorias
            foreach (var boton in botonesCategorias)
            {
                if (boton.Key.Contains(e.Location))
                {
                    string eleccion = boton.Value;
                    FormJuego pantallaPreguntas = new FormJuego(eleccion);
                    pantallaPreguntas.Show();
                    this.Hide();
                    break;
                }
            }
        }

        private void SeleccionCategoria_Resize(object sender, EventArgs e)
        {
            this.Invalidate();
        }
    }
}
