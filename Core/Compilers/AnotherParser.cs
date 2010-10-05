using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kurogane.Compilers
{
	public class AnotherParser
	{

		#region static

		public static Block Parse(Token token)
		{
			var p = new AnotherParser(null);
			var pair = p.ParseBlockInternal(token);
			if (pair.Token is NullToken) {
				// 成功
			}
			else {
				// 失敗
			}

			throw new NotImplementedException();
		}

		#endregion

		private readonly string _FileName;

		public AnotherParser(string filename)
		{
			_FileName = filename;
		}

		#region Parse

		private IPair<Block> ParseBlockInternal(Token token)
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
			var ifPair = ParseIfStatement(token);
			if (ifPair != null)
				return ifPair;
			return ParseINormalStatement(token);
		}

		private IPair<INormalStatement> ParseINormalStatement(Token token)
		{
			var ret =
				ParseDefun(token) ??
				ParseBlock(token) ??
				ParseCall(token);
			if (ret == null)
				throw new SyntaxException("解析できないトークンが現れました。" + ErrorLocation(token));

			return ret;
		}

		#region IF文

		private IPair<IfStatement> ParseIfStatement(Token token)
		{
			token = token.MatchFlow((ReservedToken t) => t.Value == "もし");
			if (token == null)
				return null;
			if (token.Match((CommaToken t) => true))
				token = token.Next;

			var thens = new List<CondThenPair>();
			while (true) {
				var pair = ParseCondThenPair(token);
				if (pair == null)
					break;
				token = pair.Token;
				thens.Add(pair.Node);
			}
			if (thens.Count == 0) {
				throw new SyntaxException("「もし」の後ろがありません。" + ErrorLocation(token));
			}
			return MakePair(new IfStatement(thens), token);
		}

		private IPair<CondThenPair> ParseCondThenPair(Token token)
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
				throw new SyntaxException("「なら」の後ろが正しく解析できませんでした。" + ErrorLocation(token));

			return MakePair(new CondThenPair(condPair.Node, thenPair.Node), thenPair.Token);
		}

		#endregion

		#region Defun

		private IPair<Defun> ParseDefun(Token token)
		{
			throw new NotImplementedException();
		}

		#endregion

		private IPair<Block> ParseBlock(Token token)
		{
			throw new NotImplementedException();
		}

		private IPair<INormalStatement> ParseCall(Token token)
		{
			throw new NotImplementedException();
		}

		#region 要素

		private IPair<Element> ParseElement(Token token)
		{
			throw new NotImplementedException();
		}

		private IPair<ListLiteral> ParseList(Token token)
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
					throw new SyntaxException("リストの要素が解析できませんでした。" + ErrorLocation(token));

				elemList.Add(elemPair.Node);
				token = elemPair.Token;

				if (token.Match((CommaToken t) => true))
					continue;
				if (token.Match((CloseBracketToken t) => true))
					break;
				throw new SyntaxException("リストの要素が解析できませんでした。" + ErrorLocation(token));
			}
			return MakePair(new ListLiteral(elemList), token.Next);
		}

		#endregion


		#endregion

		#region Util

		private string ErrorLocation(Token token)
		{
			return Environment.NewLine + String.Format("{0}の{1}行{2}文字目。", _FileName, token.LineNumber, token.CharCount);
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
