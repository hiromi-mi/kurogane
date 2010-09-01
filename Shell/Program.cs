using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

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
			using (var reader = new StreamReader(filepath, Encoding.Default)) {
				var engine = new Engine();
				engine.Execute(reader);
			}
		}

		/// <summary>
		/// コンソールで一行ずつ実行するモード
		/// </summary>
		private static void StartRepl() {
			string before = "";
			var engine = new Engine();
			Console.Write(ConsoleWait);
			while (true) {
				string line = Console.ReadLine();
				if (line == "exit" || line.StartsWith("終了"))
					break;
				if (line.Length == 0) {
					before = "";
					Console.Write(ConsoleWait);
					continue;
				}
				try {
					var result = engine.Execute(before + line);
					Console.WriteLine(result);
					before = "";
					Console.Write(ConsoleWait);
				}
				catch {
					before = before + line;
					Console.Write("... ");
				}
			}
			Console.WriteLine("See you ...");
		}
	}
}
