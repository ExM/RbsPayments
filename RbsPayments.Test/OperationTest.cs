using System;
using NUnit.Framework;
using RbsPayments;
using RbsPayments.Example;
using Configuration;

namespace RbsPayments.Test
{
	[TestFixture]
	[Category("required link to playground.paymentgate.ru")]
	public class OperationTest
	{
		RbsTranslator _tr;
		
		[TestFixtureSetUp]
		public void SetUp()
		{
			RbsConnectionConfig cfg = Env.AppSettings.Load<RbsConnectionConfig>(EmptyResult.Throw, "RbsSandbox");
			SyncConnector conn = new SyncConnector(new Uri(cfg.Uri), TimeSpan.FromSeconds(20));
			_tr = new RbsTranslator(conn, cfg);
		}
		
		[Test]
		public void Block()
		{
			//HACK: на тестовом сервере допускается дублирование платежей
			_tr.Block("5687340", 100m, TestCard.Good,
				(morder, rInfo, state) =>
				{
					Assert.Greater(morder.Length, 10);
					Assert.AreEqual(0, rInfo.PrimaryRC);
					Assert.AreEqual(0, rInfo.SecondaryRC);
					Assert.AreEqual(RbsPaymentState.Deposited, state);
				},
				(morder, acsUrl, paReq) =>
				{
					Assert.Fail("unexpected response");
				},
				(ex) => 
				{
					Assert.Fail("unexpected exception: {0}", ex);
				});
		}
		
