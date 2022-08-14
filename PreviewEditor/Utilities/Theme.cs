using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace PreviewEditor
{
    /// <summary>
    /// Theme loading class
    /// Dynamically loads themes for VSCode etc
    /// </summary>
    internal static class Theme
    {
        internal static void Load(string file)
        {
            if (Path.GetExtension(file) == ".json")
            {
                LoadJson(file);
            }
        }


        internal static void LoadJson(string file)
        {
            var buf = File.ReadAllText(file);
            var settings = new JsonSerializerSettings();
            var theme = JsonConvert.DeserializeObject<ThemeVSCode>(buf, settings);
            if (theme.name != null && theme.tokenColors.Count > 0)
            {
                //most likely this IS a VSCode theme
                ApplyVSCodeTheme(theme);
            }

            //var options = new JsonSerializerOptions { WriteIndented = true };
            //string jsonString = JsonSerializer.Serialize(weatherForecast, options);
        }


        private static Color ToColor(string value)
        {
            return (Color)ColorConverter.ConvertFromString(value);
        }


        internal static void ApplyVSCodeTheme(ThemeVSCode theme)
        {
            var c = PreviewEditor.Settings.TextEditorOptions.Colors;
            ApplyColor(nameof(c.Forecolor), theme.colors.foreground);
            ApplyColor(nameof(c.Backcolor), theme.colors.background);
            ApplyNamedColor(theme, nameof(c.Comments), "comments");
            ApplyNamedColor(theme, nameof(c.Variables), "Variables");
            ApplyNamedColor(theme, nameof(c.Namespaces), "Namespaces");
            ApplyNamedColor(theme, nameof(c.Keywords), "Keywords");
            ApplyNamedColor(theme, nameof(c.GotoKeywords), "Keyword Control");
            ApplyNamedColor(theme, nameof(c.Types), "Type Name");
            ApplyNamedColor(theme, nameof(c.Strings), "Strings");
            ApplyNamedColor(theme, nameof(c.Punctuation), "Punctuation");
            ApplyNamedColor(theme, nameof(c.Operators), "keyword.operator");

        }


        private static void ApplyColor(string settingName, string color)
        {
            Color clr = default;
            if (color != null) clr = ToColor(color);
            if (clr != default)
            {
                var typ = PreviewEditor.Settings.TextEditorOptions.Colors.GetType();
                typ.GetProperty(settingName).SetValue(PreviewEditor.Settings.TextEditorOptions.Colors, clr);
            }
        }


        private static void ApplyNamedColor(ThemeVSCode theme, string settingName, string namedColor)
        {
            var color = theme.tokenColors.Where(t => t.name == namedColor).FirstOrDefault()?.settings.foreground;
            ApplyColor(settingName, color);
        }
    }


    class SingleOrArrayConverter<T> : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return (objectType == typeof(List<T>));
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            JToken token = JToken.Load(reader);
            if (token.Type == JTokenType.Array)
            {
                return token.ToObject<List<T>>();
            }
            return new List<T> { token.ToObject<T>() };
        }

        public override bool CanWrite
        {
            get { return false; }
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}
