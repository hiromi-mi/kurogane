 

using System;
using System.Collections.ObjectModel;
using System.Dynamic;
using System.Linq.Expressions;
using System.Reflection;

namespace Kurogane.Types {

public interface IKrgnFunc {
	Delegate Func { get; }
	ReadOnlyCollection<string> Suffixes { get; }
}

public static class KrgnFunc {

	public static Type GetTypeOf(int n) {
		switch(n) {
		case 0: return typeof(KrgnFunc0);
		case 1: return typeof(KrgnFunc1);
		case 2: return typeof(KrgnFunc2);
		case 3: return typeof(KrgnFunc3);
		case 4: return typeof(KrgnFunc4);
		case 5: return typeof(KrgnFunc5);
		case 6: return typeof(KrgnFunc6);
		case 7: return typeof(KrgnFunc7);
		case 8: return typeof(KrgnFunc8);
		case 9: return typeof(KrgnFunc9);
		}
		throw new IndexOutOfRangeException("n must be less than 10");
	}
	
	public static ConstructorInfo GetConstructorInfo(int n) {
		switch(n) {
		case 0: return typeof(KrgnFunc0).GetConstructor(new[] { typeof(Func<object>), typeof(string[]) });
		case 1: return typeof(KrgnFunc1).GetConstructor(new[] { typeof(Func<object,object>), typeof(string[]) });
		case 2: return typeof(KrgnFunc2).GetConstructor(new[] { typeof(Func<object,object,object>), typeof(string[]) });
		case 3: return typeof(KrgnFunc3).GetConstructor(new[] { typeof(Func<object,object,object,object>), typeof(string[]) });
		case 4: return typeof(KrgnFunc4).GetConstructor(new[] { typeof(Func<object,object,object,object,object>), typeof(string[]) });
		case 5: return typeof(KrgnFunc5).GetConstructor(new[] { typeof(Func<object,object,object,object,object,object>), typeof(string[]) });
		case 6: return typeof(KrgnFunc6).GetConstructor(new[] { typeof(Func<object,object,object,object,object,object,object>), typeof(string[]) });
		case 7: return typeof(KrgnFunc7).GetConstructor(new[] { typeof(Func<object,object,object,object,object,object,object,object>), typeof(string[]) });
		case 8: return typeof(KrgnFunc8).GetConstructor(new[] { typeof(Func<object,object,object,object,object,object,object,object,object>), typeof(string[]) });
		case 9: return typeof(KrgnFunc9).GetConstructor(new[] { typeof(Func<object,object,object,object,object,object,object,object,object,object>), typeof(string[]) });
		}
		throw new IndexOutOfRangeException("n must be less than 10");
	}
	
	public static IKrgnFunc Create(Func<object> func, params string[] suffixes) {
		return new KrgnFunc0(func, suffixes);
	}
	public static IKrgnFunc Create(Func<object,object> func, params string[] suffixes) {
		return new KrgnFunc1(func, suffixes);
	}
	public static IKrgnFunc Create(Func<object,object,object> func, params string[] suffixes) {
		return new KrgnFunc2(func, suffixes);
	}
	public static IKrgnFunc Create(Func<object,object,object,object> func, params string[] suffixes) {
		return new KrgnFunc3(func, suffixes);
	}
	public static IKrgnFunc Create(Func<object,object,object,object,object> func, params string[] suffixes) {
		return new KrgnFunc4(func, suffixes);
	}
	public static IKrgnFunc Create(Func<object,object,object,object,object,object> func, params string[] suffixes) {
		return new KrgnFunc5(func, suffixes);
	}
	public static IKrgnFunc Create(Func<object,object,object,object,object,object,object> func, params string[] suffixes) {
		return new KrgnFunc6(func, suffixes);
	}
	public static IKrgnFunc Create(Func<object,object,object,object,object,object,object,object> func, params string[] suffixes) {
		return new KrgnFunc7(func, suffixes);
	}
	public static IKrgnFunc Create(Func<object,object,object,object,object,object,object,object,object> func, params string[] suffixes) {
		return new KrgnFunc8(func, suffixes);
	}
	public static IKrgnFunc Create(Func<object,object,object,object,object,object,object,object,object,object> func, params string[] suffixes) {
		return new KrgnFunc9(func, suffixes);
	}

private class KrgnFunc0 : IKrgnFunc, IDynamicMetaObjectProvider {
	private readonly string[] _suffixes;
	private readonly Func<object> _func;
	
	public KrgnFunc0(Func<object> func, string[] suffixes) {
		this._func = func;
		this._suffixes = suffixes;
	}

	Delegate IKrgnFunc.Func { get { return _func; } }
	ReadOnlyCollection<string> IKrgnFunc.Suffixes {
		get { return new ReadOnlyCollection<string>(_suffixes); }
	}
	
	public DynamicMetaObject GetMetaObject(Expression parameter) {
		return new MetaObject(this, parameter);
	}
	
