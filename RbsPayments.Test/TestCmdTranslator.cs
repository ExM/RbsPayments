using System;
using NUnit.Framework;

namespace RbsPayments.Test
{
	public class TestCmdTranslator: Env
	{
		public string Block(string orderNum, decimal amount)
		{
			RegisterResult result = null;
			ApiConn.Block(orderNum, amount, TestCard.Good,
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
			ApiConn.Capture(mdOrder, amount,
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
			ApiConn.Refund(mdOrder, amount,
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

