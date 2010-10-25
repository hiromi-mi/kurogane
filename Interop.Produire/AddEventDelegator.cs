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
		public object Add([で]SuffixFunc<Func<object, object, object>> sfxFunc) {
			var func = sfxFunc.Func;
			_info.AddEventHandler(_target, new EventHandler(delegate(object sender, EventArgs e) {
				func(sender, e);
			}));
			return sfxFunc;
		}

		[自分("を"), 手順("監視")]
		public object Add([で]SuffixFunc<Func<object>> sfxFunc) {
			var func = sfxFunc.Func;
			_info.AddEventHandler(_target, new EventHandler(delegate(object sender, EventArgs e) {
				func();
			}));
			return sfxFunc;
		}
	}
}
