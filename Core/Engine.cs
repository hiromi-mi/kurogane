using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Kurogane.Compilers;
using Kurogane.Libraries;
using Kurogane.Dynamics;

namespace Kurogane {

	public class Engine {

		public Encoding Encoding { get; set; }

		/// <summary>標準入力</summary>
		public TextReader Input { get; set; }
		/// <summary>標準出力</summary>
		public TextWriter Output { get; set; }

		/// <summary>グローバル変数があるスコープ</summary>
		public Scope Global { get; private set; }

		public Engine() {
			// Engineの準備
			this.Global = new Scope();
			this.Input = Console.In;
			this.Output = Console.Out;
			this.Encoding = Encoding.Default;
			InitScope(this.Global);
		}

		/// <summary>
		/// 標準関数のロードなど，グローバル変数の初期化
		/// </summary>
		private void InitScope(Scope scope) {
			var loader = new LibraryLoader(this, Global);
			loader.Load();
		}

		/// <summary>
		/// 与えられたプログラムを実行する。
		/// </summary>
		/// <param name="code">プログラム</param>
		/// <returns>実行結果</returns>
		public object Execute(string code) {
			return ExecuteCore(new StringReader(code), Global);
		}

		/// <summary>
		/// 与えられたファイルを実行する。
		/// </summary>
		/// <param name="path">ファイルのパス</param>
		/// <returns>実行結果</returns>
		public object ExecuteFile(string path) {
			return ExecuteFile(path, Global);
		}

		/// <summary>
		/// 与えられたファイルを実行する。
		/// </summary>
		/// <param name="path">ファイルのパス</param>
		/// <returns>実行結果</returns>
		public virtual object ExecuteFile(string path, Scope scope) {
			using (var file = File.Open(path, FileMode.Open, FileAccess.Read))
			using (var stream = new StreamReader(file, Encoding)) {
				return ExecuteCore(stream, scope);
			}
		}

		/// <summary>
		/// プログラムをスコープの元で実行する。
		/// </summary>
		/// <param name="code">プログラム</param>
		/// <param name="scope">スコープ</param>
		/// <returns>実行結果</returns>
		private object ExecuteCore(TextReader code, Scope scope) {
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
			var result = func(Global);
			sw.Stop();
			Debug.WriteLine("実行時間: {0}ms", sw.ElapsedMilliseconds);

			return result;
		}
	}
}
