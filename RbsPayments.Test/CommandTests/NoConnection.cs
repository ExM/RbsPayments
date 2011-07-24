using System;
using System.Net;
using NUnit.Framework;
using RbsPayments;
using RbsPayments.Example;
using Configuration;
using RbsPayments.Test;

namespace RbsPayments.CommandTests
{
	[TestFixture]
	public class NoConnection: TestCmdTranslator
	{
		[TestFixtureSetUp]
		public void SetUp()
		{
			NoConnConfigure();
		}
		
		[Test]
		public void Block()
		{
			ApiConn.Block("ABC", 100m, TestCard.Good,
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

