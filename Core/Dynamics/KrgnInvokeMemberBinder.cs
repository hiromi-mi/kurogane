using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Dynamic;

namespace Kurogane.Dynamic {

	public class KrgnInvokeMemberBinder : InvokeMemberBinder {

		public KrgnInvokeMemberBinder(string name, CallInfo callInfo)
			: base(name, false, callInfo) {
		}

		public override DynamicMetaObject FallbackInvoke(DynamicMetaObject target, DynamicMetaObject[] args, DynamicMetaObject errorSuggestion) {
			// 明示的に呼び出さない限り呼ばれない。
			throw new NotImplementedException();
		}

		public override DynamicMetaObject FallbackInvokeMember(DynamicMetaObject target, DynamicMetaObject[] args, DynamicMetaObject errorSuggestion) {
			foreach (var meta in args) {
			}


			throw new NotImplementedException();
		}
	}
}
