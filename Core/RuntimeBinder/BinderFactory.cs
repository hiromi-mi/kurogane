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

		public readonly DynamicMetaObjectBinder LessThanBinder =
			new ComparingBinder(ExpressionType.LessThan, "op_LessThan", val => val < 0);
		public readonly DynamicMetaObjectBinder GreaterThanBinder =
			new ComparingBinder(ExpressionType.GreaterThan, "op_GreaterThan", val => val > 0);
		public readonly DynamicMetaObjectBinder LessThanOrEqualBinder =
			new ComparingBinder(ExpressionType.LessThanOrEqual, "op_LessThanOrEqual", val => val <= 0);
		public readonly DynamicMetaObjectBinder GreaterThanOrEqualBinder =
			new ComparingBinder(ExpressionType.GreaterThanOrEqual, "op_GreaterThanOrEqual", val => val >= 0);

		public readonly DynamicMetaObjectBinder EqualBinder = new EqualityBinder(ExpressionType.Equal, "op_Equality");
		public readonly DynamicMetaObjectBinder NotEqualBinder = new EqualityBinder(ExpressionType.NotEqual, "op_Inequality");

		public readonly DynamicMetaObjectBinder AndBinder = new AndOperationBinder();
		public readonly DynamicMetaObjectBinder OrBinder = new OrOperationBinder();

		public readonly DynamicMetaObjectBinder NotBinder = new NotOperationBinder();

	}
}
