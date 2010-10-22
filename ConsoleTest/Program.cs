using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Kurogane;
using Kurogane.Dynamic;
using System.Linq.Expressions;
using Kurogane.Util;
using Kurogane.RuntimeBinder;

namespace ConsoleTest {

	class Program {
		static void Main(string[] args) {
			FibTest();
		}

		static void BasicTest() {
			var code =
				"「こんにちは」を出力する。※改行は出力されないことに注意。 " +
				"改行を出力する。" +
				"(1 * 2 + 3 * 4)を出力する。" +
				"改行を出力する。";
			var e = new Engine();
			e.Execute(code);
		}

		static void FibTest() {
			string code =
				"以下の手順でNをFIB変換する。" +
				"	以下の手順で頭と体をNまでFIB変換する。" +
				"		頭をAに代入する。" +
				"		体をBに代入する。" +
				"		もし(N>0)なら" +
				"			Bと(A+B)を(N-1)までFIB変換する。" +
				"		他なら" +
				"			Aをパスする。" +
				"	以上。" +
				"	0と1をNまでFIB変換する。" +
				"以上。" +
				"1000をFIB変換し、出力する。";

				//"以下の手順でNをFIB変換する。" +
				//"	以下の手順で計算する。" +
				//"		(N-1)をFIB変換し、Aとする。" +
				//"		(N-2)をFIB変換し、Bとする。" +
				//"		(A+B)をパスする。" +
				//"	以上。" +
				//"	もし(N≦1)なら" +
				//"		Nをパスする。" +
				//"	他なら" +
				//"		計算する。" +
				//"以上。" +
				//"10をFIB変換し、出力する。改行を出力する。";
			var engine = new Engine();
			engine.Global.SetVariable("パス", SuffixFunc.Create((object obj) => obj, "を"));
			engine.Execute(code);
		}

		static void Test() {
			var code =
				"「こんにちは」と「こんばんは」をペアに代入する。" +
				"ペアの頭を出力する。改行を出力する。" +
				"ペアの体を出力する。改行を出力する。";
			var engine = new Engine();
			engine.Execute(code);
		}

		static void SumTest() {
			string code =
				"以下の手順でAにBを連結する。" +
				"	以下の手順でTで判定する。" +
				"		もし(T＝0)なら" +
				"			Aをパスする。" +
				"		他なら" +
				"			Bをパスする。" +
				"	以上。" +
				"	判定をパスする。" +
				"以上。" +
				"以下の手順で分解を合計する。" +
				"	以下の手順で計算する。" +
				"		0で分解し、Aとする。" +
				"		1で分解し、合計し、Bとする。" +
				"		(A+B)をパスする。" +
				"	以上。" +
				"	もし(分解＝0)なら" +
				"		0をパスする。" +
				"	他なら" +
				"		計算する。" +
				"以上。" +
				"0を1に連結し、2に連結し、3に連結し、4に連結し、リストに代入する。" +
				"リストを合計し、出力する。";
			var engine = new Engine();
			engine.Global.SetVariable("パス", SuffixFunc.Create((object obj) => obj, "を"));
			engine.Execute(code);

		}

		static void TestGreet() {
			var code =
				"以下の手順で挨拶する。" +
				"	「こんにちは」を出力する。" +
				"	改行を出力する。" +
				"以上。" +
				"挨拶する。" +
				"挨拶する。";
			var engine = new Engine();
			engine.Execute(code);
		}
	}
}
