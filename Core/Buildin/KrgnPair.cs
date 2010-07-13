using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Dynamic;
using System.Linq.Expressions;

namespace Kurogane.Buildin {

	public class KrgnPair : IDynamicMetaObjectProvider {
		public object Head { get; private set; }
		public object Tail { get; private set; }

		public KrgnPair(object head, object tail) {
			Head = head;
			Tail = tail;
		}

		DynamicMetaObject IDynamicMetaObjectProvider.GetMetaObject(Expression parameter) {
			return new MetaObject(this, parameter);
		}

		private class MetaObject : DynamicMetaObject {
			public MetaObject(KrgnPair pair, Expression expr)
				: base(expr, BindingRestrictions.Empty, pair) {
			}

			public override DynamicMetaObject BindGetMember(GetMemberBinder binder) {
				string propName = null;
				switch (binder.Name) {
				case "頭": propName = "Head"; break;
				case "体": propName = "Tail"; break;
				}
				if (propName != null)
					return new DynamicMetaObject(
						Expression.Property(Expression.Convert(base.Expression, typeof(KrgnPair)), propName),
						BindingRestrictions.GetExpressionRestriction(Expression.TypeIs(base.Expression, typeof(KrgnPair))));
				return base.BindGetMember(binder);
			}
		}
	}

}
