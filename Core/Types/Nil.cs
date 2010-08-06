using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kurogane.Types {

	public sealed class Nil {

		public static readonly Nil Instance = new Nil();

		private Nil() { }

		public override string ToString() {
			return String.Empty;
		}
	}
}
