using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls.Primitives;
using System.Windows.Forms;
using System.Windows.Forms.Integration;
using System.Windows.Media;
using System.Windows.Threading;

using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Folding;
using ICSharpCode.AvalonEdit.Highlighting;

namespace PreviewEditor.Editors.TextControls
{
    /// <summary>
    /// This control views text files of arbitrary size.
    /// it comes into play when a file is deemed too big for the text viewer to handle it
    /// yet it's still considered a text file (as opposed to a binary file)
    /// </summary>
    internal class TextViewControl : TextControlBase
    {
        private FileWindow _window;
        private VScrollBar _vscroll;

        public TextViewControl(EditingFile file) : base(file)
        {
        }


        /// <summary>
        /// parameterless constructor for internal debugging only
        /// Should not normally be called
        /// </summary>
        public TextViewControl() : this(null)
        {
        }


        protected override void OnParentChanged()
        {
            //init the control once it's sited
            _editor.IsReadOnly = true;
            //we're going to use a custom vertical scrollbar
            _editor.VerticalScrollBarVisibility = System.Windows.Controls.ScrollBarVisibility.Hidden;
            //not sure how to support line numbers in View Only mode
            //for now, no line nums in view only mode
            _editor.ShowLineNumbers = false;

            //setup the vertical scrollbar
            _vscroll = new VScrollBar();
            _vscroll.Dock = DockStyle.Right;
            this.Controls.Add(_vscroll);

            //hook up the filewindow handler
            _window = new FileWindow(_file, _editor, _vscroll);


            try
            {
                _editor.Background = new SolidColorBrush(Color.FromRgb(30, 0, 0));
                _window.JumpToHome();
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


        private void SetDarkMode()
        {
            //foreach (var h in _editor.SyntaxHighlighting.NamedHighlightingColors)
            //{
            //    var f = h.Foreground;
            //    var b = h.Background;
            //}
        }
    }
}
