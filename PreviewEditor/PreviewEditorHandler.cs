using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

using PreviewHandler.Sdk;
using PreviewHandler.Sdk.Controls;
using PreviewHandler.Sdk.Handlers;


namespace PreviewEditor
{
    //use a semicolon delimited list of file extensions to handle
    [PreviewHandler("Preview Editor Handler", ".cs;.vb;.html;.htm;.log;.txt;.cshtml;.css;.csproj;.vbproj", "{C71E74A6-4C57-4297-90E3-A221F6EECF24}")]
    [ProgId("PreviewEditorHandler")]
    [Guid("D2A0858D-6E99-4A5A-87C9-2590A1B1B720")]
    [ClassInterface(ClassInterfaceType.None)]
    [ComVisible(true)]
    public sealed class PreviewEditorHandler : FileBasedPreviewHandler
    {
        PreviewEditorControl _control;

        public override void DoPreview()
        {
            _control.DoPreview(FilePath);
        }

        protected override IPreviewHandlerControl CreatePreviewHandlerControl()
        {
            _control = new PreviewEditorControl();
            return _control;
        }
    }
}
