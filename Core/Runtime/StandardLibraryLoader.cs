using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kurogane.Buildin;

namespace Kurogane.Runtime {
	public class StandardLibraryLoader {

		private readonly Scope _scope;

		public StandardLibraryLoader(Scope scope) {
			_scope = scope;
		}

		public void Load() {
			Set_出力する();
			Set_演算子();
			Set_パスする();
		}

		private void Set_出力する() {
			Func<object, object> write = obj => { Console.Write(obj); return obj; };
			_scope.SetVariable("出力", new KrgnFunc(write, "を"));

			Func<object, object> writeln = obj => { Console.WriteLine(obj); return obj; };
			_scope.SetVariable("改行出力", new KrgnFunc(writeln, "を"));
			_scope.SetVariable("表示", new KrgnFunc(writeln, "を"));

			_scope.SetVariable("挨拶", "やっほ～");
		}

		private void Set_パスする() {
			Func<object, object> pass = a => a;
			_scope.SetVariable("パス", new KrgnFunc(pass, "を"));
		}

		private void Set_演算子() {
			SetOperator((a, b) => a + b, "+", "＋");
			SetOperator((a, b) => a - b, "-", "－");
			SetOperator((a, b) => a * b, "*", "×");
			SetOperator((a, b) => a / b, "/");
			SetOperator((a, b) => a == b, "=", "＝");
			SetOperator((a, b) => a != b, "≠");
			SetOperator((a, b) => a < b, "<", "＜");
			SetOperator((a, b) => a <= b, "≦");
			SetOperator((a, b) => a > b, ">", "＞");
			SetOperator((a, b) => a >= b, "≧");
		}

		private void SetOperator(Func<dynamic, dynamic, dynamic> op, params string[] names) {
			foreach (var name in names)
				_scope.SetVariable(name, (Delegate)op);
		}
	}
}
