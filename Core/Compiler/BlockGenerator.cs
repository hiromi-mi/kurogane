using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using Kurogane.RuntimeBinder;

namespace Kurogane.Compiler {

	/// <summary>
	/// ブロックを作成するための，Generator。
	/// ローカル変数を持てる。
	/// </summary>
	internal class BlockGenerator : Generator {

		private readonly Generator _Parent;
		private readonly IDictionary<string, ParameterExpression> _Func;
		private readonly Dictionary<string, ParameterExpression> _LocalVariables = new Dictionary<string, ParameterExpression>();

		public BlockGenerator(BinderFactory factory, Generator parent, IDictionary<string, ParameterExpression> func)
			: base(factory) {
			_Parent = parent;
			_Func = func;
			this.Global = parent.Global;
		}

		public override BlockExpression ConvertBlock(Block block) {
			var expr = base.ConvertBlock(block);
			return Expression.Block(
				_LocalVariables.Values,
				expr.Expressions);
		}

		public override Expression ConvertSymbol(string name) {
			ParameterExpression expr;
			if (_LocalVariables.TryGetValue(name, out expr))
				return expr;
			else
				return _Parent.ConvertSymbol(name);
		}

		public override Expression ConvertDefun(Defun defun) {
			throw new NotImplementedException();
			//return base.ConvertDefun(defun);
		}

		public override Expression ConvertDefineValue(DefineValue defineValue, ref Expression lastExpr) {
			var name = defineValue.Name;
			if (_LocalVariables.ContainsKey(name))
				throw new SemanticException("変数「" + defineValue.Name + "」が二度定義されています。");

			Expression valueExpr;
			if (defineValue == null) {
				if (lastExpr != null) {
					valueExpr = lastExpr;
					lastExpr = null;
				}
				else {
					throw new SemanticException("代入する値が見つかりません。");
				}
			}
			else {
				valueExpr = ConvertElement(defineValue.Value);
			}
			var variable = Expression.Variable(valueExpr.Type, name);
			_LocalVariables[name] = variable;
			return Expression.Assign(variable, valueExpr);
		}

	}
}
