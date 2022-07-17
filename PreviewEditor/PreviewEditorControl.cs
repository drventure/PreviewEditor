using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using ICSharpCode.AvalonEdit;
using PreviewHandler.Sdk.Controls;


namespace PreviewEditor
{
    /// <summary>
    /// The Main PreviewEditorControl
    /// </summary>
    public class PreviewEditorControl
        //The Designer doesn't want to view controls based on Abstract classes
        //so wrap the abstract in a concrete class an inherit from it.
        //skip this in release mode because we don't have to use the designer in release mode
#if DEBUG
        : PreviewHandlerControlBaseWrapper
#elif RELEASE
        : PreviewHandlerControlBase
#endif
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        private FileInfo _fileInfo;
        private Panel pnlEditor;
        private TextBox tbxEditor;
        private System.Windows.Forms.Integration.ElementHost hexEditorHost;
        private System.Windows.Forms.Integration.ElementHost textEditorHost;

        private const int MAXFILESIZE = 2 * 1000 * 1000;


        public PreviewEditorControl()
        {
            InitializeComponent();

            pnlEditor = new Panel();
            this.Controls.Add(pnlEditor);

            pnlEditor.Dock = DockStyle.Fill;

            InitializeLoadingScreen();
        }


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


        private void InitializeComponent()
        {
            this.tbxEditor = new System.Windows.Forms.TextBox();
            this.hexEditorHost = new System.Windows.Forms.Integration.ElementHost();
            this.textEditorHost = new System.Windows.Forms.Integration.ElementHost();
            this.SuspendLayout();
            // 
            // tbxEditor
            // 
            this.tbxEditor.Location = new System.Drawing.Point(613, 30);
            this.tbxEditor.Multiline = true;
            this.tbxEditor.Name = "tbxEditor";
            this.tbxEditor.Size = new System.Drawing.Size(366, 209);
            this.tbxEditor.TabIndex = 0;
            // 
            // hexEditorHost
            // 
            this.hexEditorHost.Location = new System.Drawing.Point(29, 51);
            this.hexEditorHost.Name = "hexEditorHost";
            this.hexEditorHost.Size = new System.Drawing.Size(519, 188);
            this.hexEditorHost.TabIndex = 1;
            this.hexEditorHost.Text = "elementHost1";
            this.hexEditorHost.Child = null;
            // 
            // textEditorHost
            // 
            this.textEditorHost.Location = new System.Drawing.Point(1019, 51);
            this.textEditorHost.Name = "textEditorHost";
            this.textEditorHost.Size = new System.Drawing.Size(519, 188);
            this.textEditorHost.TabIndex = 2;
            this.textEditorHost.Text = "elementHost1";
            this.textEditorHost.Child = null;
            // 
            // PreviewEditorHandlerControl
            // 
            this.Controls.Add(this.textEditorHost);
            this.Controls.Add(this.hexEditorHost);
            this.Controls.Add(this.tbxEditor);
            this.Name = "PreviewEditorHandlerControl";
            this.Size = new System.Drawing.Size(1814, 643);
            this.ResumeLayout(false);
            this.PerformLayout();
        }


        public override void Unload()
        {
            Shutdown();
            base.Unload();
        }


        internal void Shutdown()
        {
            if (_statusMsgTimer != null)
            {
                _statusMsgTimer.Stop();
                _statusMsgTimer = null;
            }

            if (this.Handle != IntPtr.Zero)
            {
                this.InvokeOnControlThread(() =>
                {
                    //MessageBox.Show("Unloading");
                    if (hexEditorHost != null && hexEditorHost.Child != null)
                    {
                        ((WpfHexaEditor.HexEditor)hexEditorHost.Child).CloseProvider();
                        hexEditorHost.Child = null;
                    }
                    if (textEditorHost != null && textEditorHost.Child != null)
                    {
                        textEditorHost.Child = null;
                    }
                    this.Controls.Clear();
                });
            }
        }


