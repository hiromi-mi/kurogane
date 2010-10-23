using System;
using System.IO;
using System.Windows.Forms;
using Kurogane.Interop.Produire.RuntimeBinder;
using Produire.TypeModel;

namespace Kurogane.Interop.Produire {

	public class InteropEngine : Engine {

		private const string ProduireLibraryFile = "CoreLibrary.dll";
		private readonly ReferenceCollection _reference = new ReferenceCollection();
		private readonly string _dllPath = Path.Combine(Application.StartupPath, ProduireLibraryFile);

		private new ProduireBinderFactory Factory { get { return (ProduireBinderFactory)base.Factory; } }

		public InteropEngine()
			: base(new ProduireBinderFactory()) {
			_reference.Import(_dllPath);
			this.Factory.Reference = _reference;
			LoadLibrary();
			SetConstractor();
			SetMessageLoop();
		}

		private void LoadLibrary() {
			foreach (var ns in _reference.Namespaces.Values) {
				foreach (var typePair in ns.Types) {
					var name = typePair.Key;
					var type = typePair.Value.ManagedType;
					Global.SetVariable(name, type);
				}
			}
		}

		private void SetConstractor() {
			Func<object, object> create = type => {
				if ((type is Type) == false)
					return null;
				var cInfo = ((Type)type).GetConstructor(new Type[0]);
				if (cInfo == null)
					return null;
				return cInfo.Invoke(new object[0]);
			};
			Global.SetVariable("作成", SuffixFunc.Create(create, "を"));
		}

		private void SetMessageLoop() {
			Func<object, object> loop = win => {
				var form = win as Form;
				Application.Run(form);
				return null;
			};
			Global.SetVariable("メッセージループ", SuffixFunc.Create(loop, "を"));
		}
	}
}
