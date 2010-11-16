using System;
using System.Dynamic;
using Kurogane.Dynamic;
using System.Linq.Expressions;
using System.Collections.Generic;

namespace Kurogane.RuntimeBinder {

	/// <summary>
	/// DynamicMetaObjectBinderを返すファクトリクラス。
	/// </summary>
	public class BinderFactory {

		// ----- ----- ----- ----- fields ----- ----- ----- -----
		#region Binderのキャッシュ

		private readonly DynamicMetaObjectBinder _Not = new NotBinder();
		private readonly DynamicMetaObjectBinder _NegateBinder = new NegateBinder();

		private readonly DynamicMetaObjectBinder _AddBinder = new ArithmeticBinder(ExpressionType.Add, "加算");
		private readonly DynamicMetaObjectBinder _SubBinder = new ArithmeticBinder(ExpressionType.Subtract, "減算");
		private readonly DynamicMetaObjectBinder _MultBinder = new ArithmeticBinder(ExpressionType.Multiply, "乗算");
		private readonly DynamicMetaObjectBinder _DivideBinder = new ArithmeticBinder(ExpressionType.Divide, "除算");
		private readonly DynamicMetaObjectBinder _ModBinder = new ArithmeticBinder(ExpressionType.Modulo, "剰余算");

		private readonly DynamicMetaObjectBinder _LessThanBinder =
			new ComparingBinder(ExpressionType.LessThan, "op_LessThan", val => val < 0);
		private readonly DynamicMetaObjectBinder _GreaterThanBinder =
			new ComparingBinder(ExpressionType.GreaterThan, "op_GreaterThan", val => val > 0);
		private readonly DynamicMetaObjectBinder _LessThanOrEqualBinder =
			new ComparingBinder(ExpressionType.LessThanOrEqual, "op_LessThanOrEqual", val => val <= 0);
		private readonly DynamicMetaObjectBinder _GreaterThanOrEqualBinder =
			new ComparingBinder(ExpressionType.GreaterThanOrEqual, "op_GreaterThanOrEqual", val => val >= 0);

		private readonly DynamicMetaObjectBinder _EqualBinder = new EqualityBinder(ExpressionType.Equal, "op_Equality");
		private readonly DynamicMetaObjectBinder _NotEqualBinder = new EqualityBinder(ExpressionType.NotEqual, "op_Inequality");

		private readonly DynamicMetaObjectBinder _AndBinder = new AndOperationBinder();
		private readonly DynamicMetaObjectBinder _OrBinder = new OrOperationBinder();

		private readonly DynamicMetaObjectBinder _NotBinder = new NotOperationBinder();
		private readonly DynamicMetaObjectBinder _ToBoolBinder = new ToBoolBinder();
		private readonly DynamicMetaObjectBinder _MapBinder = new MapBinder();

		private readonly Dictionary<string, DynamicMetaObjectBinder> _GetMemberCache = new Dictionary<string, DynamicMetaObjectBinder>();
		private readonly Dictionary<string, DynamicMetaObjectBinder> _SetMemberCache = new Dictionary<string, DynamicMetaObjectBinder>();

		#endregion

		// ----- ----- ----- ----- properties ----- ----- ----- -----

		public virtual DynamicMetaObjectBinder AddBinder { get { return _AddBinder; } }
		public virtual DynamicMetaObjectBinder SubBinder { get { return _SubBinder; } }
		public virtual DynamicMetaObjectBinder MultBinder { get { return _MultBinder; } }
		public virtual DynamicMetaObjectBinder DivideBinder { get { return _DivideBinder; } }
		public virtual DynamicMetaObjectBinder ModBinder { get { return _ModBinder; } }
		public virtual DynamicMetaObjectBinder NegateBinder { get { return _NegateBinder; } }

		public virtual DynamicMetaObjectBinder LessThanBinder { get { return _LessThanBinder; } }
		public virtual DynamicMetaObjectBinder GreaterThanBinder { get { return _GreaterThanBinder; } }
		public virtual DynamicMetaObjectBinder LessThanOrEqualBinder { get { return _LessThanOrEqualBinder; } }
		public virtual DynamicMetaObjectBinder GreaterThanOrEqualBinder { get { return _GreaterThanOrEqualBinder; } }

		public virtual DynamicMetaObjectBinder NotBinder { get { return _NotBinder; } }
		public virtual DynamicMetaObjectBinder EqualBinder { get { return _EqualBinder; } }
		public virtual DynamicMetaObjectBinder NotEqualBinder { get { return _NotEqualBinder; } }

		public virtual DynamicMetaObjectBinder AndBinder { get { return _AndBinder; } }
		public virtual DynamicMetaObjectBinder OrBinder { get { return _OrBinder; } }

		public virtual DynamicMetaObjectBinder ToBoolBinder { get { return _ToBoolBinder; } }

		public virtual DynamicMetaObjectBinder MapBinder { get { return _MapBinder; } }

		// ----- ----- ----- ----- methods ----- ----- ----- -----

		public virtual DynamicMetaObjectBinder InvokeBinder(CallInfo callInfo) {
			return new KrgnInvokeBinder(callInfo);
		}

		public virtual DynamicMetaObjectBinder InvokeMemberBinder(string name, CallInfo callInfo) {
			return new KrgnInvokeMemberBinder(name, callInfo, this);
		}

		public virtual DynamicMetaObjectBinder GetMemberBinder(string name) {
			DynamicMetaObjectBinder binder;
			if (_GetMemberCache.TryGetValue(name, out binder))
				return binder;
			return _GetMemberCache[name] = new KrgnGetMemberBinder(name);
		}

		public virtual DynamicMetaObjectBinder SetMemberBinder(string name) {
			DynamicMetaObjectBinder binder;
			if (_SetMemberCache.TryGetValue(name, out binder))
				return binder;
			return _SetMemberCache[name] = new KrgnSetMemberBinder(name);
		}

	}
}
