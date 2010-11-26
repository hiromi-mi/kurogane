using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Kurogane;
using Kurogane.Compiler;
using Kurogane.Expressions;

namespace ConsoleTest {
	public class EngineTest : Engine {

		public static void Run() {
			var code =
				"【□＋△】を加算とする。" +
				"以下の定義でNをフィボナッチする。" +
				"　　もし（N≦１）なら、" +
				"　　　　Nである。" +
				"　　他なら、" +
				"　　　　(N-1)と(N-2)をそれぞれフィボナッチし、加算する。" +
				"以上。" +
				"10をフィボナッチする。";

			var engine = new EngineTest();
			var result = engine.Execute(code);
			Console.WriteLine(result);
		}

		protected override object ExecuteCore(TextReader stream, string filename) {
			Func<Scope, object> program;
			{
				var token = Tokenizer.Tokenize(stream, filename);
				var ast = Parser.Parse(token, filename);
				var expr = ExpressionGenerator.Generate(ast, this.Factory, filename);
				//expr = ExpressionOptimizer.Analyze(expr);
				program = expr.Compile();
			}
			{
				var result = program(this.Global);
				return result;
			}
		}
	}
}
