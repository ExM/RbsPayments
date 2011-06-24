using System;

namespace RbsPayments.Test
{
	public static class ExpectedMessage
	{
		public const string IncorectMdOrder = "System error =String index out of range: 6 \n" + 
				" may be some entered data is in incorrect format, try again";
		
		public const string ErrorAuth = "Вы ввели неправильный логин/пароль. В доступе отказано.";
	}
}

