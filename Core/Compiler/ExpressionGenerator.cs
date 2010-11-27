using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using Kurogane.RuntimeBinder;
using Kurogane.Util;

namespace Kurogane.Compiler {

	public static class ExpressionGenerator {

		public static Expression<Func<Scope, object>> Generate(Block block, BinderFactory factory, string fileName) {
			Contract.Requires<ArgumentNullException>(block != null);
			Contract.Requires<ArgumentNullException>(factory != null);
			Contract.Requires<ArgumentNullException>(fileName != null);

			var global = Expression.Parameter(typeof(Scope), "global");
			var gen = new GlobalGen(fileName, factory, global);
			Expression expr = gen.Generate(block);
			if (expr.Type != typeof(object))
				expr = Expression.Convert(expr, typeof(object));
			return Expression.Lambda<Func<Scope, object>>(expr, global);
		}

		public static Expression<Func<Scope, object>> GenerateAll(BinderFactory factory, IEnumerable<Tuple<Block, string>> programs) {
			Contract.Requires<ArgumentNullException>(factory != null);
			Contract.Requires<ArgumentNullException>(programs != null);

			var global = Expression.Parameter(typeof(Scope), "global");
			var exprs =
				from pair in programs/*.AsParallel()*/
				let gen = new GlobalGen(pair.Item2, factory, global)
				select gen.Generate(pair.Item1);
			Expression block = Expression.Block(exprs);
			if (block.Type != typeof(object))
				block = Expression.Convert(block, typeof(object));
			return Expression.Lambda<Func<Scope, object>>(block, global);
		}

		#region InnerClass

		private class ParamList {
			public ParameterExpression Parameter { get; private set; }
			public ParamList Next { get; private set; }

			public ParamList(ParameterExpression param, ParamList next) {
				Contract.Requires<ArgumentNullException>(param != null);
				this.Parameter = param;
				this.Next = next;
			}

			[Pure]
			public bool Contains(ParameterExpression item) {
				return Search(this, p => p == item) != null;
			}

			[ContractInvariantMethod]
			private void ObjectInvaliant() {
				Contract.Invariant(Parameter != null);
			}

			#region static

			public static ParamList FromEnum(IEnumerable<ParameterExpression> param) {
				using (var iter = param.GetEnumerator()) {
					return FromEnum(iter);
				}
			}

			private static ParamList FromEnum(IEnumerator<ParameterExpression> iter) {
				if (iter.MoveNext()) {
					var head = iter.Current;
					var tail = FromEnum(iter);
					return new ParamList(head, tail);
				}
				else {
					return null;
				}
			}

			[Pure]
			public static IEnumerable<ParameterExpression> ToEnum(ParamList list) {
				while (list != null) {
					yield return list.Parameter;
					list = list.Next;
				}
			}

			[Pure]
			public static ParameterExpression Search(ParamList list, Predicate<ParameterExpression> pred) {
				for (var search = list; search != null; search = search.Next)
					if (pred(search.Parameter))
						return search.Parameter;
				return null;
			}

			[Pure]
			public static ParamList Merge(ParamList left, ParamList right) {
				if (right == null) return left;
				for (; left != null; left = left.Next) {
					bool find = right.Contains(left.Parameter);
					if (find == false)
						right = new ParamList(left.Parameter, right);
				}
				return right;
			}

			[Pure]
			public static ParamList Intersect(ParamList left, ParamList right) {
				if (right == null) return right;
				ParamList result = null;
				for (; left != null; left = left.Next) {
					if (right.Contains(left.Parameter))
						result = new ParamList(left.Parameter, result);
				}
				return result;
			}

			#endregion

		}

		private class ExprParamPair {
			public Expression Expression { get; private set; }
			public ParamList Parameters { get; private set; }
			public ExprParamPair(Expression expr, ParamList paramList) {
				Contract.Requires<ArgumentNullException>(expr != null);
				Contract.Requires<ArgumentException>(expr.Type != typeof(void));
				this.Expression = expr;
				this.Parameters = paramList;
			}

			[ContractInvariantMethod]
			private void ObjectInvariant() {
				Contract.Invariant(Expression != null);
				Contract.Invariant(Expression.Type != typeof(void));
			}
		}

		private abstract class StmtGen {

