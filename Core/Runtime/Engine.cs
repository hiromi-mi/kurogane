using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kurogane.Compiler;
using System.IO;
using Kurogane.Buildin;
using System.Diagnostics;

namespace Kurogane.Runtime {
	public class Engine {

		private Scope _BuildinScope = new Scope();
		private Scope _DefaultGlobal;


		public Scope DefaultScope { get { return _DefaultGlobal; } }

		public Engine() {
			var loader = new StandardLibraryLoader(_BuildinScope);
			loader.Load();
			_DefaultGlobal = new Scope(_BuildinScope);
		}

		/// <summary>
		/// 与えられたプログラムを実行する。
		/// </summary>
		/// <param name="code">プログラム</param>
		/// <returns>実行結果</returns>
		public object Execute(string code) {
			return Execute(new StringReader(code), _DefaultGlobal);
		}

		/// <summary>
		/// 与えられたプログラムを実行する。
		/// </summary>
		/// <param name="code">プログラム</param>
		/// <returns>実行結果</returns>
		public object Execute(StreamReader code) {
			return Execute(code, _DefaultGlobal);
		}

		/// <summary>
		/// プログラムをスコープの元で実行する。
		/// </summary>
		/// <param name="code">プログラム</param>
		/// <param name="scope">スコープ</param>
		/// <returns>実行結果</returns>
		private object Execute(TextReader code, Scope scope) {
			Stopwatch sw = new Stopwatch();

			sw.Reset();
			sw.Start();
			var token = Tokenizer.Tokenize(code);
			var program = Parser.Parse(token);
			sw.Stop();
			Debug.WriteLine("構文解析: {0}ms", sw.ElapsedMilliseconds);

			sw.Reset();
			sw.Start();
			var expr = ExpressionGenerator.Generate(program);
			sw.Stop();
			Debug.WriteLine("意味解析: {0}ms", sw.ElapsedMilliseconds);

			sw.Reset();
			sw.Start();
			var func = expr.Compile();
			sw.Stop();
			Debug.WriteLine("最適化　: {0}ms", sw.ElapsedMilliseconds);


			sw.Reset();
			sw.Start();
			var result = func(_DefaultGlobal);
			sw.Stop();
			Debug.WriteLine("実行時間: {0}ms", sw.ElapsedMilliseconds);

			return result;
		}

		[Conditional("DEBUG")]
		private void DebugWriteLine(string format, params object[] args) {
			Console.Error.WriteLine(format, args);
		}
	}
}
