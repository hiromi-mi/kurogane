using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Dynamic;
using System.Linq.Expressions;
using Kurogane.Buildin;

namespace Kurogane.Compiler.Binders {

	/// <summary>
	/// 助詞付き関数呼び出しのBinder
	/// </summary>
	public class KrgnInvokeBinder : InvokeBinder {

		public KrgnInvokeBinder(CallInfo callInfo)
			: base(callInfo) {

		}

		public override DynamicMetaObject FallbackInvoke(DynamicMetaObject target, DynamicMetaObject[] args, DynamicMetaObject errorSuggestion) {
			if (!target.HasValue || args.Any(arg => !arg.HasValue)) {
				return Defer(target, args);
			}
			if (target.LimitType == typeof(KrgnFunc)) {
				var value = (KrgnFunc)target.Value;
				Expression[] procArgs = new Expression[value.Suffixes.Count];
				int offset = CallInfo.ArgumentCount - CallInfo.ArgumentNames.Count;
				for (int i = 0; i < procArgs.Length; i++) {
					var pp = value.Suffixes[i];
					for (int j = 0; j < CallInfo.ArgumentNames.Count; j++) {
						if (CallInfo.ArgumentNames[j] == pp) {
							// found post position argument
							procArgs[i] = Expression.Convert(args[j + offset].Expression, typeof(object));
							break;
						}
					}
					if (offset > 0 && procArgs[i] == null) {
						procArgs[i] = Expression.Convert(args[0].Expression, typeof(object));
					}
				}
				// argument complete
				Expression invokeExpr = Expression.Invoke(Expression.Constant(value.Function), procArgs);
				if (invokeExpr.Type == typeof(void)) {
					invokeExpr = Expression.Block(invokeExpr, Expression.Default(typeof(object)));
				}
				return new DynamicMetaObject(invokeExpr,
					BindingRestrictions.GetInstanceRestriction(target.Expression, target.Value));
			}
			throw new NotImplementedException();
		}
	}
}
