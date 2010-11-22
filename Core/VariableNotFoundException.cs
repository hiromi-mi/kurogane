using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kurogane {

	public class VariableNotFoundException : Exception {

		const string ErrorMsg = "「{0}」という変数は存在しません。";

		/// <summary>
		/// 変数名
		/// </summary>
		public string Name { get; private set; }

		public override string Message { get { return String.Format(ErrorMsg, this.Name); } }

		/// <summary>
		/// コンストラクタ
		/// </summary>
		/// <param name="name">見つからなかった変数の名前</param>
		public VariableNotFoundException(string name) {
			this.Name = name;
		}
	}
}
