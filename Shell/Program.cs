using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Kurogane.Compiler;

namespace Kurogane.Shell {

	public class Program {

		/// <summary>エントリポイント</summary>
		public static void Main(string[] args) {
			if (args.Length == 0)
				StartRepl();
			else
				ExecuteFile(args[0]);
		}

		/// <summary>
		/// ファイルからの実行
		/// </summary>
		private static void ExecuteFile(string filepath) {
			if (!File.Exists(filepath)) {
				Console.Error.WriteLine("ファイル「{0}」が存在しません。", filepath);
				Environment.Exit(-1);
			}
			var engine = new Engine();
			engine.ExecuteFile(filepath);
		}

		/// <summary>
		/// REPL(Read-Eval-Print Loop)モードを実行する。
		/// </summary>
		private static void StartRepl() {
			ShowStartMessage();
			const string ConsoleWait = "> ";
			string before = String.Empty;
			var engine = new Engine();
			Console.Write(ConsoleWait);
			while (true) {
				string line = Console.ReadLine();
				if (line == "exit" || line.StartsWith("終了"))
					break;
				if (line.Length == 0) {
					before = String.Empty;
					Console.Write(ConsoleWait);
					continue;
				}
				line = before + Environment.NewLine + line;
				try {
					Console.WriteLine(engine.Execute(line, "-- console input --"));
					before = String.Empty;
					Console.Write(ConsoleWait);
				}
				catch (CompilerException) {
					before = line;
					Console.Write("... ");
				}
				catch (Exception e) {
					var beforeColor = Console.ForegroundColor;
					Console.ForegroundColor = ConsoleColor.Red;
					Console.Error.WriteLine(e.Message);
					Console.ForegroundColor = beforeColor;
					Console.Write(ConsoleWait);
				}
			}
			Console.WriteLine("終了します ...");
		}

		/// <summary>REPLモードで最初に表示するメッセージ</summary>
		private static void ShowStartMessage() {
			var width = Console.WindowWidth;
			const string titleTxt = "プログラミング言語「クロガネ」";
			var versionTxt = "ver." + Engine.Version;

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
		}
	}
}
