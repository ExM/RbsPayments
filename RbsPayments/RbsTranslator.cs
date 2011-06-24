using System;
using System.Net;
using System.Collections.Specialized;

namespace RbsPayments
{
	public class RbsTranslator
	{
		private readonly ICommandConnector _conn;
		private readonly string _merchantNum;
		private readonly string _merchantPass;
		private readonly string _refundUser;
		private readonly string _refundPass;
		
		public RbsTranslator(ICommandConnector conn, string merchantNum, string merchantPass, string refundUser, string refundPass)
		{
			_conn = conn;
			_merchantNum = merchantNum;
			_merchantPass = merchantPass;
			_refundUser = refundUser;
			_refundPass = refundPass;
		}
		
		public RbsTranslator(ICommandConnector conn, RbsConnectionConfig cfg)
			:this(conn, cfg.MerchantNumber, cfg.MerchantPassword, cfg.User, cfg.Password)
		{
		}
		
		public void Merchant2Rbs(string orderNum, string orderDesc, int amount, string backUrl, bool depositFlag,
			string cardNum, string cardCvc, string cardExpDate, string cardHolderName,
			Action<RegisterResult> completed, Action<Exception> excepted)
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
			Action<ResultCode, PaymentInfo, RbsPaymentState> completed, Action<Exception> excepted)
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
				{"MERCHANTPASSWD", _merchantPass}
			};
			
			if(amount.HasValue)
				getParams.Add("DEPOSIT_AMOUNT", amount.Value.ToString());
			
			_conn.Request("DepositPayment", getParams,
				(resp) => RbsResponse.DepositPayment(resp, completed, excepted),
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
				{"MERCHANTPASSWD", _merchantPass}
			};
			
			_conn.Request("DepositReversal", getParams,
				(resp) => RbsResponse.DepositReversal(resp, completed, excepted),
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
				{"MERCHANTPASSWD", _merchantPass},
				{"user", _refundUser},
				{"pwd", _refundPass}
			};
			
			_conn.Request("DepositReversal", getParams,
				(resp) => RbsResponse.DepositReversal(resp, completed, excepted),
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
				{"MERCHANTPASSWD", _merchantPass},
				{"AMOUNT", amount.ToString()},
				{"user", _refundUser},
				{"pwd", _refundPass}
			};
			
			_conn.Request("Refund", getParams,
				(resp) => RbsResponse.Refund(resp, completed, excepted),
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
				(resp) => RbsResponse.Merchant2Rbs(resp,
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

