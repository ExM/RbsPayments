using System;
using System.Net;
using System.IO;
using System.Text;
using System.Collections.Specialized;
using NLog;

namespace RbsPayments
{
	public class SyncConnector: IConnector
	{
		private static Logger Log = LogManager.GetCurrentClassLogger();
		
		private readonly Uri _baseUri;
		private readonly int _to;
		
		public SyncConnector(Uri baseUri, TimeSpan to)
		{
			_baseUri = baseUri;
			_to = (int)to.TotalMilliseconds;
		}

		public void Request(string cmd, NameValueCollection getParams, Action<string> completed, Action<Exception> excepted)
		{
			Uri uri = new Uri(_baseUri, cmd + "?" + UriParameters.Encode(getParams));
			Log.Trace("Request for uri:`{0}'", uri);
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
				Log.Debug("WebException: {0}", err);
				excepted(err);
				return;
			}
			catch(Exception err)
			{
				Log.Debug("Exception: {0}", err);
				excepted(new WebException("data transmission error", err, WebExceptionStatus.UnknownError, null));
				return;
			}
			
			completed(respText);
		}
		
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

