using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kurogane.Libraries {

	[Library]
	public static class ListLib {

		[JpName("平坦化")]
		public static ListCell Flatten([Suffix("を")] object obj) {
			return FlattenFoldRight<ListCell>(obj, null, (o, lst) => new ListCell(o, lst));
		}

		public static T FlattenFoldLeft<T>(object obj, T start, Func<object, T, T> func) {
			if (obj == null)
				return start;
			if (obj is ListCell) {
				var cell = (ListCell)obj;
				return FlattenFoldLeft(cell.Tail, FlattenFoldLeft(cell.Head, start, func), func);
			}
			if (obj is Tuple<object, object>) {
				var pair = (Tuple<object, object>)obj;
				return FlattenFoldLeft(pair.Item2, FlattenFoldLeft(pair.Item1, start, func), func);
			}
			return func(obj, start);
		}

		public static T FlattenFoldRight<T>(object obj, T start, Func<object, T, T> func) {
			if (obj == null)
				return start;
			if (obj is ListCell) {
				var cell = (ListCell)obj;
				return FlattenFoldRight(cell.Head, FlattenFoldRight(cell.Tail, start, func), func);
			}
			if (obj is Tuple<object, object>) {
				var pair = (Tuple<object, object>)obj;
				return FlattenFoldRight(pair.Item1, FlattenFoldRight(pair.Item2, start, func), func);
			}
			return func(obj, start);
		}

		public static object Map(SuffixFunc<Func<object, object>> func, object list) {
			return Map(func.Func, list);
		}

		public static object Map(Func<object, object> func, object list) {
			if (list is ListCell) {
				return ListCell.Map(func, (ListCell)list);
			}
			var tuple = list as Tuple<object, object>;
			if (tuple == null) {
				return func(list);
			}
			var fst = func(tuple.Item1);
			var snd = Map(func, tuple.Item2);
			return new Tuple<object, object>(fst, snd);
		}

	}
}
