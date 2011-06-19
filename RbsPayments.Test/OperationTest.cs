using System;
using NUnit.Framework;
using RbsPayments;
using RbsPayments.Example;
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
			SyncConnector conn = new SyncConnector(new Uri(cfg.Uri), TimeSpan.FromSeconds(20));
			_tr = new RbsTranslator(conn, cfg);
		}
		
		[Test]
		public void Block()
		{
			//HACK: на тестовом сервере допускается дублирование платежей
			_tr.Block("5687340", 100m, TestCard.Good,
				(result) =>
				{
					Assert.IsFalse(result.Required3DSecure);
					Assert.Greater(result.MdOrder.Length, 10);
					Assert.AreEqual(0, result.Code.PrimaryRC);
					Assert.AreEqual(0, result.Code.SecondaryRC);
					Assert.AreEqual(RbsPaymentState.Deposited, result.State);
				},
				(ex) => 
				{
					Assert.Fail("unexpected exception: {0}", ex);
				});
		}
		
		[Test]
		public void Block_3DSec()
		{
			//HACK: на тестовом сервере допускается дублирование платежей
			_tr.Block("5687345", 100.12m, TestCard.Good3DSec,
				(result) =>
				{
					Assert.IsTrue(result.Required3DSecure);
					Assert.Greater(result.MdOrder.Length, 10);
					Assert.IsNotEmpty(result.AcsUrl);
					Assert.IsNotEmpty(result.PaReq);
					Assert.Greater(Convert.FromBase64String(result.PaReq).Length, 350);
				},
				(ex) => 
				{
					Assert.Fail("unexpected exception: {0}", ex);
				});
		}
		
		[Test]
		public void Return_3ds()
		{
			string mdOrder = "109195326-11-4512705498-58-87101-6210973_p1";
			string PaRes = "++"; //HACK: не читаемый PaRes, но ответ сервера "-2 	6 	Эмитент отклонил  VbV/SecureCode аутентификацию."
			_tr.Bpc3ds(mdOrder, PaRes,
				(result) =>
				{
					Assert.Greater(result.MdOrder.Length, 10);
					Assert.AreEqual(0, result.Code.PrimaryRC);
					Assert.AreEqual(0, result.Code.SecondaryRC);
					Assert.AreEqual(RbsPaymentState.Deposited, result.State);
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
		public void Block_IncorrectFormat()
		{
			_tr.Block("ABC", 100m, TestCard.Good,
				(result) =>
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

