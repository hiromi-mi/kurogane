using System;
using System.Dynamic;

namespace Kurogane.Dynamic {

	public class KrgnGetMemberBinder : GetMemberBinder {

		public KrgnGetMemberBinder(string name)
			: base(name, true) {
		}

		public override DynamicMetaObject FallbackGetMember(DynamicMetaObject target, DynamicMetaObject errorSuggestion) {
			var mo = MetaObjectLoader.Create(target.Value, target.Expression);
			if (mo != null)
				return mo.BindGetMember(this);
			throw new NotImplementedException();
		}
	}
}
