using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace Kurogane.Compilers {
	public class Parser {

		public static Block Parse(Token token, string filename) {
			var p = new Parser(filename);
			var pair = p.ParseBlock(token);

			if (pair.Token is NullToken)
				return pair.Node;
			else
				throw new SyntaxException(
					"プログラムを最後まで読み取ることができませんでした。",
					filename, pair.Token.LineNumber, pair.Token.CharCount);
		}

		private readonly string _FileName;

		private Parser(string filename) {
			_FileName = filename;
		}

		#region Parse

		private IPair<Block> ParseBlock(Token token) {
			List<IStatement> stmtList = new List<IStatement>();
			while (true) {
				var pair = ParseIStatement(token);
				if (pair == null)
					break;
				token = pair.Token;
				stmtList.Add(pair.Node);
			}
			return MakePair(new Block(stmtList), token);
		}

		private IPair<IStatement> ParseIStatement(Token token) {
			var ifPair = TryParseIfStatement(token);
			if (ifPair != null)
				return ifPair;
			return TryParseINormalStatement(token);
		}

		#region もし文

		private IPair<IfStatement> TryParseIfStatement(Token token) {
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

		private IPair<CondThenPair> TryParseCondThenPair(Token token) {
			var condPair = TryParseElement(token);
			if (condPair == null)
				return null;
			token = condPair.Token
				.MatchFlow((ReservedToken t) => t.Value == "なら");
			if (token == null)
				return null;

			var thenPair = TryParseINormalStatement(token);
			if (thenPair == null)
				ThrowError("「なら」の後ろが正しく解析できません。", token);

			return MakePair(new CondThenPair(condPair.Node, thenPair.Node), thenPair.Token);
		}

		#endregion

		private IPair<INormalStatement> TryParseINormalStatement(Token token) {
			return
				TryParseExprBlock(token) ??
				TryParseDefun(token) ??
				TryParseBlockExecute(token) ??
				TryParsePhraseChain(token) as IPair<INormalStatement>;
		}

		#region 関数定義

		private IPair<Defun> TryParseDefun(Token token) {
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

		private IPair<ParamSuffixPair> TryParseParamSuffixPair(Token token) {
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

		private IPair<BlockExecute> TryParseBlockExecute(Token token) {
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

		#region PhraseChain

		private IPair<PhraseChain> TryParsePhraseChain(Token token) {
			var list = new List<IPhrase>();
			while (true) {
				var pair = TryParsePhrase(token);
				if (pair == null)
					return null;
				list.Add(pair.Node);
				if (pair.Token is CommaToken) {
					token = pair.Token.Next;
					continue;
				}
				if (pair.Token is PeriodToken) {
					token = pair.Token.Next;
					break;
				}
				return null;
			}
			if (list.Count == 0)
				return null;
			return MakePair(new PhraseChain(list), token);
		}

		private IPair<IPhrase> TryParsePhrase(Token token) {
			var lst = new List<ArgSuffixPair>();
			bool isMap = false;
			ArgSuffixPair mappedArg = null;
			while (true) {
				if (token.Match((ReservedToken t) => t.Value == "それぞれ")) {
					if (isMap)
						ThrowError("「それぞれ」を二箇所で使うことはできません。", token);
					if (lst.Count > 1)
						ThrowError("「それぞれ」に対して、二つ以上の引数を与えることはできません。", token);
					isMap = true;
					if (lst.Count == 1)
						mappedArg = lst[0];
				}
				var argPair = TryParseArgSfxPair(token);
				if (argPair == null)
					break;
				lst.Add(argPair.Node);
				token = argPair.Token;
				if (token.Match((SuffixToken t) => true)) {
					var sfx = ((SuffixToken)token).Value;
					lst.Add(new ArgSuffixPair(NullLiteral.Instant, sfx));
					token = token.Next;
				}
			}
			if (token.Match((ReservedToken t) => t.Value == "し" || t.Value == "する")) {
				var dfn = CreateDefine(lst);
				if (dfn == null)
					ThrowError("引数が正しくありません。", token);
				return MakePair(dfn, token.Next);
			}
			var last = token
				.MatchFlow((SymbolToken t) => true)
				.MatchFlow((ReservedToken t) => t.Value == "し" || t.Value == "する");
			bool isMaybe = false;
			if (last == null) {
				last = token
					.MatchFlow((SymbolToken t) => true)
					.MatchFlow((ReservedToken t) => t.Value == "してみ" || t.Value == "してみて");
				if (last == null)
					return null;
				isMaybe = true;
			}
			if (last != null) {
				string verb = ((SymbolToken)token).Value;
				if (verb == "代入") {
					var assign = CreateAssign(lst);
					if (assign == null)
						return null;
					return MakePair(assign, last);
				}
				if (isMap)
					return MakePair(new MapCall(verb, mappedArg, lst, isMaybe), last);
				else
					return MakePair(new Call(verb, lst, isMaybe), last);
			}
			return null;
		}

		private DefineValue CreateDefine(List<ArgSuffixPair> args) {
			if (args.Count == 0)
				return null;
			var last = args.Last();
			if (last.Suffix != "と")
				return null;
			var symbol = last.Argument as Symbol;
			if (symbol == null)
				return null;
			var name = symbol.Name;
			args.RemoveAt(args.Count - 1);
			if (args.Count == 0)
				return new DefineValue(name, null);
			if (args.Last().Suffix != "を")
				return null;
			var tuple = CreateTuple(args);
			if (tuple == null)
				return null;
			return new DefineValue(name, tuple);
		}

		private Assign CreateAssign(List<ArgSuffixPair> lst) {
			if (lst.Count == 0)
				return null;
			Func<ArgSuffixPair, string> getName = pair => {
				if (pair.Suffix != "に")
					return null;
				var sym = pair.Argument as Symbol;
				if (sym == null)
					return null;
				return sym.Name;
			};
			var name = getName(lst[lst.Count - 1]);
			if (name != null) {
				lst.RemoveAt(lst.Count - 1);
				var value = CreateTuple(lst);
				if (value == null)
					return null;
				return new Assign(name, value);
			}
			name = getName(lst[0]);
			if (name != null) {
				lst.RemoveAt(0);
				var value = CreateTuple(lst);
				if (value == null)
					return null;
				return new Assign(name, value);
			}
			return null;
		}

		private Element CreateTuple(List<ArgSuffixPair> args) {
			var tuple = args.Last().Argument;
			for (int i = args.Count - 2; i >= 0; i--) {
				var elem = args[i];
				if (elem.Suffix != "の")
					return null;
				tuple = new TupleLiteral(elem.Argument, tuple);
			}
			return tuple;
		}

		private IPair<ArgSuffixPair> TryParseArgSfxPair(Token token) {
			var elemPair = TryParseElement(token);
			if (elemPair == null)
				return null;
			var sfxToken = elemPair.Token as SuffixToken;
			if (sfxToken == null)
				return null;
			return MakePair(new ArgSuffixPair(elemPair.Node, sfxToken.Value), sfxToken.Next);
		}

		#endregion

		private IPair<ExprBlock> TryParseExprBlock(Token token) {
			token = token.MatchFlow((OpenBraceToken t) => true);
			if (token == null)
				return null;
			var list = new List<IExpr>();
			while (true) {
				var exprPair = TryParseExpr(token);
				if (exprPair == null)
					return null;
				token = exprPair.Token;
				var semi = token as SemicolonToken;
				if (semi != null)
					token = semi.Next;
				var close = token as CloseBraceToken;
				if (close != null) {
					token = close.Next;
					break;
				}
				if (semi != null)
					continue;
				return null;
			}
			return MakePair(new ExprBlock(list), token);
		}

		private IPair<IExpr> TryParseExpr(Token token) {
			return
				TryParseExprBlock(token) ??
				TryParseElement(token) as IPair<IExpr>;
		}

		#region 要素

		private IPair<Element> TryParseElement(Token token) {
			return
				TryParseList(token) ??
				TryParseUnit(token);
		}

		private IPair<ListLiteral> TryParseList(Token token) {
			token = token.MatchFlow((OpenBracketToken t) => true);
			if (token == null)
				return null;
			var elemList = new List<Element>();
			while (true) {
				if (token.Match((CloseBracketToken t) => true))
					break;
				var elemPair = TryParseElement(token);
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
		/// mltExpr ::= ukExpr  | mltExpr MltOp unExpr
		/// unExpr  ::= unary   |  unExpr UnknownOp unary
		private IPair<Element> ParseBinaryExpr(Token token) {
			IPair<Element> pair = ParseAndExpr(token);
			if (pair == null)
				return null;
			token = pair.Token;
			while (true) {
				if (token is OrOpToken) {
					var opToken = (OrOpToken)token;
					var rightPair = ParseAndExpr(token.Next);
					if (rightPair == null)
						ThrowError("右辺が見つかりません。", token.Next);
					pair = MakePair(new BinaryExpr(pair.Node, opToken.Value, rightPair.Node), rightPair.Token);
				}
				break;
			}
			return pair;
		}

		private IPair<Element> ParseAndExpr(Token token) {
			IPair<Element> pair = ParseCompareExpr(token);
			if (pair == null)
				return null;
			token = pair.Token;
			while (true) {
				if (token is AndOpToken) {
					var opToken = (AndOpToken)token;
					var rightPair = ParseCompareExpr(token.Next);
					if (rightPair == null)
						ThrowError("右辺が見つかりません。", token.Next);
					pair = MakePair(new BinaryExpr(pair.Node, opToken.Value, rightPair.Node), rightPair.Token);
				}
				break;
			}
			return pair;
		}

		private IPair<Element> ParseCompareExpr(Token token) {
			IPair<Element> pair = ParseAddExpr(token);
			if (pair == null)
				return null;
			token = pair.Token;
			while (true) {
				if (token is EqualOpToken || token is NotEqualOpToken
					|| token is LessThanOpToken || token is LessThanEqualOpToken
					|| token is GreaterThanOpToken || token is GreaterThanEqualOpToken) {

					var opToken = (AbstractOperatorToken)token;
					var rightPair = ParseAddExpr(token.Next);
					if (rightPair == null)
						ThrowError("右辺が見つかりません。", token.Next);
					pair = MakePair(new BinaryExpr(pair.Node, opToken.Value, rightPair.Node), rightPair.Token);
				}
				break;
			}
			return pair;
		}

		private IPair<Element> ParseAddExpr(Token token) {
			IPair<Element> pair = ParseMultipleExpr(token);
			if (pair == null)
				return null;
			token = pair.Token;
			while (true) {
				if (token is AddOpToken || token is SubOpToken) {
					var opToken = (AbstractOperatorToken)token;
					var rightPair = ParseMultipleExpr(token.Next);
					if (rightPair == null)
						ThrowError("右辺が見つかりません。", token.Next);
					pair = MakePair(new BinaryExpr(pair.Node, opToken.Value, rightPair.Node), rightPair.Token);
				}
				break;
			}
			return pair;
		}

		private IPair<Element> ParseMultipleExpr(Token token) {
			IPair<Element> pair = ParseUnknownMultipleExpr(token);
			if (pair == null)
				return null;
			token = pair.Token;
			while (true) {
				if (token is MultipleOpToken || token is DivideOpToken || token is ModuloOpToken) {
					var opToken = (AbstractOperatorToken)token;
					var rightPair = ParseUnknownMultipleExpr(token.Next);
					if (rightPair == null)
						ThrowError("右辺が見つかりません。", token.Next);
					pair = MakePair(new BinaryExpr(pair.Node, opToken.Value, rightPair.Node), rightPair.Token);
				}
				break;
			}
			return pair;
		}

		private IPair<Element> ParseUnknownMultipleExpr(Token token) {
			IPair<Element> pair = ParseUnaryExpr(token);
			if (pair == null)
				return null;
			token = pair.Token;
			while (true) {
				if (token is UnknownOperatorToken) {
					var opToken = (UnknownOperatorToken)token;
					var rightPair = ParseUnaryExpr(token.Next);
					if (rightPair == null)
						ThrowError("右辺が見つかりません。", token.Next);
					pair = MakePair(new BinaryExpr(pair.Node, opToken.Value, rightPair.Node), rightPair.Token);
				}
				break;
			}
			return pair;
		}

		#endregion

		private IPair<Element> ParseUnaryExpr(Token token) {
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

		private IPair<Element> TryParseProperty(Token token) {
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

		private IPair<Element> TryParseUnit(Token token) {
			return
				TryParseSymbol(token) ??
				TryParseParenthesisExpr(token) ??
				TryParseLiteral(token);
		}

		private IPair<Element> TryParseParenthesisExpr(Token token) {
			token = token.MatchFlow((OpenParenthesisToken t) => true);
			if (token == null)
				return null;
			var elemPair = TryParseElement(token);
			var lastToken = elemPair.Token
				.MatchFlow((CloseParenthesisToken t) => true);
			if (lastToken == null)
				ThrowError("閉じ括弧が出現していません。", elemPair.Token);
			return MakePair(elemPair.Node, lastToken);
		}

		private IPair<Symbol> TryParseSymbol(Token token) {
			var symToken = token as SymbolToken;
			if (symToken == null)
				return null;
			else
				return MakePair(new Symbol(symToken.Value), token.Next);
		}

		private IPair<Literal> TryParseLiteral(Token token) {
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
				return MakePair(new FloatLiteral(value), nextToken);
			}
			if (token.Match((ReservedToken t) => t.Value == ConstantNames.NullText)) {
				return MakePair(NullLiteral.Instant, nextToken);
			}
			return null;
		}

		#endregion

		#endregion

		#region Util

		[DebuggerStepThrough]
		private void ThrowError(string message, Token token) {
			throw new SyntaxException(message, _FileName, token.LineNumber, token.CharCount);
		}

		private static IPair<T> MakePair<T>(T node, Token token) {
			return new Pair<T>(node, token);
		}

		private interface IPair<out T> {
			T Node { get; }
			Token Token { get; }
		}

		private class Pair<T> : IPair<T> {
			private T _node;
			private Token _token;

			public T Node { get { return _node; } }
			public Token Token { get { return _token; } }

			public Pair(T node, Token token) {
				_node = node;
				_token = token;
			}
		}

		#endregion

	}
}
