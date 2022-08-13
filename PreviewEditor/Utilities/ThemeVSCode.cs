using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PreviewEditor
{
    public class ThemeVSCode
    {
        public string name { get; set; }
        public string type { get; set; }
        public List<VSCodeTokenColor> tokenColors {get; set; }
        public VSCodeColors colors { get; set; }
    }


    public class VSCodeTokenColor
    {
        public string name { get; set; }
        [JsonConverter(typeof(SingleOrArrayConverter<string>))]
        public List<string> scope { get; set; }
        public VSCodeTokenColorSettings settings { get; set;} 
    }


    public class VSCodeTokenColorSettings
    {
        public string foreground { get; set; }
    }


    public class VSCodeColors
    {
        [JsonProperty("editor.foreground")]
        public string foreground { get; set; }
        [JsonProperty("editor.background")]
        public string background { get; set; }
    }
}
