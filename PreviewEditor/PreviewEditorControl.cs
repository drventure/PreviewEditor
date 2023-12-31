using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using Microsoft.Win32;

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
            //register to intercept assembly load calls so that we can redirect them to 
            //our embedded DLLs
            PreviewEditorHandler.SetupAssemblyInterceptor();

            //set logging if it hasn't already been set
            Log.Enabled = PreviewEditor.Settings.Logging;

            Log.Debug("PreviewEditorControl ctor");
            InitializeComponent();
            this.TopLevel = false;
            Log.Debug("Initialized Components");

            pnlEditor = new Panel();
            pnlEditor.Dock = DockStyle.Fill;
            Log.Debug("Setup editor panel");

            this.SetBackgroundColor(this.DefaultBackColor);
            this.SetTextColor(this.DefaultForeColor);
            Log.Debug("Set Default colors");

            InitializeLoadingScreen();
            Log.Debug("Setup Loading screen");

            this.Controls.Add(pnlEditor);
            Log.Debug("PreviewEditorControl ctor finished");
        }


        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            Log.Debug("PreviewEditorControl Dispose");
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
            Log.Debug("PreviewEditorControl Unload");
            Shutdown();
            Log.Debug("PreviewEditorControl Unload Shutdown");
            base.Unload();
            Log.Debug("PreviewEditorControl Unload Finished");
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
                            editor.Close();
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
                Log.Debug("PreviewEditorControl.DoPreview");
                string filename = null;
                _file = null;

                if (dataSource is string stringVal)
                {
                    if (File.Exists(stringVal))
                    {
                        Log.Debug($"PreviewEditorControl.DoPreview file '{stringVal}' will be loaded");
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
                Log.Debug($"PreviewEditorControl.DoPreview initialize EditingFile");
                _file = new EditingFile(filename);

                this.InvokeOnControlThread(() =>
                {
                    try
                    {
                        Log.Debug($"PreviewEditorControl.DoPreview getting editor for file");
                        var editor = EditorFactory.GetEditor(_file);

                        //set the editor into the parent window
                        //NOTE the datasource argument is not used
                        Log.Debug($"PreviewEditorControl.DoPreview siting editor");
                        SiteEditor(string.Empty, editor);

                        Log.Debug($"PreviewEditorControl.DoPreview hiding status");
                        HideStatus();

                        Log.Debug($"PreviewEditorControl.DoPreview done");
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
            Log.Debug($"PreviewEditorControl.SiteEditor");

            var editorControl = (Control)editor;
            editorControl.Dock = DockStyle.Fill;
            editorControl.Visible = true;

            //monitor for switch editor request
            editor.SwitchEditorRequested += Editor_SwitchEditorRequested;

            //call the base class to finish out
            Log.Debug($"PreviewEditorControl.SiteEditor calling DoPreview");
            base.DoPreview(dataSource);

            this.pnlEditor.Controls.Clear();

            Log.Debug($"PreviewEditorControl.SiteEditor adding editor to panel");
            this.pnlEditor.Controls.Add(editorControl);

            Log.Debug($"PreviewEditorControl.SiteEditor done");
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
            else if (sender is HexEditControl hec)
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
            Log.Debug($"PreviewEditorControl.InitializeLoadingScreen");
            this.InvokeOnControlThread(() =>
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
            Log.Debug($"PreviewEditorControl.InitializeLoadingScreen done");
        }


        private Timer _statusMsgTimer;
        private void ShowStatus(string message)
        {
            Log.Debug($"PreviewEditorControl.ShowStatus");
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
            Log.Debug($"PreviewEditorControl.ShowStatus done");
        }


        private void HideStatus()
        {
            Log.Debug($"PreviewEditorControl.HideStatus");
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
            Log.Debug($"PreviewEditorControl.HideStatus done");
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
                if (this.IsDarkModeActive) return Color.FromArgb(0x10, 0x10, 0x10);
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
