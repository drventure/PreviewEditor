using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

using PreviewHandler.Sdk.Handlers;


namespace PreviewEditor
{
    internal static class Registration
    {
        [ComRegisterFunction]
        private static void Register(Type t) { PreviewHandlerBase.Register(t); }

        [ComUnregisterFunction]
        private static void Unregister(Type t) { PreviewHandlerBase.Unregister(t); }
    }
}
