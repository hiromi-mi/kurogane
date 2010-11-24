using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kurogane.Libraries {

	[Library]
	public static class Int32Alias {

		[JpName("絶対値")]
		public static int Abs(this int value) {
			return Math.Abs(value);
		}
	}
}