	private class MetaObject : DynamicMetaObject {
		public MetaObject(KrgnFunc0 self, Expression expr)
			: base(expr, BindingRestrictions.GetTypeRestriction(expr, typeof(KrgnFunc0)), self) {
		}
		public override DynamicMetaObject BindInvoke(InvokeBinder binder, DynamicMetaObject[] args) {
			var value = (KrgnFunc0)base.Value;
			Expression[] procArgs = new Expression[0];
			int offset = binder.CallInfo.ArgumentCount - binder.CallInfo.ArgumentNames.Count;

			Expression target = Expression.Convert(base.Expression, base.LimitType);
			Expression invokeExpr = Expression.Invoke(
				 Expression.Convert(Expression.PropertyOrField(target, "_func"), typeof(Func<object>)),
				 procArgs);
			Expression restExpr = Expression.TypeIs(base.Expression, typeof(KrgnFunc0));
			Expression suffProperty = Expression.PropertyOrField(target, "_suffixes");
			var lambda = Expression.Lambda<Func<object, bool>>(restExpr, (ParameterExpression)base.Expression);
			var func = lambda.Compile();
			var result = func(value);
			return new DynamicMetaObject(invokeExpr,
				BindingRestrictions.GetExpressionRestriction(restExpr));
		}
	}
}
private class KrgnFunc1 : IKrgnFunc, IDynamicMetaObjectProvider {
	private readonly string[] _suffixes;
	private readonly Func<object,object> _func;
	
	public KrgnFunc1(Func<object,object> func, string[] suffixes) {
		this._func = func;
		this._suffixes = suffixes;
	}

	Delegate IKrgnFunc.Func { get { return _func; } }
	ReadOnlyCollection<string> IKrgnFunc.Suffixes {
		get { return new ReadOnlyCollection<string>(_suffixes); }
	}
	
	public DynamicMetaObject GetMetaObject(Expression parameter) {
		return new MetaObject(this, parameter);
	}
	
	private class MetaObject : DynamicMetaObject {
		public MetaObject(KrgnFunc1 self, Expression expr)
			: base(expr, BindingRestrictions.GetTypeRestriction(expr, typeof(KrgnFunc1)), self) {
		}
		public override DynamicMetaObject BindInvoke(InvokeBinder binder, DynamicMetaObject[] args) {
			var value = (KrgnFunc1)base.Value;
			Expression[] procArgs = new Expression[1];
			int offset = binder.CallInfo.ArgumentCount - binder.CallInfo.ArgumentNames.Count;
			procArgs[0] = Expression.Convert(
				args[binder.CallInfo.ArgumentNames.IndexOf(value._suffixes[0]) + offset].Expression,
				typeof(object));

			Expression target = Expression.Convert(base.Expression, base.LimitType);
			Expression invokeExpr = Expression.Invoke(
				 Expression.Convert(Expression.PropertyOrField(target, "_func"), typeof(Func<object,object>)),
				 procArgs);
			Expression restExpr = Expression.TypeIs(base.Expression, typeof(KrgnFunc1));
			Expression suffProperty = Expression.PropertyOrField(target, "_suffixes");
			restExpr = Expression.AndAlso(restExpr, Expression.Equal(
				Expression.ArrayIndex(suffProperty, Expression.Constant(0)),
				Expression.Constant(value._suffixes[0])));
			var lambda = Expression.Lambda<Func<object, bool>>(restExpr, (ParameterExpression)base.Expression);
			var func = lambda.Compile();
			var result = func(value);
			return new DynamicMetaObject(invokeExpr,
				BindingRestrictions.GetExpressionRestriction(restExpr));
		}
	}
}
private class KrgnFunc2 : IKrgnFunc, IDynamicMetaObjectProvider {
	private readonly string[] _suffixes;
	private readonly Func<object,object,object> _func;
	
	public KrgnFunc2(Func<object,object,object> func, string[] suffixes) {
		this._func = func;
		this._suffixes = suffixes;
	}

	Delegate IKrgnFunc.Func { get { return _func; } }
	ReadOnlyCollection<string> IKrgnFunc.Suffixes {
		get { return new ReadOnlyCollection<string>(_suffixes); }
	}
	
	public DynamicMetaObject GetMetaObject(Expression parameter) {
		return new MetaObject(this, parameter);
	}
	
	private class MetaObject : DynamicMetaObject {
		public MetaObject(KrgnFunc2 self, Expression expr)
			: base(expr, BindingRestrictions.GetTypeRestriction(expr, typeof(KrgnFunc2)), self) {
		}
		public override DynamicMetaObject BindInvoke(InvokeBinder binder, DynamicMetaObject[] args) {
			var value = (KrgnFunc2)base.Value;
			Expression[] procArgs = new Expression[2];
			int offset = binder.CallInfo.ArgumentCount - binder.CallInfo.ArgumentNames.Count;
			procArgs[0] = Expression.Convert(
				args[binder.CallInfo.ArgumentNames.IndexOf(value._suffixes[0]) + offset].Expression,
				typeof(object));
			procArgs[1] = Expression.Convert(
				args[binder.CallInfo.ArgumentNames.IndexOf(value._suffixes[1]) + offset].Expression,
				typeof(object));

			Expression target = Expression.Convert(base.Expression, base.LimitType);
			Expression invokeExpr = Expression.Invoke(
				 Expression.Convert(Expression.PropertyOrField(target, "_func"), typeof(Func<object,object,object>)),
				 procArgs);
			Expression restExpr = Expression.TypeIs(base.Expression, typeof(KrgnFunc2));
			Expression suffProperty = Expression.PropertyOrField(target, "_suffixes");
			restExpr = Expression.AndAlso(restExpr, Expression.Equal(
				Expression.ArrayIndex(suffProperty, Expression.Constant(0)),
				Expression.Constant(value._suffixes[0])));
			restExpr = Expression.AndAlso(restExpr, Expression.Equal(
				Expression.ArrayIndex(suffProperty, Expression.Constant(1)),
				Expression.Constant(value._suffixes[1])));
			var lambda = Expression.Lambda<Func<object, bool>>(restExpr, (ParameterExpression)base.Expression);
			var func = lambda.Compile();
			var result = func(value);
			return new DynamicMetaObject(invokeExpr,
				BindingRestrictions.GetExpressionRestriction(restExpr));
		}
	}
}
private class KrgnFunc3 : IKrgnFunc, IDynamicMetaObjectProvider {
	private readonly string[] _suffixes;
	private readonly Func<object,object,object,object> _func;
	
