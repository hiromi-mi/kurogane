using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kurogane {

	/// <summary>
	/// 指定の変数が見つからない時に発生する例外。
	/// </summary>
	[Serializable]
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

	/// <summary>属性へのアクセス方法</summary>
	public enum PropertyAccessMode {
		Read,
		Write
	}

	/// <summary>
	/// 指定の属性への読み書きの時に発生する例外。
	/// </summary>
	[Serializable]
	public class PropertyNotFoundException : Exception {
		private const string readErrorMsg = "{0}という属性から読み取りできません。";
		private const string writeErrorMsg = "{0}という属性に書き込みできません。";

		/// <summary>属性名</summary>
		public string Name { get; private set; }

		/// <summary>属性へのアクセス方法</summary>
		public PropertyAccessMode Mode { get; private set; }

		public override string Message {
			get {
				switch (this.Mode) {
				case PropertyAccessMode.Read:
					return String.Format(readErrorMsg, this.Name);
				case PropertyAccessMode.Write:
					return String.Format(writeErrorMsg, this.Name);
				}
				return base.Message;
			}
		}

		/// <summary>
		/// コンストラクタ
		/// </summary>
		/// <param name="name">属性名</param>
		/// <param name="mode">属性へのアクセス</param>
		public PropertyNotFoundException(string name, PropertyAccessMode mode) {
			this.Name = name;
			this.Mode = mode;
		}
	}

}
