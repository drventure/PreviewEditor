using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;


namespace PreviewEditor
{
    /// <summary>
    /// Used to persist config to a local JSON file
    /// </summary>
    internal class Settings
    {
        private const string _settingsFilename = "PreviewEditor.config";

        /// <summary>
        /// 
        /// </summary>
        public string DefaultZone { get; set; }
        public string AuthCode { get; set; }


        internal Settings()
        {
            this.Load();

            if (this.AuthCode == null)
            {
                this.AuthCode = Guid.NewGuid().ToString().ToUpper();
                this.Save();
            }
        }


        private string _filename = null;
        private string SettingsFile
        {
            get
            {
                if (_filename == null)
                {

                    //standard dir for config, plus the temp folder fallback
                    var basedir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "PreviewEditor");
                    var tempdir = Path.GetTempPath();
                    try
                    {
                        if (!Directory.Exists(basedir))
                        {
                            Directory.CreateDirectory(basedir);
                        }
                    }
                    catch
                    {
                        basedir = tempdir;
                    }
                    _filename = Path.Combine(basedir, _settingsFilename);
                }

                return _filename;
            }
        }


        internal void Load()
        {
            JsonConvert.DefaultSettings = () => new JsonSerializerSettings() { Formatting = Formatting.Indented };
            try
            {
                if (File.Exists(this.SettingsFile))
                {
                    JsonConvert.PopulateObject(File.ReadAllText(this.SettingsFile), this);
                }
            }
            catch (Exception ex)
            {
                _ = ex;
            }
        }


        internal void Save()
        {
            JsonConvert.DefaultSettings = () => new JsonSerializerSettings() { Formatting = Formatting.Indented };
            try
            {
                File.WriteAllText(this.SettingsFile, JsonConvert.SerializeObject(this));
            }
            catch (Exception ex)
            {
                _ = ex;
            }
        }
    }
}
