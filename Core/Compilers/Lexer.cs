using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;
using System.Diagnostics.Contracts;

namespace Kurogane.Compilers {
	/// <summary>
	/// 字句解析を行うクラス
	/// </summary>
	public class Lexer {

		#region static

		private const char kanaBegin = 'ぁ';
		private const char kanaEnd = 'ん';

		private static readonly char[] OperatorCharacter = {
			'+', '-', '*', '/', '&', '|', '!', '<', '>', '=',
			'＋', '－', '×', '÷', // 四則演算
			'＜', '≦', '＝', '≧', '＞', '≠', // 比較
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

		private int _CurrentChar = 0;

		/// <summary>読み取り中の行数</summary>
		private int line = 1;

		/// <summary>読み取り中の行の何文字目</summary>
		private int ch = 0;

		/// <summary>Windowsの改行で2カウントしないためのフラグ</summary>
		private bool flagLF = false;

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

		/// <summary>文字を一つ読み進める。</summary>
		private int _NextChar() {
			switch (_CurrentChar) {
			case '\r':
				flagLF = true;
				line++;
				ch = 1;
				break;
			case '\n':
				if (flagLF == false) {
					flagLF = false;
					line++;
					ch = 1;
				}
				break;
			default:
				flagLF = false;
				ch++;
				break;
			}
			return (_CurrentChar = _reader.Read());
		}


		/// <summary>
		/// 入力を読み進め、次のトークンを返します。
		/// 最後まで到達したときのみ null を返し、
		/// それ以外は null を返しません。
		/// </summary>
		/// <returns></returns>
		private Token NextToken() {
			while (Char.IsWhiteSpace((char)_CurrentChar)) _NextChar();
			if (_CurrentChar == '※') SkipComment();
			switch (_CurrentChar) {
			case -1:
				return new NullToken(this);

			case '「':
				return ReadLiteralToken();

			case '，':
			case '、':
			case '。':
			case '．':
				return ReadPunctuationToken();

			case '(':
			case '（':
				return ReadOpenParenthesisToken();
			case ')':
			case'）':
				return ReadCloseParenthesisToken();
			case '[':
			case '［':
				return ReadOpenBracketToken();
			case ']':
			case '］':
				return ReadCloseBracketToken();
			case '{':
			case '｛':
				return ReadOpenBraceToken();
			case '}':
			case '｝':
				return ReadCloseBraceToken();

			default:
				if (Char.IsDigit((char)_CurrentChar))
					return ReadNumberToken();
				if (OperatorCharacterSet.Contains((char)_CurrentChar))
					return ReadOperatorToken();

				return ReadPostPositionOrReservedToken();
			}

			throw new LexicalException();
		}

		private void SkipComment() {
			Debug.Assert(_CurrentChar == '※');
			int[] endChar = { '。', '.', '．'};
			int c = _NextChar();
			switch (c) {
			case '(':
			case '（':
				endChar = new int[] { ')', '）' };
				break;
			case '{':
			case '｛':
				endChar = new int[] { '}', '｝' };
				break;
			case '[':
			case '［':
				endChar = new int[] { ']', '］' };
				break;
			case '「':
				endChar = new int[] { '」' };
				break;
			}
			while (c != -1 && Array.IndexOf(endChar, c) == -1) {
				c = _NextChar();
			}
			if (c != -1)
				_NextChar();
			return;
		}

		#region 各Tokenに対するReadメソッド


		private LiteralToken ReadLiteralToken() {
			var buff = new StringBuilder();
			while (true) {
				int c = _NextChar();
				if (c == -1) {
					throw new LexicalException(
						"\"「\"に対応する \"」\" が見つかりませんでした。");
				}
				else if (c == '」') {
					_NextChar();
					return new LiteralToken(this, buff.ToString());
				}
				buff.Append((char)c);
			}
		}

		private NumberToken ReadNumberToken() {
			int num = 0;
			while (true) {
				int c = _CurrentChar;
				if ('0' <= c && c <= '9') {
					num = num * 10 + c - '0';
				}
				else if ('０' <= c && c <= '９') {
					num = num * 10 + c - '０';
				}
				else {
					break;
				}
				_NextChar();
			}

			if (_CurrentChar == '.' || _CurrentChar == '．') {
				// 小数の処理
				int denom = 1;
				int numer = 0;
				while (true) {
					int c = _NextChar();
					int n = 0;
					if ('0' <= c && c <= '9') {
						n = c - '0';
					}
					else if ('０' <= c && c <= '９') {
						n = c - '０';
					}
					else {
						break;
					}
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
			switch (c) {
			case ',':
			case '，':
			case '、':
				return new CommaToken(this, c.ToString());
			case '。':
			case '．':
			case '.':
				return new PeriodToken(this, c.ToString());
			}
			Debug.Assert(false, "未到達エラー");
			return null;
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
			if (_CurrentChar == 'と') {
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

		#region ReadBrackets

		private OpenParenthesisToken ReadOpenParenthesisToken()
		{
			_NextChar();
			return new OpenParenthesisToken(this);
		}

		private CloseParenthesisToken ReadCloseParenthesisToken()
		{
			_NextChar();
			return new CloseParenthesisToken(this);
		}

		private OpenBracketToken ReadOpenBracketToken()
		{
			_NextChar();
			return new OpenBracketToken(this);
		}

		private CloseBracketToken ReadCloseBracketToken()
		{
			_NextChar();
			return new CloseBracketToken(this);
		}

		private OpenBraceToken ReadOpenBraceToken()
		{
			_NextChar();
			return new OpenBraceToken(this);
		}

		private CloseBraceToken ReadCloseBraceToken()
		{
			_NextChar();
			return new CloseBraceToken(this);
		}

		#endregion

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

