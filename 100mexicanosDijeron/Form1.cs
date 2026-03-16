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
    public partial class pantallaCat : Form
    {
        Rectangle rectBotonJugar = new Rectangle(0, 0, 200, 60);
        public pantallaCat()
        {
            InitializeComponent();
            this.DoubleBuffered = true;
        }

        private void pantallaCat_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;

            // Dibuja la imagen
            g.DrawImage(Properties.Resources.pantallaMenu, 0, 0, this.ClientSize.Width, this.ClientSize.Height);

            // Calcula el centro
            float xCentro = this.ClientSize.Width / 2;
            float yCentro = this.ClientSize.Height / 2;

            // --- Botón ---
            int anchoBoton = 200;
            int altoBoton = 60;
            int xBoton = (int)xCentro - (anchoBoton / 2);
            int yBoton = (int)yCentro + 20;

            rectBotonJugar = new Rectangle(xBoton, yBoton, anchoBoton, altoBoton);

            // Dibujo del fondo
            g.FillRectangle(new SolidBrush(Color.FromArgb(255, 60, 180, 75)), rectBotonJugar);
            // Dibujamos el borde
            g.DrawRectangle(new Pen(Color.White, 3), rectBotonJugar);

            // Texto
            Font fuenteBoton = new Font("Showcard Gothic", 20, FontStyle.Bold);
            SizeF tamTextoBoton = g.MeasureString("JUGAR", fuenteBoton);
            g.DrawString("JUGAR", fuenteBoton, Brushes.White,
                xCentro - (tamTextoBoton.Width / 2),
                yBoton + (altoBoton / 2) - (tamTextoBoton.Height / 2));
        }

        private void pantallaCat_MouseClick(object sender, MouseEventArgs e)
        {
            // rectBotonJugar tiene las coordenadas de el Paint
            if (rectBotonJugar.Contains(e.Location))
            {
                SeleccionCategoria pantallaCategorias = new SeleccionCategoria();
                pantallaCategorias.FormClosed += (s, args) => Application.Exit();
                pantallaCategorias.Show();
                this.Hide();
            }
        }

        private void pantallaCat_Resize(object sender, EventArgs e)
        {
            this.Invalidate(); // Redibuja el tamaño de la ventana
        }
    }
}