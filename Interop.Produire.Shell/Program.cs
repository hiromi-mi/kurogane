using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Kurogane.Interop.Produire;

namespace Kurogane.Interop.Produire.Shell {
	static class Program {
		[STAThread]
		static void Main(string[] args) {
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			//Application.Run(new Form1());
			var engine = new InteropEngine();
			if (args.Length == 0) return;
			engine.ExecuteFile(args[0]);
		}
	}
}
