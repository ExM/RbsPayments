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
	public class Block: TestCmdTranslator
	{
		[Test]
		public void Success()
		{
			string orderNum = CreateOrderNumber();
			Sandbox.Block(orderNum, 100m, TestCard.Good,
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
		public void IncorrectFormat()
		{
			Sandbox.Block("ABC", 100m, TestCard.Good,
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
		
		[Test]
		[Category("NoConn")]
		public void NoConnection()
		{
			NoConn.Block("ABC", 100m, TestCard.Good,
				(result) =>
				{
					Assert.Fail("missed error");
				},
				(ex) => 
				{
					Assert.IsInstanceOf<WebException>(ex, "unexpected exception: {0}", ex);
				});
		}
	}
}

