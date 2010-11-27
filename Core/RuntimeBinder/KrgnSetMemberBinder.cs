using System.Dynamic;
using System.Linq.Expressions;
using System.Reflection;

namespace Kurogane.RuntimeBinder {

	public class KrgnSetMemberBinder : SetMemberBinder {

		public KrgnSetMemberBinder(string name)
			: base(name, false) {
		}

		public override DynamicMetaObject FallbackSetMember(DynamicMetaObject target, DynamicMetaObject value, DynamicMetaObject errorSuggestion) {
			return
				SearchAlias(target, value) ??
				DefaultSetMember(target, value);
		}

		private DynamicMetaObject SearchAlias(DynamicMetaObject target, DynamicMetaObject value) {
			var cacher = MetaObjectLoader.GetAlias(target.LimitType);
			if (cacher == null)
				return null;
			var propInfo = cacher.GetMemberInfo(this.Name) as PropertyInfo;
			if (propInfo != null && propInfo.CanWrite) {
				return MakeDynamicMetaObject(target, propInfo, value);
			}
			return null;
		}

		private DynamicMetaObject DefaultSetMember(DynamicMetaObject target, DynamicMetaObject value) {
			Expression expr;
			var propInfo = target.LimitType.GetProperty(Name);
			if (propInfo != null && propInfo.CanWrite) {
				return MakeDynamicMetaObject(target, propInfo, value);
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

		private DynamicMetaObject MakeDynamicMetaObject(DynamicMetaObject target, PropertyInfo propInfo, DynamicMetaObject value) {
			var targetExpr = BinderHelper.Wrap(target.Expression, target.LimitType);
			var propAccess = Expression.Property(targetExpr, propInfo);
			var valueExpr = BinderHelper.Wrap(value.Expression, propInfo.DeclaringType);
			var expr = BinderHelper.Wrap(Expression.Assign(propAccess, valueExpr), this.ReturnType);
			var rest = BindingRestrictions.GetTypeRestriction(target.Expression, target.LimitType);
			return new DynamicMetaObject(expr, rest);
		}

	}
}
