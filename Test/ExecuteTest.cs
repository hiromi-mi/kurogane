using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Kurogane.Test {

	[TestClass]
	public class ExecuteTest {

		[TestMethod]
		public void 助詞によって引数の順番を切り替える() {
			string[] codes = {
				"「A」が「B」を「C」にテストする。" ,
				"「A」が「C」に「B」をテストする。" ,
				"「B」を「A」が「C」にテストする。" ,
				"「B」を「C」に「A」がテストする。" ,
				"「C」に「A」が「B」をテストする。" ,
				"「C」に「B」を「A」がテストする。"};
			var engine = new Engine();
			var scope = engine.Global;
			scope.SetVariable("テスト", new SuffixFunc<Func<object, object, object, object>>(
				(a, b, c) => ((string)a == "A" && (string)b == "B" && (string)c == "C"),
				"が", "を", "に"));
			foreach (var code in codes) {
				var result = engine.Execute(code);
				Assert.IsInstanceOfType(result, typeof(bool));
				Assert.IsTrue((bool)result);
			}
		}

		[TestMethod]
		public void コメントを読み飛ばす() {
			string code =
				"※これは、コメントです。" +
				"1をパスする。" +
				"※（括弧で囲むよ。複数でもOK！。。。）" +
				"3をパスする。";
			var engine = new Engine();
			engine.Global.SetVariable("パス", SuffixFunc.Create((object obj) => obj, "を"));
			object num = engine.Execute(code);
			Assert.IsInstanceOfType(num, typeof(int));
			Assert.AreEqual((int)num, 3);
		}

		[TestMethod]
		public void フィボナッチ数を計算する() {
			var engine = new Engine();
			engine.Global.SetVariable("パス", SuffixFunc.Create((object obj) => obj, "を"));

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
				"10をFIB変換する。";
			object num = engine.Execute(code);
			Assert.IsInstanceOfType(num, typeof(int));
			Assert.AreEqual((int)num, 55);

			code =
				"以下の手順でNをFIB変換する。" +
				"	以下の手順で頭と体をNまでFIB変換する。" +
				"		頭をAに代入する。" +
				"		体をBに代入する。" +
				"		もし(N>0)なら" +
				"			Bと(A+B)を(N-1)までFIB変換する。" +
				"		他なら" +
				"			Aをパスする。" +
				"	以上。" +
				"	0と1をNまでFIB変換する。" +
				"以上。" +
				"1000をFIB変換する。";
			num = engine.Execute(code);
			Assert.IsInstanceOfType(num, typeof(int));
			Assert.AreEqual((int)num, 1556111435);
		}

		[TestMethod]
		public void リストの中身を合計する() {
			string code =
				"以下の手順でAにBを連結する。" +
				"	以下の手順でTで判定する。" +
				"		もし(T＝0)なら" +
				"			Aをパスする。" +
				"		他なら" +
				"			Bをパスする。" +
				"	以上。" +
				"	判定をパスする。" +
				"以上。" +
				"以下の手順で分解を合計する。" +
				"	以下の手順で計算する。" +
				"		0で分解し、Aとする。" +
				"		1で分解し、合計し、Bとする。" +
				"		(A+B)をパスする。" +
				"	以上。" +
				"	もし(分解＝0)なら" +
				"		0をパスする。" +
				"	他なら" +
				"		計算する。" +
				"以上。" +
				"0を1に連結し、2に連結し、3に連結し、4に連結し、リストに代入する。" +
				"リストを合計する。";
			var engine = new Engine();
			engine.Global.SetVariable("パス", SuffixFunc.Create((object obj) => obj, "を"));
			object num = engine.Execute(code);
			Assert.IsInstanceOfType(num, typeof(int));
			Assert.AreEqual((int)num, 10);
		}

		[TestMethod]
		public void 対で引数をとれる() {
			var code =
				"以下の手順でAとBを加算する。" +
				"	(A+B)をパスする。" +
				"以上。" +
				"3と5を加算する。";
			var engine = new Engine();
			engine.Global.SetVariable("パス", SuffixFunc.Create((object obj) => obj, "を"));
			object num = engine.Execute(code);
			Assert.IsInstanceOfType(num, typeof(int));
			Assert.AreEqual((int)num, 8);
		}

		[TestMethod]
		public void 集約関数() {
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
				"[1, 2, 3, 4, 5]を0から加算で集約し、Aに代入する。" +
				"[1, 2, 3, 4, 5]を1から乗算で集約し、Bに代入する。";
			var engine = new Engine();
			engine.Global.SetVariable("パス", SuffixFunc.Create((object obj) => obj, "を"));
			engine.Execute(code);

			object a = engine.Global.GetVariable("A");
			Assert.IsInstanceOfType(a, typeof(int));
			Assert.AreEqual((int)a, 15);

			object b = engine.Global.GetVariable("B");
			Assert.IsInstanceOfType(b, typeof(int));
			Assert.AreEqual((int)b, 120);
		}

		[TestMethod]
		public void ローカル変数() {
			string code =
				"「こんにちは」をAに代入する。" +
				"「さようなら」をBに代入する。" +
				"以下の手順でテストする。" +
				"	「おはよう」をAに代入する。" +
				"	「ごきげんよう」をBとする。" +
				"	「ようこそ」をCとする。" +
				"以上。" +
				"テストする。";
			var engine = new Engine();
			engine.Execute(code);
			var scope = engine.Global;
			object a = scope.GetVariable("A");
			object b = scope.GetVariable("B");
			Assert.AreEqual("おはよう", a);
			Assert.AreEqual("さようなら", b);
			Assert.IsFalse(scope.HasVariable("C"));
		}

		[TestMethod]
		public void と_によって対が作られる() {
			var code =
				"「こんにちは」と「こんばんは」をペアに代入する。" +
				"ペアの頭をAに代入する。" +
				"ペアの体をBに代入する。";
			var engine = new Engine();
			engine.Execute(code);
			Assert.AreEqual("こんにちは", engine.Global.GetVariable("A"));
			Assert.AreEqual("こんばんは", engine.Global.GetVariable("B"));
		}

		[TestMethod]
		public void もし文が動く() {
			string code =
				"3をAに代入する。" +
				"もし" +
				"　(A＝無)なら" +
				"　　「間違い」を結果に代入する。" +
				"　(A＝３)なら" +
				"　　「正しい」を結果に代入する。" +
				"　他なら" +
				"　　　「間違い」を結果に代入する。";
			var engine = new Engine();
			engine.Execute(code);
			Assert.AreEqual("正しい", engine.Global.GetVariable("結果"));
		}
	}
}
