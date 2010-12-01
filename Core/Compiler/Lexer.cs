using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Numerics;

namespace Kurogane.Compiler {

	/// <summary>
	/// 字句解析を行うクラス
	/// </summary>
	public class Lexer {

		#region static

		private const char kanaBegin = 'ぁ';
		private const char kanaEnd = 'ん';

		private static readonly char[] OperatorCharacter = {
			'+', '-', '*', '/', '%', '&', '|', '!', '<', '>', '=', ':',
			'＋', '－', '×', '＊', '÷', '／', '％', // 四則演算
			'＜', '≦', '＝', '≧', '＞', '≠', // 比較演算
			'∧', '∨', '￢', '＆', '｜', '！', // 論理演算
			'…', // 結合演算子
			'：', '・' // cons 演算子
		};

		private static readonly char[] PunctuationToken = {
			',', '，', '、', '.', '．', '。', ';', '；'
		};

		private static readonly char[] Brackets = {
			'(', '{', '[', '（', '｛', '［', '【',
			')', '}', ']', '）', '｝', '］', '】',
		};

		private static readonly char[] LambdaSpaceToken = {
			'○', '△', '□',										  
		};

		#endregion

		/// <summary>
		/// 読み取り先
		/// </summary>
		private readonly TextReader _reader;

		/// <summary>
		/// 読み取り中の文字
		/// </summary>
		private int _CurrentChar;

		/// <summary>
		/// 読み取り中の位置
		/// </summary>
		private TextLocation _Location = TextLocation.Start;

		/// <summary>
		/// 読み取り中のファイル名
		/// </summary>
		private string _FileName;

		/// <summary>Windowsの改行で2カウントしないためのフラグ</summary>
		private bool flagLF = false;

		private Stack<Token> _stack = new Stack<Token>();

		public Lexer(TextReader reader, string filename) {
			Contract.Requires<ArgumentNullException>(reader != null);
			Contract.Requires<ArgumentException>(!String.IsNullOrWhiteSpace(filename));

			_reader = reader;
			_NextChar();
			_FileName = filename;
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
				_Location = _Location.NextLine();
				break;
			case '\n':
				if (flagLF == false) {
					_Location = _Location.NextLine();
				}
				else {
					flagLF = false;
				}
				break;
			default:
				flagLF = false;
				_Location = _Location.Next();
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
				return new NullToken(this, _Location);

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

			if (Array.IndexOf(LambdaSpaceToken, c) >= 0)
				return ReadLambdSpaceToken();

			if (kanaBegin <= c && c <= kanaEnd)
				return ReadPostPositionToken();

			if (Char.IsLetter(c))
				return ReadSymbolLetterToken();

			throw Error("未知のトークン「" + ((char)c) + "」が出現しました。");
		}

