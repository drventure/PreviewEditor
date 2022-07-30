using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace PreviewEditor
{
    internal static class PreviewEditor
    {
        private static Options _settings = null;

        public static Options Settings
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
    }
}
