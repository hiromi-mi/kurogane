using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kurogane.RuntimeBinder;
using System.Linq.Expressions;

namespace Kurogane.Compiler {
	public class LambdaGenerator : Generator{

		protected readonly Generator _parent;
		private readonly List<KeyValuePair<string, ParameterExpression>> _parameters = new List<KeyValuePair<string, ParameterExpression>>();

		public LambdaGenerator(BinderFactory factory, Generator parent)
			: base(factory) {
				_parent = parent;
		}

		protected override Expression ConvertLambda(Lambda lambda) {
			throw new SemanticException("ラムダ式の中でさらにラムダ式を利用することはできません。");
		}

		public LambdaExpression ConvertLambdaCore(Lambda lambda) {
			var elem = ConvertElement(lambda.Element);
			if (_parameters.Count == 0)
				throw new SemanticException("ラムダ式に引数がありません。");
			return Expression.Lambda(elem, _parameters.Select(pair => pair.Value));
		}

		public override Expression ConvertSymbol(string name) {
			return _parent.ConvertSymbol(name);
		}

		protected override Expression ConvertLiteral(Element lit) {
			if (lit is LambdaParameter)
				return ConvertLambdaParameter((LambdaParameter)lit);
			return base.ConvertLiteral(lit);
		}

		private ParameterExpression ConvertLambdaParameter(LambdaParameter param) {
			var name = param.Name;
			foreach (var pair in _parameters)
				if (pair.Key == name)
					return pair.Value;
			var expr = Expression.Parameter(typeof(object), name);
			_parameters.Add(new KeyValuePair<string,ParameterExpression>(name, expr));
			return expr;
		}
	}
}
