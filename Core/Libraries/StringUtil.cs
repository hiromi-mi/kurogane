using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kurogane.Libraries {

	[Library]
	public static class StringUtil {

		[JpName("文字分割")]
		public static ListCell Split([Suffix("を")]string str) {
			if (str == null) return null;
			return ListCell.ConvertFrom(str.ToCharArray().Select(c => c.ToString()));
		}

		[JpName("文字整数変換")]
		public static int? ChatOrStrToInt([Suffix("を")]object obj) {
			if (obj is char)
				return (int?)(char)obj;
			var str = obj as string;
			if (str != null && str.Length == 1)
				return (int)str[0];
			return null;
		}

		[JpName("整数文字変換")]
		public static string IntToChar([Suffix("を")]int num) {
			return ((char)num).ToString();
		}
	}
}
