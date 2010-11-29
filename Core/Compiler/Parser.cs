using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Diagnostics.Contracts;

namespace Kurogane.Compiler {

	public class Parser {

		/// <summary>
		/// プログラム全体を構文解析し、Blockにして返す。
		/// 構文解析できなかった場合、SyntaxExceptionが発生する。
		/// </summary>
		/// <param name="token">プログラムのトークン列</param>
		/// <param name="filename">ファイル名（デバッグ用）</param>
		/// <returns>構文解析した結果</returns>
		public static Block Parse(Token token, string filename) {
			Contract.Requires<ArgumentNullException>(token != null);
			Contract.Requires<ArgumentNullException>(filename != null);
			Contract.Ensures(Contract.Result<Block>() != null);

			var p = new Parser(filename);
			var pair = p.ParseBlock(token);

			if (pair.Token is NullToken) {
				return pair.Node;
			}
			else {
				throw Error("プログラムを最後まで読み取ることができませんでした。", filename, pair.Token);
			}
		}

		private readonly string _FileName;

		private Parser(string filename) {
			Contract.Requires<ArgumentNullException>(filename != null);
			_FileName = filename;
		}

		#region Parse

		private IPair<Block> ParseBlock(Token token) {
			List<Statement> stmtList = new List<Statement>();
			var startLoc = token.Range.Start;
			while (true) {
				var pair = ParseIStatement(token);
				if (pair == null)
					break;
				token = pair.Token;
				stmtList.Add(pair.Node);
			}
			var range = new TextRange(startLoc, token.Range.Start);
			return MakePair(new Block(stmtList, range), token);
		}

		private IPair<Statement> ParseIStatement(Token token) {
			return
				ParseIfStatement(token) ??
				ParseDefun(token) ??
				ParseINormalStatement(token) as IPair<Statement>;
		}

		#region もし文

		private IPair<IfStatement> ParseIfStatement(Token token) {
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
			if (thens.Count == 0)
				throw Error("「もし」の後ろがありません。", token);
			return MakePair(new IfStatement(thens), token);
		}

		private IPair<CondThenPair> ParseCondThenPair(Token token) {
			var condPair = ParseProperty(token);
			if (condPair == null)
				return null;
			token = condPair.Token
				.MatchFlow((ReservedToken t) => t.Value == "なら");
			if (token == null)
				return null;
			token = token.MatchFlow((CommaToken t) => true) ?? token; // 読点が存在していれば読み飛ばす。
			var thenPair = ParseINormalStatement(token);
			if (thenPair == null)
				throw Error("「なら」の後ろが正しく解析できません。", token);

			return MakePair(new CondThenPair(condPair.Node, thenPair.Node), thenPair.Token);
		}

		#endregion

		private IPair<NormalStatement> ParseINormalStatement(Token token) {
			return
				ParseExprBlock(token) ??
				ParseBlockExecute(token) ??
				ParseReturn(token) ??
				ParsePhraseChain(token) as IPair<NormalStatement>;
		}

		#region 関数定義

