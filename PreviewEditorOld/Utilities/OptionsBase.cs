using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;


namespace SimpleJSONOptions
{
    internal class NestedOptionsBase
    {
        protected NestedOptionsBase _parent = null;

        /// <summary>
        /// NestedOptions objects ALWAYS have a parent
        /// </summary>
        /// <param name="parent"></param>
        public NestedOptionsBase(NestedOptionsBase parent)
        {
            _parent = parent;
        }


        protected Dictionary<string, object> _props = new Dictionary<string, object>();
        public void SetProperty(object value, [CallerMemberName] string name = null)
        {
            _props[name] = value;
            this.Save();
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


        protected virtual bool IsLoading
        {
            get
            {
                return _parent.IsLoading;
            }
            set
            {
                throw new NotSupportedException("Setting the Loading property on a NestedOptions object is not allowed");
            }
        }


        internal virtual void Load()
        {
            _parent.Load();
        }


        internal virtual void Save()
        {
            _parent.Save();
        }
    }


    internal abstract class OptionsBase : NestedOptionsBase
    {
        /// <summary>
        /// Your concrete class that inherits from this base class
        /// must specify the options file name
        /// This should be just a filename, no path.
        /// The file will be stored in the %AppData% path
        /// 
        /// </summary>
        /// <param name="filename"></param>
        public OptionsBase(string applicationName, string filename) : base(null)
        {
            this.LoadHandler = DefaultLoadHandler;
            this.SaveHandler = DefaultSaveHandler;

            this.ApplicationName = applicationName;
            this.SettingsFile = filename;
            this.Load();
        }


        public OptionsBase(OptionsBase parent) : base(parent)
        {
        }


        private bool _isLoading = false;
        protected override bool IsLoading
        {
            get
            {
                return _isLoading;
            }
            set
            {
                _isLoading = value;
            }
        }


        internal delegate void LoadHandlerDelegate(string filename, OptionsBase options);

        private LoadHandlerDelegate _loadHandler;
        internal LoadHandlerDelegate LoadHandler
        {
            get { return _loadHandler; }
            set { _loadHandler = value; }
        }


        /// <summary>
        /// The default handler for loading settings
        /// This can be replaced by a custom delegate to use any other technique necessary to deserialize the Options object
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="options"></param>
        private void DefaultLoadHandler(string filename, OptionsBase options)
        {
            JsonConvert.DefaultSettings = () => new JsonSerializerSettings() { Formatting = Formatting.Indented };
            if (File.Exists(this.SettingsFile))
            {
                JsonConvert.PopulateObject(File.ReadAllText(this.SettingsFile), this);
            }
        }


        /// <summary>
        /// At the root options level, we actually load the options object
        /// Defer to the delegate to actually perform the load
        /// </summary>
        internal override void Load()
        {
            try
            {
                this.IsLoading = true;
                this.LoadHandler(this.SettingsFile, this);
            }
            catch { }
            finally
            {
                this.IsLoading = false;
            }
        }


        internal delegate void SaveHandlerDelegate(string filename, OptionsBase options);

        private SaveHandlerDelegate _saveHandler;
        internal SaveHandlerDelegate SaveHandler
        {
            get { return _saveHandler; }
            set { _saveHandler = value; }
        }


        /// <summary>
        /// The default handler for saving settings
        /// This can be replaced by a custom delegate to use any other technique necessary to serialize the Options object
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="options"></param>
        private void DefaultSaveHandler(string filename, OptionsBase options)
        {
            JsonConvert.DefaultSettings = () => new JsonSerializerSettings() { Formatting = Formatting.Indented };
            var buf = JsonConvert.SerializeObject(this);
            File.WriteAllText(this.SettingsFile, buf);
        }


        internal override void Save()
        {
            // we don't save while loading
            if (this.IsLoading) return;

            try
            {
                this.SaveHandler(this.SettingsFile, this);
            }
            catch
            {
                //just ignore exceptions here
            }
        }


        /// <summary>
        /// the name to use for the application portion of the settings filename
        /// </summary>
        private string ApplicationName { get; set; }


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
                var basedir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), this.ApplicationName);
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


    /// <summary>
    /// Description of an option
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    internal class OptionDescription : Attribute
    {
        public OptionDescription(string description)
        {
            this.Description = description;
        }

        public string Description { get; private set; }
    }
}