		[Test]
		public void Block_3DSec()
		{
			//HACK: на тестовом сервере допускается дублирование платежей
			_tr.Block("5687340", 100m, TestCard.Good3DSec,
				(morder, rInfo, state) =>
				{
					Assert.Fail("unexpected response");
				},
				(morder, acsUrl, paReq) =>
				{
					Assert.Greater(morder.Length, 10);
					Assert.IsNotEmpty(acsUrl);
					Assert.IsNotEmpty(paReq);
					Assert.Greater(Convert.FromBase64String(paReq).Length, 350);
				},
				(ex) => 
				{
					Assert.Fail("unexpected exception: {0}", ex);
				});
		}
		/*
		[Test]
		public void Return_3ds()
		{
			string mdOrder = "109195326-11-4512705498-58-87101-6210973_p1";
			string PaRes = "eJydVWtzqjoU/Ssdz8dOmwRp1Q5yJjxE1FAfoOI3hAgogvIQ5NffqH14O+fDPTcjM8liZ+29spdE+F3to4cTTbMwib"+
				"sN9AwbDzR2Ey+M/W7DMntP7cZvUTCDlFJlRt0ipaJAaJY5Pn0IvW5jn/nPHEQIvqIOhByHWhC2YBOhdpvnGqIwxlOaXSObkL1pI4Z"+
				"9pBNZtmdOAJ9Lxpu6gRPnouC4R0k3xBf+lW/yAvhYCnua6orIQ8RB9jRvPwHcYAF87x8Xl1nGaq1CTyQKLm+PWhPFh2TrI1K7TaKQ"+
				"rgAuEYLn5FT81PEA4RvHvaGWAK64cLjQ4X1SMG7EZEAB3EMCO5aUndpZbCP26msl0OqQxJRFMJVfcwF8V3dwYhHeDcQG42aoYC5FI"+
				"Q/3f6zqigtZ7uRFJhoC+JgJrnM6iRhjSZIX2mCCJaJPkl1iR/g2mNpryDUQR36Shnmwv1T3b0AAl+zg2jtRmIV+zPhT+sC8EmfdRp"+
				"DnhzcAyrJ8LpvPSeoD1g8IYAewAC8L/V+N2y7q6fEm+attshMnceg6UVg7OXMFoXmQeA9ftf2JxpxemBCYqvITo3pyER8/XRDmwxf"+
				"GCf5Meqfsv2T5WWyaOU9Z4FwMDX4QicKUbujFBPTBmurdxq9v8yuhT7P8/yT8THbP8Mk3d6KCinYwXw/jxwl3Npe7jNub9VaPHm2y"+
				"Vvzu575bpAC+Kvwo/9aruzO5BZJ6n7UfkUvbuxPfSpyFtBrbxdo2CIr6+36AVi0zL6CRa1LszMFWBzTY8EGyXOR+4lXm8nVl7xJ+mrxHlR1n7Tau3kkPq8OlfVDMSO21dW7UU0fVLt3CunY9G3Y4MEc44EcjW7ZpyPWU4rhECGeRO3ZqeTvql7MXflwFWtazZuPdPKWaLGt2964PHyqH9HxTtXyBHcXJndtMpmkebpgh2J+b6LpUKLKMZ6osqWe48kipTOzBMFnpwck18ETtSRNc6rU6IninYWSpUkBkK7KqnoJnkm/MJUyIzPW2LurU9uJlZ+87PJH4pWKqJTFxRbZqbSg6JCi5YPUVM7+wv+fWtQ3BUJNnR22mr5vKRJXwxMKY1wysyFI4GUr+RKE8GYZHrcmF1nhVowk5q27/cWDmh53pHd3TWLc3+sTXX2UV7oNs+thMZ04i7XpDNTY4NTvzvXCZhIZkWcCcLAaRXyy0VVHaVbkyODcjHphHG9CbmiXO+3rfqOdHuQwGSb/QO46/3FfJ0FThYVdP5+GSWDq/hqYzWy3qIN3JCtWc3NLK0qidUlfw5XP1Q5N00yTh4TEd2oVn8eb5zLph2xYneWW2tgb2u5KkdbQeu9r0XTU9roebI7fJtbyVetDofCaVyPdYa/HYKGNpVC1Kbz9cDZuWInVAWhv9VmplYbbJDEvW5+A1bp6s94hTe2W8Ll45H1V16zHXSDY4WoeTPjuORy/HCGX9GBf9dD+FnW3hN1exh+3JKusoAvjpsBtycx/4cuS3V6831/VGvXx472/afwDZvHxH";
			
			_tr.Bpc3ds(mdOrder, PaRes,
				(morder, rInfo, state) =>
				{
					Assert.Greater(morder.Length, 10);
					Assert.AreEqual(0, rInfo.PrimaryRC);
					Assert.AreEqual(0, rInfo.SecondaryRC);
					Assert.AreEqual(RbsPaymentState.Deposited, state);
				},
				(ex) => 
				{
					Assert.Fail("unexpected exception: {0}", ex);
				});
		}
		*/
		[Test]
		public void QueryOrders_IncorrectMdOrder()
		{
			_tr.QueryOrders("123",
				(rInfo, pInfo, state) =>
				{
					Assert.Fail("missed error");
				},
				(ex) => 
				{
					Assert.IsInstanceOf<InvalidOperationException>(ex, "current exception: {0}", ex);
					Assert.AreEqual(ExpectedMessage.IncorectMdOrder, ex.Message);
				});
		}
		
		[Test]
		public void QueryOrders()
		{
			_tr.QueryOrders("98-4822-4978-117-5986-54119-41-2591-42114-19_p1",
				(rInfo, pInfo, state) =>
				{
					//TODO: создать новый платеж перед этой проверкой
				},
				(ex) => 
				{
					Assert.Fail("unexpected exception: {0}", ex);
				});
		}
		
		[Test]
		public void Block_IncorrectFormat()
		{
			_tr.Block("ABC", 100m, TestCard.Good,
				(morder, rInfo, state) =>
				{
					Assert.Fail("missed error");
				},
				(morder, acsUrl, paReq) =>
				{
					Assert.Fail("unexpected response");
				},
				(ex) => 
				{
					Assert.IsInstanceOf<InvalidOperationException>(ex);
					Assert.IsTrue(ex.Message.Contains("ABC"), "not contain `ABC' in `{0}'", ex.Message);
				});
		}
	}
}

