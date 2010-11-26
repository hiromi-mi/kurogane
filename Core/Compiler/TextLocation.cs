using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;

namespace Kurogane.Compiler {
	
	/// <summary>
	/// テキスト中の位置を表すクラス。
	/// </summary>
	public struct TextLocation : IComparable<TextLocation>, IEquatable<TextLocation>{

		/// <summary>行番号</summary>
		public readonly int Line;

		/// <summary>文字の位置</summary>
		public readonly int Column;

		public TextLocation(int line, int column) {
			this.Line = line;
			this.Column = column;
		}

		/// <summary>
		/// 次の文字の位置
		/// </summary>
		public TextLocation Next() {
			return new TextLocation(this.Line, this.Column + 1);
		}

		/// <summary>
		/// 次の行の位置
		/// </summary>
		public TextLocation NextLine() {
			return new TextLocation(this.Line + 1, 0);
		}

		#region IEquatable<TextLocation> + IComparable<TextLocation> メンバー

		public bool Equals(TextLocation other) {
			return this == other;
		}

		public override bool Equals(object obj) {
			return (obj is TextLocation) && this.Equals((TextLocation)obj);
		}

		public override int GetHashCode() {
			return (Line << 16) + Column;
		}

		public static bool operator ==(TextLocation left, TextLocation right) {
			return (left.Line == right.Line) && (left.Column == right.Column);
		}

		public static bool operator !=(TextLocation left, TextLocation right) {
			return !(left == right);
		}

		public int CompareTo(TextLocation other) {
			const int Huge = 65536;
			return
				(this.Line - other.Line) * Huge + (this.Column - other.Column);
		}

		public static bool operator <(TextLocation left, TextLocation right) {
			return left.CompareTo(right) < 0;
		}

		public static bool operator >(TextLocation left, TextLocation right) {
			return left.CompareTo(right) > 0;
		}

		public static bool operator <=(TextLocation left, TextLocation right) {
			return left.CompareTo(right) <= 0;
		}

		public static bool operator >=(TextLocation left, TextLocation right) {
			return left.CompareTo(right) >= 0;
		}

		#endregion
	}

	/// <summary>
	/// テキスト中のある範囲を表すクラス。
	/// </summary>
	public struct TextRange {
		public readonly TextLocation Start;
		public readonly TextLocation End;

		public TextRange(TextLocation start, TextLocation end) {
			Contract.Requires<ArgumentOutOfRangeException>(start <= end);
			this.Start = start;
			this.End = end;
		}
	}
}
