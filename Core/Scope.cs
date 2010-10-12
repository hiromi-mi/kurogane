using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using Kurogane.Dynamics;

namespace Kurogane {
	public class Scope : IDynamicMetaObjectProvider {

		private readonly IDictionary<string, dynamic> _values = new Dictionary<string, dynamic>();
		internal readonly ISet<string> Included = new HashSet<string>();

		public bool HasVariable(string name) {
			return _values.ContainsKey(name);
		}
		public object GetVariable(string name) {
			if (_values.ContainsKey(name)) {
				return _values[name];
			}
			throw new KrgnRuntimeException(String.Format("変数「{0}」が存在しません。", name)); ;
		}
		public object SetVariable(string name, object value) {
			return _values[name] = value;
		}
		DynamicMetaObject IDynamicMetaObjectProvider.GetMetaObject(Expression parameter) {
			return new ScopeMetaObject(this, parameter);
		}
	}
}
