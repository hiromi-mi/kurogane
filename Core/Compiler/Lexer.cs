using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Kurogane.Compiler {
	/// <summary>
	/// 字句解析を行うクラス
	/// </summary>
	public class Lexer {

		#region static

		private const char kanaBegin = 'ぁ';
		private const char kanaEnd = 'ん';

		private static readonly char[] OperatorCharacter = {
			'+', '-', '*', '/', '%', '^', '&', '|', '!', '<', '>', '=',
			'＋', '－', '×', '÷', // 四則演算
			'＜', '≦', '＝', '≧', '＞', '≠', // 比較
			'￢','∧','∨', // 真偽
			'⊆', '⊇', '∈', '∋', '⊂', '⊃', '∪', '∩', // 集合
		};

		private static readonly ISet<char> OperatorCharacterSet;
		private static readonly ISet<char> BreakTokenSet;

		static Lexer() {
			OperatorCharacterSet = new HashSet<char>(OperatorCharacter);

			var set = new HashSet<char>(OperatorCharacter);
			char[] anotherBreakToken = {
				'「', '[', '、', '，', '。', '．', '(', ')'
			};
			foreach (var c in anotherBreakToken) {
				set.Add(c);
			}
			for (int i = 0; i <= 9; i++) {
				set.Add((char)(i + '0'));
			}

			BreakTokenSet = set;
		}

		#endregion

		private readonly TextReader _reader;

		private int _CurrentChar;
		private Stack<Token> _stack = new Stack<Token>();

		public Lexer(TextReader reader) {
			_reader = reader;
			_NextChar();
		}

		/// <summary>
		/// 次のトークン
		/// </summary>
		public Token Next() {
			if (_stack.Count > 0)
				return _stack.Pop();
			return NextToken();
		}


		/// <summary>
		/// 入力を読み進め、次のトークンを返します。
		/// 最後まで到達したときのみ null を返し、
		/// それ以外は null を返しません。
		/// </summary>
		/// <returns></returns>
		private Token NextToken() {
			while (Char.IsWhiteSpace((char)_CurrentChar)) _NextChar();
			switch (_CurrentChar) {
			case -1:
				return new NullToken(this);

			case '[':
				return ReadSymbolToken();

			case '「':
				return ReadLiteralToken();

			case '，':
			case '、':
			case '。':
			case '．':
				return ReadPunctuationToken();

			case '(':
				return ReadOpenParenthesisToken();
			case ')':
				return ReadCloseParenthesisToken();

			default:
				if (Char.IsDigit((char)_CurrentChar))
					return ReadNumberToken();
				if (OperatorCharacterSet.Contains((char)_CurrentChar))
					return ReadOperatorToken();

				return ReadPostPositionOrReservedToken();
			}

			throw new LexicalException();
		}

		private int _NextChar() {
			return (_CurrentChar = _reader.Read());
		}

		#region 各Tokenに対するReadメソッド

		private SymbolToken ReadSymbolToken() {
			var buff = new StringBuilder();
			while (true) {
				int c = _NextChar();
				if (c == -1) {
					throw new LexicalException(
						"最後まで読みましたが、文字 \"]\" が発見できませんでした。");
				}
				else if (c == ']') {
					_NextChar();
					return new SymbolToken(this, buff.ToString());
				}
				buff.Append((char)c);
			}
		}

		private LiteralToken ReadLiteralToken() {
			var buff = new StringBuilder();
			while (true) {
				int c = _NextChar();
				if (c == -1) {
					throw new LexicalException(
						"最後まで読みましたが、文字 \"]\" が発見できませんでした。");
				}
				else if (c == '」') {
					_NextChar();
					return new LiteralToken(this, buff.ToString());
				}
				buff.Append((char)c);
			}
		}

		private NumberToken ReadNumberToken() {
			int num = _CurrentChar - '0';
			while (true) {
				int n = _NextChar() - '0';
				if (n < 0 || 9 < n) break;
				num = num * 10 + n;
			}
			if (_CurrentChar == '.') {
				// 小数の処理
				int denom = 1;
				int numer = 0;
				while (true) {
					int n = _NextChar() - '0';
					if (n < 0 || 9 < n) break;
					numer = numer * 10 + n;
					denom *= 10;
				}
				return new DecimalToken(this, num + (1.0 * numer / denom));
			}
			else {
				return new IntegerToken(this, num);
			}
		}

		private OperatorToken ReadOperatorToken() {
			var buff = new StringBuilder();
			buff.Append((char)_CurrentChar);
			while (true) {
				int c = _NextChar();
				if (OperatorCharacterSet.Contains((char)c)) {
					buff.Append((char)c);
				}
				else {
					return new OperatorToken(this, buff.ToString());
				}
			}
		}

		private PunctuationToken ReadPunctuationToken() {
			char c = (char)_CurrentChar;
			_NextChar();
			return new PunctuationToken(this, c.ToString());
		}

		/// <summary>
		/// 助詞トークン、あるいは予約語トークンを読み取る
		/// </summary>
		/// <returns></returns>
		private Token ReadPostPositionOrReservedToken() {
			if (kanaBegin <= (char)_CurrentChar && (char)_CurrentChar <= kanaEnd)
				return ReadPostPositionToken();
			else if (Char.IsLetter((char)_CurrentChar))
				return ReadSymbolLetterToken();
			throw new NotImplementedException();
		}

		private Token ReadSymbolLetterToken() {
			var buff = new StringBuilder();
			buff.Append((char)_CurrentChar);
			while (true) {
				char c = (char)_NextChar();
				if (Char.IsLetter(c) && (c < kanaBegin || kanaEnd < c))
					buff.Append(c);
				else
					break;
			}
			string[] reserved = { "手順", "以上", "以下", "他", "無", "失敗" };
			string str = buff.ToString();
			if (Array.IndexOf(reserved, str) >= 0)
				return new ReservedToken(this, str);
			else
				return new SymbolToken(this, buff.ToString());
		}

		private Token ReadPostPositionToken() {
			if (_CurrentChar == (int)'と') {
				_NextChar();
				return new PostPositionToken(this, "と");
			}
			var buff = new StringBuilder();
			buff.Append((char)_CurrentChar);
			while (true) {
				char c = (char)_NextChar();
				if (kanaBegin <= c && c <= kanaEnd)
					buff.Append(c);
				else
					break;
			}
			string[] reserved = { "もし", "なら", "してみて", "してみる", "して", "し", "する" };
			string str = buff.ToString();
			if (Array.IndexOf(reserved, str) >= 0)
				return new ReservedToken(this, str);
			else
				return new PostPositionToken(this, str);
		}

		/// <summary>
		/// 助詞と推定されたトークンから予約語を切り出す。
		/// </summary>
		/// <returns></returns>
		private Token SplitReservedToken(string str) {
			// 「もし」
			const string ifWord = "もし";
			if (str == ifWord) {
				return new ReservedToken(this, ifWord);
			}
			{	// ～し、～する。～したもの。
				bool execFlag = false;
				string execWord = null;
				if (_CurrentChar == '、' || _CurrentChar == '，') {
					const string word = "し";
					if (str.EndsWith(word)) {
						execFlag = true;
						execWord = word;
					}
				}
				else if (_CurrentChar == '。' || _CurrentChar == '．') {
					const string word = "する";
					if (str.EndsWith(word)) {
						execFlag = true;
						execWord = word;
					}
				}
				else {
					const string word = "したもの";
					if (str.EndsWith(word)) {
						execFlag = true;
						execWord = word;
					}
				}
				if (execFlag) {
					return SplitReservedTokenEnding(str, execWord);
				}
			}
			{	// 以上、以下
				const string wordStart = "以下";
				const string wordEnd = "以上";
				if (str.StartsWith(wordStart)) {
					return SplitReservedTokenStarting(str, wordStart);
				}
				if (str == wordEnd) {
					return new ReservedToken(this, wordEnd);
				}
			}
			{	// 手順
				const string wordProc = "手順";
				if (str.Contains(wordProc)) {
					var sideWords = str.Split(new[] { wordProc }, 2, StringSplitOptions.None);
					if (sideWords.Length != 2) {
						throw new LexicalException("うまく「手順」のキーワードを切り取れませんでした。");
					}
					if (sideWords[1].Length > 0) {
						_stack.Push(SplitReservedToken(sideWords[1]));
					}
					var token = new ReservedToken(this, wordProc);
					if (sideWords[0].Length > 0) {
						_stack.Push(token);
						return SplitReservedToken(sideWords[0]);
					}
					else {
						return token;
					}
				}
			}
			{	// もし
				const string wordIfStart = "もし";
				if (str.StartsWith(wordIfStart)) {
					return SplitReservedTokenStarting(str, wordIfStart);
				}
				const string wordIfEnd = "なら";
				if (str.EndsWith(wordIfEnd)) {
					return SplitReservedTokenEnding(str, wordIfEnd);
				}
				const string wordElse = "他";
				if (str.StartsWith(wordElse)) {
					return SplitReservedTokenStarting(str, wordElse);
				}
			}
			// 何も無い場合
			return new PostPositionToken(this, str);
		}

		/// <summary>
		/// 予約語 "reserved" で始まる文字列を分割
		/// </summary>
		private Token SplitReservedTokenStarting(string str, string reserved) {
			var token = new ReservedToken(this, reserved);
			if (str.Length > reserved.Length) {
				_stack.Push(SplitReservedToken(str.Substring(reserved.Length, str.Length - reserved.Length)));
			}
			return token;
		}

		/// <summary>
		/// 予約語 "reserved" で終わる文字列を分割
		/// </summary>
		private Token SplitReservedTokenEnding(string str, string reserved) {
			var token = new ReservedToken(this, reserved);
			if (str.Length > reserved.Length) {
				_stack.Push(token);
				return SplitReservedToken(str.Substring(0, str.Length - reserved.Length));
			}
			else {
				return token;
			}
		}

		private OpenParenthesisToken ReadOpenParenthesisToken() {
			_NextChar();
			return new OpenParenthesisToken(this);
		}

		private CloseParenthesisToken ReadCloseParenthesisToken() {
			_NextChar();
			return new CloseParenthesisToken(this);
		}

		#endregion

		public void Dispose() {
			_reader.Dispose();
		}
	}

	public class LexicalException : Exception {
		public LexicalException() : base() { }
		public LexicalException(string message) : base(message) { }
	}
}

