using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PreviewEditor.Editors
{
    internal class SaveEventArgs : EventArgs
    {
        public SaveEventArgs() 
        {
            this.Prompt = false;
        }

        public bool Prompt { get; set; }
    }
}
