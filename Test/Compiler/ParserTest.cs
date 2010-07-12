using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Kurogane.Compiler;

namespace Kurogane.Test.Compiler {

	[TestClass]
	public class ParserTest {

		[TestMethod]
		public void TestMethod1() {
			var code = "[A]を[B]に[追加]する。";
			var ast = Parser.Parse(Tokenizer.Tokenize(code));
			Assert.AreEqual(1, ast.Statements.Count);
			var line = ast.Statements[0] as StatementNode;
			Assert.IsNotNull(line);
			Assert.AreEqual(1, line.Procedures.Count);
			var addStmt = line.Procedures[0];
			Assert.AreEqual("追加", addStmt.Name);
			var argA = addStmt.Arguments.First(a => a.PostPosition == "を").Target;
			Assert.IsTrue(argA is ReferenceExpression);
			Assert.AreEqual("A", ((ReferenceExpression)argA).Name);
		}
	}
}
