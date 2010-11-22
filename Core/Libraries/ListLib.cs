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

		/// <summary>
		/// リストを平坦化した上で、左集約する。
		/// </summary>
		/// <typeparam name="T">初期値の型</typeparam>
		/// <param name="start">集約の初期値</param>
		/// <param name="tuple">集約するリスト</param>
		/// <param name="func">集約関数</param>
		/// <returns>集約結果</returns>
		public static T FlattenFoldLeft<T>(T start, object tuple, Func<object, T, T> func) {
			if (tuple == null)
				return start;
			if (tuple is ListCell) {
				var cell = (ListCell)tuple;
				return FlattenFoldLeft(FlattenFoldLeft(start, cell.Head, func), cell.Tail, func);
			}
			if (tuple is Tuple<object, object>) {
				var pair = (Tuple<object, object>)tuple;
				return FlattenFoldLeft(FlattenFoldLeft(start, pair.Item1, func), pair.Item2, func);
			}
			return func(tuple, start);
		}

		/// <summary>
		/// リストを平坦化した上で、右集約する。
		/// </summary>
		/// <typeparam name="T">初期値の型</typeparam>
		/// <param name="tuple">集約するリスト</param>
		/// <param name="start">集約の初期値</param>
		/// <param name="func">集約関数</param>
		/// <returns>集約結果</returns>
		public static T FlattenFoldRight<T>(object tuple, T start, Func<object, T, T> func) {
			if (tuple == null)
				return start;
			if (tuple is ListCell) {
				var cell = (ListCell)tuple;
				return FlattenFoldRight(cell.Head, FlattenFoldRight(cell.Tail, start, func), func);
			}
			if (tuple is Tuple<object, object>) {
				var pair = (Tuple<object, object>)tuple;
				return FlattenFoldRight(pair.Item1, FlattenFoldRight(pair.Item2, start, func), func);
			}
			return func(tuple, start);
		}

		/// <summary>
		/// キーワード「それぞれ」によって呼ばれる関数。
		/// </summary>
		public static object Map(Func<object, object> func, object list) {
			var cell = list as ListCell;
			if (cell != null) {
				return ListCell.Map(func, cell);
			}
			var tuple = list as Tuple<object, object>;
			if (tuple != null) {
				var fst = Map(func, tuple.Item1);
				var snd = Map(func, tuple.Item2);
				return new Tuple<object, object>(fst, snd);
			}
			return func(list);
		}

	}
}