		private void SkipComment() {
			Debug.Assert(_CurrentChar == '※');
			int[] endChar = { '。', '.', '．' };
			int c = _NextChar();
			switch (c) {
			//case '(':
			//case '（':
			//    endChar = new int[] { ')', '）' };
			//    break;
			case '{':
			case '｛':
				endChar = new int[] { '}', '｝' };
				break;
			//case '[':
			//case '［':
			//    endChar = new int[] { ']', '］' };
			//    break;
			//case '「':
			//    endChar = new int[] { '」' };
			//    break;
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
			var startLoc = _Location;

			var buff = new StringBuilder();
			while (true) {
				int c = _NextChar();
				if (c == -1) {
					throw Error("\"「\"に対応する \"」\" が見つかりませんでした。");
				}
				else if (c == '」') {
					break;
				}
				buff.Append((char)c);
			}
			var range = new TextRange(startLoc, _Location);
			_NextChar();
			return new LiteralToken(this, range, buff.ToString());
		}

		private LiteralToken ReadNumberToken() {
			var startLoc = _Location;
			var buff = new StringBuilder();
			while (true) {
				int c = _CurrentChar;
				if ('0' <= c && c <= '9') {
					buff.Append((char)c);
				}
				else if ('０' <= c && c <= '９') {
					buff.Append((char)(c - '０' + '0'));
				}
				else {
					break;
				}
				_NextChar();
			}

			if (_CurrentChar == '.' || _CurrentChar == '．') {
				buff.Append('.');
				// 小数の処理
				while (true) {
					int c = _NextChar();
					if ('0' <= c && c <= '9') {
						buff.Append((char)c);
					}
					else if ('０' <= c && c <= '９') {
						buff.Append((char)(c - '０' + '0'));
					}
					else {
						break;
					}
				}
				var range = new TextRange(startLoc, _Location);
				double value = Double.Parse(buff.ToString());
				return new LiteralToken(this, range, value);
			}
			else {
				var range = new TextRange(startLoc, _Location);
				var numTxt = buff.ToString();

				int num;
				if (Int32.TryParse(numTxt, out num))
					return new LiteralToken(this, range, num);
				long numLong;
				if (Int64.TryParse(numTxt, out numLong))
					return new LiteralToken(this, range, numLong);
				BigInteger numBig;
				if (BigInteger.TryParse(numTxt, out numBig))
					return new LiteralToken(this, range, numBig);
				
				var value = BigInteger.Parse(numTxt);
			}
			throw Error("解析できない数値（" + buff.ToString() + "）です。");
		}

		private Token ReadLambdSpaceToken() {
			var startLoc = _Location;
			var buff = new StringBuilder();
			while (Array.IndexOf(LambdaSpaceToken, (char)_CurrentChar) >= 0) {
				buff.Append((char)_CurrentChar);
				_NextChar();
			}
			var range = new TextRange(startLoc, _Location);
			return new LambdaSpaceToken(this, range, buff.ToString());
		}

		private AbstractOperatorToken ReadOperatorToken() {
			var startLoc = _Location;
			var buff = new StringBuilder();
			buff.Append((char)_CurrentChar);
			while (true) {
				char c = (char)_NextChar();
				if (Array.IndexOf(OperatorCharacter, c) == -1)
					break;
				buff.Append(c);
			}
			var range = new TextRange(startLoc, _Location);
			var op = buff.ToString();

			switch (op) {
			case "+":
			case "＋":
				return new AddOpToken(this, range);
			case "-":
			case "－":
				return new SubOpToken(this, range);
			case "*":
			case "＊":
			case "×":
				return new MultipleOpToken(this, range);
			case "/":
			case "／":
			case "÷":
				return new DivideOpToken(this, range);
			case "%":
			case "％":
				return new ModuloOpToken(this, range);

			case "=":
			case "＝":
				return new EqualOpToken(this, range);
			case "!=":
			case "≠":
				return new NotEqualOpToken(this, range);
			case "<":
			case "＜":
				return new LessThanOpToken(this, range);
			case "<=":
			case "≦":
				return new LessThanEqualOpToken(this, range);
			case ">=":
			case "≧":
				return new GreaterThanEqualOpToken(this, range);
			case ">":
			case "＞":
				return new GreaterThanOpToken(this, range);

			case "&":
			case "＆":
			case "∧":
				return new AndOpToken(this, range);
			case "|":
			case "｜":
			case "∨":
				return new OrOpToken(this, range);
			case "!":
			case "！":
			case "￢":
				return new NotOpToken(this, range);

			case "…":
				return new ConcatOpToken(this, range);

			case ":":
			case "：":
			case "・":
				return new ConsOpToken(this, range);

			default:
				return new UnknownOperatorToken(this, range, op);
			}
		}

		private PunctuationToken ReadPunctuationToken() {
			var startLoc = _Location;

			char c = (char)_CurrentChar;
			_NextChar();
			var range = new TextRange(startLoc, _Location);
			switch (c) {
			case ',':
			case '，':
			case '、':
				return new CommaToken(this, range);
			case '。':
			case '．':
			case '.':
				return new PeriodToken(this, range);
			case ';':
			case '；':
				return new SemicolonToken(this, range);
			}
			Contract.Assert(false, "到達不可能" + Environment.NewLine + "プログラムを見直すこと。");
			throw new InvalidOperationException();
		}

		private Token ReadSymbolLetterToken() {
			var startLoc = _Location;

			var buff = new StringBuilder();
			buff.Append((char)_CurrentChar);
			while (true) {
				char c = (char)_NextChar();
				if (Char.IsLetter(c) && (c < kanaBegin || kanaEnd < c))
					buff.Append(c);
				else
					break;
			}
			string[] reserved = { ConstantNames.Defun,
				ConstantNames.BlockBegin, ConstantNames.BlockEnd, ConstantNames.BlockExec,
				ConstantNames.TrueText, ConstantNames.FalseText, ConstantNames.ElseText,
				ConstantNames.NullText };
			string str = buff.ToString();
			var range = new TextRange(startLoc, _Location);
			if (Array.IndexOf(reserved, str) >= 0)
				return new ReservedToken(this, range, str);
			else
				return new SymbolToken(this, range, str);
		}

		private Token ReadPostPositionToken() {
			var startLoc = _Location;
			var soLoc = new TextLocation();

			if (_CurrentChar == 'と') {
				_NextChar();
				var range = new TextRange(startLoc, _Location);
				return new SuffixToken(this, range, "と");
			}
			var buff = new StringBuilder();
			buff.Append((char)_CurrentChar);
			while (true) {
				char c = (char)_NextChar();
				if (kanaBegin <= c && c <= kanaEnd) {
					buff.Append(c);
					if (c == 'そ') {
						soLoc = _Location;
					}
				}
				else
					break;
			}
			string[] reserved = { "もし", "なら", ConstantNames.ReturnText, "してみて", "してみる", "して", "し", "する" };
			string str = buff.ToString();
			var fullRange = new TextRange(startLoc, _Location);
			if (Array.IndexOf(reserved, str) >= 0)
				return new ReservedToken(this, fullRange, str);

			const string mapKeyword = "それぞれ";
			if (str.EndsWith(mapKeyword)) {
				if (mapKeyword == str) {
					return new ReservedToken(this, fullRange, mapKeyword);
				}
				else {
					var sfxRange = new TextRange(startLoc, soLoc);
					var soRange = new TextRange(soLoc, _Location);
					_stack.Push(new ReservedToken(this, soRange, mapKeyword));
					var sfxTxt = str.Substring(0, str.Length - mapKeyword.Length);
					return new SuffixToken(this, sfxRange, sfxTxt);
				}
			}

			return new SuffixToken(this, fullRange, str);
		}


		private Token ReadBracketsToken() {
			var startLoc = _Location;
			char c = (char)_CurrentChar;
			_NextChar();
			var range = new TextRange(startLoc, _Location);
			switch (c) {
			case '(':
			case '（':
				return new OpenParenthesisToken(this, range);
			case ')':
			case '）':
				return new CloseParenthesisToken(this, range);
			case '[':
			case '［':
				return new OpenBracketToken(this, range);
			case ']':
			case '］':
				return new CloseBracketToken(this, range);
			case '{':
			case '｛':
				return new OpenBraceToken(this, range);
			case '}':
			case '｝':
				return new CloseBraceToken(this, range);
			case '【':
				return new OpenSumiBracketToken(this, range);
			case '】':
				return new CloseSumiBracketToken(this, range);
			}
			Contract.Assert(false, "到着不可能フロー" + Environment.NewLine + "プログラムを見直すこと。");
			throw new InvalidOperationException();
		}

		#endregion

		private LexicalException Error(string message) {
			return new LexicalException(message, _FileName, _Location);
		}

	}
}

