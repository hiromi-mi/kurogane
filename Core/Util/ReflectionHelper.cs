using System;
using System.Linq.Expressions;
using System.Reflection;
using System.Diagnostics.Contracts;

namespace Kurogane.Util {

	internal static class ReflectionHelper {

		public static string PropertyName<T>(Expression<Func<T, object>> expr) {
			Contract.Requires<ArgumentNullException>(expr != null);
			var body = expr.Body;
			var unary = body as UnaryExpression;
			if (unary != null)
				body = unary.Operand;
			return ((MemberExpression)body).Member.Name;
		}

		public static PropertyInfo PropertyInfo<T>(Expression<Func<T, object>> expr) {
			Contract.Requires<ArgumentNullException>(expr != null);
			var body = expr.Body;
			var unary = body as UnaryExpression;
			if (unary != null)
				body = unary.Operand;
			return ((MemberExpression)body).Member as PropertyInfo;
		}

		public static MemberExpression MemberExpression<T>(Expression<Func<T, object>> expr) {
			Contract.Requires<ArgumentNullException>(expr != null);
			return (MemberExpression)expr.Body;
		}

		/// <summary>
		/// 型インスタンスから、引数および、戻り値の型の配列を返す。
		/// 戻り値の型が配列の0番目になる。
		/// 引数のデリゲート型のインスタンスでない場合、nullを返す。
		/// </summary>
		/// <param name="type">デリゲート型の型インスタンス</param>
		/// <returns></returns>
		public static Type[] GetDelegateType(Type type) {
			Contract.Requires<ArgumentNullException>(type != null);
			Contract.Ensures(Contract.Result<Type[]>() == null || Contract.ForAll(Contract.Result<Type[]>(), t => t != null));
			bool isDelegate = typeof(Delegate).IsAssignableFrom(type);
			if (isDelegate == false)
				return null;
			var mInfo = type.GetMethod("Invoke");
			if (mInfo == null) {
				return null;
			}
			var pInfos = mInfo.GetParameters();
			var types = new Type[pInfos.Length + 1];
			for (int i = 0; i < pInfos.Length; i++)
				types[i] = pInfos[i].ParameterType;
			types[types.Length - 1] = mInfo.ReturnType;
			return types;
		}

	}
}
