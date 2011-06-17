using System;
using NUnit.Framework;
using RbsPayments;
using Configuration;

namespace RbsPayments.Test
{
	[TestFixture]
	[Category("required link to playground.paymentgate.ru")]
	public class OperationTest
	{
		RbsTranslator _tr;
		
		[TestFixtureSetUp]
		public void SetUp()
		{
			RbsConnectionConfig cfg = Env.AppSettings.Load<RbsConnectionConfig>(EmptyResult.Throw, "RbsSandbox");
			SyncConnector conn = new SyncConnector(new Uri(cfg.Uri), TimeSpan.FromSeconds(10));
			_tr = new RbsTranslator(conn, cfg.MerchantNumber, cfg.MerchantPassword);
		}
		
		[Test]
		public void Merchant2Rbs()
		{
			//HACK: на тестовом сервере допускается дублирование платежей
			_tr.Merchant2Rbs("5687340", "test", 1000, "www", false, "4111111111111112", "123", "201110", "Card Holder",
				(morder, rInfo, state) =>
				{
					Assert.Greater(morder.Length, 10);
					Assert.AreEqual(0, rInfo.PrimaryRC);
					Assert.AreEqual(0, rInfo.SecondaryRC);
					Assert.AreEqual(RbsPaymentState.Deposited, state);
				},
				(ex) => 
				{
					Assert.Fail("unexpected exception: {0}", ex);
				});
		}
		
		[Test]
		public void QueryOrders_IncorrectMdOrder()
		{
			_tr.QueryOrders("123",
				(rInfo, pInfo, state) =>
				{
					Assert.Fail("missed error");
				},
				(ex) => 
				{
					Assert.IsInstanceOf<InvalidOperationException>(ex, "current exception: {0}", ex);
					Assert.AreEqual(ExpectedMessage.IncorectMdOrder, ex.Message);
				});
		}
		
		[Test]
		public void QueryOrders()
		{
			_tr.QueryOrders("98-4822-4978-117-5986-54119-41-2591-42114-19_p1",
				(rInfo, pInfo, state) =>
				{
					//TODO: создать новый платеж перед этой проверкой
				},
				(ex) => 
				{
					Assert.Fail("unexpected exception: {0}", ex);
				});
		}
		
		[Test]
		public void Merchant2Rbs_IncorrectFormat()
		{
			_tr.Merchant2Rbs("ABC", "test", 100, "www", false, "4111111111111112", "123", "201110", "Card Holder",
				(morder, rInfo, state) =>
				{
					Assert.Fail("missed error");
				},
				(ex) => 
				{
					Assert.IsInstanceOf<InvalidOperationException>(ex);
					Assert.IsTrue(ex.Message.Contains("ABC"), "not contain `ABC' in `{0}'", ex.Message);
				});
		}
	}
}

