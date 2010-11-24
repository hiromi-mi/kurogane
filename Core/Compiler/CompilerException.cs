using System;

namespace Kurogane.Compiler {

	[Serializable]
	public abstract class CompilerException : Exception {

		public string FileName { get; private set; }
		public int LineNumber { get; private set; }
		public int CharCount { get; private set; }

		internal CompilerException(string message) : base(message) { }
		internal CompilerException(string message, string filename, int lineNumber, int charCount)
			: base(message) {
			this.FileName = filename;
			this.LineNumber = lineNumber;
			this.CharCount = charCount;
		}
	}

	[Serializable]
	public sealed class LexicalException : CompilerException {
		public LexicalException(string message) : base(message) { }
	}

	[Serializable]
	public sealed class SyntaxException : CompilerException {
		public SyntaxException(string message, string filename, int lineNumber, int charCount)
			: base(message, filename, lineNumber, charCount) { }
	}

	[Serializable]
	public sealed class SemanticException : CompilerException {
		public SemanticException(string message) : base(message) { }
	}

}
