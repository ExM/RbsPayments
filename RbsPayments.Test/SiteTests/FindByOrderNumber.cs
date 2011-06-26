using System;
using System.Net;
using NUnit.Framework;
using RbsPayments;
using RbsPayments.Example;
using Configuration;
using RbsPayments.Test;

namespace RbsPayments.SiteTests
{
	[TestFixture]
	public class FindByOrderNumber: Common
	{
		[Test]
		public void List()
		{
			Site.FindByOrderNumber(Login(), "123", //FIXME: это заранее известный номер на котором висит 9 платежей
				(result) =>
				{
					Assert.AreEqual(9, result.Count);
				},
				(ex) => 
				{
					Assert.Fail("unexpected exception: {0}", ex);
				});
			
		}

		[Test]
		public void Empty()
		{
			Site.FindByOrderNumber(Login(), "923467625", //FIXME: предполагается, что сервер не знает этого номера
				(result) =>
				{
					Assert.AreEqual(0, result.Count);
				},
				(ex) => 
				{
					Assert.Fail("unexpected exception: {0}", ex);
				});
		}
		
		[Test]
		public void NoLogin()
		{
			Site.FindByOrderNumber(new CookieCollection(), "123",
				(result) =>
				{
					Assert.Fail("missed error");
				},
				(ex) => 
				{
					Assert.IsInstanceOf<InvalidOperationException>(ex);
				});
		}
	}
}

