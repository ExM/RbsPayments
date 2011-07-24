using System;
using System.Net;
using NUnit.Framework;
using RbsPayments;
using RbsPayments.Example;
using Configuration;
using RbsPayments.Test;

namespace RbsPayments.SiteTests
{
	public class Common: Env
	{
		[TestFixtureSetUp]
		public void SetUp()
		{
			SanboxSiteConfigure();
		}

		public CookieCollection Login()
		{
			CookieCollection cookies = null;
			SiteConn.Login(Sandbox.Site.User, Sandbox.Site.Pass,
				(result) =>
				{
					cookies = result;
				},
				(ex) => 
				{
					Assert.Fail("unexpected exception: {0}", ex);
				});
			
			return cookies;
		}
	}
}

