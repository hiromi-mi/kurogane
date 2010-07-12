using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.Dynamic;
using System.Linq.Expressions;

namespace Kurogane.Buildin {

	public class KrgnFunc {
		/// <summary>
		/// 実行するデリゲート
		/// </summary>
		public Delegate Function { get; private set; }

		private readonly string[] _suffixes;

		/// <summary>
		/// この手順に属する助詞の配列
		/// </summary>
		public ReadOnlyCollection<string> Suffixes {
			get { return Array.AsReadOnly(_suffixes); }
		}

		public KrgnFunc(Delegate func, params string[] suffixes) {
			_suffixes = suffixes;
			Function = func;
		}
	}
}
