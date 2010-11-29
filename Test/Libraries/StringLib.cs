using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Kurogane.Test.Libraries {

	[TestClass]
	public class StringLib : NoSideEffectTest {

		[TestMethod]
		public void 文字分割() {
			Assert.AreEqual(ListCell.Null, Execute<ListCell>("無を文字分割する。"));
			Assert.AreEqual(ListCell.Null, Execute<ListCell>("「」を文字分割する。"));
			Assert.AreEqual(
				ListCell.Of("あ"),
				Execute<ListCell>("「あ」を文字分割する。"));
			Assert.AreEqual(
				ListCell.Of("あ", "い", "う", "え", "お"),
				Execute<ListCell>("「あいうえお」を文字分割する。"));
		}
	}
}
