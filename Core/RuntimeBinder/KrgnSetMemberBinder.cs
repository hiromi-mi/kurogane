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
			var propInfo = target.LimitType.GetProperty(Name);
			if (propInfo == null)
				return RuntimeBinderException.CreateMetaObject(
					Name + "という属性を持っていません。",
					BindingRestrictions.GetTypeRestriction(target.Expression, target.LimitType));
			if (propInfo.CanWrite == false)
				return RuntimeBinderException.CreateMetaObject(
					Name + "という属性に書き込みできません。",
					BindingRestrictions.GetTypeRestriction(target.Expression, target.LimitType));
			return
				new DynamicMetaObject(
					Expression.Assign(Expression.Property(target.Expression, propInfo), value.Expression),
					BindingRestrictions.GetTypeRestriction(target.Expression, target.LimitType));
		}
	}
}
