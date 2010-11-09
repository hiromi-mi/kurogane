using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using Kurogane.Util;

namespace Kurogane {

	/// <summary>SuffixFuncのファクトリクラス</summary>
	public static class SuffixFunc {

		/// <summary>無限ループを起こさないように一つメソッドを作成</summary>
		private static SuffixFunc<T> _Create<T>(T func, params string[] suffix) {
			return new SuffixFunc<T>(func, suffix);
		}

		public static SuffixFunc<T> Create<T>(T func, params string[] suffix) {
			return _Create(func, suffix);
		}

		// ----- ----- ----- ----- ----- Generic Func ----- ----- ----- ----- -----

		public static SuffixFunc<Func<TResult>> Create<TResult>(Func<TResult> func) {
			return _Create(func);
		}

		public static SuffixFunc<Func<T1, TResult>> Create<T1, TResult>(Func<T1, TResult> func, string suffix) {
			return _Create(func, suffix);
		}

		public static SuffixFunc<Func<T1, T2, TResult>> Create<T1, T2, TResult>(Func<T1, T2, TResult> func, string suffix1, string suffix2) {
			return _Create(func, suffix1, suffix2);
		}

		public static SuffixFunc<Func<T1, T2, T3, TResult>> Create<T1, T2, T3, TResult>(
			Func<T1, T2, T3, TResult> func, string suffix1, string suffix2, string suffix3) {
			return _Create(func, suffix1, suffix2, suffix3);
		}

		public static SuffixFunc<Func<T1, T2, T3, T4, TResult>> Create<T1, T2, T3, T4, TResult>(
			Func<T1, T2, T3, T4, TResult> func, string suffix1, string suffix2, string suffix3, string suffix4) {
			return _Create(func, suffix1, suffix2, suffix3, suffix4);
		}

		// ----- ----- ----- ----- ----- Object Func ----- ----- ----- ----- -----

		public static SuffixFunc<Func<object>> Create(Func<object> func) {
			return _Create(func);
		}

		public static SuffixFunc<Func<object, object>> Create(Func<object, object> func, string suffix1, string suffix2) {
			return _Create(func, suffix1, suffix2);
		}

		public static SuffixFunc<Func<object, object, object>> Create(
			Func<object, object, object> func, string suffix1, string suffix2, string suffix3) {
			return _Create(func, suffix1, suffix2, suffix3);
		}

		public static SuffixFunc<Func<object, object, object, object>> Create(
			Func<object, object, object, object> func, string suffix1, string suffix2, string suffix3) {
			return _Create(func, suffix1, suffix2, suffix3);
		}

		internal static Expression SortAndInvoke(Expression func, string[] prmSfx, CallInfo cInfo, Func<int, Expression> argArray) {
			int offset = cInfo.ArgumentCount - cInfo.ArgumentNames.Count;
			if (offset >= 2)
				return ThrowArgumentException("助詞無しで渡された引数が多すぎます。");

			// ordering parameters
			var parameters = new Expression[prmSfx.Length];
			int prmR = prmSfx.Length - 1;
			bool offsetUsed = offset == 1 ? false : true; // Offsetを引数として利用したら立てる
			while (prmR >= 0) {
				// 仮引数の範囲を決定
				var sfx = prmSfx[prmR];
				int prmL = prmR - 1;
				while (prmL >= 0 && prmSfx[prmL] == "と") prmL--;
				var paramLen = prmR - prmL;
				// 実引数の範囲を決定
				var argSfx = cInfo.ArgumentNames;
				int argR = argSfx.Count - 1;
				while (argR >= 0 && argSfx[argR] != sfx) argR--;
				// 暗黙引数が引数になるのは1回まで（のチェック）
				if (argR < 0) {
					if (offsetUsed)
						break;
					offsetUsed = true;
				}
				int argL = argR - 1;
				while (argL >= 0 && argSfx[argL] == "と") argL--;
				int argLen = argR - argL;
				// arg->param へ 移行
				if (paramLen == argLen) {
					for (int i = 1; i <= argLen; i++) {
						parameters[prmL + i] = Wrap( argArray(argL + i + offset));
					}
				}
				else if (paramLen > argLen) {
					// arg を param に展開
					break;
					throw new NotImplementedException();
				}
				else /* paramLen < argLen */ {
					for (int i = 2; i <= paramLen; i++) {
						parameters[prmL + i] = Wrap(argArray(argL + i + offset));
					}
					// arg を param に集約
					int count = argLen - paramLen;

					Expression listExpr = Wrap(argArray(argL + 1 + count + offset));
					var ctorInfo = typeof(Tuple<object, object>).GetConstructor(new[] { typeof(object), typeof(object) });
					while (count-- > 0) {
						listExpr = Expression.New(
							ctorInfo,
							Wrap(argArray(argL + 1 + count + offset)),
							listExpr);
					}
					parameters[prmL + 1] = listExpr;
				}
				prmR = prmL;
			}
			// 正しく並びかえられたかチェック
			foreach (var p in parameters)
				if (p == null)
					return ThrowArgumentException(
						"引数または助詞が合っていません。" + Environment.NewLine +
						"呼び出し先の助詞： " + String.Join("、", prmSfx) + Environment.NewLine +
						"呼び出し元の助詞： " + String.Join("、", cInfo.ArgumentNames));
			return Expression.Invoke(func, parameters);
		}

