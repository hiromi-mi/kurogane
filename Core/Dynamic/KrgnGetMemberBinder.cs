using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Dynamic;
using Kurogane.Dynamic;

namespace Kurogane.Dynamic {
	
	class KrgnGetMemberBinder : GetMemberBinder {

		private static Dictionary<string, KrgnGetMemberBinder> cache = new Dictionary<string, KrgnGetMemberBinder>();
		public static KrgnGetMemberBinder Create(string name) {
			if (cache.ContainsKey(name)) return cache[name];
			var binder = new KrgnGetMemberBinder(name);
			cache[name] = binder;
			return binder;
		}

		private KrgnGetMemberBinder(string name)
			: base(name, false) {
		}

		public override DynamicMetaObject FallbackGetMember(DynamicMetaObject target, DynamicMetaObject errorSuggestion) {
			var mo = MetaObjectLoader.Create(target.Value, target.Expression);
			if (mo != null)
				return mo.BindGetMember(this);
			throw new NotImplementedException();
		}
	}
}
