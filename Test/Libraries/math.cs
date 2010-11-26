using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Kurogane.Test.Libraries {

	[TestClass]
	public class Math {

		// 使いまわし可能
		private Engine _engine = new Engine();

		[TestMethod]
		public void 四則演算() {
			Assert.AreEqual(8, (int)_engine.Execute("３に５を加算する。"));
			Assert.AreEqual(3, (int)_engine.Execute("７から４を減算する。"));
			Assert.AreEqual(6, (int)_engine.Execute("２と３を乗算する。"));
			Assert.AreEqual(3, (int)_engine.Execute("７を２で除算する。"));
			Assert.AreEqual(2, (int)_engine.Execute("８を３で剰余算する。"));
		}
	}
}
