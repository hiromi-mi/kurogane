using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;

namespace Kurogane.Compiler {

	#region 文

	/// <summary>文</summary>
	public abstract class IStatement {
		internal IStatement() {
		}
	}

	/// <summary>
	/// If文の中身になれる文
	/// </summary>
	public abstract class INormalStatement : IStatement {
		internal INormalStatement() {
		}
	}

	#region もし文

	/// <summary>If/Caseをまとめる文</summary>
	public class IfStatement : IStatement {
		public readonly IList<CondThenPair> Thens;

		public IfStatement(IList<CondThenPair> thens) {
			Contract.Requires<ArgumentNullException>(thens != null);
			Contract.Requires<ArgumentException>(thens.Count > 0);
			this.Thens = thens;
		}

		public override string ToString() {
			return "もし～";
		}
	}

	/// <summary>If/Case文の中身</summary>
	public class CondThenPair {
		public readonly Element Condition;
		public readonly INormalStatement Statement;

		public CondThenPair(Element cond, INormalStatement stmt) {
			Contract.Requires<ArgumentNullException>(cond != null);
			Contract.Requires<ArgumentNullException>(stmt != null);
			this.Condition = cond;
			this.Statement = stmt;
		}

		public override string ToString() {
			return Condition + "なら" + Statement;
		}
	}

	#endregion

	#region 関数定義

	/// <summary>
	/// 関数定義
	/// </summary>
	public class Defun : INormalStatement {
		/// <summary>関数名</summary>
		public string Name { get; private set; }

		/// <summary>各引数名とその助詞の対</summary>
		[DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
		public IList<ParamSuffixPair> Params { get; private set; }

		/// <summary>関数の中身</summary>
		public Block Block { get; private set; }

		public Defun(string name, IList<ParamSuffixPair> parameters, Block block) {
			Contract.Requires<ArgumentNullException>(name != null);
			Contract.Requires<ArgumentNullException>(parameters != null);
			Contract.Requires<ArgumentNullException>(block != null);
			this.Name = name;
			this.Params = parameters;
			this.Block = block;
		}

		public override string ToString() {
			return "定義 : " + Name + "する";
		}
	}

	/// <summary>
	/// 仮引数と助詞の対
	/// </summary>
	public class ParamSuffixPair {
		/// <summary>引数名</summary>
		public string Name { get; private set; }
		/// <summary>助詞</summary>
		public string Suffix { get; private set; }

		public ParamSuffixPair(string name, string sfx) {
			Contract.Requires<ArgumentNullException>(name != null);
			Contract.Requires<ArgumentNullException>(sfx != null);
			this.Name = name;
			this.Suffix = sfx;
		}

		public override string ToString() {
			return Name + Suffix;
		}
	}


	#endregion

	/// <summary>
	/// ブロックをそのまま実行する文
	/// </summary>
	public class BlockExecute : INormalStatement {

		/// <summary>実行するブロック</summary>
		[DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
		public Block Block { get; private set; }

		public BlockExecute(Block block) {
			Contract.Requires<ArgumentNullException>(block != null);
			this.Block = block;
		}
	}

	public class ExprBlock : INormalStatement, IExpr {

		[DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
		public IList<IExpr> Exprs { get; private set; }

		public ExprBlock(IList<IExpr> exprs) {
			Contract.Requires<ArgumentNullException>(exprs != null);
			this.Exprs = exprs;
		}
	}

	public class Return : INormalStatement {
		public Element Value { get; private set; }

		public Return(Element value) {
			Contract.Requires<ArgumentNullException>(value != null);
			this.Value = value;
		}

		public override string ToString() {
			return "return " + Value;
		}
	}

	public class PhraseChain : INormalStatement {

		[DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
		public IList<IPhrase> Phrases { get; private set; }

		public PhraseChain(IList<IPhrase> phrases) {
			Contract.Requires<ArgumentNullException>(phrases != null);
			Contract.Requires<ArgumentException>(phrases.Count > 0);
			this.Phrases = phrases;
		}

		public override string ToString() {
			return Phrases[Phrases.Count - 1].ToString();
		}
	}

	#endregion

	#region 句

	public abstract class IPhrase {
		internal IPhrase() {
		}
	}

	#region 関数呼び出し

	/// <summary>
	/// 関数呼び出しを表すクラス。
	/// </summary>
	public class Call : IPhrase {
		/// <summary>呼び出す関数の名前</summary>
		public string Name { get; private set; }