	public KrgnFunc3(Func<object,object,object,object> func, string[] suffixes) {
		this._func = func;
		this._suffixes = suffixes;
	}

	Delegate IKrgnFunc.Func { get { return _func; } }
	ReadOnlyCollection<string> IKrgnFunc.Suffixes {
		get { return new ReadOnlyCollection<string>(_suffixes); }
	}
	
	public DynamicMetaObject GetMetaObject(Expression parameter) {
		return new MetaObject(this, parameter);
	}
	
	private class MetaObject : DynamicMetaObject {
		public MetaObject(KrgnFunc3 self, Expression expr)
			: base(expr, BindingRestrictions.GetTypeRestriction(expr, typeof(KrgnFunc3)), self) {
		}
		public override DynamicMetaObject BindInvoke(InvokeBinder binder, DynamicMetaObject[] args) {
			var value = (KrgnFunc3)base.Value;
			Expression[] procArgs = new Expression[3];
			int offset = binder.CallInfo.ArgumentCount - binder.CallInfo.ArgumentNames.Count;
			procArgs[0] = Expression.Convert(
				args[binder.CallInfo.ArgumentNames.IndexOf(value._suffixes[0]) + offset].Expression,
				typeof(object));
			procArgs[1] = Expression.Convert(
				args[binder.CallInfo.ArgumentNames.IndexOf(value._suffixes[1]) + offset].Expression,
				typeof(object));
			procArgs[2] = Expression.Convert(
				args[binder.CallInfo.ArgumentNames.IndexOf(value._suffixes[2]) + offset].Expression,
				typeof(object));

			Expression target = Expression.Convert(base.Expression, base.LimitType);
			Expression invokeExpr = Expression.Invoke(
				 Expression.Convert(Expression.PropertyOrField(target, "_func"), typeof(Func<object,object,object,object>)),
				 procArgs);
			Expression restExpr = Expression.TypeIs(base.Expression, typeof(KrgnFunc3));
			Expression suffProperty = Expression.PropertyOrField(target, "_suffixes");
			restExpr = Expression.AndAlso(restExpr, Expression.Equal(
				Expression.ArrayIndex(suffProperty, Expression.Constant(0)),
				Expression.Constant(value._suffixes[0])));
			restExpr = Expression.AndAlso(restExpr, Expression.Equal(
				Expression.ArrayIndex(suffProperty, Expression.Constant(1)),
				Expression.Constant(value._suffixes[1])));
			restExpr = Expression.AndAlso(restExpr, Expression.Equal(
				Expression.ArrayIndex(suffProperty, Expression.Constant(2)),
				Expression.Constant(value._suffixes[2])));
			var lambda = Expression.Lambda<Func<object, bool>>(restExpr, (ParameterExpression)base.Expression);
			var func = lambda.Compile();
			var result = func(value);
			return new DynamicMetaObject(invokeExpr,
				BindingRestrictions.GetExpressionRestriction(restExpr));
		}
	}
}
private class KrgnFunc4 : IKrgnFunc, IDynamicMetaObjectProvider {
	private readonly string[] _suffixes;
	private readonly Func<object,object,object,object,object> _func;
	
	public KrgnFunc4(Func<object,object,object,object,object> func, string[] suffixes) {
		this._func = func;
		this._suffixes = suffixes;
	}

	Delegate IKrgnFunc.Func { get { return _func; } }
	ReadOnlyCollection<string> IKrgnFunc.Suffixes {
		get { return new ReadOnlyCollection<string>(_suffixes); }
	}
	
	public DynamicMetaObject GetMetaObject(Expression parameter) {
		return new MetaObject(this, parameter);
	}
	
