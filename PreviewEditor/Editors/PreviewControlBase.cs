﻿using FontAwesome.Sharp;
using ICSharpCode.AvalonEdit.Highlighting;
using PreviewEditor.Utilities;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.Integration;
using System.Windows.Media;


namespace PreviewEditor.Editors
{
    internal class PreviewControlBase : UserControl, IPreviewEditorControl
    {
        #region Events
        public event EventHandler<SwitchEditorRequestedEventArgs> SwitchEditorRequested;
        protected virtual void OnSwitchEditorRequested()
        {
            SwitchEditorRequested.Invoke(this, new SwitchEditorRequestedEventArgs(_file));
        }
        #endregion


        protected ElementHost _host;

        #region protected
        protected virtual void Cut() { }
        protected virtual void Copy() { }
        protected virtual void Paste() { }

        protected virtual void Save(bool prompt = false) { }

        protected virtual void Find() { }
        protected virtual void Replace() { }

        protected virtual void ChooseFont() { }
        protected virtual void ChooseTheme() { }

        protected virtual void About() 
        {
            (new AboutBox()).ShowDialog();
        }
        #endregion


        #region Protected
        protected EditingFile _file;
        protected virtual string AlternateViewName { get; }
        #endregion


        #region public 
        public virtual void Close() { }
        #endregion


        public new ContextMenuStrip ContextMenu
        {
            get
            {
                return BuildContextMenu();
            }
        }


        /// <summary>
        /// Build up the ContextMenu
        /// </summary>
        protected virtual ContextMenuStrip BuildContextMenu()
        {
            var menu = new ContextMenuStrip();
            menu.Items.AddRange(new ToolStripItem[] {
                new ToolStripMenuItem("Cut", null, (sender, e) =>
                    {
                        Cut();
                    }, Keys.Control | Keys.X),

                new ToolStripMenuItem("Copy", null, (sender, e) =>
                    {
                        Copy();
                    }, Keys.Control | Keys.C),

                new ToolStripMenuItem("Paste", null, (sender, e) =>
                    {
                        Paste();
                    }, Keys.Control | Keys.V)
                    {
                        Enabled = System.Windows.Clipboard.ContainsText(),
                    },

                new ToolStripSeparator(),

                new ToolStripMenuItem("Find", null, (sender, e) =>
                    {
                        Find();
                    }, Keys.Control | Keys.F),

                new ToolStripMenuItem("Replace", null, (sender, e) =>
                    {
                        Replace();
                    }, Keys.Control | Keys.H),

                new ToolStripSeparator(),

                new ToolStripMenuItem("Options", null, new ToolStripItem[] {
                    new ToolStripMenuItem("Font...", null, (sender, e) =>
                        {
                            this.ChooseFont();
                        }),
                    new ToolStripMenuItem("Theme...", null, (sender, e) =>
                        {
                            this.ChooseTheme();
                        }),
                    new ToolStripMenuItem("Colors", null, new ToolStripItem[]
                        {
                            new ToolStripMenuItem("Link Text", MakeColorIcon(PreviewEditor.Settings.TextEditorOptions.Colors.Links), (sender, e) =>
                                {
                                    this.ChooseColor(nameof(PreviewEditor.Settings.TextEditorOptions.Colors.Links));
                                }),
                        }),
                    new ToolStripSeparator(),
                    new ToolStripMenuItem("Logging", null, (sender, e) =>
                        {
                            Log.Enabled = !Log.Enabled;
                            PreviewEditor.Settings.Logging = Log.Enabled;
                                    
                            if (Log.Enabled)
                            {
                                MessageBox.Show("PreviewEditor Logging is now enabled\r\n\r\n" +
                                    "With logging on, a PreviewEditor.log file will be written to\r\n" +
                                    "the current user's Desktop.\r\n\r\n" +
                                    "It is recommended to only turn on logging when asked to by\r\n" +
                                    "the PreviewEditor development team."
                                    , "PreviewEditor", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }

                            Log.Debug("PreviewEditor Logging Started");
                        })
                        {
                            Checked = Log.Enabled,
                            MergeAction = MergeAction.Insert,
                            MergeIndex = 1,
                        },                    
                    new ToolStripSeparator(),
                    new ToolStripMenuItem("About...", null, (sender, e) =>
                        {
                            this.About();
                        }),
                    }),

                new ToolStripSeparator(),

                new ToolStripMenuItem("Save", null, (sender, e) => { this.Save(true); }, Keys.Control | Keys.S),
                new ToolStripSeparator(),

                new ToolStripMenuItem($"Show in {this.AlternateViewName} mode", null, (sender, e) =>
                    {
                        OnSwitchEditorRequested();
                    })
            });

            return menu;
        }


        private Image MakeColorIcon(System.Windows.Media.Color color)
        {
            Bitmap result = new Bitmap(16, 16);
            using (Graphics g = Graphics.FromImage((Image)result))
            {
                g.FillRectangle(new System.Drawing.SolidBrush(color.ToDrawColor()), new Rectangle(0, 0, 16, 16));
            }
            return result;
        }

        private void ChooseColor(string propName)
        {
            var f = new Cyotek.Windows.Forms.ColorPickerDialog();
            f.Color = PreviewEditor.Settings.TextEditorOptions.Colors.GetProperty<System.Windows.Media.Color>(System.Windows.Media.Colors.Black, propName).ToDrawColor();
            if (f.ShowDialog() == DialogResult.OK)
            {
                var c = System.Windows.Media.Color.FromRgb(f.Color.R, f.Color.G, f.Color.B);    
                PreviewEditor.Settings.TextEditorOptions.Colors.SetProperty(c, propName);
            }
        }
    }
}
