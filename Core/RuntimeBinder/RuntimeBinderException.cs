using System.Dynamic;
using System.Linq.Expressions;

namespace Kurogane.RuntimeBinder {

	/// <summary>
	/// KrgnRuntimeBinderで動的バインドが処理されたときに発生するエラーを表します。
	/// </summary>
	public class RuntimeBinderException : KrgnRuntimeException {

		/// <summary>例外を投げる式木を作成します。</summary>
		/// <param name="errorMessage">例外のメッセージ</param>
		/// <param name="restrictions">Bindingでの制限</param>
		public static DynamicMetaObject CreateMetaObject(string errorMessage, BindingRestrictions restrictions) {
			var ctorInfo = typeof(RuntimeBinderException).GetConstructor(new[] { typeof(string) });
			return new DynamicMetaObject(
				Expression.Throw(Expression.New(ctorInfo, Expression.Constant(errorMessage)), typeof(object)),
				restrictions);
		}

		/// <summary>
		/// 通常のコンストラクタ
		/// </summary>
		public RuntimeBinderException(string message)
			: base(message) {
		}
	}
}
