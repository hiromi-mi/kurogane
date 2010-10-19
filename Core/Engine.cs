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

		private readonly BinderFactory _factory;

		/// <summary>グローバルスコープ</summary>
		public Scope Global { get; private set; }

		/// <summary>入力先</summary>
		public TextReader In { get; set; }

		/// <summary>出力先</summary>
		public TextWriter Out { get; set; }

		// ----- ----- ----- ----- ----- ctor ----- ----- ----- ----- -----

		/// <summary>通常のコンストラクタ</summary>
		public Engine()
			: this(new Scope(), new BinderFactory()) {
			InitLibrary();
		}

		/// <summary>継承して、特殊なグローバルスコープを利用する場合、こちらを利用すること。</summary>
		/// <param name="global">呼ばれるグローバルスコープ</param>
		protected Engine(Scope global, BinderFactory factory) {
			Debug.Assert(global != null, "global is null");
			Debug.Assert(factory != null, "factory is null");

			_factory = factory;
			Global = global;
			In = Console.In;
			Out = Console.Out;
		}

		// ----- ----- ----- ----- ----- methods ----- ----- ----- ----- -----

		public object Execute(string code) {
			var token = Tokenizer.Tokenize(code);
			var ast = Parser.Parse(token, null);
			var expr = Generator.Generate(ast, _factory);
			var func = expr.Compile();
			return func(this.Global);
		}

		private void InitLibrary() {
			Global.SetVariable("入力", new SuffixFunc<Func<object>>(this.In.ReadLine));
			Global.SetVariable("出力", new SuffixFunc<Func<object, object>>(
				obj => { this.Out.Write(obj); return obj; }, "を"));
			Global.SetVariable("改行", Environment.NewLine);
		}

		public void ExecuteFile(string filepath) {
			throw new NotImplementedException();
		}
	}
}
