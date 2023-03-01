namespace BarCodeSplitter
{
    partial class frmMain
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
            this.pnlTop = new System.Windows.Forms.Panel();
            this.btnRun = new System.Windows.Forms.Button();
            this.lblInputPath = new System.Windows.Forms.Label();
            this.lblOutputTypes = new System.Windows.Forms.Label();
            this.cmbOutputType = new System.Windows.Forms.ComboBox();
            this.txtInput = new System.Windows.Forms.TextBox();
            this.btnLog = new System.Windows.Forms.Button();
            this.txtLog = new System.Windows.Forms.TextBox();
            this.pnlTop.SuspendLayout();
            this.SuspendLayout();
            // 
            // pnlTop
            // 
            this.pnlTop.Controls.Add(this.btnRun);
            this.pnlTop.Controls.Add(this.lblInputPath);
            this.pnlTop.Controls.Add(this.lblOutputTypes);
            this.pnlTop.Controls.Add(this.cmbOutputType);
            this.pnlTop.Controls.Add(this.txtInput);
            this.pnlTop.Controls.Add(this.btnLog);
            this.pnlTop.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlTop.Location = new System.Drawing.Point(0, 0);
            this.pnlTop.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.pnlTop.Name = "pnlTop";
            this.pnlTop.Size = new System.Drawing.Size(1403, 41);
            this.pnlTop.TabIndex = 6;
            // 
            // btnRun
            // 
            this.btnRun.Dock = System.Windows.Forms.DockStyle.Right;
            this.btnRun.Location = new System.Drawing.Point(1087, 0);
            this.btnRun.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.btnRun.Name = "btnRun";
            this.btnRun.Size = new System.Drawing.Size(158, 41);
            this.btnRun.TabIndex = 11;
            this.btnRun.Text = "&Executar";
            this.btnRun.UseVisualStyleBackColor = true;
            this.btnRun.Click += new System.EventHandler(this.btnRun_Click);
            // 
            // lblInputPath
            // 
            this.lblInputPath.AutoSize = true;
            this.lblInputPath.Location = new System.Drawing.Point(10, 9);
            this.lblInputPath.Name = "lblInputPath";
            this.lblInputPath.Size = new System.Drawing.Size(142, 20);
            this.lblInputPath.TabIndex = 10;
            this.lblInputPath.Text = "Diretório de leitura:";
            // 
            // lblOutputTypes
            // 
            this.lblOutputTypes.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblOutputTypes.AutoSize = true;
            this.lblOutputTypes.Location = new System.Drawing.Point(861, 9);
            this.lblOutputTypes.Name = "lblOutputTypes";
            this.lblOutputTypes.Size = new System.Drawing.Size(51, 20);
            this.lblOutputTypes.TabIndex = 9;
            this.lblOutputTypes.Text = "Tipos:";
            // 
            // cmbOutputType
            // 
            this.cmbOutputType.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.cmbOutputType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbOutputType.FormattingEnabled = true;
            this.cmbOutputType.Location = new System.Drawing.Point(918, 6);
            this.cmbOutputType.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.cmbOutputType.Name = "cmbOutputType";
            this.cmbOutputType.Size = new System.Drawing.Size(163, 28);
            this.cmbOutputType.Sorted = true;
            this.cmbOutputType.TabIndex = 8;
            // 
            // txtInput
            // 
            this.txtInput.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtInput.Location = new System.Drawing.Point(158, 6);
            this.txtInput.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.txtInput.Name = "txtInput";
            this.txtInput.ReadOnly = true;
            this.txtInput.Size = new System.Drawing.Size(653, 26);
            this.txtInput.TabIndex = 6;
            this.txtInput.Click += new System.EventHandler(this.txtInput_Click);
            // 
            // btnLog
            // 
            this.btnLog.Dock = System.Windows.Forms.DockStyle.Right;
            this.btnLog.Location = new System.Drawing.Point(1245, 0);
            this.btnLog.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.btnLog.Name = "btnLog";
            this.btnLog.Size = new System.Drawing.Size(158, 41);
            this.btnLog.TabIndex = 12;
            this.btnLog.Text = "&Processos";
            this.btnLog.UseVisualStyleBackColor = true;
            this.btnLog.Click += new System.EventHandler(this.btnLog_Click);
            // 
            // txtLog
            // 
            this.txtLog.BackColor = System.Drawing.Color.Black;
            this.txtLog.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtLog.Font = new System.Drawing.Font("Consolas", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtLog.ForeColor = System.Drawing.Color.Yellow;
            this.txtLog.Location = new System.Drawing.Point(0, 41);
            this.txtLog.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.txtLog.Multiline = true;
            this.txtLog.Name = "txtLog";
            this.txtLog.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.txtLog.Size = new System.Drawing.Size(1403, 607);
            this.txtLog.TabIndex = 7;
            this.txtLog.WordWrap = false;
            // 
            // frmMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1403, 648);
            this.Controls.Add(this.txtLog);
            this.Controls.Add(this.pnlTop);
            this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.MinimumSize = new System.Drawing.Size(1100, 600);
            this.Name = "frmMain";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "PDF Splitter - v1.13";
            this.pnlTop.ResumeLayout(false);
            this.pnlTop.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Panel pnlTop;
        private System.Windows.Forms.Button btnRun;
        private System.Windows.Forms.Label lblInputPath;
        private System.Windows.Forms.Label lblOutputTypes;
        private System.Windows.Forms.ComboBox cmbOutputType;
        private System.Windows.Forms.TextBox txtInput;
        private System.Windows.Forms.TextBox txtLog;
        private System.Windows.Forms.Button btnLog;
    }
}

