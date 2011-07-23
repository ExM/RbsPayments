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
			Block_3DSec("5687845", 100.12m);
		}
		
		[Test]
		public void Full_3DSec_FromBrowsers()
		{
			RegisterResult result = Block_3DSec(Env.CreateOrderNumber(), 100.12m);
			
			RunBrowser(result.AcsUrl, "http://localhost:55000/", result.MdOrder, result.PaReq);
			
			HttpListener listener = new HttpListener();
			listener.Prefixes.Add("http://localhost:55000/");
			listener.Start();
			HttpListenerContext context = listener.GetContext();
			
			Assert.AreEqual("post", context.Request.HttpMethod.ToLower());
			NameValueCollection postParams = ReadPostParams(context.Request.InputStream);
			listener.Stop();
			
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
		
		public NameValueCollection ReadPostParams(Stream stream)
		{
			string postData = new StreamReader(stream, Encoding.ASCII).ReadToEnd();
			Console.WriteLine(postData);
			
			NameValueCollection result = new NameValueCollection();
			string[] pairs = postData.Split(new char[] { '&' }, StringSplitOptions.RemoveEmptyEntries);
			foreach (string pair in pairs)
			{
				int d = pair.IndexOf('=');
				if (d == -1)
					result.Add(string.Empty, pair);
				else
					result.Add(
						HttpUtility.UrlDecode(pair.Substring(0, d), Encoding.ASCII),
						HttpUtility.UrlDecode(pair.Substring(d + 1, pair.Length - d - 1), Encoding.ASCII));
			}
			return result;
		}

		public void RunBrowser(string url, string backUrl, string mdOrder, string paReq)
		{
			Assembly asm = Assembly.GetExecutingAssembly();
			string reqPage = new StreamReader(asm.GetManifestResourceStream("RbsPayments.Test.Secure3D.request.html")).ReadToEnd();
			
			reqPage = reqPage.Replace("{Url}", url);
			reqPage = reqPage.Replace("{BackUrl}", backUrl);
			reqPage = reqPage.Replace("{MdOrder}", mdOrder);
			reqPage = reqPage.Replace("{PaReq}", paReq);
			
			File.WriteAllText("req.html", reqPage);
			System.Diagnostics.Process.Start("firefox", "req.html");
		}
	}
}

