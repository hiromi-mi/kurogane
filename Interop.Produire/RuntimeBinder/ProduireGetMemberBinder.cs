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
		private static readonly MethodInfo MethodInfo = typeof(PType).GetMethod("GetPropertyValue", new[] { typeof(PPropertyInfo), InstanceType });

		private readonly ReferenceCollection _reference;

		public ProduireGetMemberBinder(string name, ReferenceCollection reference)
			: base(name) {
			_reference = reference;
		}

		public override DynamicMetaObject FallbackGetMember(DynamicMetaObject target, DynamicMetaObject errorSuggestion) {
			var pType = _reference.GetProduireType(target.LimitType);
			if (pType == null)
				return base.FallbackGetMember(target, errorSuggestion);
			return
				GetProperty(target, pType) ??
				GetEvent(target, pType) ??
				base.FallbackGetMember(target, errorSuggestion);
		}

		private DynamicMetaObject GetProperty(DynamicMetaObject target, PType pType) {
			PPropertyInfo propInfo = null;
			pType.TryGetPropertyInfo(Name, out propInfo);
			if (propInfo == null)
				return null;
			return new DynamicMetaObject(
				Expression.Call(
					Expression.Constant(pType),
					MethodInfo,
					Expression.Constant(propInfo),
					Expression.Convert(target.Expression, InstanceType)),
				BindingRestrictions.GetTypeRestriction(target.Expression, target.LimitType));
		}

		private DynamicMetaObject GetEvent(DynamicMetaObject target, PType pType) {
			var eventName = Name + "された";
			EventInfo evInfo = null;
			var evs = pType.GetEventNames();
			pType.TryGetEvent(eventName, out evInfo);
			if (evInfo == null)
				return null;
			var ctorInfo = typeof(AddEventDelegator).GetConstructor(new[] { typeof(object), typeof(EventInfo) });
			return new DynamicMetaObject(
				Expression.New(ctorInfo, target.Expression, Expression.Constant(evInfo)),
				BindingRestrictions.GetTypeRestriction(target.Expression, target.LimitType));
		}
	}
}
