using System;
using System.Net;
using System.Collections.Specialized;
using System.Collections.Generic;

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
		
		public void FindByOrderNumber(CookieCollection cookies, string orderNumber,
			Action<IList<string>> completed,
			Action<Exception> excepted)
		{
			NameValueCollection postParams = new NameValueCollection
			{
				{"f_orderNumber", orderNumber},

				{"from_day", "01"}, // сервер требует указание интервала дат
				{"from_hour", "00"},
				{"from_min", "00"},
				{"from_month", "01"},
				{"from_year", "1900"},

				{"till_day", "30"},
				{"till_hour", "23"},
				{"till_min", "59"},
				{"till_month", "12"},
				{"till_year", "9999"}
			};
			
			_conn.Request("ReportMaker", cookies, null, postParams,
				(resp) => RbsSiteResponse.PaymentList(resp, completed, excepted),
				excepted);
		}
	}
}

