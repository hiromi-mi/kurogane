using System.Dynamic;
using Kurogane.RuntimeBinder;
using Produire.TypeModel;

namespace Kurogane.Interop.Produire.RuntimeBinder {

	public class ProduireBinderFactory : BinderFactory {

		public ReferenceCollection Reference { get; set; }

		public override DynamicMetaObjectBinder SetMemberBinder(string name) {
			return new ProduireSetMemberBinder(name, this.Reference);
		}

		public override DynamicMetaObjectBinder InvokeMemberBinder(string name, CallInfo callInfo) {
			return new ProduireInvokeMemberBinder(name, callInfo, this, this.Reference);
		}
	}
}
