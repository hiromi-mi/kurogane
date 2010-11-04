using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Kurogane.Test {

	[TestClass]
	public class ExecuteTest {

		private Engine _engine;

		[TestInitialize]
		public void Init() {
			_engine = new Engine();
			_engine.Execute(
				"以下の定義でNをパスする。" +
				"	Nである。" +
				"以上。");
		}


		[TestMethod]
		public void 助詞によって引数の順番を切り替える() {
			string[] codes = {
				"「A」が「B」を「C」にテストする。" ,
				"「A」が「C」に「B」をテストする。" ,
				"「B」を「A」が「C」にテストする。" ,
				"「B」を「C」に「A」がテストする。" ,
				"「C」に「A」が「B」をテストする。" ,
				"「C」に「B」を「A」がテストする。"};
			_engine.Global.SetVariable("テスト", new SuffixFunc<Func<object, object, object, object>>(
				(a, b, c) => ((string)a == "A" && (string)b == "B" && (string)c == "C"),
				"が", "を", "に"));
			foreach (var code in codes) {
				var result = _engine.Execute(code);
				Assert.IsInstanceOfType(result, typeof(bool));
				Assert.IsTrue((bool)result);
			}
		}

		[TestMethod]
		public void コメントを読み飛ばす() {
			_engine.Execute(
				"以下の定義でNをパスする。" +
				"	Nである。" +
				"以上。");
			object num = _engine.Execute(
				"※これは、コメントです。" +
				"1をパスする。" +
				"※（括弧で囲むよ。複数でもOK！。。。）" +
				"3をパスする。");
			Assert.IsInstanceOfType(num, typeof(int));
			Assert.AreEqual((int)num, 3);
		}

		[TestMethod]
		public void フィボナッチ数を計算する() {
			var code =
				"以下の定義でNをフィボナッチする。" +
				"	もし(N≦1)なら" +
				"		Nである。" +
				"	(N-1)をフィボナッチし、Aとする。" +
				"	(N-2)をフィボナッチし、Bとする。" +
				"	(A+B)である。" +
				"以上。" +
				"10をフィボナッチする。";
			object num = _engine.Execute(code);
			Assert.IsInstanceOfType(num, typeof(int));
			Assert.AreEqual((int)num, 55);

			code =
				"以下の定義でNをFIB変換する。" +
				"	以下の定義で頭と体をNまでFIB変換する。" +
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
			num = _engine.Execute(code);
			Assert.IsInstanceOfType(num, typeof(int));
			Assert.AreEqual((int)num, 1556111435);
		}

		[TestMethod]
		public void 対で引数をとれる() {
			object num = _engine.Execute(
				"以下の定義でAとBを加算する。" +
				"	(A+B)である。" +
				"以上。" +
				"3と5を加算する。");
			Assert.IsInstanceOfType(num, typeof(int));
			Assert.AreEqual((int)num, 8);
		}

		[TestMethod]
		public void 集約関数() {
			_engine.Execute(
				"0と[1, 2, 3, 4, 5]を【○＋△】で集約し、Aとする。" +
				"1と[1, 2, 3, 4, 5]を【○×△】で集約し、Bとする。");

			object a = _engine.Global.GetVariable("A");
			Assert.IsInstanceOfType(a, typeof(int));
			Assert.AreEqual((int)a, 15);

			object b = _engine.Global.GetVariable("B");
			Assert.IsInstanceOfType(b, typeof(int));
			Assert.AreEqual((int)b, 120);
		}

		[TestMethod]
		public void ローカル変数() {
			_engine.Execute(
				"「こんにちは」をAに代入する。" +
				"「さようなら」をBに代入する。" +
				"以下の定義でテストする。" +
				"	「おはよう」をAに代入する。" +
				"	「ごきげんよう」をBとする。" +
				"	「ようこそ」をCとする。" +
				"以上。" +
				"テストする。");

			var scope = _engine.Global;
			Assert.AreEqual("おはよう", scope.GetVariable("A"));
			Assert.AreEqual("さようなら", scope.GetVariable("B"));
			Assert.IsFalse(scope.HasVariable("C"));
		}

		[TestMethod]
		public void と_によって対が作られる() {
			_engine.Execute(
				"「こんにちは」と「こんばんは」をペアに代入する。" +
				"ペアの頭をAに代入する。" +
				"ペアの体をBに代入する。");
			Assert.AreEqual("こんにちは", _engine.Global.GetVariable("A"));
			Assert.AreEqual("こんばんは", _engine.Global.GetVariable("B"));
		}

		[TestMethod]
		public void もし文が動く() {
			var result = _engine.Execute(
				"3をAとする。" +
				"もし" +
				"　(A＝無)なら" +
				"　　「失敗」である。" +
				"　(A＝３)なら" +
				"　　「成功」である。" +
				"　他なら" +
				"　　「失敗」である。");

			Assert.AreEqual("成功", result);
		}
	}
}
