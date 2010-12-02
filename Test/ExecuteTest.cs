using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Numerics;

namespace Kurogane.Test {

	/// <summary>
	/// 適当なテストを実行するファイル。
	/// 通過しても何の意味もないが、通過しない場合は問題。
	/// </summary>
	[TestClass]
	public class ExecuteTest {

		private Engine _engine = new Engine();
		private object Execute(string code) { return Execute<object>(code); }
		private T Execute<T>(string code) { return (T)_engine.Execute(code, Statics.TestName); }

		public void Init() {
			Execute(
				"以下の定義でNをパスする。" +
				"	Nである。" +
				"以上。");
		}

		[TestMethod]
		public void コメントを読み飛ばす() {
			Execute(
				"以下の定義でNをパスする。" +
				"	Nである。" +
				"以上。");
			object num = Execute(
				"※これは、コメントです。" +
				"1をパスする。" +
				"※｛括弧で囲むよ。複数でもOK！。。。｝" +
				"3をパスする。");
			Assert.AreEqual(3, (int)num);
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
			object num = Execute(code);
			Assert.AreEqual(55, (int)num);

			code =
				"以下の定義でNをFIB変換する。" +
				"	以下の定義でAとBをNまでFIB変換する。" +
				"		もし(N>0)なら" +
				"			Bと(A+B)を(N-1)までFIB変換する。" +
				"		他なら" +
				"			Aである。" +
				"	以上。" +
				"	0と1をNまでFIB変換する。" +
				"以上。" +
				"1000をFIB変換する。";
			num = Execute(code);
			var resultTxt =
				"4346655768" + "6937456435" + "6885276750" + "4062580256" + "4660517371" +
				"7804024817" + "2908953655" + "5417949051" + "8904038798" + "4007925516" +
				"9295922593" + "0803226347" + "7520968962" + "3239873322" + "4711616429" +
				"9644090653" + "3187938298" + "9696499285" + "1600370447" + "6137795166" +
				"849228875";
			var expected = BigInteger.Parse(resultTxt);
			Assert.AreEqual(expected, (BigInteger)num);

			code =
				"以下の定義でNをフィボナッチする。" +
				"　　もしN≦1なら、" +
				"　　　　Nである。" +
				"　　他なら、" +
				"　　　　(N-1)と(N-2)をそれぞれフィボナッチし、加算する。" +
				"以上。" +
				"10をフィボナッチする。";
			num = Execute(code);
			Assert.AreEqual(55, (int)num);
		}

		[TestMethod]
		public void 対で引数をとれる() {
			object num = Execute(
				"以下の定義でAとBを加算する。" +
				"	(A+B)である。" +
				"以上。" +
				"3と5を加算する。");
			Assert.AreEqual(8, (int)num);
		}

		[TestMethod]
		public void ローカル変数() {
			Execute(
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
			Execute(
				"「こんにちは」と「こんばんは」をペアに代入する。" +
				"ペアの頭をAに代入する。" +
				"ペアの体をBに代入する。");
			Assert.AreEqual("こんにちは", _engine.Global.GetVariable("A"));
			Assert.AreEqual("こんばんは", _engine.Global.GetVariable("B"));
		}


		[TestMethod]
		public void リストの中身を合計する() {
			string code =
				"以下の定義でAにBを連結する。" +
				"	以下の定義でTで判定する。" +
				"		もし(T＝0)なら" +
				"			Aである。" +
				"		他なら" +
				"			Bである。" +
				"	以上。" +
				"	判定である。" +
				"以上。" +
				"以下の定義で分解を合計する。" +
				"	もし(分解＝0)なら、0である。" +
				"	0で分解し、Aとする。" +
				"	1で分解し、合計し、Bとする。" +
				"	(A+B)である。" +
				"以上。" +
				"0を1に連結し、2に連結し、3に連結し、4に連結し、合計する。";
			object num = Execute(code);
			Assert.AreEqual(10, (int)num);
		}
	}
}
