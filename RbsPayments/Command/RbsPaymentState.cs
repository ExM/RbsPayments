using System;

namespace RbsPayments
{
	/// <summary>
	/// Состояние платежа в системе RBS
	/// </summary>
	public enum RbsPaymentState
	{
		/// <summary>
		/// состояние не определено
		/// </summary>
		Unknown = -1,
		
		/// <summary>
		/// Платеж предавторизован.
		/// Состояние означает, что процесс предавторизации платежа в Системе РБС прошел успешно.
		/// Для данного состояния могут быть выполнены команды поставторизации или отмены предавторизации платежа.
		/// </summary>
		Approved,
		
		/// <summary>
		/// Платеж поставторизован.
		/// Состояние означает, что процесс поставторизации платежа в Системе РБС прошел успешно.
		/// Для данного состояния может быть выполнена команда отмены поставторизации платежа.
		/// </summary>
		Deposited,
		
		/// <summary>
		/// Состояние означает, что процесс предавторизации платежа в Системе РБС был отклонен Банком.
		/// </summary>
		Declined,
		
		/// <summary>
		/// Состояние означает, что процесс отмены предавторизации платежа в Системе РБС прошел успешно.
		/// </summary>
		Void,
		
		/// <summary>
		/// Состояние означает, что платеж находится в ожидании ответа от процессингового центра Банка-экайера.
		/// Для данного состояния не может быть выполнена никакая команда.
		/// </summary>
		Pending,
		
		/// <summary>
		/// Был произведен возврат денег на карту клиента.
		/// </summary>
		Refunded,
		
		/// <summary>
		/// Была неуспешная попытка возврата денег на карту клиента.
		/// </summary>
		RefundFailed
	}
}

