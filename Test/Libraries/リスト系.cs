using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Kurogane.Test.Libraries {

	[TestClass]
	public class リスト系 {

		private Engine engine;

		[TestInitialize]
		public void Init() {
			engine = new Engine();
		}

		[TestMethod]
		public void 検索() {
			var result = engine.Execute("[1,2,3,4,5]から【□＝3】を検索する。");
			Assert.AreEqual(3, (int)result);
		}

		[TestMethod]
		public void 集約() {
			var result = engine.Execute("0と[1,2,3,4,5,6,7,8,9,10]を【○＋□】で集約する。");
			Assert.AreEqual(55, (int)result);
		}
	}
}
