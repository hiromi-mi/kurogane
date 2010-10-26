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
			MapTest();
		}

		static void MapTest() {
			var code =
				"以下の手順でNを改行出力する。" +
				"	Nを出力する。" +
				"	改行を出力する。" +
				"以上。" +
				"以下の手順でPをパスする。" +
				"	Pを返す。" +
				"以上。" +
				"[1,2,3,4,5]をパスし、それぞれ改行出力する。";
			var e = new Engine();
			e.Execute(code);
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

		static void FibTest() {
			string code =
				"以下の手順でNをFIB変換する。" +
				"	もし(N≦1)ならNを返す。" +
				"	(N-1)をFIB変換し、Aとする。" +
				"	(N-2)をFIB変換し、Bとする。" +
				"	(A+B)を返す。" +
				"以上。" +
				"36をFIB変換し、出力する。改行を出力する。";
				//"以下の手順でAをパスする。" +
				//"	Aを返す。" +
				//"以上。" +
				//"以下の手順でNをFIB変換する。" +
				//"	以下の手順で計算する。" +
				//"		(N-1)をFIB変換し、Aとする。" +
				//"		(N-2)をFIB変換し、Bとする。" +
				//"		(A+B)をパスする。" +
				//"	以上。" +
				//"	もし(N≦1)なら" +
				//"		Nをパスする。" +
				//"	他なら" +
				//"		計算する。" +
				//"以上。" +
				//"27をFIB変換し、出力する。";

			var engine = new Engine();
			engine.Execute(code);
		}

		static void Test() {
			var code =
				"「こんにちは」と「こんばんは」をペアに代入する。" +
				"ペアの頭を出力する。改行を出力する。" +
				"ペアの体を出力する。改行を出力する。";
			var engine = new Engine();
			engine.Execute(code);
		}

		static void SumTest() {

			var libCode =
				"以下の手順でリストを初期値から関数で集約する。" +
				"	以下の手順で計算する。" +
				"		リストの体を初期値から関数で集約し、次に代入する。" +
				"		リストの頭と次を関数する。" +
				"	以上。" +
				"	もし(リスト＝無)なら" +
				"		初期値をパスする。" +
				"	他なら" +
				"		計算する。" +
				"以上。";

			var userCodeA =
				"以下の手順でAとBを加算する。" +
				"	(A＋B)をパスする。" +
				"以上。" +
				"[1, 2, 3, 4, 5]を0から加算で集約し、出力する。改行を出力する。";

			var userCodeB =
				"以下の手順でAとBを乗算する。" +
				"	(A×B)をパスする。" +
				"以上。" +
				"[1, 2, 3, 4, 5]を1から乗算で集約し、出力する。改行を出力する。";

			var engine = new Engine();
			engine.Global.SetVariable("パス", SuffixFunc.Create((object obj) => obj, "を"));
			engine.Execute(libCode);
			engine.Execute(userCodeA);
			engine.Execute(userCodeB);
		}

		static void TestGreet() {
			var code =
				"以下の手順で挨拶する。" +
				"	「こんにちは」を出力する。" +
				"	改行を出力する。" +
				"以上。" +
				"挨拶する。" +
				"挨拶する。";
			var engine = new Engine();
			engine.Execute(code);
		}
	}
}
