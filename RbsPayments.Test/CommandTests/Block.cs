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
		
		[TestCase("4154810031676743", 101)] // - истек срок действия карты
		[TestCase("4154810084754256", 116)] // - недостаточно средств на карте
		[TestCase("4154810037418173", 120)] // - отказ эмитента проводить транзакцию.
		[TestCase("4154810066940261", 125)] // - неверный номер карты.
		[TestCase("5486736048978929", 902)] // - владелец карты пытается выполнить транзакцию, которая для него не разрешена.
		public void NotSuccessful(string num, int code)
		{
			PaymentCard card = new PaymentCard()
			{
				Number = num,
				CVV = "123",
				Holder = "EXPLORER MACHINE",
				ExpDate = DateTime.UtcNow.AddYears(1)
			};
			
			
			string orderNum = CreateOrderNumber();
			ApiConn.Block(orderNum, 100m, card,
				(result) =>
				{
					Assert.IsFalse(result.Required3DSecure);
					Assert.Greater(result.MdOrder.Length, 10);
					Assert.IsTrue(result.Code.NotSuccessfulTransaction, "unexpected result code: {0}", result.Code);
					Assert.AreEqual(RbsPaymentState.Declined, result.State);
					Assert.AreEqual(code, result.ActionCode);
				},
				(ex) => 
				{
					Assert.Fail("unexpected exception: {0}", ex);
				});
		}
		
		[Test]
		public void FraudDetected()
		{
			PaymentCard card = new PaymentCard()
			{
				Number = "1234567890123456",
				CVV = "123",
				Holder = "EXPLORER MACHINE",
				ExpDate = DateTime.UtcNow.AddYears(1)
			};
			
			
			string orderNum = CreateOrderNumber();
			ApiConn.Block(orderNum, 100m, card,
				(result) =>
				{
					Assert.IsFalse(result.Required3DSecure);
					Assert.Greater(result.MdOrder.Length, 10);
					Assert.IsTrue(result.Code.FraudTransaction, "unexpected result code: {0}", result.Code);
					Assert.AreEqual(RbsPaymentState.Declined, result.State);
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

