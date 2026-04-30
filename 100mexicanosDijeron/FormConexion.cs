using System;
using System.Collections.Generic;
using System.Drawing;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Windows.Forms;

namespace _100mexicanosDijeron
{
    public partial class FormConexion : Form
    {
        const int PUERTO = 54321;

        string nombre = "";
        string ipServidor = "127.0.0.1";
        bool escribiendoNombre = true;

        Rectangle rectCajaNombre, rectCajaIP, rectBotonConectar;
        System.Windows.Forms.Timer timerCursor = new System.Windows.Forms.Timer { Interval = 500 };
        bool mostrarCursor = true;

        bool conectado = false;
        List<string> jugadoresConectados = new List<string>();
        string mensajeLobby = "Esperando jugadores...";
        int esperando = 0;

        TcpClient tcpCliente;
        NetworkStream stream;

        volatile bool escuchando = true;

        public FormConexion()
        {
            InitializeComponent();
            this.DoubleBuffered = true;
            this.WindowState = FormWindowState.Maximized;
            this.KeyPress += OnKeyPress;
            this.MouseClick += OnMouseClick;
            timerCursor.Tick += (s, e) => { mostrarCursor = !mostrarCursor; Invalidate(); };
            timerCursor.Start();
        }

        private void OnKeyPress(object sender, KeyPressEventArgs e)
        {
            if (conectado) return;

            if (e.KeyChar == '\t')
            {
                escribiendoNombre = !escribiendoNombre;
                Invalidate();
                return;
            }

            if (escribiendoNombre)
            {
                if (e.KeyChar == (char)Keys.Back) { if (nombre.Length > 0) nombre = nombre.Substring(0, nombre.Length - 1); }
                else if (!char.IsControl(e.KeyChar) && nombre.Length < 30) nombre += e.KeyChar;
            }
            else
            {
                if (e.KeyChar == (char)Keys.Back) { if (ipServidor.Length > 0) ipServidor = ipServidor.Substring(0, ipServidor.Length - 1); }
                else if (!char.IsControl(e.KeyChar) && ipServidor.Length < 30) ipServidor += e.KeyChar;
            }
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            g.DrawImage(Properties.Resources.pantallaMenu, 0, 0, ClientSize.Width, ClientSize.Height);

            int cx = ClientSize.Width / 2;
            int cy = ClientSize.Height / 2;

            if (!conectado) DibujarPantallaConexion(g, cx, cy);
            else DibujarLobby(g, cx, cy);
        }

        void DibujarPantallaConexion(Graphics g, int cx, int cy)
        {
            Font fTitulo = new Font("Showcard Gothic", 28, FontStyle.Bold);
            Font fLabel = new Font("Showcard Gothic", 14);
            Font fTexto = new Font("Arial", 20, FontStyle.Bold);
            Font fHint = new Font("Arial", 11, FontStyle.Italic);

            rectCajaNombre = new Rectangle(cx - 220, cy - 110, 440, 55);
            DibujarCampo(g, rectCajaNombre, "TU NOMBRE:", nombre, escribiendoNombre, fLabel, fTexto);

            rectCajaIP = new Rectangle(cx - 220, cy - 20, 440, 55);
            DibujarCampo(g, rectCajaIP, "IP DEL SERVIDOR:", ipServidor, !escribiendoNombre, fLabel, fTexto);

            g.DrawString("Presiona TAB para cambiar de campo", fHint,
                new SolidBrush(Color.FromArgb(180, 255, 255, 255)), cx - 160, cy + 48);

            rectBotonConectar = new Rectangle(cx - 120, cy + 80, 240, 60);
            g.FillRectangle(Brushes.MediumSlateBlue, rectBotonConectar);
            g.DrawRectangle(new Pen(Color.White, 3), rectBotonConectar);
            Font fBtn = new Font("Showcard Gothic", 18);
            SizeF tamBtn = g.MeasureString("CONECTAR", fBtn);
            g.DrawString("CONECTAR", fBtn, Brushes.White,
                rectBotonConectar.X + (rectBotonConectar.Width - tamBtn.Width) / 2,
                rectBotonConectar.Y + (rectBotonConectar.Height - tamBtn.Height) / 2);

            g.DrawString($"Puerto: {PUERTO}", fHint,
                new SolidBrush(Color.FromArgb(160, 255, 255, 255)), cx - 40, cy + 155);
        }

        void DibujarCampo(Graphics g, Rectangle rect, string label, string valor, bool activo, Font fLabel, Font fTexto)
        {
            Color colorBorde = activo ? Color.Gold : Color.MediumSlateBlue;
            using (SolidBrush bg = new SolidBrush(Color.FromArgb(220, 255, 255, 255)))
                g.FillRectangle(bg, rect);
            g.DrawRectangle(new Pen(colorBorde, activo ? 4 : 2), rect);
            g.DrawString(label, fLabel, activo ? Brushes.Gold : Brushes.White, rect.X, rect.Y - 28);
            string mostrar = valor + (activo && mostrarCursor ? "|" : "");
            g.DrawString(mostrar, fTexto, Brushes.Black, rect.X + 12, rect.Y + 10);
        }

