using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using System.Dynamic;

namespace Kurogane.Compiler.Binders {

	public static class BinderExtension {

		public static string PropertyName<TArg, TProp>(
			this DynamicMetaObjectBinder binder,
			Expression<Func<TArg, TProp>> propExpr) {
			return ((MemberExpression)propExpr.Body).Member.Name;
		}
	}
}
