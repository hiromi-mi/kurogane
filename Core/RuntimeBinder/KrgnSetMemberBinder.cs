using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Dynamic;
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
			throw new NotImplementedException();
		}
	}
}
