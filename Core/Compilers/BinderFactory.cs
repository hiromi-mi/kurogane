using System.Dynamic;
using Kurogane.Dynamic;

namespace Kurogane.Compiler {

	/// <summary>
	/// DynamicMetaObjectBinderを返すファクトリクラス。
	/// </summary>
	public class BinderFactory {

		public DynamicMetaObjectBinder InvokeBinder(CallInfo callInfo) {
			return new KrgnInvokeBinder(callInfo);
		}

		public DynamicMetaObjectBinder GetMemberBinder(string name) {
			return new KrgnGetMemberBinder(name);
		}

		
	}
}
