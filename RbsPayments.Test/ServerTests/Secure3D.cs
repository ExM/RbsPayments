using System;
using NUnit.Framework;
using RbsPayments;
using RbsPayments.Example;
using Configuration;
using RbsPayments.Test;

namespace RbsPayments.ServerTests
{
	[TestFixture]
	[Category("required link to playground.paymentgate.ru")]
	public class Secure3D
	{
		RbsTranslator _tr;
		
		[TestFixtureSetUp]
		public void SetUp()
		{
			RbsConnectionConfig cfg = Env.AppSettings.Load<RbsConnectionConfig>(EmptyResult.Throw, "RbsSandbox");
			SyncConnector conn = new SyncConnector(new Uri(cfg.Uri), TimeSpan.FromSeconds(30));
			_tr = new RbsTranslator(conn, cfg);
		}
		
		[Test]
		public void Block_3DSec()
		{
			//HACK: на тестовом сервере допускается дублирование платежей
			_tr.Block("5687845", 100.12m, TestCard.Good3DSec,
				(result) =>
				{
					Assert.IsTrue(result.Required3DSecure);
					Assert.Greater(result.MdOrder.Length, 10);
					Assert.IsNotEmpty(result.AcsUrl);
					Assert.IsNotEmpty(result.PaReq);
					Assert.Greater(Convert.FromBase64String(result.PaReq).Length, 350);
				},
				(ex) => 
				{
					Assert.Fail("unexpected exception: {0}", ex);
				});
		}
		
		[Test]
		public void Return_3ds()
		{
			string mdOrder = "109195326-11-4512705498-58-87101-6210973_p1";
			//string PaRes = "++"; //HACK: не читаемый PaRes, но ответ сервера "-2 	6 	Эмитент отклонил  VbV/SecureCode аутентификацию."
			
			string PaRes = "eJydVWtzqjoU/Ssdz8dOmwRp1Q5yJjxE1FAfoOI3hAgogvIQ5NffqH14O+fDPTcjM8liZ+29spdE+F3to4cTTbMwib"+
				"sN9AwbDzR2Ey+M/W7DMntP7cZvUTCDlFJlRt0ipaJAaJY5Pn0IvW5jn/nPHEQIvqIOhByHWhC2YBOhdpvnGqIwxlOaXSObkL1pI4Z"+
				"9pBNZtmdOAJ9Lxpu6gRPnouC4R0k3xBf+lW/yAvhYCnua6orIQ8RB9jRvPwHcYAF87x8Xl1nGaq1CTyQKLm+PWhPFh2TrI1K7TaKQ"+
				"rgAuEYLn5FT81PEA4RvHvaGWAK64cLjQ4X1SMG7EZEAB3EMCO5aUndpZbCP26msl0OqQxJRFMJVfcwF8V3dwYhHeDcQG42aoYC5FI"+
				"Q/3f6zqigtZ7uRFJhoC+JgJrnM6iRhjSZIX2mCCJaJPkl1iR/g2mNpryDUQR36Shnmwv1T3b0AAl+zg2jtRmIV+zPhT+sC8EmfdRp"+
				"DnhzcAyrJ8LpvPSeoD1g8IYAewAC8L/V+N2y7q6fEm+attshMnceg6UVg7OXMFoXmQeA9ftf2JxpxemBCYqvITo3pyER8/XRDmwxf"+
				"GCf5Meqfsv2T5WWyaOU9Z4FwMDX4QicKUbujFBPTBmurdxq9v8yuhT7P8/yT8THbP8Mk3d6KCinYwXw/jxwl3Npe7jNub9VaPHm2y"+
				"Vvzu575bpAC+Kvwo/9aruzO5BZJ6n7UfkUvbuxPfSpyFtBrbxdo2CIr6+36AVi0zL6CRa1LszMFWBzTY8EGyXOR+4lXm8nVl7xJ+mrxHlR1n7Tau3kkPq8OlfVDMSO21dW7UU0fVLt3CunY9G3Y4MEc44EcjW7ZpyPWU4rhECGeRO3ZqeTvql7MXflwFWtazZuPdPKWaLGt2964PHyqH9HxTtXyBHcXJndtMpmkebpgh2J+b6LpUKLKMZ6osqWe48kipTOzBMFnpwck18ETtSRNc6rU6IninYWSpUkBkK7KqnoJnkm/MJUyIzPW2LurU9uJlZ+87PJH4pWKqJTFxRbZqbSg6JCi5YPUVM7+wv+fWtQ3BUJNnR22mr5vKRJXwxMKY1wysyFI4GUr+RKE8GYZHrcmF1nhVowk5q27/cWDmh53pHd3TWLc3+sTXX2UV7oNs+thMZ04i7XpDNTY4NTvzvXCZhIZkWcCcLAaRXyy0VVHaVbkyODcjHphHG9CbmiXO+3rfqOdHuQwGSb/QO46/3FfJ0FThYVdP5+GSWDq/hqYzWy3qIN3JCtWc3NLK0qidUlfw5XP1Q5N00yTh4TEd2oVn8eb5zLph2xYneWW2tgb2u5KkdbQeu9r0XTU9roebI7fJtbyVetDofCaVyPdYa/HYKGNpVC1Kbz9cDZuWInVAWhv9VmplYbbJDEvW5+A1bp6s94hTe2W8Ll45H1V16zHXSDY4WoeTPjuORy/HCGX9GBf9dD+FnW3hN1exh+3JKusoAvjpsBtycx/4cuS3V6831/VGvXx472/afwDZvHxH";
			
			PaRes = PaRes.Replace("+", "-").Replace("/", "_");
			
			_tr.Bpc3ds(mdOrder, PaRes,
				(result) =>
				{
					Assert.Greater(result.MdOrder.Length, 10);
					Assert.AreEqual(0, result.Code.PrimaryRC);
					Assert.AreEqual(0, result.Code.SecondaryRC);
					Assert.AreEqual(RbsPaymentState.Deposited, result.State);
				},
				(ex) => 
				{
					Assert.Fail("unexpected exception: {0}", ex);
				});
		}
	}
}

