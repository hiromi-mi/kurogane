using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Kurogane.Types {
	public class SuffixFunc<T> : IDynamicMetaObjectProvider {

		private const string Separator = "|";

		/// <summary>型パラメタ T が有効な型かどうか</summary>
		private static readonly bool IsValidType;

		/// <summary>助詞をSeparatorで連結したもの</summary>
		public readonly string Suffix;

		/// <summary>関数</summary>
		public readonly T Func;

		static SuffixFunc() {
			IsValidType = ReflectionHelper.TypeOfFunc.Contains(typeof(T));
		}

		public SuffixFunc(T func, params string[] suffix) {
			if (IsValidType == false) {
				throw new InvalidOperationException(typeof(T) + "には対応していません。");
			}
			this.Func = func;
			this.Suffix = String.Intern(String.Join(Separator, suffix));
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

				string[] prmSfx = Value.Suffix.Split(new[] { Separator }, StringSplitOptions.None);
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
						for (int i = 0; i < argLen; i++)
							parameters[prmL + i + 1] = Expression.Convert(args[argL + i + 1 + offset].Expression, typeof(object));
					}
					else if (paramLen > argLen) {
						for (int i = 0; i < argLen - 1; i++)
							parameters[prmL + 1] = Expression.Convert(args[argL + 1 + offset].Expression, typeof(object));
						// arg を param に展開
						throw new NotImplementedException();
					}
					else /* paramLen < argLen */ {
						for (int i = 0; i < paramLen - 1; i++)
							parameters[prmL + 1] = Expression.Convert(args[argL + 1 + offset].Expression, typeof(object));
						// arg を param に集約
						throw new NotImplementedException();
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
				return new DynamicMetaObject(Expression.Invoke(funcExpr, parameters), GetRestrictions());
			}

			private BindingRestrictions GetRestrictions() {
				string propName = ReflectionHelper.PropertyName((SuffixFunc<T> func) => func.Suffix);
				var equalExpr = Expression.Equal(Expression.Field(this.Expression, propName), Expression.Constant(Value.Suffix));
				var typeExpr = Expression.TypeIs(base.Expression, this.Value.GetType());
				var combine = Expression.AndAlso(typeExpr, equalExpr);
				return BindingRestrictions.GetExpressionRestriction(combine);
			}

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
	}
}
