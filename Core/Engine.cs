using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Kurogane.Dynamic;
using Kurogane.Compiler;
using System.Diagnostics;
using Kurogane.RuntimeBinder;

namespace Kurogane {
	public class Engine {
		// ----- ----- ----- ----- ----- fields ----- ----- ----- ----- -----

		const string LibraryPath = "Libraries";

		protected BinderFactory Factory { get; private set; }

		/// <summary>グローバルスコープ</summary>
		public Scope Global { get; private set; }

		/// <summary>入力先</summary>
		public TextReader In { get; set; }

		/// <summary>出力先</summary>
		public TextWriter Out { get; set; }

		public Encoding DefaultEncoding { get; set; }

		// ----- ----- ----- ----- ----- ctor ----- ----- ----- ----- -----

		/// <summary>通常のコンストラクタ</summary>
		public Engine()
			: this(new BinderFactory()) {
			InitLibrary();
			LoadStandardLibraries();
		}

		protected Engine(BinderFactory factory) {
			Debug.Assert(factory != null, "factory is null");

			Factory = factory;
			Global = new Scope();
			In = Console.In;
			Out = Console.Out;
			DefaultEncoding = Encoding.Default;
		}

		// ----- ----- ----- ----- ----- methods ----- ----- ----- ----- -----

		public object Execute(string code) {
			return ExecuteCore(new StringReader(code), null);
		}

		public object ExecuteFile(string filepath) {
			using (var file = File.OpenRead(filepath))
			using (var stream = new StreamReader(file, DefaultEncoding)) {
				return ExecuteCore(stream, filepath);
			}
		}

		private object ExecuteStream(Stream stream, string filename) {
			using (var reader = new StreamReader(stream, DefaultEncoding)) {
				return ExecuteCore(reader, filename);
			}
		}

		private object ExecuteCore(TextReader stream, string filename) {
			var token = Tokenizer.Tokenize(stream, filename);
			var ast = Parser.Parse(token, filename);
			var expr = Generator.Generate(ast, this.Factory, filename);
			//expr = FuncAnalyzer.Analyze(expr);
			var func = expr.Compile();
			return func(this.Global);
		}

		private void InitLibrary() {
			Global.SetVariable("入力", new SuffixFunc<Func<object>>(this.In.ReadLine));
			Global.SetVariable("出力", new SuffixFunc<Func<object, object>>(
				obj => { this.Out.Write(obj); return obj; }, "を"));
			Global.SetVariable("改行", Environment.NewLine);
			Global.SetVariable("文字分割", new SuffixFunc<Func<object, object>>(input => {
				var str = input as string;
				return ListCell.ConvertFrom(str.ToCharArray());
			}, "を"));
			Global.SetVariable("整数文字変換", new SuffixFunc<Func<object, object>>(input => {
				var i = input as int?;
				if (i.HasValue) return (char)i;
				else return null;
			}, "を"));
			Global.SetVariable("文字整数変換", new SuffixFunc<Func<object, object>>(input => {
				var c = input as char?;
				if (c.HasValue) return (int)c;
				var str = input as string;
				if (str != null) return (int)str[0];
				return null;
			}, "を"));
		}

		private void LoadStandardLibraries() {
			var asm = this.GetType().Assembly;
			foreach (var name in asm.GetManifestResourceNames())
				if (name.EndsWith(".krg"))
					this.ExecuteStream(asm.GetManifestResourceStream(name), name);
		}
	}
}
