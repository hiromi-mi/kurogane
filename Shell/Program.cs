using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Kurogane.Compiler;

namespace Kurogane.Shell {

	public class Program {

		const string ConsoleWait = "> ";

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
		/// コンソールで一行ずつ実行するモード
		/// </summary>
		private static void StartRepl() {
			ShowStartMessage();

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
				try {
					var result = engine.Execute(before + line);
					if (result != null)
						Console.WriteLine(result);
					else
						Console.WriteLine(ConstantNames.NullText);
					before = String.Empty;
					Console.Write(ConsoleWait);
				}
				catch (CompilerException) {
					before = before + line;
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

		private static void ShowStartMessage() {
			var width = Console.WindowWidth;
			const string titleTxt = "プログラミング言語「クロガネ」";
			var versionTxt = "ver." + typeof(Engine).Assembly.GetName().Version;

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

		private static void ShowStartMessage2() {
			var width = Console.WindowWidth;
			const string titleTxt = "プログラミング言語「クロガネ」";
			var versionTxt = "ver." + typeof(Engine).Assembly.GetName().Version;
			string title = (titleTxt + "  " + versionTxt + "    * ").PadLeft(width - titleTxt.Length - 3);

			// 1st
			for (int i = 1; i < width - 1; i += 2)
				Console.Write(" *");
			Console.WriteLine();
			// 2nd
			Console.Write(" *");
			for (int i = 0; i < width - 5; i++)
				Console.Write(" ");
			Console.WriteLine("*");
			// 3rd
			Console.WriteLine(" *" + title);
			// 4th
			Console.Write(" *");
			for (int i = 0; i < width - 5; i++)
				Console.Write(" ");
			Console.WriteLine("*");
			// 5th
			for (int i = 1; i < width - 1; i += 2)
				Console.Write(" *");
			Console.WriteLine();
		}

	}
}
