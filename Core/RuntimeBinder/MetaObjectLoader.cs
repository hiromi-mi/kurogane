using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Dynamic;
using System.Linq.Expressions;
using System.Reflection;
using System.Diagnostics.Contracts;

namespace Kurogane.RuntimeBinder {

	/// <summary>
	/// 型からMetaObjectを引くためのヘルパクラス
	/// </summary>
	public static class MetaObjectLoader {

		private static readonly Dictionary<Type, AliasReflectionCacher> _alias = new Dictionary<Type, AliasReflectionCacher>();
		private static readonly object _alias_lock = new object();

		public static void RegisterAlias(Type target, Type alias) {
			Contract.Requires<ArgumentNullException>(target != null);
			Contract.Requires<ArgumentNullException>(alias != null);
			// lock only write
			lock (_alias_lock) {
				_alias[target] = new AliasReflectionCacher(target, alias);
			}
		}

		public static AliasReflectionCacher GetAlias(Type type) {
			Contract.Requires<ArgumentNullException>(type != null);
			AliasReflectionCacher value;
			if (_alias.TryGetValue(type, out value))
				return value;
			else
				return null;
		}
	}

	/// <summary>
	/// あるクラスをクロガネ用にAliasして、その情報をもつクラス。
	/// 情報取得にはリフレクションを用いる。
	/// リフレクションは遅延で行われ、その処理はlockされる。
	/// </summary>
	public class AliasReflectionCacher {

		private IDictionary<string, MemberInfo> _members = null;
		private readonly object load_key = new object();

		public string Name { get; private set; }
		public Type Target { get; private set; }
		public Type Alias { get; private set; }

		public AliasReflectionCacher(Type target, Type alias) {
			Contract.Requires<ArgumentNullException>(target != null);
			Contract.Requires<ArgumentNullException>(alias != null);
			this.Target = target;
			this.Alias = alias;
			var attrs = Alias.GetCustomAttributes(typeof(JpNameAttribute), true);
			this.Name =
				attrs.OfType<JpNameAttribute>().Select(jpName => jpName.Name).SingleOrDefault() ??
				alias.Name;
		}

		private void Load() {
			Contract.Ensures(_members != null);
			if (_members != null)
				return;
			lock (load_key) {
				if (_members != null)
					return;
				_members = new SortedList<string, MemberInfo>();

				foreach (var propInfo in Alias.GetProperties()) {
					var nameAttrs = propInfo.GetCustomAttributes(typeof(JpNameAttribute), false);
					if (nameAttrs.Length == 0) continue;
					var info = Target.GetProperty(propInfo.Name);
					if (info != null) {
						foreach (JpNameAttribute nameAttr in nameAttrs) {
							_members[nameAttr.Name] = info;
						}
					}
				}
				foreach (var methInfo in Alias.GetMethods()) {
					var nameAttrs = methInfo.GetCustomAttributes(typeof(JpNameAttribute), false);
					if (nameAttrs.Length == 0) continue;
					var paramInfo = methInfo.GetParameters();
					Type[] types = new Type[paramInfo.Length];
					for (int i = 0; i < paramInfo.Length; i++)
						types[i] = paramInfo[i].GetType();
					var info = Target.GetMethod(methInfo.Name, types);
					if (info != null) {
						foreach (JpNameAttribute nameAttr in nameAttrs) {
							_members[nameAttr.Name] = info;
						}
					}
				}
			}
		}

		public MemberInfo GetMemberInfo(string name) {
			Contract.Requires<ArgumentNullException>(name != null);
			Load();
			MemberInfo info;
			if (_members.TryGetValue(name, out info))
				return info;
			else
				return null;
		}
	}
}
