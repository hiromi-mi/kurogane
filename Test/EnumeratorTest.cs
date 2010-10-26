using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections;

namespace Kurogane.Test {
	/// <summary>
	/// EnumeratorTest の概要の説明
	/// </summary>
	[TestClass]
	public class EnumeratorTest {

		[TestMethod]
		public void Test_FromEnumerable() {
			var array = new[] { 1, 2, 3 };
			var list = array.ToList() as IEnumerable;

			Tuple<object, object> obj = Enumerator.FromEnumerable(list);

			Assert.IsInstanceOfType(obj.Item1, typeof(int));
			Assert.AreEqual(1, (int)obj.Item1);
			obj = obj.Item2 as Tuple<object, object>;
			Assert.IsNotNull(obj);

			Assert.IsInstanceOfType(obj.Item1, typeof(int));
			Assert.AreEqual(2, (int)obj.Item1);
			obj = obj.Item2 as Tuple<object, object>;
			Assert.IsNotNull(obj);

			Assert.IsInstanceOfType(obj.Item1, typeof(int));
			Assert.AreEqual(3, (int)obj.Item1);
			Assert.IsNull(obj.Item2);
		}

		[TestMethod]
		public void Test_Map() {
			var array = new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 0 };
			var obj = Enumerator.FromEnumerable(array.ToList() as IEnumerable);
			var func = new SuffixFunc<Func<object, object>>((object o) => (object)((int)o * 2), "を");
			var mapped = Enumerator.Map(func, obj);
			var list = Enumerator.ToEnumerable(mapped).ToList();
			Assert.AreEqual(array.Length, list.Count);
			for (int i = 0; i < array.Length; i++) {
				Assert.AreEqual(array[i] * 2, (int)list[i]);
			}
		}
	}
}
