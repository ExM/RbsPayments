using System;
using System.Text;
using System.Collections.Specialized;
using System.Web;

namespace RbsPayments
{
	public static class UriParameters
	{
		private static void AppendKey(StringBuilder sb, string key, Encoding enc)
		{
			if (sb.Length != 0)
				sb.Append('&');
			sb.Append(HttpUtility.UrlEncode(key, enc));
		}

		private static void AppendPair(StringBuilder sb, string key, string val, Encoding enc)
		{
			if (sb.Length != 0)
				sb.Append('&');
			sb.Append(HttpUtility.UrlEncode(key, enc));
			sb.Append('=');
			sb.Append(HttpUtility.UrlEncode(val, enc));
		}

		public static string Encode(NameValueCollection parameters)
		{
			return Encode(parameters, Encoding.UTF8);
		}

		public static string Encode(NameValueCollection parameters, Encoding enc)
		{
			if (parameters == null)
				return null;

			StringBuilder sb = new StringBuilder();

			for (int i = 0; i < parameters.Count; i++)
			{
				string key = parameters.GetKey(i);
				string[] values = parameters.GetValues(i);
				if (values == null)
					AppendKey(sb, key, enc);
				else
					foreach (string val in values)
						AppendPair(sb, key, val, enc);
			}

			return sb.ToString();
		}
	}
}

