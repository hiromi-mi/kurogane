using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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

		public readonly int LineNumber;
		public readonly int CharCount;

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

		public Token(Lexer lexer) {
			_lexer = lexer;
			this.LineNumber = lexer.LineNumber;
			this.CharCount = lexer.CharCount;
		}

		public override string ToString() {
			return this.GetType().Name + " : " + Value;
		}
	}

	/// <summary>
	/// 最後をnullにする代わりにこのトークンを用いる。
	/// </summary>
	public class NullToken : Token {
		public NullToken(Lexer lexer) : base(lexer) { }
		public override bool HasNext { get { return false; } }

		public override string Value {
			get { throw new NotImplementedException(); }
		}
	}

	/// <summary>
	/// SymbolやLiteralなど、名詞として対象となりえるトークン
	/// </summary>
	public abstract class TargetToken : Token {
		public TargetToken(Lexer lexer) : base(lexer) { }
	}

	/// <summary>
	/// 変数名やクラス名，関数名などを示すトークン
	/// </summary>
	public class SymbolToken : TargetToken {
		private readonly string _value;
		public override string Value { get { return _value; } }

		public SymbolToken(Lexer lexer, string value)
			: base(lexer) {
			_value = value;
		}
	}

	/// <summary>
	/// 文字列リテラルを示すトークン
	/// </summary>
	public class LiteralToken : TargetToken {
		private readonly string _value;
		public override string Value { get { return _value; } }

		public LiteralToken(Lexer lexer, string value)
			: base(lexer) {
			_value = value;
		}
	}

	/// <summary>
	/// 数値リテラルを示すトークン
	/// </summary>
	public abstract class NumberToken : TargetToken {
		public NumberToken(Lexer lexer) : base(lexer) { }
	}

	/// <summary>
	/// 整数リテラルを示すトークン
	/// </summary>
	public class IntegerToken : NumberToken {
		private readonly int _value;
		public int IntValue { get { return _value; } }
		public override string Value { get { return _value.ToString(); } }

		public IntegerToken(Lexer lexer, int value)
			: base(lexer) {
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

		public DecimalToken(Lexer lexer, double value)
			: base(lexer) {
			_value = value;
		}
	}

	/// <summary>
	/// 助詞を示すトークン
	/// </summary>
	public class SuffixToken : Token {
		private readonly string _value;
		public override string Value { get { return _value; } }

		public SuffixToken(Lexer lexer, string value)
			: base(lexer) {
			_value = value;
		}
	}

	#region OperatorToken

	/// <summary>
	/// 演算子を示すトークン
	/// </summary>
	public abstract class AbstractOperatorToken : Token {
		public AbstractOperatorToken(Lexer lexer) : base(lexer) { }
	}

	public class UnknownOperatorToken : AbstractOperatorToken {
		private readonly string _Value;

		public override string Value { get { return _Value; } }

		public UnknownOperatorToken(Lexer lexer, string value)
			: base(lexer) {
			_Value = value;
		}
	}

	#region 算術演算

	public class AddOpToken : AbstractOperatorToken {
		public AddOpToken(Lexer lexer) : base(lexer) { }
		public override string Value { get { return "＋"; } }
	}

	public class SubOpToken : AbstractOperatorToken {
		public SubOpToken(Lexer lexer) : base(lexer) { }
		public override string Value { get { return "－"; } }
	}

	public class MultipleOpToken : AbstractOperatorToken {
		public MultipleOpToken(Lexer lexer) : base(lexer) { }
		public override string Value { get { return "×"; } }
	}

	public class DivideOpToken : AbstractOperatorToken {
		public DivideOpToken(Lexer lexer) : base(lexer) { }
		public override string Value { get { return "÷"; } }
	}

	public class ModuloOpToken : AbstractOperatorToken {
		public ModuloOpToken(Lexer lexer) : base(lexer) { }
		public override string Value { get { return "％"; } }
	}

	#endregion

	#region 比較演算

	public class EqualOpToken : AbstractOperatorToken {
		public EqualOpToken(Lexer lexer) : base(lexer) { }
		public override string Value { get { return "＝"; } }
	}

	public class NotEqualOpToken : AbstractOperatorToken {
		public NotEqualOpToken(Lexer lexer) : base(lexer) { }
		public override string Value { get { return "≠"; } }
	}

	public class GreaterThanOpToken : AbstractOperatorToken {
		public GreaterThanOpToken(Lexer lexer) : base(lexer) { }
		public override string Value { get { return "＞"; } }
	}

	public class LessThanOpToken : AbstractOperatorToken {
		public LessThanOpToken(Lexer lexer) : base(lexer) { }
		public override string Value { get { return "＜"; } }
	}

	public class GreaterThanEqualOpToken : AbstractOperatorToken {
		public GreaterThanEqualOpToken(Lexer lexer) : base(lexer) { }
		public override string Value { get { return "≧"; } }
	}

	public class LessThanEqualOpToken : AbstractOperatorToken {
		public LessThanEqualOpToken(Lexer lexer) : base(lexer) { }
		public override string Value { get { return "≦"; } }
	}

	#endregion

	#region 論理演算子

	public class AndOpToken : AbstractOperatorToken {
		public AndOpToken(Lexer lexer) : base(lexer) { }
		public override string Value { get { return "∧"; } }
	}

	public class OrOpToken : AbstractOperatorToken {
		public OrOpToken(Lexer lexer) : base(lexer) { }
		public override string Value { get { return "∨"; } }
	}

	public class NotOpToken : AbstractOperatorToken {
		public NotOpToken(Lexer lexer) : base(lexer) { }
		public override string Value { get { return "￢"; } }
	}

	#endregion

	public class ConcatOpToken : AbstractOperatorToken {
		public ConcatOpToken(Lexer lexer) : base(lexer) { }
		public override string Value { get { return "…"; } }
	}

	#endregion

	/// <summary>
	/// 予約語を示すトークン
	/// </summary>
	public class ReservedToken : Token {
		private readonly string _value;
		public override string Value { get { return _value; } }
		public ReservedToken(Lexer lexer, string value)
			: base(lexer) {
			_value = value;
		}

	}

	/// <summary>
	/// 句読点を示すトークン
	/// </summary>
	public abstract class PunctuationToken : Token {
		private readonly string _value;
		public override string Value { get { return _value; } }

		public PunctuationToken(Lexer lexer, string value)
			: base(lexer) {
			_value = value;
		}
	}

	/// <summary>
	/// 句点を示すトークン
	/// </summary>
	public class PeriodToken : PunctuationToken {
		public PeriodToken(Lexer lexer, string value)
			: base(lexer, value) {
		}
	}

	/// <summary>
	/// 読点を示すトークン
	/// </summary>
	public class CommaToken : PunctuationToken {
		public CommaToken(Lexer lexer, string value)
			: base(lexer, value) {
		}
	}

	public class SemicolonToken : PunctuationToken {
		public SemicolonToken(Lexer lexer) : base(lexer, ";") { }
	}

	#region 括弧群

	/// <summary>
	/// 開き括弧を示すトークン
	/// </summary>
	public class OpenParenthesisToken : Token {
		public override string Value { get { return "("; } }
		public OpenParenthesisToken(Lexer lexer) : base(lexer) { }
	}

	/// <summary>
	/// 閉じ括弧を示すトークン
	/// </summary>
	public class CloseParenthesisToken : Token {
		public override string Value { get { return ")"; } }
		public CloseParenthesisToken(Lexer lexer) : base(lexer) { }
	}

	/// <summary>
	/// 開き大括弧を示すトークン
	/// </summary>
	public class OpenBracketToken : Token {
		public override string Value { get { return "["; } }
		public OpenBracketToken(Lexer lexer) : base(lexer) { }
	}

	/// <summary>
	/// 閉じ大括弧を示すトークン
	/// </summary>
	public class CloseBracketToken : Token {
		public override string Value { get { return "]"; } }
		public CloseBracketToken(Lexer lexer) : base(lexer) { }
	}

	/// <summary>
	/// 開き波括弧を示すトークン
	/// </summary>
	public class OpenBraceToken : Token {
		public override string Value { get { return "{"; } }
		public OpenBraceToken(Lexer lexer) : base(lexer) { }
	}

	/// <summary>
	/// 閉じ波括弧を示すトークン
	/// </summary>
	public class CloseBraceToken : Token {
		public override string Value { get { return "}"; } }
		public CloseBraceToken(Lexer lexer) : base(lexer) { }
	}

	#endregion

}