		private static Expression ThrowArgumentException(string message) {
			var ctorInfo = typeof(ArgumentException).GetConstructor(new[] { typeof(string) });
			return Expression.Throw(Expression.New(ctorInfo, Expression.Constant(message)), typeof(object));
		}

		private static Expression Wrap(Expression expr, Type type) {
			if (expr.Type == type)
				return expr;
			else
				return Expression.Convert(expr, type);
		}

		private static Expression Wrap(Expression expr) {
			return Wrap(expr, typeof(object));
		}
	}

	public class SuffixFunc<T> : IDynamicMetaObjectProvider {

		private const string Separator = "|";

		/// <summary>
		/// Genericな型 T がデリゲート型かどうか。
		/// </summary>
		private static readonly bool IsValidType = typeof(T).IsSubclassOf(typeof(Delegate));

		/// <summary>
		/// Tの型を分解した型。
		/// 配列の0番目には戻り値の型が入る。
		/// </summary>
		private static readonly Type[] Types;

		static SuffixFunc() {
			var mInfo = typeof(T).GetMethod("Invoke");
			if (mInfo == null) {
				Types = null;
				return;
			}
			var pInfos = mInfo.GetParameters();
			var types = new Type[pInfos.Length + 1];
			Types = types;
			types[0] = mInfo.ReturnType;
			for (int i = 0; i < pInfos.Length; i++)
				types[i + 1] = pInfos[i].ParameterType;
		}

		/// <summary>助詞をSeparatorで連結したもの</summary>
		public readonly string Suffix;

		/// <summary>関数</summary>
		public readonly T Func;

		/// <summary>
		/// 通常のコンストラクタ
		/// </summary>
		/// <param name="func"></param>
		/// <param name="suffix"></param>
		public SuffixFunc(T func, params string[] suffix) {
			if (IsValidType == false)
				throw new ArgumentException("型" + typeof(T).Name + "はデリゲート型ではありません。");
			if (suffix.Length != Types.Length - 1)
				throw new ArgumentException("引数の数と助詞の数が一致していません。");

			this.Func = func;
			this.Suffix = String.Intern(String.Join(Separator, suffix));
		}

		public override string ToString() {
			var sfxs = Suffix.Split(new[] { Separator }, StringSplitOptions.RemoveEmptyEntries);
			const string blank = "～";
			return blank + String.Join(blank, sfxs) + blank + "する。";
		}

		#region IDynamicMetaObjectProvider メンバー

		DynamicMetaObject IDynamicMetaObjectProvider.GetMetaObject(Expression parameter) {
			return new MetaObject(this, parameter);
		}

		private class MetaObject : DynamicMetaObject {

			private new SuffixFunc<T> Value { get { return (SuffixFunc<T>)base.Value; } }
			private new Expression Expression { get { return Expression.Convert(base.Expression, typeof(SuffixFunc<T>)); } }

			public MetaObject(SuffixFunc<T> self, Expression expr)
				: base(expr, BindingRestrictions.GetTypeRestriction(expr, self.GetType()), self) {
			}

