using System;
using NUnit.Framework;
using RbsPayments;
using RbsPayments.Test;
using System.IO;

namespace RbsPayments
{
	[TestFixture]
	public class ParseSitePageTests
	{
		[Test]
		public void Login_ErrorAuth()
		{
			string text = File.ReadAllText("SiteResponse/ErrorAuth.html");
			
			RbsSiteResponse.Login(text, 
			() =>
			{
				Assert.Fail("missed error");
			},
			(ex) => 
			{
				Assert.IsInstanceOf<InvalidOperationException>(ex);
				Assert.AreEqual(ExpectedMessage.ErrorAuth, ex.Message);
			});
		}
		
		[Test]
		public void ParsePaymentList()
		{
			string text = File.ReadAllText("SiteResponse/PaymentList.html");
			
			RbsSiteResponse.PaymentList(text, 
			(list) =>
			{
				Assert.AreEqual(9, list.Count);
				Assert.AreEqual("-90-48-77-104-100-101-11049592717-109-91-121-23-84_p1", list[0]);
				Assert.AreEqual("-788972-6102111-12385-601498-89-73-3520-51_p1", list[8]);
			},
			(ex) => 
			{
				Assert.Fail("unexpected exception: {0}", ex);
			});
		}
		
		[Test]
		public void ParsePaymentListEmpty()
		{
			string text = File.ReadAllText("SiteResponse/PaymentList_Empty.html");
			
			RbsSiteResponse.PaymentList(text, 
			(list) =>
			{
				Assert.AreEqual(0, list.Count);
			},
			(ex) => 
			{
				Assert.Fail("unexpected exception: {0}", ex);
			});
		}
		
	}
}

