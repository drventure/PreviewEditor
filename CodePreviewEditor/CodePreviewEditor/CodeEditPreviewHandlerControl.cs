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
using System.Windows.Forms.Integration;

using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Highlighting;
using SharpShell.SharpPreviewHandler;


namespace CodeEditPreviewHandler
{
    public class CodeEditPreviewHandlerControl : PreviewHandlerControl, IDisposable
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // CodeEditPreviewHandlerControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(12F, 25F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Name = "CodeEditPreviewHandlerControl";
            this.Size = new System.Drawing.Size(794, 355);
            this.Load += new System.EventHandler(this.CodeEditPreviewHandlerControl_Load);
            this.ResumeLayout(false);

        }

        #endregion

        /// <summary>
        /// Loading label
        /// </summary>
        private Label _loading;


        public CodeEditPreviewHandlerControl()
        {
            InitializeComponent();
        }


        public void DoPreview(string filePath)
        {
            MessageBox.Show("Before Doing Preview");
            InvokeOnControlThread(() =>
            {
                MessageBox.Show("Doing Preview");
                // Starts loading screen
                InitializeLoadingScreen();

                // Check if the file is too big.
                long fileSize = new FileInfo(filePath).Length;

                MessageBox.Show($"FileSize {fileSize}");
                if (fileSize < 2 * 1000 * 1000)
                {
                    try
                    {
                        MessageBox.Show($"Invoking");
                        try
                        {
                            MessageBox.Show($"Loading {filePath}");
                            string fileContent;
                            using (StreamReader fileReader = new StreamReader(new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)))
                            {
                                fileContent = fileReader.ReadToEnd();
                                fileReader.Close();
                            }

                            //var t = new FastColoredTextBoxNS.FastColoredTextBox();
                            //Controls.Add(t);
                            //t.Dock = DockStyle.Fill;
                            //t.Text = fileContent;
                            //t.Language = FastColoredTextBoxNS.Language.CSharp;
                            //t.HighlightingRangeType = FastColoredTextBoxNS.HighlightingRangeType.VisibleRange;
                            //t.SyntaxHighlighter.HighlightSyntax(t.Language, t.Range);

                            //var l = new Label();
                            //Controls.Add(l);
                            //l.Location = new Point(0, 0);
                            //l.AutoSize = true;
                            //l.Text = $"{filePath}";

                            var editorHost = new ElementHost();
                            var lbl = new System.Windows.Controls.TextBox();
                            lbl.Text = fileContent;
                            editorHost.Child = lbl;
                            var editor = new TextEditor();

                            editorHost.Child = editor;

                            editor.Text = fileContent;
                            editor.SyntaxHighlighting = HighlightingManager.Instance.GetDefinitionByExtension(Path.GetExtension(filePath));

                            this.Controls.Add(editorHost);
                            editorHost.BackColor = Color.Blue;
                            editorHost.Dock = DockStyle.Fill;
                        }
                        finally
                        {
                            Controls.Remove(_loading);
                        }
                    }
                    catch (Exception e)
                    {
                        Controls.Remove(_loading);
                        Label text = new Label();
                        text.Text = "Error";
                        text.Text += e.Message;
                        text.Text += "\n" + e.Source;
                        text.Text += "\n" + e.StackTrace;
                        text.Width = 500;
                        text.Height = 10000;
                        Controls.Add(text);
                    }
                }
                else
                {
                    Controls.Remove(_loading);
                    Label errorMessage = new Label();
                    errorMessage.Text = "Max Filesize Exceeded";
                    errorMessage.Width = 500;
                    errorMessage.Height = 50;
                    Controls.Add(errorMessage);
                }
            });
        }


        /// <summary>
        /// Sets the color of the background, if possible, to coordinate with the windows
        /// color scheme.
        /// </summary>
        /// <param name="color">The color.</param>
        protected override void SetVisualsBackgroundColor(Color color)
        {
            this.BackColor = color;
        }


        /// <summary>
        /// Sets the color of the text, if possible, to coordinate with the windows
        /// color scheme.
        /// </summary>
        /// <param name="color">The color.</param>
        protected override void SetVisualsTextColor(Color color)
        {
            //lblName.ForeColor = color;
        }


        /// <summary>
        /// Sets the font, if possible, to coordinate with the windows
        /// color scheme.
        /// </summary>
        /// <param name="font">The font.</param>
        protected override void SetVisualsFont(Font font)
        {
            this.Font = font;
        }


        private void CodeEditPreviewHandlerControl_Load(object sender, EventArgs e)
        {

        }


        private void InitializeLoadingScreen()
        {
            MessageBox.Show("Loading");
            InvokeOnControlThread(() =>
            {
                MessageBox.Show("Invoked");
                _loading = new Label();
                _loading.Text = "Loading";
                _loading.Width = this.Width;
                _loading.Height = this.Height;
                _loading.Font = new Font("MS Sans Serif", 16, FontStyle.Bold);
                _loading.ForeColor = Color.White; // Settings.TextColor;
                _loading.BackColor = Color.Black; // Settings.BackgroundColor;
                Controls.Add(_loading);
                MessageBox.Show("Done Loading");
            });
        }


        /// <summary>
        /// Executes the specified delegate on the thread that owns the control's underlying window handle.
        /// </summary>
        /// <param name="func">Delegate to run.</param>
        private void InvokeOnControlThread(Action func)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(func);
            }
            else
            {
                func();
            }

        }
    }
}
