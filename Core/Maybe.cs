﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kurogane {

	/// <summary>
	/// 失敗するかもしれない結果を示すクラス。
	/// </summary>
	/// <typeparam name="T">結果の値</typeparam>
	public interface Maybe<out T> {
		T Value { get; }

		Maybe<TResult> Execute<TResult>(Func<T, Maybe<TResult>> func);
	}

	/// <summary>
	/// 失敗を表すクラス。
	/// </summary>
	public sealed class Nothing<T> : Maybe<T>, Nothing, IEquatable<Nothing<T>> {

		public static readonly Nothing<T> Instance = new Nothing<T>();

		public T Value {
			get { throw new KrgnException("value is nothing"); }
		}

		public Maybe<TResult> Execute<TResult>(Func<T, Maybe<TResult>> func) {
			return Nothing<TResult>.Instance;
		}

		public bool Equals(Nothing<T> other) {
			return other != null;
		}

		public override int GetHashCode() {
			return typeof(T).GetHashCode();
		}
	}

	public interface Nothing { }

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
	}
}
