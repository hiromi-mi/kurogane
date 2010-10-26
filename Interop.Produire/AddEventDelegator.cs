using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using Produire;

namespace Kurogane.Interop.Produire {

	public class AddEventDelegator : IProduireClass {
		private readonly object _target;
		private readonly EventInfo _info;

		public AddEventDelegator(object target, EventInfo info) {
			_target = target;
			_info = info;
		}

		[自分("を"), 手順("監視")]
		public object Add([で]object func) {
			if (func is SuffixFunc<Func<object>>) {
				var sfxFunc = func as SuffixFunc<Func<object>>;
				_info.AddEventHandler(_target, new EventHandler(delegate(object sender, EventArgs e) {
					sfxFunc.Func();
				}));
			}
			if (func is SuffixFunc<Func<object, object, object>>) {
				var sfxFunc = func as SuffixFunc<Func<object, object, object>>;
				_info.AddEventHandler(_target, new EventHandler(delegate(object sender, EventArgs e) {
					sfxFunc.Func(sender, e);
				}));
			}
			return func;
		}

	}
}
