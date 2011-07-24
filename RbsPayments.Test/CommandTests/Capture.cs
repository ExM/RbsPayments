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
	public class Capture: TestCmdTranslator
	{
		[TestFixtureSetUp]
		public void SetUp()
		{
			SandboxConfigure();
		}
		
		[Test]
		public void IncorrectMdOrder()
		{
			ApiConn.Capture("123", null,
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
			
			ApiConn.Capture(mdOrder, null,
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
		public void PartAmount()
		{
			string orderNum = CreateOrderNumber();
			string mdOrder = Block(orderNum, 123.45m);
			
			ApiConn.Capture(mdOrder, 100m,
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
			
			ApiConn.Capture(mdOrder, 200m,
				(rCode) =>
				{
					//HACK: неожиданно, по видимому если есть доступные средства, то снятие произойдет
					Assert.IsTrue(rCode.Success, "unexpected result code: {0}", rCode);
				},
				(ex) => 
				{
					Assert.Fail("unexpected exception: {0}", ex);
				});
		}
		
		[Test]
		public void TwoPart_AllAmount()
		{
			string orderNum = CreateOrderNumber();
			string mdOrder = Block(orderNum, 123.45m);
			Capture(mdOrder, 100);
			Capture(mdOrder, null);
		}
		
		[Test]
		public void TwoPart_OverAmount()
		{
			string orderNum = CreateOrderNumber();
			string mdOrder = Block(orderNum, 123.45m);
			Capture(mdOrder, 100);
			Capture(mdOrder, 100);
		}
	}
}

