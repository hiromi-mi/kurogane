using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Kurogane;
using System.IO;
using Kurogane.Compiler;

namespace Kurogane.SimpleEditor {
	public partial class StartForm : Form {

		public StartForm() {
			InitializeComponent();
		}

		private void Execute() {
			var engine = new Engine();
			var writer = new StringWriter();
			engine.Out = writer;
			var code = txtProgram.Text;
			try {
				engine.Execute(code);
				txtOut.ForeColor = Color.Black;
				txtOut.Text = writer.ToString();
			}
			catch (CompilerException ex) {
				txtOut.ForeColor = Color.Red;
				txtOut.Text = "コンパイルエラーです。" + Environment.NewLine +
					ex.StackTrace;
			}
			catch (Exception ex){
				txtOut.ForeColor = Color.Red;
				txtOut.Text = ex.StackTrace;
			}
		}

		private void OpenFile() {
			var win = new OpenFileDialog();
			win.FileName = "プログラム.krg";
			win.Filter = "クロガネファイル(*.krg)|*.krg|すべてのファイル|*.*";
			win.RestoreDirectory = true;
			if (win.ShowDialog() == DialogResult.OK) {
				txtProgram.Text = File.ReadAllText(win.FileName, Encoding.Default);
			}
		}

		private void Save() {
			var win = new SaveFileDialog();
			win.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
			win.FileName = "プログラム.krg";
			win.Filter = "クロガネファイル(*.krg)|*.krg";
			win.RestoreDirectory = true;
			if (win.ShowDialog() == DialogResult.OK) {
				File.WriteAllText(win.FileName, txtProgram.Text, Encoding.Default);
			}
		}

		private void txtProgram_KeyDown(object sender, KeyEventArgs e) {
			if (e.KeyCode == Keys.F5)
				Execute();
			if (e.KeyCode == (Keys.ControlKey | Keys.S))
				Save();
			if (e.KeyCode == (Keys.ControlKey | Keys.O))
				OpenFile();
		}

		private void btnExecute_Click(object sender, EventArgs e) {
			Execute();
		}

		private void btnSave_Click(object sender, EventArgs e) {
			Save();
		}

		private void btnOpen_Click(object sender, EventArgs e) {
			OpenFile();
		}
	}
}
