namespace Agenda
{
    partial class Main
    {
        /// <summary>
        /// Variável de designer necessária.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Limpar os recursos que estão sendo usados.
        /// </summary>
        /// <param name="disposing">true se for necessário descartar os recursos gerenciados; caso contrário, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Código gerado pelo Windows Form Designer

        /// <summary>
        /// Método necessário para suporte ao Designer - não modifique 
        /// o conteúdo deste método com o editor de código.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Main));
            this.pnlAgenda = new System.Windows.Forms.Panel();
            this.lbl_agenda = new System.Windows.Forms.Label();
            this.pbAgenda = new System.Windows.Forms.PictureBox();
            this.pnlAgenda.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pbAgenda)).BeginInit();
            this.SuspendLayout();
            // 
            // pnlAgenda
            // 
            this.pnlAgenda.BackColor = System.Drawing.Color.White;
            this.pnlAgenda.Controls.Add(this.lbl_agenda);
            this.pnlAgenda.Controls.Add(this.pbAgenda);
            this.pnlAgenda.Cursor = System.Windows.Forms.Cursors.Hand;
            this.pnlAgenda.Location = new System.Drawing.Point(23, 322);
            this.pnlAgenda.Name = "pnlAgenda";
            this.pnlAgenda.Size = new System.Drawing.Size(224, 271);
            this.pnlAgenda.TabIndex = 0;
            this.pnlAgenda.Click += new System.EventHandler(this.pnlAgenda_Click);
            this.pnlAgenda.MouseEnter += new System.EventHandler(this.pnlAgenda_MouseEnter);
            this.pnlAgenda.MouseLeave += new System.EventHandler(this.pnlAgenda_MouseLeave);
            // 
            // lbl_agenda
            // 
            this.lbl_agenda.AutoSize = true;
            this.lbl_agenda.Font = new System.Drawing.Font("Franklin Gothic Heavy", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbl_agenda.Location = new System.Drawing.Point(50, 10);
            this.lbl_agenda.Name = "lbl_agenda";
            this.lbl_agenda.Size = new System.Drawing.Size(105, 30);
            this.lbl_agenda.TabIndex = 1;
            this.lbl_agenda.Text = "AGENDA";
            // 
            // pbAgenda
            // 
            this.pbAgenda.Image = global::Agenda.Properties.Resources.agenda;
            this.pbAgenda.Location = new System.Drawing.Point(-1, 43);
            this.pbAgenda.Name = "pbAgenda";
            this.pbAgenda.Size = new System.Drawing.Size(225, 228);
            this.pbAgenda.TabIndex = 0;
            this.pbAgenda.TabStop = false;
            // 
            // Main
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1120, 639);
            this.Controls.Add(this.pnlAgenda);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "Main";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Menu";
            this.pnlAgenda.ResumeLayout(false);
            this.pnlAgenda.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pbAgenda)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel pnlAgenda;
        private System.Windows.Forms.PictureBox pbAgenda;
        private System.Windows.Forms.Label lbl_agenda;
    }
}