        void DibujarLobby(Graphics g, int cx, int cy)
        {
            Font fTitulo = new Font("Showcard Gothic", 26, FontStyle.Bold);
            Font fSub = new Font("Arial", 18, FontStyle.Bold);
            Font fJugador = new Font("Arial", 16);

            Rectangle panel = new Rectangle(cx - 280, cy - 200, 560, 400);
            using (SolidBrush bg = new SolidBrush(Color.FromArgb(210, 10, 10, 40)))
                g.FillRectangle(bg, panel);
            g.DrawRectangle(new Pen(Color.MediumSlateBlue, 3), panel);

            g.DrawString("SALA DE ESPERA", fTitulo, Brushes.Gold, panel.X + 60, panel.Y + 20);
            g.DrawString("Jugadores conectados:", fSub, Brushes.White, panel.X + 20, panel.Y + 80);

            for (int i = 0; i < jugadoresConectados.Count; i++)
                g.DrawString("  ✔ " + jugadoresConectados[i], fJugador, Brushes.LimeGreen,
                    panel.X + 20, panel.Y + 120 + i * 38);

            Color colorMsg = esperando > 0 ? Color.Yellow : Color.LimeGreen;
            g.DrawString(mensajeLobby, fSub, new SolidBrush(colorMsg), panel.X + 20, panel.Bottom - 70);
        }

        private void OnMouseClick(object sender, MouseEventArgs e)
        {
            if (conectado) return;
            if (rectCajaNombre.Contains(e.Location)) { escribiendoNombre = true; Invalidate(); }
            else if (rectCajaIP.Contains(e.Location)) { escribiendoNombre = false; Invalidate(); }
            else if (rectBotonConectar.Contains(e.Location)) Conectar();
        }

        void Conectar()
        {
            if (string.IsNullOrWhiteSpace(nombre))
            { MessageBox.Show("Escribe tu nombre.", "Falta nombre", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
            if (string.IsNullOrWhiteSpace(ipServidor))
            { MessageBox.Show("Escribe la IP.", "Falta IP", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }

            try
            {
                tcpCliente = new TcpClient();
                tcpCliente.Connect(ipServidor.Trim(), PUERTO);
                stream = tcpCliente.GetStream();

                string msg = JsonSerializer.Serialize(new { tipo = "unirse", nombre = nombre.Trim() }) + "\n";
                byte[] datos = Encoding.UTF8.GetBytes(msg);
                stream.Write(datos, 0, datos.Length);

                conectado = true;
                Invalidate();

                new Thread(EscucharServidor) { IsBackground = true }.Start();
            }
            catch (Exception ex)
            {
                MessageBox.Show("No se pudo conectar:\n" + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        void EscucharServidor()
        {
            var sb = new StringBuilder();
            byte[] buffer = new byte[1];
            try
            {
                while (tcpCliente.Connected && escuchando)
                {
                    int n = stream.Read(buffer, 0, 1);
                    if (n == 0) break;
                    char c = (char)buffer[0];
                    if (c == '\n') { ProcesarMensaje(sb.ToString()); sb.Clear(); }
                    else sb.Append(c);
                }
            }
            catch { }
        }

        void ProcesarMensaje(string linea)
        {
            if (string.IsNullOrEmpty(linea)) return;
            var msg = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(linea);
            string tipo = msg["tipo"].GetString();

            switch (tipo)
            {
                case "bienvenida":
                case "jugador_unido":
                    ActualizarLobby(msg);
                    break;
                case "inicio_juego":
                    escuchando = false;  // 🛑 detener el hilo viejo PRIMERO
                    Thread.Sleep(50);    // pequeña pausa para asegurarse
                    this.Invoke((Action)(() =>
                    {
                        timerCursor.Stop();
                        // AQUÍ ESTÁ LA MAGIA: LE PASAMOS LA ipServidor AL JUEGO 👇
                        new FormJuegoRed(stream, nombre.Trim(), ipServidor.Trim()).Show();
                        this.Hide();
                    }));
                    break;
                case "error":
                    string err = msg["mensaje"].GetString();
                    this.Invoke((Action)(() =>
                        MessageBox.Show(err, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)));
                    break;
            }
        }

        void ActualizarLobby(Dictionary<string, JsonElement> msg)
        {
            jugadoresConectados.Clear();
            if (msg.ContainsKey("jugadores"))
            {
                foreach (var j in msg["jugadores"].EnumerateArray())
                    jugadoresConectados.Add(j.GetProperty("nombre").GetString() + " (" + j.GetProperty("ip").GetString() + ")");
            }
            if (msg.ContainsKey("esperando"))
                esperando = msg["esperando"].GetInt32();

            mensajeLobby = esperando > 0 ? $"Esperando {esperando} jugador(es) más..." : "¡Todos listos! Iniciando...";
            this.Invoke((Action)Invalidate);
        }

        private void FormConexion_Resize(object sender, EventArgs e) => Invalidate();
    }
}