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
    public partial class pantallaCat : Form
    {
        // Cadena de conexión (mantengo la tuya)
        string cadenaConexion = "Server=127.0.0.1;Database=juego_trivia;Uid=root;Pwd=orion6363Vv!;";

        // Coordenadas para detectar clics y dibujos
        Rectangle rectBotonJugar;
        Rectangle rectCajaTexto; // El "TextBox" dibujado

        // Variable que guardará lo que el usuario escribe
        string nombreIngresado = "";

        // Control visual para el cursor parpadeante (| )
        Timer timerCursor = new Timer();
        bool mostrarCursor = true;

        public pantallaCat()
        {
            InitializeComponent();
            this.DoubleBuffered = true; // Evita el parpadeo de la pantalla
            this.WindowState = FormWindowState.Maximized; // Pantalla completa

            // --- PASO 1: Escuchar el teclado ---
            this.KeyPress += PantallaCat_KeyPress; // Asociamos el evento KEYPRESS

            // Configurar el timer para el cursor parpadeante
            timerCursor.Interval = 500; // Parpadea cada medio segundo
            timerCursor.Tick += (s, e) => { mostrarCursor = !mostrarCursor; this.Invalidate(); };
            timerCursor.Start();
        }

        // --- PASO 2: Lógica de escritura (GDI+) ---
        private void PantallaCat_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Si presionó Backspace (borrar)
            if (e.KeyChar == (char)Keys.Back)
            {
                if (nombreIngresado.Length > 0)
                {
                    nombreIngresado = nombreIngresado.Substring(0, nombreIngresado.Length - 1); // Quitar última letra
                }
            }
            // Si presionó una tecla válida (letra, número, espacio)
            else if (!char.IsControl(e.KeyChar))
            {
                // Limitamos el nombre a 15 caracteres para que no se salga de la caja
                if (nombreIngresado.Length < 15)
                {
                    nombreIngresado += e.KeyChar; // Sumar la letra
                }
            }

            // Forzamos a redibujar la pantalla para que se vea la nueva letra
            this.Invalidate();
        }

        private void pantallaCat_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            // 1. Dibujar imagen de fondo original
            g.DrawImage(Properties.Resources.pantallaMenu, 0, 0, this.ClientSize.Width, this.ClientSize.Height);

            // Calcular el centro de la pantalla
            int xCentro = this.ClientSize.Width / 2;
            int yCentro = this.ClientSize.Height / 2;

            // --- PASO 3: Dibujar la Caja de Texto ---
            rectCajaTexto = new Rectangle(xCentro - 200, yCentro - 100, 400, 60);

            // Fondo de la caja (un blanco traslúcido para que se vea "bonito")
            using (SolidBrush bFondo = new SolidBrush(Color.FromArgb(220, 255, 255, 255)))
                g.FillRectangle(bFondo, rectCajaTexto);

            // Borde dorado pixelado
            g.DrawRectangle(new Pen(Color.MediumSlateBlue, 4), rectCajaTexto);

            // Etiqueta arriba de la caja
            Font fuenteEtiqueta = new Font("Showcard Gothic", 16);
            g.DrawString("INGRESA TU NOMBRE:", fuenteEtiqueta, Brushes.White, rectCajaTexto.X, rectCajaTexto.Y - 35);

            // --- PASO 4: Pintar el Texto que escribió el usuario ---
            Font fuenteTexto = new Font("Arial", 22, FontStyle.Bold);
            string textoAMostrar = nombreIngresado;

            // Si el timer dice que mostremos el cursor, lo sumamos al final
            if (mostrarCursor) textoAMostrar += "|";

            g.DrawString(textoAMostrar, fuenteTexto, Brushes.Black, rectCajaTexto.X + 15, rectCajaTexto.Y + 12);


            // --- Botón JUGAR (Mismo estilo pero dibujado) ---
            rectBotonJugar = new Rectangle(xCentro - 100, rectCajaTexto.Bottom + 50, 200, 60);

            // Relleno y borde (LimeGreen para resaltar)
            g.FillRectangle(new SolidBrush(Color.LimeGreen), rectBotonJugar);
            g.DrawRectangle(new Pen(Color.White, 3), rectBotonJugar);


            // Texto "JUGAR"
            Font fuenteBoton = new Font("Showcard Gothic", 20);
            SizeF tamTxtJugar = g.MeasureString("JUGAR", fuenteBoton);
            g.DrawString("JUGAR", fuenteBoton, Brushes.White,
                rectBotonJugar.X + (rectBotonJugar.Width / 2) - (tamTxtJugar.Width / 2),
                rectBotonJugar.Y + (rectBotonJugar.Height / 2) - (tamTxtJugar.Height / 2));
        }

        private void pantallaCat_MouseClick(object sender, MouseEventArgs e)
        {
            // Verificamos si el clic fue dentro del botón JUGAR
            if (rectBotonJugar.Contains(e.Location))
            {
                // Validamos que haya escrito algo
                if (string.IsNullOrWhiteSpace(nombreIngresado))
                {
                    MessageBox.Show("Por favor, ingresa tu nombre antes de jugar.", "Nombre Requerido", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // --- PASO 5: Base de Datos y Registro ---
                using (MySqlConnection con = new MySqlConnection(cadenaConexion))
                {
                    try
                    {
                        con.Open();
                        // SQL para insertar o recuperar el ID si ya existe
                        string sql = "INSERT INTO usuarios (nombre) VALUES (@n) " +
                                     "ON DUPLICATE KEY UPDATE id=LAST_INSERT_ID(id); SELECT LAST_INSERT_ID();";

                        MySqlCommand cmd = new MySqlCommand(sql, con);
                        cmd.Parameters.AddWithValue("@n", nombreIngresado.Trim());

                        // GUARDAR EL ID EN LA CLASE GLOBAL SESION
                        Sesion.IdUsuario = Convert.ToInt32(cmd.ExecuteScalar());
                        Sesion.Nombre = nombreIngresado.Trim();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error de conexión a la base de datos: " + ex.Message);
                        return; // No avanzamos si falla la BD
                    }
                }

                // Cerrar timer y pasar a la selección (Sin mandar parámetros)
                timerCursor.Stop();
                new SeleccionCategoria().Show();
                this.Hide();
            }
        }

        private void pantallaCat_Resize(object sender, EventArgs e) => this.Invalidate();
    }
}