using System;
using System.Net;
using System.IO;
using System.Text;
using System.Collections.Specialized;
using NLog;
using System.Collections.Generic;
using System.Threading;

namespace RbsPayments
{
	public class AsyncConnector: IConnector
	{
		private static Logger Log = LogManager.GetCurrentClassLogger();
		
		private readonly Uri _baseUri;
		private readonly int _to;
		
		public AsyncConnector(Uri baseUri, TimeSpan to)
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
			
			IAsyncResult result = webReq.BeginGetResponse((ar) =>
			{
				try
				{
					Stream s = webReq.EndGetResponse(ar).GetResponseStream();
					AsyncRead(s, completed, excepted);
				}
				catch(WebException err)
				{
					Log.Debug("WebException: {0}", err);
					excepted(err);
				}
				catch(Exception err)
				{
					Log.Debug("Exception: {0}", err);
					OtherErrorWrap(err, excepted);
				}
			}, null);
			
			ThreadPool.RegisterWaitForSingleObject(result.AsyncWaitHandle, TimeoutCallback, webReq, _to, true);
		}
		
		private static void OtherErrorWrap(Exception ex, Action<Exception> excepted)
		{
			excepted(new WebException("data transmission error", ex, WebExceptionStatus.UnknownError, null));
		}

		private static void TimeoutCallback(object state, bool timedOut)
		{
			if (timedOut)
				((HttpWebRequest)state).Abort();
		}
		
		private static void AsyncRead(Stream source, Action<string> completed, Action<Exception> excepted)
		{
			new CopyStreamContext(
				source,
				1024,
				(buff) =>
				{
					string result;
					try
					{
						result = Encoding.UTF8.GetString(buff);
					}
					catch (Exception err)
					{
						OtherErrorWrap(err, excepted);
						return;
					}
					completed(result);
				},
				(err) => OtherErrorWrap(err, excepted));
		}

		private class CopyStreamContext
		{
			private readonly int _partLenght;
			private long _totalBytes = 0;
			private List<byte[]> _buffers = new List<byte[]>();
			private byte[] _buffer;
			private Stream _src;
			private Action<byte[]> _completed;
			private Action<Exception> _excepted;

			public CopyStreamContext(Stream source, int partLenght, Action<byte[]> completed, Action<Exception> excepted)
			{
				_partLenght = partLenght;
				_buffer = new byte[_partLenght];

				_src = source;
				_completed = completed;
				_excepted = excepted;

				_src.BeginRead(_buffer, 0, _partLenght, OnRead, null);
			}

			private void OnRead(IAsyncResult readResult)
			{
				byte[] localCopy;

				try
				{
					int read = _src.EndRead(readResult);
					if (read > 0)
					{
						localCopy = new byte[read];
						Array.Copy(_buffer, localCopy, read);
						_totalBytes += read;
						_buffers.Add(localCopy);
						_src.BeginRead(_buffer, 0, _partLenght, OnRead, null);
						return;
					}

					int N = _buffers.Count;

					if (N == 1)
					{
						localCopy = _buffers[0];
					}
					else
					{
						localCopy = new byte[_totalBytes];
						int offset = 0;

						for (int i = 0; i < N; i++)
						{
							byte[] cur = _buffers[i];
							Array.Copy(cur, 0, localCopy, offset, cur.Length);
							offset += cur.Length;
						}
					}
				}
				catch (Exception exc)
				{
					_excepted(exc);
					return;
				}

				_completed(localCopy);
			}
		}
	}
}

