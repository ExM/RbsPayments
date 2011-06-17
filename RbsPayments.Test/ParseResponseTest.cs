using System;
using NUnit.Framework;
using RbsPayments;

namespace RbsPayments.Test
{
	[TestFixture]
	public class ParseResponseTest
	{
		[Test]
		public void Merchant2Rbs_Deposited()
		{
			string text = "MDORDER=98-4822-4978-117-5986-54119-41-2591-42114-19_p1&" +
				"ANSWER=%3C%3Fxml+version%3D%221.0%22+encoding%3D%22UTF-8%22%3F%3E%0A%3CPSApiResult" +
				"+primaryRC%3D%220%22+secondaryRC%3D%220%22%2F%3E&STATE=payment_deposited";
			
			RbsResponse.Merchant2Rbs(text, 
			(morder, rInfo, state) =>
			{
				Assert.AreEqual("98-4822-4978-117-5986-54119-41-2591-42114-19_p1", morder);
				Assert.AreEqual(0, rInfo.PrimaryRC);
				Assert.AreEqual(0, rInfo.SecondaryRC);
				Assert.AreEqual(RbsPaymentState.Deposited, state);
			},
			(ex) => 
			{
				Assert.Fail("unexpected exception: {0}", ex);
			});
		}
		
		[Test]
		public void Merchant2Rbs_IncorrectFormat()
		{
			string text = "orderNumber is not a number\r\nSystem error =For input string: \"ABC\" <p> may be" +
				" some enetered data is in incorrect format, try again\r\n";
			
			RbsResponse.Merchant2Rbs(text, 
			(morder, rInfo, state) =>
			{
				Assert.Fail("missed error");
			},
			(ex) => 
			{
				Assert.IsInstanceOf<InvalidOperationException>(ex);
				Assert.AreEqual(text, ex.Message);
			});
		}
		
		[Test]
		public void DateTimeParse()
		{
			DateTime dt = RbsResponse.ParseTime("Fri Jun 10 17:16:17 MSD 2011");
			Assert.AreEqual(new DateTime(2011, 6, 10, 17, 16, 17, DateTimeKind.Unspecified), dt);
		}
		
		[Test]
		public void QueryOrders_Approved()
		{
			string text = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\r\n" +
				"<PSApiResult objectCount=\"1\" primaryRC=\"0\" secondaryRC=\"0\">"+
				"<PSOrder PAN=\"411111**1112\" amount=\"1000\" currency=\"810\" expiry=\"201110\" "+
				"merchantNumber=\"118600604\" orderNumber=\"5687340\" state=\"order_ordered\">"+
				"<PaymentCollection><PSPayment approvalCode=\"123456\" approveAmount=\"1000\" authCode=\"0\" "+
				"authTime=\"Fri Jun 10 17:16:17 MSD 2011\" depositAmount=\"0\" paymentNumber=\"1\" "+
				"paymentType=\"BPC\" payment_state=\"payment_approved\"/></PaymentCollection></PSOrder>"+
				"</PSApiResult><!-- transaction_type=SSL_transaction -->";

			RbsResponse.QueryOrders(text, 
			(rInfo, pInfo, state) =>
			{
				Assert.AreEqual(1000, pInfo.Amount);
				Assert.AreEqual("123456", pInfo.ApprovalCode);
				Assert.AreEqual(1000, pInfo.ApproveAmount);
				Assert.AreEqual(0, pInfo.AuthCode);
				Assert.AreEqual(new DateTime(2011, 6, 10, 17, 16, 17, DateTimeKind.Unspecified), pInfo.AuthTime);
				Assert.AreEqual(0, pInfo.DepositAmount);
				Assert.AreEqual("118600604", pInfo.MerchantNumber);
				Assert.AreEqual("5687340", pInfo.OrderNumber);
				Assert.AreEqual("411111**1112", pInfo.Pan);
				Assert.AreEqual(RbsPaymentState.Approved, state);
				Assert.AreEqual(0, rInfo.PrimaryRC);
				Assert.AreEqual(0, rInfo.SecondaryRC);
			},
			(ex) => 
			{
				Assert.Fail("unexpected exception: {0}", ex);
			});
		}
		
		[Test]
		public void QueryOrders_IncorrectMdOrder()
		{
			string text = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\r\n<error>System error =String index out of " +
				"range: 6 <p> may be some entered data is in incorrect format, try again</error>\r\n";
			
			RbsResponse.QueryOrders(text, 
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
		
		
	}
}

