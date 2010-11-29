using System;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Numerics;
using Kurogane;
using Kurogane.RuntimeBinder;

namespace ConsoleTest {

	class Program {

		static void Main(string[] args) {
			var code =
				"４を値とする。" +
				"以下の定義でテストする。" +
				"　　もし真なら，３を値とする。" +
				"　　　　他なら，５を値とする。" +
				"　　値である。" +
				"以上。" +
				"テストし，テスト結果とする。" +
				"[値,テスト結果]である。";
			Execute(code);
		}

		private static void Execute(string code) {
			var engine = new Engine();
			var result = engine.Execute(code, "console-test");
			Console.WriteLine(result);
		}
	}
}
