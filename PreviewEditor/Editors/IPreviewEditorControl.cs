using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PreviewEditor.Editors
{
    internal interface IPreviewEditorControl
    {
        /// <summary>
        /// Returns the "other" kind of editor, either Hex or text
        /// </summary>
        /// <returns></returns>
        event EventHandler<SwitchEditorRequestedEventArgs> SwitchEditorRequested;


        void Close();
    }
}
