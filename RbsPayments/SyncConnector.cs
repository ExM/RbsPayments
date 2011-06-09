using System;
using System.Net;
using System.IO;
using System.Text;

namespace RbsPayments
{
	public class SyncConnector: IConnector
	{
		private readonly string _baseUri;
		private readonly int _to;
		
		public SyncConnector(string baseUri, TimeSpan to)
		{
			_baseUri = baseUri;
			_to = (int)to.TotalMilliseconds;
		}

		#region IConnector implementation
		public void Request(string cmd, System.Collections.Specialized.NameValueCollection getParams, Action<string> completed, Action<Exception> excepted)
		{
			string uri = _baseUri + "/" + cmd + "?" + UriParameters.Encode(getParams);
			
			Console.WriteLine(_baseUri.ToString());
			Console.WriteLine(uri.ToString());
			
			HttpWebRequest webReq = (HttpWebRequest)WebRequest.Create(uri);
			webReq.Method = "GET";
			webReq.Timeout = _to;
			
			string respText;
			try
			{
				using (Stream respS = webReq.GetResponse().GetResponseStream())
					respText = Encoding.UTF8.GetString(ReadToEnd(respS));
			}
			catch(WebException err)
			{
				excepted(err);
				return;
			}
			catch(Exception err)
			{
				excepted(new WebException("data transmission error", err, WebExceptionStatus.UnknownError, null));
				return;
			}
			
			completed(respText);
		}
		#endregion
		
		private static byte[] ReadToEnd(Stream input)
		{
			byte[] buffer = new byte[16 * 1024]; //16kb
			using (MemoryStream ms = new MemoryStream())
			{
				int read;
				while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
					ms.Write(buffer, 0, read);
				return ms.ToArray();
			}
		}
	}
}

