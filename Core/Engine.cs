using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using Kurogane.Compiler;
using Kurogane.Expressions;
using Kurogane.RuntimeBinder;

namespace Kurogane {
	public class Engine {
		// ----- ----- ----- ----- ----- fields ----- ----- ----- ----- -----

		protected BinderFactory Factory { get; private set; }

		/// <summary>グローバルスコープ</summary>
		public Scope Global { get; private set; }

		/// <summary>入力先</summary>
		public TextReader In { get; set; }

		/// <summary>出力先</summary>
		public TextWriter Out { get; set; }

		public Encoding DefaultEncoding { get; set; }

		// ----- ----- ----- ----- ----- ctor ----- ----- ----- ----- -----

		/// <summary>通常のコンストラクタ</summary>
		public Engine()
			: this(new BinderFactory()) {
			LoadLibrary(this.GetType().Assembly);
		}

		protected Engine(BinderFactory factory) {
			Debug.Assert(factory != null, "factory is null");

			Factory = factory;
			Global = new Scope();
			In = Console.In;
			Out = Console.Out;
			DefaultEncoding = Encoding.Default;
		}

		// ----- ----- ----- ----- ----- methods ----- ----- ----- ----- -----

		public object Execute(string code) {
			using (var reader = new StringReader(code)) {
				return ExecuteCore(reader, null);
			}
		}

		public object ExecuteFile(string filepath) {
			using (var file = File.OpenRead(filepath))
			using (var stream = new StreamReader(file, DefaultEncoding)) {
				return ExecuteCore(stream, filepath);
			}
		}

		private object ExecuteStream(Stream stream, string filename) {
			using (var reader = new StreamReader(stream, DefaultEncoding)) {
				return ExecuteCore(reader, filename);
			}
		}

		private object ExecuteCore(TextReader stream, string filename) {
			//var sw = new Stopwatch();
			//sw.Start();
			var token = Tokenizer.Tokenize(stream, filename);
			var ast = Parser.Parse(token, filename);
			var expr = Generator.Generate(ast, this.Factory, filename);
			expr = ExpressionOptimizer.Analyze(expr);
			var func = expr.Compile();
			//sw.Stop();
			//var compileTime = sw.ElapsedMilliseconds;
			//sw.Reset();
			//sw.Start();
			var result = func(this.Global);
			//sw.Stop();
			//var executeTime = sw.ElapsedMilliseconds;
			//Console.WriteLine("Compile : " + compileTime);
			//Console.WriteLine("Execute : " + executeTime);
			return result;
		}

		#region LoadLibrary

		private void LoadLibrary(Assembly asm) {
			// 静的ライブラリのロード
			foreach (var type in asm.GetTypes()) {
				// 関数と変数
				var typeAttrs = type.GetCustomAttributes(typeof(LibraryAttribute), false);
				if (typeAttrs.Length > 0) {
					foreach (var fInfo in type.GetFields(BindingFlags.Public | BindingFlags.Static))
						SetField(fInfo);
					foreach (var mInfo in type.GetMethods(BindingFlags.Public | BindingFlags.Static))
						SetMethod(mInfo);
				}
				// クラス
				var aliasAttr = type.GetCustomAttributes(typeof(AliasForAttribute), false);
				if (aliasAttr.Length > 0) {
					foreach (AliasForAttribute attr in aliasAttr) {
						MetaObjectLoader.RegisterAlias(attr.Type, type);
					}
				}
				else {
					var nameAttr = type.GetCustomAttributes(typeof(JpNameAttribute), false);
					if (nameAttr.Length > 0) {
						MetaObjectLoader.RegisterAlias(type, type);
					}
				}
			}
			// 動的ライブラリのロード
			foreach (var name in asm.GetManifestResourceNames())
				if (name.EndsWith(".krg"))
					this.ExecuteStream(asm.GetManifestResourceStream(name), name);
		}

		private void SetField(FieldInfo fInfo) {
			var nameAttrs = fInfo.GetCustomAttributes(typeof(JpNameAttribute), false);
			if (nameAttrs.Length == 0) return;
			var value = fInfo.GetValue(null);
			foreach (var attr in nameAttrs) {
				var nameAttr = attr as JpNameAttribute;
				Global.SetVariable(nameAttr.Name, value);
			}
		}

		private void SetMethod(MethodInfo mInfo) {
			var nameAttrs = mInfo.GetCustomAttributes(typeof(JpNameAttribute), false);
			if (nameAttrs.Length == 0)
				return;
			var paramInfos = mInfo.GetParameters();
			int ext = mInfo.GetCustomAttributes(typeof(ExtensionAttribute), false).Length > 0 ? 1 : 0;
			var sfxs = new String[paramInfos.Length - ext];
			for (int i = 0; i < sfxs.Length; i++) {
				var sfxAttrs = paramInfos[i + ext].GetCustomAttributes(typeof(SuffixAttribute), false);
				if (sfxAttrs.Length == 0)
					return;
				var sfxAttr = sfxAttrs[0] as SuffixAttribute;
				sfxs[i] = sfxAttr.Name;
			}
			var types = new Type[paramInfos.Length - ext + 1];
			for (int i = 0; i < types.Length - 1; i++)
				types[i] = paramInfos[i + ext].ParameterType;
			types[types.Length - 1] = mInfo.ReturnType;
			var funcType = Expression.GetFuncType(types);

			var outerParam = Expression.Parameter(paramInfos[0].ParameterType);
			var innerParams = new ParameterExpression[types.Length - 1];
			for (int i = 0; i < innerParams.Length; i++)
				innerParams[i] = Expression.Parameter(types[i]);
			var lambda = Expression.Lambda(Expression.Call(mInfo,
				(ext == 0 ? innerParams : Enumerable.Concat(new[] { outerParam }, innerParams))),
				innerParams);
			var ctorInfo = typeof(SuffixFunc<>).MakeGenericType(funcType).GetConstructor(new[] { funcType, typeof(string[]) });
			var body = Expression.New(ctorInfo, lambda, Expression.Constant(sfxs));
			if (ext > 0) {
				// this パラメタが存在する
				if (paramInfos[0].ParameterType == typeof(Engine)) {
					var func = Expression.Lambda<Func<Engine, object>>(Expression.Convert(body, typeof(object)), outerParam);
					var exec = func.Compile();
					foreach (var nAttr in nameAttrs) {
						var a = nAttr as JpNameAttribute;
						Global.SetVariable(a.Name, exec(this));
					}
				}
				else if (paramInfos[0].ParameterType == typeof(Scope)) {
					var func = Expression.Lambda<Func<Scope, object>>(Expression.Convert(body, typeof(object)), outerParam);
					var exec = func.Compile();
					foreach (var nAttr in nameAttrs) {
						var a = nAttr as JpNameAttribute;
						Global.SetVariable(a.Name, exec(Global));
					}
				}
			}
			else {
				// this パラメタが存在しない。
				var func = Expression.Lambda<Func<object>>(Expression.Convert(body, typeof(object)));
				var exec = func.Compile();
				foreach (var nAttr in nameAttrs) {
					var a = nAttr as JpNameAttribute;
					Global.SetVariable(a.Name, exec());
				}
			}
		}

		#endregion

	}
}
