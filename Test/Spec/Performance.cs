using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Kurogane.Test.Spec {

	[TestClass]
	public class Performance : TestHelper {

		[TestMethod]
		public void 末尾再帰最適化が行われる() {
			// 副作用を有り。
			var engine = new Engine();
			var code =
				"以下の定義でAとBを加算する。" +
				"	もし（A≦0）なら、Bである。" +
				"	他なら、(A-1)と(B+1)を加算する。" +
				"以上。" +
				"(1000*1000)と(1000*1000)を加算する。";
			Assert.AreEqual(2 * 1000 * 1000, (int)engine.Execute(code, Statics.TestName));
		}
	}
}
