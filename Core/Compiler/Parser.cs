using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace Kurogane.Compiler {
	public class Parser {

		/// <summary>
		/// 引数をパースして抽象構文木を返す。
		/// </summary>
		/// <returns></returns>
		public static ProgramNode Parse(Token token) {
			var parser = new Parser();
			var pair = parser.ParseProgram(token);
			if (pair == null) Throw("プログラムが正しく読めませんでした。");
			if (pair.Token != null) Throw("プログラムに読み残しがあります。");
			return pair.Node;
		}

		private IPair<ProgramNode> ParseProgram(Token token) {
			var result = new ProgramNode();
			while (true) {
				var pair = ParseStatement(token);
				if (pair == null) break;
				result.Statements.Add(pair.Node);
				token = pair.Token;
			}
			return MakePair(result, token);
		}

		#region 文

		private IPair<AbstractStatementNode> ParseStatement(Token token) {
			IPair<AbstractStatementNode> pair = null;
			pair = ParseIf(token);
			if (pair != null) return pair;
			pair = ParseDefun(token);
			if (pair != null) return pair;
			pair = ParseNomalStmt(token);
			if (pair != null) return pair;
			return null;
		}

		#region もし文

		private IPair<IfNode> ParseIf(Token token) {
			if (!(token is ReservedToken)) return null;
			if (((ReservedToken)token).Value != "もし") return null;
			token = token.Next;
			var ifNode = new IfNode();
			while (true) {
				var pair = ParseThen(token);
				if (pair == null) break;
				ifNode.Thens.Add(pair.Node);
				token = pair.Token;
			}
			if (ifNode.Thens.Count == 0) Throw("もし文の中身がありません。");
			return MakePair(ifNode, token);
		}

		private IPair<ThenNode> ParseThen(Token token) {
			var condPair = ParseCond(token);
			if (condPair == null) return null;
			var thenToken = condPair.Token;
			if (!(thenToken is ReservedToken)) return null;
			if (((ReservedToken)thenToken).Value != "なら") return null;
			var stmtPair = ParseNomalStmt(thenToken.Next);
			if (stmtPair == null) Throw("「なら」の後が見つかりません。");
			return MakePair(new ThenNode(condPair.Node, stmtPair.Node), stmtPair.Token);
		}

		private IPair<ConditionNode> ParseCond(Token token) {
			if (token is ReservedToken && ((ReservedToken)token).Value == "他") {
				return MakePair(new ElseConditionNode(), token.Next);
			}
			if (token is OpenParenthesisToken) {
				var pair = ParseExpression(token);
				if (pair != null) {
					return MakePair(new ExpressionConditionNode(pair.Node), pair.Token);
				}
			}
			// TODO: 「AがBなら」のような文もパース出来るようにする。
			return null;
		}

		#endregion

		#region 関数定義

		private IPair<DefunNode> ParseDefun(Token token) {
			var next = token
				.MatchFlow((ReservedToken t) => t.Value == "以下")
				.MatchFlow((PostPositionToken t) => t.Value == "の")
				.MatchFlow((ReservedToken t) => t.Value == "手順")
				.MatchFlow((PostPositionToken t) => t.Value == "で");
			if (next == null) return null;
			var decPair = ParseFuncDeclare(next);
			if (decPair == null) Throw("関数定義がありません。");
			var bodyPair = ParseBlock(decPair.Token);
			if (bodyPair == null) Throw("関数の本体がありません。");
			var endDefun = bodyPair.Token
				.MatchFlow((ReservedToken t) => t.Value == "以上")
				.MatchFlow((PunctuationToken t) => t.Value == "。" || t.Value == "．");
			if (endDefun == null) Throw("関数が閉じられていません。");
			return MakePair(new DefunNode(decPair.Node, bodyPair.Node), endDefun);
		}

		private IPair<FuncDeclareNode> ParseFuncDeclare(Token token) {
			// 「AをBにCする。」を読みとる
			List<ParamPair> lst = new List<ParamPair>();
			while (true) {
				var paramPair = ParseParam(token);
				if (paramPair == null) break;
				lst.Add(paramPair.Node);
				token = paramPair.Token;
			}
			var next = token
				.MatchFlow((SymbolToken t) => true)
				.MatchFlow((ReservedToken t) => t.Value == "する")
				.MatchFlow((PunctuationToken t) => t.Value == "。" || t.Value == "．");
			if (next == null) return null;
			var funcName = ((SymbolToken)token).Value;
			return MakePair(new FuncDeclareNode(lst, funcName), next);
		}

		private IPair<ParamPair> ParseParam(Token token) {
			var namePair = ParseRef(token);
			if (namePair == null) return null;
			if (!(namePair.Token is PostPositionToken)) return null;
			var ppToken = (PostPositionToken)namePair.Token;
			var pp = ppToken.Value;
			if (pp != "と") {
				return MakePair(new ParamPair(new NormalParam(namePair.Node.Name), pp), ppToken.Next);
			}
			else {
				var pair = ParseParam(ppToken.Next);
				return MakePair(new ParamPair(
					new PairParam(namePair.Node.Name, pair.Node.Param),
						pair.Node.PostPosition),
					pair.Token);
			}
		}

		private IPair<BlockNode> ParseBlock(Token token) {
			List<AbstractStatementNode> stmts = new List<AbstractStatementNode>();
			while (true) {
				var pair = ParseStatement(token);
				if (pair == null) break;
				token = pair.Token;
				stmts.Add(pair.Node);
			}
			return MakePair(new BlockNode(stmts), token);
		}

		#endregion

		#region 通常文

		private IPair<StatementNode> ParseNomalStmt(Token token) {
			List<Procedure> procs = new List<Procedure>();
			while (true) {
				var procPair = ParseProc(token);
				if (procPair == null) break;
				if (!(procPair.Token is PunctuationToken)) break;
				var puncToken = (PunctuationToken)procPair.Token;
				procs.Add(procPair.Node);
				token = puncToken.Next;
				if (puncToken.Value == "。" || puncToken.Value == "．") break;
			}
			if (procs.Count > 0)
				return MakePair(new StatementNode(procs), token);
			else
				return null;
		}

		private IPair<Procedure> ParseProc(Token token) {
			List<ArgumentPair> args = new List<ArgumentPair>();
			Procedure proc = null;
			while (true) {
				var expPair = ParseExpression(token);
				if (expPair == null) return null;
				if (expPair.Token is PostPositionToken) {
					var ppToken = (PostPositionToken)expPair.Token;
					args.Add(new ArgumentPair(expPair.Node, ppToken.Value));
					token = ppToken.Next;
					continue;
				}
				if (expPair.Token.Match((ReservedToken t) => t.Value == "し" || t.Value == "する") && expPair.Node is ReferenceExpression) {
					proc = new Procedure(args, ((ReferenceExpression)expPair.Node).Name);
					token = expPair.Token.Next;
					break;
				}
				Throw("解析できないトークンが現れました。");
			}
			return MakePair(proc, token);
		}

		#endregion

		#endregion

		#region 式

		private IPair<ExpressionNode> ParseExpression(Token token) {
			if (token is OpenParenthesisToken) {
				var maybeOP = token.Next; // 開き括弧を飛ばす
				if (maybeOP is OperatorToken) {
					// 単項演算子
					var pair = ParseExpression(maybeOP.Next);
					if (pair == null) Throw("式が見つかりません。");
					var unary = new UnaryExpression(((OperatorToken)maybeOP).Value, pair.Node);
					if (pair.Token is CloseParenthesisToken)
						token = pair.Token.Next;
					else
						Throw("閉じ括弧がありません。");
					return MakePair(unary, token);
				}
				else {
					// 二項演算子
					var pairLeft = ParseExpression(token.Next);
					if (pairLeft == null) Throw("左辺式が見つかりません。");
					var opToken = (OperatorToken)pairLeft.Token;
					var pairRight = ParseExpression(opToken.Next);
					if (pairRight == null) Throw("右辺式が見つかりません。");
					var binary = new BinaryExpression(pairLeft.Node, opToken.Value, pairRight.Node);
					if (pairRight.Token is CloseParenthesisToken)
						token = pairRight.Token.Next;
					else
						Throw("閉じ括弧がありません。");
					return MakePair(binary, token);
				}
			}
			return ParseTuple(token);
		}

		private IPair<ExpressionNode> ParseTuple(Token token) {
			var pair = ParseProperty(token);
			if (pair != null && pair.Token is PostPositionToken && ((PostPositionToken)pair.Token).Value == "と") {
				var next = pair.Token.Next;
				var tailPair = ParseExpression(next);
				return MakePair(new TuppleExpression(pair.Node, tailPair.Node), tailPair.Token);
			}
			return pair;
		}

		private IPair<ExpressionNode> ParseProperty(Token token) {
			var pair = ParseUnit(token);
			while (pair != null && pair.Token is PostPositionToken && ((PostPositionToken)pair.Token).Value == "の") {
				var after = ParseUnit(pair.Token.Next);
				if (after == null) break;
				if (after.Node is ReferenceExpression) {
					var propExp = new PropertyExpression(pair.Node, ((ReferenceExpression)after.Node).Name);
					pair = MakePair(propExp, after.Token);
					continue;
				}
				if (after.Node is CastExpression) {
					var ixExp = new IndexerExpression(pair.Node, after.Node);
					pair = MakePair(ixExp, after.Token);
				}
			}
			return pair;
		}

		private IPair<ExpressionNode> ParseUnit(Token token) {
			var pair = ParseAtom(token);
			while (pair != null && pair.Token is SymbolToken) {
				var refPair = ParseRef(pair.Token);
				if (refPair != null) {
					var castExp = new CastExpression(pair.Node, refPair.Node.Name);
					pair = MakePair(castExp, refPair.Token);
				}
			}
			return pair;
		}

		private IPair<ExpressionNode> ParseAtom(Token token) {
			if (token is SymbolToken) {
				var refExp = new ReferenceExpression(((SymbolToken)token).Value);
				return MakePair(refExp, token.Next);
			}
			LiteralExpression literal = null;
			if (token is IntegerToken)
				literal = new LiteralExpression(((IntegerToken)token).IntValue);
			if (token is DecimalToken)
				literal = new LiteralExpression(((DecimalToken)token).DecimalValue);
			if (token is LiteralToken)
				literal = new LiteralExpression(((LiteralToken)token).Value);
			if (token is ReservedToken) {
				var resToken = (ReservedToken)token;
				if (resToken.Value == "無")
					literal = new LiteralExpression(default(object));
			}
			if (literal != null)
				return MakePair(literal, token.Next);
			return null;
		}

		private IPair<ReferenceExpression> ParseRef(Token token) {
			if (token is SymbolToken) {
				var refExp = new ReferenceExpression(((SymbolToken)token).Value);
				return MakePair(refExp, token.Next);
			}
			return null;
		}

		/// <summary>変数参照，プロパティアクセス，インデクサを読み取る。</summary>
		private IPair<ExpressionNode> ParseReference(Token token) {
			Debug.Assert(token is SymbolToken);
			var next = token.Next;
			ExpressionNode node = new ReferenceExpression(token.Value);
			while (true) {
				if (next is PostPositionToken) {
					var pp = (PostPositionToken)next;
					if (pp.Value == "の") {

					}
				}
				if (next is SymbolToken) {

				}
				break;
			}
			return MakePair(node, next);
		}

		#endregion

		#region util

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

		[DebuggerStepThrough]
		private static void Throw(string message) {
			throw new ParseException(message);
		}
		#endregion

	}

	public class ParseException : Exception {
		public ParseException() : base() { }
		public ParseException(string message) : base(message) { }
	}

}
