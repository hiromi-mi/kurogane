 
 

using System;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Kurogane.Buildin {

public abstract class KrgnFunc {

	public static Type GetTypeOf(int n) {
		switch(n) {
		case 0: return typeof(KrgnFunc0);
		case 1: return typeof(KrgnFunc1);
		case 2: return typeof(KrgnFunc2);
		case 3: return typeof(KrgnFunc3);
		case 4: return typeof(KrgnFunc4);
		}
		throw new IndexOutOfRangeException("n must be less than 4");
	}
	
	public static ConstructorInfo GetConstructorInfo(int n) {
		switch(n) {
		case 0: return typeof(KrgnFunc0).GetConstructor(new[] { typeof(Func<object>), typeof(string[]) });
		case 1: return typeof(KrgnFunc1).GetConstructor(new[] { typeof(Func<object,object>), typeof(string[]) });
		case 2: return typeof(KrgnFunc2).GetConstructor(new[] { typeof(Func<object,object,object>), typeof(string[]) });
		case 3: return typeof(KrgnFunc3).GetConstructor(new[] { typeof(Func<object,object,object,object>), typeof(string[]) });
		case 4: return typeof(KrgnFunc4).GetConstructor(new[] { typeof(Func<object,object,object,object,object>), typeof(string[]) });
		}
		throw new IndexOutOfRangeException("n must be less than 4");
	}
	
	public static KrgnFunc Create(Func<object> func, params string[] suffixes) {
		return new KrgnFunc0(func, suffixes);
	}
	public static KrgnFunc Create(Func<object,object> func, params string[] suffixes) {
		return new KrgnFunc1(func, suffixes);
	}
	public static KrgnFunc Create(Func<object,object,object> func, params string[] suffixes) {
		return new KrgnFunc2(func, suffixes);
	}
	public static KrgnFunc Create(Func<object,object,object,object> func, params string[] suffixes) {
		return new KrgnFunc3(func, suffixes);
	}
	public static KrgnFunc Create(Func<object,object,object,object,object> func, params string[] suffixes) {
		return new KrgnFunc4(func, suffixes);
	}

private class KrgnFunc0 : KrgnFunc, IDynamicMetaObjectProvider {
	private readonly string[] Suffixes;
	private readonly Func<object> Func;
	
	public KrgnFunc0(Func<object> func, string[] suffixes) {
		this.Func = func;
		this.Suffixes = suffixes;
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
				 Expression.Convert(Expression.PropertyOrField(target, "Func"), typeof(Func<object>)),
				 procArgs);
			Expression restExpr = Expression.TypeIs(base.Expression, typeof(KrgnFunc0));
			var func = Expression.Lambda<Func<object, bool>>(restExpr, (ParameterExpression)base.Expression).Compile();
			var result = func(value);
			return new DynamicMetaObject(invokeExpr,
				BindingRestrictions.GetExpressionRestriction(restExpr));
		}
	}
}
private class KrgnFunc1 : KrgnFunc, IDynamicMetaObjectProvider {
	private readonly string[] Suffixes;
	private readonly Func<object,object> Func;
	
	public KrgnFunc1(Func<object,object> func, string[] suffixes) {
		this.Func = func;
		this.Suffixes = suffixes;
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
									{
				var pp = value.Suffixes[0];
				for (int j = 0; j < binder.CallInfo.ArgumentNames.Count; j++) {
					if (binder.CallInfo.ArgumentNames[j] == pp) {
						procArgs[0] = Expression.Convert(args[j + offset].Expression, typeof(object));
						break;
					}
				}
				if (offset > 0 && procArgs[0] == null) {
					procArgs[0] = Expression.Convert(args[0].Expression, typeof(object));
				}
			}
						Expression target = Expression.Convert(base.Expression, base.LimitType);
			Expression invokeExpr = Expression.Invoke(
				 Expression.Convert(Expression.PropertyOrField(target, "Func"), typeof(Func<object,object>)),
				 procArgs);
			Expression restExpr = Expression.TypeIs(base.Expression, typeof(KrgnFunc1));
			Expression suffProperty = Expression.PropertyOrField(target, "Suffixes");
			restExpr = Expression.AndAlso(restExpr,
				Expression.Equal(Expression.ArrayIndex(suffProperty, Expression.Constant(0)), Expression.Constant(value.Suffixes[0])
 ) );
			var func = Expression.Lambda<Func<object, bool>>(restExpr, (ParameterExpression)base.Expression).Compile();
			var result = func(value);
			return new DynamicMetaObject(invokeExpr,
				BindingRestrictions.GetExpressionRestriction(restExpr));
		}
	}
}
private class KrgnFunc2 : KrgnFunc, IDynamicMetaObjectProvider {
	private readonly string[] Suffixes;
	private readonly Func<object,object,object> Func;
	