	private class MetaObject : DynamicMetaObject {
		public MetaObject(KrgnFunc4 self, Expression expr)
			: base(expr, BindingRestrictions.GetTypeRestriction(expr, typeof(KrgnFunc4)), self) {
		}
		public override DynamicMetaObject BindInvoke(InvokeBinder binder, DynamicMetaObject[] args) {
			var value = (KrgnFunc4)base.Value;
			Expression[] procArgs = new Expression[4];
			int offset = binder.CallInfo.ArgumentCount - binder.CallInfo.ArgumentNames.Count;
			procArgs[0] = Expression.Convert(
				args[binder.CallInfo.ArgumentNames.IndexOf(value._suffixes[0]) + offset].Expression,
				typeof(object));
			procArgs[1] = Expression.Convert(
				args[binder.CallInfo.ArgumentNames.IndexOf(value._suffixes[1]) + offset].Expression,
				typeof(object));
			procArgs[2] = Expression.Convert(
				args[binder.CallInfo.ArgumentNames.IndexOf(value._suffixes[2]) + offset].Expression,
				typeof(object));
			procArgs[3] = Expression.Convert(
				args[binder.CallInfo.ArgumentNames.IndexOf(value._suffixes[3]) + offset].Expression,
				typeof(object));

			Expression target = Expression.Convert(base.Expression, base.LimitType);
			Expression invokeExpr = Expression.Invoke(
				 Expression.Convert(Expression.PropertyOrField(target, "_func"), typeof(Func<object,object,object,object,object>)),
				 procArgs);
			Expression restExpr = Expression.TypeIs(base.Expression, typeof(KrgnFunc4));
			Expression suffProperty = Expression.PropertyOrField(target, "_suffixes");
			restExpr = Expression.AndAlso(restExpr, Expression.Equal(
				Expression.ArrayIndex(suffProperty, Expression.Constant(0)),
				Expression.Constant(value._suffixes[0])));
			restExpr = Expression.AndAlso(restExpr, Expression.Equal(
				Expression.ArrayIndex(suffProperty, Expression.Constant(1)),
				Expression.Constant(value._suffixes[1])));
			restExpr = Expression.AndAlso(restExpr, Expression.Equal(
				Expression.ArrayIndex(suffProperty, Expression.Constant(2)),
				Expression.Constant(value._suffixes[2])));
			restExpr = Expression.AndAlso(restExpr, Expression.Equal(
				Expression.ArrayIndex(suffProperty, Expression.Constant(3)),
				Expression.Constant(value._suffixes[3])));
			var lambda = Expression.Lambda<Func<object, bool>>(restExpr, (ParameterExpression)base.Expression);
			var func = lambda.Compile();
			var result = func(value);
			return new DynamicMetaObject(invokeExpr,
				BindingRestrictions.GetExpressionRestriction(restExpr));
		}
	}
}
private class KrgnFunc5 : IKrgnFunc, IDynamicMetaObjectProvider {
	private readonly string[] _suffixes;
	private readonly Func<object,object,object,object,object,object> _func;
	
	public KrgnFunc5(Func<object,object,object,object,object,object> func, string[] suffixes) {
		this._func = func;
		this._suffixes = suffixes;
	}

	Delegate IKrgnFunc.Func { get { return _func; } }
	ReadOnlyCollection<string> IKrgnFunc.Suffixes {
		get { return new ReadOnlyCollection<string>(_suffixes); }
	}
	
	public DynamicMetaObject GetMetaObject(Expression parameter) {
		return new MetaObject(this, parameter);
	}
	
	private class MetaObject : DynamicMetaObject {
		public MetaObject(KrgnFunc5 self, Expression expr)
			: base(expr, BindingRestrictions.GetTypeRestriction(expr, typeof(KrgnFunc5)), self) {
		}
		public override DynamicMetaObject BindInvoke(InvokeBinder binder, DynamicMetaObject[] args) {
			var value = (KrgnFunc5)base.Value;
			Expression[] procArgs = new Expression[5];
			int offset = binder.CallInfo.ArgumentCount - binder.CallInfo.ArgumentNames.Count;
			procArgs[0] = Expression.Convert(
				args[binder.CallInfo.ArgumentNames.IndexOf(value._suffixes[0]) + offset].Expression,
				typeof(object));
			procArgs[1] = Expression.Convert(
				args[binder.CallInfo.ArgumentNames.IndexOf(value._suffixes[1]) + offset].Expression,
				typeof(object));
			procArgs[2] = Expression.Convert(
				args[binder.CallInfo.ArgumentNames.IndexOf(value._suffixes[2]) + offset].Expression,
				typeof(object));
			procArgs[3] = Expression.Convert(
				args[binder.CallInfo.ArgumentNames.IndexOf(value._suffixes[3]) + offset].Expression,
				typeof(object));
			procArgs[4] = Expression.Convert(
				args[binder.CallInfo.ArgumentNames.IndexOf(value._suffixes[4]) + offset].Expression,
				typeof(object));

			Expression target = Expression.Convert(base.Expression, base.LimitType);
			Expression invokeExpr = Expression.Invoke(
				 Expression.Convert(Expression.PropertyOrField(target, "_func"), typeof(Func<object,object,object,object,object,object>)),
				 procArgs);
			Expression restExpr = Expression.TypeIs(base.Expression, typeof(KrgnFunc5));
			Expression suffProperty = Expression.PropertyOrField(target, "_suffixes");
			restExpr = Expression.AndAlso(restExpr, Expression.Equal(
				Expression.ArrayIndex(suffProperty, Expression.Constant(0)),
				Expression.Constant(value._suffixes[0])));
			restExpr = Expression.AndAlso(restExpr, Expression.Equal(
				Expression.ArrayIndex(suffProperty, Expression.Constant(1)),
				Expression.Constant(value._suffixes[1])));
			restExpr = Expression.AndAlso(restExpr, Expression.Equal(
				Expression.ArrayIndex(suffProperty, Expression.Constant(2)),
				Expression.Constant(value._suffixes[2])));
			restExpr = Expression.AndAlso(restExpr, Expression.Equal(
				Expression.ArrayIndex(suffProperty, Expression.Constant(3)),
				Expression.Constant(value._suffixes[3])));
			restExpr = Expression.AndAlso(restExpr, Expression.Equal(
				Expression.ArrayIndex(suffProperty, Expression.Constant(4)),
				Expression.Constant(value._suffixes[4])));
			var lambda = Expression.Lambda<Func<object, bool>>(restExpr, (ParameterExpression)base.Expression);
			var func = lambda.Compile();
			var result = func(value);
			return new DynamicMetaObject(invokeExpr,
				BindingRestrictions.GetExpressionRestriction(restExpr));
		}
	}
}
private class KrgnFunc6 : IKrgnFunc, IDynamicMetaObjectProvider {
	private readonly string[] _suffixes;
	private readonly Func<object,object,object,object,object,object,object> _func;
	
