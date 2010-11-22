using System.Dynamic;
using System.Linq.Expressions;
using Kurogane.Dynamic;

namespace Kurogane.RuntimeBinder {

	public class KrgnSetMemberBinder : SetMemberBinder {

		public KrgnSetMemberBinder(string name)
			: base(name, false) {
		}

		public override DynamicMetaObject FallbackSetMember(DynamicMetaObject target, DynamicMetaObject value, DynamicMetaObject errorSuggestion) {
			var mo = MetaObjectLoader.Create(target.Value, target.Expression);
			if (mo != null)
				return mo.BindSetMember(this, value);
			return DefaultSetMember(target, value);
		}

		private DynamicMetaObject DefaultSetMember(DynamicMetaObject target, DynamicMetaObject value) {
			Expression expr;
			var propInfo = target.LimitType.GetProperty(Name);
			if (propInfo != null && propInfo.CanWrite) {
				expr = Expression.Assign(
					Expression.Property(target.Expression, propInfo),
					Expression.Convert(value.Expression, propInfo.DeclaringType));
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
	}
}
