﻿using System;
using System.Linq.Expressions;

namespace Kurogane.Util {

	internal static class ReflectionHelper {

		public static string PropertyName<T>(Expression<Func<T, object>> expr) {
			var body = expr.Body;
			var unary = body as UnaryExpression;
			if (unary != null)
				body = unary.Operand;
			return ((MemberExpression)body).Member.Name;
		}

		public static MemberExpression MemberExpression<T>(Expression<Func<T, object>> expr) {
			return (MemberExpression)expr.Body;
		}
	}
}
