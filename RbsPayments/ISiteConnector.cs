using System;
using System.Net;
using System.Collections.Specialized;

namespace RbsPayments
{
	public interface ISiteConnector
	{
		void Request(string cmd, NameValueCollection getParams, NameValueCollection postParams,
			Action<string, CookieCollection> completed, Action<Exception> excepted);
	}
}

