using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Dynamic;
using System.Linq.Expressions;
using Kurogane.Types;

namespace Kurogane.Dynamic {

	/// <summary>
	/// 型からBinderを引くためのヘルパクラス
	/// </summary>
	public class MetaObjectLoader {

		public static DynamicMetaObject Create(object obj, Expression expr) {
			if (obj is Pair)
				return new PairMetaObject((Pair)obj, expr);

			return null;
		}

	}
}
