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
				"(真＆偽｜真)である。";
			Execute(code);
		}

		private static void Execute(string code) {
			var engine = new Engine();
			var result = engine.Execute(code, "console-test");
			Console.WriteLine(result);
		}
	}
}
