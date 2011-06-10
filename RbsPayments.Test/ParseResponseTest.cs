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
			(morder, f, s, state) =>
			{
				Assert.AreEqual("98-4822-4978-117-5986-54119-41-2591-42114-19_p1", morder);
				Assert.AreEqual(0, f);
				Assert.AreEqual(0, s);
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
			(morder, f, s, state) =>
			{
				Assert.Fail("missed error");
			},
			(ex) => 
			{
				Assert.IsInstanceOf<InvalidOperationException>(ex);
				Assert.AreEqual(text, ex.Message);
			});
		}
		
		
	}
}

