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


namespace PreviewEditor.Editors
{
    internal class TextEditControl : TextControlBase
    {
        private bool _isDirty = false;


        public TextEditControl(EditingFile file) : base(file)
        {
            this.ParentChanged += OnParentChanged;
        }


        public TextEditControl() : this(null)
        {
        }


        private void OnParentChanged(object sender, EventArgs e)
        {
            try
            {
                var buf = File.ReadAllText(_file.FileInfo.FullName);
                _editor.Text = buf;

                //track whether document has been changed
                _isDirty = false;
                _editor.DocumentChanged += (sender, e) =>
                {
                    _isDirty = true;
                };
            }
            catch 
            {
                //TODO update the status label?
            }

            //once we've initialized, unhook the event
            this.ParentChanged -= OnParentChanged;
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


        /// <summary>
        /// Build up the ContextMenu
        /// </summary>
        public override ContextMenu ContextMenu
        {
            get
            {
                var menu = new ContextMenu();
                menu.MenuItems.Add(
                    new MenuItem("Undo", (sender, e) =>
                    {
                        _editor.Undo();
                    }) 
                    { 
                        Shortcut = Shortcut.CtrlZ, 
                        MergeOrder = 20,
                        Enabled = _editor.CanUndo
                    }
                );

                menu.MenuItems.Add(
                    new MenuItem("Redo", (sender, e) =>
                    {
                        _editor.Redo();
                    })
                    {
                        Shortcut = Shortcut.CtrlShiftZ,
                        MergeOrder = 20,
                        Enabled = _editor.CanRedo
                    }
                ); ;

                //merge menu items
                var baseMenu = base.ContextMenu;
                baseMenu.MergeMenu(menu);

                return baseMenu;
            }
        }


        public void Save()
        {
            if (_isDirty)
            {
                if (MessageBox.Show("Save Changes?", "File has changed", MessageBoxButtons.OKCancel) == DialogResult.OK)
                {
                    try
                    {
                        _editor.Save(_file.FileInfo.FullName);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, "Error saving changes", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            _isDirty = false;
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

        internal override void OnSwitchEditor()
        {
            _file.Stream = new MemoryStream();
            _editor.Save(_file.Stream);
            _file.Stream.Position = 0;
            base.OnSwitchEditor();
        }
    }
}
