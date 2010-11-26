using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;

namespace Kurogane.Compiler {

	public interface IToken<out T> where T : IToken<T> {
		T Next { get; }
		bool HasNext { get; }
	}

	/// <summary>
	/// 字句解析して，切り出したもの
	/// </summary>
	public abstract class Token : IToken<Token> {

		private Lexer _lexer;
		private Token _next;

		public TextRange Range { get; private set; }

		public abstract string Value { get; }
		public virtual bool HasNext { get { return true; } }

		/// <summary>
		/// 次のトークン
		/// </summary>
		public Token Next {
			get {
				if (_lexer != null) {
					_next = _lexer.Next();
					_lexer = null;
				}
				return _next;
			}
		}

		public Token(Lexer lexer, TextRange range) {
			Contract.Requires<ArgumentNullException>(lexer != null);
			_lexer = lexer;
			this.Range = range;
		}

		public override string ToString() {
			return this.GetType().Name + " : " + Value;
		}
	}

	/// <summary>
	/// 最後をnullにする代わりにこのトークンを用いる。
	/// </summary>
	public class NullToken : Token {
		public NullToken(Lexer lexer, TextLocation location) : base(lexer, new TextRange(location, location)) { }
		public override bool HasNext { get { return false; } }

		public override string Value {
			get { throw new InvalidOperationException(); }
		}
	}

	/// <summary>
	/// SymbolやLiteralなど、名詞として対象となりえるトークン
	/// </summary>
	public abstract class TargetToken : Token {
		public TargetToken(Lexer lexer, TextRange range) : base(lexer, range) { }
	}

	/// <summary>
	/// 変数名やクラス名，関数名などを示すトークン
	/// </summary>
	public class SymbolToken : TargetToken {
		private readonly string _value;
		public override string Value { get { return _value; } }

		public SymbolToken(Lexer lexer, TextRange range, string value)
			: base(lexer, range) {
			Contract.Requires<ArgumentNullException>(value != null);
			_value = value;
		}
	}

	/// <summary>
	/// ラムダ式の引数の代わりとなる記号を示すトークン
	/// </summary>
	public class LambdaSpaceToken : TargetToken {
		private readonly string _value;
		public override string Value { get { return _value; } }

		public LambdaSpaceToken(Lexer lexer, TextRange range, string value)
			: base(lexer, range) {
			Contract.Requires<ArgumentNullException>(value != null);
			_value = value;
		}
	}

	/// <summary>
	/// 文字列リテラルを示すトークン
	/// </summary>
	public class LiteralToken : TargetToken {
		private readonly string _value;
		public override string Value { get { return _value; } }

		public LiteralToken(Lexer lexer, TextRange range, string value)
			: base(lexer, range) {
			Contract.Requires<ArgumentNullException>(value != null);
			_value = value;
		}
	}

	/// <summary>
	/// 数値リテラルを示すトークン
	/// </summary>
	public abstract class NumberToken : TargetToken {
		public NumberToken(Lexer lexer, TextRange range) : base(lexer, range) { }
	}

	/// <summary>
	/// 整数リテラルを示すトークン
	/// </summary>
	public class IntegerToken : NumberToken {
		private readonly int _value;
		public int IntValue { get { return _value; } }
		public override string Value { get { return _value.ToString(); } }

		public IntegerToken(Lexer lexer, TextRange range, int value)
			: base(lexer, range) {
			_value = value;
		}
	}

	/// <summary>
	/// 小数リテラルを示すトークン
	/// </summary>
	public class DecimalToken : NumberToken {
		private readonly double _value;
		public double DecimalValue { get { return _value; } }
		public override string Value { get { return _value.ToString(); } }

		public DecimalToken(Lexer lexer, TextRange range, double value)
			: base(lexer, range) {
			_value = value;
		}
	}

	/// <summary>
	/// 助詞を示すトークン
	/// </summary>
	public class SuffixToken : Token {
		private readonly string _value;
		public override string Value { get { return _value; } }

