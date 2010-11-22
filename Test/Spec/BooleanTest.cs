using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Kurogane.Test.Spec {

	[TestClass]
	public class BooleanTest {

		private Engine _engine = new Engine();

		[TestMethod]
		public void 真偽リテラルがある() {
			Assert.IsTrue((bool)_engine.Execute("真である。"));
			Assert.IsFalse((bool)_engine.Execute("偽である。"));
		}

		[TestMethod]
		public void 演算可能() {
			Assert.IsFalse((bool)_engine.Execute("(￢真)である。"));
			Assert.IsTrue((bool)_engine.Execute("(￢偽)である。"));

			Assert.IsTrue((bool)_engine.Execute("(真∧真)である。"));
			Assert.IsFalse((bool)_engine.Execute("(真∧偽)である。"));
			Assert.IsFalse((bool)_engine.Execute("(偽∧真)である。"));
			Assert.IsFalse((bool)_engine.Execute("(偽∧偽)である。"));

			Assert.IsTrue((bool)_engine.Execute("(真∨真)である。"));
			Assert.IsTrue((bool)_engine.Execute("(真∨偽)である。"));
			Assert.IsTrue((bool)_engine.Execute("(偽∨真)である。"));
			Assert.IsFalse((bool)_engine.Execute("(偽∨偽)である。"));
		}

		[TestMethod]
		public void 無は偽でそれ以外は真として扱われる() {
			Assert.IsTrue((bool)_engine.Execute("(！無)である。"));
			Assert.IsFalse((bool)_engine.Execute("(！１)である。"));
			Assert.IsFalse((bool)_engine.Execute("(！1.0)である。"));
			Assert.IsFalse((bool)_engine.Execute("(！「A」)である。"));
			Assert.IsFalse((bool)_engine.Execute("(！[1,2,3])である。"));

			Assert.AreEqual(2, (int)_engine.Execute("(１＆２)である。"));
			Assert.IsNull(_engine.Execute("(1.2＆無)である。"));
			Assert.IsFalse((bool)_engine.Execute("(偽＆「A」)である。"));
			Assert.IsFalse((bool)_engine.Execute("(偽＆無)である。"));

			Assert.AreEqual("A", (string)_engine.Execute("(「A」｜５)である。"));
			Assert.AreEqual(3, (int)_engine.Execute("(３｜無)である。"));
			Assert.AreEqual(2.5, (double)_engine.Execute("(偽｜2.5)である。"));
			Assert.IsNull(_engine.Execute("(偽｜無)である。"));
		}

		[TestMethod]
		public void OrよりAndの方が優先度が高い() {
			Assert.IsTrue((bool)_engine.Execute("(真＆偽｜真)である。"));
			Assert.IsTrue((bool)_engine.Execute("(真｜偽＆真)である。"));
		}
	}
}
