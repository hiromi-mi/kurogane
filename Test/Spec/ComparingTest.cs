using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Kurogane.Test.Spec {
	
	/// <summary>
	/// 比較演算のテスト
	/// </summary>
	[TestClass]
	public class ComparingTest : NoSideEffectTest {

		[TestMethod]
		public void 整数の比較() {
			Assert.IsTrue(Execute<bool>("(1<2)である。"));
			Assert.IsFalse(Execute<bool>("(1<1)である。"));

			Assert.IsTrue(Execute<bool>("(2≦2)である。"));
			Assert.IsFalse(Execute<bool>("(3≦2)である。"));

			Assert.IsTrue(Execute<bool>("(4≧4)である。"));
			Assert.IsFalse(Execute<bool>("(4≧5)である。"));

			Assert.IsTrue(Execute<bool>("(3＞2)である。"));
			Assert.IsFalse(Execute<bool>("(3＞3)である。"));
		}

		[TestMethod]
		public void 少数の比較() {
			Assert.IsTrue(Execute<bool>("(1.99<2.0)である。"));
			Assert.IsFalse(Execute<bool>("(1.0<1.0)である。"));

			Assert.IsTrue(Execute<bool>("(2.0≦2.00)である。"));
			Assert.IsFalse(Execute<bool>("(2.001≦2.0)である。"));

			Assert.IsTrue(Execute<bool>("(4.0≧4.0)である。"));
			Assert.IsFalse(Execute<bool>("(4.99≧5.0)である。"));

			Assert.IsTrue(Execute<bool>("(3.0＞2.99)である。"));
			Assert.IsFalse(Execute<bool>("(3.0＞3.0)である。"));
		}

		[TestMethod]
		public void 文字列の比較() {
			Assert.IsTrue(Execute<bool>("(「A」＜「B」)である。"));
			Assert.IsFalse(Execute<bool>("(「C」＜「C」)である。"));

			Assert.IsTrue(Execute<bool>("(「ABC」≦「ABC」)である。"));
			Assert.IsFalse(Execute<bool>("(「AB」≦「AA」)である。"));

			Assert.IsTrue(Execute<bool>("(「あ」≧「あ」)である。"));
			Assert.IsFalse(Execute<bool>("(「い」≧「う」)である。"));

			Assert.IsTrue(Execute<bool>("(「い」＞「あ」)である。"));
			Assert.IsFalse(Execute<bool>("(「い」＞「い」)である。"));
		}

		[TestMethod]
		public void 整数と少数を比較() {
			Assert.IsTrue(Execute<bool>("(1＜1.1)である。"));
			Assert.IsTrue(Execute<bool>("(1≦1.0)である。"));
			Assert.IsTrue(Execute<bool>("(1≧1.0)である。"));
			Assert.IsTrue(Execute<bool>("(1＞0.9)である。"));
		}

		[TestMethod]
		public void 無同士を比較() {
			Assert.IsFalse(Execute<bool>("(無＜無)である。"));
			Assert.IsTrue(Execute<bool>("(無≦無)である。"));
			Assert.IsTrue(Execute<bool>("(無≧無)である。"));
			Assert.IsFalse(Execute<bool>("(無＞無)である。"));
		}

		[TestMethod]
		public void 無は何と比較しても小さいとみなされる() {
			Assert.IsTrue(Execute<bool>("(無＜1)である。"));
			Assert.IsTrue(Execute<bool>("(無≦1)である。"));
			Assert.IsFalse(Execute<bool>("(無≧1)である。"));
			Assert.IsFalse(Execute<bool>("(無＞1)である。"));

			Assert.IsFalse(Execute<bool>("(1＜無)である。"));
			Assert.IsFalse(Execute<bool>("(1≦無)である。"));
			Assert.IsTrue(Execute<bool>("(1≧無)である。"));
			Assert.IsTrue(Execute<bool>("(1＞無)である。"));

			Assert.IsTrue(Execute<bool>("(無＜1.0)である。"));
			Assert.IsTrue(Execute<bool>("(無＜「A」)である。"));
			Assert.IsTrue(Execute<bool>("(無＜[1,2,3])である。"));

		}
	}
}
