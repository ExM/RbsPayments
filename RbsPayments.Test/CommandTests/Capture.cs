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
	public class Capture: TestCmdTranslator
	{
		[Test]
		public void IncorrectMdOrder()
		{
			Sandbox.Capture("123", null,
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
			
			Sandbox.Capture(mdOrder, null,
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
			
			Sandbox.Capture(mdOrder, 100m,
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
			
			Sandbox.Capture(mdOrder, 200m,
				(rCode) =>
				{
					//HACK: неожиданно
					Assert.IsTrue(rCode.Success, "unexpected result code: {0}", rCode);
				},
				(ex) => 
				{
					Assert.Fail("unexpected exception: {0}", ex);
				});
		}
	}
}

