using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Kurogane;
using Kurogane.Dynamic;

namespace ConsoleTest {
	class Program {
		static void Main(string[] args) {
			BasicTest();
		}

		static void BasicTest() {
			var code =
				"「こんにちは」を出力する。※改行は出力されないことに注意。 " +
				"改行を出力する。" +
				"「さようなら」を出力する。" +
				"改行を出力する。";
			var e = new Engine();
			e.Execute(code);
		}

		static void NewEngineTest() {
			var code = "挨拶する。";
			var engine = new Engine();
			var greet = SuffixFunc.Create(() => { Console.WriteLine("こんにちは"); return null; });
			engine.Global.SetVariable("挨拶", greet);
			engine.Execute(code);
		}

		static void SfxFuncTest() {
			var func = new SuffixFunc<Func<object, object, object>>(
				(a, b) => (object)((int)a + (int)b),
				new[] { "に", "を" });
			dynamic add = func;
			object ret = add(を:2, げ: 3);
			Console.WriteLine(ret);
		}

		static void MyabeMonad() {
			var code =
				"以下の手順で甲する。" +
				"	「A」をイに代入する。" +
				"	失敗をパスする。" +
				"以上。" +
				"以下の手順で乙する。" +
				"	「B」をロに代入する。" +
				"以上。" +
				"以下の手順で丙する。" +
				"	「C」をハに代入する。" +
				"	失敗をパスする。" +
				"以上。" +
				"以下の手順で丁する。" +
				"	「D」をニに代入する。" +
				"以上。" +
				"甲して、乙する。" +
				"丙して、丁してみる。" +
				"";
			var engine = new Engine();
			engine.Execute(code);
			var scope = engine.Global;
		}

		static void Tree() {
			var code = @"
以下の手順でAにBを連結する。
	AとBをパスする。
以上。

0と1に
0と2を連結し，
1と3を連結し，
1と4を連結し，
1と5を連結し，
2と6を連結し，
2と7を連結し，
3と8を連結し，
3と9を連結し，
7, 10を連結し，
無を連結し，リストに代入する。

以下の手順でNまで線表示する。
	以下の手順で第二線表示する。
		「＿」を出力する。
		(N-1)まで線表示する。
	以上。
	もし(N>0)なら第二線表示する。
以上。

以下の手順でリストをNでフィルタする。
	もし
		(リスト＝無)なら
			無をパスする。
		(リストの頭の頭＝N)なら
			リストの体をフィルタし，リストの頭の体に連結する。
		他なら
			リストの体をフィルタする。
以上。

以下の手順でリストを関数で逐次処理する。
	以下の手順で処理する。
		リストの頭をパスし，関数する。
		リストの体を関数で逐次処理する。
	以上。
	もし(リスト≠無)なら
		処理する。
以上。

以下の手順で基点と深度からツリー表示する。
	深度まで線表示する。
	基点を改行出力する。
	以下の手順で基点から，次処理する。
		基点と(深度+1)からツリー表示する。
	以上。
	リストを基点でフィルタし，次処理で逐次処理する。
以上。
0からリストをツリー表示する。
";

			var engine = new Engine();
			engine.Execute(code);
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

		static void AnotherTestList() {
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
				"1と3と5と7と9とを11から加算で集約し、Aに代入する。" +
				"2と2と2と2と2とを2から乗算で集約し、Bに代入する。" +
				"AとBを表示する。";

			var engine = new Engine();
			engine.Execute(code);
		}

		public static void Fibonacci() {
			string code =
				"以下の手順でNをFIB変換する。" +
				"	以下の手順で計算する。" +
				"		(N-1)をFIB変換し、Aとする。" +
				"		(N-2)をFIB変換し、Bとする。" +
				"		(A+B)をパスする。" +
				"	以上。" +
				"	もし(N≦1)なら" +
				"		Nをパスする。" +
				"	他なら" +
				"		計算する。" +
				"以上。" +
				"10をFIB変換し、表示する。";
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
