using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kurogane.Compiler {

	public class DocumentLocation {
		public int StartLine { get; private set; }
		public int StartColumn { get; private set; }
		public int EndLine { get; private set; }
		public int EndColumn { get; private set; }

		public DocumentLocation(int startLine, int startColumn, int endLine, int endColumn) {
			this.StartLine = startLine;
			this.StartColumn = startColumn;
			this.EndLine = EndLine;
			this.EndColumn = endColumn;
		}
	}

}