		private IPair<Defun> ParseDefun(Token token) {
			var keywordSkipped = token
				.MatchFlow((ReservedToken t) => t.Value == ConstantNames.BlockBegin)
				.MatchFlow((SuffixToken t) => t.Value == "の")
				.MatchFlow((ReservedToken t) => t.Value == ConstantNames.Defun)
				.MatchFlow((SuffixToken t) => t.Value == "で");
			if (keywordSkipped == null)
				return null;
			token = keywordSkipped.MatchFlow((CommaToken t) => true) ?? keywordSkipped;
			var paramList = new List<ParamSuffixPair>();
			while (true) {
				var paramPair = ParseParamSuffixPair(token);
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
				throw Error("正しく関数が定義出来ていません。", token);
			var name = ((SymbolToken)token).Value;
			var blockPair = ParseBlock(blockToken);
			var lastToken = blockPair.Token
				.MatchFlow((ReservedToken t) => t.Value == ConstantNames.BlockEnd)
				.MatchFlow((PeriodToken t) => true);
			if (lastToken == null)
				throw Error("関数のブロックが正しく閉じられていません。", blockPair.Token);
			return MakePair(new Defun(name, paramList, blockPair.Node), lastToken);
		}

		private IPair<ParamSuffixPair> ParseParamSuffixPair(Token token) {
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

		private IPair<BlockExecute> ParseBlockExecute(Token token) {
			var blockToken = token
				.MatchFlow((ReservedToken t) => t.Value == ConstantNames.BlockBegin)
				.MatchFlow((SuffixToken t) => t.Value == "を")
				.MatchFlow((ReservedToken t) => t.Value == ConstantNames.BlockExec)
				.MatchFlow((ReservedToken t) => t.Value == "する")
				.MatchFlow((PeriodToken t) => true);
			if (blockToken == null)
				return null;
			var blockPair = ParseBlock(blockToken);
			var lastToken = blockPair.Token
				.MatchFlow((ReservedToken t) => t.Value == ConstantNames.BlockEnd)
				.MatchFlow((PeriodToken t) => true);
			if (lastToken == null)
				throw Error("関数が正しく閉じられていません。", blockPair.Token);
			return MakePair(new BlockExecute(blockPair.Node), lastToken);
		}

		private IPair<Return> ParseReturn(Token token) {
			var elemPair = ParseProperty(token);
			if (elemPair == null)
				return null;
			token = elemPair.Token;
			var lst = new List<ArgumentTuple>();
			while (token.Match((SuffixToken t) => t.Value == "と")) {
				lst.Add(new ArgumentTuple(elemPair.Node, "と"));
				elemPair = ParseProperty(token.Next);
				if (elemPair == null)
					break;
				token = elemPair.Token;
			}
			Element retValue = null;
			if (lst.Count == 0) {
				retValue = elemPair.Node;
			}
			else {
				lst.Add(new ArgumentTuple(elemPair.Node, "で"));
				retValue = CreateTuple(lst).Argument;
			}
			var nextToken = elemPair.Token
				.MatchFlow((ReservedToken t) => t.Value == ConstantNames.ReturnText)
				.MatchFlow((PeriodToken t) => true);
			if (nextToken == null)
				return null;
			return MakePair(new Return(retValue), nextToken);
		}

		#region PhraseChain

		private IPair<PhraseChain> ParsePhraseChain(Token token) {
			var list = new List<Phrase>();
			bool isFirst = true;
			while (true) {
				var pair = ParsePhrase(token, isFirst);
				isFirst = false;
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

		private IPair<Phrase> ParsePhrase(Token token, bool isFirst) {
			var startLoc = token.Range.Start;
			var lst = new List<ArgumentTuple>();
			bool isMap = false;
			ArgumentTuple mappedArg = null;
			while (true) {
				if (token.Match((ReservedToken t) => t.Value == "それぞれ")) {
					if (isMap)
						throw Error("「それぞれ」を二箇所で使うことはできません。", token);
					isMap = true;
					if (lst.Count == 1)
						mappedArg = lst[0];
					else if (lst.Count > 1) {
						var tmpArg = CreateTuple(lst);
						if (tmpArg != null) {
							mappedArg = tmpArg;
						}
						else {
							throw Error("「それぞれ」に対して、二つ以上の引数を与えることはできません。", token);
						}
					}
					lst.Clear();
					token = token.Next;
				}
				var argPair = ParseArgSfxPair(token);
				if (argPair == null)
					break;
				lst.Add(argPair.Node);
				token = argPair.Token;
				if (token.Match((SuffixToken t) => true)) {
					var sfx = ((SuffixToken)token).Value;
					var range = new TextRange(token.Range.End, token.Range.End);
					lst.Add(new ArgumentTuple(new NullLiteral(range), sfx));
					token = token.Next;
				}
			}

			if (token.Match((ReservedToken t) => t.Value == "し" || t.Value == "する")) {
				var range = new TextRange(startLoc, token.Range.End);
				var dfn = CreateDefine(lst, range);
				if (dfn == null)
					throw Error("引数が正しくありません。", token);
				return MakePair(dfn, token.Next);
			}
			var execTarget = ParseBinaryExpr(token);
			if (execTarget == null)
				return null;
			var execToken = execTarget.Token;
			var last = execToken
				.MatchFlow((ReservedToken t) => t.Value == "し" || t.Value == "する");
			bool isMaybe = false;
			if (last == null) {
				last = execToken
					.MatchFlow((ReservedToken t) => t.Value == "してみ" || t.Value == "してみて");
				if (last == null)
					return null;
				isMaybe = true;
			}
			if (last != null) {
				var range = new TextRange(startLoc, execToken.Range.End);
				var sym = token as SymbolToken;
				if (sym != null && sym.Value == ConstantNames.Assign) {
					var assign = CreateAssign(lst, isFirst, isMaybe, range);
					if (assign == null)
						return null;
					return MakePair(assign, last);
				}
				var verb = execTarget.Node;
				if (isMap)
					return MakePair(new MapCall(verb, mappedArg, lst, isMaybe, range), last);
				else
					return MakePair(new Call(verb, lst, isMaybe, range), last);
			}
			return null;
		}

		private DefineValue CreateDefine(List<ArgumentTuple> args, TextRange range) {
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
				return new DefineValue(name, null, range);
			if (args.Last().Suffix != "を")
				return null;
			var tuple = CreateTuple(args).Argument;
			if (tuple == null)
				return null;
			return new DefineValue(name, tuple, range);
		}

		private Phrase CreateAssign(List<ArgumentTuple> lst, bool isFirst, bool isMaybe, TextRange range) {
			if (lst.Count == 0)
				return null;
			Func<ArgumentTuple, string> getName = pair => {
				if (pair.Suffix != "に")
					return null;
				var sym = pair.Argument as Symbol;
				if (sym == null)
					return null;
				return sym.Name;
			};
			Func<ArgumentTuple, PropertyAccess> getProp = pair => {
				if (pair.Suffix != "に")
					return null;
				return pair.Argument as PropertyAccess;
			};
			var name = getName(lst[lst.Count - 1]);
			var prop = getProp(lst[lst.Count - 1]);
			if (name != null || prop != null) {
				if (isFirst == false && lst.Count == 1) {
					if (name != null)
						return new Assign(name, null,isMaybe, range);
					else
						return new PropertySet(prop, null,isMaybe, range);
				}

				lst.RemoveAt(lst.Count - 1);
				var value = CreateTuple(lst).Argument;
				if (value == null)
					return null;
				if (name != null)
					return new Assign(name, value,isMaybe, range);
				else
					return new PropertySet(prop, value, isMaybe,range);
			}
			name = getName(lst[0]);
			prop = getProp(lst[0]);
			if (name != null || prop != null) {
				lst.RemoveAt(0);
				var value = CreateTuple(lst).Argument;
				if (value == null)
					return null;
				if (name != null)
					return new Assign(name, value,isMaybe, range);
				else
					return new PropertySet(prop, value,isMaybe, range);
			}
			return null;
		}

		private ArgumentTuple CreateTuple(List<ArgumentTuple> args) {
			var last = args.Last();
			var tuple = last.Argument;
			var sfx = last.Suffix;
			var endLoc = last.Argument.Range.End;
			for (int i = args.Count - 2; i >= 0; i--) {
				var elem = args[i];
				if (elem.Suffix != "と")
					return null;
				var range = new TextRange(elem.Argument.Range.Start, endLoc);
				tuple = new TupleLiteral(elem.Argument, tuple, range);
			}
			return new ArgumentTuple(tuple, sfx);
		}

		private IPair<ArgumentTuple> ParseArgSfxPair(Token token) {
			var elemPair = ParseProperty(token);
			if (elemPair == null)
				return null;
			var sfxToken = elemPair.Token as SuffixToken;
			if (sfxToken == null)
				return null;
			return MakePair(new ArgumentTuple(elemPair.Node, sfxToken.Value), sfxToken.Next);
		}

		#endregion

		private IPair<ExprBlock> ParseExprBlock(Token token) {
			token = token.MatchFlow((OpenBraceToken t) => true);
			if (token == null)
				return null;
			var list = new List<IExpr>();
			while (true) {
				var exprPair = ParseExpr(token);
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

		private IPair<IExpr> ParseExpr(Token token) {
			return
				ParseExprBlock(token) ??
				ParseProperty(token) as IPair<IExpr>;
		}

		#region 要素

		private IPair<Element> ParseProperty(Token token) {
			IPair<Element> pair = ParseUnit(token);
			if (pair == null)
				return null;
			var startLoc = token.Range.Start;
			while (true) {
				var nextToken = pair.Token
					.MatchFlow((SuffixToken t) => t.Value == "の")
					.MatchFlow((SymbolToken t) => true);
				if (nextToken == null)
					break;
				var propToken = (SymbolToken)pair.Token.Next;
				var propName = propToken.Value;
				var range = new TextRange(startLoc, propToken.Range.End);
				pair = MakePair(new PropertyAccess(pair.Node, propName, range), nextToken);
			}
			return pair;
		}

		private IPair<Element> ParseUnit(Token token) {
			return
				ParseParenthesisExpr(token) ??
				ParseList(token) ??
				ParseSumiBracketExpr(token) ??
				ParseSymbol(token) ??
				ParseLiteral(token) as IPair<Element>;
		}

		#region BinaryExpr

		/// consExpr ::= concatExpr | concatExpr OrOp  consExpr
		/// concatExpr ::= orExpr | concatExpr OrOp  concatExpr
		/// orExpr  ::= andExpr | binExpr OrOp  andExpr
		/// andExpr ::= cmpExpr |  orExpr AndOp cmpExpr
		/// cmpExpr ::= addExpr | cmpExpr CmpOp addExpr
		/// addExpr ::= mltExpr | addExpr AddOp mltExpr
		/// mltExpr ::= ukExpr  | mltExpr MltOp unExpr
		/// unExpr  ::= unary   |  unExpr UnknownOp unary
		private IPair<Element> ParseBinaryExpr(Token token) {
			// concatExprのみ右優先結合であることに注意すること
			var startLoc = token.Range.Start;
			IPair<Element> pair = ParseConcatExpr(token);
			if (pair == null)
				return null;
			token = pair.Token;
			if (token is ConsOpToken) {
				var opToken = (ConsOpToken)token;
				var rightPair = ParseBinaryExpr(token.Next);
				if (rightPair == null)
					throw Error("右辺が見つかりません。", token.Next);
				var range = new TextRange(startLoc, rightPair.Token.Range.End);
				pair = MakePair(new BinaryExpr(pair.Node, BinaryOperationType.Cons, rightPair.Node, range), rightPair.Token);
			}
			return pair;
		}

		private IPair<Element> ParseConcatExpr(Token token) {
			var startLoc = token.Range.Start;
			IPair<Element> pair = ParseOrExpr(token);
			if (pair == null)
				return null;
			while (true) {
				token = pair.Token;
				if (token is ConcatOpToken) {
					var opToken = (ConcatOpToken)token;
					var rightPair = ParseOrExpr(token.Next);
					if (rightPair == null)
						throw Error("右辺が見つかりません。", token.Next);
					var range = new TextRange(startLoc, rightPair.Token.Range.End);
					pair = MakePair(new BinaryExpr(pair.Node, BinaryOperationType.Concat, rightPair.Node, range), rightPair.Token);
				}
				else
					break;
			}
			return pair;
		}

		private IPair<Element> ParseOrExpr(Token token) {
			var startLoc = token.Range.Start;
			IPair<Element> pair = ParseAndExpr(token);
			if (pair == null)
				return null;
			while (true) {
				token = pair.Token;
				if (token is OrOpToken) {
					var opToken = (OrOpToken)token;
					var rightPair = ParseAndExpr(token.Next);
					if (rightPair == null)
						throw Error("右辺が見つかりません。", token.Next);
					var range = new TextRange(startLoc, rightPair.Token.Range.End);
					pair = MakePair(new BinaryExpr(pair.Node, BinaryOperationType.Or, rightPair.Node, range), rightPair.Token);
				}
				else
					break;
			}
			return pair;
		}

		private IPair<Element> ParseAndExpr(Token token) {
			IPair<Element> pair = ParseCompareExpr(token);
			if (pair == null)
				return null;
			var startLoc = token.Range.Start;
			while (true) {
				token = pair.Token;
				if (token is AndOpToken) {
					var opToken = (AndOpToken)token;
					var rightPair = ParseCompareExpr(token.Next);
					if (rightPair == null)
						throw Error("右辺が見つかりません。", token.Next);
					var range = new TextRange(startLoc, rightPair.Token.Range.End);
					pair = MakePair(new BinaryExpr(pair.Node, BinaryOperationType.And, rightPair.Node, range), rightPair.Token);
				}
				else
					break;
			}
			return pair;
		}

		private IPair<Element> ParseCompareExpr(Token token) {
			IPair<Element> pair = ParseAddExpr(token);
			if (pair == null)
				return null;
			var startLoc = token.Range.Start;
			while (true) {
				token = pair.Token;
				var opType =
					token is EqualOpToken ? BinaryOperationType.Equal :
					token is NotEqualOpToken ? BinaryOperationType.NotEqual :
					token is LessThanOpToken ? BinaryOperationType.LessThan :
					token is GreaterThanOpToken ? BinaryOperationType.GreaterThan :
					token is LessThanEqualOpToken ? BinaryOperationType.LessThanOrEqual :
					token is GreaterThanEqualOpToken ? BinaryOperationType.GreaterThanOrEqual :
					BinaryOperationType.Unknown;

				if (opType != BinaryOperationType.Unknown) {
					var rightPair = ParseAddExpr(token.Next);
					if (rightPair == null)
						throw Error("右辺が見つかりません。", token.Next);
					var range = new TextRange(startLoc, rightPair.Token.Range.End);
					pair = MakePair(new BinaryExpr(pair.Node, opType, rightPair.Node, range), rightPair.Token);
				}
				else
					break;
			}
			return pair;
		}

		private IPair<Element> ParseAddExpr(Token token) {
			IPair<Element> pair = ParseMultipleExpr(token);
			if (pair == null)
				return null;
			var startLoc = token.Range.Start;
			while (true) {
				token = pair.Token;
				var opType =
					token is AddOpToken ? BinaryOperationType.Add :
					token is SubOpToken ? BinaryOperationType.Subtract :
					BinaryOperationType.Unknown;

				if (opType != BinaryOperationType.Unknown) {
					var rightPair = ParseMultipleExpr(token.Next);
					if (rightPair == null)
						throw Error("右辺が見つかりません。", token.Next);
					var range = new TextRange(startLoc, rightPair.Token.Range.End);
					pair = MakePair(new BinaryExpr(pair.Node, opType, rightPair.Node, range), rightPair.Token);
				}
				else
					break;
			}
			return pair;
		}

		private IPair<Element> ParseMultipleExpr(Token token) {
			IPair<Element> pair = ParseUnknownMultipleExpr(token);
			if (pair == null)
				return null;
			var startLoc = token.Range.Start;
			while (true) {
				token = pair.Token;
				var opType =
					token is MultipleOpToken ? BinaryOperationType.Multiply :
					token is DivideOpToken ? BinaryOperationType.Divide :
					token is ModuloOpToken ? BinaryOperationType.Modulo :
					BinaryOperationType.Unknown;

				if (opType != BinaryOperationType.Unknown) {
					var rightPair = ParseUnknownMultipleExpr(token.Next);
					if (rightPair == null)
						throw Error("右辺が見つかりません。", token.Next);
					var range = new TextRange(startLoc, rightPair.Token.Range.End);
					pair = MakePair(new BinaryExpr(pair.Node, opType, rightPair.Node, range), rightPair.Token);
				}
				else
					break;
			}
			return pair;
		}

		private IPair<Element> ParseUnknownMultipleExpr(Token token) {
			IPair<Element> pair = ParseUnaryExpr(token);
			if (pair == null)
				return null;
			var startLoc = token.Range.Start;
			while (true) {
				token = pair.Token;
				if (token is UnknownOperatorToken) {
					var rightPair = ParseUnaryExpr(token.Next);
					if (rightPair == null)
						throw Error("右辺が見つかりません。", token.Next);
					var range = new TextRange(startLoc, rightPair.Token.Range.End);
					pair = MakePair(new BinaryExpr(pair.Node, BinaryOperationType.Unknown, rightPair.Node, range), rightPair.Token);
				}
				else
					break;
			}
			return pair;
		}

		#endregion

		#region UnaryExpr

		private IPair<Element> ParseUnaryExpr(Token token) {
			UnaryOperationType? op = null;
			var startLoc = token.Range.Start;
			if (token.Match((AbstractOperatorToken t) => true)) {
				var opType =
					token is SubOpToken ? UnaryOperationType.Negate :
					token is NotOpToken ? UnaryOperationType.Not :
					UnaryOperationType.Unknown;
				op = opType;
				token = token.Next;
			}

			var propPair = ParseProperty(token);
			if (op == null)
				return propPair;
			var range = new TextRange(startLoc, propPair.Node.Range.End);
			return MakePair(new UnaryExpr(op.Value, propPair.Node, range), propPair.Token);
		}

		#endregion

		#region StartWithBracket

		private IPair<Element> ParseParenthesisExpr(Token token) {
			token = token.MatchFlow((OpenParenthesisToken t) => true);
			if (token == null)
				return null;
			var elemPair = ParseBinaryExpr(token);
			var lastToken = elemPair.Token
				.MatchFlow((CloseParenthesisToken t) => true);
			if (lastToken == null)
				throw Error("閉じ括弧が出現していません。", elemPair.Token);
			return MakePair(elemPair.Node, lastToken);
		}

		private IPair<Element> ParseSumiBracketExpr(Token token) {
			var bodyToken = token.MatchFlow((OpenSumiBracketToken t) => true);
			if (bodyToken == null)
				return null;
			var elemPair = ParseBinaryExpr(bodyToken);
			var lastToken = elemPair.Token
				.MatchFlow((CloseSumiBracketToken t) => true);
			if (lastToken == null)
				throw Error("閉じ括弧が出現していません。", elemPair.Token);
			var range = TextRange.IncludeRange(token.Range, elemPair.Token.Range);
			return MakePair(new Lambda(elemPair.Node, range), lastToken);
		}

		private IPair<ListLiteral> ParseList(Token token) {
			var startLoc = token.Range.Start;
			token = token.MatchFlow((OpenBracketToken t) => true);
			if (token == null)
				return null;
			var elemList = new List<Element>();
			while (true) {
				if (token.Match((CloseBracketToken t) => true))
					break;
				var elemPair = ParseBinaryExpr(token);
				if (elemPair == null)
					throw Error("リストの要素が解析できません。", token);

				elemList.Add(elemPair.Node);
				token = elemPair.Token;

				if (token.Match((CommaToken t) => true)) {
					token = token.Next;
					continue;
				}
				if (token.Match((CloseBracketToken t) => true))
					break;
				throw Error("リストの要素が解析できません。", token);
			}
			var range = new TextRange(startLoc, token.Range.End);
			return MakePair(new ListLiteral(elemList, range), token.Next);
		}

		#endregion

		#region Single

		private IPair<Symbol> ParseSymbol(Token token) {
			var symToken = token as SymbolToken;
			if (symToken == null)
				return null;
			else
				return MakePair(new Symbol(symToken.Value, token.Range), token.Next);
		}

		private IPair<Literal> ParseLiteral(Token token) {
			var nextToken = token.Next;
			if (token is LiteralToken) {
				var value = ((LiteralToken)token).Value;
				return MakePair(new StringLiteral(value, token.Range), nextToken);
			}
			if (token is IntegerToken) {
				int value = ((IntegerToken)token).IntValue;
				return MakePair(new IntLiteral(value, token.Range), nextToken);
			}
			if (token is DecimalToken) {
				double value = ((DecimalToken)token).DecimalValue;
				return MakePair(new FloatLiteral(value, token.Range), nextToken);
			}
			if (token is LambdaSpaceToken) {
				return MakePair(new LambdaParameter(((LambdaSpaceToken)token).Value, token.Range), nextToken);
			}

			if (token.Match((ReservedToken t) => t.Value == ConstantNames.NullText))
				return MakePair(new NullLiteral(token.Range), nextToken);

			if (token.Match((ReservedToken t) => t.Value == ConstantNames.ElseText))
				return MakePair(new BoolLiteral(true, token.Range), nextToken);

			if (token.Match((ReservedToken t) => t.Value == ConstantNames.TrueText))
				return MakePair(new BoolLiteral(true, token.Range), nextToken);

			if (token.Match((ReservedToken t) => t.Value == ConstantNames.FalseText))
				return MakePair(new BoolLiteral(false, token.Range), nextToken);
			return null;
		}

		#endregion

		#endregion

		#endregion

		#region Util

		private SyntaxException Error(string message, Token token) {
			return Error(message, _FileName, token);
		}

		private static SyntaxException Error(string message, string filename, Token token) {
			return new SyntaxException(message, filename, token.Range.Start);
		}

		private static IPair<T> MakePair<T>(T node, Token token) where T : class {
			return new Pair<T>(node, token);
		}

		private interface IPair<out T> {
			T Node { get; }
			Token Token { get; }
		}

		private class Pair<T> : IPair<T> where T : class {
			public T Node { get; private set; }
			public Token Token { get; private set; }

			public Pair(T node, Token token) {
				Contract.Requires<ArgumentNullException>(node != null);
				Contract.Requires<ArgumentNullException>(token != null);
				this.Node = node;
				this.Token = token;
			}
		}

		#endregion

	}
}