		public SuffixToken(Lexer lexer, TextRange range, string value)
			: base(lexer, range) {
			Contract.Requires<ArgumentNullException>(value != null);
			_value = value;
		}
	}

	#region OperatorToken

	/// <summary>
	/// 演算子を示すトークン
	/// </summary>
	public abstract class AbstractOperatorToken : Token {
		public AbstractOperatorToken(Lexer lexer, TextRange range) : base(lexer, range) { }
	}

	public class UnknownOperatorToken : AbstractOperatorToken {
		private readonly string _Value;

		public override string Value { get { return _Value; } }

		public UnknownOperatorToken(Lexer lexer, TextRange range, string value)
			: base(lexer, range) {
			Contract.Requires<ArgumentNullException>(value != null);
			_Value = value;
		}
	}

	#region 算術演算

	public class AddOpToken : AbstractOperatorToken {
		public AddOpToken(Lexer lexer, TextRange range) : base(lexer, range) { }
		public override string Value { get { return "＋"; } }
	}

	public class SubOpToken : AbstractOperatorToken {
		public SubOpToken(Lexer lexer, TextRange range) : base(lexer, range) { }
		public override string Value { get { return "－"; } }
	}

	public class MultipleOpToken : AbstractOperatorToken {
		public MultipleOpToken(Lexer lexer, TextRange range) : base(lexer, range) { }
		public override string Value { get { return "×"; } }
	}

	public class DivideOpToken : AbstractOperatorToken {
		public DivideOpToken(Lexer lexer, TextRange range) : base(lexer, range) { }
		public override string Value { get { return "÷"; } }
	}

	public class ModuloOpToken : AbstractOperatorToken {
		public ModuloOpToken(Lexer lexer, TextRange range) : base(lexer, range) { }
		public override string Value { get { return "％"; } }
	}

	#endregion

	#region 比較演算

	public class EqualOpToken : AbstractOperatorToken {
		public EqualOpToken(Lexer lexer, TextRange range) : base(lexer, range) { }
		public override string Value { get { return "＝"; } }
	}

	public class NotEqualOpToken : AbstractOperatorToken {
		public NotEqualOpToken(Lexer lexer, TextRange range) : base(lexer, range) { }
		public override string Value { get { return "≠"; } }
	}

	public class GreaterThanOpToken : AbstractOperatorToken {
		public GreaterThanOpToken(Lexer lexer, TextRange range) : base(lexer, range) { }
		public override string Value { get { return "＞"; } }
	}

	public class LessThanOpToken : AbstractOperatorToken {
		public LessThanOpToken(Lexer lexer, TextRange range) : base(lexer, range) { }
		public override string Value { get { return "＜"; } }
	}

	public class GreaterThanEqualOpToken : AbstractOperatorToken {
		public GreaterThanEqualOpToken(Lexer lexer, TextRange range) : base(lexer, range) { }
		public override string Value { get { return "≧"; } }
	}

	public class LessThanEqualOpToken : AbstractOperatorToken {
		public LessThanEqualOpToken(Lexer lexer, TextRange range) : base(lexer, range) { }
		public override string Value { get { return "≦"; } }
	}

	#endregion

	#region 論理演算子

	public class AndOpToken : AbstractOperatorToken {
		public AndOpToken(Lexer lexer, TextRange range) : base(lexer, range) { }
		public override string Value { get { return "∧"; } }
	}

	public class OrOpToken : AbstractOperatorToken {
		public OrOpToken(Lexer lexer, TextRange range) : base(lexer, range) { }
		public override string Value { get { return "∨"; } }
	}

	public class NotOpToken : AbstractOperatorToken {
		public NotOpToken(Lexer lexer, TextRange range) : base(lexer, range) { }
		public override string Value { get { return "￢"; } }
	}

	#endregion

