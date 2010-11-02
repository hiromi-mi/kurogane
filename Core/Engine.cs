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
			var token = Tokenizer.Tokenize(code);
			var ast = Parser.Parse(token, null);
			var expr = Generator.Generate(ast, this.Factory, null);
			var func = expr.Compile();
			return func(this.Global);
		}

		public object ExecuteFile(string filepath) {
			using (var file = File.OpenRead(filepath))
			using (var stream = new StreamReader(file, DefaultEncoding)) {
				var token = Tokenizer.Tokenize(stream, filepath);
				var ast = Parser.Parse(token, filepath);
				var expr = Generator.Generate(ast, this.Factory, filepath);
				var func = expr.Compile();
				return func(this.Global);
			}
		}

		private object ExecuteStream(Stream stream, string filename) {
			using (var reader = new StreamReader(stream, DefaultEncoding)) {
				var token = Tokenizer.Tokenize(reader, filename);
				var ast = Parser.Parse(token, filename);
				var expr = Generator.Generate(ast, this.Factory, filename);
				var func = expr.Compile();
				return func(this.Global);
			}
		}

		private void InitLibrary() {
			Global.SetVariable("入力", new SuffixFunc<Func<object>>(this.In.ReadLine));
			Global.SetVariable("出力", new SuffixFunc<Func<object, object>>(
				obj => { this.Out.Write(obj); return obj; }, "を"));
			Global.SetVariable("改行", Environment.NewLine);
		}

		private void LoadStandardLibraries() {
			var asm = this.GetType().Assembly;
			foreach (var name in asm.GetManifestResourceNames())
				if (name.EndsWith(".krg.txt"))
					this.ExecuteStream(asm.GetManifestResourceStream(name), name);
		}
	}
}
