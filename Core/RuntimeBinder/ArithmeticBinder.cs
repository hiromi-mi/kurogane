using System;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using Kurogane.Util;
using System.Diagnostics.Contracts;
using System.Numerics;
using System.Collections.ObjectModel;

namespace Kurogane.RuntimeBinder {

	/// <summary>
	/// 算術演算用のBinderクラス。
	/// 型が違う場合の組み込み数値計算はint<->double間でのみ可能。
	/// nullが含まれる場合，エラーを投げる。
	/// </summary>
	public class ArithmeticBinder : BinaryOperationBinder {

		/// <summary>
		/// キャストするパターン
		/// </summary>
		internal static readonly BinderHelper.TypeCastPattern[] CastPatterns = new[]{
			new BinderHelper.TypeCastPattern(typeof(double), typeof(decimal)),
			new BinderHelper.TypeCastPattern(typeof(int), typeof(long)),
			new BinderHelper.TypeCastPattern(typeof(long), typeof(BigInteger)),
		};

		private readonly string _name;

		public ArithmeticBinder(ExpressionType operation, string name)
			: base(operation) {
			Contract.Requires<ArgumentException>(
				operation == ExpressionType.Add ||
				operation == ExpressionType.Subtract ||
				operation == ExpressionType.Multiply ||
				operation == ExpressionType.Divide ||
				operation == ExpressionType.Modulo);

			_name = name;
		}

		public override DynamicMetaObject FallbackBinaryOperation(DynamicMetaObject target, DynamicMetaObject arg, DynamicMetaObject errorSuggestion) {
			if (target.Value == null || arg.Value == null)
				return BinderHelper.NullErrorOnOperation(_name, this.ReturnType, target, arg);

			try {
				if (target.LimitType == arg.LimitType) {
					var type = target.LimitType;
					var leftExpr = BinderHelper.Wrap(target.Expression, type);
					var rightExpr = BinderHelper.Wrap(arg.Expression, type);
					var checkedExpr = OverflowCheckingCalc(leftExpr, rightExpr);
					if (checkedExpr != null) {
						var bindings = BinderHelper.GetTypeRestriction(target, arg);
						return new DynamicMetaObject(checkedExpr, bindings);
					}
				}
				Expression expr = Expression.MakeBinary(
					this.Operation,
					BinderHelper.Wrap(target.Expression, target.LimitType),
					BinderHelper.Wrap(arg.Expression, arg.LimitType));
				return new DynamicMetaObject(
					BinderHelper.Wrap(expr, this.ReturnType),
					BinderHelper.GetTypeRestriction(target, arg));
			}
			catch (InvalidOperationException) {
				return DefferentType(target, arg);
			}
		}

		/// <summary>
		/// 暗黙のキャストを探す。
		/// </summary>
		private DynamicMetaObject DefferentType(DynamicMetaObject left, DynamicMetaObject right) {
			var val = TryCalcNumeric(left, right);
			if (val != null) return val;
			// 左辺のキャスト
			var mInfo = BinderHelper.GetImplicitCast(left.LimitType, right.LimitType);
			if (mInfo != null) {
				Expression expr = Expression.MakeBinary(this.Operation,
						BinderHelper.Wrap(left.Expression, left.LimitType, right.LimitType),
						Expression.Convert(right.Expression, right.LimitType));
				return new DynamicMetaObject(
					BinderHelper.Wrap(expr, this.ReturnType),
					BinderHelper.GetTypeRestriction(left, right));
			}
			// 右辺のキャスト
			mInfo = BinderHelper.GetImplicitCast(right.LimitType, left.LimitType);
			if (mInfo != null) {
				Expression expr = Expression.MakeBinary(this.Operation,
						Expression.Convert(left.Expression, left.LimitType),
						BinderHelper.Wrap(right.Expression, right.LimitType, left.LimitType));
				return new DynamicMetaObject(
					BinderHelper.Wrap(expr, this.ReturnType),
					BinderHelper.GetTypeRestriction(left, right));
			}
			return BinderHelper.NoResult(_name, this.ReturnType, left, right);
		}

		/// <summary>
		/// 整数と小数の演算を行う
		/// </summary>
		private DynamicMetaObject TryCalcNumeric(DynamicMetaObject left, DynamicMetaObject right) {
			Expression leftExpr = BinderHelper.LimitTypeConvert(left);
			Expression rightExpr = BinderHelper.LimitTypeConvert(right);
			Expression expr = null;
			foreach (var pattern in BinderHelper.CastPatterns) {
				if (leftExpr.Type == pattern.Narrow && rightExpr.Type == pattern.Wide) {
					var upperLeft = Expression.Convert(leftExpr, pattern.Wide);
					expr = OverflowCheckingCalc(upperLeft, rightExpr);
					break;
				}
				if (leftExpr.Type == pattern.Wide && rightExpr.Type == pattern.Narrow) {
					var upperRight = Expression.Convert(rightExpr, pattern.Wide);
					expr = OverflowCheckingCalc(leftExpr, upperRight);
					break;
				}
			}
			if (expr == null)
				return null;
			var bindings = BinderHelper.GetTypeRestriction(left, right);
			return new DynamicMetaObject(expr, bindings);
		}

		private Expression OverflowCheckingCalc(Expression left, Expression right) {
			var type = left.Type;
			if (left.Type != right.Type) return null;
			Type upper = CastPatterns.Where(p => p.Narrow == type).Select(p => p.Wide).SingleOrDefault();
			if (upper == null)
				return null;
			Expression expr = null;
			switch (this.Operation) {
			case ExpressionType.Add:
				expr = Expression.AddChecked(left, right);
				break;
			case ExpressionType.Subtract:
				expr = Expression.SubtractChecked(left, right);
				break;
			case ExpressionType.Multiply:
				expr = Expression.MultiplyChecked(left, right);
				break;
			}
			if (expr == null) {
				expr = Expression.MakeBinary(this.Operation, left, right);
				if (expr.Type != this.ReturnType)
					expr = Expression.Convert(expr, this.ReturnType);
				return expr;
			}
			if (expr.Type != this.ReturnType)
				expr = Expression.Convert(expr, this.ReturnType);
			var upperLeft = Expression.Convert(left, upper);
			var upperRight = Expression.Convert(right, upper);
			Expression upperCalc = Expression.MakeBinary(this.Operation, upperLeft, upperRight);
			if (upperCalc.Type != this.ReturnType)
				upperCalc = Expression.Convert(upperCalc, this.ReturnType);
			return Expression.TryCatch(expr, Expression.Catch(typeof(OverflowException), upperCalc));
		}

	}
}
