using System;
using System.Text;
using System.Collections.Generic;
using System.Numerics;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Kurogane.Test.Spec {

	/// <summary>
	/// 四則演算のテスト
	/// </summary>
	[TestClass]
	public class ArithmeticTest : NoSideEffectTest {

		[TestMethod]
		public void 負の数() {
			Assert.AreEqual(-3, Execute<int>("(-3)である。"));
			Assert.AreEqual(-5.5, Execute<double>("(-5.5)である。"));
			Assert.AreEqual(0, Execute<int>("(-0)である。"));
			Assert.AreEqual(0.0, Execute<double>("(-0.0)である。"));
		}

		[TestMethod]
		public void 整数の計算() {
			Assert.AreEqual(3, Execute<int>("（１＋２）である。"));
			Assert.AreEqual(3, Execute<int>("（１９９－１９６）である。"));
			Assert.AreEqual(6, Execute<int>("（３×２）である。"));
			Assert.AreEqual(2, Execute<int>("（１１÷４）である。"));
			Assert.AreEqual(3, Execute<int>("（１１％４）である。"));
		}

		[TestMethod]
		public void 少数の計算() {
			Assert.AreEqual(5.5, Execute<double>("（2.3+3.2）である。"));
			Assert.AreEqual(2.75, Execute<double>("（４．２５－１．５）である。"));
			Assert.AreEqual(1.44, Execute<double>("（１．２×１．２）である。"));
			Assert.AreEqual(2.75, Execute<double>("（１１．０÷４．０）である。"));
			Assert.AreEqual(3.5, Execute<double>("（１１．５％４．０）である。"));
		}

		[TestMethod]
		public void 整数と少数の計算() {
			Assert.AreEqual(5.5, Execute<double>("（2.5+3）である。"));
			Assert.AreEqual(5.5, Execute<double>("（3+2.5）である。"));
			Assert.AreEqual(2.75, Execute<double>("（１１÷４．０）である。"));
		}

		[TestMethod]
		public void 無を計算した場合例外() {
			try {
				Execute("(無+1)である。");
				Assert.Fail();
			}
			catch (InvalidOperationException) { }

			try {
				Execute("(3-無)である。");
				Assert.Fail();
			}
			catch (InvalidOperationException) { }

			try {
				Execute("(無×無)である。");
				Assert.Fail();
			}
			catch (InvalidOperationException) { }
		}

		[TestMethod]
		public void 文字列を計算すると例外() {
			try {
				Execute("(「こんにちは」＋１)である。");
				Assert.Fail();
			}
			catch (InvalidOperationException) { }

			try {
				Execute("(「こんにちは」＋「さようなら」)である。");
				Assert.Fail();
			}
			catch (InvalidOperationException) { }

			try {
				Execute("(「こんにちは」×３)である。");
				Assert.Fail();
			}
			catch (InvalidOperationException) { }
		}

		[TestMethod]
		public void リストを計算すると例外() {
			try {
				Execute("([1,2,3] + [4,5,6])である。");
				Assert.Fail();
			}
			catch (InvalidOperationException) { }

			try {
				Execute("([1,2,3,4,5]×3)である。");
				Assert.Fail();
			}
			catch (InvalidOperationException) { }
		}

		[TestMethod]
		public void 複雑な計算式を計算() {
			Assert.AreEqual(9, Execute<int>("(2+3+4)である。"));
			Assert.AreEqual(14, Execute<int>("(2+3*4)である。"));
			Assert.AreEqual(10, Execute<int>("(2*3+4)である。"));
			Assert.AreEqual(20, Execute<int>("((2+3)*4)である。"));
			Assert.AreEqual(14, Execute<int>("(2*(3+4))である。"));
		}
	}
}
