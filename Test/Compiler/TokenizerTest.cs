using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Kurogane.Compilers;

namespace Kurogane.Test.Compiler {
	[TestClass]
	public class TokenizerTest {
		
		[TestMethod]
		public void Tokenize_AをBにCする() {
			string code = "AをBにCする。";
			var token = Tokenizer.Tokenize(code);

			Assert.IsTrue(token
				.MatchFlow((SymbolToken t) => t.Value == "A")
				.MatchFlow((SuffixToken t) => t.Value == "を")
				.MatchFlow((SymbolToken t) => t.Value == "B")
				.MatchFlow((SuffixToken t) => t.Value == "に")
				.MatchFlow((SymbolToken t) => t.Value == "C")
				.MatchFlow((ReservedToken t) => t.Value == "する")
				.MatchFinish((PunctuationToken t) => t.Value == "。"));
		}

		[TestMethod]
		public void Tokenize_関数宣言() {
			var code =
				"以下の手順で処理する。" +
				"	「こんにちは」を表示する。" +
				"以上。";
			var token = Tokenizer.Tokenize(code);
			Assert.IsTrue(token
				.MatchFlow((ReservedToken t) => t.Value == "以下")
				.MatchFlow((SuffixToken t) => t.Value == "の")
				.MatchFlow((ReservedToken t) => t.Value == "手順")
				.MatchFlow((SuffixToken t) => t.Value == "で")
				.MatchFlow((SymbolToken t) => t.Value == "処理")
				.MatchFlow((ReservedToken t) => t.Value == "する")
				.MatchFlow((PunctuationToken t) => t.Value == "。")
				.MatchFlow((LiteralToken t) => t.Value == "こんにちは")
				.MatchFlow((SuffixToken t) => t.Value == "を")
				.MatchFlow((SymbolToken t) => t.Value == "表示")
				.MatchFlow((ReservedToken t) => t.Value == "する")
				.MatchFlow((PunctuationToken t) => t.Value == "。")
				.MatchFlow((ReservedToken t) => t.Value == "以上")
				.MatchFinish((PunctuationToken t) => t.Value == "。"));
		}

		[TestMethod]
		public void Tokenize_分岐文() {
			var code =
				"もし(1≦N)なら" +
				"	「正しい」を表示する。" +
				"他なら" +
				"	「間違い」を表示する。";
			var token = Tokenizer.Tokenize(code);

			var token2 = token
				.MatchFlow((ReservedToken t) => t.Value == "もし")
				.MatchFlow((OpenParenthesisToken t) => true)
				.MatchFlow((IntegerToken t) => t.IntValue == 1)
				.MatchFlow((AbstractOperatorToken t) => t.Value == "≦")
				.MatchFlow((SymbolToken t) => t.Value == "N")
				.MatchFlow((CloseParenthesisToken t) => true)
				.MatchFlow((ReservedToken t) => t.Value == "なら");

			var token3 = token2
				.MatchFlow((LiteralToken t) => t.Value == "正しい")
				.MatchFlow((SuffixToken t) => t.Value == "を")
				.MatchFlow((SymbolToken t) => t.Value == "表示")
				.MatchFlow((ReservedToken t) => t.Value == "する")
				.MatchFlow((PunctuationToken t) => t.Value == "。");

			var token4 = token3
				.MatchFlow((ReservedToken t) => t.Value == "他")
				.MatchFlow((ReservedToken t) => t.Value == "なら");

			var end = token4
				.MatchFlow((LiteralToken t) => t.Value == "間違い")
				.MatchFlow((SuffixToken t) => t.Value == "を")
				.MatchFlow((SymbolToken t) => t.Value == "表示")
				.MatchFlow((ReservedToken t) => t.Value == "する")
				.MatchFinish((PunctuationToken t) => t.Value == "。");

			Assert.IsTrue(end);
		}
	}
}
