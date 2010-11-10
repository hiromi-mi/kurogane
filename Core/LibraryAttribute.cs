using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kurogane {

	[AttributeUsage(AttributeTargets.Class)]
	public class LibraryAttribute : Attribute {
	}

	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Method | AttributeTargets.Field)]
	public class JpNameAttribute : Attribute {
		public string Name { get; private set; }
		public JpNameAttribute(string name) {
			this.Name = name;
		}
	}

	[AttributeUsage(AttributeTargets.Parameter)]
	public class SuffixAttribute : Attribute {
		public string Name { get; private set; }
		public SuffixAttribute(string name) {
			this.Name = name;
		}
	}
}
