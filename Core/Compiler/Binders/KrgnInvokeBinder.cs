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
				//var dexpr = Expression.Constant(value.Function);
				//Expression invokeExpr = Expression.Invoke(dexpr, procArgs);
				//if (invokeExpr.Type == typeof(void)) {
				//    invokeExpr = Expression.Block(invokeExpr, Expression.Default(typeof(object)));
				//}
				//return new DynamicMetaObject(invokeExpr,
				//    BindingRestrictions.GetInstanceRestriction(target.Expression, target.Value));

				Expression castedTarget = Expression.Convert(target.Expression, target.LimitType);
				Expression<Func<KrgnFunc, Delegate>> getMemberFunc = f => f.Function;
				var propName = ((MemberExpression)getMemberFunc.Body).Member.Name;
				Expression delExpr = Expression.Property(castedTarget, propName);
				switch (value._suffixes.Length) {
				case 0: delExpr = Expression.Convert(delExpr, typeof(Func<object>)); break;
				case 1: delExpr = Expression.Convert(delExpr, typeof(Func<object, object>)); break;
				case 2: delExpr = Expression.Convert(delExpr, typeof(Func<object, object, object>)); break;
				case 3: delExpr = Expression.Convert(delExpr, typeof(Func<object, object, object, object>)); break;
				case 4: delExpr = Expression.Convert(delExpr, typeof(Func<object, object, object, object, object>)); break;
				}
				Expression invokeExpr = Expression.Invoke(delExpr, procArgs);

				Expression<Func<KrgnFunc, ICollection<string>>> getMemberSuffix = f => f._suffixes;
				propName = ((MemberExpression)getMemberSuffix.Body).Member.Name;
				Expression restrictExpr = null;
				Expression suffProperty = Expression.PropertyOrField(castedTarget, propName);
				restrictExpr = Expression.Equal(
					Expression.PropertyOrField(suffProperty, "Length"),
					Expression.Constant(value.Suffixes.Count));
				if (value.Suffixes.Count > 0) {
					Expression[] sufExprs = new Expression[value.Suffixes.Count];
					for (int i = 0; i < sufExprs.Length; i++) {
						var left = Expression.ArrayIndex(suffProperty, Expression.Constant(0));
						var right = Expression.Constant(value.Suffixes[i]);
						sufExprs[i] = Expression.Equal(left, right);
					}
					if (sufExprs.Length == 1) {
						restrictExpr = Expression.AndAlso(restrictExpr, sufExprs[0]);
					}
					else {
						Expression expr = Expression.AndAlso(sufExprs[0], sufExprs[1]);
						for (int i = 2; i < sufExprs.Length; i++)
							expr = Expression.AndAlso(expr, sufExprs[i]);
						restrictExpr = Expression.AndAlso(restrictExpr, expr);
					}
				}
				var rule = Expression.Lambda<Func<KrgnFunc, bool>>(restrictExpr, (ParameterExpression)target.Expression);
				var result = rule.Compile()(value);
				return new DynamicMetaObject(invokeExpr, BindingRestrictions.GetExpressionRestriction(restrictExpr));
			}
			throw new NotImplementedException();
		}
	}
}
