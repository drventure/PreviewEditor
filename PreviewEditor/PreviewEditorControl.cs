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
using System.Windows.Forms.Integration;

using Microsoft.Win32;

using ICSharpCode.AvalonEdit;
using PreviewHandler.Sdk.Controls;
using PreviewEditor.Editors;
using PreviewEditor.Editors.TextControls;


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

        private EditingFile _file;
        private Panel pnlEditor;


        public PreviewEditorControl()
        {
            InitializeComponent();
            this.TopLevel = false;

            pnlEditor = new Panel();
            pnlEditor.Dock = DockStyle.Fill;

            this.SetBackgroundColor(this.DefaultBackColor);
            this.SetTextColor(this.DefaultForeColor);

            InitializeLoadingScreen();

            this.Controls.Add(pnlEditor);
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
            this.SuspendLayout();
            // 
            // PreviewEditorHandlerControl
            // 
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
                    if (this.pnlEditor.Controls.Count == 1)
                    {
                        var editor = this.pnlEditor.Controls[0] as IPreviewEditorControl;
                        if (editor != null)
                        {
                            if (editor != null)
                            {
                                editor.Close();
                            }
                        }
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
                string filename = null;
                _file = null;

                if (dataSource is string stringVal)
                {
                    if (File.Exists(stringVal))
                    {
                        filename = stringVal;
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
                _file = new EditingFile(filename);

                this.InvokeOnControlThread(() =>
                {
                    try
                    {
                        var editor = EditorFactory.GetEditor(_file);

                        //set the editor into the parent window
                        //NOTE the datasource argument is not used
                        SiteEditor(string.Empty, editor);

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
                    ShowStatus("Unable to preview this file.");
                });
            }
        }


        private void SiteEditor<T>(T dataSource, IPreviewEditorControl editor)
        {
            var editorControl = (Control)editor;
            editorControl.Dock = DockStyle.Fill;
            editorControl.Visible = true;

            //monitor for switch editor request
            editor.SwitchEditorRequested += Editor_SwitchEditorRequested;

            //call the base class to finish out
            base.DoPreview(dataSource);

            this.pnlEditor.Controls.Clear();
            this.pnlEditor.Controls.Add(editorControl);
        }


        private void Editor_SwitchEditorRequested(object sender, SwitchEditorRequestedEventArgs e)
        {
            IPreviewEditorControl newEditor = null;

            if (sender is TextEditControl tec)
            {
                //Switching to hex editor
                newEditor = EditorFactory.GetHexEditor(e.EditingFile);
            }
            else if (sender is TextViewControl tvc)
            {
                //Switching to hex editor
                newEditor = EditorFactory.GetHexEditor(e.EditingFile);
            }
            else if (sender is WPFHexEditControl hec)
            {
                //Switching to Text editor
                //will automatically determine whether to use a viewer or editor
                newEditor = EditorFactory.GetTextEditor(e.EditingFile);
            }

            if (newEditor is not null)
            {
                //the dataSource is irrelevant here
                SiteEditor(string.Empty, newEditor);
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
        public override void SetTextColor(Color color)
        {
            InvokeOnControlThread(() => 
            {
                base.SetTextColor(color);
                pnlEditor.ForeColor = color;
            });
        }


        /// <summary>
        /// hook into color setter
        /// </summary>
        /// <param name="argbColor"></param>
        public override void SetBackgroundColor(Color color)
        {
            InvokeOnControlThread(() => 
            {
                base.SetBackgroundColor(color);
                pnlEditor.BackColor = color;
            });
        }


        /// <summary>
        /// Give the user a nice loading message
        /// </summary>
        private void InitializeLoadingScreen()
        {
            InvokeOnControlThread(() =>
            {

                var loading = new Label();
                loading.Dock = DockStyle.Fill;
                loading.Text = "Loading...";
                loading.TextAlign = ContentAlignment.MiddleCenter;
                loading.AutoSize = false;
                loading.Font = new Font("MS Sans Serif", 16, FontStyle.Bold);
                loading.ForeColor = PreviewEditor.Settings.TextEditorOptions.Colors.Forecolor.ToDrawColor();
                loading.BackColor = PreviewEditor.Settings.TextEditorOptions.Colors.Backcolor.ToDrawColor();

                this.pnlEditor.Controls.Add(loading);
                this.pnlEditor.Refresh();
            });
        }


        private Timer _statusMsgTimer;
        private void ShowStatus(string message)
        {
            var label = this.pnlEditor.Controls[0] as Label;
            if (label != null)
            {
                this.InvokeOnControlThread(() =>
                {
                    label.Text = message;

                    _statusMsgTimer = new Timer();
                    _statusMsgTimer.Interval = 3000;
                    _statusMsgTimer.Tick += (sender, e) =>
                    {
                        HideStatus();
                    };
                    _statusMsgTimer.Start();
                });
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


        private bool IsDarkModeActive
        {
            get
            {
                try
                {
                    var res = Registry.GetValue("HKEY_CURRENT_USER\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Themes\\Personalize", "AppsUseLightTheme", -1) as int?;
                    return (!res.HasValue || res.Value == 0);
                }
                catch
                {
                    return false;
                }
            }
        }


        internal new Color DefaultForeColor
        {
            get
            {
                if (this.IsDarkModeActive) return Color.LightGray;
                return Color.FromArgb(0x1a, 0x1a, 0x1a);
            }
        }


        internal new Color DefaultBackColor
        {
            get
            {
                if (this.IsDarkModeActive) return Color.FromArgb(0x1a, 0x1a, 0x1a);
                return Color.LightGray;
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
