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
	public class CancelBlock: TestCmdTranslator
	{
		[TestFixtureSetUp]
		public void SetUp()
		{
			SandboxConfigure();
		}
		
		[Test]
		public void IncorrectMdOrder()
		{
			ApiConn.CancelBlock("123",
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
		public void SuccessCancelAfterBlock()
		{
			string orderNum = CreateOrderNumber();
			string mdOrder = Block(orderNum, 123.45m);
			
			ApiConn.CancelBlock(mdOrder,
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