			// ===== ===== ===== ===== ===== fields ===== ===== ===== ===== =====
			protected static readonly Expression NullExpr = Expression.Constant(null);

			// ===== ===== ===== ===== ===== property ===== ===== ===== ===== =====
			protected ElemGen ElemGen { get; private set; }
			protected LabelTarget Target { get; private set; }

			// ----- ----- ----- ----- abstract & virtual ----- ----- ----- -----
			public abstract StmtGen Parent { get; }

			public virtual ParameterExpression Global { get { return Parent.Global; } }
			public virtual BinderFactory Factory { get { return Parent.Factory; } }
			public virtual SymbolDocumentInfo DocumentInfo { get { return Parent.DocumentInfo; } }
			public virtual string FileName { get { return Parent.FileName; } }

			// ===== ===== ===== ===== ===== ctor ===== ===== ===== ===== =====
			internal StmtGen() { ElemGen = new ElemGen(this); }

			// ===== ===== ===== ===== ===== method ===== ===== ===== ===== =====

			// ----- ----- ----- ----- abstract  ----- ----- ----- -----
			public abstract Expression GetValue(string name);

			private LabelTarget GetTarget() {
				Contract.Ensures(Contract.Result<LabelTarget>() != null);
				return Target ?? (Target = Expression.Label(typeof(object), "#return"));
			}

			#region Statement

			protected ExprParamPair GenStmt(Statement stmt, ParamList overlapCandidate) {
				Contract.Requires<ArgumentNullException>(stmt != null);
				Contract.Ensures(Contract.Result<ExprParamPair>() != null);
				if (stmt is IfStatement)
					return GenIf((IfStatement)stmt, overlapCandidate);
				if (stmt is Defun)
					return GenDefun((Defun)stmt, overlapCandidate);
				if (stmt is Return)
					return GenReturn((Return)stmt, overlapCandidate);
				if (stmt is PhraseChain)
					return GenChain((PhraseChain)stmt, overlapCandidate);
				if (stmt is BlockExecute)
					return GenBlockExec((BlockExecute)stmt, overlapCandidate);
				Contract.Assert(false);
				throw new InvalidOperationException();
			}

			private ExprParamPair GenIf(IfStatement stmt, ParamList overlapCandidate) {
				Contract.Requires<ArgumentNullException>(stmt != null);
				Contract.Ensures(Contract.Result<ExprParamPair>() != null);

				var last = stmt.Thens.Last();
				var pair = GenStmt(last.Statement, overlapCandidate);
				Expression expr;
				if (last.Condition is BoolLiteral && ((BoolLiteral)last.Condition).Value) {
					expr = pair.Expression;
				}
				else {
					expr = Expression.Condition(ElemGen.GenBoolElem(last.Condition), pair.Expression, NullExpr);
				}
				overlapCandidate = ParamList.Merge(pair.Parameters, overlapCandidate);
				foreach (var then in stmt.Thens.Reverse().Skip(1)) {
					pair = GenStmt(then.Statement, overlapCandidate);
					expr = Expression.Condition(ElemGen.GenBoolElem(then.Condition), pair.Expression, expr);
					overlapCandidate = ParamList.Merge(pair.Parameters, overlapCandidate);
				}
				return new ExprParamPair(expr, overlapCandidate);
			}

			private ExprParamPair GenDefun(Defun stmt, ParamList overlapCandidate) {
				Contract.Requires<ArgumentNullException>(stmt != null);
				Contract.Ensures(Contract.Result<ExprParamPair>() != null);
				var gen = new DefunGen(this);
				return gen.Generate(stmt);
			}

			private ExprParamPair GenReturn(Return stmt, ParamList overlapCandidate) {
				Contract.Requires<ArgumentNullException>(stmt != null);
				Contract.Ensures(Contract.Result<ExprParamPair>() != null);
				var target = GetTarget();
				var expr = Expression.Return(target, ElemGen.GenElem(stmt.Value), typeof(object));
				return new ExprParamPair(expr, null);
			}

			private ExprParamPair GenBlockExec(BlockExecute stmt, ParamList overlapCandidate) {
				Contract.Requires<ArgumentNullException>(stmt != null);
				Contract.Ensures(Contract.Result<ExprParamPair>() != null);
				var gen = new BlockGen(this);
				var block = gen.Generate(stmt.Block);
				return new ExprParamPair(block, null);
			}

