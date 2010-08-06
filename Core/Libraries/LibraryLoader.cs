using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.IO;
using System.Linq.Expressions;
using Kurogane.Types;

namespace Kurogane.Libraries {
	public class LibraryLoader {

		private Engine _engine;
		private Scope _scope;

		private ISet<string> _loadLibraries = new HashSet<string>();
		private ISet<Assembly> _loadedAssemblies = new HashSet<Assembly>();

		public LibraryLoader(Engine engine, Scope scope) {
			_engine = engine;
			_scope = scope;
		}

		public void Load() {
			Assembly asm = typeof(Engine).Assembly;
			LoadAssembly(asm);
			string dllPath = asm.Location;
			string dirPath = Path.GetDirectoryName(dllPath);
			LoadFromDirectory(dirPath);
		}

		private void LoadFromDirectory(string dirPath) {
			foreach (var file in Directory.GetFiles(dirPath)) {
				if (_loadLibraries.Contains(file)) continue;
				Assembly asm;
				try {
					asm = Assembly.LoadFile(file);
					_loadLibraries.Add(file);
				}
				catch {
					continue;
				}
				LoadAssembly(asm);
			}
		}

		private void LoadAssembly(Assembly asm) {
			if (_loadedAssemblies.Contains(asm)) return;
			_loadedAssemblies.Add(asm);

			foreach (var type in asm.GetTypes()) {
				var attrs = type.GetCustomAttributes(typeof(LibraryAttribute), false);
				if (attrs.Length > 0)
					LoadType(type);
			}
		}

		private void LoadType(Type type) {
			var param = new object[] { _engine, _scope };

			foreach (var method in type.GetMethods()) {
				// 普通のロード実行
				var paramInfos = method.GetParameters();
				if (paramInfos.Length == 2 &&
					paramInfos[0].ParameterType == typeof(Engine) &&
					paramInfos[1].ParameterType == typeof(Scope)) {

					method.Invoke(null, param);
				}
			}
		}

	}
}
