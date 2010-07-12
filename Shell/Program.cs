using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kurogane.Compiler;
using System.IO;
using Kurogane.Runtime;

namespace Kurogane.Shell {
	public class Program {

		public static void Main(string[] args) {
			if (args.Length == 0)
				StartRepl();
		}

		/// <summary>
		/// コンソールで一行ずつ実行するモード
		/// </summary>
		private static void StartRepl() {
			string before = "";
			var engine = new Engine();
			while (true) {
				Console.Write("> ");
				string line = Console.ReadLine();
				if (line == "exit" || line == "終了")
					break;
				engine.Execute(before + line);
				before = "";
			}
			Console.WriteLine("See you ...");
		}
	}
}
