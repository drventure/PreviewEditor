using System;
using System.Diagnostics;
using System.IO;
using System.Xml;

using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;


namespace PreviewEditor
{
	/// <summary>
	/// Override the syntax defs built into
	/// Avalon and use those built into the preview editor(or overridden via files)
	/// </summary>
	internal static class CustomHighlighting
	{
		//TODO Some way to may this hardwired string not hardwired
		static readonly string Prefix = "PreviewEditor.Editors.TextControls.SyntaxDefinitions.AsResources.";

		public static Stream OpenStream(string name)
		{
			Stream s = typeof(CustomHighlighting).Assembly.GetManifestResourceStream(Prefix + name);
			if (s == null)
				throw new FileNotFoundException("The resource file '" + name + "' was not found.");
			return s;
		}


		public static void Load()
        {
			//RegisterHighlighting("XmlDoc", null, "XmlDoc.xshd");
			RegisterHighlighting("C#", (".cs").Split(';'), "CSharp-Mode.xshd");

			//RegisterHighlighting("JavaScript", new[] { ".js" }, "JavaScript-Mode.xshd");
			//RegisterHighlighting("HTML", new[] { ".htm", ".html" }, "HTML-Mode.xshd");
			//RegisterHighlighting("ASP/XHTML", new[] { ".asp", ".aspx", ".asax", ".asmx", ".ascx", ".master" }, "ASPX.xshd");

			//RegisterHighlighting("Boo", new[] { ".boo" }, "Boo.xshd");
			//RegisterHighlighting("Coco", new[] { ".atg" }, "Coco-Mode.xshd");
			//RegisterHighlighting("CSS", new[] { ".css" }, "CSS-Mode.xshd");
			//RegisterHighlighting("C++", new[] { ".c", ".h", ".cc", ".cpp", ".hpp" }, "CPP-Mode.xshd");
			//RegisterHighlighting("Java", new[] { ".java" }, "Java-Mode.xshd");
			//RegisterHighlighting("Patch", new[] { ".patch", ".diff" }, "Patch-Mode.xshd");
			//RegisterHighlighting("PowerShell", new[] { ".ps1", ".psm1", ".psd1" }, "PowerShell.xshd");
			//RegisterHighlighting("PHP", new[] { ".php" }, "PHP-Mode.xshd");
			//RegisterHighlighting("Python", new[] { ".py", ".pyw" }, "Python-Mode.xshd");
			//RegisterHighlighting("TeX", new[] { ".tex" }, "Tex-Mode.xshd");
			//RegisterHighlighting("TSQL", new[] { ".sql" }, "TSQL-Mode.xshd");
			//RegisterHighlighting("VB", new[] { ".vb" }, "VB-Mode.xshd");
			//RegisterHighlighting("XML", (".xml;.xsl;.xslt;.xsd;.manifest;.config;.addin;" +
			//								 ".xshd;.wxs;.wxi;.wxl;.proj;.csproj;.vbproj;.ilproj;" +
			//								 ".booproj;.build;.xfrm;.targets;.xaml;.xpt;" +
			//								 ".xft;.map;.wsdl;.disco;.ps1xml;.nuspec").Split(';'),
			//						 "XML-Mode.xshd");
			//RegisterHighlighting("MarkDown", new[] { ".md" }, "MarkDown-Mode.xshd");
			//RegisterHighlighting("MarkDownWithFontSize", new[] { ".md" }, "MarkDownWithFontSize-Mode.xshd");
			//RegisterHighlighting("Json", new[] { ".json" }, "Json.xshd");
		}


		private static void RegisterHighlighting(string name, string[] extensions, string resourceName)
        {
			using (var stream = OpenStream(resourceName))
			{
				using (var reader = new XmlTextReader(stream))
				{
					var hm = HighlightingManager.Instance;
					hm.RegisterHighlighting(name, extensions, HighlightingLoader.Load(reader, hm));
				}
			}
		}
	}
}