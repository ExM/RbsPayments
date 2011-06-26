using System;
using System.Net;
using System.Collections.Specialized;

namespace RbsPayments
{
	public interface ICommandConnector
	{
		void Request(string cmd, NameValueCollection getParams, Action<string> completed, Action<Exception> excepted);
	}
}

