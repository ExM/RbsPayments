using System;
using System.Text;
using System.Collections.Specialized;
using System.Web;
using System.IO;

namespace RbsPayments
{
	public class PostParameters
	{
		private static byte _eqByte = Convert.ToByte('=');
		private static byte _ampByte = Convert.ToByte('&');

		public static byte[] Encode(NameValueCollection parameters)
		{
			return Encode(parameters, Encoding.UTF8);
		}

		public static byte[] Encode(NameValueCollection parameters, Encoding enc)
		{
			int N = parameters.Count;
			if (N == 0)
				return new byte[0];

			MemoryStream ms = new MemoryStream();
			for (int i = 0; i < parameters.Count; i++)
			{
				string key = parameters.GetKey(i);
				string[] values = parameters.GetValues(i);
				if (values == null)
				{
					if (ms.Position != 0)
						ms.WriteByte(_ampByte);
					UrlEncodeToStream(ms, key, enc);
				}
				else
					foreach (string val in values)
					{
						if (ms.Position != 0)
							ms.WriteByte(_ampByte);
						UrlEncodeToStream(ms, key, enc);
						ms.WriteByte(_eqByte);
						UrlEncodeToStream(ms, val, enc);
					}
			}

			return ms.ToArray();
		}
		
		private static void UrlEncodeToStream(Stream stream, string text, Encoding enc)
		{
			byte[] buff = HttpUtility.UrlEncodeToBytes(text, enc);
			stream.Write(buff, 0, buff.Length);
		}
	}
}

