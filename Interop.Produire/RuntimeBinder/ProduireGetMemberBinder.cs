using System;
using System.Dynamic;
using System.Linq.Expressions;
using System.Reflection;
using Kurogane.RuntimeBinder;
using Produire;
using Produire.TypeModel;

namespace Kurogane.Interop.Produire.RuntimeBinder {

	public class ProduireGetMemberBinder : KrgnGetMemberBinder {

		private static readonly Type InstanceType = typeof(IProduireClass);
		private static readonly MethodInfo MethodInfo = typeof(PType).GetMethod("GetPropertyValue", new[] { InstanceType, typeof(string) });

		private readonly ReferenceCollection _reference;

		public ProduireGetMemberBinder(string name, ReferenceCollection reference)
			: base(name) {
			_reference = reference;
		}

		public override DynamicMetaObject FallbackGetMember(DynamicMetaObject target, DynamicMetaObject errorSuggestion) {
			var pType = _reference.GetProduireType(target.LimitType);
			if (pType == null)
				return base.FallbackGetMember(target, errorSuggestion);
			return new DynamicMetaObject(
				Expression.Call(
					Expression.Constant(pType),
					MethodInfo,
					Expression.Convert(target.Expression, InstanceType),
					Expression.Constant(this.Name)),
				BindingRestrictions.GetTypeRestriction(target.Expression, target.LimitType));
		}

	}
}
