using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kurogane.Libraries {

	[Library]
	public static class StandardIO {

		[JpName("改行")]
		public static string NewLine = Environment.NewLine;

		[JpName("行入力")]
		public static string Read(this Engine engine) {
			return engine.In.ReadLine();
		}

		[JpName("出力")]
		public static object Write(this Engine engine, [Suffix("を")]object obj) {
			engine.Out.Write(obj);
			return obj;
		}
	}
}
