using System;
using System.Net;
using System.Collections.Specialized;

namespace RbsPayments
{
	public class RbsApi
	{
		private readonly ICommandConnector _conn;
		private readonly AuthenticateConfig _merchant;
		private readonly AuthenticateConfig _refund;
		
		public RbsApi(ICommandConnector conn, string merchantNum, string merchantPass, string refundUser, string refundPass)
		{
			_conn = conn;
			_merchant = new AuthenticateConfig {User = merchantNum, Pass = merchantPass};
			_refund = new AuthenticateConfig {User = refundUser, Pass = refundPass};
		}
		
		public RbsApi(ICommandConnector conn, AuthenticateConfig merchant, AuthenticateConfig refund)
			:this(conn, merchant.User, merchant.Pass, refund.User, refund.Pass)
		{
		}
		
		public void Merchant2Rbs(string orderNum, string orderDesc, int amount, string backUrl, bool depositFlag,
			string cardNum, string cardCvc, string cardExpDate, string cardHolderName,
			Action<RegisterResult> completed, Action<Exception> excepted)
		{
			NameValueCollection getParams = new NameValueCollection
			{
				{"MERCHANTNUMBER", _merchant.User},
				{"ORDERNUMBER", orderNum},
				{"AMOUNT", amount.ToString()},
				{"BACKURL", backUrl},
				{"$ORDERDESCRIPTION", orderDesc},
				{"LANGUAGE", "RU"},
				{"DEPOSITFLAG", depositFlag?"1":"0"},
				{"MERCHANTPASSWD", _merchant.Pass},
				{"PAN_MKO", cardNum},
				{"CVC_MKO", cardCvc},
				{"EXP_MKO", cardExpDate},
				{"CARDHOLDER_MKO", cardHolderName},
				{"CHARSET", "UTF-8"}
			};
			
			_conn.Request("Merchant2Rbs", getParams,
				(resp) => RbsApiResponse.Merchant2Rbs(resp, completed, excepted),
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
			Action<ResultCode, PaymentInfo, RbsPaymentState> completed, Action<Exception> excepted)
		{
			NameValueCollection getParams = new NameValueCollection
			{
				{"MDORDER", mdOrder},
				{"MERCHANTPASSWD", _merchant.Pass}
			};
			
			_conn.Request("QueryOrders", getParams,
				(resp) => RbsApiResponse.QueryOrders(resp, completed, excepted),
				excepted);
		}
		
		/// <summary>
		/// Завершение двухстадийного платежа. (Проведение отложенной авторизации).
		/// </summary>
		/// <param name='mdOrder'>
		/// Уникальный идентификатор заказа, полученный при регистрации заказа от Системы РБС
		/// </param>
		/// <param name='amount'>
		/// Необязательный параметр. Сумма в копейках, на которую будет производно завершение платежа.
		/// Блокировка средств на карте на сумму разницы между заблокированной суммой и суммой завершения будет
		/// автоматически отменена. Если данный параметр не указан то с карты покупателя будет списана целиком
		/// вся сумма, на которую первоначально была сделана блокировка. 
		/// </param>
		/// <param name='completed'>
		/// результат выполнения операции
		/// </param>
		/// <param name='excepted'>
		/// ошибка операции
		/// </param>
		public void DepositPayment(string mdOrder, int? amount,
			Action<ResultCode> completed, Action<Exception> excepted)
		{
			NameValueCollection getParams = new NameValueCollection
			{
				{"MDORDER", mdOrder},
				{"MERCHANTPASSWD", _merchant.Pass}
			};
			
			if(amount.HasValue)
				getParams.Add("DEPOSIT_AMOUNT", amount.Value.ToString());
			
			_conn.Request("DepositPayment", getParams,
				(resp) => RbsApiResponse.DepositPayment(resp, completed, excepted),
				excepted);
		}
		
		/// <summary>
		/// Инициация Снятие блокировки средств на счету клиента.
		/// </summary>
		/// <param name='mdOrder'>
		/// Уникальный идентификатор заказа, полученный при регистрации заказа от Системы РБС.
		/// </param>
		/// <param name='completed'>
		/// результат выполнения операции
		/// </param>
		/// <param name='excepted'>
		/// ошибка операции
		/// </param>
		public void DepositReversal(string mdOrder,
			Action<ResultCode> completed, Action<Exception> excepted)
		{
			NameValueCollection getParams = new NameValueCollection
			{
				{"MDORDER", mdOrder},
				{"MERCHANTPASSWD", _merchant.Pass}
			};
			
			_conn.Request("DepositReversal", getParams,
				(resp) => RbsApiResponse.DepositReversal(resp, completed, excepted),
				excepted);
		}
		
		/// <summary>
		/// Инициация отмены одностадийного платежа.
		/// </summary>
		/// <param name='mdOrder'>
		/// Уникальный идентификатор заказа, полученный при регистрации заказа от Системы РБС.
		/// </param>
		/// <param name='completed'>
		/// результат выполнения операции
		/// </param>
		/// <param name='excepted'>
		/// ошибка операции
		/// </param>
		public void Refund(string mdOrder,
			Action<ResultCode> completed, Action<Exception> excepted)
		{
			NameValueCollection getParams = new NameValueCollection
			{
				{"MDORDER", mdOrder},
				{"MERCHANTPASSWD", _merchant.Pass},
				{"user", _refund.User},
				{"pwd", _refund.Pass}
			};
			
			_conn.Request("DepositReversal", getParams,
				(resp) => RbsApiResponse.DepositReversal(resp, completed, excepted),
				excepted);
		}
		
		/// <summary>
		/// Возврат денег на карту
		/// </summary>
		/// <param name='mdOrder'>
		/// уникальный идентификатор заказа, полученный при регистрации заказа от системы РБС
		/// </param>
		/// <param name='amount'>
		/// Сумма возврата в копейках
		/// </param>
		/// <param name='completed'>
		/// результат выполнения операции
		/// </param>
		/// <param name='excepted'>
		/// ошибка операции
		/// </param>
		public void Refund(string mdOrder, int amount,
			Action<ResultCode> completed, Action<Exception> excepted)
		{
			NameValueCollection getParams = new NameValueCollection
			{
				{"MDORDER", mdOrder},
				{"MERCHANTPASSWD", _merchant.Pass},
				{"AMOUNT", amount.ToString()},
				{"user", _refund.User},
				{"pwd", _refund.Pass}
			};
			
			_conn.Request("Refund", getParams,
				(resp) => RbsApiResponse.Refund(resp, completed, excepted),
				excepted);
		}
		
		public void Bpc3ds(string mdOrder, string paRes,
			Action<RegisterResult> completed,
			Action<Exception> excepted)
		{
			NameValueCollection getParams = new NameValueCollection
			{
				{"MD", mdOrder},
				{"PaRes", paRes}
			};
			
			_conn.Request("BPC3DS", getParams,
				(resp) => RbsApiResponse.Merchant2Rbs(resp,
					(result) =>
					{
						if(result.Required3DSecure)
							excepted(new InvalidOperationException("again 3DS required"));
						else
							completed(result);
					}, excepted),
				excepted);
		}

	}
}

