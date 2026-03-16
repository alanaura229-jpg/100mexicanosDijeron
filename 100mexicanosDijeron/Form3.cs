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
    public partial class FormJuego : Form
    {
        private string categoriaSeleccionada;
        private Image imagenFondo;
        public FormJuego(String categoria)
        {
            InitializeComponent();
            this.categoriaSeleccionada = categoria;
            this.DoubleBuffered = true;
            this.WindowState = FormWindowState.Maximized;

            // Ajusta los nombres de tus recursos aquí
            switch (categoria)
            {
                case "Geografía":
                    imagenFondo = Properties.Resources.Geografia;
                    break;
                case "Deportes":
                    imagenFondo = Properties.Resources.Deporte;
                    break;
                case "Música":
                    imagenFondo = Properties.Resources.musica;
                    break;
                case "Cine":
                    imagenFondo = Properties.Resources.Cine;
                    break;
                case "Tecnología y video juegos":
                    imagenFondo = Properties.Resources.VideoJuego;
                    break;
                default:
                    imagenFondo = Properties.Resources.categoria; // Imagen por defecto
                    break;
            }
        }

        private void seleccionMusica_Load(object sender, EventArgs e)
        {

        }

        private void FormJuego_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            if (imagenFondo != null)
                g.DrawImage(imagenFondo, 0, 0, this.ClientSize.Width, this.ClientSize.Height);

            using (SolidBrush filtro = new SolidBrush(Color.FromArgb(120, 0, 0, 0)))
                g.FillRectangle(filtro, 0, 0, this.ClientSize.Width, this.ClientSize.Height);

            Font fuentePregunta = new Font("Arial", 25, FontStyle.Bold);
            g.DrawString("Categoría: " + categoriaSeleccionada, fuentePregunta, Brushes.White, 100, 100);
        }
    }
}
