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
	}
}
