using System;
using System.Net;
using System.Collections.Specialized;

namespace RbsPayments
{
	public class RbsSite
	{
		private readonly ISiteConnector _conn;
		private readonly string _user;
		private readonly string _pass;
		
		public RbsSite(ISiteConnector conn, string user, string pass)
		{
			_conn = conn;
			_user = user;
			_pass = pass;
		}
		
		public void LogIn(
			Action<CookieCollection> completed,
			Action<Exception> excepted)
		{
			NameValueCollection postParams = new NameValueCollection
			{
				{"userId", _user},
				{"password", _pass}
			};
			
			_conn.Request("MerchantServlet", null, postParams,
				(resp, cookies) => completed(cookies),
				excepted);
		}
	}
}

