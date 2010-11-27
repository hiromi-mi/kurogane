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
		public void グローバル変数は可変() {
			// 副作用を有り。
			var engine = new Engine();
			var code =
				"０を値とする。" +
				"以下の定義で増加する。" +
				"	（値＋１）を値に代入する。" +
				"以上。" +
				"増加する。増加する。値である。";
			Assert.AreEqual(2, (int)engine.Execute(code, Statics.TestName));
		}

		[TestMethod]
		public void ローカル変数によって上書きされない() {
			// 副作用有り
			var engine = new Engine();
			var code =
				"１を値とする。" +
				"以下の定義で増加する。" +
				"	２を値とする。" +
				"以上。" +
				"値である。";
			Assert.AreEqual(1, (int)engine.Execute(code, Statics.TestName));
		}

		[TestMethod]
		public void 助詞によって引数の順番を切り替える() {
			// 副作用を有り。
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
		public void ラムダ式は第一級関数である() {
			Assert.AreEqual(7, Execute<int>("２を【□＋５】する。"));
			Assert.AreEqual(8, Execute<int>("３と５を【□＋△】する。"));

			var actual1 = Execute<ListCell>("[1,2,3]をそれぞれ【□＋１】する。");
			var expected1 = ListCell.ConvertFrom(2, 3, 4);
			Assert.AreEqual(expected1, actual1);

			var actual2 = Execute<ListCell>("[1:2,3:4,5:6,7:8]をそれぞれ【○＋△】する。");
			var expected2 = ListCell.ConvertFrom(3, 7, 11, 15);
			Assert.AreEqual(expected2, actual2);
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
