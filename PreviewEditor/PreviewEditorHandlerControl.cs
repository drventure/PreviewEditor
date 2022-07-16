using System;
using System.Collections.Generic;
using System.ComponentModel;
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
    public class PreviewEditorHandlerControl : PreviewHandlerControlBaseWrapper
    {
        private TextBox tbxEditor;

        public PreviewEditorHandlerControl()
        {
            InitializeComponent();
        }

        private System.Windows.Forms.Integration.ElementHost hexEditorHost;
        private System.Windows.Forms.Integration.ElementHost textEditorHost;


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


        public override void DoPreview<T>(T dataSource) 
        {
            try
            {
                MessageBox.Show($"Previewing");
                var buf = "";
                var filename = "";

                if (dataSource is string stringVal)
                {
                    if (File.Exists(stringVal))
                    {
                        filename = stringVal;
                        buf = File.ReadAllText(stringVal);
                    }
                }
                else if (dataSource is IStream streamVal)
                {
                    var stream = ToMemoryStream(streamVal);
                    buf = Encoding.ASCII.GetString(stream.ToArray());
                    MessageBox.Show($"Previewing STREAM {streamVal}");
                }
                else
                {
                    MessageBox.Show($"Previewing {dataSource.GetType().Name}");
                    throw new ArgumentException($"{nameof(dataSource)} for {nameof(PreviewEditorHandlerControl)} must be a stream but was a '{typeof(T)}'");
                }

                this.InvokeOnControlThread(() =>
                {
                    try
                    {
                        MessageBox.Show($"Set text");
                        tbxEditor.Text = buf;

                        MessageBox.Show($"Set Hex");
                        var hexEditor = new WpfHexaEditor.HexEditor();
                        hexEditorHost.Child = hexEditor;
                        hexEditor.FileName = filename;

                        var editor = new TextEditor();
                        textEditorHost.Child = editor;
                        editor.Text = buf;

                        //call the base class to finish out
                        MessageBox.Show($"Call base");
                        base.DoPreview(dataSource);
                        MessageBox.Show($"Done calling base");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Exc {ex.ToString()}");
                    }
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Exc {ex.ToString()}");

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


    public class AbstractControlDescriptionProvider<TAbstract, TBase> : TypeDescriptionProvider
    {
        public AbstractControlDescriptionProvider()
            : base(TypeDescriptor.GetProvider(typeof(TAbstract)))
        {
        }

        public override Type GetReflectionType(Type objectType, object instance)
        {
            if (objectType == typeof(TAbstract))
                return typeof(TBase);

            return base.GetReflectionType(objectType, instance);
        }

        public override object CreateInstance(IServiceProvider provider, Type objectType, Type[] argTypes, object[] args)
        {
            if (objectType == typeof(TAbstract))
                objectType = typeof(TBase);

            return base.CreateInstance(provider, objectType, argTypes, args);
        }
    }


    /// <summary>
    /// Create a wrapper abstract class around the base class
    /// and inherit from it so we can use the designer
    /// </summary>
    [TypeDescriptionProvider(typeof(AbstractControlDescriptionProvider<PreviewHandlerControlBaseWrapper, UserControl>))]
    public abstract class PreviewHandlerControlBaseWrapper : PreviewHandlerControlBase
    { }
}
