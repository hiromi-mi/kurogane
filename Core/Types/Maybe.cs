using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kurogane.Types {

	/// <summary>
	/// 失敗するかもしれない結果を示すクラス。
	/// </summary>
	/// <typeparam name="T">結果の値</typeparam>
	public interface Maybe<out T> : IInspectable {
		T Value { get; }

		Maybe<TResult> Execute<TResult>(Func<T, Maybe<TResult>> func);
	}

	/// <summary>
	/// 失敗を表すクラス。Singleton
	/// </summary>
	public sealed class Nothing<T> : Maybe<T> {

		public static readonly Nothing<T> Instance = new Nothing<T>();

		public T Value {
			get { throw new InvalidOperationException("value is nothing"); }
		}

		public Maybe<TResult> Execute<TResult>(Func<T, Maybe<TResult>> func) {
			return Nothing<TResult>.Instance;
		}

		public string Inspect() {
			return "-";
		}
	}

	/// <summary>
	/// 成功を表すクラス。
	/// </summary>
	/// <typeparam name="T">値の型</typeparam>
	public class Just<T> : Maybe<T> {
		public T Value { get; private set; }
		public Just(T value) {
			this.Value = value;
		}

		public Maybe<TResult> Execute<TResult>(Func<T, Maybe<TResult>> func) {
			return func(Value);
		}

		public string Inspect() {
			var i = Value as IInspectable;
			if (i != null)
				return i.Inspect();
			else
				return Value.ToString();
		}
	}
}
