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
	public class ComparingTest {

		// 使い回し可能
		private Engine _engine = new Engine();

		[TestMethod]
		public void 整数の比較() {
			Assert.IsTrue((bool)_engine.Execute("(1<2)である。"));
			Assert.IsFalse((bool)_engine.Execute("(1<1)である。"));

			Assert.IsTrue((bool)_engine.Execute("(2≦2)である。"));
			Assert.IsFalse((bool)_engine.Execute("(3≦2)である。"));

			Assert.IsTrue((bool)_engine.Execute("(4≧4)である。"));
			Assert.IsFalse((bool)_engine.Execute("(4≧5)である。"));

			Assert.IsTrue((bool)_engine.Execute("(3＞2)である。"));
			Assert.IsFalse((bool)_engine.Execute("(3＞3)である。"));
		}

		[TestMethod]
		public void 少数の比較() {
			Assert.IsTrue((bool)_engine.Execute("(1.99<2.0)である。"));
			Assert.IsFalse((bool)_engine.Execute("(1.0<1.0)である。"));

			Assert.IsTrue((bool)_engine.Execute("(2.0≦2.00)である。"));
			Assert.IsFalse((bool)_engine.Execute("(2.001≦2.0)である。"));

			Assert.IsTrue((bool)_engine.Execute("(4.0≧4.0)である。"));
			Assert.IsFalse((bool)_engine.Execute("(4.99≧5.0)である。"));

			Assert.IsTrue((bool)_engine.Execute("(3.0＞2.99)である。"));
			Assert.IsFalse((bool)_engine.Execute("(3.0＞3.0)である。"));
		}

		[TestMethod]
		public void 文字列の比較() {
			Assert.IsTrue((bool)_engine.Execute("(「A」＜「B」)である。"));
			Assert.IsFalse((bool)_engine.Execute("(「C」＜「C」)である。"));

			Assert.IsTrue((bool)_engine.Execute("(「ABC」≦「ABC」)である。"));
			Assert.IsFalse((bool)_engine.Execute("(「AB」≦「AA」)である。"));

			Assert.IsTrue((bool)_engine.Execute("(「あ」≧「あ」)である。"));
			Assert.IsFalse((bool)_engine.Execute("(「い」≧「う」)である。"));

			Assert.IsTrue((bool)_engine.Execute("(「い」＞「あ」)である。"));
			Assert.IsFalse((bool)_engine.Execute("(「い」＞「い」)である。"));
		}

		[TestMethod]
		public void 整数と少数を比較() {
			Assert.IsTrue((bool)_engine.Execute("(1＜1.1)である。"));
			Assert.IsTrue((bool)_engine.Execute("(1≦1.0)である。"));
			Assert.IsTrue((bool)_engine.Execute("(1≧1.0)である。"));
			Assert.IsTrue((bool)_engine.Execute("(1＞0.9)である。"));
		}

		[TestMethod]
		public void 無同士を比較() {
			Assert.IsFalse((bool)_engine.Execute("(無＜無)である。"));
			Assert.IsTrue((bool)_engine.Execute("(無≦無)である。"));
			Assert.IsTrue((bool)_engine.Execute("(無≧無)である。"));
			Assert.IsFalse((bool)_engine.Execute("(無＞無)である。"));
		}

		[TestMethod]
		public void 無は何と比較しても小さいとみなされる() {
			Assert.IsTrue((bool)_engine.Execute("(無＜1)である。"));
			Assert.IsTrue((bool)_engine.Execute("(無≦1)である。"));
			Assert.IsFalse((bool)_engine.Execute("(無≧1)である。"));
			Assert.IsFalse((bool)_engine.Execute("(無＞1)である。"));

			Assert.IsFalse((bool)_engine.Execute("(1＜無)である。"));
			Assert.IsFalse((bool)_engine.Execute("(1≦無)である。"));
			Assert.IsTrue((bool)_engine.Execute("(1≧無)である。"));
			Assert.IsTrue((bool)_engine.Execute("(1＞無)である。"));

			Assert.IsTrue((bool)_engine.Execute("(無＜1.0)である。"));
			Assert.IsTrue((bool)_engine.Execute("(無＜「A」)である。"));
			Assert.IsTrue((bool)_engine.Execute("(無＜[1,2,3])である。"));

		}
	}
}
