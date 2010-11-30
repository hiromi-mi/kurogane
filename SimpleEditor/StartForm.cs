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
using System.Text.RegularExpressions;

namespace Kurogane.SimpleEditor {
	public partial class StartForm : Form {

		const string tmpName = "-- textbox --";

		public StartForm() {
			InitializeComponent();
		}

		private void Execute() {
			var engine = new Engine();
			var writer = new StringWriter();
			engine.Out = writer;
			var code = txtProgram.Text;
			try {
				engine.Execute(code, tmpName);
				txtOut.ForeColor = Color.Black;
				txtOut.Text = writer.ToString();
			}
			catch (CompilerException ex) {
				txtOut.ForeColor = Color.Red;
				txtOut.Text = "構文が間違っています。";
				SelectLine(ex.Location.Line - 1);
			}
			catch (Exception ex) {
				txtOut.ForeColor = Color.Red;
				txtOut.Text = ex.Message;
				var regex = new Regex(@"行\s+(\d+)");
				int lineNumber = 0;
				foreach (var line in ex.StackTrace.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)) {
					if (line.Contains(tmpName)) {
						var res = regex.Match(line);
						if (res.Success) {
							lineNumber = Int32.Parse(res.Groups[1].Value);
							break;
						}
					}
				}
				if (lineNumber > 0) {
					SelectLine(lineNumber - 1);
				}
			}
		}

		private void SelectLine(int lineIndex) {
			txtProgram.Focus();
			int start = txtProgram.GetFirstCharIndexFromLine(lineIndex);
			int len = txtProgram.Lines[lineIndex].Length;
			txtProgram.Select(start, len);
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
