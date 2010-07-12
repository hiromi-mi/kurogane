using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq.Expressions;
using System.Linq;
using System.Text;

namespace Kurogane.Compiler.Binders {

	/// <summary>
	/// 一般的な二項演算をbindするクラス
	/// </summary>
	public class KrgnBinaryExpressionBinder : BinaryOperationBinder {

		private static Dictionary<ExpressionType, KrgnBinaryExpressionBinder> cache = new Dictionary<ExpressionType, KrgnBinaryExpressionBinder>();
		public static KrgnBinaryExpressionBinder Create(ExpressionType type) {
			if (cache.ContainsKey(type)) return cache[type];
			var binder = new KrgnBinaryExpressionBinder(type);
			cache[type] = binder;
			return binder;
		}

		private KrgnBinaryExpressionBinder(ExpressionType type)
			: base(type) {

		}

		public override DynamicMetaObject FallbackBinaryOperation(DynamicMetaObject target, DynamicMetaObject arg, DynamicMetaObject errorSuggestion) {

			var restrictions = target.Restrictions
				.Merge(arg.Restrictions)
				.Merge(BindingRestrictions.GetTypeRestriction(
					target.Expression, target.LimitType))
				.Merge(BindingRestrictions.GetTypeRestriction(
					arg.Expression, arg.LimitType));
			if (target.LimitType == arg.LimitType) {
				return new DynamicMetaObject(
					Expression.Convert(
						Expression.MakeBinary(
							this.Operation,
							Expression.Convert(target.Expression, target.LimitType),
							Expression.Convert(arg.Expression, arg.LimitType)),
						typeof(object)),
					restrictions
				);
			}
			else {
				return new DynamicMetaObject(
					Expression.Convert(
						Expression.MakeBinary(
							this.Operation,
							Expression.Convert(target.Expression, typeof(object)),
							Expression.Convert(arg.Expression, typeof(object))),
						typeof(object)),
					restrictions
				);
			}
		}
	}

	/// <summary>
	/// 一般的な二項演算をbindするクラス
	/// </summary>
	public class KrgnUnaryExpressionBinder : UnaryOperationBinder {

		private static Dictionary<ExpressionType, KrgnUnaryExpressionBinder> cache = new Dictionary<ExpressionType, KrgnUnaryExpressionBinder>();
		public static KrgnUnaryExpressionBinder Create(ExpressionType type) {
			if (cache.ContainsKey(type)) return cache[type];
			var binder = new KrgnUnaryExpressionBinder(type);
			cache[type] = binder;
			return binder;
		}

		private KrgnUnaryExpressionBinder(ExpressionType type)
			: base(type) {

		}

		public override DynamicMetaObject FallbackUnaryOperation(DynamicMetaObject target, DynamicMetaObject errorSuggestion) {
			return new DynamicMetaObject(
				Expression.MakeUnary(this.Operation, Expression.Convert(target.Expression, target.LimitType), typeof(object)),
				target.Restrictions.Merge(BindingRestrictions.GetTypeRestriction(target.Expression, target.LimitType)));
		}
	}
}
