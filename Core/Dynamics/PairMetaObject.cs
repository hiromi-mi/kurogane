using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Dynamic;
using System.Linq.Expressions;

namespace Kurogane.Dynamic {

	class PairMetaObject : DynamicMetaObject {

		private static readonly Type pairType = typeof(Tuple<object, object>);

		public PairMetaObject(Tuple<object, object> pair, Expression expr)
			: base(expr, BindingRestrictions.Empty, pair) {
		}

		public override DynamicMetaObject BindGetMember(GetMemberBinder binder) {
			string propName = null;
			switch (binder.Name) {
			case ConstantNames.Head: propName = "Item1"; break;
			case ConstantNames.Tail: propName = "Item2"; break;
			}
			if (propName != null)
				return new DynamicMetaObject(
					Expression.Property(Expression.Convert(base.Expression, pairType), propName),
					BindingRestrictions.GetExpressionRestriction(Expression.TypeIs(base.Expression, pairType)));
			return base.BindGetMember(binder);
		}
	}
}
