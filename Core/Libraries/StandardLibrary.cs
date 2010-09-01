using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kurogane.Types;
using System.Diagnostics;
using System.IO;

namespace Kurogane.Libraries {

	[Library]
	public static class StandardLibrary {
		public static void Set(Engine engine, Scope scope) {
			new Standard(engine, scope).Set();
			new StandardIO(engine, scope).Set();
		}
	}

	public abstract class LibraryBase {
		protected readonly Engine Engine;
		protected readonly Scope Scope;

		public LibraryBase(Engine engine, Scope scope) {
			Debug.Assert(engine != null);
			Debug.Assert(scope != null);
			this.Engine = engine;
			this.Scope = scope;
		}

		public abstract void Set();
	}

	public class Standard : LibraryBase {

		public Standard(Engine engine, Scope scope)
			: base(engine, scope) {
		}

		public override void Set() {
			// include
			Func<object, object> include = Include;
			Scope.SetVariable("参照", KrgnFunc.Create(include, "を"));

			// pass
			Func<object, object> pass = o => o;
			Scope.SetVariable("パス", KrgnFunc.Create(pass, "を"));
		}

		private object Include(object obj) {
			var fail = Nothing<object>.Instance;
			string path = obj as string;
			if (path == null) return fail;
			try {
				if (File.Exists(path)) {
					if (Scope.Included.Contains(path)) return fail;
					return Engine.ExecuteFile(path, Scope);
				}
				return fail;
			}
			catch {
				return fail;
			}
		}
	}

	public class StandardIO : LibraryBase {

		public StandardIO(Engine engine, Scope scope)
			: base(engine, scope) {
		}

		public override void Set() {
			// stdin
			Func<object> read = delegate {
				return Engine.Input.ReadLine();
			};
			Scope.SetVariable("入力", KrgnFunc.Create(read));

			// stdout
			Func<object, object> write = delegate(object obj) {
				var str = obj.ToString();
				Engine.Output.Write(str);
				return str;
			};
			Scope.SetVariable("出力", KrgnFunc.Create(write, "を"));

			// newline
			Scope.SetVariable("改行", Environment.NewLine);
		}
	}

}
