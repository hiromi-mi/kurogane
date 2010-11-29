using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Kurogane.Test.Libraries {

	[TestClass]
	public class Math : TestHelper {

		[TestMethod]
		public void 和名四則演算() {
			Assert.AreEqual(8, Execute<int>("３に５を加算する。"));
			Assert.AreEqual(3, Execute<int>("７から４を減算する。"));
			Assert.AreEqual(6, Execute<int>("２と３を乗算する。"));
			Assert.AreEqual(3, Execute<int>("７を２で除算する。"));
			Assert.AreEqual(2, Execute<int>("８を３で剰余算する。"));
		}
	}
}
