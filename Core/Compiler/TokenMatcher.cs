using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace Kurogane.Compiler {

	/// <summary>
	/// TokenをMaybeMonadのようにテストするための拡張クラス
	/// </summary>
	public static class TokenMatcher {

		/// <summary>
		/// トークンに対して，与えられた関数が真を返す時，次のトークンを返す。
		/// 失敗した場合は，nullを返す。
		/// </summary>
		/// <param name="token">調べるトークン</param>
		/// <param name="pred">判定する関数</param>
		/// <returns>次のトークン</returns>
		public static T MatchFlow<T>(this T token, Predicate<T> pred) where T : class, IToken<T> {
			return MatchFlow<T, T>(token, pred);
		}

		/// <summary>
		/// トークンに対して，与えられた関数が真を返す時，次のトークンを返す。
		/// 失敗した場合は，nullを返す。
		/// </summary>
		/// <param name="token">調べるトークン</param>
		/// <param name="pred">判定する関数</param>
		/// <returns>次のトークン</returns>
		public static TToken MatchFlow<TToken, T>(this TToken token, Predicate<T> pred)
			where T : TToken
			where TToken : class, IToken<TToken> {

				return Match(token, pred) ? token.Next : null;
		}

		/// <summary>
		/// トークンが与えられた関数にマッチするかどうか。
		/// </summary>
		/// <param name="token">調べるトークン</param>
		/// <param name="pred">判定する関数</param>
		/// <returns>真偽</returns>
		public static bool Match<T>(this T token, Predicate<T> pred) where T : class, IToken<T> {
			return Match<T, T>(token, pred);
		}

		/// <summary>
		/// トークンが与えられた関数にマッチするかどうか。
		/// </summary>
		/// <param name="token">調べるトークン</param>
		/// <param name="pred">判定する関数</param>
		/// <returns>真偽</returns>
		public static bool Match<TToken, T>(this TToken token, Predicate<T> pred)
			where T : TToken
			where TToken : class, IToken<TToken> {

			Debug.Assert(pred != null, "pred is null");
			if (token != null && token is T)
				return pred((T)token);
			return false;
		}

		/// <summary>
		/// トークンが与えられた関数にマッチするかどうか。
		/// </summary>
		/// <param name="token">調べるトークン</param>
		/// <param name="pred">判定する関数</param>
		/// <returns>真偽</returns>
		public static bool MatchFinish<T>(this T token, Predicate<T> pred) where T : class, IToken<T> {
			return MatchFinish<T, T>(token, pred);
		}

		/// <summary>
		/// トークンが与えられた関数にマッチするかどうか。
		/// </summary>
		/// <param name="token">調べるトークン</param>
		/// <param name="pred">判定する関数</param>
		/// <returns>真偽</returns>
		public static bool MatchFinish<TToken, T>(this TToken token, Predicate<T> pred)
			where T : TToken
			where TToken : class, IToken<TToken> {

			return Match(token, pred) && token.Next == null;
		}
	}
}
