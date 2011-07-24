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
	}
}