	public KrgnFunc6(Func<object,object,object,object,object,object,object> func, string[] suffixes) {
		this._func = func;
		this._suffixes = suffixes;
	}

	Delegate IKrgnFunc.Func { get { return _func; } }
	ReadOnlyCollection<string> IKrgnFunc.Suffixes {
		get { return new ReadOnlyCollection<string>(_suffixes); }
	}
	
	public DynamicMetaObject GetMetaObject(Expression parameter) {
		return new MetaObject(this, parameter);
	}
	
	private class MetaObject : DynamicMetaObject {
		public MetaObject(KrgnFunc6 self, Expression expr)
			: base(expr, BindingRestrictions.GetTypeRestriction(expr, typeof(KrgnFunc6)), self) {
		}
		public override DynamicMetaObject BindInvoke(InvokeBinder binder, DynamicMetaObject[] args) {
			var value = (KrgnFunc6)base.Value;
			Expression[] procArgs = new Expression[6];
			int offset = binder.CallInfo.ArgumentCount - binder.CallInfo.ArgumentNames.Count;
			procArgs[0] = Expression.Convert(
				args[binder.CallInfo.ArgumentNames.IndexOf(value._suffixes[0]) + offset].Expression,
				typeof(object));
			procArgs[1] = Expression.Convert(
				args[binder.CallInfo.ArgumentNames.IndexOf(value._suffixes[1]) + offset].Expression,
				typeof(object));
			procArgs[2] = Expression.Convert(
				args[binder.CallInfo.ArgumentNames.IndexOf(value._suffixes[2]) + offset].Expression,
				typeof(object));
			procArgs[3] = Expression.Convert(
				args[binder.CallInfo.ArgumentNames.IndexOf(value._suffixes[3]) + offset].Expression,
				typeof(object));
			procArgs[4] = Expression.Convert(
				args[binder.CallInfo.ArgumentNames.IndexOf(value._suffixes[4]) + offset].Expression,
				typeof(object));
			procArgs[5] = Expression.Convert(
				args[binder.CallInfo.ArgumentNames.IndexOf(value._suffixes[5]) + offset].Expression,
				typeof(object));

			Expression target = Expression.Convert(base.Expression, base.LimitType);
			Expression invokeExpr = Expression.Invoke(
				 Expression.Convert(Expression.PropertyOrField(target, "_func"), typeof(Func<object,object,object,object,object,object,object>)),
				 procArgs);
			Expression restExpr = Expression.TypeIs(base.Expression, typeof(KrgnFunc6));
			Expression suffProperty = Expression.PropertyOrField(target, "_suffixes");
			restExpr = Expression.AndAlso(restExpr, Expression.Equal(
				Expression.ArrayIndex(suffProperty, Expression.Constant(0)),
				Expression.Constant(value._suffixes[0])));
			restExpr = Expression.AndAlso(restExpr, Expression.Equal(
				Expression.ArrayIndex(suffProperty, Expression.Constant(1)),
				Expression.Constant(value._suffixes[1])));
			restExpr = Expression.AndAlso(restExpr, Expression.Equal(
				Expression.ArrayIndex(suffProperty, Expression.Constant(2)),
				Expression.Constant(value._suffixes[2])));
			restExpr = Expression.AndAlso(restExpr, Expression.Equal(
				Expression.ArrayIndex(suffProperty, Expression.Constant(3)),
				Expression.Constant(value._suffixes[3])));
			restExpr = Expression.AndAlso(restExpr, Expression.Equal(
				Expression.ArrayIndex(suffProperty, Expression.Constant(4)),
				Expression.Constant(value._suffixes[4])));
			restExpr = Expression.AndAlso(restExpr, Expression.Equal(
				Expression.ArrayIndex(suffProperty, Expression.Constant(5)),
				Expression.Constant(value._suffixes[5])));
			var lambda = Expression.Lambda<Func<object, bool>>(restExpr, (ParameterExpression)base.Expression);
			var func = lambda.Compile();
			var result = func(value);
			return new DynamicMetaObject(invokeExpr,
				BindingRestrictions.GetExpressionRestriction(restExpr));
		}
	}
}
private class KrgnFunc7 : IKrgnFunc, IDynamicMetaObjectProvider {
	private readonly string[] _suffixes;
	private readonly Func<object,object,object,object,object,object,object,object> _func;
	
