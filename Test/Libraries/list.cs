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
			var result = Execute<ListCell>("[1,2,3,4,5]を反転する。");
			var expected = ListCell.From(new[] { 5, 4, 3, 2, 1 });
			Assert.AreEqual(expected, result);
		}

		[TestMethod]
		public void 検索() {
			Assert.AreEqual(3, Execute<int?>("[1,2,3,4,5]から【□＝3】を検索する。"));
			Assert.IsNull(Execute<int?>("[1,2,3,4,5]から【□＝10】を検索する。"));
		}

		[TestMethod]
		public void 列挙() {
			var result = Execute<ListCell>("[1,2,3,4,5]から【□％２＝０】を列挙する。");
			var expected = ListCell.From(new[] { 2, 4 });
			Assert.AreEqual(expected, result);
		}

		[TestMethod]
		public void 集約() {
			Assert.AreEqual(55, Execute<int>("0と[1,2,3,4,5,6,7,8,9,10]を【○＋□】で集約する。"));
			Assert.AreEqual(10, Execute<int>("[10]を【○＋□】で集約する。"));
			Assert.IsNull(Execute("[]を【○＋□】で集約する。"));
		}

		[TestMethod]
		public void 点呼() {
			Assert.AreEqual(0, Execute<int>("[]を点呼する。"));
			Assert.AreEqual(10, Execute<int>("[1,2,3,4,5,6,7,8,9,10]を点呼する。"));
		}

		[TestMethod]
		public void 射影() {
			Assert.AreEqual(ListCell.Null,  Execute<ListCell>("[]を【○×○】で射影する。"));

			var result2 = Execute<ListCell>("[1,2,3,4,5]を【○×○】で射影する。");
			var expected2 = ListCell.From(new[] { 1, 4, 9, 16, 25 });
			Assert.AreEqual(expected2, result2);
		}

		[TestMethod]
		public void 並列() {
			Assert.AreEqual(ListCell.Null, Execute<ListCell>("[]と[]を並列する。"));
			Assert.AreEqual(ListCell.Null, Execute<ListCell>("[1,2,3]と[]を並列する。"));
			Assert.AreEqual(ListCell.Null, Execute<ListCell>("[]と[4,5,6]を並列する。"));

			var expected = Execute<ListCell>("[(1:6),(2:7),(3:8)]である。");
			Assert.AreEqual(expected, Execute<ListCell>("[1,2,3]と[6,7,8]を並列する。"));
			Assert.AreEqual(expected, Execute<ListCell>("[1,2,3,4,5]と[6,7,8]を並列する。"));
			Assert.AreEqual(expected, Execute<ListCell>("[1,2,3]と[6,7,8,9,10]を並列する。"));
		}

		[TestMethod]
		public void 反復() {
			Assert.AreEqual(ListCell.Null, Execute<ListCell>("１を０だけ反復する。"));
			Assert.AreEqual(ListCell.Of(1), Execute<ListCell>("１を1だけ反復する。"));
			Assert.AreEqual(ListCell.Of(1, 1), Execute<ListCell>("１を２だけ反復する。"));
			Assert.AreEqual(ListCell.Of(1, 1,1), Execute<ListCell>("１を３だけ反復する。"));
		}
	}
}
