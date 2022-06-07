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

//using ICSharpCode.AvalonEdit;
//using ICSharpCode.AvalonEdit.Highlighting;
using SharpShell.SharpPreviewHandler;
using FastColoredTextBoxNS;
using Hb.Windows.Forms;


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
        private FastColoredTextBox _textbox;
        private HexBox _hexbox;


        public CodeEditPreviewHandlerControl()
        {
            InitializeComponent();
        }


        public void DoPreview(string filePath)
        {
            InvokeOnControlThread(() =>
            {
                // Starts loading screen
                InitializeLoadingScreen();

                // Check if the file is too big.
                long fileSize = new FileInfo(filePath).Length;

                if (fileSize < 2 * 1000 * 1000)
                {
                    try
                    {
                        try
                        {
                            if (FileUtilities.IsTextFile(filePath))
                            {
                                string fileContent;
                                using (StreamReader fileReader = new StreamReader(new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)))
                                {
                                    fileContent = fileReader.ReadToEnd();
                                    fileReader.Close();
                                }
                                _textbox = new FastColoredTextBox();
                                Controls.Add(_textbox);
                                _textbox.Dock = DockStyle.Fill;
                                _textbox.Text = fileContent;
                                _textbox.Language = FastColoredTextBoxNS.Language.CSharp;
                                _textbox.HighlightingRangeType = FastColoredTextBoxNS.HighlightingRangeType.VisibleRange;
                                _textbox.SyntaxHighlighter.HighlightSyntax(_textbox.Language, _textbox.Range);
                            }
                            else
                            {
                                //treat as hex
                                _hexbox = new HexBox();
                                var b = new Hb.Windows.Forms.DynamicFileByteProvider(filePath);
                                _hexbox.ByteProvider = b;
                                _hexbox.LineInfoVisible = true;
                                _hexbox.ColumnInfoVisible = true;
                                _hexbox.StringViewVisible = true;
                                Controls.Add(_hexbox);
                                _hexbox.Dock = DockStyle.Fill;
                            }
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
                    _loading.Text = "Max file size exceeded.";
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
            InvokeOnControlThread(() =>
            {
                _loading = new Label();
                _loading.Text = "Loading";
                _loading.Width = this.Width;
                _loading.Height = this.Height;
                _loading.TextAlign = ContentAlignment.MiddleCenter;
                _loading.Font = new Font("MS Sans Serif", 16, FontStyle.Bold);
                _loading.ForeColor = Color.White; // Settings.TextColor;
                _loading.BackColor = Color.Black; // Settings.BackgroundColor;
                Controls.Add(_loading);
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
