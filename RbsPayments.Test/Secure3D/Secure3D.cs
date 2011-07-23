using System;
using NUnit.Framework;
using RbsPayments;
using RbsPayments.Example;
using Configuration;
using RbsPayments.Test;
using System.Collections.Specialized;
using System.Resources;
using System.Reflection;
using System.IO;
using System.Threading;
using System.Net;
using System.Text;
using System.Web;

namespace RbsPayments.CommandTests
{
	[TestFixture]
	public class Secure3D
	{
		RbsTranslator _tr;
		
		[TestFixtureSetUp]
		public void SetUp()
		{
			RbsConnectionConfig cfg = Env.Sandbox;
			SyncConnector conn = new SyncConnector(new Uri(cfg.Uri), TimeSpan.FromSeconds(30));
			_tr = new RbsTranslator(conn, cfg.Merchant, cfg.Refund);
		}
		
		public RegisterResult Block_3DSec(string orderNumber, decimal amount)
		{
			RegisterResult result = null;

			_tr.Block(orderNumber, amount, TestCard.Good3DSec,
				(res) =>
				{
					result = res;
				},
				(ex) => 
				{
					Assert.Fail("unexpected exception: {0}", ex);
				});
			
			if(!result.Required3DSecure)
				Assert.Ignore("test card not required 3d secure");
			Assert.Greater(result.MdOrder.Length, 10);
			Assert.IsNotEmpty(result.AcsUrl);
			Assert.IsNotEmpty(result.PaReq);
			Assert.Greater(Convert.FromBase64String(result.PaReq).Length, 350);
			
			return result;
		}
		
		[Test]
		public void Block_3DSec()
		{
			Block_3DSec(Env.CreateOrderNumber(), 100.12m);
		}
		
		[Test]
		public void Full_3DSec_FromBrowser()
		{
			RegisterResult result = Block_3DSec(Env.CreateOrderNumber(), 100.12m);
			
			string backUrl = "http://localhost:55000/";
			
			Secure3DHelper.RunBrowser(result.AcsUrl, backUrl, result.MdOrder, result.PaReq);
			NameValueCollection postParams = Secure3DHelper.WaitPostRequest(backUrl, TimeSpan.FromSeconds(30));
			
			string paRes = postParams["PaRes"];
			Assert.AreEqual(result.MdOrder, postParams["MD"]);
			
			_tr.Bpc3ds(result.MdOrder, paRes,
				(result2) =>
				{
					Assert.Greater(result2.MdOrder.Length, 10);
					Assert.AreEqual(0, result2.Code.PrimaryRC);
					Assert.AreEqual(0, result2.Code.SecondaryRC);
					Assert.AreEqual(RbsPaymentState.Deposited, result2.State);
				},
				(ex) => 
				{
					Assert.Fail("unexpected exception: {0}", ex);
				});
		}
		
		[Test]
		public void Full_3DSec()
		{
			RegisterResult res3ds = Block_3DSec(Env.CreateOrderNumber(), 100.12m);
			
			NameValueCollection postParams = new NameValueCollection
			{
				{"TermUrl", "localhost"},
				{"MD", res3ds.MdOrder},
				{"PaReq", res3ds.PaReq}
			};
			
			HttpWebRequest webReq = (HttpWebRequest)WebRequest.Create(res3ds.AcsUrl);
			webReq.Method = "POST";
			webReq.Timeout = 10000;
			webReq.ContentType = "application/x-www-form-urlencoded";
			webReq.Headers.Add("Content-Encoding", "UTF8");
			
			byte[] postContent = PostParameters.Encode(postParams);
			webReq.ContentLength = postContent.Length;
			webReq.AllowAutoRedirect = false;
			webReq.ServicePoint.Expect100Continue = false;
			
			string respText;

			using(Stream respS = webReq.GetRequestStream())
				respS.Write(postContent, 0, postContent.Length);

			using (HttpWebResponse resp = (HttpWebResponse)webReq.GetResponse())
			using (Stream respS = resp.GetResponseStream())
				respText = new StreamReader(respS).ReadToEnd();
			
			Console.WriteLine("Response: \r\n{0}", respText);
		}
	}
}

