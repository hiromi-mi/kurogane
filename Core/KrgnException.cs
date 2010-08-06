using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kurogane {

	public abstract class KrgnException :Exception {
		public KrgnException() : base() { }
		public KrgnException(string message) : base(message) { }
		public KrgnException(string message, Exception innerException) : base(message, innerException) { }
	}
}
