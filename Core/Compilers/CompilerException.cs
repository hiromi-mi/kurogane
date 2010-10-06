using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kurogane.Compilers
{

	public abstract class CompilerException : KrgnException
	{
		public string FileName { get; private set; }
		public int LineNumber { get; private set; }
		public int CharCount { get; private set; }

		public CompilerException(string message) : base(message) { }
		public CompilerException(string message, string filename, int lineNumber, int charCount) 
			: base(message)
		{
			this.FileName = filename;
			this.LineNumber = lineNumber;
			this.CharCount = charCount;
		}
	}

	public class LexicalException : CompilerException
	{
		public LexicalException(string message) : base(message) { }
	}

	public class SyntaxException : CompilerException
	{
		public SyntaxException(string message, string filename, int lineNumber, int charCount) 
			: base(message, filename, lineNumber, charCount) { }
	}

	public class SemanticException : CompilerException
	{
		public SemanticException(string message) : base(message) { }
	}

}
