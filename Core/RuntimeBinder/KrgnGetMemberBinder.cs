using System;
using System.Diagnostics.Contracts;
using System.Dynamic;
using System.Linq.Expressions;
using System.Reflection;
using Kurogane.Util;

namespace Kurogane.RuntimeBinder {

	public class KrgnGetMemberBinder : GetMemberBinder {

		public KrgnGetMemberBinder(string name)
			: base(name, true) {
		}

		public override DynamicMetaObject FallbackGetMember(DynamicMetaObject target, DynamicMetaObject errorSuggestion) {
			return
				DefaultGetMember(target) ??
				SearchAlias(target) ??
				NotFound(target);
		}

		/// <summary>
		/// 通常のプロパティを実行する。
		/// </summary>
		private DynamicMetaObject DefaultGetMember(DynamicMetaObject target) {
			Contract.Requires<ArgumentNullException>(target != null);
			var propInfo = target.LimitType.GetProperty(Name);
			if (propInfo != null && propInfo.CanRead) {
				var expr = Expression.Property(target.Expression, propInfo);
				var rest = BindingRestrictions.GetTypeRestriction(target.Expression, target.LimitType);
				return new DynamicMetaObject(expr, rest);
			}
			return null;
		}

		/// <summary>
		/// 登録されたAliasを検索して実行する。
		/// </summary>
		private DynamicMetaObject SearchAlias(DynamicMetaObject target) {
			Contract.Requires<ArgumentNullException>(target != null);
			var cacher = MetaObjectLoader.GetAlias(target.LimitType);
			if (cacher == null)
				return null;
			var memInfo = cacher.GetMemberInfo(this.Name);
			var propInfo = memInfo as PropertyInfo;
			if (propInfo != null && propInfo.CanRead) {
				return MakeDynamicMetaObject(target, propInfo);
			}
			return null;
		}

		/// <summary>
		/// 見つからない例外を投げる。
		/// </summary>
		/// <param name="target"></param>
		/// <returns></returns>
		private DynamicMetaObject NotFound(DynamicMetaObject target) {
			Contract.Requires<ArgumentNullException>(target != null);
			var ctorInfo = typeof(PropertyNotFoundException).GetConstructor(
				new[] { typeof(string), typeof(PropertyAccessMode) });
			var exception = Expression.New(ctorInfo, Expression.Constant(this.Name), Expression.Constant(PropertyAccessMode.Read));
			//exception = ExpressionHelper.BetaReduction<string, PropertyAccessMode, PropertyNotFoundException>(
			//    ((msg, mode) => new PropertyNotFoundException(msg, mode)),
			//    Expression.Constant(this.Name),
			//     Expression.Constant(PropertyAccessMode.Read));
			var expr = Expression.Throw(exception, this.ReturnType);
			var rest = BindingRestrictions.GetTypeRestriction(target.Expression, target.LimitType);
			return new DynamicMetaObject(expr, rest);
		}

		private DynamicMetaObject MakeDynamicMetaObject(DynamicMetaObject target, PropertyInfo propInfo) {
			Contract.Requires<ArgumentNullException>(target != null);
			Contract.Requires<ArgumentNullException>(propInfo != null);
			Contract.Requires<ArgumentException>(propInfo.CanRead == true);
			Contract.Ensures(Contract.Result<DynamicMetaObject>() != null);

			var targetExpr = BinderHelper.Wrap(target.Expression, target.LimitType);
			var propAccess = Expression.Property(targetExpr, propInfo);
			var expr = BinderHelper.Wrap(propAccess, this.ReturnType);
			var rest = BindingRestrictions.GetTypeRestriction(target.Expression, target.LimitType);
			return new DynamicMetaObject(expr, rest);
		}
	}
}
