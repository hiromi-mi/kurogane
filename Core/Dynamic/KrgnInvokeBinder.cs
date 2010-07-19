using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Dynamic;
using System.Linq.Expressions;

namespace Kurogane.Dynamic {

	/// <summary>
	/// 助詞付き関数呼び出しのBinder
	/// </summary>
	public class KrgnInvokeBinder : InvokeBinder {

		public KrgnInvokeBinder(CallInfo callInfo)
			: base(callInfo) {
		}

		public override DynamicMetaObject FallbackInvoke(DynamicMetaObject target, DynamicMetaObject[] args, DynamicMetaObject errorSuggestion) {
			throw new NotImplementedException();
		}
	}
}
