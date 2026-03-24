namespace _100mexicanosDijeron
{
    partial class SeleccionCategoria
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // SeleccionCategoria
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.ForeColor = System.Drawing.SystemColors.ControlText;
            this.Name = "SeleccionCategoria";
            this.Text = "Seleccion-Categoria";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.Paint += new System.Windows.Forms.PaintEventHandler(this.SeleccionCategoria_Paint);
            this.MouseClick += new System.Windows.Forms.MouseEventHandler(this.SeleccionCategoria_MouseClick);
            this.Resize += new System.EventHandler(this.SeleccionCategoria_Resize);
            this.ResumeLayout(false);

        }

        #endregion
    }
}