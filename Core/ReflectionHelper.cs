using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;

namespace Kurogane {

	internal static class ReflectionHelper {

		public static string PropertyName<T>(Expression<Func<T, object>> expr) {
			return ((MemberExpression)expr.Body).Member.Name;
		}

		public static MemberExpression MemberExpression<T>(Expression<Func<T, object>> expr) {
			return (MemberExpression)expr.Body;
		}
	}
}
