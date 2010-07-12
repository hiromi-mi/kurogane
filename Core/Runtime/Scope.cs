using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using Kurogane.Buildin;

namespace Kurogane.Runtime {

	/// <summary>
	/// 値を保持するオブジェクトのインタフェース。
	/// </summary>
	public interface IScope {
		/// <summary>
		/// 変数に値を設定する。
		/// </summary>
		/// <param name="name">変数名</param>
		/// <param name="value">値</param>
		/// <returns>設定した値をそのまま返す。</returns>
		object SetVariable(string name, object value);

		/// <summary>
		/// 変数の値を取得する。
		/// </summary>
		/// <param name="name">変数名</param>
		/// <returns>値</returns>
		object GetVariable(string name);
	}

	public class Scope {

		// ----- ----- ----- ----- ----- fields ----- ----- ----- ----- -----

		private readonly Scope _parent;
		private readonly IDictionary<string, dynamic> _values = new Dictionary<string, dynamic>();

		// ----- ----- ----- ----- ----- ctor ----- ----- ----- ----- -----
		public Scope()
			: this(null) {

		}

		public Scope(Scope parent) {
			_parent = parent;
		}

		// ----- ----- ----- ----- ----- methods ----- ----- ----- ----- -----
		public bool HasVariable(string name) {
			return _values.ContainsKey(name);
		}

		public dynamic GetVariable(string name) {
			if (_values.ContainsKey(name)) {
				return _values[name];
			}
			else if (_parent != null) {
				return _parent.GetVariable(name);
			}
			throw new KrgnException(String.Format("変数「{0}」が存在しません。", name)); ;
		}

		public dynamic SetVariable(string name, dynamic value) {
			return _values[name] = value;
		}
	}
}
