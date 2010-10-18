using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;

namespace Kurogane.Util {

	internal static class ReflectionHelper {

		public static readonly IList<Type> TypeOfFunc = Array.AsReadOnly(new[]{
			typeof(Func<object>),
			typeof(Func<object,object>),

			typeof(Func<object,object,object>),
			typeof(Func<object,object,object,object>),

			typeof(Func<object,object,object,object,object>),
			typeof(Func<object,object,object,object,object,object>),

			typeof(Func<object,object,object,object,object,object,object>),
			typeof(Func<object,object,object,object,object,object,object,object>),

			typeof(Func<object,object,object,object,object,object,object,object,object>),
			typeof(Func<object,object,object,object,object,object,object,object,object,object>),
		});


		public static string PropertyName<T>(Expression<Func<T, object>> expr) {
			var body = expr.Body;
			var unary = body as UnaryExpression;
			if (unary != null)
				body = unary.Operand;
			var name = ((MemberExpression)body).Member.Name;

			return name;
		}

		public static MemberExpression MemberExpression<T>(Expression<Func<T, object>> expr) {
			return (MemberExpression)expr.Body;
		}
	}
}
