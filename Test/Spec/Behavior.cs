using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Kurogane.Test.Spec {

	/// <summary>
	/// 言語の振る舞いの仕様についてテストします。
	/// </summary>
	[TestClass]
	public class Behavior : TestHelper {

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
		public void 分岐文の中で同じ名前の変数を使っても問題が発生しない() {
			// 副作用を有り。
			var engine = new Engine();
			var code =
				"４を値とする。" +
				"以下の定義で条件でテストする。" +
				"　　もし条件なら，３を値とする。" +
				"　　　　　他なら，５を値とする。" +
				"　　値である。" +
				"以上。" +
				"真でテストし，結果Aとする。" +
				"偽でテストし，結果Bとする。" +
				"[値，結果A，結果B]である。";
			Assert.AreEqual(ListCell.Of(4, 3, 5), (ListCell)engine.Execute(code, Statics.TestName));
		}

		[TestMethod]
		public void 条件文の中でReturnできる() {
			Assert.AreEqual(2, Execute<int>("もし（１＝１）なら，２である。４である。"));
		}
	}
}
