using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kurogane.Libraries {

	[JpName("対"), AliasFor(typeof(Tuple<object, object>))]
	internal interface TupleAlias {

		[JpName(ConstantNames.Head)]
		object Item1 { get; }

		[JpName(ConstantNames.Tail)]
		object Item2 { get; }
	}

}
