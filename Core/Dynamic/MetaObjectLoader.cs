using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Dynamic;
using System.Linq.Expressions;

namespace Kurogane.Dynamic {

	/// <summary>
	/// 型からMetaObjectを引くためのヘルパクラス
	/// </summary>
	public class MetaObjectLoader {

		public static DynamicMetaObject Create(object obj, Expression expr) {
			if (obj is Tuple<object,object>)
				return new PairMetaObject((Tuple<object, object>)obj, expr);

			return null;
		}

	}
}
