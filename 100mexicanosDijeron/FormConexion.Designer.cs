namespace _100mexicanosDijeron
{
    partial class FormConexion
    {
        private System.ComponentModel.IContainer components = null;
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }
        private void InitializeComponent()
        {
            this.SuspendLayout();
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1280, 720);
            this.Name = "FormConexion";
            this.Text = "100 Mexicanos Dijeron - Conectar";
            this.Resize += new System.EventHandler(this.FormConexion_Resize);
            this.ResumeLayout(false);
        }
    }
}
