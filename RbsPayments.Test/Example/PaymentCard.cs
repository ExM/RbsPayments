using System;

namespace RbsPayments.Example
{
	/// <summary>
	/// информация для платежа по карте
	/// </summary>
	public class PaymentCard
	{
		/// <summary>
		/// Номер карты
		/// </summary>
		public string Number {get; set;}

		/// <summary>
		/// Полное имя владельца
		/// </summary>
		public string Holder { get; set; }

		/// <summary>
		/// Срок действия карты
		/// </summary>
		public DateTime ExpDate { get; set; }

		/// <summary>
		/// Верификационный CVV код
		/// </summary>
		public string CVV { get; set; }
	}
}