	public class ConcatOpToken : AbstractOperatorToken {
		public ConcatOpToken(Lexer lexer, TextRange range) : base(lexer, range) { }
		public override string Value { get { return "…"; } }
	}

	public class ConsOpToken : AbstractOperatorToken {
		public ConsOpToken(Lexer lexer, TextRange range) : base(lexer, range) { }
		public override string Value { get { return "："; } }
	}

	#endregion

	/// <summary>
	/// 予約語を示すトークン
	/// </summary>
	public class ReservedToken : Token {
		private readonly string _value;
		public override string Value { get { return _value; } }
		public ReservedToken(Lexer lexer, TextRange range, string value)
			: base(lexer, range) {
			Contract.Requires<ArgumentNullException>(value != null);
			_value = value;
		}

	}

	/// <summary>
	/// 句読点を示すトークン
	/// </summary>
	public abstract class PunctuationToken : Token {
		public PunctuationToken(Lexer lexer, TextRange range) : base(lexer, range) { }
	}

	/// <summary>
	/// 句点を示すトークン
	/// </summary>
	public class PeriodToken : PunctuationToken {
		public override string Value { get { return "。"; } }
		public PeriodToken(Lexer lexer, TextRange range) : base(lexer, range) { }
	}

	/// <summary>
	/// 読点を示すトークン
	/// </summary>
	public class CommaToken : PunctuationToken {
		public override string Value { get { return "，"; } }
		public CommaToken(Lexer lexer, TextRange range) : base(lexer, range) { }
	}

	public class SemicolonToken : PunctuationToken {
		public override string Value { get { return "；"; } }
		public SemicolonToken(Lexer lexer, TextRange range) : base(lexer, range) { }
	}

	#region 括弧群

	/// <summary>
	/// 開き括弧を示すトークン
	/// </summary>
	public class OpenParenthesisToken : Token {
		public override string Value { get { return "("; } }
		public OpenParenthesisToken(Lexer lexer, TextRange range) : base(lexer, range) { }
	}

	/// <summary>
	/// 閉じ括弧を示すトークン
	/// </summary>
	public class CloseParenthesisToken : Token {
		public override string Value { get { return ")"; } }
		public CloseParenthesisToken(Lexer lexer, TextRange range) : base(lexer, range) { }
	}

	/// <summary>
	/// 開き大括弧を示すトークン
	/// </summary>
	public class OpenBracketToken : Token {
		public override string Value { get { return "["; } }
		public OpenBracketToken(Lexer lexer, TextRange range) : base(lexer, range) { }
	}

	/// <summary>
	/// 閉じ大括弧を示すトークン
	/// </summary>
	public class CloseBracketToken : Token {
		public override string Value { get { return "]"; } }
		public CloseBracketToken(Lexer lexer, TextRange range) : base(lexer, range) { }
	}

	/// <summary>
	/// 開き波括弧を示すトークン
	/// </summary>
	public class OpenBraceToken : Token {
		public override string Value { get { return "{"; } }
		public OpenBraceToken(Lexer lexer, TextRange range) : base(lexer, range) { }
	}

	/// <summary>
	/// 閉じ波括弧を示すトークン
	/// </summary>
	public class CloseBraceToken : Token {
		public override string Value { get { return "}"; } }
		public CloseBraceToken(Lexer lexer, TextRange range) : base(lexer, range) { }
	}


	/// <summary>
	/// 開きすみつき括弧を示すトークン
	/// </summary>
	public class OpenSumiBracketToken : Token {
		public override string Value { get { return "【"; } }
		public OpenSumiBracketToken(Lexer lexer, TextRange range) : base(lexer, range) { }
	}

	/// <summary>
	/// 閉じすみつき括弧を示すトークン
	/// </summary>
	public class CloseSumiBracketToken : Token {
		public override string Value { get { return "】"; } }
		public CloseSumiBracketToken(Lexer lexer, TextRange range) : base(lexer, range) { }
	}

	#endregion

}