	public KrgnFunc7(Func<object,object,object,object,object,object,object,object> func, string[] suffixes) {
		this._func = func;
		this._suffixes = suffixes;
	}

	Delegate IKrgnFunc.Func { get { return _func; } }
	ReadOnlyCollection<string> IKrgnFunc.Suffixes {
		get { return new ReadOnlyCollection<string>(_suffixes); }
	}
	
	public DynamicMetaObject GetMetaObject(Expression parameter) {
		return new MetaObject(this, parameter);
	}
	
	private class MetaObject : DynamicMetaObject {
		public MetaObject(KrgnFunc7 self, Expression expr)
			: base(expr, BindingRestrictions.GetTypeRestriction(expr, typeof(KrgnFunc7)), self) {
		}
		public override DynamicMetaObject BindInvoke(InvokeBinder binder, DynamicMetaObject[] args) {
			var value = (KrgnFunc7)base.Value;
			Expression[] procArgs = new Expression[7];
			int offset = binder.CallInfo.ArgumentCount - binder.CallInfo.ArgumentNames.Count;
			procArgs[0] = Expression.Convert(
				args[binder.CallInfo.ArgumentNames.IndexOf(value._suffixes[0]) + offset].Expression,
				typeof(object));
			procArgs[1] = Expression.Convert(
				args[binder.CallInfo.ArgumentNames.IndexOf(value._suffixes[1]) + offset].Expression,
				typeof(object));
			procArgs[2] = Expression.Convert(
				args[binder.CallInfo.ArgumentNames.IndexOf(value._suffixes[2]) + offset].Expression,
				typeof(object));
			procArgs[3] = Expression.Convert(
				args[binder.CallInfo.ArgumentNames.IndexOf(value._suffixes[3]) + offset].Expression,
				typeof(object));
			procArgs[4] = Expression.Convert(
				args[binder.CallInfo.ArgumentNames.IndexOf(value._suffixes[4]) + offset].Expression,
				typeof(object));
			procArgs[5] = Expression.Convert(
				args[binder.CallInfo.ArgumentNames.IndexOf(value._suffixes[5]) + offset].Expression,
				typeof(object));
			procArgs[6] = Expression.Convert(
				args[binder.CallInfo.ArgumentNames.IndexOf(value._suffixes[6]) + offset].Expression,
				typeof(object));

			Expression target = Expression.Convert(base.Expression, base.LimitType);
			Expression invokeExpr = Expression.Invoke(
				 Expression.Convert(Expression.PropertyOrField(target, "_func"), typeof(Func<object,object,object,object,object,object,object,object>)),
				 procArgs);
			Expression restExpr = Expression.TypeIs(base.Expression, typeof(KrgnFunc7));
			Expression suffProperty = Expression.PropertyOrField(target, "_suffixes");
			restExpr = Expression.AndAlso(restExpr, Expression.Equal(
				Expression.ArrayIndex(suffProperty, Expression.Constant(0)),
				Expression.Constant(value._suffixes[0])));
			restExpr = Expression.AndAlso(restExpr, Expression.Equal(
				Expression.ArrayIndex(suffProperty, Expression.Constant(1)),
				Expression.Constant(value._suffixes[1])));
			restExpr = Expression.AndAlso(restExpr, Expression.Equal(
				Expression.ArrayIndex(suffProperty, Expression.Constant(2)),
				Expression.Constant(value._suffixes[2])));
			restExpr = Expression.AndAlso(restExpr, Expression.Equal(
				Expression.ArrayIndex(suffProperty, Expression.Constant(3)),
				Expression.Constant(value._suffixes[3])));
			restExpr = Expression.AndAlso(restExpr, Expression.Equal(
				Expression.ArrayIndex(suffProperty, Expression.Constant(4)),
				Expression.Constant(value._suffixes[4])));
			restExpr = Expression.AndAlso(restExpr, Expression.Equal(
				Expression.ArrayIndex(suffProperty, Expression.Constant(5)),
				Expression.Constant(value._suffixes[5])));
			restExpr = Expression.AndAlso(restExpr, Expression.Equal(
				Expression.ArrayIndex(suffProperty, Expression.Constant(6)),
				Expression.Constant(value._suffixes[6])));
			var lambda = Expression.Lambda<Func<object, bool>>(restExpr, (ParameterExpression)base.Expression);
			var func = lambda.Compile();
			var result = func(value);
			return new DynamicMetaObject(invokeExpr,
				BindingRestrictions.GetExpressionRestriction(restExpr));
		}
	}
}
private class KrgnFunc8 : IKrgnFunc, IDynamicMetaObjectProvider {
	private readonly string[] _suffixes;
	private readonly Func<object,object,object,object,object,object,object,object,object> _func;
	
