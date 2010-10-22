using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using Kurogane.RuntimeBinder;
using Kurogane.Util;

namespace Kurogane.Compiler {

	/// <summary>
	/// ブロックを作成するための，Generator。
	/// ローカル変数を持てる。
	/// </summary>
	internal class BlockGenerator : Generator {

		private readonly Generator _Parent;
		private readonly IDictionary<string, ParameterExpression> _FuncVariable;
		private readonly IDictionary<string, ParameterExpression> _LocalVariables = new Dictionary<string, ParameterExpression>();

		public BlockGenerator(BinderFactory factory, Generator parent, IDictionary<string, ParameterExpression> func)
			: base(factory) {
			_Parent = parent;
			_FuncVariable = func;
			this.Global = parent.Global;
		}

		public override Expression ConvertBlock(Block block) {
			return Expression.Block(
				_LocalVariables.Values,
				base.ConvertBlock(block));
		}

		public override Expression ConvertSymbol(string name) {
			ParameterExpression expr;
			if (_LocalVariables.TryGetValue(name, out expr))
				return expr;
			if (_FuncVariable.TryGetValue(name, out expr))
				return expr;
			return _Parent.ConvertSymbol(name);
		}

		public override Expression ConvertDefun(Defun defun) {
			var name = defun.Name;
			if (_LocalVariables.ContainsKey(name))
				throw new SemanticException("変数「" + name + "」が二度定義されています。");
			var funcType = ReflectionHelper.TypeOfFunc[defun.Params.Count];
			var sfxFuncType = typeof(SuffixFunc<>).MakeGenericType(funcType);
			var funcExpr = Expression.Parameter(sfxFuncType);
			_LocalVariables[name] = funcExpr;
			return ConvertDefunCore(defun, funcExpr);
		}

		public override Expression ConvertDefineValue(DefineValue defineValue, ref Expression lastExpr) {
			var name = defineValue.Name;
			if (_LocalVariables.ContainsKey(name))
				throw new SemanticException("変数「" + name + "」が二度定義されています。");

			Expression valueExpr;
			if (defineValue.Value == null) {
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
