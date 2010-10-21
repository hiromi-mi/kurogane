using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Kurogane;
using Kurogane.Dynamic;
using System.Linq.Expressions;
using Kurogane.Util;
using Kurogane.RuntimeBinder;

namespace ConsoleTest {

	class Program {
		static void Main(string[] args) {
			Test();
		}

		static void BasicTest() {
			var code =
				"「こんにちは」を出力する。※改行は出力されないことに注意。 " +
				"改行を出力する。" +
				"(1 * 2 + 3 * 4)を出力する。" +
				"改行を出力する。";
			var e = new Engine();
			e.Execute(code);
		}

		static void Test() {
			var code =
				"「こんにちは」と「こんばんは」をペアに代入する。" +
				"ペアの頭を出力する。改行を出力する。" +
				"ペアの体を出力する。改行を出力する。";
			var engine = new Engine();
			engine.Execute(code);
		}
	}
}
