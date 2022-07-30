using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace PreviewEditor
{
    internal static class PreviewEditor
    {
        private static Settings _settings = null;

        public static Settings Settings
        {
            get
            {
                if (_settings == null)
                {
                    _settings = new Settings();
                }
                return _settings;
            }
        }
    }
}
