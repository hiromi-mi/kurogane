using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Dynamic;
using System.Linq.Expressions;

namespace Kurogane.Types {

	public interface IPair {
		object Head { get; }
		object Tail { get; }
	}

	public class Pair : IPair{
		public object Head { get; private set; }
		public object Tail { get; private set; }

		public Pair(object head, object tail) {
			Head = head;
			Tail = tail;
		}

		public override string ToString() {
			return "(" + Head + " . " + Tail + ")";
		}
	}

}
