using System;
using System.Net;
using NUnit.Framework;
using RbsPayments;
using RbsPayments.Example;
using Configuration;
using RbsPayments.Test;

namespace RbsPayments.CommandTests
{
	[TestFixture]
	public class QueryOrders: TestCmdTranslator
	{
		[Test]
		public void IncorrectMdOrder()
		{
			Sandbox.QueryOrders("123",
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
		public void SuccessBlock()
		{
			string orderNum = CreateOrderNumber();
			string mdOrder = Block(orderNum, 123.45m);
			
			Sandbox.QueryOrders(mdOrder,
				(rInfo, pInfo, state) =>
				{
					Assert.IsTrue(rInfo.Success, "unexpected result code: {0}", rInfo);
					Assert.AreEqual(RbsPaymentState.Approved, state);
					Assert.AreEqual(orderNum, pInfo.OrderNumber);
					Assert.AreEqual(12345, pInfo.Amount);
					Assert.AreEqual(12345, pInfo.ApproveAmount);
					Assert.AreEqual(0, pInfo.DepositAmount);
					Assert.AreNotEqual("000000", pInfo.ApprovalCode);
					Assert.AreEqual(0, pInfo.AuthCode);
				},
				(ex) => 
				{
					Assert.Fail("unexpected exception: {0}", ex);
				});
		}
	}
}

