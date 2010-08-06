using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kurogane {
	public class KrgnRuntimeException : KrgnException {
		public KrgnRuntimeException(string message)
			: base(message) {
		}
	}
}
