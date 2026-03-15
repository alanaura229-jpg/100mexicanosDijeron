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
        public SeleccionCategoria()
        {
            InitializeComponent();
        }

        private void btnMusica_Click(object sender, EventArgs e)
        {
            seleccionMusica seleccionCategoria = new seleccionMusica();
            seleccionCategoria.Show();

            this.Hide();
        }

        private void btnCine_Click(object sender, EventArgs e)
        {
            MessageBox.Show("¡Elegiste Cine! Próximamente cargaremos las preguntas.");
        }

        private void btnGeografía_Click(object sender, EventArgs e)
        {
            MessageBox.Show("¡Elegiste Geografía! Próximamente cargaremos las preguntas.");
        }

        private void btnDeportes_Click(object sender, EventArgs e)
        {
            MessageBox.Show("¡Elegiste Deportes! Próximamente cargaremos las preguntas.");
        }

        private void btnTec_Click(object sender, EventArgs e)
        {
            MessageBox.Show("¡Elegiste Tecnología y Video Juegos! Próximamente cargaremos las preguntas.");
        }
    }
}
