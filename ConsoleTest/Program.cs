using System;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Numerics;
using Kurogane;
using Kurogane.RuntimeBinder;

namespace ConsoleTest {

	class Program {

		class Hoge1 {
			public int Value { get; set; }
			public Hoge1(int value) {
				this.Value = value;
			}
		}

		class Hoge2 {
			public int Value { get; set; }
			public Hoge2(int value) {
				this.Value = value;
			}

			public static bool operator <(Hoge1 left, Hoge2 right) {
				return left.Value < right.Value;
			}
			public static bool operator >(Hoge1 left, Hoge2 right) {
				return left.Value > right.Value;
			}
		}


		static void Main(string[] args) {
			CompTest();
		}

		static void Hoge() {
			var factory = new BinderFactory();
			var paramA = Expression.Parameter(typeof(object));
			var paramB = Expression.Parameter(typeof(object));
			var dyn = Expression.Dynamic(factory.LessThanBinder, typeof(object), paramA, paramB);
			var lambda = Expression.Lambda<Func<object, object, object>>(dyn, paramA, paramB);
			var func = lambda.Compile();
			var result = func(new Hoge1(3), new Hoge2(5));
			Console.WriteLine(result);
		}

		static void CompTest() {
			{
				dynamic a = null;
				dynamic b = 1;
				dynamic c = a > b;
				Console.WriteLine(c);
			}
			var paramA = Expression.Parameter(typeof(object));
			var paramB = Expression.Parameter(typeof(object));
			var lessThan = Expression.Lambda<Func<object, object, object>>(
				Expression.Convert(Expression.LessThan(paramA, paramB), typeof(object)),
				paramA, paramB);
			var func = lessThan.Compile();
			var result = func(null, null);
			Console.WriteLine(result);
		}

		static void SumTestCore() {
			var engine = new Engine();
			engine.Execute(
				"0と[1, 2, 3, 4, 5]を【○＋△】で集約し、Aとする。" +
				"1と[1, 2, 3, 4, 5]を【○×△】で集約し、Bとする。");
		}

		static void Add() {
			var engine = new Engine();
			engine.Execute(@"
以下の定義でAとBを加算する。
	もし（A≦0）なら、Bである。
	他なら、(A-1)と(B+1)を加算する。
以上。
(1000*1000)と(1000*1000)を加算し、出力する。改行を出力する。
");
		}

		static void SearchOne() {
			var code =
				"3を対象とする。" +
				"[3,1,4,1,5,9,2,6,5]から【□＝対象】を検索し，出力する。";
			var engine = new Engine();
			engine.Execute(code);
		}

		static void UniqueTest() {
			var engine = new Engine();
			engine.Execute(@"
以下の定義でリストをユニークする。
　　もし（リスト＝無）なら、無である。
　　リストの体をユニークし、ユニーク体とする。
	リストの頭を対象とする。
　　ユニーク体から【□＝対象】を検索し、発見とする。
　　もし
　　　　発見なら、ユニーク体である。
　　　　他なら、リストの頭とユニーク体である。
以上。
[3,1,4,1,5,9,2,6,5]をユニークし、それぞれ出力する。
");
		}

		static void LinkedList() {
			string code = @"
[「A」]を甲とする。
[「B」]を乙とする。
[「C」]を丙とする。

以下の定義でリストから値を検索する。
　　もし
　　　　（リスト＝無）なら
　　　　　　無を返す。
　　　　（リストの頭＝値）なら
　　　　　　リストの頭を返す。
　　　　他なら
　　　　　　リストの体から値を検索する。
以上。

以下の定義で前者に後者を挿入する。
　　前者の頭と後者の頭と前者の体を返す。
以上。

甲に乙を挿入し，丙を挿入し，甲弐式とする。
甲弐式をそれぞれ出力する。

「|」を出力する。
甲弐式から「D」を検索し，出力する。
「|」を出力する。
甲弐式から「A」を検索し，出力する。
改行を出力する。
";
			var e = new Engine();
			e.Execute(code);
		}

		static void MapTest() {
			var code =
				"以下の定義でNを改行出力する。" +
				"	Nを出力する。" +
				"	改行を出力する。" +
				"以上。" +
				"以下の定義でPをパスする。" +
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
				"以下の定義でNをFIB変換する。" +
				"	もし(N≦1)ならNである。" +
				"	他なら以下を実行する。" +
				"		(N-1)をFIB変換し、Aとする。" +
				"		(N-2)をFIB変換し、Bとする。" +
				"		(A+B)である。" +
				"	以上。" +
				"以上。" +
				"39をFIB変換し、出力する。改行を出力する。";
			var sw = Stopwatch.StartNew();
			var engine = new Engine();
			engine.Execute(code);
			sw.Stop();
			Console.WriteLine(sw.ElapsedMilliseconds);
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

			//var libCode =
			//    "以下の定義でリストを初期値から関数で集約する。" +
			//    "	以下の定義で計算する。" +
			//    "		リストの体を初期値から関数で集約し、次に代入する。" +
			//    "		リストの頭と次を関数する。" +
			//    "	以上。" +
			//    "	もし(リスト＝無)なら" +
			//    "		初期値をパスする。" +
			//    "	他なら" +
			//    "		計算する。" +
			//    "以上。";

			var userCodeA =
				"以下の定義でAとBを加算する。" +
				"	(A＋B)である。" +
				"以上。" +
				"0と[1, 2, 3, 4, 5]を加算で集約し、出力する。改行を出力する。";

			//var userCodeB =
			//    "以下の定義でAとBを乗算する。" +
			//    "	(A×B)をパスする。" +
			//    "以上。" +
			//    "[1, 2, 3, 4, 5]を1から乗算で集約し、出力する。改行を出力する。";

			var engine = new Engine();
			engine.Global.SetVariable("パス", SuffixFunc.Create((object obj) => obj, "を"));
			//engine.Execute(libCode);
			engine.Execute(userCodeA);
			//engine.Execute(userCodeB);
		}

		static void TestGreet() {
			var code =
				"以下の定義で挨拶する。" +
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
