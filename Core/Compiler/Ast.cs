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
	public abstract class Statement {
		internal Statement() { }
	}

	/// <summary>
	/// If文の中身になれる文
	/// </summary>
	public abstract class NormalStatement : Statement {
		internal NormalStatement() { }
	}

	#region もし文

	/// <summary>If/Caseをまとめる文</summary>
	public class IfStatement : Statement {
		public readonly ReadOnlyCollection<CondThenPair> Thens;

		public IfStatement(IList<CondThenPair> thens) {
			Contract.Requires<ArgumentNullException>(thens != null);
			Contract.Requires<ArgumentException>(thens.Count > 0);
			Contract.Requires<ArgumentException>(Contract.ForAll(thens, then => then != null));
			this.Thens = Array.AsReadOnly(thens.ToArray());
		}

		public override string ToString() {
			return "もし～";
		}
	}

	/// <summary>If/Case文の中身</summary>
	public class CondThenPair {
		public readonly Element Condition;
		public readonly NormalStatement Statement;

		public CondThenPair(Element cond, NormalStatement stmt) {
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
	public class Defun : Statement {
		/// <summary>関数名</summary>
		public string Name { get; private set; }

		/// <summary>各引数名とその助詞の対</summary>
		[DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
		public ReadOnlyCollection<ParamSuffixPair> Params { get; private set; }

		/// <summary>関数の中身</summary>
		public Block Block { get; private set; }

		public Defun(string name, IEnumerable<ParamSuffixPair> parameters, Block block) {
			Contract.Requires<ArgumentNullException>(name != null);
			Contract.Requires<ArgumentNullException>(parameters != null);
			Contract.Requires<ArgumentNullException>(block != null);
			this.Name = name;
			this.Params = Array.AsReadOnly(parameters.ToArray());
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
	public class BlockExecute : NormalStatement {

		/// <summary>実行するブロック</summary>
		[DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
		public Block Block { get; private set; }

		public BlockExecute(Block block) {
			Contract.Requires<ArgumentNullException>(block != null);
			this.Block = block;
		}
	}

	public class ExprBlock : NormalStatement, IExpr {

		[DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
		public ReadOnlyCollection<IExpr> Exprs { get; private set; }

		public ExprBlock(IEnumerable<IExpr> exprs) {
			Contract.Requires<ArgumentNullException>(exprs != null);
			this.Exprs = Array.AsReadOnly(exprs.ToArray());
		}
	}

	public class Return : NormalStatement {
		public Element Value { get; private set; }

		public Return(Element value) {
			Contract.Requires<ArgumentNullException>(value != null);
			this.Value = value;
		}

		public override string ToString() {
			return "return " + Value;
		}
	}

	public class PhraseChain : NormalStatement {

		[DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
		public ReadOnlyCollection<Phrase> Phrases { get; private set; }

		public PhraseChain(IList<Phrase> phrases) {
			Contract.Requires<ArgumentNullException>(phrases != null);
			Contract.Requires<ArgumentException>(phrases.Count > 0);
			this.Phrases = Array.AsReadOnly(phrases.ToArray());
		}

		public override string ToString() {
			return Phrases[Phrases.Count - 1].ToString();
		}
	}

	#endregion

	#region 句

	public abstract class Phrase {

		/// <summary>Maybe呼び出しを行っているかどうか</summary>
		public bool IsMaybe { get; private set; }

		/// <summary>プログラム中の位置</summary>
		public TextRange Range { get; private set; }

		internal Phrase(bool isMaybe, TextRange range) {
			this.IsMaybe = isMaybe;
			this.Range = range;
		}
	}

	#region 関数呼び出し

	/// <summary>
	/// 関数呼び出しを表すクラス。
	/// </summary>
	public class Call : Phrase {
		/// <summary>呼び出す関数の名前</summary>
		public Element Target { get; private set; }

		/// <summary>呼び出す際の実引数</summary>
		[DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
		public ReadOnlyCollection<ArgumentTuple> Arguments { get; private set; }

		public Call(Element target, IEnumerable<ArgumentTuple> args, bool isMaybe, TextRange range)
			: base(isMaybe, range) {
			Contract.Requires<ArgumentNullException>(target != null);
			Contract.Requires<ArgumentNullException>(args != null);
			this.Target = target;
			Arguments = Array.AsReadOnly(args.ToArray());
		}

		public override string ToString() {
			return Target + "する。";
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
		public ArgumentTuple FirstArg { get; private set; }

		public MapCall(Element name, ArgumentTuple firstArg, IList<ArgumentTuple> args, bool isMaybe, TextRange range)
			: base(name, args, isMaybe, range) {
			this.FirstArg = firstArg;
		}

		public override string ToString() {
			return "それぞれ" + Target + "する。";
		}
	}

	/// <summary>
	/// 実引数と助詞の対
	/// </summary>
	public class ArgumentTuple {
		public Element Argument { get; private set; }
		public string Suffix { get; private set; }

		public ArgumentTuple(Element arg, string sfx) {
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
	public class Assign : Phrase {
		/// <summary>代入の対象。</summary>
		public string Name { get; private set; }

		/// <summary>代入する引数。nullの場合、暗黙の引数を用いること。</summary>
		public Element Value { get; private set; }

		public Assign(string name, Element value, bool isMaybe, TextRange range)
			: base(isMaybe, range) {
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
	public class PropertySet : Phrase {

		/// <summary>代入先</summary>
		public PropertyAccess Property { get; private set; }

		/// <summary>代入する引数。nullの場合、暗黙の引数を用いること。</summary>
		public Element Value { get; private set; }

		public PropertySet(PropertyAccess property, Element value, bool isMaybe, TextRange range)
			: base(isMaybe, range) {
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
	public class DefineValue : Phrase {

		/// <summary>変数名</summary>
		public string Name;

		/// <summary>値。nullなら暗黙の引数を用いること。</summary>
		public Element Value;

		public DefineValue(string name, Element value, TextRange range)
			: base(false, range) {
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
		public TextRange Range { get; private set; }

		[DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
		public ReadOnlyCollection<Statement> Statements { get; private set; }

		public Block(IEnumerable<Statement> stmts, TextRange range) {
			Contract.Requires<ArgumentNullException>(stmts != null);
			this.Statements = Array.AsReadOnly(stmts.ToArray());
			this.Range = range;
		}
	}

	#endregion

	#region 要素

	public interface IExpr {
	}

	public enum ElementType {
		None = 0,

		// リテラル値
		Integer, Float, String, Bool, Null,
		// 式
		Tuple, List, Binary, Unary, Property, Lambda,
		// 変数
		Symbol, LambdaParam,
	}

	/// <summary>要素</summary>
	public abstract class Element : IExpr {
		public TextRange Range { get; private set; }

		/// <summary>要素の種類</summary>
		public abstract ElementType Type { get; }

		internal Element(TextRange range) {
			this.Range = range;
		}
	}

	#region リテラル値

	public abstract class Literal : Element {
		internal Literal(TextRange range) : base(range) { }
	}

	/// <summary>
	/// リストの各要素を示すクラス。
	/// </summary>
	public class ListLiteral : Literal {

		[DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
		public ReadOnlyCollection<Element> Elements { get; private set; }
		public override ElementType Type { get { return ElementType.List; } }

		public ListLiteral(IEnumerable<Element> elems, TextRange range)
			: base(range) {
			Contract.Requires<ArgumentNullException>(elems != null);
			Contract.Requires<ArgumentException>(Contract.ForAll(elems, e => e != null));
			this.Elements = Array.AsReadOnly(elems.ToArray());
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

		public override ElementType Type { get { return ElementType.Tuple; } }

		public TupleLiteral(Element head, Element tail, TextRange range)
			: base(range) {
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
		public override ElementType Type { get { return ElementType.String; } }

		public StringLiteral(string value, TextRange range)
			: base(range) {
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
		public override ElementType Type { get { return ElementType.Integer; } }

		public IntLiteral(int value, TextRange range)
			: base(range) {
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
		public override ElementType Type { get { return ElementType.Float; } }

		public FloatLiteral(double value, TextRange range)
			: base(range) {
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
		public readonly bool Value;
		public override ElementType Type { get { return ElementType.Bool; } }

		public BoolLiteral(bool value, TextRange range)
			: base(range) {
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
		public override ElementType Type { get { return ElementType.Null; } }
		public NullLiteral(TextRange range) : base(range) { }

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
		public BinaryOperationType ExprType { get; private set; }
		public override ElementType Type { get { return ElementType.Binary; } }

		public BinaryExpr(Element left, BinaryOperationType type, Element right, TextRange range)
			: base(range) {
			Contract.Requires<ArgumentNullException>(left != null);
			Contract.Requires<ArgumentNullException>(right != null);
			this.Left = left;
			this.Right = right;
			this.ExprType = type;
		}
	}

	/// <summary>
	/// 単項演算を示すクラス。
	/// </summary>
	public class UnaryExpr : Element {
		public Element Value { get; private set; }
		public UnaryOperationType ExprType { get; private set; }
		public override ElementType Type { get { return ElementType.Unary; } }

		public UnaryExpr(UnaryOperationType type, Element value, TextRange range)
			: base(range) {
			Contract.Requires<ArgumentNullException>(value != null);
			this.ExprType = type;
			this.Value = value;
		}
	}

	/// <summary>
	/// 括弧による関数呼び出しを示すクラス。
	/// </summary>
	public class FuncCall : Element {
		public string Name { get; private set; }
		public IList<Element> Arguments { get; private set; }
		public override ElementType Type { get { return ElementType.None; } }

		public FuncCall(string name, IList<Element> args, TextRange range)
			: base(range) {
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
		public override ElementType Type { get { return ElementType.Property; } }

		public PropertyAccess(Element value, string name, TextRange range)
			: base(range) {
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
		public override ElementType Type { get { return ElementType.Symbol; } }

		public Symbol(string name, TextRange range)
			: base(range) {
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
		public override ElementType Type { get { return ElementType.Lambda; } }

		public Lambda(Element elem, TextRange range)
			: base(range) {
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
		public override ElementType Type { get { return ElementType.LambdaParam; } }

		public LambdaParameter(string name, TextRange range)
			: base(range) {
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