		/// <summary>呼び出す際の実引数</summary>
		[DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
		public IList<ArgSuffixPair> Arguments { get; private set; }

		/// <summary>Maybe呼び出しを行っているかどうか</summary>
		public bool IsMaybe { get; private set; }

		public Call(string name, IList<ArgSuffixPair> args, bool isMaybe) {
			Contract.Requires<ArgumentNullException>(name != null);
			Contract.Requires<ArgumentNullException>(args != null);
			this.Name = name;
			this.Arguments = args;
			this.IsMaybe = isMaybe;
		}

		public override string ToString() {
			return Name + "する。";
		}
	}

	/// <summary>
	/// 「それぞれ」による関数呼び出しを表すクラス。
	/// </summary>
	public class MapCall : Call {

		/// <summary>
		/// 「それぞれ」の対象となる引数。
		/// これがnullになる場合、暗黙の引数が用いられていると判断すること。
		/// </summary>
		public ArgSuffixPair FirstArg { get; private set; }

		public MapCall(string name, ArgSuffixPair firstArg, IList<ArgSuffixPair> args, bool isMaybe)
			: base(name, args, isMaybe) {
			this.FirstArg = firstArg;
		}

		public override string ToString() {
			return "それぞれ" + Name + "する。";
		}
	}

	/// <summary>
	/// 実引数と助詞の対
	/// </summary>
	public class ArgSuffixPair {
		public Element Argument { get; private set; }
		public string Suffix { get; private set; }

		public ArgSuffixPair(Element arg, string sfx) {
			Contract.Requires<ArgumentNullException>(arg != null);
			Contract.Requires<ArgumentNullException>(sfx != null);
			this.Argument = arg;
			this.Suffix = sfx;
		}

		public override string ToString() {
			return Argument + Suffix;
		}
	}

	#endregion

	/// <summary>
	/// 代入文を示すクラス。
	/// </summary>
	public class Assign : IPhrase {
		/// <summary>代入の対象。</summary>
		public string Name { get; private set; }

		/// <summary>代入する引数。nullの場合、暗黙の引数を用いること。</summary>
		public Element Value { get; private set; }

		public Assign(string name, Element value) {
			Contract.Requires<ArgumentNullException>(name != null);
			this.Name = name;
			this.Value = value;
		}

		public override string ToString() {
			return Name + "←" + Value;
		}
	}

	/// <summary>
	/// プロパティへの代入を示すクラス。
	/// </summary>
	public class PropertySet : IPhrase {

		/// <summary>代入先</summary>
		public PropertyAccess Property { get; private set; }

		/// <summary>代入する引数。nullの場合、暗黙の引数を用いること。</summary>
		public Element Value { get; private set; }

		public PropertySet(PropertyAccess property, Element value) {
			Contract.Requires<ArgumentNullException>(property != null);
			this.Property = property;
			this.Value = value;
		}

		public override string ToString() {
			return Property + "←" + Value;
		}
	}

	/// <summary>
	/// 変数の束縛を表すクラス。
	/// </summary>
	public class DefineValue : IPhrase {

		/// <summary>変数名</summary>
		public string Name;

		/// <summary>値。nullなら暗黙の引数を用いること。</summary>
		public Element Value;

		public DefineValue(string name, Element value) {
			Contract.Requires<ArgumentNullException>(name != null);
			this.Name = name;
			this.Value = value;
		}

		public override string ToString() {
			return Name + " : " + Value;
		}
	}

	/// <summary>
	/// 「以下」「以上」で囲まれた、複数の文の集合。
	/// </summary>
	public class Block {
		[DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
		public readonly IList<IStatement> Statements;

		public Block(IList<IStatement> stmts) {
			Contract.Requires<ArgumentNullException>(stmts != null);
			this.Statements = stmts;
		}
	}

	#endregion

	#region 要素

	public interface IExpr {
	}

	/// <summary>要素</summary>
	public abstract class Element : IExpr {
		internal Element() {
		}
	}

	#region リテラル値

	public abstract class Literal : Element {
		internal Literal() {
		}
	}

	/// <summary>
	/// リストの各要素を示すクラス。
	/// </summary>
	public class ListLiteral : Literal {

		[DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
		public IList<Element> Elements { get; private set; }

		public ListLiteral(IList<Element> elems) {
			Contract.Requires<ArgumentNullException>(elems != null);
			Contract.Requires<ArgumentException>(Contract.ForAll(elems, e => e != null));
			this.Elements = elems;
		}

		public override string ToString() {
			return "リスト(" + Elements.Count + ")";
		}
	}

	/// <summary>
	/// 対を示すクラス。
	/// </summary>
	public class TupleLiteral : Literal {
		public Element Head { get; private set; }
		public Element Tail { get; private set; }

		public TupleLiteral(Element head, Element tail) {
			Contract.Requires<ArgumentNullException>(head != null);
			Contract.Requires<ArgumentNullException>(tail != null);
			this.Head = head;
			this.Tail = tail;
		}

