using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;

namespace Kurogane.Libraries {

	[Library]
	public static class StandardIO {

		[JpName("改行")]
		public static string NewLine = Environment.NewLine;

		[JpName("行入力")]
		public static string Read(this Engine engine) {
			Contract.Requires<ArgumentNullException>(engine != null);
			return engine.In.ReadLine();
		}

		[JpName("出力")]
		public static object Write(this Engine engine, [Suffix("を")]object obj) {
			Contract.Requires<ArgumentNullException>(engine != null);
			engine.Out.Write(obj);
			return obj;
		}
	}
}