			public override DynamicMetaObject BindInvoke(InvokeBinder binder, DynamicMetaObject[] args) {
				// 入力チェック
				var cInfo = binder.CallInfo;
				int offset = cInfo.ArgumentCount - cInfo.ArgumentNames.Count;
				if (offset >= 2)
					return ThrowArgumentException("助詞無しで渡された引数が多すぎます。");

				string[] prmSfx = Value.Suffix.Split(new[] { Separator }, StringSplitOptions.RemoveEmptyEntries);
				// ordering parameters
				var parameters = new Expression[prmSfx.Length];
				int prmR = prmSfx.Length - 1;
				bool offsetUsed = offset == 1 ? false : true; // Offsetを引数として利用したら立てる
				while (prmR >= 0) {
					// 仮引数の範囲を決定
					var sfx = prmSfx[prmR];
					int prmL = prmR - 1;
					while (prmL >= 0 && prmSfx[prmL] == "と") prmL--;
					var paramLen = prmR - prmL;
					// 実引数の範囲を決定
					var argSfx = cInfo.ArgumentNames;
					int argR = argSfx.Count - 1;
					while (argR >= 0 && argSfx[argR] != sfx) argR--;
					// 暗黙引数が引数になるのは1回まで（のチェック）
					if (argR < 0) {
						if (offsetUsed)
							break;
						offsetUsed = true;
					}
					int argL = argR - 1;
					while (argL >= 0 && argSfx[argL] == "と") argL--;
					int argLen = argR - argL;
					// arg->param へ 移行
					if (paramLen == argLen) {
						for (int i = 1; i <= argLen; i++)
							parameters[prmL + i] = Expression.Convert(args[argL + i + offset].Expression, Types[prmL + i + 1]);
					}
					else if (paramLen > argLen) {
						// arg を param に展開
						break;
						throw new NotImplementedException();
					}
					else /* paramLen < argLen */ {
						for (int i = 2; i <= paramLen; i++)
							parameters[prmL + i] = Expression.Convert(args[argL + i + offset].Expression, Types[prmL + i + 1]);
						// arg を param に集約
						int count = argLen - paramLen;
						Expression listExpr = Expression.Convert(args[argL + 1 + count + offset].Expression, typeof(object));
						var ctorInfo = typeof(Tuple<object, object>).GetConstructor(new[] { typeof(object), typeof(object) });
						while (count-- > 0) {
							listExpr = Expression.New(
								ctorInfo,
								Expression.Convert(args[argL + 1 + count + offset].Expression, typeof(object)),
								listExpr);
						}
						parameters[prmL + 1] = listExpr;
					}
					prmR = prmL;
				}
				// 正しく並びかえられたかチェック
				foreach (var p in parameters)
					if (p == null)
						return ThrowArgumentException(
							"引数または助詞が合っていません。" + Environment.NewLine +
							"呼び出し先の助詞： " + String.Join("、", prmSfx) + Environment.NewLine +
							"呼び出し元の助詞： " + String.Join("、", cInfo.ArgumentNames));
				// create bindings
				string propName = ReflectionHelper.PropertyName((SuffixFunc<T> func) => func.Func);
				var funcExpr = Expression.PropertyOrField(this.Expression, propName);
				Expression expr = Expression.Invoke(funcExpr, parameters);
				if (Types[0].IsValueType)
					expr = Expression.Convert(expr, typeof(object));
				return new DynamicMetaObject(expr, GetRestrictions());
			}

			/// <summary>SuffixFuncのGeneric型と助詞からBindingRestrictionsを作成する。</summary>
			private BindingRestrictions GetRestrictions() {
				string propName = ReflectionHelper.PropertyName((SuffixFunc<T> func) => func.Suffix);
				var equalExpr = Expression.Equal(Expression.Field(this.Expression, propName), Expression.Constant(Value.Suffix));
				var typeExpr = Expression.TypeIs(base.Expression, this.Value.GetType());
				var combine = Expression.AndAlso(typeExpr, equalExpr);
				return BindingRestrictions.GetExpressionRestriction(combine);
			}

			/// <summary>ArgumentExceptionを投げる式木を返す。</summary>
			/// <param name="message">例外のメッセージ</param>
			private DynamicMetaObject ThrowArgumentException(string message) {
				return new DynamicMetaObject(
					Expression.Throw(Expression.New(
							typeof(ArgumentException).GetConstructor(new[] { typeof(string) }),
							Expression.Constant(message)),
						typeof(object)),
					GetRestrictions());
			}
		}

		#endregion

		public static explicit operator Delegate(SuffixFunc<T> func) {
			return func.Func as Delegate;
		}
	}
}
