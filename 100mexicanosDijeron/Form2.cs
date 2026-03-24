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
        string[] nombresCategorias = { "Geografía", "Deportes", "Música", "Cine", "Tecnología y video juegos", "Aleatorio"};
        Rectangle botonHistorial;

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

            // Fondo
            g.DrawImage(Properties.Resources.categoria, 0, 0, this.ClientSize.Width, this.ClientSize.Height);

            // Botón Historial (Dorado)
            botonHistorial = new Rectangle(this.ClientSize.Width - 250, 40, 200, 50);
            g.FillRectangle(Brushes.Gold, botonHistorial);
            g.DrawRectangle(new Pen(Color.White, 2), botonHistorial);
            g.DrawString("VER HISTORIAL", new Font("Arial", 12, FontStyle.Bold), Brushes.Black, botonHistorial.X + 30, botonHistorial.Y + 15);

            botonesCategorias.Clear();
            int yInicio = 250;
            for (int i = 0; i < nombresCategorias.Length; i++)
            {
                Rectangle r = new Rectangle(this.ClientSize.Width / 2 - 200, yInicio + (i * 60), 400, 45);
                botonesCategorias.Add(r, nombresCategorias[i]);
                g.FillRectangle(Brushes.MediumSlateBlue, r);
                g.DrawString(nombresCategorias[i], new Font("Arial", 14, FontStyle.Bold), Brushes.White, r.X + 20, r.Y + 10);
            }
        }

        private void SeleccionCategoria_MouseClick(object sender, MouseEventArgs e)
        {
            if (botonHistorial.Contains(e.Location))
            {
                new FormHistorial().ShowDialog();
                return;
            }

            foreach (var b in botonesCategorias)
            {
                if (b.Key.Contains(e.Location))
                {
                    new FormJuego(b.Value).Show();
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
