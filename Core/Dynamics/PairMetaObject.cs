using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Dynamic;
using System.Linq.Expressions;
using Kurogane.Types;

namespace Kurogane.Dynamics {

	class PairMetaObject : DynamicMetaObject {

		public PairMetaObject(Pair pair, Expression expr)
			: base(expr, BindingRestrictions.Empty, pair) {
		}

		public override DynamicMetaObject BindGetMember(GetMemberBinder binder) {
			string propName = null;
			switch (binder.Name) {
			case ConstantNames.Head: propName = Pair.txtHead; break;
			case ConstantNames.Tail: propName = Pair.txtTail; break;
			}
			if (propName != null)
				return new DynamicMetaObject(
					Expression.Property(Expression.Convert(base.Expression, typeof(Pair)), propName),
					BindingRestrictions.GetExpressionRestriction(Expression.TypeIs(base.Expression, typeof(Pair))));
			return base.BindGetMember(binder);
		}
	}
}
