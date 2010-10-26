using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;

namespace Kurogane.Compiler {

	/// <summary>
	/// 字句解析を行うクラス
	/// </summary>
	public class Lexer {

		#region static

		private const string NoFile = "-- no file --";

		private const char kanaBegin = 'ぁ';
		private const char kanaEnd = 'ん';

		private static readonly char[] OperatorCharacter = {
			'+', '-', '*', '/', '%', '&', '|', '!', '<', '>', '=',
			'＋', '－', '×', '＊', '÷', '／', '％', // 四則演算
			'＜', '≦', '＝', '≧', '＞', '≠', // 比較演算
			'∧', '∨', '￢', // 論理演算
		};

		private static readonly char[] PunctuationToken = {
			',', '，', '、', '.', '．', '。', ';', '；'
		};

		private static readonly char[] Brackets = {
			'(', '{', '[', '（', '｛', '［',
			')', '}', ']', '）', '｝', '］'
		};

		#endregion

		private readonly TextReader _reader;

		private int _CurrentChar = 0;

		/// <summary>読み取り中の行数</summary>
		private int line = 1;

		/// <summary>読み取り中の行の何文字目</summary>
		private int ch = 0;

		// これらはTokenをnewする時に書き換えること。
		public int LineNumber { get; private set; }
		public int CharCount { get; private set; }

		private string _FileName;

		/// <summary>Windowsの改行で2カウントしないためのフラグ</summary>
		private bool flagLF = false;

		private Stack<Token> _stack = new Stack<Token>();

		public Lexer(TextReader reader, string filename) {
			_reader = reader;
			_NextChar();
			_FileName = filename ?? NoFile;
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
					line++;
					ch = 1;
				}
				else {
					flagLF = false;
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
			while (Char.IsWhiteSpace((char)_CurrentChar)) _NextChar();

			if (_CurrentChar == -1)
				return new NullToken(this);

			char c = (char)_CurrentChar;

			if (c == '「')
				return ReadLiteralToken();

			if (Array.IndexOf(PunctuationToken, c) >= 0)
				return ReadPunctuationToken();

			if (Array.IndexOf(Brackets, c) >= 0)
				return ReadBracketsToken();

			if (Char.IsDigit(c))
				return ReadNumberToken();

			if (Array.IndexOf(OperatorCharacter, c) >= 0)
				return ReadOperatorToken();

			if (kanaBegin <= c && c <= kanaEnd)
				return ReadPostPositionToken();

			if (Char.IsLetter(c))
				return ReadSymbolLetterToken();

			throw new LexicalException(String.Format(
				"{0}の{1}行{2}文字目に，未知のトークンが出現しました。",
				_FileName, LineNumber, CharCount));
		}

		private void SkipComment() {
			Debug.Assert(_CurrentChar == '※');
			int[] endChar = { '。', '.', '．' };
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
			LineNumber = line;
			CharCount = ch;

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
			LineNumber = line;
			CharCount = ch;

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

		private AbstractOperatorToken ReadOperatorToken() {
			LineNumber = line;
			CharCount = ch;

			var buff = new StringBuilder();
			buff.Append((char)_CurrentChar);
			while (true) {
				char c = (char)_NextChar();
				if (Array.IndexOf(OperatorCharacter, c) == -1)
					break;
				buff.Append(c);
			}
			var op = buff.ToString();
			switch (op) {
			case "+":
			case "＋":
				return new AddOpToken(this);
			case "-":
			case "－":
				return new SubOpToken(this);
			case "*":
			case "＊":
			case "×":
				return new MultipleOpToken(this);
			case "/":
			case "／":
			case "÷":
				return new DivideOpToken(this);
			case "%":
			case "％":
				return new ModuloOpToken(this);

			case "=":
			case "＝":
				return new EqualOpToken(this);
			case "!=":
			case "≠":
				return new NotEqualOpToken(this);
			case "<":
			case "＜":
				return new LessThanOpToken(this);
			case "<=":
			case "≦":
				return new LessThanEqualOpToken(this);
			case ">=":
			case "≧":
				return new GreaterThanEqualOpToken(this);
			case ">":
			case "＞":
				return new GreaterThanOpToken(this);

			case "&":
			case "＆":
			case "∧":
				return new AndOpToken(this);
			case "|":
			case "｜":
			case "∨":
				return new OrOpToken(this);
			case "!":
			case "！":
			case "￢":
				return new NotOpToken(this);

			default:
				return new UnknownOperatorToken(this, op);
			}
		}

		private PunctuationToken ReadPunctuationToken() {
			LineNumber = line;
			CharCount = ch;

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
			case ';':
			case '；':
				return new SemicolonToken(this);
			}
			Debug.Assert(false, "到達不可能" + Environment.NewLine + "プログラムを見直すこと。");
			return null;
		}

