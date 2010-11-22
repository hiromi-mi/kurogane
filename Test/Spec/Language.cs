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
	public class Language {

		private Engine _engine = new Engine();

		[TestMethod]
		public void である_によるReturn文() {
			var result1 = _engine.Execute("１である。");
			Assert.AreEqual(1, (int)result1);

			var result2 = _engine.Execute(
				"３である。" +
				"５である。");
			Assert.AreEqual(3, (int)result2);
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

	}
}
