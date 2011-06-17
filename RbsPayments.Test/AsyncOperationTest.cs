using System;
using NUnit.Framework;
using RbsPayments;
using Configuration;
using System.Threading;

namespace RbsPayments.Test
{
	[TestFixture]
	[Category("required link to playground.paymentgate.ru")]
	public class AsyncOperationTest
	{
		RbsTranslator _tr;
		
		[TestFixtureSetUp]
		public void SetUp()
		{
			RbsConnectionConfig cfg = Env.AppSettings.Load<RbsConnectionConfig>(EmptyResult.Throw, "RbsSandbox");
			AsyncConnector conn = new AsyncConnector(new Uri(cfg.Uri), TimeSpan.FromSeconds(10));
			_tr = new RbsTranslator(conn, cfg.MerchantNumber, cfg.MerchantPassword);
		}

		[Test]
		public void Merchant2Rbs_IncorrectFormat()
		{
			ManualResetEvent wait =  new ManualResetEvent(false);
			Exception ex = null;
			
			_tr.Merchant2Rbs("ABC", "test", 100, "www", false, "4111111111111112", "123", "201110", "Card Holder",
				(morder, rInfo, state) =>
				{
					wait.Set();
				},
				(iex) => 
				{
					ex = iex;
					wait.Set();
				});
			
			Assert.IsTrue(wait.WaitOne(10000), "elapsed 10 seconds");
			Assert.IsNotNull(ex, "missed error");
			Assert.IsInstanceOf<InvalidOperationException>(ex);
			Assert.IsTrue(ex.Message.Contains("ABC"), "not contain `ABC' in `{0}'", ex.Message);
		}
	}
}

