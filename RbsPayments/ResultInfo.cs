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
		
		/// <summary>
		/// Операция запроса прошла успешно
		/// </summary>
		public bool Success
		{
			get
			{
				return PrimaryRC == 0 && SecondaryRC == 0;
			}
		}
		
		/// <summary>
		/// Заказ с данным MDORDER не найден
		/// </summary>
		public bool MdOrderNotFound
		{
			get
			{
				return PrimaryRC == 2 && SecondaryRC == 204;
			}
		}
		
		public override string ToString ()
		{
			return string.Format ("[ResultInfo: PrimaryRC={0}, SecondaryRC={1}]", PrimaryRC, SecondaryRC);
		}
	}
}

