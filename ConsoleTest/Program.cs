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
				"以下の定義でAとBを加算する。" +
				"	もし（A≦0）なら、Bである。" +
				"	他なら、(A-1)と(B+1)を加算する。" +
				"以上。" +
				"(1000*1000)と(1000*1000)を加算し、出力する。改行を出力する。";
			Execute(code);
		}

		private static void Execute(string code) {
			var engine = new Engine();
			var result = engine.Execute(code, "console-test");
			Console.WriteLine(result);
		}
	}
}