	public KrgnFunc2(Func<object,object,object> func, string[] suffixes) {
		this.Func = func;
		this.Suffixes = suffixes;
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
									{
				var pp = value.Suffixes[0];
				for (int j = 0; j < binder.CallInfo.ArgumentNames.Count; j++) {
					if (binder.CallInfo.ArgumentNames[j] == pp) {
						procArgs[0] = Expression.Convert(args[j + offset].Expression, typeof(object));
						break;
					}
				}
				if (offset > 0 && procArgs[0] == null) {
					procArgs[0] = Expression.Convert(args[0].Expression, typeof(object));
				}
			}
						{
				var pp = value.Suffixes[1];
				for (int j = 0; j < binder.CallInfo.ArgumentNames.Count; j++) {
					if (binder.CallInfo.ArgumentNames[j] == pp) {
						procArgs[1] = Expression.Convert(args[j + offset].Expression, typeof(object));
						break;
					}
				}
				if (offset > 0 && procArgs[1] == null) {
					procArgs[1] = Expression.Convert(args[0].Expression, typeof(object));
				}
			}
						Expression target = Expression.Convert(base.Expression, base.LimitType);
			Expression invokeExpr = Expression.Invoke(
				 Expression.Convert(Expression.PropertyOrField(target, "Func"), typeof(Func<object,object,object>)),
				 procArgs);
			Expression restExpr = Expression.TypeIs(base.Expression, typeof(KrgnFunc2));
			Expression suffProperty = Expression.PropertyOrField(target, "Suffixes");
			restExpr = Expression.AndAlso(restExpr,
				Expression.AndAlso(Expression.Equal(
					Expression.ArrayIndex(suffProperty, Expression.Constant(0)), Expression.Constant(value.Suffixes[0])),
				Expression.Equal(Expression.ArrayIndex(suffProperty, Expression.Constant(1)), Expression.Constant(value.Suffixes[1])
 )  ) );
			var func = Expression.Lambda<Func<object, bool>>(restExpr, (ParameterExpression)base.Expression).Compile();
			var result = func(value);
			return new DynamicMetaObject(invokeExpr,
				BindingRestrictions.GetExpressionRestriction(restExpr));
		}
	}
}
private class KrgnFunc3 : KrgnFunc, IDynamicMetaObjectProvider {
	private readonly string[] Suffixes;
	private readonly Func<object,object,object,object> Func;
	
	public KrgnFunc3(Func<object,object,object,object> func, string[] suffixes) {
		this.Func = func;
		this.Suffixes = suffixes;
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
						for (int i = 0; i < 3; i++)
									{
				var pp = value.Suffixes[i];
				for (int j = 0; j < binder.CallInfo.ArgumentNames.Count; j++) {
					if (binder.CallInfo.ArgumentNames[j] == pp) {
						procArgs[i] = Expression.Convert(args[j + offset].Expression, typeof(object));
						break;
					}
				}
				if (offset > 0 && procArgs[i] == null) {
					procArgs[i] = Expression.Convert(args[0].Expression, typeof(object));
				}
			}
						Expression target = Expression.Convert(base.Expression, base.LimitType);
			Expression invokeExpr = Expression.Invoke(
				 Expression.Convert(Expression.PropertyOrField(target, "Func"), typeof(Func<object,object,object,object>)),
				 procArgs);
			Expression restExpr = Expression.TypeIs(base.Expression, typeof(KrgnFunc3));
			Expression suffProperty = Expression.PropertyOrField(target, "Suffixes");
			restExpr = Expression.AndAlso(restExpr,
				Expression.AndAlso(Expression.Equal(
					Expression.ArrayIndex(suffProperty, Expression.Constant(0)), Expression.Constant(value.Suffixes[0])),
				Expression.AndAlso(Expression.Equal(
					Expression.ArrayIndex(suffProperty, Expression.Constant(1)), Expression.Constant(value.Suffixes[1])),
				Expression.Equal(Expression.ArrayIndex(suffProperty, Expression.Constant(2)), Expression.Constant(value.Suffixes[2])
 )  )  ) );
			var func = Expression.Lambda<Func<object, bool>>(restExpr, (ParameterExpression)base.Expression).Compile();
			var result = func(value);
			return new DynamicMetaObject(invokeExpr,
				BindingRestrictions.GetExpressionRestriction(restExpr));
		}
	}
}
private class KrgnFunc4 : KrgnFunc, IDynamicMetaObjectProvider {
	private readonly string[] Suffixes;
	private readonly Func<object,object,object,object,object> Func;
	
	public KrgnFunc4(Func<object,object,object,object,object> func, string[] suffixes) {
		this.Func = func;
		this.Suffixes = suffixes;
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
						for (int i = 0; i < 4; i++)
									{
				var pp = value.Suffixes[i];
				for (int j = 0; j < binder.CallInfo.ArgumentNames.Count; j++) {
					if (binder.CallInfo.ArgumentNames[j] == pp) {
						procArgs[i] = Expression.Convert(args[j + offset].Expression, typeof(object));
						break;
					}
				}
				if (offset > 0 && procArgs[i] == null) {
					procArgs[i] = Expression.Convert(args[0].Expression, typeof(object));
				}
			}
						Expression target = Expression.Convert(base.Expression, base.LimitType);
			Expression invokeExpr = Expression.Invoke(
				 Expression.Convert(Expression.PropertyOrField(target, "Func"), typeof(Func<object,object,object,object,object>)),
				 procArgs);
			Expression restExpr = Expression.TypeIs(base.Expression, typeof(KrgnFunc4));
			Expression suffProperty = Expression.PropertyOrField(target, "Suffixes");
			restExpr = Expression.AndAlso(restExpr,
				Expression.AndAlso(Expression.Equal(
					Expression.ArrayIndex(suffProperty, Expression.Constant(0)), Expression.Constant(value.Suffixes[0])),
				Expression.AndAlso(Expression.Equal(
					Expression.ArrayIndex(suffProperty, Expression.Constant(1)), Expression.Constant(value.Suffixes[1])),
				Expression.AndAlso(Expression.Equal(
					Expression.ArrayIndex(suffProperty, Expression.Constant(2)), Expression.Constant(value.Suffixes[2])),
				Expression.Equal(Expression.ArrayIndex(suffProperty, Expression.Constant(3)), Expression.Constant(value.Suffixes[3])
 )  )  )  ) );
			var func = Expression.Lambda<Func<object, bool>>(restExpr, (ParameterExpression)base.Expression).Compile();
			var result = func(value);
			return new DynamicMetaObject(invokeExpr,
				BindingRestrictions.GetExpressionRestriction(restExpr));
		}
	}
}
} 
} 

