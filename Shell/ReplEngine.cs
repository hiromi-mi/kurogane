using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;
using Kurogane.Compiler;

namespace Kurogane.Shell {
	public sealed class ReplEngine {
		private readonly Engine _engine;

		public ConsoleColor InputColor { get; set; }
		public ConsoleColor OutputColor { get; set; }
		public ConsoleColor ErrorColor { get; set; }
		public ConsoleColor ResultColor { get; set; }
		public ConsoleColor MessageColor { get; set; }

		public ReplEngine(Engine engine) {
			Contract.Requires<ArgumentNullException>(engine != null);
			_engine = engine;
			Initialize();
		}

		[ContractInvariantMethod]
		private void ObjectInvariant() {
			Contract.Invariant(_engine != null);
		}

		private void Initialize() {
			var defaultColor = Console.ForegroundColor;
			this.InputColor = defaultColor;
			this.OutputColor = defaultColor;
			this.ErrorColor = defaultColor;
			this.ResultColor = defaultColor;
			this.MessageColor = defaultColor;
		}

		public void Start() {
			ShowStartMessage();
			while (true) {
				try {
					var result = EachRepl();
					if (result != null)
						ColorIn(ResultColor, delegate { Console.WriteLine(result); });
				}
				catch (Exception e) {
					ColorIn(ErrorColor, delegate { Console.Error.WriteLine(e.Message); });
				}
			}
		}

		private void ShowStartMessage() {
			var width = Console.WindowWidth;
			const string titleTxt = "プログラミング言語「クロガネ」";
			var versionTxt = "ver." + Engine.Version;
			ColorIn(MessageColor, delegate {
				// start line
				for (int i = 1; i < width - 1; i += 2)
					Console.Write(" *");
				Console.WriteLine();
				Console.WriteLine();
				// show title
				Console.WriteLine("    " + titleTxt);
				// show version
				Console.WriteLine(versionTxt.PadLeft(width - 4));
				// end line
				Console.WriteLine();
				for (int i = 1; i < width - 1; i += 2)
					Console.Write(" *");
				Console.WriteLine();
			});
		}

		/// <summary>ユーザの入力をコードを実行する。</summary>
		private object EachRepl() {
			String buff = String.Empty;
			ColorIn(MessageColor, delegate { Console.Write("> "); });
			while (true) {
				string line = String.Empty; ;
				ColorIn(InputColor, delegate { line = Console.ReadLine(); });
				if (line == "exit" || line.StartsWith("終了")) {
					ColorIn(MessageColor, delegate { Console.WriteLine("終了します ..."); });
					Environment.Exit(0);
				}
				if (line.Length == 0) {
					return null;
				}
				line = buff + Environment.NewLine + line;
				try {
					object result = null;
					ColorIn(this.OutputColor, delegate { result = _engine.Execute(line, "-- console input --"); });
					return result;
				}
				catch (CompilerException) {
					buff = line;
					ColorIn(MessageColor, delegate { Console.Write("... "); });
				}
			}
		}

		private static void ColorIn(ConsoleColor color, Action action) {
			Contract.Requires<ArgumentNullException>(action != null);
			var back = Console.ForegroundColor;
			Console.ForegroundColor = color;
			try {
				action();
			}
			finally {
				Console.ForegroundColor = back;
			}
		}
	}
}
