using System;
using System.IO;
using System.Windows.Forms;
using Kurogane.Interop.Produire.RuntimeBinder;
using Produire.TypeModel;
using Produire;

namespace Kurogane.Interop.Produire {

	public class InteropEngine : Engine {

		private const string ProduireLibraryFile = "CoreLibrary.dll";
		private readonly ReferenceCollection _reference = new ReferenceCollection();
		private readonly string _dllPath = Path.Combine(Application.StartupPath, ProduireLibraryFile);

		private new ProduireBinderFactory Factory { get { return (ProduireBinderFactory)base.Factory; } }

		public InteropEngine()
			: base(new ProduireBinderFactory()) {

			_reference.Import(_dllPath);
			_reference.Import(this.GetType().Assembly.Location);
			this.Factory.Reference = _reference;
			LoadLibrary();
			SetConstractor();
			SetMessageBox();
		}

		private void LoadLibrary() {
			foreach (var ns in _reference.Namespaces.Values) {
				foreach (var typePair in ns.Types) {
					var name = typePair.Key;
					var type = typePair.Value.ManagedType;
					Global.SetVariable(name, type);
					bool isStatic = (type.GetInterface(typeof(IProduireStaticClass).FullName)) != null;
					if (isStatic) {
						var ctorInfo = type.GetConstructor(new Type[0]);
						var obj = ctorInfo.Invoke(new object[0]);
						if (obj != null && obj.GetType() == type)
							Global.SetVariable(name, obj);
					}
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

		private void SetMessageBox() {
			Func<object, object> ShowMsg = delegate(object o) {
				var txt = o.ToString();
				MessageBox.Show(txt);
				return o;
			};
			Global.SetVariable("表示", SuffixFunc.Create<Func<object, object>>(ShowMsg, "を"));
		}
	}
}
