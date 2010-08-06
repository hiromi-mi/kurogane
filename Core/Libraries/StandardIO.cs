using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kurogane.Types;

namespace Kurogane.Libraries {

	[Library]
	public static class StandardIO {

		public static void Read(Engine engine, Scope scope) {
			Func<object> read = delegate {
				return engine.Input.ReadLine();
			};
			scope.SetVariable("入力", KrgnFunc.Create(read));
		}

		public static void Write(Engine engine, Scope scope) {
			Func<object, object> write = delegate(object obj) {
				var str = obj.ToString();
				engine.Output.Write(str);
				return str;
			};
			scope.SetVariable("出力", KrgnFunc.Create(write, "を"));
		}

		[Name("改行")]
		public static string NewLine = Environment.NewLine;
	}
}
