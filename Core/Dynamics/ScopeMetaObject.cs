using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Dynamic;
using System.Linq.Expressions;

namespace Kurogane.Dynamic {

	internal class ScopeMetaObject : DynamicMetaObject {

		public ScopeMetaObject(Scope self, Expression expr)
			: base(expr, BindingRestrictions.GetExpressionRestriction(Expression.TypeIs(expr, typeof(Scope))), self) {
		}

		public override DynamicMetaObject BindGetMember(GetMemberBinder binder) {
			return new DynamicMetaObject(
				Expression.Call(
					Expression.Convert(this.Expression, typeof(Scope)),
					typeof(Scope).GetMethod("GetVariable"),
					Expression.Constant(binder.Name)),
				this.Restrictions);
		}

		public override DynamicMetaObject BindSetMember(SetMemberBinder binder, DynamicMetaObject value) {
			return new DynamicMetaObject(
				Expression.Call(
					Expression.Convert(this.Expression, typeof(Scope)),
					typeof(Scope).GetMethod("SetVariable"),
					Expression.Constant(binder.Name),
					value.Expression),
				this.Restrictions);
		}
	}
}
