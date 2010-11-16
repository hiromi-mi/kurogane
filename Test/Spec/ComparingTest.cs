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
		private Engine _engine;

		[TestInitialize]
		public void Init() {
			_engine = new Engine();
		}

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

	}
}
