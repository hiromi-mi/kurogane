using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Kurogane.Util;
using System.Linq.Expressions;

namespace Kurogane.Test.Util {

	[TestClass]
	public class ExpressionUtilTest {

		[TestMethod]
		public void Test1() {
			var param = Expression.Parameter(typeof(int), "foo");
			var expr = ExpressionHelper.BetaReduction((int a) => a + a, param);
			Assert.AreEqual(Expression.Add(param, param).ToString(), expr.ToString());
		}

		[TestMethod]
		public void Test2() {
			var param1 = Expression.Parameter(typeof(int), "foo");
			var param2 = Expression.Parameter(typeof(int), "bar");
			var expr = ExpressionHelper.BetaReduction((int a, int b) => a - b, param1, param2);
			Assert.AreEqual(Expression.Subtract(param1, param2).ToString(), expr.ToString());
		}

		[TestMethod]
		public void Test3() {
			var param1 = Expression.Parameter(typeof(int), "foo");
			var param2 = Expression.Parameter(typeof(int), "bar");
			var param3 = Expression.Parameter(typeof(bool), "baz");
			var actual = ExpressionHelper.BetaReduction((int a, int b, bool c) => a < b != c, param1, param2, param3);
			var expected = Expression.NotEqual(Expression.LessThan(param1, param2), param3);
			Assert.AreEqual(expected.ToString(), actual.ToString());
		}

		[TestMethod]
		public void TestLambda() {
			var param = Expression.Parameter(typeof(int), "foo");
			Expression<Func<int, int>> square = a => a * a;
			var lambda = (LambdaExpression)square;
			var expr = ExpressionHelper.BetaReduction(lambda, param);
			Assert.AreEqual(Expression.Multiply(param, param).ToString(), expr.ToString());
			Assert.AreNotEqual(Expression.Multiply(param, param).ToString(), lambda.ToString());
		}
	}
}
