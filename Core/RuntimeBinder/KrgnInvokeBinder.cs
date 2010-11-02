using System;
using System.Dynamic;
using System.Linq.Expressions;

namespace Kurogane.RuntimeBinder {

	/// <summary>
	/// 助詞付き関数呼び出しのBinder
	/// </summary>
	public class KrgnInvokeBinder : InvokeBinder {

		public KrgnInvokeBinder(CallInfo callInfo)
			: base(callInfo) {
		}

		public override DynamicMetaObject FallbackInvoke(DynamicMetaObject target, DynamicMetaObject[] args, DynamicMetaObject errorSuggestion) {
			var funcType = target.LimitType;
			var typeArgs = funcType.GetGenericArguments();
			Type type = null;
			if (Expression.TryGetFuncType(typeArgs, out type)) {
				if (typeArgs.Length != args.Length + 1) {
					return ThrowArgumentException(
						"引数の数が一致していません。",
						BindingRestrictions.GetTypeRestriction(target.Expression, target.LimitType));
				}
				return new DynamicMetaObject(
					Expression.Invoke(
						Expression.Convert(target.Expression, target.LimitType),
						ConvertArguments(args, typeArgs)),
					BindingRestrictions.GetTypeRestriction(target.Expression, target.LimitType));
			}
			if (Expression.TryGetActionType(typeArgs, out type)) {
				if (typeArgs.Length != args.Length) {
					return ThrowArgumentException(
						"引数の数が一致していません。",
						BindingRestrictions.GetTypeRestriction(target.Expression, target.LimitType));
				}
				return new DynamicMetaObject(
					Expression.Block(
						Expression.Invoke(
							Expression.Convert(target.Expression, target.LimitType),
							ConvertArguments(args, typeArgs)),
						Expression.Constant(null)),
					BindingRestrictions.GetTypeRestriction(target.Expression, target.LimitType));
			}
			var ctorInfo = typeof(InvalidOperationException).GetConstructor(new[] { typeof(object) });
			var exception = Expression.New(ctorInfo, Expression.Constant(target.LimitType + "は実行可能な型ではありません。"));
			return new DynamicMetaObject(
				Expression.Throw(exception, typeof(object)),
				BindingRestrictions.GetTypeRestriction(target.Expression, target.LimitType));
		}

		private Expression[] ConvertArguments(DynamicMetaObject[] args, Type[] typeArgs) {
			var argsExpr = new Expression[args.Length];
			for (int i = 0; i < argsExpr.Length; i++) {
				if (args[i].Expression.Type == typeArgs[i])
					argsExpr[i] = args[i].Expression;
				else
					argsExpr[i] = Expression.Convert(args[i].Expression, typeArgs[i]);
			}
			return argsExpr;
		}

		private DynamicMetaObject ThrowArgumentException(string message, BindingRestrictions restriction) {
			var ctorInfo = typeof(ArgumentException).GetConstructor(new[] { typeof(string) });
			var exception = Expression.New(ctorInfo, Expression.Constant(message));
			return new DynamicMetaObject(Expression.Throw(exception, typeof(object)), restriction);
		}
	}
}
