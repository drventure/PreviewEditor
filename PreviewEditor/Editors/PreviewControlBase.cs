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

        protected virtual void Find() { }
        protected virtual void Replace() { }

        protected virtual void ChooseFont() { }
        protected virtual void ChooseTheme() { }
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

                new ToolStripMenuItem("Options", null, new ToolStripMenuItem[] {
                    new ToolStripMenuItem("Font...", null, (sender, e) =>
                        {
                            this.ChooseFont();
                        }),
                    new ToolStripMenuItem("Theme...", null, (sender, e) =>
                        {
                            this.ChooseTheme();
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
