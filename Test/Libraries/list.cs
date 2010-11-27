using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Kurogane.Test.Libraries {

	[TestClass]
	public class リスト系 : NoSideEffectTest {

		[TestMethod]
		public void 反転() {
			var result = Execute("[1,2,3,4,5]を反転する。");
			var actual = ListCell.ConvertFrom(new[] { 5, 4, 3, 2, 1 });
			Assert.AreEqual(actual, result);
		}

		[TestMethod]
		public void 検索() {
			var result1 = Execute("[1,2,3,4,5]から【□＝3】を検索する。");
			Assert.AreEqual(3, (int)result1);

			var result2 = Execute("[1,2,3,4,5]から【□＝10】を検索する。");
			Assert.IsNull(result2);
		}

		[TestMethod]
		public void 列挙() {
			var result = Execute("[1,2,3,4,5]から【□％２＝０】を列挙する。");
			var actual = ListCell.ConvertFrom(new[] { 2, 4 });
			Assert.AreEqual(actual, result);
		}

		[TestMethod]
		public void 集約() {
			var result1 = Execute("0と[1,2,3,4,5,6,7,8,9,10]を【○＋□】で集約する。");
			Assert.AreEqual(55, (int)result1);

			var result2 = Execute("[10]を【○＋□】で集約する。");
			Assert.AreEqual(10, (int)result2);

			var result3 = Execute("[]を【○＋□】で集約する。");
			Assert.IsNull(result3);
		}

		[TestMethod]
		public void 点呼() {
			var result1 = Execute("[]を点呼する。");
			Assert.AreEqual(0, (int)result1);

			var result2 = Execute("[1,2,3,4,5,6,7,8,9,10]を点呼する。");
			Assert.AreEqual(10, (int)result2);
		}

		[TestMethod]
		public void 射影() {
			var result1 = Execute("[]を【○×○】で射影する。");
			Assert.IsNull(result1);

			var result2 = Execute("[1,2,3,4,5]を【○×○】で射影する。");
			var actual2 = ListCell.ConvertFrom(new[] { 1, 4, 9, 16, 25 });
			Assert.AreEqual(actual2, result2);
		}
	}
}
