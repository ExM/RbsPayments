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

namespace RbsPayments
{
	[TestFixture]
	[Category("server required")]
	public class Secure3D_FromBrowser: Env
	{
		[TestFixtureSetUp]
		public void SetUp()
		{
			SandboxConfigure();
		}
		
		[Test]
		[Category("browser required")]
		public void Run()
		{
			RegisterResult result = Block_3DSec(CreateOrderNumber(), 100.12m);
			
			string backUrl = "http://localhost:55000/";
			
			RunBrowser(result.AcsUrl, backUrl, result.MdOrder, result.PaReq);
			
			NameValueCollection postParams = WaitPostRequest(backUrl, TimeSpan.FromSeconds(30));
			
			string paRes = postParams["PaRes"];
			Assert.AreEqual(result.MdOrder, postParams["MD"]);
			
			Conn.Bpc3ds(result.MdOrder, paRes,
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
		
		public NameValueCollection WaitPostRequest(string prefix, TimeSpan to)
		{
			HttpListener listener = new HttpListener();
			listener.Prefixes.Add(prefix);
			
			ManualResetEvent toWait = new ManualResetEvent(false);
			ThreadPool.RegisterWaitForSingleObject(toWait, (state, timedOut) => 
			{
				if(timedOut)
					listener.Stop();
			}, null, to, true);
			
			listener.Start();
			HttpListenerContext context = listener.GetContext();
			
			Assert.AreEqual("post", context.Request.HttpMethod.ToLower());
			NameValueCollection postParams = ReadPostParams(context.Request.InputStream);
			listener.Stop();
			toWait.Set();
			
			return postParams;
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

