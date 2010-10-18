using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Dynamic;

namespace Kurogane.Dynamic {

	public class KrgnSetMemberBinder : SetMemberBinder {

		private static Dictionary<string, KrgnSetMemberBinder> cache = new Dictionary<string, KrgnSetMemberBinder>();
		public static KrgnSetMemberBinder Create(string name) {
			if (cache.ContainsKey(name)) return cache[name];
			var binder = new KrgnSetMemberBinder(name);
			cache[name] = binder;
			return binder;
		}

		private KrgnSetMemberBinder(string name)
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
