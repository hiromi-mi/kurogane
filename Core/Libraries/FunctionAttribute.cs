using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kurogane.Libraries {

	public class FunctionAttribute : Attribute {

		public string Name { get; private set; }
		public string[] Suffix { get; private set; }

		public FunctionAttribute(string name, params string[] suffix) {
			this.Name = name;
			this.Suffix = suffix;
		}
	}
}
