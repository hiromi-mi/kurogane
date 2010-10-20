using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Dynamic;
using System.Linq.Expressions;

namespace Kurogane.RuntimeBinder {

	public class KrgnInvokeMemberBinder : InvokeMemberBinder {

		private readonly BinderFactory _factory;

		public KrgnInvokeMemberBinder(string name, CallInfo callInfo, BinderFactory factory)
			: base(name, false, callInfo) {
				_factory = factory;
		}

		public override DynamicMetaObject FallbackInvoke(DynamicMetaObject target, DynamicMetaObject[] args, DynamicMetaObject errorSuggestion) {
			// 明示的に呼び出さない限り呼ばれない。
			throw new NotImplementedException();
		}

		public override DynamicMetaObject FallbackInvokeMember(DynamicMetaObject target, DynamicMetaObject[] args, DynamicMetaObject errorSuggestion) {
			var argsExpr = new Expression[args.Length + 1];
			argsExpr[0] = Expression.Dynamic(_factory.GetMemberBinder(this.Name), typeof(object), target.Expression);
			for (int i = 0; i < args.Length; i++)
				argsExpr[i + 1] = args[i].Expression;
			var callExpr = Expression.Dynamic(_factory.InvokeBinder(this.CallInfo), typeof(object), argsExpr);
			return new DynamicMetaObject(callExpr, BindingRestrictions.GetInstanceRestriction(target.Expression, target.Value));
		}
	}
}
