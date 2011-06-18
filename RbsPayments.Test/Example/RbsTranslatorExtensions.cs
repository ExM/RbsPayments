using System;
using System.Net;
using System.Collections.Specialized;
using RbsPayments.Example;

namespace RbsPayments
{
	/// <summary>
	/// Пример специализации под конкретные задачи клиента
	/// </summary>
	public static class RbsTranslatorExtensions
	{
		/// <summary>
		/// переводит сумму платежа из рублей в копейки
		/// </summary>
		/// <returns>
		/// число копеек
		/// </returns>
		/// <param name='amount'>
		/// сумма в рублях
		/// </param>
		public static int ToMinorOfUnit(decimal amount)
		{
			decimal muAmount = amount * 100;
			int iMuAmount = (int)Math.Truncate(muAmount);
			if(muAmount != iMuAmount)
				throw new ArgumentOutOfRangeException("amount must contain an integer cents");
			
			return iMuAmount;
		}
		
		public static void Block(this RbsTranslator tr, string orderNum, decimal amount, PaymentCard card,
			Action<string, ResultInfo, RbsPaymentState> completed,
			Action<string, string, string> req3DSecure,
			Action<Exception> excepted)
		{
			tr.Merchant2Rbs(orderNum, "example payment", ToMinorOfUnit(amount), "www.example.com", false,
				card.Number, card.CVV, card.ExpDate.ToString("yyyyMM"), card.Holder,
				completed, req3DSecure, excepted);
		}
		
		public static void Capture(this RbsTranslator tr, string orderNum, decimal amount, PaymentCard card,
			Action<string, ResultInfo, RbsPaymentState> completed,
			Action<string, string, string> req3DSecure,
			Action<Exception> excepted)
		{
			tr.Merchant2Rbs(orderNum, "example payment", ToMinorOfUnit(amount), "www.example.com", true,
				card.Number, card.CVV, card.ExpDate.ToString("yyyyMM"), card.Holder,
				completed, req3DSecure, excepted);
		}
		
		public static void Capture(this RbsTranslator tr, string mdOrder, decimal? amount,
			Action<ResultInfo> completed, Action<Exception> excepted)
		{
			int? iAmount = null;
			if(amount.HasValue)
				iAmount = ToMinorOfUnit(amount.Value);
			tr.DepositPayment(mdOrder, iAmount, completed, excepted);
		}
		
		public static void CancelBlock(this RbsTranslator tr, string mdOrder,
			Action<ResultInfo> completed, Action<Exception> excepted)
		{
			tr.DepositReversal(mdOrder, completed, excepted);
		}
	}
}

