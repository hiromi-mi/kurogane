using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Kurogane.Test.Spec {

	[TestClass]
	public class BooleanTest : NoSideEffectTest {

		[TestMethod]
		public void 真偽リテラルがある() {
			Assert.IsTrue(Execute<bool>("真である。"));
			Assert.IsFalse(Execute<bool>("偽である。"));
		}

		[TestMethod]
		public void 演算可能() {
			Assert.IsFalse(Execute<bool>("(￢真)である。"));
			Assert.IsTrue(Execute<bool>("(￢偽)である。"));

			Assert.IsTrue(Execute<bool>("(真∧真)である。"));
			Assert.IsFalse(Execute<bool>("(真∧偽)である。"));
			Assert.IsFalse(Execute<bool>("(偽∧真)である。"));
			Assert.IsFalse(Execute<bool>("(偽∧偽)である。"));

			Assert.IsTrue(Execute<bool>("(真∨真)である。"));
			Assert.IsTrue(Execute<bool>("(真∨偽)である。"));
			Assert.IsTrue(Execute<bool>("(偽∨真)である。"));
			Assert.IsFalse(Execute<bool>("(偽∨偽)である。"));
		}

		[TestMethod]
		public void 無は偽でそれ以外は真として扱われる() {
			Assert.IsTrue(Execute<bool>("(！無)である。"));
			Assert.IsFalse(Execute<bool>("(！１)である。"));
			Assert.IsFalse(Execute<bool>("(！1.0)である。"));
			Assert.IsFalse(Execute<bool>("(！「A」)である。"));
			Assert.IsFalse(Execute<bool>("(！[1,2,3])である。"));

			Assert.AreEqual(2, Execute<int>("(１＆２)である。"));
			Assert.IsNull(Execute("(1.2＆無)である。"));
			Assert.IsFalse(Execute<bool>("(偽＆「A」)である。"));
			Assert.IsFalse(Execute<bool>("(偽＆無)である。"));

			Assert.AreEqual("A", Execute<string>("(「A」｜５)である。"));
			Assert.AreEqual(3, Execute<int>("(３｜無)である。"));
			Assert.AreEqual(2.5, Execute<double>("(偽｜2.5)である。"));
			Assert.IsNull(Execute("(偽｜無)である。"));
		}

		[TestMethod]
		public void OrよりAndの方が優先度が高い() {
			Assert.IsTrue(Execute<bool>("(真＆偽｜真)である。"));
			Assert.IsTrue(Execute<bool>("(真｜偽＆真)である。"));
		}
	}
}
