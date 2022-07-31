using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

using SimpleJSONOptions;


namespace PreviewEditor
{
    /// <summary>
    /// Used to persist config to a local JSON file
    /// </summary>
    internal class Options : OptionsBase
    {
        /// <summary>
        /// Options constructor
        /// You must provide the default filename (just the name, no path)
        /// and the ApplicationName to the base constructor
        /// </summary>
        public Options() : base("PreviewEditor", "PreviewEditor.config") { }


        /// <summary>
        /// 
        /// </summary>
        [OptionDescription("Various options related to the Preview Text Editor")]
        public _TextEditorOptions TextEditorOptions 
        { 
            get { return this.GetProperty<_TextEditorOptions>(new _TextEditorOptions(this)); } 
            set { this.SetProperty(value); }
        }
        internal class _TextEditorOptions : NestedOptionsBase
        {
            public _TextEditorOptions(OptionsBase parent) : base(parent) { }

            [OptionDescription("Display line numbers on the left side of the editor (or not)")]
            public bool ShowLineNumbers { get { return this.GetProperty<bool>(true); } set { this.SetProperty(value); } }

            [OptionDescription("Display a column ruler")]
            public bool ShowColumnRuler { get { return this.GetProperty<bool>(true); } set { this.SetProperty(value); } }

            [OptionDescription("Display Space characters")]
            public bool ShowSpaces { get { return this.GetProperty<bool>(true); } set { this.SetProperty(value); } }

            [OptionDescription("Display Tab characters")]
            public bool ShowTabs{ get { return this.GetProperty<bool>(true); } set { this.SetProperty(value); } }

            [OptionDescription("Maximum Editable FileSize in bytes")]
            public long MaxEditableFileSize { get { return this.GetProperty<long>(50 * 1000 * 1000); } set { this.SetProperty(value); } }
        }



        [OptionDescription("Various options related to the Preview Hex Editor")]
        public _HexEditorOptions HexEditorOptions 
        { 
            get { return this.GetProperty<_HexEditorOptions>(new _HexEditorOptions(this)); } 
            set { this.SetProperty(value); } 
        }
        internal class _HexEditorOptions : NestedOptionsBase
        {
            public _HexEditorOptions(OptionsBase parent) : base(parent) { }
            [OptionDescription("Display line numbers on the left side of the editor (or not)")]
            public bool ShowLineNumbers { get { return this.GetProperty<bool>(true); } set { this.SetProperty(value); } }
        }
    }
}
