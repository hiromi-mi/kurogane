using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Dynamic;

namespace Kurogane.Dynamics {

	public class KrgnInvokeMemberBinder : InvokeMemberBinder {

		public KrgnInvokeMemberBinder(string name, CallInfo callInfo)
			: base(name, false, callInfo) {
		}

		public override DynamicMetaObject FallbackInvoke(DynamicMetaObject target, DynamicMetaObject[] args, DynamicMetaObject errorSuggestion) {
			throw new NotImplementedException();
		}

		public override DynamicMetaObject FallbackInvokeMember(DynamicMetaObject target, DynamicMetaObject[] args, DynamicMetaObject errorSuggestion) {
			throw new NotImplementedException();
		}
	}
}
