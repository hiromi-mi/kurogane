using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Dynamic;
using System.Linq.Expressions;
using Kurogane.Util;

namespace Kurogane.RuntimeBinder {

	public class MapBinder : BinaryOperationBinder {

		public MapBinder()
			: base(ExpressionType.Multiply) {
		}

		/// <summary>
		/// Map関数を呼び出す。
		/// </summary>
		/// <param name="target">関数</param>
		/// <param name="arg">リスト</param>
		/// <param name="errorSuggestion"></param>
		/// <returns>リスト</returns>
		public override DynamicMetaObject FallbackBinaryOperation(DynamicMetaObject target, DynamicMetaObject arg, DynamicMetaObject errorSuggestion) {
			var funcType = typeof(Func<object, object>);
			if (target.LimitType == funcType) {
				var mInfo = typeof(Enumerator).GetMethod("Map", new[] { funcType, typeof(object) });
				return new DynamicMetaObject(
					Expression.Call(mInfo, Expression.Convert(target.Expression, funcType), arg.Expression),
					BindingRestrictions.GetTypeRestriction(target.Expression, funcType));
			}
			funcType = typeof(SuffixFunc<Func<object, object>>);
			if (target.LimitType == funcType) {
				var mInfo = typeof(Enumerator).GetMethod("Map", new[] { funcType, typeof(object) });
				return new DynamicMetaObject(
					Expression.Call(mInfo, Expression.Convert(target.Expression, funcType), arg.Expression),
					BindingRestrictions.GetTypeRestriction(target.Expression, funcType));
			}
			return ThrowArgumentException(
				target.LimitType + "を用いて射影（それぞれ）できません。",
				BindingRestrictions.GetTypeRestriction(target.Expression, target.LimitType));
		}

		private static DynamicMetaObject ThrowArgumentException(string message, BindingRestrictions restrictions) {
			var ctorInfo = typeof(InvalidOperationException).GetConstructor(new[] { typeof(string) });
			return new DynamicMetaObject(
				Expression.Throw(Expression.New(ctorInfo, Expression.Constant(message)), typeof(object)),
				restrictions);
		}
	}
}
