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
			if (target.Value == null || arg.Value == null)
				return FallbackOnNull(target, arg);

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

		private DynamicMetaObject FallbackOnNull(DynamicMetaObject target, DynamicMetaObject arg) {
			var nullExpr = Expression.Constant(null);
			var restL = target.Value == null
				? Expression.Equal(target.Expression, nullExpr)
				: Expression.NotEqual(target.Expression, nullExpr);
			var restR = arg.Value == null
				? Expression.Equal(arg.Expression, nullExpr)
				: Expression.NotEqual(arg.Expression, nullExpr);

			var rest = BindingRestrictions.GetExpressionRestriction(Expression.And(restL, restR));
			bool result = false;
			switch (this.Operation) {
			case ExpressionType.Equal: result = (target.Value == arg.Value); break;
			case ExpressionType.NotEqual: result = (target.Value != arg.Value); break;

			default:
				if (target.Value == null)
					throw new NullReferenceException("left hand is null");
				else
					throw new NullReferenceException("right hand is null");
			}
			return new DynamicMetaObject(Expression.Convert(Expression.Constant(result), typeof(object)), rest);
		}
	}

	/// <summary>
	/// 一般的な単項演算をbindするクラス
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
