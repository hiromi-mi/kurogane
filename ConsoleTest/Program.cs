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
			LinerFib();
		}

		static void Test() {
			var code =
				"3をAに代入する。" +
				"もし(A＝無)なら" +
				"	「正しい」をパスする。" +
				"他なら" +
				"	「間違い」をパスする。";

			var engine = new Engine();
			var result = engine.Execute(code);
			Console.WriteLine(result);
		}

		static void Pair() {
			var code =
				"「こんにちは」と「こんばんは」をペアに代入する。" +
				"ペアの頭を表示する。" +
				"ペアの体を表示する。";
			var engine = new Engine();
			engine.Execute(code);
		}

		static void List() {
			var code =
				"以下の手順でAとBを加算する。" +
				"	(A＋B)をパスする。" +
				"以上。" +
				"以下の手順でAとBを乗算する。" +
				"	(A×B)をパスする。" +
				"以上。" +
				"以下の手順でリストを初期値から関数で集約する。" +
				"	以下の手順で計算する。" +
				"		リストの体を初期値から関数で集約し、次に代入する。" +
				"		リストの頭と次を関数する。" +
				"	以上。" +
				"	もし(リスト＝無)なら" +
				"		初期値をパスする。" +
				"	他なら" +
				"		計算する。" +
				"以上。" +
				"1と2と3と4と5とを0から加算で集約し、Aに代入する。" +
				"1と2と3と4と5とを1から乗算で集約し、Bに代入する。" +
				"AとBを表示する。";

			var engine = new Engine();
			engine.Execute(code);
		}

		public static void Fibonacci() {
			string code =
				"以下の手順でNをFIB変換する。" +
				"	以下の手順で計算する。" +
				"		(N-1)をFIB変換し、Aに代入する。" +
				"		(N-2)をFIB変換し、Bに代入する。" +
				"		(A+B)をパスする。" +
				"	以上。" +
				"	もし(N≦1)なら" +
				"		Nをパスする。" +
				"	他なら" +
				"		計算する。" +
				"以上。" +
				"27をFIB変換し、表示する。";
			var engine = new Engine();
			engine.Execute(code);
		}

		public static void LinerFib() {
			string code =
				"以下の手順でNをFIB変換する。" +
				"	以下の手順でペアをNまでFIB変換する。" +
				"		ペアの頭をAに代入する。" +
				"		ペアの体をBに代入する。" +
				"		もし(N>0)なら" +
				"			Bと(A+B)を(N-1)までFIB変換する。" +
				"		他なら" +
				"			Aをパスする。" +
				"	以上。" +
				"	0と1をNまでFIB変換する。" +
				"以上。" +
				"1000をFIB変換し、表示する。";
			var engine = new Engine();
			engine.Execute(code);
		}


	}
}
