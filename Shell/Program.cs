using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Kurogane.Compiler;
using System.Diagnostics.Contracts;

namespace Kurogane.Shell {

	public class Program {
		/// <summary>エントリポイント</summary>
		public static void Main(string[] args) {
			var p = new Program();
			if (args.Length == 0) {
				p.RunRepl();
			}
			else {
				Contract.Assume(Contract.ForAll(args, str => str != null));
				p.RunFiles(args);
			}
		}

		protected virtual Engine GetEngine() {
			Contract.Ensures(Contract.Result<Engine>() != null);
			return new Engine();
		}

		private void RunFiles(string[] files) {
			Contract.Requires<ArgumentNullException>(files != null);
			Contract.Requires<ArgumentException>(Contract.ForAll(files, str => str != null));
			var engine = GetEngine();
			foreach (var file in files) {
				if (!File.Exists(file)) {
					Console.Error.WriteLine("ファイル「{0}」が存在しません。", file);
					Environment.Exit(-1);
				}
				engine.ExecuteFile(file);
			}
		}

		private void RunRepl() {
			var engine = new ReplEngine(GetEngine()) {
				ResultColor = ConsoleColor.Yellow,
				OutputColor = ConsoleColor.Green,
				ErrorColor = ConsoleColor.Red,
			};
			engine.Start();
		}
	}
}
