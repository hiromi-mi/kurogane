using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Kurogane.Compilers;

namespace Kurogane
{
	public class NewEngine
	{
		/// <summary>グローバルスコープ</summary>
		public Scope Global { get; private set; }

		/// <summary>入力先</summary>
		public TextReader In { get; set; }

		/// <summary>出力先</summary>
		public TextWriter Out { get; set; }


		public NewEngine()
		{
			// プログラムにとっての標準入出力を設定。
			In = Console.In;
			Out = Console.Out;
			Global = new Scope();
		}

		public object Execute(string code)
		{
			var token = Tokenizer.Tokenize(code);
			var ast = AnotherParser.Parse(token, null);
			var expr = Generator.Generate(ast);
			var func = expr.Compile();
			return func(this.Global);
		}
	}
}
