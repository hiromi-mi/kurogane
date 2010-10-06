using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace Kurogane.Compilers
{
	public class AnotherParser
	{

		public static Block Parse(Token token, string filename)
		{
			var p = new AnotherParser(filename);
			var pair = p.ParseBlock(token);

			if (pair.Token is NullToken)
				return pair.Node;
			else
				throw new SyntaxException(
					"プログラムを最後まで読み取ることができませんでした。",
					filename, pair.Token.LineNumber, pair.Token.CharCount);
		}

		private readonly string _FileName;

		private AnotherParser(string filename)
		{
			_FileName = filename;
		}

		#region Parse

		private IPair<Block> ParseBlock(Token token)
		{
			List<IStatement> stmtList = new List<IStatement>();
			while (true) {
				var pair = ParseIStatement(token);
				if (pair.Token != null)
					break;
				token = pair.Token;
				stmtList.Add(pair.Node);
			}
			return MakePair(new Block(stmtList), token);
		}

		private IPair<IStatement> ParseIStatement(Token token)
		{
			var ifPair = TryParseIfStatement(token);
			if (ifPair != null)
				return ifPair;
			return ParseINormalStatement(token);
		}

		private IPair<INormalStatement> ParseINormalStatement(Token token)
		{
			var ret =
				TryParseDefun(token) ??
				TryParseBlockExecute(token) ??
				TryParseCall(token);
			if (ret == null)
				ThrowError("解析できないトークンが現れました。", token);

			return ret;
		}

		#region もし文

		private IPair<IfStatement> TryParseIfStatement(Token token)
		{
			token = token.MatchFlow((ReservedToken t) => t.Value == "もし");
			if (token == null)
				return null;
			if (token.Match((CommaToken t) => true))
				token = token.Next;

			var thens = new List<CondThenPair>();
			while (true) {
				var pair = TryParseCondThenPair(token);
				if (pair == null)
					break;
				token = pair.Token;
				thens.Add(pair.Node);
			}
			if (thens.Count == 0)
				ThrowError("「もし」の後ろがありません。", token);
			return MakePair(new IfStatement(thens), token);
		}

		private IPair<CondThenPair> TryParseCondThenPair(Token token)
		{
			var condPair = ParseElement(token);
			if (condPair == null)
				return null;
			token = condPair.Token
				.MatchFlow((ReservedToken t) => t.Value == "なら");
			if (token == null)
				return null;

			var thenPair = ParseINormalStatement(token);
			if (thenPair == null)
				ThrowError("「なら」の後ろが正しく解析できません。", token);

			return MakePair(new CondThenPair(condPair.Node, thenPair.Node), thenPair.Token);
		}

		#endregion

		#region 関数定義

		private IPair<Defun> TryParseDefun(Token token)
		{
			var keywordSkipped = token
				.MatchFlow((ReservedToken t) => t.Value == "以下")
				.MatchFlow((SuffixToken t) => t.Value == "の")
				.MatchFlow((ReservedToken t) => t.Value == "手順")
				.MatchFlow((SuffixToken t) => t.Value == "で");
			if (keywordSkipped == null)
				return null;
			token = keywordSkipped.MatchFlow((CommaToken) => true) ?? keywordSkipped;
			var paramList = new List<ParamSuffixPair>();
			while (true) {
				var paramPair = TryParseParamSuffixPair(token);
				if (paramPair == null)
					break;
				paramList.Add(paramPair.Node);
				token = paramPair.Token;
			}
			var blockToken = token
				.MatchFlow((SymbolToken t) => true)
				.MatchFlow((ReservedToken t) => t.Value == "する")
				.MatchFlow((PeriodToken t) => true);
			if (blockToken == null)
				ThrowError("正しく関数が定義出来ていません。", token);
			var name = ((SymbolToken)token).Value;
			var blockPair = ParseBlock(token);
			var lastToken = blockPair.Token
				.MatchFlow((ReservedToken t) => t.Value == "以上")
				.MatchFlow((PeriodToken t) => true);
			if (lastToken == null)
				ThrowError("関数のブロックが正しく閉じられていません。", blockPair.Token);
			return MakePair(new Defun(name, paramList, blockPair.Node), lastToken);
		}

		private IPair<ParamSuffixPair> TryParseParamSuffixPair(Token token)
		{
			var lastToken = token
				.MatchFlow((SymbolToken t) => true)
				.MatchFlow((SuffixToken t) => true);
			if (lastToken == null)
				return null;
			var name = ((SymbolToken)token).Value;
			var sfx = ((SuffixToken)token.Next).Value;
			return MakePair(new ParamSuffixPair(name, sfx), lastToken);
		}

		#endregion

		private IPair<BlockExecute> TryParseBlockExecute(Token token)
		{
			var blockToken = token
				.MatchFlow((ReservedToken t) => t.Value == "以下")
				.MatchFlow((SuffixToken t) => t.Value == "を")
				.MatchFlow((ReservedToken t) => t.Value == "する")
				.MatchFlow((PeriodToken t) => true);
			if (blockToken == null)
				return null;
			var blockPair = ParseBlock(blockToken);
			var lastToken = blockPair.Token
				.MatchFlow((ReservedToken t) => t.Value == "以上")
				.MatchFlow((PeriodToken t) => true);
			if (lastToken == null)
				ThrowError("関数が正しく閉じられていません。", blockPair.Token);
			return MakePair(new BlockExecute(blockPair.Node), blockPair.Token);
		}

		private IPair<INormalStatement> TryParseCall(Token token)
		{
			throw new NotImplementedException();
		}

		#region 要素

		private IPair<Element> ParseElement(Token token)
		{
			throw new NotImplementedException();
		}

		private IPair<ListLiteral> TryParseList(Token token)
		{
			token = token.MatchFlow((OpenBracketToken) => true);
			if (token == null)
				return null;
			var elemList = new List<Element>();
			while (true) {
				if (token.Match((CloseBracketToken t) => true))
					break;
				var elemPair = ParseElement(token);
				if (elemPair == null)
					ThrowError("リストの要素が解析できません。", token);

				elemList.Add(elemPair.Node);
				token = elemPair.Token;

				if (token.Match((CommaToken t) => true)) {
					token = token.Next;
					continue;
				}
				if (token.Match((CloseBracketToken t) => true))
					break;
				ThrowError("リストの要素が解析できません。", token);
			}
			return MakePair(new ListLiteral(elemList), token.Next);
		}

		#region BinaryExpr

		/// binExpr ::= andExpr | binExpr OrOp  andExpr
		/// orExpr  ::= cmpExpr |  orExpr AndOp cmpExpr
		/// cmpExpr ::= addExpr | cmpExpr CmpOp addExpr
		/// addExpr ::= mltExpr | addExpr AddOp mltExpr
		/// mltExpr ::= Element | mltExpr MltOp Element
		private IPair<Element> ParseBinaryExpr(Token token)
		{
			throw new NotImplementedException();
		}

		private IPair<Element> ParseAndExpr(Token token)
		{
			throw new NotImplementedException();
		}

		private IPair<Element> ParseCompareExpr(Token token)
		{
			throw new NotImplementedException();
		}

		private IPair<Element> ParseAddExpr(Token token)
		{
			throw new NotImplementedException();
		}

		private IPair<Element> ParseMultipleExpr(Token token)
		{
			throw new NotImplementedException();

		}

		#endregion

		private IPair<Element> ParseUnaryExpr(Token token)
		{
			string op = null;
			if (token.Match((AbstractOperatorToken t) => true)) {
				op = ((AbstractOperatorToken)token).Value;
				token = token.Next;
			}
			var propPair = TryParseProperty(token);
			if (op == null)
				return propPair;
			return MakePair(new UnaryExpr(op, propPair.Node), propPair.Token);
		}

		private IPair<Element> TryParseProperty(Token token)
		{
			IPair<Element> pair = TryParseUnit(token);
			if (pair == null)
				return null;
			while (true) {
				var nextToken = pair.Token
					.MatchFlow((SuffixToken t) => t.Value == "の")
					.MatchFlow((SymbolToken t) => true);
				if (nextToken == null)
					break;
				var propName = ((SymbolToken)pair.Token.Next).Value;
				pair = MakePair(new PropertyAccess(pair.Node, propName), nextToken);
			}
			return pair;
		}

		private IPair<Element> TryParseUnit(Token token)
		{
			return
				TryParseSymbol(token) ??
				TryParseParenthesisExpr(token) ??
				TryParseLiteral(token);
		}

		private IPair<Element> TryParseParenthesisExpr(Token token)
		{
			token = token.MatchFlow((OpenParenthesisToken t) => true);
			if (token == null)
				return null;
			var elemPair = ParseElement(token);
			var lastToken = elemPair.Token
				.MatchFlow((CloseParenthesisToken t) => true);
			if (lastToken == null)
				ThrowError("閉じ括弧が出現していません。", elemPair.Token);
			return MakePair(elemPair.Node, lastToken);
		}

		private IPair<Symbol> TryParseSymbol(Token token)
		{
			var symToken = token as SymbolToken;
			if (symToken == null)
				return null;
			else
				return MakePair(new Symbol(symToken.Value), token.Next);
		}

		private IPair<Literal> TryParseLiteral(Token token)
		{
			var nextToken = token.Next;
			if (token is LiteralToken) {
				var value = ((LiteralToken)token).Value;
				return MakePair(new StringLiteral(value), nextToken);
			}
			if (token is IntegerToken) {
				int value = ((IntegerToken)token).IntValue;
				return MakePair(new IntLiteral(value), nextToken);
			}
			if (token is DecimalToken) {
				double value = ((DecimalToken)token).DecimalValue;
				return MakePair(new RealLiteral(value), nextToken);
			}
			if (token.Match((ReservedToken t) => t.Value == ConstantNames.NullText)) {
				return MakePair(new NullLiteral(), nextToken);
			}
			return null;
		}

		#endregion

		#endregion

		#region Util

		[DebuggerStepThrough]
		private void ThrowError(string message, Token token)
		{
			throw new SyntaxException(message, _FileName, token.LineNumber, token.CharCount);
		}

		private static IPair<T> MakePair<T>(T node, Token token)
		{
			return new Pair<T>(node, token);
		}

		private interface IPair<out T>
		{
			T Node { get; }
			Token Token { get; }
		}

		private class Pair<T> : IPair<T>
		{
			private T _node;
			private Token _token;

			public T Node { get { return _node; } }
			public Token Token { get { return _token; } }

			public Pair(T node, Token token)
			{
				_node = node;
				_token = token;
			}
		}


		#endregion

	}
}
