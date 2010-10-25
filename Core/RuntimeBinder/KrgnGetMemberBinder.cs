using System;
using System.Dynamic;
using Kurogane.Dynamic;
using System.Linq.Expressions;

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
			var propInfo = target.LimitType.GetProperty(Name);
			if (propInfo == null)
				return RuntimeBinderException.CreateMetaObject(
					Name + "という属性を持っていません。",
					BindingRestrictions.GetTypeRestriction(target.Expression, target.LimitType));
			return
				new DynamicMetaObject(
					Expression.Property(target.Expression, propInfo),
					BindingRestrictions.GetTypeRestriction(target.Expression, target.LimitType));

		}
	}
}
