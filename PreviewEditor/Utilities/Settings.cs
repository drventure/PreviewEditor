using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;


namespace PreviewEditor
{
    /// <summary>
    /// Used to persist config to a local JSON file
    /// </summary>
    internal class Settings : OptionsBase
    {
        /// <summary>
        /// 
        /// </summary>
        public _TextEditorOptions TextEditorOptions 
        { 
            get { return this.GetProperty<_TextEditorOptions>(new _TextEditorOptions(this)); } 
            set { this.SetProperty(value); } 
        }
        internal class _TextEditorOptions : OptionsBase
        {
            public _TextEditorOptions(OptionsBase parent) : base(parent) { }
            public bool ShowLineNumbers { get { return this.GetProperty<bool>(true); } set { this.SetProperty(value); } }
        }


        public _HexEditorOptions HexEditorOptions 
        { 
            get { return this.GetProperty<_HexEditorOptions>(new _HexEditorOptions(this)); } 
            set { this.SetProperty(value); } 
        }
        internal class _HexEditorOptions : OptionsBase
        {
            public _HexEditorOptions(OptionsBase parent) : base(parent) { }
            public bool ShowLineNumbers { get { return this.GetProperty<bool>(true); } set { this.SetProperty(value); } }
        }


        public Settings(OptionsBase parent) : base(parent) { }
        public Settings()
        {
            this.SettingsFile = "PreviewEditor.config";
            this.Loading = true;
            this.Load();
            this.Loading = false;
        }
    }


    internal class OptionsBase
    {
        protected OptionsBase _parent = null;

        public OptionsBase() {}

        public OptionsBase(OptionsBase parent)
        {
            _parent = parent;
        }


        protected Dictionary<string, object> _props = new Dictionary<string, object>();
        public void SetProperty(object value, [CallerMemberName] string name = null)
        {
            _props[name] = value;
            if (!this.Loading) this.Save();
        }


        public T GetProperty<T>(T def = default(T), [CallerMemberName] string name = null)
        {
            if (_props.ContainsKey(name))
            {
                if (_props[name] is T v) return v;
            }
            _props[name] = def;
            return def;
        }


        private bool _loading = false;  
        protected bool Loading
        {
            get
            {
                if (_parent is not null) return _parent.Loading;
                return _loading;
            }
            set
            {
                _loading = value;
            }
        }


        internal void Load()
        {
            if (_parent is not null) _parent.Load();

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
            if (_parent is not null) _parent.Save();

            JsonConvert.DefaultSettings = () => new JsonSerializerSettings() { Formatting = Formatting.Indented };
            try
            {
                var buf = JsonConvert.SerializeObject(this);
                File.WriteAllText(this.SettingsFile, buf);
            }
            catch (Exception ex)
            {
                _ = ex;
            }
        }


        private string _filename = null;
        protected string SettingsFile
        {
            get
            {
                return _filename;
            }
            set
            {
                if (value == null) throw new ArgumentException("Filename can't be null");

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
                _filename = Path.Combine(basedir, value);
            }
        }
    }
}
