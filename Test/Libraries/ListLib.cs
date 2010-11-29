using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Kurogane.Test.Libraries {

	[TestClass]
	public class ListLib : NoSideEffectTest {

		[TestMethod]
		public void 平坦化() {
			Assert.AreEqual(
				ListCell.Null,
				Execute<ListCell>("無を平坦化する。"));
			Assert.AreEqual(
				ListCell.Of(3),
				Execute<ListCell>("3を平坦化する。"));
			Assert.AreEqual(
				ListCell.Of(3, 5, 7),
				Execute<ListCell>("３と５と７を平坦化する。"));
			Assert.AreEqual(
				ListCell.Of(1, 2, 3),
				Execute<ListCell>("[1,2,3]を平坦化する。"));

			Assert.AreEqual(
				ListCell.Of(0, 1, 2, 3, 4, 5, 6),
				Execute<ListCell>("[0,[],[1],[2,3],[4,[],[5,[6]]]]を平坦化する。"));
			Assert.AreEqual(
				ListCell.Of(1, 2, 3, 4, 5, 6, 7, 8, 9),
				Execute<ListCell>("(((1:2):(3:4:5)):(6:(7:8):9))を平坦化する。"));
		}

	}
}
