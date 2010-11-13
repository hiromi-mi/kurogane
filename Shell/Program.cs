using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Kurogane.Compiler;

namespace Kurogane.Shell {
	public class Program {

		const string ConsoleWait = "> ";

		/// <summary>????????</summary>
		public static void Main(string[] args) {
			if (args.Length == 0)
				StartRepl();
			else
				ExecuteFile(args[0]);
		}

		/// <summary>
		/// ?????????
		/// </summary>
		private static void ExecuteFile(string filepath) {
			if (!File.Exists(filepath)) {
				Console.Error.WriteLine("?????{0}?????????", filepath);
				Environment.Exit(-1);
			}
			var engine = new Engine();
			engine.ExecuteFile(filepath);
		}

		/// <summary>
		/// ?????????????????
		/// </summary>
		private static void StartRepl() {
			string before = String.Empty;
			var engine = new Engine();
			Console.Write(ConsoleWait);
			while (true) {
				string line = Console.ReadLine();
				if (line == "exit" || line.StartsWith("??"))
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
					Console.Error.WriteLine(e.StackTrace);
					Console.ForegroundColor = beforeColor;
					Console.Write(ConsoleWait);
				}
			}
			Console.WriteLine("See you ...");
		}
	}
}
