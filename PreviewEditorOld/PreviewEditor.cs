using PreviewHandler.Sdk.Handlers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;


namespace PreviewEditor
{
    internal static class PreviewEditor
    {
        private static Options _settings = null;

        internal static Options Settings
        {
            get
            {
                if (_settings == null)
                {
                    _settings = new Options();
                }
                return _settings;
            }
        }


        [ComRegisterFunction]
        internal static void Register(Type t) { PreviewHandlerBase.Register(t); }


        [ComUnregisterFunction]
        internal static void Unregister(Type t) { PreviewHandlerBase.Unregister(t); }
    }
}
