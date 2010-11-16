using System;
using System.Text;
using System.Collections.Generic;
using System.Numerics;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Kurogane.Test.Spec {
	/// <summary>
	/// UnitTest1 の概要の説明
	/// </summary>
	[TestClass]
	public class ArithmeticTest {
		private Engine _engine;

		[TestInitialize]
		public void Init() {
			_engine = new Engine();
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
	}
}
