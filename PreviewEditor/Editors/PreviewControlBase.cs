using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.Integration;


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
        #endregion


        #region Protected
        protected EditingFile _file;
        protected virtual string AlternateViewName { get; }
        #endregion


        #region public 
        public virtual void Close() { }
        #endregion
        
        
        /// <summary>
        /// Build up the ContextMenu
        /// </summary>
        public virtual new ContextMenuStrip ContextMenu
        {
            get
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

                    new ToolStripMenuItem("Find", null, (sender, e) =>
                    {
                        //Find();
                    }, Keys.Control | Keys.F),

                    new ToolStripMenuItem("Replace", null, (sender, e) =>
                    {
                        //Replace();
                    }, Keys.Control | Keys.H),

                    new ToolStripSeparator(),

                    new ToolStripMenuItem("Options", null, new ToolStripMenuItem[] {
                        new ToolStripMenuItem("Show Line Numbers", null, (sender, e) =>
                            {
                                //_editor.ShowLineNumbers = !_editor.ShowLineNumbers;
                                //PreviewEditor.Settings.TextEditorOptions.ShowLineNumbers = _editor.ShowLineNumbers;
                            }, Keys.Control | Keys.Shift | Keys.L)
                            {
                                //Checked = _editor.ShowLineNumbers,
                                //Enabled = _file.IsTextLoadable
                            },
                        new ToolStripMenuItem("Show Column Ruler", null, (sender, e) =>
                            {
                                //_editor.Options.ShowColumnRuler = !_editor.Options.ShowColumnRuler;
                                //_editor.Options.ColumnRulerPosition = 80;
                                //PreviewEditor.Settings.TextEditorOptions.ShowColumnRuler = _editor.Options.ShowColumnRuler;
                            }, Keys.Control | Keys.Shift | Keys.C)
                            {
                                //Checked = _editor.Options.ShowColumnRuler
                            },
                        //new ToolStripMenuItem("Set Column Ruler", null, SetColumnRuler),
                        new ToolStripMenuItem("Show Spaces", null, (sender, e) =>
                            {
                                //_editor.Options.ShowSpaces = !_editor.Options.ShowSpaces;
                                //PreviewEditor.Settings.TextEditorOptions.ShowSpaces = _editor.Options.ShowSpaces;
                            })
                            {
                                //Checked = _editor.Options.ShowSpaces
                            },
                        new ToolStripMenuItem("Show Tabs", null, (sender, e) =>
                            {
                                //_editor.Options.ShowTabs = !_editor.Options.ShowTabs;
                                //PreviewEditor.Settings.TextEditorOptions.ShowTabs = _editor.Options.ShowTabs;
                            })
                            {
                                //Checked = _editor.Options.ShowTabs
                            },
                        new ToolStripMenuItem("Font...", null, (sender, e) =>
                            {
                                //this.ChooseFont();
                            }),
                        new ToolStripMenuItem("Theme...", null, (sender, e) =>
                            {
                                //this.ChooseTheme();
                            })
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
        }
    }
}
