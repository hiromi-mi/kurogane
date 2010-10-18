using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kurogane {

	/// <summary>
	/// 組み込みの単項演算子の種類を表す。
	/// </summary>
	public enum UnaryOperationType {
		
		Unknown = 0,

		Not,

		Negate,
	}
}
