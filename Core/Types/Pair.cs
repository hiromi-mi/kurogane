using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Dynamic;
using System.Linq.Expressions;

namespace Kurogane.Types {

	public class Pair :  IInspectable {
		public object Head { get; private set; }
		public object Tail { get; private set; }

		public Pair(object head, object tail) {
			Head = head;
			Tail = tail;
		}

		public override string ToString() {
			return String.Concat(Head, Tail);
		}

		public string Inspect() {
			var hStr = Head is IInspectable ? ((IInspectable)Head).Inspect() : Head.ToString();
			var tStr = Tail is IInspectable ? ((IInspectable)Tail).Inspect() : Tail.ToString();
			return "(" + hStr + " . " + tStr + ")";
		}
	}
}
