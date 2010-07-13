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
		public void 助詞が正しく選択される() {
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
		public void フィボナッチ数が正しく計算される() {
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
	}
}
