using System;
using NUnit.Framework;
using RbsPayments;
using Configuration;

namespace RbsPayments.Test
{
	[TestFixture]
	public class OperationTest
	{
		[Test]
		public void Block()
		{
			RbsConnectionConfig cfg = Env.AppSettings.Load<RbsConnectionConfig>(EmptyResult.Throw);
			SyncConnector conn = new SyncConnector(cfg.Uri, TimeSpan.FromSeconds(10));
			RbsTranslator translator = new RbsTranslator(conn, cfg.MerchantNumber, cfg.MerchantPassword);
			translator.Merchant2Rbs("5687340", "test", 1000, "www", false, "4111111111111112", "123", "201110", "Card Holder",
				(morder, f, s, state) =>
			{
				Assert.Greater(morder.Length, 10);
				Assert.AreEqual(0, f);
				Assert.AreEqual(0, s);
				Assert.AreEqual(RbsPaymentState.Deposited, state);
			},
			(ex) => 
			{
				Assert.Fail("unexpected exception: {0}", ex);
			});
		}
		
		[Test]
		public void NoBlock_ParamsError()
		{
			RbsConnectionConfig cfg = Env.AppSettings.Load<RbsConnectionConfig>(EmptyResult.Throw);
			SyncConnector conn = new SyncConnector(cfg.Uri, TimeSpan.FromSeconds(10));
			RbsTranslator translator = new RbsTranslator(conn, cfg.MerchantNumber, cfg.MerchantPassword);
			translator.Merchant2Rbs("ABC", "test", 100, "www", false, "4111111111111112", "123", "201110", "Card Holder",
				(morder, f, s, state) =>
			{
				Assert.Greater(morder.Length, 10);
				Assert.AreEqual(0, f);
				Assert.AreEqual(0, s);
				Assert.AreEqual(RbsPaymentState.Deposited, state);
			},
			(ex) => 
			{
				Assert.Fail("unexpected exception: {0}", ex);
			});
		}
	}
}

