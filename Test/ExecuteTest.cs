using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kurogane.Runtime;
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
			var scope = engine.DefaultScope;
			scope.SetVariable("テスト", KrgnFunc.Create(
				(a, b, c) => ((string)a == "A" && (string)b == "B" && (string)c == "C"),
				"が", "を", "に"));
			foreach (var code in codes) {
				var result = engine.Execute(code);
				Assert.IsInstanceOfType(result, typeof(bool));
				Assert.IsTrue((bool)result);
			}
		}

		[TestMethod]
		public void フィボナッチ数を計算する() {
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
				"10をFIB変換する。";
			var engine = new Engine();
			object num = engine.Execute(code);
			Assert.IsInstanceOfType(num, typeof(int));
			Assert.AreEqual((int)num, 55);
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
				"		0で分解し、Aに代入する。" +
				"		1で分解し、合計し、Bに代入する。" +
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
				"1と2と3と4と5とを0から加算で集約し、Aに代入する。" +
				"1と2と3と4と5とを1から乗算で集約し、Bに代入する。";
			var engine = new Engine();
			engine.Execute(code);

			object a = engine.DefaultScope.GetVariable("A");
			Assert.IsInstanceOfType(a, typeof(int));
			Assert.AreEqual((int)a, 15);

			object b = engine.DefaultScope.GetVariable("B");
			Assert.IsInstanceOfType(b, typeof(int));
			Assert.AreEqual((int)b, 120);
		}
	}
}
