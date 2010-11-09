using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Dynamic;
using System.Reflection;
using Kurogane.Util;
using System.Collections.ObjectModel;
using System.Collections;

namespace Kurogane {

	public class ListCell : IDynamicMetaObjectProvider, IEquatable<ListCell> {

		#region static

		public static object Cons(object left, object right) {
			var tail = right as ListCell;
			if (tail == right)
				return new ListCell(left, tail);
			else
				return new Tuple<object, object>(left, right);
		}

		public static ListCell FromEnumerable(IEnumerable list) {
			var tor = list.GetEnumerator();
			using (var disp = tor as IDisposable) {
				return FromEnumerable(tor);
			}
		}

		private static ListCell FromEnumerable(IEnumerator tor) {
			if (tor.MoveNext() == false)
				return null;
			return new ListCell(tor.Current, FromEnumerable(tor));
		}

		#endregion

		public virtual object Head { get; private set; }
		public virtual ListCell Tail { get; private set; }

		public ListCell(object head, ListCell tail) {
			this.Head = head;
			this.Tail = tail;
		}

		public override string ToString() {
			var buff = new StringBuilder("[");
			AppendItem(buff);
			return buff.ToString();
		}

		private void AppendItem(StringBuilder buff) {
			buff.Append(Head);
			if (Tail != null) {
				buff.Append(", ");
				this.Tail.AppendItem(buff);
			}
			else {
				buff.Append("]");
			}
		}

		public override int GetHashCode() {
			int head = Head == null ? 0 : Head.GetHashCode();
			int tail = Tail == null ? 0 : Tail.GetHashCode();
			const int magic = 13;
			return (head << magic) ^ tail ^ (head >> (32 - magic));
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

		#endregion

		#region IDynamicMetaObjectProvider メンバー

		DynamicMetaObject IDynamicMetaObjectProvider.GetMetaObject(Expression parameter) {
			return new MetaObject(this, parameter);
		}

		private class MetaObject : DynamicMetaObject {

			private static PropertyInfo headInfo = ReflectionHelper.PropertyInfo<ListCell>(cell => cell.Head);
			private static PropertyInfo tailInfo = ReflectionHelper.PropertyInfo<ListCell>(cell => cell.Tail);
			private static ReadOnlyCollection<string> memberNames = Array.AsReadOnly(new[]{
				ConstantNames.Head,
				ConstantNames.Tail
			});

			public new Expression Expression {
				get {
					if (base.Expression.Type == typeof(ListCell))
						return base.Expression;
					else
						return Expression.Convert(base.Expression, typeof(ListCell));
				}
			}

			public MetaObject(ListCell cell, Expression expr)
				: base(expr, BindingRestrictions.GetTypeRestriction(expr, typeof(ListCell)), cell) {
			}

			public override DynamicMetaObject BindGetMember(GetMemberBinder binder) {
				PropertyInfo info = null;
				switch (binder.Name) {
				case ConstantNames.Head:
					info = headInfo;
					break;
				case ConstantNames.Tail:
					info = tailInfo;
					break;
				}
				if (info == null)
					return base.BindGetMember(binder);
				return new DynamicMetaObject(
					Expression.Property(this.Expression, info),
					BindingRestrictions.GetTypeRestriction(this.Expression, typeof(ListCell)));
			}

			public override IEnumerable<string> GetDynamicMemberNames() {
				return memberNames;
			}
		}

		#endregion

	}
}
