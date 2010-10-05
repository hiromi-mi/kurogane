using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kurogane.Compilers
{

	public class CompilerException : KrgnException
	{
		public CompilerException(string message) : base(message) { }
	}

	public class LexicalException : CompilerException
	{
		public LexicalException(string message) : base(message) { }
	}

	public class SyntaxException : CompilerException
	{
		public SyntaxException(string message) : base(message) { }
	}

	public class SemanticException : CompilerException
	{
		public SemanticException(string message) : base(message) { }
	}

}
