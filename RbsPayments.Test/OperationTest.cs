using System;
using NUnit.Framework;
using RbsPayments;

namespace RbsPayments.Test
{
	[TestFixture]
	public class OperationTest
	{
		[Test]
		public void Block()
		{
			SyncConnector conn = new SyncConnector("http://playground.paymentgate.ru/bpcservlet", TimeSpan.FromSeconds(10));
			RbsTranslator translator = new RbsTranslator(conn, "118600118603000118604", "lazY2k");
			translator.Merchant2Rbs("5687340", "test", 1000, "www", false, "4111111111111112", "123", "201110", "Card Holder",
				(morder, f, s, state) =>
			{
				Console.WriteLine(morder);
				
			},
			(ex) => 
			{
				Console.WriteLine(ex);
				
			});
			
			
		}
	}
}

