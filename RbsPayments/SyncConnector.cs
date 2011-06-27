using System;
using System.Net;
using System.IO;
using System.Text;
using System.Collections.Specialized;
using NLog;

namespace RbsPayments
{
	public class SyncConnector: ICommandConnector, ISiteConnector
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
				using (WebResponse resp = webReq.GetResponse())
				using (Stream respS = resp.GetResponseStream())
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

		public void Request(string cmd, NameValueCollection getParams, NameValueCollection postParams, Action<string, CookieCollection> completed, Action<Exception> excepted)
		{
			if(getParams != null)
				cmd = cmd + "?" + UriParameters.Encode(getParams);
			Uri uri = new Uri(_baseUri, cmd);
			Log.Trace("Request for uri:`{0}'", uri);
			HttpWebRequest webReq = (HttpWebRequest)WebRequest.Create(uri);
			
			webReq.Method = "POST";
			webReq.Timeout = _to;
			webReq.ContentType = "application/x-www-form-urlencoded";
			webReq.Headers.Add("Content-Encoding", "UTF8");
			
			byte[] postContent = PostParameters.Encode(postParams);
			webReq.ContentLength = postContent.Length;
			webReq.AllowAutoRedirect = false;
			webReq.ServicePoint.Expect100Continue = false;
			webReq.CookieContainer = new CookieContainer();
			
			string respText;
			CookieCollection cookies;
			try
			{
				using(Stream respS = webReq.GetRequestStream())
					respS.Write(postContent, 0, postContent.Length);

				using (HttpWebResponse resp = (HttpWebResponse)webReq.GetResponse())
				{
					cookies = resp.Cookies;
					
					if(resp.ContentLength != 0)
					{
						Encoding enc = Encoding.GetEncoding(resp.CharacterSet);
						using (Stream respS = resp.GetResponseStream())
							respText = enc.GetString(ReadToEnd(respS));
					}
					else
						respText = string.Empty;
				}
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
			
			completed(respText, cookies);
		}
		
		public void Request(string cmd, CookieCollection inCookies, NameValueCollection getParams, NameValueCollection postParams, Action<string> completed, Action<Exception> excepted)
		{
			if(getParams != null)
				cmd = cmd + "?" + UriParameters.Encode(getParams);
			Uri uri = new Uri(_baseUri, cmd);
			Log.Trace("Request for uri:`{0}'", uri);
			HttpWebRequest webReq = (HttpWebRequest)WebRequest.Create(uri);
			
			webReq.Method = "POST";
			webReq.Timeout = _to;
			webReq.ContentType = "application/x-www-form-urlencoded";
			webReq.Headers.Add("Content-Encoding", "UTF8");
			
			byte[] postContent = PostParameters.Encode(postParams);
			webReq.ContentLength = postContent.Length;
			webReq.AllowAutoRedirect = false;
			webReq.ServicePoint.Expect100Continue = false;
			webReq.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8";

			CookieContainer cc = new CookieContainer();
			foreach(Cookie cookie in inCookies)
				cc.Add(cookie);
			webReq.CookieContainer = cc;
			
			string respText;
			try
			{
				using(Stream respS = webReq.GetRequestStream())
					respS.Write(postContent, 0, postContent.Length);

				using (HttpWebResponse resp = (HttpWebResponse)webReq.GetResponse())
				{
					if(resp.ContentLength != 0)
					{
						Encoding enc = Encoding.GetEncoding(resp.CharacterSet);
						using (Stream respS = resp.GetResponseStream())
							respText = enc.GetString(ReadToEnd(respS));
					}
					else
						respText = string.Empty;
				}
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
	}
}

