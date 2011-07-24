using System;
using NUnit.Framework;
using RbsPayments;
using RbsPayments.Example;
using Configuration;
using System.Threading;
using System.Net;
using RbsPayments.Test;

namespace RbsPayments.CommandTests
{
	[TestFixture]
	[Category("server required")]
	public class AsyncExample: Env
	{
		RbsApi _asyncTr;
		
		[TestFixtureSetUp]
		public void SetUp()
		{
			SandboxConfigure();
			AsyncConnector conn = new AsyncConnector(new Uri(Sandbox.Uri), TimeSpan.FromSeconds(10));
			_asyncTr = new RbsApi(conn, Sandbox.Merchant, Sandbox.Refund);
		}

		[Test]
		public void Block_IncorrectFormat()
		{
			ManualResetEvent wait =  new ManualResetEvent(false);
			Exception ex = null;
			
			_asyncTr.Block("ABC", 100m, TestCard.Good,
				(result) =>
				{
					wait.Set();
				},
				(iex) => 
				{
					ex = iex;
					wait.Set();
				});
			
			Assert.IsTrue(wait.WaitOne(20000), "elapsed 20 seconds");
			Assert.IsNotNull(ex, "missed error");
			Assert.IsNotInstanceOf<WebException>(ex);
			Assert.IsInstanceOf<InvalidOperationException>(ex);
			Assert.IsTrue(ex.Message.Contains("ABC"), "not contain `ABC' in `{0}'", ex.Message);
		}
	}
}

