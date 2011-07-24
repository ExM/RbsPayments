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
	[Category("server required")]
	public class Block: TestCmdTranslator
	{
		[TestFixtureSetUp]
		public void SetUp()
		{
			SandboxConfigure();
		}
		
		[Test]
		public void Success()
		{
			string orderNum = CreateOrderNumber();
			ApiConn.Block(orderNum, 100m, TestCard.Good,
				(result) =>
				{
					Assert.IsFalse(result.Required3DSecure);
					Assert.Greater(result.MdOrder.Length, 10);
					Assert.IsTrue(result.Code.Success, "unexpected result code: {0}", result.Code);
					Assert.AreEqual(RbsPaymentState.Deposited, result.State);
				},
				(ex) => 
				{
					Assert.Fail("unexpected exception: {0}", ex);
				});
		}
		
		[Test]
		public void NotEnoughMoney()
		{
			string orderNum = CreateOrderNumber();
			ApiConn.Block(orderNum, 100m, TestCard.Bad116,
				(result) =>
				{
					Assert.IsFalse(result.Required3DSecure);
					Assert.Greater(result.MdOrder.Length, 10);
					Assert.IsTrue(result.Code.NotSuccessfulTransaction, "unexpected result code: {0}", result.Code);
					Assert.AreEqual(RbsPaymentState.Declined, result.State);
					Assert.AreEqual(116, result.ActionCode);
				},
				(ex) => 
				{
					Assert.Fail("unexpected exception: {0}", ex);
				});
		}
		
		[Test]
		public void IncorrectFormat()
		{
			ApiConn.Block("ABC", 100m, TestCard.Good,
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

