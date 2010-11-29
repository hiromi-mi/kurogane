using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Kurogane.Test.Spec {

	/// <summary>
	/// 構文に関してのテストを行います。
	/// </summary>
	[TestClass]
	public class Language : TestHelper {

		[TestMethod]
		public void 何も無いプログラムを実行できる() {
			Assert.IsNull(Execute(""));
		}

		[TestMethod]
		public void である_によるReturn文() {
			Assert.AreEqual(1, Execute<int>("１である。"));
			Assert.AreEqual(3, Execute<int>("３である。５である。"));
		}

		[TestMethod]
		public void ラムダ式は第一級関数である() {
			Assert.IsInstanceOfType(Execute("【△＋□】である。"), typeof(Func<object, object, object>));
			Assert.AreEqual(7, Execute<int>("２を【□＋５】する。"));
			Assert.AreEqual(8, Execute<int>("３と５を【□＋△】する。"));
		}

		[TestMethod]
		public void とする_による代入() {
			Assert.AreEqual(2, Execute<int>("２を数とする。数である。"));
			Assert.AreEqual(3, Execute<int>("２を【□＋１】し，数とする。数である。"));
			Assert.AreEqual(4, Execute<int>("２をMとし，Nとする。（M+N）である。"));
		}

		[TestMethod]
		public void 代入_による代入() {
			Assert.AreEqual(1, Execute<int>("数に１を代入する。数である。"));
			Assert.AreEqual(2, Execute<int>("２を数に代入する。数である。"));
			Assert.AreEqual(3, Execute<int>("２を【□＋１】し，数に代入する。数である。"));
			Assert.AreEqual(4, Execute<int>("Mに２を代入し，Nに代入する。（M+N）である。"));
		}

		[TestMethod]
		public void 条件文() {
			Assert.IsNull(Execute("もし偽なら，「失敗」である。"));
			Assert.AreEqual(3, Execute<int>("もし（１＝１）なら，３である。他なら５である。"));
			Assert.AreEqual(5, Execute<int>("もし（１≠１）なら，３である。他なら５である。"));
			var code1 =
				"３を値とする。" +
				"もし（値＝１）なら，９である。" +
				"　　（値＝２）なら，８である。" +
				"　　（値＝３）なら，７である。" +
				"　　（値＝４）なら，６である。" +
				"　　（値＝５）なら，５である。";
			Assert.AreEqual(7, Execute<int>(code1));

			var code2 =
				"もし真なら，３を値に代入する。" +
				"　　他なら，５を値に代入する。" +
				"値である。";
			Assert.AreEqual(3, Execute<int>(code2));

			var code3 =
				"もし偽なら，３を値に代入する。" +
				"　　他なら，５を値に代入する。" +
				"値である。";
			Assert.AreEqual(5, Execute<int>(code3));
		}

		[TestMethod]
		public void それぞれ_によるMap処理() {
			// リストに対して
			var actual1 = Execute<ListCell>("【○×○】を二乗とする。[1,2,3,4,5]をそれぞれ二乗する。");
			var expected1 = ListCell.Of(1, 4, 9, 16, 25);
			Assert.AreEqual(expected1, actual1);

			var actual2 = Execute<Tuple<object, object>>("【○×２】を二倍とする。３と４をそれぞれ二倍する。");
			var expected2 = new Tuple<object, object>(6, 8);
			Assert.AreEqual(expected2, actual2);

			var actual3 = Execute<ListCell>("[1,2,3]をそれぞれ【□＋１】する。");
			var expected3 = ListCell.Of(2, 3, 4);
			Assert.AreEqual(expected3, actual3);

			var actual4 = Execute<ListCell>("[1:2,3:4,5:6,7:8]をそれぞれ【○＋△】する。");
			var expected4 = ListCell.Of(3, 7, 11, 15);
			Assert.AreEqual(expected4, actual4);
		}

		[TestMethod]
		public void 関数を定義し実行する() {
			var code1 =
				"以下の定義でテストする。" +
				"　　「成功」である。" +
				"以上。" +
				"テストする。";
			Assert.AreEqual("成功", Execute<string>(code1));
			var code2 =
				"以下の定義で値を二倍する。" +
				"　　（値×２）である。" +
				"以上。" +
				"３を二倍する。";
			Assert.AreEqual(6, Execute<int>(code2));
		}

		[TestMethod]
		public void ブロックを定義する() {
			Assert.IsNull(Execute("以下を実行する。以上。"));
			Assert.AreEqual(3, Execute<int>("以下を実行する。３である。以上。"));
			Assert.AreEqual(3, Execute<int>("以下を実行する。３である。５である。以上。"));
			Assert.AreEqual(3, Execute<int>("以下を実行する。以上。３である。"));
			Assert.AreEqual(5, Execute<int>("以下を実行する。３である。以上。５である。"));
		}

		[TestMethod]
		public void 条件文でブロックを使う() {
			var code =
				"もし（１＝２）なら，以下を実行する。" +
				"　　３である。" +
				"以上。" +
				"他なら，以下を実行する。" +
				"　　７である。" +
				"以上。";
			Assert.AreEqual(7, Execute<int>(code));
			Assert.AreEqual(3, Execute<int>(code + "３である。"));
		}

		[TestMethod]
		public void コメントを記述する() {
			Assert.IsNull(Execute("※これはコメントです。"));
			Assert.AreEqual(6, Execute("※５である。６である。"));
			Assert.AreEqual(4, Execute("４である。※５である。６である。"));
		}
	}
}
