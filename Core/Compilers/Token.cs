using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kurogane.Compilers {

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

		public abstract string Value { get; }

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
		public virtual bool HasNext { get { return true; } }

		public Token(Lexer lexer) {
			_lexer = lexer;
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

		public override string Value {
			get { throw new NotImplementedException(); }
		}
		public override bool HasNext { get { return false; } }
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

		public SymbolToken(Lexer lexer, string value)
			: base(lexer) {
			_value = value;
		}

		public override string Value { get { return _value; } }
	}

	/// <summary>
	/// 文字列リテラルを示すトークン
	/// </summary>
	public class LiteralToken : TargetToken {
		private readonly string _value;

		public LiteralToken(Lexer lexer, string value)
			: base(lexer) {
			_value = value;
		}

		public override string Value { get { return _value; } }
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

		public IntegerToken(Lexer lexer, int value)
			: base(lexer) {
			_value = value;
		}

		public override string Value { get { return _value.ToString(); } }
		public int IntValue { get { return _value; } }
	}

	/// <summary>
	/// 小数リテラルを示すトークン
	/// </summary>
	public class DecimalToken : NumberToken {
		private readonly double _value;

		public DecimalToken(Lexer lexer, double value)
			: base(lexer) {
			_value = value;
		}

		public override string Value { get { return _value.ToString(); } }
		public double DecimalValue { get { return _value; } }
	}

	/// <summary>
	/// 助詞を示すトークン
	/// </summary>
	public class PostPositionToken : Token {
		private readonly string _value;

		public PostPositionToken(Lexer lexer, string value)
			: base(lexer) {
			_value = value;
		}

		public override string Value { get { return _value; } }

	}

	/// <summary>
	/// 演算子を示すトークン
	/// </summary>
	public class OperatorToken : Token {
		private readonly string _value;

		public OperatorToken(Lexer lexer, string value)
			: base(lexer) {
			_value = value;
		}

		public override string Value { get { return _value; } }
	}

	/// <summary>
	/// 予約語を示すトークン
	/// </summary>
	public class ReservedToken : Token {
		private readonly string _value;

		public ReservedToken(Lexer lexer, string value)
			: base(lexer) {
			_value = value;
		}

		public override string Value { get { return _value; } }
	}

	/// <summary>
	/// 句読点を示すトークン
	/// </summary>
	public class PunctuationToken : Token {
		private readonly string _value;

		public PunctuationToken(Lexer lexer, string value)
			: base(lexer) {
			_value = value;
		}

		public override string Value { get { return _value; } }
	}

	/// <summary>
	/// 開き括弧を示すトークン
	/// </summary>
	public class OpenParenthesisToken : Token {
		public const char Char = '(';
		public override string Value { get { return "("; } }

		public OpenParenthesisToken(Lexer lexer) : base(lexer) { }
	}

	/// <summary>
	/// 閉じ括弧を示すトークン
	/// </summary>
	public class CloseParenthesisToken : Token {
		public const char Char = ')';
		public override string Value { get { return ")"; } }

		public CloseParenthesisToken(Lexer lexer) : base(lexer) { }
	}
}
