using System;
using NUnit.Framework;
using RbsPayments;
using Configuration;
using System.Net;

namespace RbsPayments.Test
{
	[TestFixture]
	public class NoConnectionTest
	{
		RbsTranslator _tr;
		
		[TestFixtureSetUp]
		public void SetUp()
		{
			RbsConnectionConfig cfg = Env.AppSettings.Load<RbsConnectionConfig>(EmptyResult.Throw, "NoConnection");
			SyncConnector conn = new SyncConnector(new Uri(cfg.Uri), TimeSpan.FromSeconds(10));
			_tr = new RbsTranslator(conn, cfg);
		}
		
		[Test]
		public void Block()
		{
			_tr.Merchant2Rbs("123", "test", 100, "www", false, "4111111111111112", "123", "201110", "Card Holder",
				(result) =>
				{
					Assert.Fail("missed error");
				},
				(ex) => 
				{
					Assert.IsInstanceOf<WebException>(ex, "unexpected exception: {0}", ex);
				});
		}
	}
}

