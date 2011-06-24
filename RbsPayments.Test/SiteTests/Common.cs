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
		RbsSite _site;
		
		[TestFixtureSetUp]
		public void SetUp()
		{
			RbsConnectionConfig cfg = Env.AppSettings.Load<RbsConnectionConfig>(EmptyResult.Throw, "RbsSandbox");
			SyncConnector conn = new SyncConnector(new Uri(cfg.Uri), TimeSpan.FromSeconds(30));
			_site = new RbsSite(conn, cfg.User, cfg.Password);
		}
		
		[Test]
		public void LogIn()
		{
			_site.LogIn(
				(result) =>
				{
					Assert.AreNotEqual(0, result.Count, "cookies expected");
				},
				(ex) => 
				{
					Assert.Fail("unexpected exception: {0}", ex);
				});
		}
	}
}

