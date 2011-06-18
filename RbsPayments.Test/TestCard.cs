using System;
using RbsPayments.Example;

namespace RbsPayments.Test
{
	/// <summary>
	/// Набор платежных карт для тестовых операций
	/// </summary>
	public static class TestCard
	{
		/// <summary>
		/// 4111111111111112, cvc2/cvv2 код 123
		/// </summary>
		public static PaymentCard Good = new PaymentCard()
		{
			Number = "4111111111111112",
			CVV = "123",
			Holder = "EXPLORER MACHINE",
			ExpDate = DateTime.UtcNow.AddYears(1)
		};
		
		/// <summary>
		/// 5222230546300090, cvc2/cvv2 код 123
		/// </summary>
		public static PaymentCard Good2 = new PaymentCard()
		{
			Number = "5222230546300090",
			CVV = "123",
			Holder = "EXPLORER MACHINE",
			ExpDate = DateTime.UtcNow.AddYears(1)
		};
		
		/// <summary>
		/// 4111111111111111, cvc2/cvv2 код 123
		/// (карта "вовлечена" в 3D-Secure, пароль на странице аутентификации "qweasd")
		/// </summary>
		public static PaymentCard Good3DSec = new PaymentCard()
		{
			Number = "4111111111111111",
			CVV = "123",
			Holder = "EXPLORER MACHINE",
			ExpDate = DateTime.UtcNow.AddYears(1)
		};
		
		/// <summary>
		/// 4154810031676743 дает actionCode 101 - истек срок действия карты
		/// </summary>
		public static PaymentCard Bad101 = new PaymentCard()
		{
			Number = "4154810031676743",
			CVV = "123",
			Holder = "EXPLORER MACHINE",
			ExpDate = DateTime.UtcNow.AddYears(1)
		};
		
		/// <summary>
		/// 4154810084754256 дает actionCode 116 - недостаточно средств на карте
		/// </summary>
		public static PaymentCard Bad116 = new PaymentCard()
		{
			Number = "4154810084754256",
			CVV = "123",
			Holder = "EXPLORER MACHINE",
			ExpDate = DateTime.UtcNow.AddYears(1)
		};
		
		/// <summary>
		/// 4154810037418173 дает actionCode 120 - отказ эмитента проводить транзакцию.
		/// </summary>
		public static PaymentCard Bad120 = new PaymentCard()
		{
			Number = "4154810037418173",
			CVV = "123",
			Holder = "EXPLORER MACHINE",
			ExpDate = DateTime.UtcNow.AddYears(1)
		};
		
		/// <summary>
		/// 4154810066940261 дает actionCode 125 - неверный номер карты.
		/// </summary>
		public static PaymentCard Bad125 = new PaymentCard()
		{
			Number = "4154810066940261",
			CVV = "123",
			Holder = "EXPLORER MACHINE",
			ExpDate = DateTime.UtcNow.AddYears(1)
		};
		
		/// <summary>
		/// 5486736048978929 дает actionCode 902 - владелец карты пытается выполнить транзакцию, которая для него не разрешена.
		/// </summary>
		public static PaymentCard Bad902 = new PaymentCard()
		{
			Number = "5486736048978929",
			CVV = "123",
			Holder = "EXPLORER MACHINE",
			ExpDate = DateTime.UtcNow.AddYears(1)
		};
		
	}
}

