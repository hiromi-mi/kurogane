using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Kurogane.Compiler;

namespace Kurogane.Test.Compiler {
	[TestClass]
	public class TokenizerTest {
		
		[TestMethod]
		public void Tokenize_AをBにCする() {
			string code = "[A]を[B]に[C]する。";
			var token = Tokenizer.Tokenize(code);

			Assert.IsTrue(token
				.MatchFlow((SymbolToken t) => t.Value == "A")
				.MatchFlow((PostPositionToken t) => t.Value == "を")
				.MatchFlow((SymbolToken t) => t.Value == "B")
				.MatchFlow((PostPositionToken t) => t.Value == "に")
				.MatchFlow((SymbolToken t) => t.Value == "C")
				.MatchFlow((ReservedToken t) => t.Value == "する")
				.MatchFinish((PunctuationToken t) => t.Value == "。"));
		}

		[TestMethod]
		public void Tokenize_関数宣言() {
			var code =
				"以下の手順で[処理]する。" +
				"以上。";
			var token = Tokenizer.Tokenize(code);
			Assert.IsTrue(token
				.MatchFlow((ReservedToken t) => t.Value == "以下")
				.MatchFlow((PostPositionToken t) => t.Value == "の")
				.MatchFlow((ReservedToken t) => t.Value == "手順")
				.MatchFlow((PostPositionToken t) => t.Value == "で")
				.MatchFlow((SymbolToken t) => t.Value == "処理")
				.MatchFlow((ReservedToken t) => t.Value == "する")
				.MatchFlow((PunctuationToken t) => t.Value == "。")
				.MatchFlow((ReservedToken t) => t.Value == "以上")
				.MatchFinish((PunctuationToken t) => t.Value == "。"));
		}
	}
}
