using MySql.Data.MySqlClient;
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
    public partial class FormHistorial : Form
    {
        string cadena = "Server=127.0.0.1;Database=juego_trivia;Uid=root;Pwd=orion6363Vv!;";
        List<string> lista = new List<string>();
        public FormHistorial()
        {
            InitializeComponent();
            this.DoubleBuffered = true;
            this.WindowState = FormWindowState.Maximized;
            Cargar();
        }
        void Cargar()
        {
            lista.Clear();
            using (MySqlConnection con = new MySqlConnection(cadena))
            {
                try
                {
                    con.Open();
                    // Usamos un SELECT más sencillo por si 'fecha' tiene otro nombre en tu tabla
                    string sql = "SELECT * FROM historial_partidas WHERE usuario_id = @id ORDER BY id DESC";

                    MySqlCommand cmd = new MySqlCommand(sql, con);
                    cmd.Parameters.AddWithValue("@id", Sesion.IdUsuario);

                    using (MySqlDataReader r = cmd.ExecuteReader())
                    {
                        while (r.Read())
                        {
                            string fecha = r.HasColumn("fecha") ? Convert.ToDateTime(r["fecha"]).ToString("dd/MM/yyyy HH:mm") : "N/A";
                            string puntos = r["puntuacion_final"].ToString();

                            // Si agregaste las columnas de correctas/incorrectas como vimos antes:
                            string correctas = r.HasColumn("preguntas_correctas") ? r["preguntas_correctas"].ToString() : "?";
                            string incorrectas = r.HasColumn("preguntas_incorrectas") ? r["preguntas_incorrectas"].ToString() : "?";

                            // Creamos un formato mucho más claro
                            lista.Add($"{fecha} | Puntaje: {puntos} | ✅ {correctas} | ❌ {incorrectas}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Esto te dirá exactamente qué columna falta en el historial
                    //MessageBox.Show("Error al cargar historial: " + ex.Message);
                }
            }
            this.Invalidate(); // Obliga al Form a dibujarse de nuevo con los datos nuevos
        }


        private void FormHistorial_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            // Fondo y Contenedor
            g.DrawImage(Properties.Resources.Aleatorio, 0, 0, this.ClientSize.Width, this.ClientSize.Height);
            Rectangle r = new Rectangle(this.ClientSize.Width / 2 - 350, 50, 700, this.ClientSize.Height - 150);
            g.FillRectangle(new SolidBrush(Color.FromArgb(230, 20, 20, 20)), r);
            g.DrawRectangle(new Pen(Color.Gold, 3), r);

            // Título
            Font fuenteTitulo = new Font("Arial", 22, FontStyle.Bold);
            g.DrawString("HISTORIAL GENERAL DE PARTIDAS", fuenteTitulo, Brushes.Gold, r.X + 20, r.Y + 20);

            // Cabecera de columnas para que sea intuitivo
            Font fuenteHeader = new Font("Consolas", 12, FontStyle.Bold);
            string encabezado = string.Format("{0,-18} | {1,-10} | {2,-5} | {3,-5}", "FECHA", "PUNTOS", "CORR", "ERR");
            g.DrawString(encabezado, fuenteHeader, Brushes.Cyan, r.X + 30, r.Y + 70);
            g.DrawLine(new Pen(Color.Cyan, 1), r.X + 30, r.Y + 90, r.X + 670, r.Y + 90);

            // Listado de datos
            Font fuenteDatos = new Font("Consolas", 11);
            if (lista.Count == 0)
            {
                g.DrawString("No hay registros disponibles.", fuenteDatos, Brushes.White, r.X + 30, r.Y + 110);
            }
            else
            {
                for (int i = 0; i < lista.Count; i++)
                {
                    // Dibujamos cada línea con un color distinto si es par/impar para que sea fácil de leer
                    Brush colorTexto = (i % 2 == 0) ? Brushes.White : Brushes.LightGray;
                    g.DrawString(lista[i], fuenteDatos, colorTexto, r.X + 30, r.Y + 110 + (i * 30));
                }
            }

            g.DrawString("Haz clic en cualquier parte para salir", new Font("Arial", 10, FontStyle.Italic), Brushes.Gray, r.X + 20, r.Bottom + 10);
        }

        private void FormHistorial_MouseClick(object sender, MouseEventArgs e) => this.Close();
    }

    // Función auxiliar para evitar que el programa truene si falta una columna
    public static class ExtensionesReader
    {
        public static bool HasColumn(this MySqlDataReader reader, string columnName)
        {
            for (int i = 0; i < reader.FieldCount; i++)
            {
                if (reader.GetName(i).Equals(columnName, StringComparison.OrdinalIgnoreCase)) return true;
            }
            return false;
        }
    }

}
