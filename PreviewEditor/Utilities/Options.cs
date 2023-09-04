using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
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

            [OptionDescription("Column ruler position")]
            public int ColumnRulerPosition { get { return this.GetProperty<int>(80); } set { this.SetProperty(value); } }

            [OptionDescription("Display Space characters")]
            public bool ShowSpaces { get { return this.GetProperty<bool>(true); } set { this.SetProperty(value); } }

            [OptionDescription("Display Tab characters")]
            public bool ShowTabs { get { return this.GetProperty<bool>(true); } set { this.SetProperty(value); } }

            [OptionDescription("Maximum Editable FileSize in bytes")]
            public long MaxEditableFileSize { get { return this.GetProperty<long>(5 * 1000 * 1000); } set { this.SetProperty(value); } }

            [OptionDescription("Find only within the current selection")]
            public bool FindInSelection { get { return this.GetProperty<bool>(false); } set { this.SetProperty(value); } }

            [OptionDescription("Find Whole Words Only")]
            public bool FindWholeWordsOnly { get { return this.GetProperty<bool>(false); } set { this.SetProperty(value); } }

            [OptionDescription("Find Using Regular Expressions")]
            public bool FindWithRegex { get { return this.GetProperty<bool>(false); } set { this.SetProperty(value); } }

            [OptionDescription("Find Case Sensitive")]
            public bool FindCaseSensitive { get { return this.GetProperty<bool>(false); } set { this.SetProperty(value); } }

            [OptionDescription("Font Name to use")]
            public string FontFamily { get { return this.GetProperty<string>("Consolas"); } set { this.SetProperty(value); } }

            [OptionDescription("Font Size to use")]
            public float FontSize { get { return this.GetProperty<float>(8); } set { this.SetProperty(value); } }

            /// <summary>
            /// 
            /// </summary>
            [OptionDescription("Various Syntax Coloring Options for the Preview Text Editor")]
            public _TextEditorColors Colors
            {
                get { return this.GetProperty<_TextEditorColors>(new _TextEditorColors(this)); }
                set { this.SetProperty(value); }
            }
            internal class _TextEditorColors : NestedOptionsBase
            {
                public _TextEditorColors(NestedOptionsBase parent) : base(parent) { }

                [OptionDescription("Editor Foreground")]
                public Color Forecolor { get { return this.GetProperty<Color>(System.Windows.Media.Colors.White); } set { this.SetProperty(value); } }
                [OptionDescription("Editor Background")]
                public Color Backcolor { get { return this.GetProperty<Color>(System.Windows.Media.Colors.Black); } set { this.SetProperty(value); } }
                [OptionDescription("Comments")]
                public Color Comments { get { return this.GetProperty<Color>(System.Windows.Media.Colors.DarkCyan); } set { this.SetProperty(value); } }
                [OptionDescription("Variables")]
                public Color Variables { get { return this.GetProperty<Color>(System.Windows.Media.Colors.DarkGoldenrod); } set { this.SetProperty(value); } }
                [OptionDescription("Namespaces")]
                public Color Namespaces { get { return this.GetProperty<Color>(System.Windows.Media.Colors.DarkGoldenrod); } set { this.SetProperty(value); } }
                [OptionDescription("Keywords")]
                public Color Keywords { get { return this.GetProperty<Color>(System.Windows.Media.Colors.DarkGoldenrod); } set { this.SetProperty(value); } }
                [OptionDescription("Goto Keywords")]
                public Color GotoKeywords { get { return this.GetProperty<Color>(System.Windows.Media.Colors.DarkGoldenrod); } set { this.SetProperty(value); } }
                [OptionDescription("Types")]
                public Color Types { get { return this.GetProperty<Color>(System.Windows.Media.Colors.DarkCyan); } set { this.SetProperty(value); } }
                [OptionDescription("Strings")]
                public Color Strings { get { return this.GetProperty<Color>(System.Windows.Media.Colors.Goldenrod); } set { this.SetProperty(value); } }
                [OptionDescription("Punctuation")]
                public Color Punctuation { get { return this.GetProperty<Color>(System.Windows.Media.Colors.DarkGray); } set { this.SetProperty(value); } }
                [OptionDescription("Operators")]
                public Color Operators { get { return this.GetProperty<Color>(System.Windows.Media.Colors.DarkMagenta); } set { this.SetProperty(value); } }
                [OptionDescription("Visibility")]
                public Color Visibility { get { return this.GetProperty<Color>(System.Windows.Media.Colors.DarkMagenta); } set { this.SetProperty(value); } }
                [OptionDescription("Functions")]
                public Color Functions { get { return this.GetProperty<Color>(System.Windows.Media.Colors.CornflowerBlue); } set { this.SetProperty(value); } }
                [OptionDescription("Integers")]
                public Color Integers { get { return this.GetProperty<Color>(System.Windows.Media.Colors.Goldenrod); } set { this.SetProperty(value); } }
            }
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

            [OptionDescription("Maximum Editable FileSize in bytes")]
            public long MaxEditableFileSize { get { return this.GetProperty<long>(50 * 1000 * 1000); } set { this.SetProperty(value); } }

            [OptionDescription("Show the StatusBar on the Hex Editor (or not)")]
            public bool ShowStatusBar{ get { return this.GetProperty<bool>(true); } set { this.SetProperty(value); } }

            [OptionDescription("Font Name to use")]
            public string FontFamily { get { return this.GetProperty<string>("Consolas"); } set { this.SetProperty(value); } }

            [OptionDescription("Font Size to use")]
            public float FontSize { get { return this.GetProperty<float>(8); } set { this.SetProperty(value); } }

        }
    }
}
