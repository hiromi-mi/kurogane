using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kurogane.Compiler;
using System.IO;
using Kurogane.Runtime;
using Kurogane.Buildin;

namespace ConsoleTest {
	class Program {
		static void Main(string[] args) {
			Test();
		}

		static void Test() {
			var code =
				"以下の手順でAとBを加算する。" +
				"	(A+B)をパスする。" +
				"以上。" +
				"3と5を加算する。";

			var engine = new Engine();
			var result = engine.Execute(code);
			Console.WriteLine(result);
		}
	}
}
