using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kurogane.Libraries {

	public class NameAttribute : Attribute {
		public string Name { get; private set; }
		public NameAttribute(string name) {
			this.Name = name;
		}
	}

}
