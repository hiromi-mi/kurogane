using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Kurogane.Test.Spec {

	/// <summary>
	/// 等価比較のテスト
	/// </summary>
	[TestClass]
	public class EqualityTest {

		private Engine _engine = new Engine();

		[TestMethod]
		public void 無の等価比較() {
			Assert.IsTrue((bool)_engine.Execute("(無＝無)である。"));
			Assert.IsFalse((bool)_engine.Execute("(無≠無)である。"));
		}

		[TestMethod]
		public void 整数の等価比較() {
			Assert.IsTrue((bool)_engine.Execute("(1＝1)である。"));
			Assert.IsTrue((bool)_engine.Execute("(2≠3)である。"));
			Assert.IsFalse((bool)_engine.Execute("(4＝5)である。"));
			Assert.IsFalse((bool)_engine.Execute("(6≠6)である。"));
		}

		[TestMethod]
		public void 少数の等価比較() {
			Assert.IsTrue((bool)_engine.Execute("(1.1＝1.1)である。"));
			Assert.IsTrue((bool)_engine.Execute("(2.99≠3.0)である。"));
			Assert.IsFalse((bool)_engine.Execute("(4.99＝5.0)である。"));
			Assert.IsFalse((bool)_engine.Execute("(6.6≠6.6)である。"));
		}

		[TestMethod]
		public void 文字列の等価比較() {
			Assert.IsTrue((bool)_engine.Execute("(「あいう」＝「あいう」)である。"));
			Assert.IsTrue((bool)_engine.Execute("(「あいう」≠「いろは」)である。"));
			Assert.IsFalse((bool)_engine.Execute("(「ABC」＝「abc」)である。"));
			Assert.IsFalse((bool)_engine.Execute("(「甲乙丙」≠「甲乙丙」)である。"));
		}

		[TestMethod]
		public void リストの等価比較() {
			Assert.IsTrue((bool)_engine.Execute("([1,2,3]＝[1,2,3])である。"));
			Assert.IsTrue((bool)_engine.Execute("([1,2,3]≠[2,3,4])である。"));
			Assert.IsFalse((bool)_engine.Execute("([1,2,3,4]＝[3,2,1])である。"));
			Assert.IsFalse((bool)_engine.Execute("([-1]≠[-1])である。"));
		}

		[TestMethod]
		public void 整数と少数との比較() {
			Assert.IsTrue((bool)_engine.Execute("(1＝1.0)である。"));
			Assert.IsTrue((bool)_engine.Execute("(3≠2.99)である。"));
			Assert.IsFalse((bool)_engine.Execute("(4.99＝5)である。"));
			Assert.IsFalse((bool)_engine.Execute("(6.0≠6)である。"));
		}

		[TestMethod]
		public void 違う型の比較は偽を返す() {
			Assert.IsFalse((bool)_engine.Execute("(1＝無)である。"));
			Assert.IsFalse((bool)_engine.Execute("(1.0＝無)である。"));
			Assert.IsFalse((bool)_engine.Execute("(「A」＝無)である。"));
			Assert.IsFalse((bool)_engine.Execute("([1,2,3]＝無)である。"));
			Assert.IsFalse((bool)_engine.Execute("([1,2,3]＝「A」)である。"));
		}
	}
}
