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
				
				//{"BANK_CC", ""},
				//{"IP", ""},
				//{"btnSubmit", "Âûïîëíèòü"},
				//{"f_isFirst", "true"},
				//{"f_maxAmount", ""},
				//{"f_minAmount", ""},
				//{"f_orderNumber", "123"},
				//{"fromDate", ""},
				{"from_day", "24"},
				{"from_hour", "00"},
				{"from_min", "00"},
				{"from_month", "06"},
				{"from_year", "2008"},
				//{"pan", ""},
				//{"payment_state", "0"},
				//{"tillDate", ""},
				{"till_day", "24"},
				{"till_hour", "23"},
				{"till_min", "59"},
				{"till_month", "06"},
				{"till_year", "2011"}
			};
			
			_conn.Request("ReportMaker", cookies, null, postParams,
				(resp) => RbsSiteResponse.PaymentList(resp, completed, excepted),
				excepted);
		}
	}
}

