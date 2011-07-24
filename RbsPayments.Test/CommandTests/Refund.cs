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
	public class Refund: TestCmdTranslator
	{
		[TestFixtureSetUp]
		public void SetUp()
		{
			SandboxConfigure();
		}
		
		[Test]
		public void IncorrectMdOrder()
		{
			ApiConn.Refund("123",
				(rCode) =>
				{
					Assert.IsTrue(rCode.MdOrderNotFound, "unexpected result code: {0}", rCode);
				},
				(ex) => 
				{
					Assert.Fail("unexpected exception: {0}", ex);
				});
		}
		
		[Test]
		public void AllAmount()
		{
			string orderNum = CreateOrderNumber();
			string mdOrder = Block(orderNum, 123.45m);
			Capture(mdOrder, null);
			
			ApiConn.Refund(mdOrder,
				(rCode) =>
				{
					Assert.IsTrue(rCode.Success, "unexpected result code: {0}", rCode);
				},
				(ex) => 
				{
					Assert.Fail("unexpected exception: {0}", ex);
				});
		}
		
		[Test]
		public void PartAmountAfterAllCapture()
		{
			string orderNum = CreateOrderNumber();
			string mdOrder = Block(orderNum, 123.45m);
			Capture(mdOrder, null);
			
			ApiConn.Refund(mdOrder, 100m,
				(rCode) =>
				{
					Assert.IsTrue(rCode.Success, "unexpected result code: {0}", rCode);
				},
				(ex) => 
				{
					Assert.Fail("unexpected exception: {0}", ex);
				});
		}
		
		[Test]
		public void OverAmount()
		{
			string orderNum = CreateOrderNumber();
			string mdOrder = Block(orderNum, 123.45m);
			Capture(mdOrder, null);
			ApiConn.Refund(mdOrder, 200m,
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

