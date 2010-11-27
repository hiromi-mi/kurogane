using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Kurogane.Test.Spec {

	/// <summary>
	/// このクラスでは，言語仕様に関してのテストを行います。
	/// </summary>
	[TestClass]
	public class Language : NoSideEffectTest {

		[TestMethod]
		public void 何も無いプログラムを実行できる() {
			Assert.IsNull(Execute(""));
		}

		[TestMethod]
		public void である_によるReturn文() {
			Assert.AreEqual(1, Execute<int>("１である。"));
			Assert.AreEqual(3, Execute<int>("３である。５である。"));
		}

		[TestMethod]
		public void 助詞によって引数の順番を切り替える() {
			var engine = new Engine();
			string[] codes = {
				"「A」が「B」を「C」にテストする。" ,
				"「A」が「C」に「B」をテストする。" ,
				"「B」を「A」が「C」にテストする。" ,
				"「B」を「C」に「A」がテストする。" ,
				"「C」に「A」が「B」をテストする。" ,
				"「C」に「B」を「A」がテストする。"};
			engine.Global.SetVariable("テスト", new SuffixFunc<Func<object, object, object, object>>(
				(a, b, c) => ((string)a == "A" && (string)b == "B" && (string)c == "C"),
				"が", "を", "に"));
			foreach (var code in codes) {
				Assert.IsTrue((bool)engine.Execute(code, Statics.TestName));
			}
			var defun =
				"以下の定義でAがBをCにテストする。" +
				"	[A,B,C]を文字列連結する。" +
				"以上。";
			engine.Execute(defun, Statics.TestName);
			foreach (var code in codes) {
				Assert.AreEqual("ABC", (string)engine.Execute(code, Statics.TestName));
			}
		}

		[TestMethod]
		public void それぞれ_によるMap処理() {
			// リストに対して
			var actual1 = Execute<ListCell>("【○×○】を二乗とする。[1,2,3,4,5]をそれぞれ二乗する。");
			var expected1 = ListCell.ConvertFrom(1, 4, 9, 16, 25);
			Assert.AreEqual(expected1, actual1);

			var actual2 = Execute<Tuple<object, object>>("【○×２】を二倍とする。３と４をそれぞれ二倍する。");
			var expected2 = new Tuple<object, object>(6, 8);
			Assert.AreEqual(expected2, actual2);
		}
	}
}
