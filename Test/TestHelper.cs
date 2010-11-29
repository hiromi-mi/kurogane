using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kurogane.Test {

	/// <summary>
	/// エンジンを初期化する必要が無いテストはこのクラスを継承すると良い。
	/// </summary>
	public abstract class TestHelper {

		private Engine _engine = new Engine();

		protected object Execute(string code) { return Execute<object>(code); }
		protected T Execute<T>(string code) { return (T)_engine.Execute(code, Statics.TestName); }

	}
}
