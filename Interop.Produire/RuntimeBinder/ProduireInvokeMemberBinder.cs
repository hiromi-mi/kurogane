using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Kurogane.RuntimeBinder;
using Produire;
using Produire.TypeModel;

namespace Kurogane.Interop.Produire.RuntimeBinder {

	public class ProduireInvokeMemberBinder : KrgnInvokeMemberBinder {

		private static readonly MethodInfo InvokeMethodInfo =
			typeof(PType).GetMethod("Invoke", new[] { typeof(IProduireClass), typeof(string), typeof(object[]) });
		private readonly ReferenceCollection _reference;

		public ProduireInvokeMemberBinder(string name, CallInfo callInfo, BinderFactory factory, ReferenceCollection reference)
			: base(name, callInfo, factory) {
			_reference = reference;
		}

		public override DynamicMetaObject FallbackInvokeMember(DynamicMetaObject target, DynamicMetaObject[] args, DynamicMetaObject errorSuggestion) {
			int offset = CallInfo.ArgumentCount - CallInfo.ArgumentNames.Count;
			for (int argIx = 0; argIx < args.Length; argIx++) {
				// レシーバとその助詞を探す。
				var arg = args[argIx];
				if (arg.Value == null)
					continue;
				//if ((arg.Value is IProduireClass) == false)
				//    continue;
				var pType = _reference.GetProduireType(arg.LimitType);
				if (pType == null) continue;
				var suffix = argIx < offset ? null : CallInfo.ArgumentNames[argIx + offset];
				// 対応する動詞が存在するか探す。
				bool hasVerb = pType.Verbs
					.Where(v => v.Name == Name)
					.Where(v => v.Complements
						.Where(c => c is ReceiverComplement)
						.Any(c => c.ParticleText == suffix))
					.Any();
				if (hasVerb)
					return FindVerb(argIx, args);
			}
			return base.FallbackInvokeMember(target, args, errorSuggestion);
		}

		private DynamicMetaObject FindVerb(int index, DynamicMetaObject[] argsMO) {
			var arg = argsMO[index];
			var pType = _reference.GetProduireType(arg.LimitType);
			var args = new Expression[argsMO.Length - 1];
			for (int i = 0; i < args.Length; i++)
				args[i] = argsMO[i < index ? i : i + 1].Expression;
			var expr = Expression.Call(
				Expression.Constant(pType),
				InvokeMethodInfo,
				Expression.Convert(arg.Expression, typeof(IProduireClass)),
				Expression.Constant(this.Name),
				Expression.NewArrayInit(typeof(object), args));
			return new DynamicMetaObject(expr, BindingRestrictions.GetTypeRestriction(arg.Expression, arg.LimitType));
		}
	}
}
