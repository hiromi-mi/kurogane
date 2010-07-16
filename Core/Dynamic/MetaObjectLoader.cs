using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Dynamic;
using System.Linq.Expressions;

namespace Kurogane.Dynamic {

	/// <summary>
	/// 型からBinderを引くためのヘルパクラス
	/// </summary>
	public class MetaObjectLoader {

		public static DynamicMetaObject Create(object obj, Expression expr) {

			if (obj is IPair) {
				return new PairMetaObject((IPair)obj, expr);
			}

			return null;
		}

	}
}
