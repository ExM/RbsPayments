using System;
using System.Collections.Specialized;

namespace RbsPayments
{
	public interface IConnector
	{
		void Request(string cmd, NameValueCollection getParams, Action<string> completed, Action<Exception> excepted);
	}
}

