using System;

namespace RbsPayments
{
	/// <summary>
	/// Результат регистрации платежа
	/// </summary>
	public class RegisterResult
	{
		/// <summary>
		/// Уникальный идентификатор заказа, полученный при регистрации заказа от Системы РБС
		/// </summary>
		public readonly string MdOrder;
		/// <summary>
		/// Код возврата
		/// </summary>
		public readonly ResultCode Code;
		/// <summary>
		/// Параметр более подробно описывает причину отклонения платежа
		/// </summary>
		public readonly int ActionCode;
		/// <summary>
		/// Состояние платежа
		/// </summary>
		public readonly RbsPaymentState State;
		/// <summary>
		/// Ссылка на страницу сайта Банка-Эмитента, созданная банком для авторизации владельца карты
		/// </summary>
		public readonly string AcsUrl;
		/// <summary>
		/// Упакованное с помощью алгоритма BASE64 значение запроса аутентификации покупки
		/// </summary>
		public readonly string PaReq;
		/// <summary>
		/// Платежу требуется авторизация 3D secure
		/// </summary>
		public readonly bool Required3DSecure;
		
		/// <summary>
		/// Плятеж не зарегистрирован
		/// </summary>
		/// <param name='resCode'>
		/// код возврата об ошибке
		/// </param>
		public RegisterResult(ResultCode resCode)
		{
			MdOrder = null;
			Code = resCode;
			ActionCode = 0;
			State = RbsPaymentState.Unknown;
			AcsUrl = null;
			PaReq = null;
			Required3DSecure = false;
		}
		
		/// <summary>
		/// Платеж успешно зарегистрирован
		/// </summary>
		/// <param name='mdOrder'>
		/// уникальный идентификатор заказа, полученный при регистрации заказа от Системы РБС
		/// </param>
		/// <param name='resCode'>
		/// код возврата
		/// </param>
		/// <param name='state'>
		/// состояние платежа
		/// </param>
		/// <param name='actionCode'>
		/// Параметр более подробно описывает причину отклонения платежа
		/// </param>
		public RegisterResult(string mdOrder, ResultCode resCode, RbsPaymentState state, int actionCode)
		{
			MdOrder = mdOrder;
			Code = resCode;
			ActionCode = actionCode;
			State = state;
			AcsUrl = null;
			PaReq = null;
			Required3DSecure = false;
		}
		
		/// <summary>
		/// Для регистрации платежа требуется авторизация 3D secure
		/// </summary>
		/// <param name='mdOrder'>
		/// уникальный идентификатор заказа, полученный при регистрации заказа от Системы РБС
		/// </param>
		/// <param name='acsUrl'>
		/// ссылка на страницу сайта Банка-Эмитента, созданная банком для авторизации владельца карты
		/// </param>
		/// <param name='paReq'>
		/// упакованное с помощью алгоритма BASE64 значение запроса аутентификации покупки
		/// </param>
		public RegisterResult(string mdOrder, string acsUrl, string paReq)
		{
			MdOrder = mdOrder;
			Code = new ResultCode(){PrimaryRC = 0, SecondaryRC = 0};
			ActionCode = 0;
			State = RbsPaymentState.Unknown;
			AcsUrl = acsUrl;
			PaReq = paReq;
			Required3DSecure = true;
		}
	}
}

