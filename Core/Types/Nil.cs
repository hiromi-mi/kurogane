using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kurogane.Types {

	/// <summary>
	/// Nullの代わりをするクラス。Singleton
	/// </summary>
	public sealed class Nil : IInspectable {

		public static readonly Nil Instance = new Nil();

		private Nil() { }

		public override string ToString() {
			return String.Empty;
		}

		public string Inspect() {
			return "nil";
		}
	}
}
