using System;
using System.Net;
using System.Collections.Specialized;

namespace RbsPayments
{
	public class RbsSite
	{
		private readonly ISiteConnector _conn;
		
		public RbsSite(ISiteConnector conn)
		{
			_conn = conn;
		}
		
		public void Login(string user, string pass,
			Action<CookieCollection> completed,
			Action<Exception> excepted)
		{
			NameValueCollection postParams = new NameValueCollection
			{
				{"userId", user},
				{"password", pass}
			};
			
			_conn.Request("MerchantServlet", null, postParams,
				(resp, cookies) => RbsSiteResponse.Login(resp, () => completed(cookies), excepted),
				excepted);
		}
	}
}

