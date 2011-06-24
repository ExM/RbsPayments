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
	[Category("required link to playground.paymentgate.ru")]
	public class Common
	{
		RbsConnectionConfig _cfg;
		RbsSite _site;
		
		[TestFixtureSetUp]
		public void SetUp()
		{
			_cfg = Env.AppSettings.Load<RbsConnectionConfig>(EmptyResult.Throw, "RbsSandbox");
			SyncConnector conn = new SyncConnector(new Uri(_cfg.Uri), TimeSpan.FromSeconds(30));
			_site = new RbsSite(conn);
		}
		
		[Test]
		public void Login()
		{
			_site.Login(_cfg.User, _cfg.Password,
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
		public void Login_WrongPass()
		{
			_site.Login(_cfg.User, "???",
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

