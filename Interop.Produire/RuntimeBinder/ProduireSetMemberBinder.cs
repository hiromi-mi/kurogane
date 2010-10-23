using System;
using System.Dynamic;
using System.Linq.Expressions;
using System.Reflection;
using Kurogane.RuntimeBinder;
using Produire;
using Produire.TypeModel;

namespace Kurogane.Interop.Produire.RuntimeBinder {

	public class ProduireSetMemberBinder : KrgnSetMemberBinder {

		private static readonly Type InstanceType = typeof(IProduireClass);
		private static readonly MethodInfo MethodInfo = typeof(PType).GetMethod("SetPropertyValue", new[] { InstanceType, typeof(string), typeof(object) });

		private readonly ReferenceCollection _reference;

		public ProduireSetMemberBinder(string name, ReferenceCollection reference)
			: base(name) {
			_reference = reference;
		}

		public override DynamicMetaObject FallbackSetMember(DynamicMetaObject target, DynamicMetaObject value, DynamicMetaObject errorSuggestion) {
			var pType = _reference.GetProduireType(target.LimitType);
			if (pType == null)
				return base.FallbackSetMember(target, value, errorSuggestion);
			var tmpVar = Expression.Variable(typeof(object));
			return new DynamicMetaObject(
				Expression.Block(
					new[] { tmpVar },
					Expression.Assign(tmpVar, value.Expression),
					Expression.Call(
						Expression.Constant(pType),
						MethodInfo,
						Expression.Convert(target.Expression, InstanceType),
						Expression.Constant(this.Name),
						tmpVar),
					tmpVar),
				BindingRestrictions.GetTypeRestriction(target.Expression, target.LimitType));
		}
	}
}
