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

        // ui y animaciones
        Rectangle rectCajaNombre, rectCajaIP, rectBotonConectar;
        System.Windows.Forms.Timer timerCursor = new System.Windows.Forms.Timer { Interval = 500 };
        bool mostrarCursor = true;

        // estado de la red
        bool conectado = false;
        List<string> jugadoresConectados = new List<string>();
        string mensajeLobby = "Esperando jugadores...";
        int esperando = 0;

        TcpClient tcpCliente;
        NetworkStream stream;
        volatile bool escuchando = true;

        // control de sala y categorias
        bool soyHost = false;
        bool categoriaElegida = false;
        string nombreCategoriaElegida = "";

        readonly string[] NOMBRES_CATEGORIAS = {
            "Música", "Deportes", "Geografía y Viajes",
            "Cine y Series", "Videojuegos", "Aleatorio"
        };
        readonly int[] IDS_CATEGORIAS = { 1, 2, 3, 4, 5, 0 };
        Rectangle[] rectsCategorias = new Rectangle[6];

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

            // tabulador para alternar entre campos
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

            // pintamos la pantalla segun el estado del jugador
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

            int panelH = soyHost ? 520 : 420;
            Rectangle panel = new Rectangle(cx - 300, cy - 220, 600, panelH);
            using (SolidBrush bg = new SolidBrush(Color.FromArgb(210, 10, 10, 40)))
                g.FillRectangle(bg, panel);
            g.DrawRectangle(new Pen(Color.MediumSlateBlue, 3), panel);

            g.DrawString("SALA DE ESPERA", fTitulo, Brushes.Gold, panel.X + 60, panel.Y + 20);
            g.DrawString("Jugadores conectados:", fSub, Brushes.White, panel.X + 20, panel.Y + 80);

            for (int i = 0; i < jugadoresConectados.Count; i++)
                g.DrawString("  ✔ " + jugadoresConectados[i], fJugador, Brushes.LimeGreen,
                    panel.X + 20, panel.Y + 120 + i * 38);

            // dibuja los botones de categorias si eres el host
            if (soyHost)
            {
                DibujarSelectorCategoria(g, panel);
            }
            else
            {
                Font fEsp = new Font("Arial", 14, FontStyle.Italic);
                string textoEsp = categoriaElegida
                    ? $"Categoría: {nombreCategoriaElegida}"
                    : "Esperando que el host elija categoría...";
                Color colorEsp = categoriaElegida ? Color.Gold : Color.LightGray;
                g.DrawString(textoEsp, fEsp, new SolidBrush(colorEsp),
                    panel.X + 20, panel.Bottom - 100);
            }

            Color colorMsg = esperando > 0 ? Color.Yellow : Color.LimeGreen;
            g.DrawString(mensajeLobby, fSub, new SolidBrush(colorMsg), panel.X + 20, panel.Bottom - 60);
        }

        void DibujarSelectorCategoria(Graphics g, Rectangle panel)
        {
            Font fLabel = new Font("Showcard Gothic", 13);
            Font fCat = new Font("Arial", 12, FontStyle.Bold);

            int yBase = panel.Y + 210;

            if (!categoriaElegida)
                g.DrawString("ELIGE LA CATEGORÍA:", fLabel, Brushes.Gold, panel.X + 20, yBase);
            else
                g.DrawString($"CATEGORÍA: {nombreCategoriaElegida.ToUpper()}", fLabel, Brushes.LimeGreen, panel.X + 20, yBase);

            int btnW = 172, btnH = 44, gap = 8;
            int startX = panel.X + 14;
            int startY = yBase + 36;

            Color[] coloresCat = {
                Color.FromArgb(180, 148, 0, 211),
                Color.FromArgb(180, 0, 128, 0),
                Color.FromArgb(180, 0, 100, 180),
                Color.FromArgb(180, 180, 80, 0),
                Color.FromArgb(180, 0, 140, 140),
                Color.FromArgb(180, 120, 120, 120),
            };

            for (int i = 0; i < 6; i++)
            {
                int col = i % 3;
                int fila = i / 3;
                int x = startX + col * (btnW + gap);
                int y = startY + fila * (btnH + gap);
                rectsCategorias[i] = new Rectangle(x, y, btnW, btnH);

                bool estaSeleccionado = categoriaElegida && NOMBRES_CATEGORIAS[i] == nombreCategoriaElegida;

                using (SolidBrush br = new SolidBrush(estaSeleccionado ? Color.Gold : coloresCat[i]))
                    g.FillRectangle(br, rectsCategorias[i]);

                g.DrawRectangle(new Pen(estaSeleccionado ? Color.White : Color.FromArgb(200, 255, 255, 255),
                    estaSeleccionado ? 3 : 1), rectsCategorias[i]);

                Color textColor = estaSeleccionado ? Color.DarkSlateBlue : Color.White;
                SizeF sz = g.MeasureString(NOMBRES_CATEGORIAS[i], fCat);
                g.DrawString(NOMBRES_CATEGORIAS[i], fCat, new SolidBrush(textColor),
                    x + (btnW - sz.Width) / 2,
                    y + (btnH - sz.Height) / 2);
            }
        }

        private void OnMouseClick(object sender, MouseEventArgs e)
        {
            if (!conectado)
            {
                if (rectCajaNombre.Contains(e.Location)) { escribiendoNombre = true; Invalidate(); }
                else if (rectCajaIP.Contains(e.Location)) { escribiendoNombre = false; Invalidate(); }
                else if (rectBotonConectar.Contains(e.Location)) Conectar();
                return;
            }

            // detecta clics en los botones de categoria si eres el host y no se ha elegido una
            if (soyHost && !categoriaElegida)
            {
                for (int i = 0; i < rectsCategorias.Length; i++)
                {
                    if (rectsCategorias[i].Contains(e.Location))
                    {
                        EnviarCategoria(IDS_CATEGORIAS[i], NOMBRES_CATEGORIAS[i]);
                        return;
                    }
                }
            }
        }

        void EnviarCategoria(int categoriaId, string nombreCat)
        {
            try
            {
                string msg = JsonSerializer.Serialize(new
                {
                    tipo = "elegir_categoria",
                    categoria_id = categoriaId,
                    nombre = nombreCat
                }) + "\n";
                byte[] datos = Encoding.UTF8.GetBytes(msg);
                stream.Write(datos, 0, datos.Length);

                categoriaElegida = true;
                nombreCategoriaElegida = nombreCat;
                Invalidate();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al enviar categoría:\n" + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        void Conectar()
        {
            if (string.IsNullOrWhiteSpace(nombre))
            { MessageBox.Show("Escribe tu nombre.", "Falta nombre", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
            if (string.IsNullOrWhiteSpace(ipServidor))
            { MessageBox.Show("Escribe la IP.", "Falta IP", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }

            try
            {
                // iniciamos conexion con el servidor tcp
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
            // leemos los json que manda el servidor
            var msg = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(linea);
            string tipo = msg["tipo"].GetString();

            switch (tipo)
            {
                case "bienvenida":
                    if (msg.ContainsKey("esHost") && msg["esHost"].GetBoolean())
                        soyHost = true;
                    ActualizarLobby(msg);
                    break;

                case "jugador_unido":
                    ActualizarLobby(msg);
                    break;

                case "categoria_elegida":
                    string nomCat = msg.ContainsKey("nombre") ? msg["nombre"].GetString() : "?";
                    this.Invoke((Action)(() =>
                    {
                        categoriaElegida = true;
                        nombreCategoriaElegida = nomCat;
                        Invalidate();
                    }));
                    break;

                case "inicio_juego":
                    escuchando = false;
                    Thread.Sleep(50);
                    this.Invoke((Action)(() =>
                    {
                        timerCursor.Stop();
                        // abrimos el form del juego pasandole la ip del servidor
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