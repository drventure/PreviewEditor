using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PreviewEditorHost
{
    public class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        private Button btnSelect;
        private Panel pnlPreviewHost;

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
            this.btnSelect = new System.Windows.Forms.Button();
            this.pnlPreviewHost = new System.Windows.Forms.Panel();
            this.SuspendLayout();
            // 
            // btnSelect
            // 
            this.btnSelect.Location = new System.Drawing.Point(12, 12);
            this.btnSelect.Name = "btnSelect";
            this.btnSelect.Size = new System.Drawing.Size(172, 48);
            this.btnSelect.TabIndex = 0;
            this.btnSelect.Text = "Select File";
            this.btnSelect.UseVisualStyleBackColor = true;
            this.btnSelect.Click += new System.EventHandler(this.btnSelect_Click);
            // 
            // pnlPreviewHost
            // 
            this.pnlPreviewHost.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pnlPreviewHost.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlPreviewHost.Location = new System.Drawing.Point(199, 12);
            this.pnlPreviewHost.Name = "pnlPreviewHost";
            this.pnlPreviewHost.Size = new System.Drawing.Size(1242, 583);
            this.pnlPreviewHost.TabIndex = 1;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(12F, 25F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1453, 607);
            this.Controls.Add(this.pnlPreviewHost);
            this.Controls.Add(this.btnSelect);
            this.Name = "MainForm";
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private void MainForm_Load(object sender, EventArgs e)
        {
            //fileExplorer.PopulateView(@"C:\");
        }

        private void btnSelect_Click(object sender, EventArgs e)
        {
            var opn = new OpenFileDialog();
            opn.InitialDirectory = @"C:\Dev\Darin\CodePreviewEditor\PreviewEditor";
            var r = opn.ShowDialog();
            if (r == DialogResult.OK)
            {
                var file = Path.Combine(opn.InitialDirectory, opn.SafeFileName);

                var prv = new PreviewEditor.PreviewEditorControl();
                pnlPreviewHost.Controls.Clear();
                ((Form)prv).TopLevel = false;
                ((Form)prv).Parent = pnlPreviewHost;
                prv.Visible = true;
                prv.DoPreview<string>(file);
                prv.Dock = DockStyle.Fill;
            }
        }
    }
}
