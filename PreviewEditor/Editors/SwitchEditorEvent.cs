using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Channels;
using System.Text;
using System.Threading.Tasks;

namespace PreviewEditor.Editors
{
    internal class SwitchEditorRequestedEventArgs : EventArgs
    {
        internal SwitchEditorRequestedEventArgs(EditingFile editingFile)
        {
            this.EditingFile = editingFile;
        }

        public EditingFile EditingFile { get; private set; }
    }
}
