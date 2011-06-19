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
			(result) =>
			{
				Assert.IsFalse(result.Required3DSecure);
				Assert.AreEqual("98-4822-4978-117-5986-54119-41-2591-42114-19_p1", result.MdOrder);
				Assert.AreEqual(0, result.Code.PrimaryRC);
				Assert.AreEqual(0, result.Code.SecondaryRC);
				Assert.AreEqual(RbsPaymentState.Deposited, result.State);
			},
			(ex) => 
			{
				Assert.Fail("unexpected exception: {0}", ex);
			});
		}
		
		[Test]
		public void Merchant2Rbs_3Dsec()
		{
			string text = "MDORDER=1139797-87-91-68-103-1843-85-111-1106910-521_p1&ACSUrl=https://playground.paymentgate.ru/acs/PAReq&PaReq=eJxVUttuozAQ/RXE+2KbO9HEVVq62j6kTS/7AV4zIqyAUGO6oV+/44T0ItnSnBn7HM8cw9Wxa703NGNz6Ne+CLjvYa8PVdPXa//3y88fuX8l4WVvEMtn1JNBCVscR1Wj11RrvxvrIORC8FQUnHMRCs6LPI4Tnosi8iXsNk/4KmGRkKQQhMAukLiM3qveSlD69fruXiZxGkcxsAVCh+aulDExO/bovICd09CrDqXF0XYLD7BTCvRh6q2ZJZEBuwCYTCv31g4rxoZWzbWhQhUMau6wt7WyGJiJ/Rn0iOatRcuudzdR+QzM3QP2+dbd5KKRdI5NJbfl5t95375vy5pv/+r5obxNHsrHNTB3Airilpc5eZyvuFiFAtgpD6pzD5Q0Os6p8zOCwYlsvpW+poDMMOTVLHNBpQ8EeBwOPfUjac4fMVQ4aolH1Q0tekvHpO+ywD77ufnlnNCWhiuSKMuKNEuTQCRZmKdZnuZhkcZZ5uw5nXFiDU02dP6kJz0HgTketnjPlg9D0beP9B/7Us1q\r\n";
			
			RbsResponse.Merchant2Rbs(text, 
			(result) =>
			{
				Assert.IsTrue(result.Required3DSecure);
				Assert.AreEqual("1139797-87-91-68-103-1843-85-111-1106910-521_p1", result.MdOrder);
				Assert.AreEqual("https://playground.paymentgate.ru/acs/PAReq", result.AcsUrl);
				Assert.AreEqual(399, Convert.FromBase64String(result.PaReq).Length);
			},
			(ex) => 
			{
				Assert.Fail("unexpected exception: {0}", ex);
			});
		}
		
		[Test]
		public void Merchant2Rbs_3Dsec_End_n2p6()
		{
			//HACK: такой варинт ответа не описан в документации
			string text = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\r\n<PSApiResult primaryRC=\"-2\" secondaryRC=\"6\"/>";
			
			RbsResponse.Merchant2Rbs(text, 
			(result) =>
			{
				Assert.IsFalse(result.Required3DSecure);
				Assert.IsNull(result.MdOrder);
				Assert.AreEqual(-2, result.Code.PrimaryRC);
				Assert.AreEqual(6, result.Code.SecondaryRC);
				Assert.AreEqual(RbsPaymentState.Unknown, result.State);
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
			(result) =>
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

