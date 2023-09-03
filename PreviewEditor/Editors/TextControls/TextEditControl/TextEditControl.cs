using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.Integration;
using System.Windows.Input;
using System.Windows.Threading;

using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Folding;
using ICSharpCode.AvalonEdit.Highlighting;


namespace PreviewEditor.Editors.TextControls
{
    internal class TextEditControl : TextControlBase
    {
        private bool _isDirty = false;


        public TextEditControl(EditingFile file) : base(file)
        {
        }


        public TextEditControl() : this(null)
        {
        }


        protected override void OnParentChanged()
        {
            try
            {
                var buf = File.ReadAllText(_file.FileInfo.FullName);
                _editor.Text = buf;

                //track whether document has been changed
                _isDirty = false;
                _editor.TextChanged += (sender, e) =>
                {
                    _isDirty = true;
                };
            }
            catch
            {
                //TODO update the status label?
            }
        }


        private void SetFoldingStrategy()
        {
            if (_editor.SyntaxHighlighting == null)
            {
                _foldingStrategy = null;
            }
            else
            {
                switch (_editor.SyntaxHighlighting.Name)
                {
                    case "XML":
                        _foldingStrategy = new XmlFoldingStrategy();
                        _editor.TextArea.IndentationStrategy = new ICSharpCode.AvalonEdit.Indentation.DefaultIndentationStrategy();
                        break;
                    case "C#":
                    case "C++":
                    case "PHP":
                    case "Java":
                        _editor.TextArea.IndentationStrategy = new ICSharpCode.AvalonEdit.Indentation.CSharp.CSharpIndentationStrategy(_editor.Options);
                        _foldingStrategy = new BraceFoldingStrategy();
                        break;
                    default:
                        _editor.TextArea.IndentationStrategy = new ICSharpCode.AvalonEdit.Indentation.DefaultIndentationStrategy();
                        _foldingStrategy = null;
                        break;
                }
            }

            if (_foldingStrategy != null)
            {
                if (_foldingManager == null)
                {
                    _foldingManager = FoldingManager.Install(_editor.TextArea);
                }
                _foldingStrategy?.UpdateFoldings(_foldingManager, _editor.Document);

                //setup up a timer to refold periodically
                _foldingUpdateTimer = new DispatcherTimer();
                _foldingUpdateTimer.Interval = TimeSpan.FromSeconds(2);
                _foldingUpdateTimer.Tick += (sender, e) =>
                {
                    if (_foldingStrategy != null)
                    {
                        _foldingStrategy.UpdateFoldings(_foldingManager, _editor.Document);
                    }
                };
                _foldingUpdateTimer.Start();
            }
            else if (_foldingManager != null)
            {
                FoldingManager.Uninstall(_foldingManager);
                _foldingManager = null;
            }
        }


        /// <summary>
        /// Handle TextEdit specific keys
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected override void GeneralKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            var isCtrl = Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl);
            var isShift = Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.LeftShift);
            var isAlt = Keyboard.IsKeyDown(Key.LeftAlt) || Keyboard.IsKeyDown(Key.LeftAlt);
            if (isCtrl && !isShift && !isAlt)
            {
                if (e.Key == Key.S)
                {
                    this.Save();
                }
            }
            if (isShift && !isCtrl && !isAlt)
            {
                if (e.Key == Key.Z)
                {
                    _editor.Undo();
                }
            }
            if (isShift && isCtrl && !isAlt)
            {
                if (e.Key == Key.A)
                {
                    this.SaveAs();
                }
                else if (e.Key == Key.Z)
                {
                    _editor.Redo();
                }
            }

            base.GeneralKeyDown(sender, e);
        }


        protected override void Cut() { _editor.Cut(); }


        protected override void Copy() { _editor.Copy(); }


        protected override void Paste() { _editor.Paste(); }


        /// <summary>
        /// Build up the ContextMenu
        /// </summary>
        /*
        public override ContextMenuStrip ContextMenu
        {
            get
            {
                var menu = new ContextMenuStrip();
                menu.Items.AddRange(new ToolStripItem[] {
                    new ToolStripMenuItem("Undo", null, (sender, e) =>
                    {
                        _editor.Undo();
                    }, Keys.Control | Keys.Z)
                    {
                        MergeAction = MergeAction.Insert,
                        MergeIndex = 3,
                        Enabled = _editor.CanUndo
                    },

                    new ToolStripMenuItem("Redo", null, (sender, e) =>
                    {
                        _editor.Redo();
                    }, Keys.Control | Keys.Shift | Keys.Z)
                    {
                        MergeAction = MergeAction.Insert,
                        MergeIndex = 4,
                        Enabled = _editor.CanRedo
                    }
                }); 

                //merge menu items
                var baseMenu = base.ContextMenu;
                ToolStripManager.Merge(menu, baseMenu);

                return baseMenu;
            }
        }
        */


        protected override void Save(bool prompt = false)
        {
            var doSave = false;
            if (_isDirty && prompt)
            {
                if (MessageBox.Show("Save Changes?", "File has changed", MessageBoxButtons.OKCancel) == DialogResult.OK) doSave = true;
            }
            if (doSave)
            {
                try
                {
                    _editor.Save(_file.FileInfo.FullName);
                    _isDirty = false;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Error saving changes", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }


        public void SaveAs()
        {
            try
            {
                var dlg = new SaveFileDialog();
                dlg.InitialDirectory = _file.FileInfo.DirectoryName;
                dlg.FileName = _file.FileInfo.Name;
                dlg.ShowDialog();

                if (dlg.FileName != null)
                {
                    _editor.Save(dlg.FileName);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error saving changes", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        protected override void OnSwitchEditorRequested()
        {
            _file.Stream = new MemoryStream();
            _editor.Save(_file.Stream);
            _file.Stream.Position = 0;
            base.OnSwitchEditorRequested();
        }
    }
}
