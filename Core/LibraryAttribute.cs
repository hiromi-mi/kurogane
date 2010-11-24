using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kurogane {

	[AttributeUsage(AttributeTargets.Class)]
	public sealed class LibraryAttribute : Attribute {
	}

	[AttributeUsage(AttributeTargets.Interface | AttributeTargets.Class)]
	public sealed class AliasForAttribute : Attribute {
		public Type Type { get; private set; }
		public AliasForAttribute(Type type) {
			this.Type = type;
		}
	}

	[AttributeUsage(
		AttributeTargets.Interface |
		AttributeTargets.Class |
		AttributeTargets.Property |
		AttributeTargets.Method |
		AttributeTargets.Method |
		AttributeTargets.Field)]
	public sealed class JpNameAttribute : Attribute {
		public string Name { get; private set; }
		public JpNameAttribute(string name) {
			this.Name = name;
		}
	}

	[AttributeUsage(AttributeTargets.Parameter)]
	public sealed class SuffixAttribute : Attribute {
		public string Name { get; private set; }
		public SuffixAttribute(string name) {
			this.Name = name;
		}
	}
}