			private ExprParamPair GenChain(PhraseChain stmt, ParamList overlapCandidate) {
				Contract.Requires<ArgumentNullException>(stmt != null);
				Contract.Ensures(Contract.Result<ExprParamPair>() != null);
				Contract.Ensures(Contract.Result<ExprParamPair>().Expression != null);
				Contract.Ensures(Contract.Result<ExprParamPair>().Expression.Type != typeof(void));

				ParameterExpression tmpVar = null;
				ParamList assigned = null;
				var exprs = new List<Expression>();
				var local = new List<ParameterExpression>();
				foreach (var ph in stmt.Phrases) {
					exprs.Add(DebugInfo(ph.Range));
					var phExpr = GenPhrase(ph, tmpVar, assigned, overlapCandidate);
					assigned = phExpr.Parameters;
					tmpVar = Expression.Parameter(phExpr.Expression.Type);
					local.Add(tmpVar);
					exprs.Add(Expression.Assign(tmpVar, phExpr.Expression));
				}
				exprs.Add(ClearInfo());
				exprs.Add(tmpVar);
				return new ExprParamPair(Expression.Block(local, exprs), assigned);
			}

			#endregion

			#region Phrase

			private ExprParamPair GenPhrase(Phrase ph, Expression prev, ParamList defined, ParamList overlapCandidate) {
				Contract.Requires<ArgumentNullException>(ph != null);
				Contract.Ensures(Contract.Result<ExprParamPair>() != null);
				if (ph is DefineValue)
					return GenDefValue((DefineValue)ph, prev, defined, overlapCandidate);
				Expression expr = null;
				if (ph is Assign)
					expr = GenAssign((Assign)ph, prev);
				if (ph is PropertySet)
					expr = GenPropSet((PropertySet)ph, prev);
				if (ph is Call)
					expr = GenCall((Call)ph, prev);
				return new ExprParamPair(expr, defined);
			}

			private ExprParamPair GenDefValue(DefineValue ph, Expression prev, ParamList defined, ParamList overlapCandidate) {
				Contract.Requires<ArgumentNullException>(ph != null);
				if (ph.Value == null && prev == null)
					throw Error("値が指定されていません。", ph.Range.Start);
				if (ph.Value != null && prev != null)
					throw Error("値が二重に指定されています。", ph.Range.Start);
				ParameterExpression variable =
					ParamList.Search(overlapCandidate, p => p.Name == ph.Name) ??
					Expression.Parameter(typeof(object), ph.Name);
				var value = prev ?? ElemGen.GenElem(ph.Value);
				var assign = Expression.Assign(variable, value);
				defined = new ParamList(variable, defined);
				return new ExprParamPair(assign, defined);
			}

			private Expression GenPropSet(PropertySet ph, Expression prev) {
				Contract.Requires<ArgumentNullException>(ph != null);
				if (ph.Value == null && prev == null)
					throw Error("値が指定されていません。", ph.Range.Start);
				if (ph.Value != null && prev != null)
					throw Error("値が二重に指定されています。", ph.Range.Start);

				var target = ElemGen.GenElem(ph.Property.Value);
				var value = prev ?? ElemGen.GenElem(ph.Value);
				return Expression.Dynamic(Factory.SetMemberBinder(ph.Property.Name), typeof(object), target, value);
			}

			private Expression GenAssign(Assign ph, Expression prev) {
				Contract.Requires<ArgumentNullException>(ph != null);
				if (ph.Value == null && prev == null)
					throw Error("値が指定されていません。", ph.Range.Start);
				if (ph.Value != null && prev != null)
					throw Error("値が二重に指定されています。", ph.Range.Start);

				var value = prev ?? ElemGen.GenElem(ph.Value);
				return AssignGlobal(ph.Name, value);
			}

