using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Dynamic;
using System.Linq.Expressions;
using System.Reflection;

namespace Kurogane.RuntimeBinder {

	/// <summary>
	/// 型からMetaObjectを引くためのヘルパクラス
	/// </summary>
	public static class MetaObjectLoader {

		private static readonly Dictionary<Type, AliasReflectionCacher> _alias = new Dictionary<Type, AliasReflectionCacher>();

		public static void RegisterAlias(Type target, Type alias) {
			_alias[target] = new AliasReflectionCacher(target, alias);
		}

		public static AliasReflectionCacher GetAlias(Type type) {
			AliasReflectionCacher value;
			if (_alias.TryGetValue(type, out value))
				return value;
			else
				return null;
		}
	}

	public class AliasReflectionCacher {

		public string Name { get; private set; }
		public Type Target { get; private set; }
		public Type Alias { get; private set; }

		private IDictionary<string, MemberInfo> _members = null;

		public AliasReflectionCacher(Type target, Type alias) {
			this.Target = target;
			this.Alias = alias;
			var attrs = Alias.GetCustomAttributes(typeof(JpNameAttribute), true);
			this.Name = 
				attrs.OfType<JpNameAttribute>().Select(jpName => jpName.Name).SingleOrDefault() ??
				alias.Name;
		}

		private void Load() {
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

		public MemberInfo GetMemberInfo(string name) {
			Load();
			MemberInfo info;
			if (_members.TryGetValue(name, out info))
				return info;
			else
				return null;
		}
	}
}
