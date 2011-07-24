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
		
		public static void Block(this RbsApi tr, string orderNum, decimal amount, PaymentCard card,
			Action<RegisterResult> completed, Action<Exception> excepted)
		{
			tr.Merchant2Rbs(orderNum, "example payment", ToMinorOfUnit(amount), "www.example.com", false,
				card.Number, card.CVV, card.ExpDate.ToString("yyyyMM"), card.Holder,
				completed, excepted);
		}
		
		public static void Capture(this RbsApi tr, string orderNum, decimal amount, PaymentCard card,
			Action<RegisterResult> completed, Action<Exception> excepted)
		{
			tr.Merchant2Rbs(orderNum, "example payment", ToMinorOfUnit(amount), "www.example.com", true,
				card.Number, card.CVV, card.ExpDate.ToString("yyyyMM"), card.Holder,
				completed, excepted);
		}
		
		public static void Capture(this RbsApi tr, string mdOrder, decimal? amount,
			Action<ResultCode> completed, Action<Exception> excepted)
		{
			int? iAmount = null;
			if(amount.HasValue)
				iAmount = ToMinorOfUnit(amount.Value);
			tr.DepositPayment(mdOrder, iAmount, completed, excepted);
		}
		
		public static void CancelBlock(this RbsApi tr, string mdOrder,
			Action<ResultCode> completed, Action<Exception> excepted)
		{
			tr.DepositReversal(mdOrder, completed, excepted);
		}
		
		public static void Refund(this RbsApi tr, string mdOrder, decimal? amount,
			Action<ResultCode> completed, Action<Exception> excepted)
		{
			if(amount.HasValue)
				tr.Refund(mdOrder, ToMinorOfUnit(amount.Value), completed, excepted);
			else
				tr.Refund(mdOrder, completed, excepted);
		}
	}
}

