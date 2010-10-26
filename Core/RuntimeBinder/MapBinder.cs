using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Dynamic;
using System.Linq.Expressions;

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
			return RuntimeBinderException.CreateMetaObject(
				target.LimitType + "を射影関数（それぞれ）に適用できません。",
				BindingRestrictions.GetTypeRestriction(target.Expression, target.LimitType));
		}
	}
}
