using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kurogane.Types;

namespace Kurogane.Libraries {

	[Library]
	public static class Utility {

		public static void LoadPass(Engine engine, Globals scope) {
			Func<object, object> pass = o => o;
			scope.SetVariable("パス", KrgnFunc.Create(pass, "を"));
		}
	}
}
