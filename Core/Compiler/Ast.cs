using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Kurogane.Compiler {

	/// <summary>
	/// プログラム全体を示すノード。
	/// 0以上の文(statement)からなる。
	/// </summary>
	public class ProgramNode {

		[DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
		public readonly IList<AbstractStatementNode> Statements = new List<AbstractStatementNode>();
	}

	/// <summary>
	/// 文を示すノード
	/// 抽象型。
	/// </summary>
	public abstract class AbstractStatementNode {

	}

	#region 普通の手続き

	public class AssignmentNode {
		public readonly string Name;
		public readonly ExpressionNode Value;
		public AssignmentNode(string name, ExpressionNode value) {
			this.Name = name;
			this.Value = value;
		}
	}

	/// <summary>
	/// 普通の命令文の示すノード。
	/// </summary>
	public class StatementNode : AbstractStatementNode {

		[DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
		public readonly IList<Procedure> Procedures;
		public StatementNode(IList<Procedure> procs) {
			Debug.Assert(procs != null);
			Debug.Assert(procs.Count > 0);
			this.Procedures = procs;
		}

		public override string ToString() {
			return
				(Procedures.Count > 1 ? "～し，" : "") +
				Procedures[Procedures.Count - 1];
		}
	}

	/// <summary>
	/// 文の中の一つの手続きを示すノード
	/// </summary>
	public class Procedure {

		[DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
		public readonly IList<ArgumentPair> Arguments;
		public readonly string Name;
		public Procedure(IList<ArgumentPair> args, string name) {
			this.Arguments = args;
			this.Name = name;
		}

		public override string ToString() {
			return Name + "する。";
		}
	}

	public class ArgumentPair {
		public readonly ExpressionNode Target;
		public readonly string PostPosition;
		public ArgumentPair(ExpressionNode target, string postPos) {
			this.Target = target;
			this.PostPosition = postPos;
		}
		public override string ToString() {
			return Target + PostPosition;
		}
	}

	#endregion

	#region ブロック群

	/// <summary>
	/// ブロックをそのまま実行する文を示すノード
	/// </summary>
	public class ExecuteBlockNode : AbstractStatementNode {
		public readonly BlockNode Block;
		public ExecuteBlockNode(BlockNode block) {
			this.Block = block;
		}
	}

	/// <summary>
	/// ブロックを示すノード
	/// </summary>
	public class BlockNode {
		[DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
		public readonly IList<AbstractStatementNode> Statements;
		public BlockNode(IList<AbstractStatementNode> stmts) {
			this.Statements = stmts;
		}
	}

	#endregion

	#region 関数ノード群

	/// <summary>
	/// 関数定義を示すノード
	/// </summary>
	public class DefunNode : AbstractStatementNode {
		public readonly FuncDeclareNode Declare;
		public readonly BlockNode Body;

		public DefunNode(FuncDeclareNode declare, BlockNode body) {
			this.Declare = declare;
			this.Body = body;
		}

		public override string ToString() {
			return "<<" + Declare.Name + "する>>";
		}
	}

	public class FuncDeclareNode {

		[DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
		public IList<ParamPair> Params;
		public readonly string Name;

		public FuncDeclareNode(IList<ParamPair> parameters, string name) {
			this.Params = parameters;
			this.Name = name;
		}

		public override string ToString() {
			return Name.ToString();
		}
	}

	public class ParamPair {
		public readonly Param Param;
		public readonly string PostPosition;

		public ParamPair(Param param, string postPos) {
			this.Param = param;
			this.PostPosition = postPos;
		}

		public override string ToString() {
			return Param + PostPosition;
		}
	}

	public abstract class Param { }

	public class NormalParam : Param {
		public readonly string Name;
		public NormalParam(string name) {
			this.Name = name;
		}

		public override string ToString() {
			return Name.ToString();
		}
	}

	public class PairParam : Param {
		public readonly string Head;
		public readonly Param Tail;
		public PairParam(string head, Param tail) {
			this.Head = head;
			this.Tail = tail;
		}

		public override string ToString() {
			return Head + "と" + Tail;
		}
	}

	#endregion

	#region IF文ノード群

	/// <summary>
	/// if文を示すノード
	/// </summary>
	[DebuggerDisplay("もし")]
	public class IfNode : AbstractStatementNode {

		[DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
		public readonly IList<ThenNode> Thens = new List<ThenNode>();
	}

	/// <summary>
	/// ～なら～。を示すノード
	/// </summary>
	public class ThenNode {
		public readonly ConditionNode Condition;
		public readonly StatementNode Statement;
		public ThenNode(ConditionNode cond, StatementNode stmt) {
			this.Condition = cond;
			this.Statement = stmt;
		}

		public override string ToString() {
			return Condition + "なら，" + Statement;
		}
	}

	/// <summary>
	/// 条件文で用いる，条件を示すノード
	/// </summary>
	public abstract class ConditionNode {

	}

	/// <summary>
	/// 条件を返す文の示すノード
	/// </summary>
	public class ConditionStatementNode : ConditionNode {

	}

	public class ElseConditionNode : ConditionNode {
		public override string ToString() {
			return "他";
		}
	}

	public class ExpressionConditionNode : ConditionNode {
		public readonly ExpressionNode Expression;

		public ExpressionConditionNode(ExpressionNode exp) {
			this.Expression = exp;
		}

		public override string ToString() {
			return this.Expression.ToString();
		}
	}

	#endregion

	#region Expression

	/// <summary>
	/// 式を示すノード。
	/// </summary>
	public abstract class ExpressionNode {

	}

	/// <summary>
	/// 二項演算子を示すノード
	/// </summary>
	public class BinaryExpression : ExpressionNode {
		public readonly string Operator;
		public readonly ExpressionNode Left, Right;

		public BinaryExpression(ExpressionNode left, string op, ExpressionNode right) {
			this.Operator = op;
			this.Left = left;
			this.Right = right;
		}

		public override string ToString() {
			return "(" + Left + Operator + Right + ")";
		}
	}

	/// <summary>
	/// 単項演算子を示すノード
	/// </summary>
	public class UnaryExpression : ExpressionNode {
		public readonly string Operator;
		public readonly ExpressionNode Expression;

		public UnaryExpression(string op, ExpressionNode exp) {
			this.Operator = op;
			this.Expression = exp;
		}

		public override string ToString() {
			return "(" + Operator + Expression + ")";
		}
	}

	/// <summary>
	/// 対を示すノード
	/// </summary>
	public class TuppleExpression : ExpressionNode {
		public readonly ExpressionNode Head, Tail;

		public TuppleExpression(ExpressionNode head, ExpressionNode tail) {
			this.Head = head;
			this.Tail = tail;
		}

		public override string ToString() {
			return Head + "と" + Tail;
		}
	}

	/// <summary>
	/// プロパティアクセスを示すノード
	/// </summary>
	public class PropertyExpression : ExpressionNode {
		public readonly ExpressionNode Target;
		public readonly string PropertyName;

		public PropertyExpression(ExpressionNode target, string property) {
			this.Target = target;
			this.PropertyName = property;
		}

		public override string ToString() {
			return Target + "の" + PropertyName;
		}
	}

	/// <summary>
	/// インデクスアクセスを示すノード
	/// </summary>
	public class IndexerExpression : ExpressionNode {
		public readonly ExpressionNode Target, Indexer;

		public IndexerExpression(ExpressionNode target, ExpressionNode indexer) {
			this.Target = target;
			this.Indexer = indexer;
		}
	}

	/// <summary>
	/// 変数参照を示すノード
	/// </summary>
	public class ReferenceExpression : ExpressionNode {
		public readonly string Name;

		public ReferenceExpression(string name) {
			this.Name = name;
		}

		public override string ToString() {
			return Name;
		}
	}

	/// <summary>
	/// リテラルを示すノード
	/// </summary>
	public class LiteralExpression : ExpressionNode {
		public readonly object Value;

		public LiteralExpression(object value) {
			this.Value = value;
		}

		public override string ToString() {
			if (Value is String)
				return "「" + Value + "」";
			return Value.ToString();
		}
	}

	/// <summary>
	/// 単位への変換を示すノード
	/// </summary>
	public class CastExpression : ExpressionNode {
		public readonly ExpressionNode Target;
		public readonly string Unit;

		public CastExpression(ExpressionNode target, string unit) {
			this.Target = target;
			this.Unit = unit;
		}
	}

	#endregion
}