			private Expression GenCall(Call ph, Expression prev) {
				Contract.Requires<ArgumentNullException>(ph != null);
				if (ph is MapCall)
					return GenMapCall((MapCall)ph, prev);
				// 関数あるいは，Globalを入れるために，0番目を開けた状態で，引数配列を作る。
				var sfxs = ph.Arguments.Select(a => a.Suffix).ToArray();
				var args = new List<Expression> { null }; // index : 0
				if (prev != null)
					args.Add(prev);
				args.AddRange(ph.Arguments.Select(a => ElemGen.GenElem(a.Argument)));
				var callInfo = new CallInfo(sfxs.Length + (prev == null ? 0 : 1), sfxs);
				args[0] = GetValue(ph.Name);
				DynamicMetaObjectBinder binder;
				if (args[0] is ParameterExpression) {
					binder = Factory.InvokeBinder(callInfo);
				}
				else {
					binder = Factory.InvokeMemberBinder(ph.Name, callInfo);
					args[0] = Global;
				}
				return Expression.Dynamic(binder, typeof(object), args);
			}

			private Expression GenMapCall(MapCall ph, Expression prev) {
				Contract.Requires<ArgumentNullException>(ph != null);
				if (ph.FirstArg == null && prev == null)
					throw Error("「それぞれ」の対象が指定されていません。", ph.Range.Start);
				if (ph.FirstArg != null && prev != null)
					throw Error("「それぞれ」の対象が二重に指定されています。", ph.Range.Start);
				var value = prev ?? ElemGen.GenElem(ph.FirstArg.Argument);

				Expression lambda;
				{
					var param = Expression.Parameter(typeof(object), "要素");
					var args = new List<Expression> { GetValue(ph.Name), param };
					var sfxs = new List<string>();
					if (ph.FirstArg != null)
						sfxs.Add(ph.FirstArg.Suffix);
					foreach (var pair in ph.Arguments) {
						args.Add(ElemGen.GenElem(pair.Argument));
						sfxs.Add(pair.Suffix);
					}
					var callInfo = new CallInfo(args.Count - 1, sfxs);
					DynamicMetaObjectBinder binder;
					if (args[0] is ParameterExpression) {
						binder = Factory.InvokeBinder(callInfo);
					}
					else {
						binder = Factory.InvokeMemberBinder(ph.Name, callInfo);
						args[0] = Global;
					}
					lambda = Expression.Lambda(Expression.Dynamic(binder, typeof(object), args), param);
				}
				return Expression.Dynamic(Factory.MapBinder, typeof(object), lambda, value);
			}

			protected Expression AssignGlobal(string name, Expression value) {
				Contract.Requires<ArgumentNullException>(name != null);
				Contract.Requires<ArgumentNullException>(value != null);
				if (value.Type != typeof(object))
					value = Expression.Convert(value, typeof(object));
				return Expression.Dynamic(Factory.SetMemberBinder(name), typeof(object), Global, value);
			}

			#endregion

			public Expression GetGlobal(string name) {
				Contract.Requires<ArgumentNullException>(name != null);
				Contract.Ensures(Contract.Result<Expression>() != null);
				return Expression.Dynamic(Factory.GetMemberBinder(name), typeof(object), Global);
			}

			#region Helper

			public DebugInfoExpression DebugInfo(TextRange range) {
				Contract.Ensures(Contract.Result<DebugInfoExpression>() != null);
				return Expression.DebugInfo(DocumentInfo, range.Start.Line, range.Start.Column, range.End.Line, range.End.Column);
			}

			public DebugInfoExpression ClearInfo() {
				Contract.Ensures(Contract.Result<DebugInfoExpression>() != null);
				return Expression.ClearDebugInfo(DocumentInfo);
			}

			internal SyntaxException Error(string message, TextLocation location) {
				Contract.Requires<ArgumentNullException>(message != null);
				return new SyntaxException(message, FileName, location);
			}

			#endregion
		}

		private class GlobalGen : StmtGen {

			// ===== ===== ===== ===== ===== fields ===== ===== ===== ===== =====
			private readonly ParameterExpression _global;
			private readonly string _filename;
			private readonly BinderFactory _factory;
			private readonly SymbolDocumentInfo _docInfo;

			// ===== ===== ===== ===== ===== property ===== ===== ===== ===== =====
			public override string FileName { get { return _filename; } }
			public override BinderFactory Factory { get { return _factory; } }
			public override ParameterExpression Global { get { return _global; } }
			public override SymbolDocumentInfo DocumentInfo { get { return _docInfo; } }

			public override StmtGen Parent {
				get { throw new InvalidOperationException("グローバルは親を持ちません"); }
			}

