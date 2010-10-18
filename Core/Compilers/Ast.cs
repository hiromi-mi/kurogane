using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Kurogane.Compiler {

	#region 文

	/// <summary>文</summary>
	public interface IStatement { }

	/// <summary>
	/// If文の中身になれる文
	/// </summary>
	public interface INormalStatement : IStatement { }

	#region もし文

	/// <summary>If/Caseをまとめる文</summary>
	public class IfStatement : IStatement {
		public readonly IList<CondThenPair> Thens;

		public IfStatement(IList<CondThenPair> thens) {
			this.Thens = thens;
		}
	}

	/// <summary>If/Case文の中身</summary>
	public class CondThenPair {
		public readonly Element Condition;
		public readonly INormalStatement Statement;

		public CondThenPair(Element cond, INormalStatement stmt) {
			this.Condition = cond;
			this.Statement = stmt;
		}
	}

	#endregion

	#region 関数定義

	/// <summary>
	/// 関数定義
	/// </summary>
	public class Defun : INormalStatement {
		public readonly string Name;
		public readonly IList<ParamSuffixPair> Params;
		public readonly Block Block;

		public Defun(string name, IList<ParamSuffixPair> parameters, Block block) {
			this.Name = name;
			this.Params = parameters;
			this.Block = block;
		}
	}

	public class ParamSuffixPair {
		public readonly string Name;
		public readonly string Suffix;

		public ParamSuffixPair(string name, string sfx) {
			this.Name = name;
			this.Suffix = sfx;
		}
	}


	#endregion

	public class BlockExecute : INormalStatement {
		public readonly Block Block;

		public BlockExecute(Block block) {
			this.Block = block;
		}
	}

	public class ExprBlock : INormalStatement, IExpr {
		public readonly IList<IExpr> Exprs;

		public ExprBlock(IList<IExpr> exprs) {
			this.Exprs = exprs;
		}
	}

	public class PhraseChain : INormalStatement {
		public readonly IList<IPhrase> Phrases;

		public PhraseChain(IList<IPhrase> phrases) {
			this.Phrases = phrases;
		}
	}

	#endregion

	#region 句

	public interface IPhrase { }

	#region 関数呼び出し

	public class Call : IPhrase {
		public readonly string Name;
		public readonly IList<ArgSuffixPair> Arguments;
		public readonly bool IsMaybe;

		public Call(string name, IList<ArgSuffixPair> args, bool isMaybe) {
			this.Name = name;
			this.Arguments = args;
			this.IsMaybe = isMaybe;
		}
	}

	public class MapCall : Call {
		public readonly ArgSuffixPair FirstArg;

		public MapCall(string name, ArgSuffixPair firstArg, IList<ArgSuffixPair> args, bool isMaybe)
			: base(name, args, isMaybe) {
			this.FirstArg = firstArg;
		}
	}

	public class ArgSuffixPair {
		public readonly Element Argument;
		public readonly string Suffix;

		public ArgSuffixPair(Element arg, string sfx) {
			this.Argument = arg;
			this.Suffix = sfx;
		}
	}

	#endregion

	public class Assign : IPhrase {
		public string Name;
		public Element Value;

		public Assign(string name, Element value) {
			this.Name = name;
			this.Value = value;
		}
	}

	public class DefineValue : IPhrase {
		public string Name;
		public Element Value;
		public DefineValue(string name, Element value) {
			this.Name = name;
			this.Value = value;
		}
	}

	public class Return : IPhrase {
		public Element Value;

		public Return(Element value) {
			this.Value = value;
		}
	}

	public class Block {
		public readonly IList<IStatement> Statements;

		public Block(IList<IStatement> stmts) {
			this.Statements = stmts;
		}
	}

	#endregion

	#region 要素

	public interface IExpr { }

	/// <summary>要素</summary>
	public interface Element : IExpr {

	}

	#region リテラル値

	public interface Literal : Element { }

	public class ListLiteral : Literal {
		public readonly IList<Element> Elements;

		public ListLiteral(IList<Element> elems) {
			this.Elements = elems;
		}
	}

	public class TupleLiteral : Literal {
		public readonly Element Head;
		public readonly Element Tail;

		public TupleLiteral(Element head, Element tail) {
			this.Head = head;
			this.Tail = tail;
		}
	}

	public class StringLiteral : Literal {
		public readonly string Value;

		public StringLiteral(string value) {
			this.Value = value;

		}
	}

	public class IntLiteral : Literal {
		public readonly int Value;

		public IntLiteral(int value) {
			this.Value = value;
		}
	}

	public class FloatLiteral : Literal {
		public readonly double Value;

		public FloatLiteral(double value) {
			this.Value = value;
		}
	}

	public sealed class BoolLiteral : Literal {
		public static readonly BoolLiteral True = new BoolLiteral(true);
		public static readonly BoolLiteral False = new BoolLiteral(false);

		public readonly bool Value;

		private BoolLiteral(bool value) {
			this.Value = value;
		}
	}

	public sealed class NullLiteral : Literal {
		public static readonly NullLiteral Instant = new NullLiteral();

		private NullLiteral() { }
	}

	#endregion

	#region 式

	public class BinaryExpr : Element {
		public readonly Element Left;
		public readonly Element Right;
		public readonly string Operation;

		public BinaryExpr(Element left, string op, Element right) {
			this.Left = left;
			this.Right = right;
			this.Operation = op;
		}
	}

	public class UnaryExpr : Element {
		public readonly string Operation;
		public readonly Element Value;

		public UnaryExpr(string op, Element value) {
			this.Operation = op;
			this.Value = value;
		}
	}

	public class FuncCall : Element {
		public readonly string Name;
		public readonly IList<Element> Arguments;

		public FuncCall(string name, IList<Element> args) {
			this.Name = name;
			this.Arguments = args;
		}
	}

	public class PropertyAccess : Element {
		public readonly Element Value;
		public readonly string Name;

		public PropertyAccess(Element value, string name) {
			this.Value = value;
			this.Name = name;
		}
	}

	public class Symbol : Element {
		public readonly string Name;

		public Symbol(string name) {
			this.Name = name;
		}
	}

	#endregion

	#endregion

}
