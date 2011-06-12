using System;

namespace RbsPayments
{
	/// <summary>
	/// Детализация состояния заказа
	/// </summary>
	public class PaymentInfo
	{
		/// <summary>
		/// Сумма платежа в копейках
		/// </summary>
		public int Amount;
		/// <summary>
		/// Уникальный идентификатор Магазина в Системе РБС
		/// </summary>
		public string MerchantNumber;
		/// <summary>
		/// Уникальный номер заказа созданный Магазином в момент регистрации заказа 
		/// </summary>
		public string OrderNumber;
		/// <summary>
		/// Код авторизации платежа возвращаемый эмитентом карты.
		/// Если не равен 000000, то платеж успешен.
		/// </summary>
		public string ApprovalCode;
		/// <summary>
		/// Значение суммы заблокированной на карте клиента.
		/// Значение указано в копейках. 
		/// </summary>
		public int ApproveAmount;
		/// <summary>
		/// Состояние операции.
		/// </summary>
		public int AuthCode;
		/// <summary>
		/// Дата и время операции 
		/// </summary>
		public DateTime AuthTime;
		/// <summary>
		/// Значение суммы переведенной на счет Магазина с карты клиента.
		/// Значение указано в копейках. 
		/// </summary>
		public DateTime DepositAmount;
		/// <summary>
		/// Маскированный номер карты
		/// </summary>
		public string Pan;
	}
}

