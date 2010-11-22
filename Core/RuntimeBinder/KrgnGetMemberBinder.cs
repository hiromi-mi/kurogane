using System;
using System.Diagnostics.Contracts;
using System.Dynamic;
using System.Linq.Expressions;
using System.Reflection;

namespace Kurogane.RuntimeBinder {

	public class KrgnGetMemberBinder : GetMemberBinder {

		public KrgnGetMemberBinder(string name)
			: base(name, true) {
		}

		public override DynamicMetaObject FallbackGetMember(DynamicMetaObject target, DynamicMetaObject errorSuggestion) {
			return
				SearchAlias(target) ??
				DefaultGetMember(target);
		}

		private DynamicMetaObject SearchAlias(DynamicMetaObject target) {
			var cacher = MetaObjectLoader.GetAlias(target.LimitType);
			if (cacher == null)
				return null;
			var propInfo = cacher.GetMemberInfo(this.Name) as PropertyInfo;
			if (propInfo != null && propInfo.CanRead) {
				return MakeDynamicMetaObject(target, propInfo);
			}
			return null;
		}

		private DynamicMetaObject DefaultGetMember(DynamicMetaObject target) {
			Expression expr;
			var propInfo = target.LimitType.GetProperty(Name);
			if (propInfo != null && propInfo.CanRead) {
				expr = Expression.Property(target.Expression, propInfo);
			}
			else {
				var ctorInfo = typeof(PropertyNotFoundException).GetConstructor(
					new[] { typeof(string), typeof(PropertyAccessMode) });
				expr = Expression.Throw(
					Expression.New(ctorInfo, Expression.Constant(this.Name), Expression.Constant(PropertyAccessMode.Read)),
					this.ReturnType);
			}
			var rest = BindingRestrictions.GetTypeRestriction(target.Expression, target.LimitType);
			return new DynamicMetaObject(expr, rest);
		}

		private DynamicMetaObject MakeDynamicMetaObject(DynamicMetaObject target, PropertyInfo propInfo) {
			Contract.Requires<ArgumentNullException>(target != null);
			Contract.Requires<ArgumentNullException>(propInfo != null);
			Contract.Requires<ArgumentException>(propInfo.CanRead == true);

			var targetExpr = BinderHelper.Wrap(target.Expression, target.LimitType);
			var propAccess = Expression.Property(targetExpr, propInfo);
			var expr = BinderHelper.Wrap(propAccess, this.ReturnType);
			var rest = BindingRestrictions.GetTypeRestriction(target.Expression, target.LimitType);
			return new DynamicMetaObject(expr, rest);
		}
	}
}
