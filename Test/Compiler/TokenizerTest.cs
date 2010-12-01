using Kurogane.Compiler;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

namespace Kurogane.Test.Compiler {
	[TestClass]
	public class TokenizerTest {

		[TestMethod]
		public void Tokenize_AをBにCする() {
			string code = "AをBにCする。";
			using (var reader = new StringReader(code)) {
				var token = Tokenizer.Tokenize(reader, "-- text program --");
				Assert.IsTrue(token
					.MatchFlow((SymbolToken t) => t.Text == "A")
					.MatchFlow((SuffixToken t) => t.Text == "を")
					.MatchFlow((SymbolToken t) => t.Text == "B")
					.MatchFlow((SuffixToken t) => t.Text == "に")
					.MatchFlow((SymbolToken t) => t.Text == "C")
					.MatchFlow((ReservedToken t) => t.Text == "する")
					.MatchFinish((PunctuationToken t) => t.Text == "。"));
			}
		}

		[TestMethod]
		public void Tokenize_関数宣言() {
			var code =
				"以下の定義で処理する。" +
				"	「こんにちは」を表示する。" +
				"以上。";
			using (var reader = new StringReader(code)) {
				var token = Tokenizer.Tokenize(reader, "-- text program --");
				Assert.IsTrue(token
					.MatchFlow((ReservedToken t) => t.Text == "以下")
					.MatchFlow((SuffixToken t) => t.Text == "の")
					.MatchFlow((ReservedToken t) => t.Text == "定義")
					.MatchFlow((SuffixToken t) => t.Text == "で")
					.MatchFlow((SymbolToken t) => t.Text == "処理")
					.MatchFlow((ReservedToken t) => t.Text == "する")
					.MatchFlow((PunctuationToken t) => t.Text == "。")
					.MatchFlow((LiteralToken t) => t.Text == "こんにちは")
					.MatchFlow((SuffixToken t) => t.Text == "を")
					.MatchFlow((SymbolToken t) => t.Text == "表示")
					.MatchFlow((ReservedToken t) => t.Text == "する")
					.MatchFlow((PunctuationToken t) => t.Text == "。")
					.MatchFlow((ReservedToken t) => t.Text == "以上")
					.MatchFinish((PunctuationToken t) => t.Text == "。"));
			}
		}

		[TestMethod]
		public void Tokenize_分岐文() {
			var code =
				"もし(1≦N)なら" +
				"	「正しい」を表示する。" +
				"他なら" +
				"	「間違い」を表示する。";
			using (var reader = new StringReader(code)) {
				var token = Tokenizer.Tokenize(reader, "-- text program --");
				var token2 = token
					.MatchFlow((ReservedToken t) => t.Text == "もし")
					.MatchFlow((OpenParenthesisToken t) => true)
					.MatchFlow((LiteralToken t) => (t.Value as int?) == 1)
					.MatchFlow((AbstractOperatorToken t) => t.Text == "≦")
					.MatchFlow((SymbolToken t) => t.Text == "N")
					.MatchFlow((CloseParenthesisToken t) => true)
					.MatchFlow((ReservedToken t) => t.Text == "なら");
				var token3 = token2
					.MatchFlow((LiteralToken t) => t.Text == "正しい")
					.MatchFlow((SuffixToken t) => t.Text == "を")
					.MatchFlow((SymbolToken t) => t.Text == "表示")
					.MatchFlow((ReservedToken t) => t.Text == "する")
					.MatchFlow((PunctuationToken t) => t.Text == "。");
				var token4 = token3
					.MatchFlow((ReservedToken t) => t.Text == "他")
					.MatchFlow((ReservedToken t) => t.Text == "なら");
				var end = token4
					.MatchFlow((LiteralToken t) => t.Text == "間違い")
					.MatchFlow((SuffixToken t) => t.Text == "を")
					.MatchFlow((SymbolToken t) => t.Text == "表示")
					.MatchFlow((ReservedToken t) => t.Text == "する")
					.MatchFinish((PunctuationToken t) => t.Text == "。");
				Assert.IsTrue(end);
			}
		}
	}
}
