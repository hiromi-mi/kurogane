using System;

namespace Kurogane.Compiler {

	/// <summary>
	/// 言語の静的解析で発生する例外を示すクラス。
	/// </summary>
	[Serializable]
	public abstract class CompilerException : Exception {
		public string FileName { get; private set; }
		public TextLocation Location { get; private set; }

		internal CompilerException(string message, string filename, TextLocation location)
			: base(message) {
			this.FileName = filename;
			this.Location = location;
		}
	}

	[Serializable]
	public sealed class LexicalException : CompilerException {
		public LexicalException(string message, string filename, TextLocation location)
			: base(message, filename, location) { }
	}

	[Serializable]
	public sealed class SyntaxException : CompilerException {
		public SyntaxException(string message, string filename, TextLocation location)
			: base(message, filename, location) { }
	}
}
