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
        private TextPager _pager;
        private VScrollBar _vscroll;

        public TextViewControl(EditingFile file) : base(file)
        {
            _pager = new TextPager(file);
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
            _editor.ShowLineNumbers = false;
            _vscroll = new VScrollBar();
            _vscroll.Maximum = (int)_file.FileInfo.Length - 1;
            _vscroll.Dock = DockStyle.Right;
            _vscroll.Scroll += _vscroll_Scroll;
            this.Controls.Add(_vscroll);

            _editor.PreviewKeyDown += _editor_PreviewKeyDown;
            try
            {
                LoadPage(0);
                _editor.Background = new SolidColorBrush(Color.FromRgb(30,0,0));
            }
            catch 
            {
                //TODO update the status label?
            }
        }

        private void _editor_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.End)
            {
                _vscroll.Value = _vscroll.Maximum;
                LoadPage(_vscroll.Value);
                e.Handled = true;
            }
            if (e.Key == System.Windows.Input.Key.PageUp)
            {
                _vscroll.Value = _vscroll.Value - 1 * 1000 * 1000;
                LoadPage(_vscroll.Value);
                e.Handled = true;
            }
            if (e.Key == System.Windows.Input.Key.PageDown)
            {
                _vscroll.Value = _vscroll.Value + 1 * 1000 * 1000;
                LoadPage(_vscroll.Value);
                e.Handled = true;
            }
        }

        private void _editor_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
        }

        private void _vscroll_Scroll(object sender, System.Windows.Forms.ScrollEventArgs e)
        {
            var location = e.NewValue;
            LoadPage(location);    
        }


        private void LoadPage(int location)
        {
            var mstream = _pager.Page(location);
            mstream.Position = 0;
            _editor.Load(mstream);
            mstream.Close();
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


        internal override void OnSwitchEditor()
        {
            //we'll just pass the EditingFile on through with no stream
            base.OnSwitchEditor();
        }
    }
}
