using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

namespace Kurogane {
	public class Enumerator {

		public static IEnumerable<object> ToEnumerable(object list) {
			while (true) {
				var pair = list as Tuple<object, object>;
				if (pair == null)
					break;
				yield return pair.Item1;
				list = pair.Item2;
			}
		}

		public static Tuple<object, object> FromEnumerable(object obj) {
			var list = obj as IEnumerable;
			if (list == null)
				return null;
			return FromEnumerable(list);
		}
		public static Tuple<object, object> FromEnumerable(IEnumerable list) {
			var tor = list.GetEnumerator();
			using (var disp = tor as IDisposable) {
				return FromEnumerable(tor);
			}
		}

		private static Tuple<object, object> FromEnumerable(IEnumerator tor) {
			if (tor.MoveNext() == false)
				return null;
			return new Tuple<object, object>(tor.Current, FromEnumerable(tor));
		}

		public static Tuple<object, object> Map(SuffixFunc<Func<object, object>> func, object list) {
			return FromEnumerable(ToEnumerable(list).Select(obj => func.Func(obj)));
		}

		public static Tuple<object, object> Map(Func<object, object> func, object list) {
			return FromEnumerable(ToEnumerable(list).Select(obj => func(obj)));
		}
	}
}
