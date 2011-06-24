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
		
	}
}

