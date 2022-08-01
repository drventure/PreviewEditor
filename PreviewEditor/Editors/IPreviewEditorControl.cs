using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PreviewEditor.Editors
{
    delegate void SwitchEditorRequestedEventHandler(object sender, SwitchEditorRequestedEventArgs e);


    internal class SwitchEditorRequestedEventArgs : EventArgs
    {
        internal SwitchEditorRequestedEventArgs(EditingFile editingFile)
        {
            this.EditingFile = editingFile;
        }

        public EditingFile EditingFile { get; private set; }    
    }


    internal interface IPreviewEditorControl
    {
        /// <summary>
        /// Close any loaded file/resources
        /// </summary>
        void Close();


        /// <summary>
        /// Returns the "other" kind of editor, either Hex or text
        /// </summary>
        /// <returns></returns>
        event SwitchEditorRequestedEventHandler SwitchEditorRequested;
    }
}
