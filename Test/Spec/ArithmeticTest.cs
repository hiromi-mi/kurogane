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
	public class ArithmeticTest {

		// 使いまわし可能
		private Engine _engine = new Engine();

		[TestMethod]
		public void 負の数() {
			Assert.AreEqual(-3, (int)_engine.Execute("(-3)である。"));
			Assert.AreEqual(-5.5, (double)_engine.Execute("(-5.5)である。"));
			Assert.AreEqual(0, (int)_engine.Execute("(-0)である。"));
			Assert.AreEqual(0.0, (double)_engine.Execute("(-0.0)である。"));
		}

		[TestMethod]
		public void 整数の計算() {
			Assert.AreEqual(3, (int)_engine.Execute("（１＋２）である。"));
			Assert.AreEqual(3, (int)_engine.Execute("（１９９－１９６）である。"));
			Assert.AreEqual(6, (int)_engine.Execute("（３×２）である。"));
			Assert.AreEqual(2, (int)_engine.Execute("（１１÷４）である。"));
			Assert.AreEqual(3, (int)_engine.Execute("（１１％４）である。"));
		}

		[TestMethod]
		public void 少数の計算() {
			Assert.AreEqual(5.5, (double)_engine.Execute("（2.3+3.2）である。"));
			Assert.AreEqual(2.75, (double)_engine.Execute("（４．２５－１．５）である。"));
			Assert.AreEqual(1.44, (double)_engine.Execute("（１．２×１．２）である。"));
			Assert.AreEqual(2.75, (double)_engine.Execute("（１１．０÷４．０）である。"));
			Assert.AreEqual(3.5, (double)_engine.Execute("（１１．５％４．０）である。"));
		}

		[TestMethod]
		public void 整数と少数の計算() {
			Assert.AreEqual(5.5, (double)_engine.Execute("（2.5+3）である。"));
			Assert.AreEqual(5.5, (double)_engine.Execute("（3+2.5）である。"));
			Assert.AreEqual(2.75, (double)_engine.Execute("（１１÷４．０）である。"));
		}

		[TestMethod]
		public void 無を計算した場合例外() {
			try {
				_engine.Execute("(無+1)である。");
				Assert.Fail();
			}
			catch (InvalidOperationException) { }

			try {
				_engine.Execute("(3-無)である。");
				Assert.Fail();
			}
			catch (InvalidOperationException) { }

			try {
				_engine.Execute("(無×無)である。");
				Assert.Fail();
			}
			catch (InvalidOperationException) { }
		}

		[TestMethod]
		public void 文字列を計算すると例外() {
			try {
				_engine.Execute("(「こんにちは」＋１)である。");
				Assert.Fail();
			}
			catch (InvalidOperationException) { }

			try {
				_engine.Execute("(「こんにちは」＋「さようなら」)である。");
				Assert.Fail();
			}
			catch (InvalidOperationException) { }

			try {
				_engine.Execute("(「こんにちは」×３)である。");
				Assert.Fail();
			}
			catch (InvalidOperationException) { }
		}

		[TestMethod]
		public void リストを計算すると例外() {
			try {
				_engine.Execute("([1,2,3] + [4,5,6])である。");
				Assert.Fail();
			}
			catch (InvalidOperationException) { }

			try {
				_engine.Execute("([1,2,3,4,5]×3)である。");
				Assert.Fail();
			}
			catch (InvalidOperationException) { }
		}

		[TestMethod]
		public void 複雑な計算式を計算() {
			Assert.AreEqual(9, (int)_engine.Execute("(2+3+4)である。"));
			Assert.AreEqual(14, (int)_engine.Execute("(2+3*4)である。"));
			Assert.AreEqual(10, (int)_engine.Execute("(2*3+4)である。"));
			Assert.AreEqual(20, (int)_engine.Execute("((2+3)*4)である。"));
			Assert.AreEqual(14, (int)_engine.Execute("(2*(3+4))である。"));
		}
	}
}