		public override string ToString() {
			return "(" + Head + "." + Tail + ")";
		}
	}

	/// <summary>
	/// 文字列リテラルを示すクラス。
	/// </summary>
	public class StringLiteral : Literal {
		public string Value { get; private set; }

		public StringLiteral(string value) {
			Contract.Requires<ArgumentNullException>(value != null);
			this.Value = value;
		}

		public override string ToString() {
			return "「" + Value + "」";
		}
	}

	/// <summary>
	/// 整数リテラルを示すクラス。
	/// </summary>
	public class IntLiteral : Literal {
		public readonly int Value;

		public IntLiteral(int value) {
			this.Value = value;
		}

		public override string ToString() {
			return Value.ToString();
		}
	}

	/// <summary>
	/// 少数リテラルを示すクラス。
	/// </summary>
	public class FloatLiteral : Literal {
		public readonly double Value;

		public FloatLiteral(double value) {
			this.Value = value;
		}

		public override string ToString() {
			return Value.ToString();
		}
	}

	/// <summary>
	/// 真偽値のリテラルを示すクラス。
	/// </summary>
	public sealed class BoolLiteral : Literal {
		public static readonly BoolLiteral True = new BoolLiteral(true);
		public static readonly BoolLiteral False = new BoolLiteral(false);

		public readonly bool Value;

		private BoolLiteral(bool value) {
			this.Value = value;
		}

		public override string ToString() {
			return Value.ToString();
		}
	}

	/// <summary>
	/// Nullのリテラルを示すクラス。
	/// </summary>
	public sealed class NullLiteral : Literal {
		public static readonly NullLiteral Instance = new NullLiteral();

		private NullLiteral() { }

		public override string ToString() {
			return "null";
		}
	}

	#endregion

	#region 式

	/// <summary>
	/// 二項演算を示すクラス。
	/// </summary>
	public class BinaryExpr : Element {
		public Element Left { get; private set; }
		public Element Right { get; private set; }
		public BinaryOperationType Type { get; private set; }

		public BinaryExpr(Element left, BinaryOperationType type, Element right) {
			Contract.Requires<ArgumentNullException>(left != null);
			Contract.Requires<ArgumentNullException>(right != null);
			this.Left = left;
			this.Right = right;
			this.Type = type;
		}
	}

	/// <summary>
	/// 単項演算を示すクラス。
	/// </summary>
	public class UnaryExpr : Element {
		public UnaryOperationType Type { get; private set; }
		public Element Value { get; private set; }

		public UnaryExpr(UnaryOperationType type, Element value) {
			Contract.Requires<ArgumentNullException>(value != null);
			this.Type = type;
			this.Value = value;
		}
	}

	/// <summary>
	/// 括弧による関数呼び出しを示すクラス。
	/// </summary>
	public class FuncCall : Element {
		public string Name { get; private set; }
		public IList<Element> Arguments { get; private set; }

		public FuncCall(string name, IList<Element> args) {
			Contract.Requires<ArgumentNullException>(name != null);
			Contract.Requires<ArgumentNullException>(args != null);
			Contract.Requires<ArgumentException>(Contract.ForAll(args, e => e != null));
			this.Name = name;
			this.Arguments = args;
		}
	}

	/// <summary>
	/// プロパティへのアクセスを示すクラス。
	/// </summary>
	public class PropertyAccess : Element {
		public Element Value { get; private set; }
		public string Name { get; private set; }

		public PropertyAccess(Element value, string name) {
			Contract.Requires<ArgumentNullException>(value != null);
			Contract.Requires<ArgumentNullException>(name != null);
			this.Value = value;
			this.Name = name;
		}

		public override string ToString() {
			return Value + "の" + Name;
		}
	}

	/// <summary>
	/// 変数名などを示すクラス。
	/// </summary>
	public class Symbol : Element {
		public string Name { get; private set; }

		public Symbol(string name) {
			Contract.Requires<ArgumentNullException>(name != null);
			this.Name = name;
		}

		public override string ToString() {
			return Name;
		}
	}

	/// <summary>
	/// ラムダ式を示すクラス。
	/// </summary>
	public class Lambda : Element {
		public Element Element { get; private set; }

		public Lambda(Element elem) {
			Contract.Requires<ArgumentNullException>(elem != null);
			this.Element = elem;
		}

		public override string ToString() {
			return "【" + Element + "】";
		}
	}

	/// <summary>
	/// ラムダ式の引数を示すクラス。
	/// </summary>
	public class LambdaParameter : Literal {
		public string Name { get; private set; }

		public LambdaParameter(string name) {
			Contract.Requires<ArgumentNullException>(name != null);
			this.Name = name;
		}

		public override string ToString() {
			return Name;
		}
	}

	#endregion

	#endregion

}
