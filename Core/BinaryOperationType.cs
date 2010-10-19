using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kurogane {

	/// <summary>
	/// 組み込みの二項演算子の種類を表す。
	/// </summary>
	public enum BinaryOperationType {

		Unknown = 0,

		And,
		Or,

		Equal,
		NotEqual,
		GreaterThan,
		GreaterThanOrEqual,
		LessThan,
		LessThanOrEqual,

		Add,
		Subtract,
		Multiply,
		Divide,
		Modulo,

		Concat,
	}
}
