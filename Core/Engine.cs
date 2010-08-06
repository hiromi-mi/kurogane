using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Kurogane.Compilers;
using Kurogane.Libraries;

namespace Kurogane {

	public class Engine {

		private Globals _BuildinScope = new Globals();
		private Globals _DefaultGlobal;

		/// <summary>標準入力</summary>
		public TextReader Input { get; set; }
		/// <summary>標準出力</summary>
		public TextWriter Output { get; set; }

		public Globals DefaultScope { get { return _DefaultGlobal; } }

		public Engine() {
			//var loader = new StandardLibraryLoader(_BuildinScope);
			//loader.Load();
			_DefaultGlobal = new Globals();
			var loader = new LibraryLoader(this, _DefaultGlobal);
			loader.Load();

			// 標準入出力
			this.Input = Console.In;
			this.Output = Console.Out;
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
		private object Execute(TextReader code, Globals scope) {
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
