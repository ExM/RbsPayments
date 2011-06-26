using System;
using System.Net;
using NUnit.Framework;
using RbsPayments;
using RbsPayments.Example;
using Configuration;
using RbsPayments.Test;

namespace RbsPayments.SiteTests
{
	[Category("RbsSandbox")]
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
			_site.Login(_cfg.User, _cfg.Password,
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