			// ===== ===== ===== ===== ===== ctor ===== ===== ===== ===== =====
			public GlobalGen(string filename, BinderFactory factory, ParameterExpression global) {
				_filename = filename;
				_factory = factory;
				_global = global;
				_docInfo = Expression.SymbolDocument(filename);
			}

			// ===== ===== ===== ===== ===== method ===== ===== ===== ===== =====

			public Expression Generate(Block block) {
				Contract.Requires<ArgumentNullException>(block != null);
				Contract.Ensures(Contract.Result<Expression>() != null);

				if (block.Statements.Count == 0)
					return NullExpr;
				var list = new List<Expression> { DebugInfo(block.Range) };
				foreach (var stmt in block.Statements) {
					var pair = GenStmt(stmt, null);
					if (pair.Parameters == null) {
						list.Add(pair.Expression);
					}
					else {
						var tmp = Expression.Parameter(pair.Expression.Type, "tmp_global_stmt");
						var tmpLocal = new List<Expression>();
						tmpLocal.Add(Expression.Assign(tmp, pair.Expression));
						foreach (var p in ParamList.ToEnum(pair.Parameters))
							tmpLocal.Add(AssignGlobal(p.Name, p));
						tmpLocal.Add(tmp);
						var ps = new ParamList(tmp, pair.Parameters);
						list.Add(Expression.Block(ParamList.ToEnum(ps), tmpLocal));
						Contract.Assume(list[list.Count - 1].Type == tmp.Type);
					}
				}
				var blockExpr = Expression.Block(list);
				if (Target == null) {
					return blockExpr;
				}
				else {
					return Expression.Label(Target, blockExpr);
				}
			}

			// ----- ----- ----- ----- override ----- ----- ----- -----

			public override Expression GetValue(string name) {
				return GetGlobal(name);
			}
		}

		private class BlockGen : StmtGen {

			// ===== ===== ===== ===== ===== fields ===== ===== ===== ===== =====
			private ParamList _locals = null;
			private readonly StmtGen _parent;

			// ===== ===== ===== ===== ===== property ===== ===== ===== ===== =====
			public override StmtGen Parent { get { return _parent; } }

			// ===== ===== ===== ===== ===== ctor ===== ===== ===== ===== =====
			public BlockGen(StmtGen parent) {
				_parent = parent;
			}

			// ===== ===== ===== ===== ===== method ===== ===== ===== ===== =====

			public Expression Generate(Block block) {
				if (block.Statements.Count == 0)
					return NullExpr;
				var list = new List<Expression> { DebugInfo(block.Range) };
				foreach (var stmt in block.Statements) {
					var pair = GenStmt(stmt, null);
					UpdateLocals(pair.Parameters, block.Range.Start);
					list.Add(pair.Expression);
				}
				var blockExpr = Expression.Block(ParamList.ToEnum(_locals), list);
				if (Target == null)
					return blockExpr;
				else
					return Expression.Label(Target, blockExpr);
			}

			private void UpdateLocals(ParamList parameters, TextLocation location) {
				foreach (var param in ParamList.ToEnum(parameters)) {
					if (ParamList.Search(_locals, p => p.Name == param.Name) == null) {
						_locals = new ParamList(param, _locals);
					}
					else {
						throw Error("同じ名前の変数「" + param.Name + "」が二度宣言されています。", location);
					}
				}
			}

			// ----- ----- ----- ----- override ----- ----- ----- -----

			public override Expression GetValue(string name) {
				return
					ParamList.Search(_locals, p => p.Name == name) ??
					Parent.GetValue(name);
			}
		}

		private class DefunGen : BlockGen {

			// ===== ===== ===== ===== ===== fields ===== ===== ===== ===== =====

			private ParamList _parameters = null;

			// ===== ===== ===== ===== ===== ctor ===== ===== ===== ===== =====

			public DefunGen(StmtGen parent) : base(parent) { }

			// ===== ===== ===== ===== ===== method ===== ===== ===== ===== =====