        /// <summary>
        /// Main entry point. dataSource might be a filename or a stream
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dataSource"></param>
        public override void DoPreview<T>(T dataSource)
        {
            try
            {
                //MessageBox.Show($"Previewing");
                var buf = "";
                string filename = null;
                _fileInfo = null;

                if (dataSource is string stringVal)
                {
                    if (File.Exists(stringVal))
                    {
                        filename = stringVal;
                        buf = File.ReadAllText(stringVal);
                    }
                }
                else
                {
                    //technically this should never happen because 
                    //this is based on the FileBasedPreviewControl
                    ShowStatus("Filename not available, unable to load preview.");
                    return;
                }

                // at this point, we have the filename and we know the file exists
                _fileInfo = new FileInfo(filename);

                if (_fileInfo.Length > MAXFILESIZE)
                {
                    ShowStatus("The File is too big to preview.");
                    return;
                }

                System.Windows.Forms.MessageBox.Show($"this is null {this == null}");
                this.InvokeOnControlThread(() =>
                {
                    try
                    {
                        //MessageBox.Show($"Set text");
                        System.Windows.Forms.MessageBox.Show($"tbxEditor is null {tbxEditor == null}");
                        tbxEditor.Text = buf;

                        //MessageBox.Show($"Set Hex");
                        System.Windows.Forms.MessageBox.Show($"loading hex editor");
                        var hexEditor = new WpfHexaEditor.HexEditor();
                        System.Windows.Forms.MessageBox.Show($"hex editor is null {hexEditor == null}");
                        hexEditorHost.Child = hexEditor;
                        System.Windows.Forms.MessageBox.Show($"1");
                        hexEditor.Stream = new MemoryStream(Encoding.ASCII.GetBytes(buf));
                        System.Windows.Forms.MessageBox.Show($"2");

                        var editor = new TextEditor();
                        textEditorHost.Child = editor;
                        editor.Text = buf;

                        //call the base class to finish out
                        //MessageBox.Show($"Call base");
                        base.DoPreview(dataSource);
                        //MessageBox.Show($"Done calling base");

                        HideStatus();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error {ex.ToString()}");
                    }
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error {ex.ToString()}");

                //if any exception happens, we just have to eat it and show nothing
                this.InvokeOnControlThread(() =>
                {
                    tbxEditor.Visible = false;

                    var lbl = new Label();
                    lbl.Text = "File could not be loaded for preview";
                    this.Controls.Add(lbl);
                });
            }
        }


        /// <summary>
        /// Hook into font setter
        /// </summary>
        /// <param name="font"></param>
        public new void SetFont(Font font)
        {
            base.SetFont(font);
            pnlEditor.Font = font;
        }


        /// <summary>
        /// Hook into color setter
        /// </summary>
        /// <param name="color"></param>
        public new void SetTextColor(Color color)
        {
            base.SetTextColor(color);
            pnlEditor.ForeColor = color;
        }


        /// <summary>
        /// hook into color setter
        /// </summary>
        /// <param name="argbColor"></param>
        public new void SetBackgroundColor(Color color)
        {
            base.SetBackgroundColor(color);
            pnlEditor.BackColor = color;
        }


        /// <summary>
        /// Give the user a nice loading message
        /// </summary>
        private void InitializeLoadingScreen()
        {
            InvokeOnControlThread(() =>
            {

                var loading = new Label();
                this.pnlEditor.Controls.Add(loading);

                loading.Dock = DockStyle.Fill;
                loading.Text = "Loading...";
                loading.TextAlign = ContentAlignment.MiddleCenter;
                loading.AutoSize = false;
                loading.Font = new Font("MS Sans Serif", 16, FontStyle.Bold);
                loading.ForeColor = Color.White; // Settings.TextColor;
                loading.BackColor = Color.Black; // Settings.BackgroundColor;
            });
        }


        private Timer _statusMsgTimer;
        private void ShowStatus(string message)
        {
            var label = this.pnlEditor.Controls[0] as Label;
            if (label != null)
            {
                label.Text = message;

                _statusMsgTimer = new Timer();
                _statusMsgTimer.Interval = 3000;
                _statusMsgTimer.Tick += (sender, e) =>
                {
                    HideStatus();
                };
                _statusMsgTimer.Start();
            }
        }


        private void HideStatus()
        {
            var label = this.pnlEditor.Controls[0] as Label;
            if (label != null)
            {
                this.InvokeOnControlThread(() =>
                {
                    if (_statusMsgTimer != null)
                    {
                        _statusMsgTimer.Stop();
                        _statusMsgTimer = null;
                    }
                    label.Text = "";

                    if (this.pnlEditor.Controls.Contains(label))
                    {
                        this.pnlEditor.Controls.Remove(label);
                    }
                });
            }
        }


        public static unsafe MemoryStream ToMemoryStream(IStream comStream)
        {
            MemoryStream stream = new MemoryStream();
            byte[] pv = new byte[100];
            uint num = 0;

            IntPtr pcbRead = new IntPtr((void*)&num);

            do
            {
                num = 0;
                comStream.Read(pv, pv.Length, pcbRead);
                stream.Write(pv, 0, (int)num);
            }
            while (num > 0);
            return stream;
        }
    }


    public class PreviewHandlerControlBaseWrapper : PreviewHandlerControlBase
    { }
}
