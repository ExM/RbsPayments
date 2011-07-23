using System;
using System.Net;
using NUnit.Framework;
using RbsPayments;
using RbsPayments.Example;
using Configuration;
using RbsPayments.Test;

namespace RbsPayments.SiteTests
{
	public class Common
	{
		RbsConnectionConfig _cfg;
		RbsSite _site;
		
		[TestFixtureSetUp]
		public void SetUp()
		{
			_cfg = Env.Sandbox;
			SyncConnector conn = new SyncConnector(new Uri(_cfg.Uri), TimeSpan.FromSeconds(30));
			_site = new RbsSite(conn);
		}
		
		public RbsSite Site
		{
			get{
				return _site;
			}
		}
		
		public RbsConnectionConfig Cfg
		{
			get{
				return _cfg;
			}
		}
		
		public CookieCollection Login()
		{
			CookieCollection cookies = null;
			_site.Login(_cfg.Site.User, _cfg.Site.Pass,
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