			public ExprParamPair Generate(Defun stmt) {
				Contract.Requires<ArgumentNullException>(stmt != null);
				// prepare
				var funcType = ExpressionHelper.GetFuncType(stmt.Params.Count);
				var sfxFuncType = typeof(SuffixFunc<>).MakeGenericType(funcType);
				var funcTarget = Expression.Parameter(sfxFuncType, stmt.Name);
				var ps = ParamList.FromEnum(stmt.Params.Select(p => Expression.Parameter(typeof(object), p.Name)));
				_parameters = new ParamList(funcTarget, ps);
				// generate block
				var gen = new BlockGen(this);
				var block = gen.Generate(stmt.Block);
				// crean up
				var lambda = Expression.Lambda(block, ParamList.ToEnum(_parameters.Next));
				var ctorInfo = sfxFuncType.GetConstructor(new[] { funcType, typeof(string[]) });
				var sfxs = stmt.Params.Select(pair => pair.Suffix).ToArray();
				var createExpr = Expression.New(ctorInfo, lambda, Expression.Constant(sfxs));
				return new ExprParamPair(Expression.Assign(funcTarget, createExpr), new ParamList(funcTarget, null));
			}

			// ----- ----- ----- ----- override ----- ----- ----- -----

			public override Expression GetValue(string name) {
				return
					ParamList.Search(_parameters, p => p.Name == name) ??
					Parent.GetValue(name);
			}
		}

		private class ElemGen {

			// ===== ===== ===== ===== ===== field ===== ===== ===== ===== =====

			// ===== ===== ===== ===== ===== property ===== ===== ===== ===== =====
			protected StmtGen StmtGen { get; private set; }
			protected BinderFactory Factory { get { return StmtGen.Factory; } }

			// ===== ===== ===== ===== ===== ctor ===== ===== ===== ===== =====
			public ElemGen(StmtGen gen) {
				Contract.Requires<ArgumentNullException>(gen != null);
				StmtGen = gen;
			}

			// ===== ===== ===== ===== ===== method ===== ===== ===== ===== =====

			/// <summary>
			/// 要素がBoolでなければBoolに変換した状態。
			/// </summary>
			public Expression GenBoolElem(Element elem) {
				Contract.Requires<ArgumentNullException>(elem != null);
				Contract.Ensures(Contract.Result<Expression>() != null);
				Contract.Ensures(Contract.Result<Expression>().Type == typeof(bool));
				var expr = GenElemCore(elem);
				if (expr.Type == typeof(bool))
					return expr;
				return Expression.Dynamic(StmtGen.Factory.ToBoolBinder, typeof(bool), expr);
			}

			/// <summary>
			/// 要素を必要に応じてObjectに変換した状態。
			/// </summary>
			public Expression GenElem(Element elem) {
				Contract.Requires<ArgumentNullException>(elem != null);
				Contract.Ensures(Contract.Result<Expression>() != null);
				Contract.Ensures(Contract.Result<Expression>().Type == typeof(object));
				var expr = GenElemCore(elem);
				if (expr.Type == typeof(object))
					return expr;
				return Expression.Convert(expr, typeof(object));
			}

			/// <summary>
			/// そのままExpressionに直した状態。
			/// </summary>
			private Expression GenElemCore(Element elem) {
				Contract.Requires<ArgumentNullException>(elem != null);
				Contract.Ensures(Contract.Result<Expression>() != null);
				Contract.Ensures(Contract.Result<Expression>().Type != typeof(void));
				switch (elem.Type) {
				// literal
				case ElementType.String:
					return GenString((StringLiteral)elem);
				case ElementType.Integer:
					return GenInteger((IntLiteral)elem);
				case ElementType.Float:
					return GenFloat((FloatLiteral)elem);
				case ElementType.Bool:
					return GenBool((BoolLiteral)elem);
				case ElementType.Null:
					return GenNull((NullLiteral)elem);
				// structure
				case ElementType.Tuple:
					return GenTuple((TupleLiteral)elem);
				case ElementType.List:
					return GenList((ListLiteral)elem);
				// expr
				case ElementType.Binary:
					return GenBinary((BinaryExpr)elem);
				case ElementType.Unary:
					return GenUnary((UnaryExpr)elem);
				// other
				case ElementType.Property:
					return GenProp((PropertyAccess)elem);
				case ElementType.Lambda:
					return GenLambda((Lambda)elem);
				case ElementType.Symbol:
					return GenSymbol((Symbol)elem);
				case ElementType.LambdaParam:
					return GenLambdaParam((LambdaParameter)elem);
				}
				throw Error("未知の要素が出現しました。", elem.Range.Start);
			}

			#region リテラル

