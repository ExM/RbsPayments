using System;
using NUnit.Framework;

namespace RbsPayments.Test
{
	public class TestCmdTranslator
	{
		public static Random _rnd = new Random();
		
		public RbsTranslator NoConn;
		public RbsTranslator Sandbox;
		
		public TestCmdTranslator()
		{
			NoConn = CreateSyncConn(Env.NoConn);
			Sandbox = CreateSyncConn(Env.Sandbox);
		}
		
		public static RbsTranslator CreateSyncConn(RbsConnectionConfig cfg)
		{
			SyncConnector conn = new SyncConnector(new Uri(cfg.Uri), TimeSpan.FromSeconds(10));
			return new RbsTranslator(conn, cfg.Merchant, cfg.Refund);
		}
		
		public string CreateOrderNumber()
		{
			//HACK: тут можно сделать проверку на уникальность номера, если изменится поведение сервера
			return _rnd.Next(999999999).ToString();
		}
		
		public string Block(string orderNum, decimal amount)
		{
			RegisterResult result = null;
			Sandbox.Block(orderNum, amount, TestCard.Good,
				(resInfo) =>
				{
					Assert.IsFalse(resInfo.Required3DSecure);
					Assert.Greater(resInfo.MdOrder.Length, 10);
					Assert.IsTrue(resInfo.Code.Success, "unexpected result code: {0}", resInfo.Code);
					Assert.AreEqual(RbsPaymentState.Deposited, resInfo.State);
					result = resInfo;
				},
				(ex) => 
				{
					Assert.Fail("unexpected exception: {0}", ex);
				});
			return result.MdOrder;
		}
		
		public void Capture(string mdOrder, decimal? amount)
		{
			Sandbox.Capture(mdOrder, amount,
				(rCode) =>
				{
					Assert.IsTrue(rCode.Success, "unexpected result code: {0}", rCode);
				},
				(ex) => 
				{
					Assert.Fail("unexpected exception: {0}", ex);
				});
		}
		
		public void Refund(string mdOrder, decimal? amount)
		{
			Sandbox.Refund(mdOrder, amount,
				(rCode) =>
				{
					Assert.IsTrue(rCode.Success, "unexpected result code: {0}", rCode);
				},
				(ex) => 
				{
					Assert.Fail("unexpected exception: {0}", ex);
				});
		}
	}
}

