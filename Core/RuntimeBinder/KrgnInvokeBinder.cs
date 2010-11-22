using System;
using System.Dynamic;
using System.Linq.Expressions;
using Kurogane.Util;

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
			if (target.Value == null)
				return ThrowNullReferenceException(target);
			bool isDelegate = typeof(Delegate).IsAssignableFrom(funcType);
			if (isDelegate) {
				var mo = InvokeDelegate(target, args);
				if (mo != null)
					return mo;
			}

			var ctorInfo = typeof(InvalidOperationException).GetConstructor(new[] { typeof(string) });
			var exception = Expression.New(ctorInfo, Expression.Constant(target.LimitType + "は実行可能な型ではありません。"));
			return new DynamicMetaObject(
				Expression.Throw(exception, typeof(object)),
				BindingRestrictions.GetTypeRestriction(target.Expression, target.LimitType));
		}

		private DynamicMetaObject InvokeDelegate(DynamicMetaObject target, DynamicMetaObject[] args) {
			var funcType = target.LimitType;
			var typeArgs = funcType.GetGenericArguments();
			Type type = null;
			if (Expression.TryGetFuncType(typeArgs, out type)) {
				Expression[] argExprs = null;
				if (typeArgs.Length == args.Length + 1) {
					argExprs = ConvertArguments(args, typeArgs);
				}
				else if (typeArgs.Length == (2 + 1) && args.Length == 1) {
					// try inline tuple
					if (args[0].LimitType == typeof(Tuple<object, object>)) {
						var tupleExpr = BinderHelper.Wrap(args[0].Expression, typeof(Tuple<object, object>));
						var fst = ExpressionHelper.BetaReduction<Tuple<object, object>, object>(t => t.Item1, tupleExpr);
						var snd = ExpressionHelper.BetaReduction<Tuple<object, object>, object>(t => t.Item2, tupleExpr);
						argExprs = new Expression[]{
							BinderHelper.Wrap(fst, typeArgs[0]),
							BinderHelper.Wrap(snd, typeArgs[1])
						};
					}
				}
				if (argExprs != null) {
					return new DynamicMetaObject(
						Expression.Invoke(Expression.Convert(target.Expression, target.LimitType), argExprs),
						BindingRestrictions.GetTypeRestriction(target.Expression, target.LimitType));
				}
			}
			if (Expression.TryGetActionType(typeArgs, out type)) {
				if (typeArgs.Length == args.Length) {
					return new DynamicMetaObject(
						Expression.Block(
							Expression.Invoke(
								Expression.Convert(target.Expression, target.LimitType),
								ConvertArguments(args, typeArgs)),
							Expression.Constant(null)),
						BindingRestrictions.GetTypeRestriction(target.Expression, target.LimitType));
				}
			}
			return ThrowArgumentException(
				"引数の数が一致していません。",
				BindingRestrictions.GetTypeRestriction(target.Expression, target.LimitType));
		}

		private Expression[] ConvertArguments(DynamicMetaObject[] args, Type[] typeArgs) {
			var argsExpr = new Expression[args.Length];
			for (int i = 0; i < argsExpr.Length; i++) {
				argsExpr[i] = BinderHelper.Wrap(args[i].Expression, typeArgs[i]);
			}
			return argsExpr;
		}

		private DynamicMetaObject ThrowArgumentException(string message, BindingRestrictions restriction) {
			var ctorInfo = typeof(ArgumentException).GetConstructor(new[] { typeof(string) });
			var exception = Expression.New(ctorInfo, Expression.Constant(message));
			return new DynamicMetaObject(Expression.Throw(exception, typeof(object)), restriction);
		}

		private DynamicMetaObject ThrowNullReferenceException(DynamicMetaObject target) {
			var ctorInfo = typeof(NullReferenceException).GetConstructor(new[] { typeof(string) });
			var exception = Expression.New(ctorInfo, Expression.Constant("無は実行可能な型ではありません。"));
			var isNull = Expression.ReferenceEqual(target.Expression, Expression.Constant(null));
			return new DynamicMetaObject(
				Expression.Throw(exception, typeof(object)),
				BindingRestrictions.GetExpressionRestriction(isNull));
		}
	}
}