			private ConstantExpression GenString(StringLiteral lit) {
				Contract.Requires<ArgumentNullException>(lit != null);
				Contract.Ensures(Contract.Result<ConstantExpression>() != null);
				return Expression.Constant(lit.Value);
			}

			private ConstantExpression GenInteger(IntLiteral lit) {
				Contract.Requires<ArgumentNullException>(lit != null);
				Contract.Ensures(Contract.Result<ConstantExpression>() != null);
				return Expression.Constant(lit.Value);
			}

			private ConstantExpression GenFloat(FloatLiteral lit) {
				Contract.Requires<ArgumentNullException>(lit != null);
				Contract.Ensures(Contract.Result<ConstantExpression>() != null);
				return Expression.Constant(lit.Value);
			}

			private ConstantExpression GenBool(BoolLiteral lit) {
				Contract.Requires<ArgumentNullException>(lit != null);
				Contract.Ensures(Contract.Result<ConstantExpression>() != null);
				return Expression.Constant(lit.Value);
			}

			private ConstantExpression GenNull(NullLiteral lit) {
				Contract.Requires<ArgumentNullException>(lit != null);
				Contract.Ensures(Contract.Result<ConstantExpression>() != null);
				return Expression.Constant(null);
			}

			#endregion

			#region データ構造

			private Expression GenTuple(TupleLiteral elem) {
				Contract.Requires<ArgumentNullException>(elem != null);
				Contract.Ensures(Contract.Result<Expression>() != null);
				return Cons(GenElemCore(elem.Head), GenElemCore(elem.Tail));
			}

			private Expression GenList(ListLiteral elem) {
				Contract.Requires<ArgumentNullException>(elem != null);
				Contract.Ensures(Contract.Result<Expression>() != null);
				Expression tail = Expression.Constant(ListCell.Null);
				var elems = elem.Elements;
				foreach (var item in elem.Elements.Reverse()) {
					tail = Cons(GenElemCore(item), tail);
				}
				return tail;
			}

			/// <summary>
			/// 式をListCell.Consでリストにする。
			/// 必要に応じて，展開する。
			/// </summary>
			private Expression Cons(Expression head, Expression tail) {
				Contract.Requires<ArgumentNullException>(head != null);
				Contract.Requires<ArgumentNullException>(tail != null);
				Contract.Ensures(Contract.Result<Expression>() != null);
				var constHead = head as ConstantExpression;
				var constTail = tail as ConstantExpression;
				if (constHead != null && constTail != null) {
					var cons = ListCell.Cons(constHead.Value, constTail.Value);
					return Expression.Constant(cons);
				}
				else {
					if (head.Type != typeof(object))
						head = Expression.Convert(head, typeof(object));
					if (tail.Type != typeof(object))
						tail = Expression.Convert(tail, typeof(object));
					return ExpressionHelper.BetaReduction<object, object, object>(
						(left, right) => ListCell.Cons(left, right), head, tail);
				}
			}

			#endregion

			#region 式

			private Expression GenUnary(UnaryExpr elem) {
				Contract.Requires<ArgumentNullException>(elem != null);
				Contract.Ensures(Contract.Result<Expression>() != null);
				var value = GenElem(elem.Value);
				var binder = SearchBinder(elem.ExprType);
				if (binder != null)
					return Expression.Dynamic(binder, typeof(object), value);
				throw Error("未定義の演算が現れました。", elem.Range.Start);
			}

			private DynamicMetaObjectBinder SearchBinder(UnaryOperationType op) {
				switch (op) {
				case UnaryOperationType.Not:
					return Factory.NotBinder;
				case UnaryOperationType.Negate:
					return Factory.NegateBinder;
				default:
					return null;
				}
			}

			private Expression GenBinary(BinaryExpr elem) {
				Contract.Requires<ArgumentNullException>(elem != null);
				Contract.Ensures(Contract.Result<Expression>() != null);
				var left = GenElem(elem.Left);
				var right = GenElem(elem.Right);
				var binder = SearchBinder(elem.ExprType);
				if (binder != null)
					return Expression.Dynamic(binder, typeof(object), left, right);
				switch (elem.ExprType) {
				case BinaryOperationType.Cons:
					return ExpressionHelper.BetaReduction<object, object, object>(
						(arg0, arg1) => ListCell.Cons(arg0, arg1), left, right);
				case BinaryOperationType.Concat:
					return ExpressionHelper.BetaReduction<object, object, object>(
						(arg0, arg1) => String.Concat(arg0, arg1), left, right);
				}
				throw Error("未定義の演算が現れました。", elem.Range.Start);
			}

