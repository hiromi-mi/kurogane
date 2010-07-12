using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kurogane.Compiler;
using System.IO;
using Kurogane.Runtime;
using Kurogane.Buildin;

namespace ConsoleTest {
	class Program {
		static void Main(string[] args) {
			FibTest2();
		}

		static void KrgnProcDynamic() {
			Func<dynamic, dynamic, dynamic> add = (x, y) => x + y;
			dynamic 加算 = new KrgnFunc(add, "と", "を");
			var three = 加算(と: 1, を: 2);
			Console.WriteLine(three);
		}

		static void SimpleGreet() {
			var code =
				"「こんにちは」を[改行出力]する。" +
				"「さようなら」を[パス]する。" +
				"「ようこそ」を[パス]し，[改行出力]する。";

			var engine = new Engine();
			var result = engine.Execute(code);
			Console.WriteLine("resutl is \"{0}\"", result);
		}

		static void PairTest() {
			string code = @"
以下の手順で[A]に[B]を[連結]する。
	以下の手順で[T]で[判定]する。
		もし([T]＝1)なら
			[A]を[パス]する。
		他なら
			[B]を[パス]する。
	以上。
	[判定]を[パス]する。
以上。
5に3を[連結]し、[リスト]に[代入]する。
0で[リスト]し、[表示]する。
";
			var engine = new Engine();
			engine.Execute(code);
		}

		static void ListTest() {
			string code = @"
以下の手順で[A]に[B]を[連結]する。
	以下の手順で[T]で[判定]する。
		もし([T]＝0)なら
			[A]を[パス]する。
		他なら
			[B]を[パス]する。
	以上。
	[判定]を[パス]する。
以上。
以下の手順で[分解]を[合計]する。
	以下の手順で[計算]する。
		0で[分解]し、[A]に[代入]する。
		1で[分解]し、[合計]し、[B]に[代入]する。
		([A]+[B])を[パス]する。
	以上。
	もし([分解]＝0)なら
		0を[パス]する。
	他なら
		[計算]する。
以上。
0を1に[連結]し、2に[連結]し、3に[連結]し、4に[連結]し、[リスト]に[代入]する。
[リスト]を[合計]し、[表示]する。
";
			var engine = new Engine();
			engine.Execute(code);
		}
		static void LoopTest() {
			string code = @"
以下の手順で[X]を[表示パス]する。
	[X]を[表示]する。
	[X]を[パス]する。
以上。

以下の手順で[N]まで[段階表示]する。
	以下の手順で[n]を[段階表示']する。
		もし([n]＜[N])なら
			([n]+1)を[表示パス]し、[段階表示']する。
	以上。
	0を[段階表示']する。
以上。
10まで[段階表示]する。
";
			var engine = new Engine();
			engine.Execute(code);
		}

		static void AssignTest() {
			string code = @"
以下の手順で[挨拶]する。
	[彼]に「山田さん」を[代入]する。
	「こんにちは，」を[出力]する。
	[彼]を[改行出力]する。
以上。
[挨拶]する。
";
			var engine = new Engine();
			engine.Execute(code);
		}

		static void InnterFuncTest() {
			string code = @"
以下の手順で[彼]に[挨拶]する。
	以下の手順で[昼の挨拶]する。
		「こんにちは，」を[出力]する。
	以上。
	[昼の挨拶]する。
	[彼]を[改行出力]する。
以上。
「山田さん」に[挨拶]する。
";
			var engine = new Engine();
			engine.Execute(code);
		}

		static void GreetTest() {
			//Test(); return;
			string code = @"
以下の手順で[彼]に[挨拶]する。
	「こんにちは、」を[出力]する。
	[彼]を[出力]する。
	「。」を[改行出力]する。
以上。
「山田さん」に[挨拶]する。
";
			var engine = new Engine();
			engine.Execute(code);
			var obj = engine.DefaultScope.GetVariable("挨拶") as KrgnFunc;
		}

		static void ScopeTest() {
			string code = @"
以下の手順で[生成]する。
	以下の手順で[昼の挨拶]する。
		「こんにちは，」を[出力]する。
	以上。
	[昼の挨拶]を[パス]する。
以上。
[生成]する。
";
			var engine = new Engine();
			var obj = engine.Execute(code);
			Console.WriteLine(obj == null);
		}

		static void IfTest() {
			var code = @"
もし
 ([数]==1)なら
	「いちデス」を[表示]する。
 ([数]==2)なら
	「にデス」を[表示]する。
 他なら
	「他デス」を[表示]する。
";
			var engine = new Engine();
			engine.DefaultScope.SetVariable("数", 2);
			engine.Execute(code);
		}

		static void FibTest() {
			var fibs = @"
以下の手順で[N]を[FIB変換]する。
  もし([N]≦1)なら
	[N]を[返す]。
  他なら
	(([N] - 1)を[FIB変換]したもの + ([N] - 2)を[FIB変換]したもの)を[返す]。
以上。
";
			throw new NotImplementedException(fibs);
		}

		static void FibTest2() {
			string code = @"
以下の手順で[N]を[FIB変換]する。
	以下の手順で[計算]する。
		([N]-1)を[FIB変換]し、[A]に[代入]する。
		([N]-2)を[FIB変換]し、[B]に[代入]する。
		([A]+[B])を[パス]する。
	以上。
	もし([N]≦1)なら
		[N]を[パス]する。
	他なら
		[計算]する。
以上。
18を[FIB変換]し、[表示]する。
";
			var engine = new Engine();
			engine.Execute(code);
		}
	}
}
