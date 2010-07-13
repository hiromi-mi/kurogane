using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Kurogane.Runtime;
using Kurogane.Buildin;

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
				"10をFIB変換し、表示する。";
			var engine = new Engine();
			object num = engine.Execute(code);
			Assert.IsInstanceOfType(num, typeof(int));
			Assert.AreEqual((int)num, 55);
		}

		[TestMethod]
		public void リストの中身を合計する() {
			string code =
				"以下の手順で[A]に[B]を[連結]する。" +
				"	以下の手順で[T]で[判定]する。" +
				"		もし([T]＝0)なら" +
				"			[A]を[パス]する。" +
				"		他なら" +
				"			[B]を[パス]する。" +
				"	以上。" +
				"	[判定]を[パス]する。" +
				"以上。" +
				"以下の手順で[分解]を[合計]する。" +
				"	以下の手順で[計算]する。" +
				"		0で[分解]し、[A]に[代入]する。" +
				"		1で[分解]し、[合計]し、[B]に[代入]する。" +
				"		([A]+[B])を[パス]する。" +
				"	以上。" +
				"	もし([分解]＝0)なら" +
				"		0を[パス]する。" +
				"	他なら" +
				"		[計算]する。" +
				"以上。" +
				"0を1に[連結]し、2に[連結]し、3に[連結]し、4に[連結]し、[リスト]に[代入]する。" +
				"[リスト]を[合計]する。";
			var engine = new Engine();
			object num = engine.Execute(code);
			Assert.IsInstanceOfType(num, typeof(int));
			Assert.AreEqual((int)num, 10);
		}

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
	}
}
