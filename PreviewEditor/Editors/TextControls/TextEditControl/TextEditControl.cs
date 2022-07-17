using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms.Integration;
using System.Windows.Threading;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Folding;
using ICSharpCode.AvalonEdit.Highlighting;

namespace PreviewEditor.Editors
{
    internal class TextEditControl : TextControlBase
    {
        public TextEditControl(FileInfo fileInfo) : base(fileInfo)
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
                var buf = File.ReadAllText(_fileInfo.FullName);
                _editor.Text = buf;
            }
            catch (Exception ex)
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


        private void SetDarkMode()
        {
            return;
            foreach (var h in _editor.SyntaxHighlighting.NamedHighlightingColors)
            {
                var f = h.Foreground;
                var b = h.Background;
            }
        }


        public bool IsApplicable
        {
            get
            {
                if (_fileInfo.Length <= MAXEDITABLESIZE)
                {
                    if (EXTENSIONS.Contains(_fileInfo.Extension))
                    {
                        //we can be reasonably sure any of these are text files
                        return true;
                    }
                }

                //for now just return false otherwise
                return false;
            }
        }


        public void Close()
        {
            if (_editor != null)
            {
                _editor = null;
            }

            this.Child = null;
        }
    }
}
