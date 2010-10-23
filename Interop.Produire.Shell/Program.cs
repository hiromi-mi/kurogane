using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Kurogane.Interop.Produire;

namespace Kurogane.Interop.Produire.Shell {
	static class Program {
		[STAThread]
		static void Main(string[] args) {
			if (args.Length == 0) return;
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			//Application.Run(new Form1());
			var engine = new InteropEngine();
			engine.ExecuteFile(args[0]);
		}
	}
}
