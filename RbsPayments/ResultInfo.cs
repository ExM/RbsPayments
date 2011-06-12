using System;

namespace RbsPayments
{
	/// <summary>
	/// Результат выполнения операции
	/// </summary>
	public struct ResultInfo
	{
		/// <summary>
		/// Результат прохождения команды
		/// </summary>
		public int PrimaryRC;
		/// <summary>
		/// Результат прохождения команды
		/// </summary>
		public int SecondaryRC;
	}
}