	public KrgnFunc8(Func<object,object,object,object,object,object,object,object,object> func, string[] suffixes) {
		this._func = func;
		this._suffixes = suffixes;
	}

	Delegate IKrgnFunc.Func { get { return _func; } }
	ReadOnlyCollection<string> IKrgnFunc.Suffixes {
		get { return new ReadOnlyCollection<string>(_suffixes); }
	}
	
	public DynamicMetaObject GetMetaObject(Expression parameter) {
		return new MetaObject(this, parameter);
	}
	
	private class MetaObject : DynamicMetaObject {
		public MetaObject(KrgnFunc8 self, Expression expr)
			: base(expr, BindingRestrictions.GetTypeRestriction(expr, typeof(KrgnFunc8)), self) {
		}
		public override DynamicMetaObject BindInvoke(InvokeBinder binder, DynamicMetaObject[] args) {
			var value = (KrgnFunc8)base.Value;
			Expression[] procArgs = new Expression[8];
			int offset = binder.CallInfo.ArgumentCount - binder.CallInfo.ArgumentNames.Count;
			procArgs[0] = Expression.Convert(
				args[binder.CallInfo.ArgumentNames.IndexOf(value._suffixes[0]) + offset].Expression,
				typeof(object));
			procArgs[1] = Expression.Convert(
				args[binder.CallInfo.ArgumentNames.IndexOf(value._suffixes[1]) + offset].Expression,
				typeof(object));
			procArgs[2] = Expression.Convert(
				args[binder.CallInfo.ArgumentNames.IndexOf(value._suffixes[2]) + offset].Expression,
				typeof(object));
			procArgs[3] = Expression.Convert(
				args[binder.CallInfo.ArgumentNames.IndexOf(value._suffixes[3]) + offset].Expression,
				typeof(object));
			procArgs[4] = Expression.Convert(
				args[binder.CallInfo.ArgumentNames.IndexOf(value._suffixes[4]) + offset].Expression,
				typeof(object));
			procArgs[5] = Expression.Convert(
				args[binder.CallInfo.ArgumentNames.IndexOf(value._suffixes[5]) + offset].Expression,
				typeof(object));
			procArgs[6] = Expression.Convert(
				args[binder.CallInfo.ArgumentNames.IndexOf(value._suffixes[6]) + offset].Expression,
				typeof(object));
			procArgs[7] = Expression.Convert(
				args[binder.CallInfo.ArgumentNames.IndexOf(value._suffixes[7]) + offset].Expression,
				typeof(object));

			Expression target = Expression.Convert(base.Expression, base.LimitType);
			Expression invokeExpr = Expression.Invoke(
				 Expression.Convert(Expression.PropertyOrField(target, "_func"), typeof(Func<object,object,object,object,object,object,object,object,object>)),
				 procArgs);
			Expression restExpr = Expression.TypeIs(base.Expression, typeof(KrgnFunc8));
			Expression suffProperty = Expression.PropertyOrField(target, "_suffixes");
			restExpr = Expression.AndAlso(restExpr, Expression.Equal(
				Expression.ArrayIndex(suffProperty, Expression.Constant(0)),
				Expression.Constant(value._suffixes[0])));
			restExpr = Expression.AndAlso(restExpr, Expression.Equal(
				Expression.ArrayIndex(suffProperty, Expression.Constant(1)),
				Expression.Constant(value._suffixes[1])));
			restExpr = Expression.AndAlso(restExpr, Expression.Equal(
				Expression.ArrayIndex(suffProperty, Expression.Constant(2)),
				Expression.Constant(value._suffixes[2])));
			restExpr = Expression.AndAlso(restExpr, Expression.Equal(
				Expression.ArrayIndex(suffProperty, Expression.Constant(3)),
				Expression.Constant(value._suffixes[3])));
			restExpr = Expression.AndAlso(restExpr, Expression.Equal(
				Expression.ArrayIndex(suffProperty, Expression.Constant(4)),
				Expression.Constant(value._suffixes[4])));
			restExpr = Expression.AndAlso(restExpr, Expression.Equal(
				Expression.ArrayIndex(suffProperty, Expression.Constant(5)),
				Expression.Constant(value._suffixes[5])));
			restExpr = Expression.AndAlso(restExpr, Expression.Equal(
				Expression.ArrayIndex(suffProperty, Expression.Constant(6)),
				Expression.Constant(value._suffixes[6])));
			restExpr = Expression.AndAlso(restExpr, Expression.Equal(
				Expression.ArrayIndex(suffProperty, Expression.Constant(7)),
				Expression.Constant(value._suffixes[7])));
			var lambda = Expression.Lambda<Func<object, bool>>(restExpr, (ParameterExpression)base.Expression);
			var func = lambda.Compile();
			var result = func(value);
			return new DynamicMetaObject(invokeExpr,
				BindingRestrictions.GetExpressionRestriction(restExpr));
		}
	}
}
private class KrgnFunc9 : IKrgnFunc, IDynamicMetaObjectProvider {
	private readonly string[] _suffixes;
	private readonly Func<object,object,object,object,object,object,object,object,object,object> _func;
	
	public KrgnFunc9(Func<object,object,object,object,object,object,object,object,object,object> func, string[] suffixes) {
		this._func = func;
		this._suffixes = suffixes;
	}