			private DynamicMetaObjectBinder SearchBinder(BinaryOperationType op) {
				switch (op) {
				// arithmetic
				case BinaryOperationType.Add:
					return Factory.AddBinder;
				case BinaryOperationType.Subtract:
					return Factory.SubBinder;
				case BinaryOperationType.Multiply:
					return Factory.MultBinder;
				case BinaryOperationType.Divide:
					return Factory.DivideBinder;
				case BinaryOperationType.Modulo:
					return Factory.ModBinder;
				// compare
				case BinaryOperationType.LessThan:
					return Factory.LessThanBinder;
				case BinaryOperationType.LessThanOrEqual:
					return Factory.LessThanOrEqualBinder;
				case BinaryOperationType.GreaterThan:
					return Factory.GreaterThanBinder;
				case BinaryOperationType.GreaterThanOrEqual:
					return Factory.GreaterThanOrEqualBinder;
				// equality
				case BinaryOperationType.Equal:
					return Factory.EqualBinder;
				case BinaryOperationType.NotEqual:
					return Factory.NotEqualBinder;
				// bool
				case BinaryOperationType.And:
					return Factory.AndBinder;
				case BinaryOperationType.Or:
					return Factory.OrBinder;
				default:
					return null;
				}
			}

			#endregion

			#region 他

			private Expression GenProp(PropertyAccess elem) {
				Contract.Requires<ArgumentNullException>(elem != null);
				Contract.Ensures(Contract.Result<Expression>() != null);
				var target = GenElem(elem.Value);
				return Expression.Dynamic(StmtGen.Factory.GetMemberBinder(elem.Name), typeof(object), target);
			}

			private LambdaExpression GenLambda(Lambda elem) {
				Contract.Requires<ArgumentNullException>(elem != null);
				Contract.Ensures(Contract.Result<Expression>() != null);
				return new LambdaGen(this.StmtGen).Generate(elem);
			}

			private Expression GenSymbol(Symbol elem) {
				Contract.Requires<ArgumentNullException>(elem != null);
				Contract.Ensures(Contract.Result<Expression>() != null);
				return StmtGen.GetValue(elem.Name);
			}

			protected virtual Expression GenLambdaParam(LambdaParameter elem) {
				Contract.Requires<ArgumentNullException>(elem != null);
				Contract.Ensures(Contract.Result<Expression>() != null);
				throw StmtGen.Error("ラムダパラメタはラムダ式の内部でのみ利用出来ます。", elem.Range.Start);
			}

			#endregion

			protected SyntaxException Error(string message, TextLocation location) {
				return StmtGen.Error(message, location);
			}
		}

		private class LambdaGen : ElemGen {

			// ===== ===== ===== ===== ===== field ===== ===== ===== ===== =====

			private ParamList _params = null;

			// ===== ===== ===== ===== ===== ctor ===== ===== ===== ===== =====

			public LambdaGen(StmtGen gen) : base(gen) { }

			// ===== ===== ===== ===== ===== method ===== ===== ===== ===== =====

			public LambdaExpression Generate(Lambda lambda) {
				Contract.Requires<ArgumentNullException>(lambda != null);
				Contract.Ensures(Contract.Result<LambdaExpression>() != null);
				var expr = GenElem(lambda.Element);
				var tmp = Expression.Parameter(expr.Type);
				var body = Expression.Block(
					new[] { tmp },
					StmtGen.DebugInfo(lambda.Range),
					Expression.Assign(tmp, expr),
					StmtGen.ClearInfo(),
					tmp);
				return Expression.Lambda(body, ParamList.ToEnum(_params).Reverse());
			}

			// ----- ----- ----- ----- override ----- ----- ----- -----

			protected override Expression GenLambdaParam(LambdaParameter elem) {
				var param = ParamList.Search(_params, p => p.Name == elem.Name);
				if (param != null)
					return param;
				param = Expression.Parameter(typeof(object), elem.Name);
				_params = new ParamList(param, _params);
				return param;
			}
		}

		#endregion

	}
}
