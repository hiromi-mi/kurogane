using System;
using System.Dynamic;
using Kurogane.Dynamic;
using System.Linq.Expressions;

namespace Kurogane.RuntimeBinder {

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

		public readonly DynamicMetaObjectBinder AddBinder = new ArithmeticBinder(ExpressionType.Add, "加算", "op_Addition");
		public readonly DynamicMetaObjectBinder SubBinder = new ArithmeticBinder(ExpressionType.Subtract, "減算", "op_Subtraction");
		public readonly DynamicMetaObjectBinder MultBinder = new ArithmeticBinder(ExpressionType.Multiply, "乗算", "op_Multiply");
		public readonly DynamicMetaObjectBinder DivideBinder = new ArithmeticBinder(ExpressionType.Multiply, "除算", "op_Division");
		public readonly DynamicMetaObjectBinder ModBinder = new ArithmeticBinder(ExpressionType.Modulo, "剰余算", "op_Modulus");



	}
}