	Delegate IKrgnFunc.Func { get { return _func; } }
	ReadOnlyCollection<string> IKrgnFunc.Suffixes {
		get { return new ReadOnlyCollection<string>(_suffixes); }
	}
	
	public DynamicMetaObject GetMetaObject(Expression parameter) {
		return new MetaObject(this, parameter);
	}
	
	private class MetaObject : DynamicMetaObject {
		public MetaObject(KrgnFunc9 self, Expression expr)
			: base(expr, BindingRestrictions.GetTypeRestriction(expr, typeof(KrgnFunc9)), self) {
		}
		public override DynamicMetaObject BindInvoke(InvokeBinder binder, DynamicMetaObject[] args) {
			var value = (KrgnFunc9)base.Value;
			Expression[] procArgs = new Expression[9];
			int offset = binder.CallInfo.ArgumentCount - binder.CallInfo.ArgumentNames.Count;
			procArgs[0] = Expression.Convert(
				args[binder.CallInfo.ArgumentNames.IndexOf(value._suffixes[0]) + offset].Expression,
				typeof(object));
			procArgs[1] = Expression.Convert(
				args[binder.CallInfo.ArgumentNames.IndexOf(value._suffixes[1]) + offset].Expression,
				typeof(object));
			procArgs[2] = Expression.Convert(
				args[binder.CallInfo.ArgumentNames.IndexOf(value._suffixes[2]) + offset].Expression,
				typeof(object));
			procArgs[3] = Expression.Convert(
				args[binder.CallInfo.ArgumentNames.IndexOf(value._suffixes[3]) + offset].Expression,
				typeof(object));
			procArgs[4] = Expression.Convert(
				args[binder.CallInfo.ArgumentNames.IndexOf(value._suffixes[4]) + offset].Expression,
				typeof(object));
			procArgs[5] = Expression.Convert(
				args[binder.CallInfo.ArgumentNames.IndexOf(value._suffixes[5]) + offset].Expression,
				typeof(object));
			procArgs[6] = Expression.Convert(
				args[binder.CallInfo.ArgumentNames.IndexOf(value._suffixes[6]) + offset].Expression,
				typeof(object));
			procArgs[7] = Expression.Convert(
				args[binder.CallInfo.ArgumentNames.IndexOf(value._suffixes[7]) + offset].Expression,
				typeof(object));
			procArgs[8] = Expression.Convert(
				args[binder.CallInfo.ArgumentNames.IndexOf(value._suffixes[8]) + offset].Expression,
				typeof(object));

			Expression target = Expression.Convert(base.Expression, base.LimitType);
			Expression invokeExpr = Expression.Invoke(
				 Expression.Convert(Expression.PropertyOrField(target, "_func"), typeof(Func<object,object,object,object,object,object,object,object,object,object>)),
				 procArgs);
			Expression restExpr = Expression.TypeIs(base.Expression, typeof(KrgnFunc9));
			Expression suffProperty = Expression.PropertyOrField(target, "_suffixes");
			restExpr = Expression.AndAlso(restExpr, Expression.Equal(
				Expression.ArrayIndex(suffProperty, Expression.Constant(0)),
				Expression.Constant(value._suffixes[0])));
			restExpr = Expression.AndAlso(restExpr, Expression.Equal(
				Expression.ArrayIndex(suffProperty, Expression.Constant(1)),
				Expression.Constant(value._suffixes[1])));
			restExpr = Expression.AndAlso(restExpr, Expression.Equal(
				Expression.ArrayIndex(suffProperty, Expression.Constant(2)),
				Expression.Constant(value._suffixes[2])));
			restExpr = Expression.AndAlso(restExpr, Expression.Equal(
				Expression.ArrayIndex(suffProperty, Expression.Constant(3)),
				Expression.Constant(value._suffixes[3])));
			restExpr = Expression.AndAlso(restExpr, Expression.Equal(
				Expression.ArrayIndex(suffProperty, Expression.Constant(4)),
				Expression.Constant(value._suffixes[4])));
			restExpr = Expression.AndAlso(restExpr, Expression.Equal(
				Expression.ArrayIndex(suffProperty, Expression.Constant(5)),
				Expression.Constant(value._suffixes[5])));
			restExpr = Expression.AndAlso(restExpr, Expression.Equal(
				Expression.ArrayIndex(suffProperty, Expression.Constant(6)),
				Expression.Constant(value._suffixes[6])));
			restExpr = Expression.AndAlso(restExpr, Expression.Equal(
				Expression.ArrayIndex(suffProperty, Expression.Constant(7)),
				Expression.Constant(value._suffixes[7])));
			restExpr = Expression.AndAlso(restExpr, Expression.Equal(
				Expression.ArrayIndex(suffProperty, Expression.Constant(8)),
				Expression.Constant(value._suffixes[8])));
			var lambda = Expression.Lambda<Func<object, bool>>(restExpr, (ParameterExpression)base.Expression);
			var func = lambda.Compile();
			var result = func(value);
			return new DynamicMetaObject(invokeExpr,
				BindingRestrictions.GetExpressionRestriction(restExpr));
		}
	}
}
} 
} 