		private Token ReadSymbolLetterToken() {
			LineNumber = line;
			CharCount = ch;

			var buff = new StringBuilder();
			buff.Append((char)_CurrentChar);
			while (true) {
				char c = (char)_NextChar();
				if (Char.IsLetter(c) && (c < kanaBegin || kanaEnd < c))
					buff.Append(c);
				else
					break;
			}
			string[] reserved = { "手順", ConstantNames.BlockBegin, ConstantNames.BlockEnd, ConstantNames.BlockExec, ConstantNames.ElseText, ConstantNames.NullText };
			string str = buff.ToString();
			if (str == "返" && _CurrentChar == 'す') {
				_NextChar();
				return new ReservedToken(this, "返す");
			}
			if (Array.IndexOf(reserved, str) >= 0)
				return new ReservedToken(this, str);
			else
				return new SymbolToken(this, buff.ToString());
		}

		private Token ReadPostPositionToken() {
			LineNumber = line;
			CharCount = ch;
			int soLine = -1;
			int soCh = -1;

			if (_CurrentChar == 'と') {
				_NextChar();
				return new SuffixToken(this, "と");
			}
			var buff = new StringBuilder();
			buff.Append((char)_CurrentChar);
			while (true) {
				char c = (char)_NextChar();
				if (kanaBegin <= c && c <= kanaEnd) {
					buff.Append(c);
					if (ch == 'そ') {
						soLine = line;
						soCh = ch;
					}
				}
				else
					break;
			}
			string[] reserved = { "もし", "なら", "してみて", "してみる", "して", "し", "する" };
			string str = buff.ToString();
			if (Array.IndexOf(reserved, str) >= 0)
				return new ReservedToken(this, str);

			const string mapKeyword = "それぞれ";
			if (str.EndsWith(mapKeyword)) {
				if (mapKeyword == str) {
					return new ReservedToken(this, mapKeyword);
				}
				var token = new SuffixToken(this, str.Substring(0, str.Length - mapKeyword.Length));
				LineNumber = soLine;
				CharCount = soCh;
				var soToken = new ReservedToken(this, mapKeyword);
				_stack.Push(soToken);
				return token;
			}

			return new SuffixToken(this, str);
		}


		private Token ReadBracketsToken() {
			LineNumber = line;
			CharCount = ch;

			char c = (char)_CurrentChar;
			_NextChar();
			switch (c) {
			case '(':
			case '（':
				return new OpenParenthesisToken(this);
			case ')':
			case '）':
				return new CloseParenthesisToken(this);
			case '[':
			case '［':
				return new OpenBracketToken(this);
			case ']':
			case '］':
				return new CloseBracketToken(this);
			case '{':
			case '｛':
				return new OpenBraceToken(this);
			case '}':
			case '｝':
				return new CloseBraceToken(this);
			}
			Debug.Assert(false, "到着不可能フロー" + Environment.NewLine + "プログラムを見直すこと。");
			throw new NotImplementedException();
		}

		#endregion

		public void Dispose() {
			_reader.Dispose();
		}
	}
}

