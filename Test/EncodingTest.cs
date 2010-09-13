using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Kurogane.Test {

	[TestClass]
	public class EncodingTest {

		readonly Encoding JIS = Encoding.GetEncoding("iso-2022-jp");
		readonly Encoding S_JIS = Encoding.GetEncoding("shift-jis");
		readonly Encoding EUC_JP = Encoding.GetEncoding("euc-jp");
		readonly Encoding UTF8 = Encoding.UTF8;
		readonly Encoding UTF16LE = Encoding.Unicode;

		[TestMethod]
		public void JISを判定できる() {
			var txt = "ソース";
			byte[] bytes = JIS.GetBytes(txt);
			var enc = Util.DelectEncoding(bytes);
			Assert.AreEqual(JIS, enc);
		}

		[TestMethod]
		public void S_JISを判定できる() {
			var txt = "ソース";
			byte[] bytes = S_JIS.GetBytes(txt);
			var enc = Util.DelectEncoding(bytes);
			Assert.AreEqual(S_JIS, enc);
		}

		[TestMethod]
		public void EUCを判定できる() {
			var txt = "EUCですよ";
			byte[] bytes = EUC_JP.GetBytes(txt);
			var enc = Util.DelectEncoding(bytes);
			Assert.AreEqual(EUC_JP, enc);
		}

		[TestMethod]
		public void UTF8を判定できる() {
			var txt = "UTF8か～";
			byte[] bytes = UTF8.GetBytes(txt);
			var enc = Util.DelectEncoding(bytes);
			Assert.AreEqual(UTF8, enc);
		}
	}
}
