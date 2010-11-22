using System.Dynamic;
using System.Linq.Expressions;
using Kurogane.Dynamic;

namespace Kurogane.RuntimeBinder {

	public class KrgnGetMemberBinder : GetMemberBinder {

		public KrgnGetMemberBinder(string name)
			: base(name, true) {
		}

		public override DynamicMetaObject FallbackGetMember(DynamicMetaObject target, DynamicMetaObject errorSuggestion) {
			var mo = MetaObjectLoader.Create(target.Value, target.Expression);
			if (mo != null)
				return mo.BindGetMember(this);
			return DefaultGetMember(target);
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
					Expression.New(ctorInfo, Expression.Constant(this.Name), Expression.Constant(PropertyAccessMode.Write)),
					this.ReturnType);
			}
			var rest = BindingRestrictions.GetTypeRestriction(target.Expression, target.LimitType);
			return new DynamicMetaObject(expr, rest);
		}
	}
}
