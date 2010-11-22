using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Dynamic;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using Kurogane.Util;

namespace Kurogane {

	[JpName("リスト")]
	public class ListCell : IEquatable<ListCell>, IEnumerable {

		#region static

		public static object Cons(object left, object right) {
			var tail = right as ListCell;
			if (tail == right)
				return new ListCell(left, tail);
			else
				return new Tuple<object, object>(left, right);
		}

		public static ListCell ConvertFrom(IEnumerable list) {
			var tor = list.GetEnumerator();
			using (var disp = tor as IDisposable) {
				return ConvertFrom(tor);
			}
		}

		private static ListCell ConvertFrom(IEnumerator tor) {
			if (tor.MoveNext() == false)
				return null;
			return new ListCell(tor.Current, ConvertFrom(tor));
		}

		public static ListCell Map(Func<object, object> func, ListCell list) {
			if (list == null)
				return null;
			var head = func(list.Head);
			var tail = Map(func, list.Tail);
			return new ListCell(head, tail);
		}

		#endregion

		// ----- ----- ----- ----- property ----- ----- ----- -----

		[JpName(ConstantNames.Head)]
		public virtual object Head { get; private set; }

		[JpName(ConstantNames.Tail)]
		public virtual ListCell Tail { get; private set; }

		// ----- ----- ----- ----- ctor ----- ----- ----- -----

		public ListCell(object head, ListCell tail) {
			this.Head = head;
			this.Tail = tail;
		}

		// ----- ----- ----- ----- method ----- ----- ----- -----
		
		public override string ToString() {
			var buff = new StringBuilder("[");
			AppendItem(buff);
			buff.Append("]");
			return buff.ToString();
		}

		private void AppendItem(StringBuilder buff) {
			buff.Append(Head);
			if (Tail != null) {
				buff.Append(", ");
				this.Tail.AppendItem(buff);
			}
		}

		#region IEquatable<ListCell> メンバー

		public override bool Equals(object obj) {
			return this.Equals(obj as ListCell);
		}

		public bool Equals(ListCell other) {
			return other != null
				&& (this.Head == null && other.Head == null ||
					this.Head.Equals(other.Head))
				&& (this.Tail == null && other.Tail == null ||
					this.Tail.Equals(other.Tail));
		}

		public override int GetHashCode() {
			int head = Head == null ? 0 : Head.GetHashCode();
			int tail = Tail == null ? 0 : Tail.GetHashCode();
			const int magic = 13;
			return (head << magic) ^ tail ^ (head >> (32 - magic));
		}

		#endregion

		#region IEnumerable メンバー

		public IEnumerator GetEnumerator() {
			var cell = this;
			while (cell != null) {
				yield return cell.Head;
				cell = cell.Tail;
			}
		}

		#endregion
	}
}
