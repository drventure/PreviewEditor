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
            this.pnlEditor = new System.Windows.Forms.Panel();
            this.SuspendLayout();
            // 
            // pnlEditor
            // 
            this.pnlEditor.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlEditor.Location = new System.Drawing.Point(0, 0);
            this.pnlEditor.Name = "pnlEditor";
            this.pnlEditor.Size = new System.Drawing.Size(1174, 699);
            this.pnlEditor.TabIndex = 0;
            // 
            // CodeEditPreviewHandlerControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(12F, 25F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.pnlEditor);
            this.Name = "CodeEditPreviewHandlerControl";
            this.Size = new System.Drawing.Size(1174, 699);
            this.ResumeLayout(false);

        }

        #endregion

        /// <summary>
        /// Loading label
        /// </summary>
        private Label _loading;

        private FastColoredTextBox _textbox;
        private HexBox _hexbox;

        private FileInfo _fileInfo;
        private Panel pnlEditor;
        private bool _isDirty = false;


        public CodeEditPreviewHandlerControl()
        {
            Log.Debug("Control: Constructor");
            InitializeComponent();
        }


        public void DoPreview(string filePath)
        {
            Log.Debug("Control: DoPreview");
            InvokeOnControlThread(() =>
            {
                // Starts loading screen
                Log.Debug("Init Loading screen");
                InitializeLoadingScreen();

                // Check if the file is too big.
                _fileInfo = new FileInfo(filePath);
                long fileSize = _fileInfo.Length;
                _isDirty = false;

                Log.Debug($"FileSize: {fileSize}");
                if (fileSize < 2 * 1000 * 1000)
                {
                    try
                    {
                        try
                        {
                            if (FileUtilities.IsTextFile(_fileInfo))
                            {
                                Log.Debug($"Using TextBox");
                                UseTextBox(_fileInfo);
                            }
                            else
                            {
                                Log.Debug($"Using HexBox");
                                UseHexBox(_fileInfo);
                            }
                        }
                        finally
                        {
                            this.pnlEditor.Controls.Remove(_loading);
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex);
                        _loading.Text = $"Error: {ex.Message}\r\n{ex.Source}\r\n{ex.StackTrace}";
                    }
                }
                else
                {
                    _loading.Text = "Max file size exceeded.";
                }
            });
        }


        private void GeneralKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.T && e.Control)
            {
                ToggleEditor();
            }
            else if (e.KeyCode == Keys.S && e.Control)
            {
                Save();
            }
        }


        /// <summary>
        /// Sets the color of the background, if possible, to coordinate with the windows
        /// color scheme.
        /// </summary>
        /// <param name="color">The color.</param>
        protected override void SetVisualsBackgroundColor(Color color)
        {
            this.pnlEditor.BackColor = color;
        }


        /// <summary>
        /// Sets the color of the text, if possible, to coordinate with the windows
        /// color scheme.
        /// </summary>
        /// <param name="color">The color.</param>
        protected override void SetVisualsTextColor(Color color)
        {
            this.pnlEditor.ForeColor = color;
        }


        /// <summary>
        /// Sets the font, if possible, to coordinate with the windows
        /// color scheme.
        /// </summary>
        /// <param name="font">The font.</param>
        protected override void SetVisualsFont(Font font)
        {
            this.pnlEditor.Font = font;
        }


        private void InitializeLoadingScreen()
        {
            InvokeOnControlThread(() =>
            {
                _loading = new Label();
                _loading.Dock = DockStyle.Fill;
                _loading.Text = "Loading...";
                _loading.TextAlign = ContentAlignment.MiddleCenter;
                _loading.AutoSize = false;
                _loading.Font = new Font("MS Sans Serif", 16, FontStyle.Bold);
                _loading.ForeColor = Color.White; // Settings.TextColor;
                _loading.BackColor = Color.Black; // Settings.BackgroundColor;
                this.pnlEditor.Controls.Add(_loading);
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


        private void UseTextBox(FileInfo filePath)
        {
            using (StreamReader fileReader = new StreamReader(new FileStream(filePath.FullName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)))
            {
                var buffer = fileReader.ReadToEnd();
                UseTextBox(buffer);
                fileReader.Close();
            }
        }


        private void UseTextBox(HexBox hexBox)
        {
            var pos = hexBox.SelectionStart;
            if (hexBox.ByteProvider is DynamicFileByteProvider)
            {
                if (hexBox.ByteProvider.Length > 2 * 1000 * 1000)
                {
                    //too long
                }
            }
            var bytes = new byte[(int)hexBox.ByteProvider.Length];
            for (var i = 0; i < hexBox.ByteProvider.Length; i++)
            {
                bytes[i] = hexBox.ByteProvider.ReadByte(i);
            }
            UseTextBox(Encoding.ASCII.GetString(bytes));
            _textbox.SelectionStart = (int)pos;
        }


        private void UseTextBox(string buffer)
        {
            _textbox = new FastColoredTextBox();
            this.pnlEditor.Controls.Clear();
            this.pnlEditor.Controls.Add(_textbox);
            _textbox.Dock = DockStyle.Fill;
            _textbox.Text = buffer;
            _textbox.Language = FastColoredTextBoxNS.Language.CSharp;
            _textbox.HighlightingRangeType = FastColoredTextBoxNS.HighlightingRangeType.VisibleRange;
            _textbox.SyntaxHighlighter.HighlightSyntax(_textbox.Language, _textbox.Range);

            _textbox.KeyDown += GeneralKeyDown;
            _textbox.TextChanged += (s, e) =>
            {
                _isDirty = true;
            };

            _textbox.Focus();
        }


        private void UseHexBox(FileInfo filePath)
        {
            using (StreamReader fileReader = new StreamReader(new FileStream(filePath.FullName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)))
            {
                var buffer = fileReader.ReadToEnd();
                UseHexBox(buffer);
                fileReader.Close();
            }
        }


        /// <summary>
        /// Switch from a Textbox to a hexbox
        /// </summary>
        /// <param name="fileInfo"></param>
        /// <param name="textBox"></param>
        private void UseHexBox(FastColoredTextBox textBox)
        {
            var pos = textBox.SelectionStart;
            UseHexBox(textBox.Text);
            _hexbox.SelectionStart = pos;
        }


        private void UseHexBox(string buffer)
        {
            _hexbox = new HexBox();
            var b = new Hb.Windows.Forms.DynamicByteProvider(Encoding.ASCII.GetBytes(buffer));
            //var b = new Hb.Windows.Forms.DynamicFileByteProvider(filePath);
            _hexbox.ByteProvider = b;
            _hexbox.LineInfoVisible = true;
            _hexbox.ColumnInfoVisible = true;
            _hexbox.StringViewVisible = true;
            _hexbox.VScrollBarVisible = true;
            this.pnlEditor.Controls.Clear();
            this.pnlEditor.Controls.Add(_hexbox);
            _hexbox.Dock = DockStyle.Fill;

            _hexbox.KeyDown += GeneralKeyDown;
            _hexbox.TextChanged += (s, e) =>
            {
                _isDirty = true;
            };

            _hexbox.Focus();
        }


        /// <summary>
        /// Switch between hex and text editors
        /// </summary>
        private void ToggleEditor()
        {
            if (this.IsHexEditing)
            {
                UseTextBox(_hexbox);
            }
            else
            {
                UseHexBox(_textbox);
            }
        }


        private bool IsTextEditing
        {
            get
            {
                return this.pnlEditor.Controls.Contains(_textbox);
            }
        }

        private bool IsHexEditing
        {
            get 
            {
                return !this.IsTextEditing;
            }
        }


        private void Save()
        {
            if (_isDirty)
            {
                if (MessageBox.Show("Save Changes?", "File has changed", MessageBoxButtons.OKCancel) == DialogResult.OK)
                {
                    try
                    {
                        if (this.IsTextEditing)
                        {
                            _textbox.SaveToFile(_fileInfo.FullName, Encoding.Default);
                        }
                        else
                        {
                            _hexbox.ByteProvider.ApplyChanges();    
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, "Error saving changes", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }
    }
}
