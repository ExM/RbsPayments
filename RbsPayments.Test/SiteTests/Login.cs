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
	public class Login: Common
	{
		[Test]
		public void Valid()
		{
			Site.Login(Cfg.User, Cfg.Password,
				(result) =>
				{
					Assert.AreNotEqual(0, result.Count, "cookies expected");
				},
				(ex) => 
				{
					Assert.Fail("unexpected exception: {0}", ex);
				});
		}
		
		[Test]
		public void WrongPass()
		{
			Site.Login(Cfg.User, "???",
				(result) =>
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

