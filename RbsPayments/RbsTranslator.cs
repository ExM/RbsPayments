using System;
using System.Net;
using System.Collections.Specialized;

namespace RbsPayments
{
	public class RbsTranslator
	{
		private readonly IConnector _conn;
		private readonly string _merchantNum;
		private readonly string _merchantPass;
		
		public RbsTranslator(IConnector conn, string merchantNum, string merchantPass)
		{
			_conn = conn;
			_merchantNum = merchantNum;
			_merchantPass = merchantPass;
		}
		
		public void Merchant2Rbs(string orderNum, string orderDesc, int amount, string backUrl, bool depositFlag,
			string cardNum, string cardCvc, string cardExpDate, string cardHolderName,
			Action<string, ResultInfo, RbsPaymentState> completed, Action<Exception> excepted)
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
			
			_conn.Request("Merchant2Rbs", getParams,
				(resp) => RbsResponse.Merchant2Rbs(resp, completed, excepted),
				excepted);
		}
		
		/// <summary>
		/// Запрос состояния платежа
		/// </summary>
		/// <param name='mdOrder'>
		/// уникальный идентификатор заказа, полученный при регистрации заказа от Системы РБС
		/// </param>
		/// <param name='completed'>
		/// детализация состояния заказа
		/// </param>
		/// <param name='excepted'>
		/// ошибка операции
		/// </param>
		public void QueryOrders(string mdOrder,
			Action<ResultInfo, PaymentInfo, RbsPaymentState> completed, Action<Exception> excepted)
		{
			NameValueCollection getParams = new NameValueCollection
			{
				{"MDORDER", mdOrder},
				{"MERCHANTPASSWD", _merchantPass}
			};
			
			_conn.Request("QueryOrders", getParams,
				(resp) => RbsResponse.QueryOrders(resp, completed, excepted),
				excepted);
		}


	}
}

