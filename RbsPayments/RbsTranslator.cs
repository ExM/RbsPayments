using System;
using System.Net;
using System.Collections.Specialized;

namespace RbsPayments
{
	public abstract class RbsTranslator
	{
		private readonly string _merchantNum;
		private readonly string _merchantPass;
		
		public RbsTranslator(string merchantNum, string merchantPass)
		{
			_merchantNum = merchantNum;
			_merchantPass = merchantPass;
		}
		
		public void Merchant2Rbs(string orderNum, string orderDesc, int amount, string backUrl, bool depositFlag,
			string cardNum, string cardCvc, string cardExpDate, string cardHolderName,
			Action<string, int, int, RbsPaymentState> completed, Action<Exception> excepted)
		{
			NameValueCollection getParams = new NameValueCollection
			{
				{"MERCHANTNUMBER", _merchantNum},
				{"ORDERNUMBER", orderNum},
				{"AMOUNT", amount.ToString()},
				{"BACKURL", backUrl},
				{"$ORDERDESCRIPTION", orderDesc},
				{"LANGUAGE", "RU"},
				{"DEPOSITFLAG", depositFlag?"1":"0"},
				{"MERCHANTPASSWD", _merchantPass},
				{"PAN_MKO", cardNum},
				{"CVC_MKO", cardCvc},
				{"EXP_MKO", cardExpDate},
				{"CARDHOLDER_MKO", cardHolderName},
				{"CHARSET", "UTF-8"}
			};
			
			Request("Merchant2Rbs", getParams,
				(resp) => completed(resp, 0, 0, RbsPaymentState.Deposited),
				excepted);
		}
		
		protected abstract void Request(string cmd, NameValueCollection getParams,
			Action<string> completed, Action<Exception> excepted);
	}
}

