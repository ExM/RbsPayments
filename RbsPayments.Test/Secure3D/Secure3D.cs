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
using System.Text.RegularExpressions;

namespace RbsPayments.CommandTests
{
	[TestFixture]
	[Category("server required")]
	public class Secure3D: Env
	{
		[TestFixtureSetUp]
		public void SetUp()
		{
			SandboxConfigure();
		}
		
		[Test]
		public void Block_3DSec()
		{
			Block_3DSec(CreateOrderNumber(), 100.12m);
		}
		
		[Test]
		public void Full_3DSec()
		{
			RegisterResult res3ds = Block_3DSec(CreateOrderNumber(), 100.12m);
			
			NameValueCollection postParams1 = new NameValueCollection
			{
				{"TermUrl", "localhost"},
				{"MD", res3ds.MdOrder},
				{"PaReq", res3ds.PaReq}
			};
			
			string page1 = PostRequest(res3ds.AcsUrl, postParams1);
			string acct_id = ExtractAcctId(page1);
			
			NameValueCollection postParams2 = new NameValueCollection
			{
				{"ACCT_ID", acct_id},
				{"GET_PWD", "Получить пароль"}
			};
			
			string page2 = PostRequest(res3ds.AcsUrl, postParams2);
			string password = ExtractPassword(page2);
			
			NameValueCollection postParams3 = new NameValueCollection
			{
				{"ACCT_ID", acct_id},
				{"PWD", password},
				{"SEND", "Äàëåå"}
			};
			
			string page3 = PostRequest(res3ds.AcsUrl, postParams3);
			string paRes = ExtractPaRes(page3);
			
			ApiConn.Bpc3ds(res3ds.MdOrder, paRes,
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
		
		public string PostRequest(string url, NameValueCollection postParams)
		{
			HttpWebRequest webReq = (HttpWebRequest)WebRequest.Create(url);
			webReq.Method = "POST";
			webReq.Timeout = 10000;
			webReq.ContentType = "application/x-www-form-urlencoded";
			webReq.Headers.Add("Content-Encoding", "UTF8");
			
			byte[] postContent = PostParameters.Encode(postParams);
			webReq.ContentLength = postContent.Length;
			webReq.AllowAutoRedirect = false;
			webReq.ServicePoint.Expect100Continue = false;
			
			using(Stream respS = webReq.GetRequestStream())
				respS.Write(postContent, 0, postContent.Length);

			using (HttpWebResponse resp = (HttpWebResponse)webReq.GetResponse())
			using (Stream respS = resp.GetResponseStream())
				return new StreamReader(respS, Encoding.GetEncoding("CP1251")).ReadToEnd();
		}
		
		public string ExtractAcctId(string page)
		{
			//Console.WriteLine("page: \r\n{0}", page);
			Regex r = new Regex("<input type=HIDDEN name=\"ACCT_ID\" value=\"(\\d*.\\d*)\">");
			Match m = r.Match(page);
			Assert.IsTrue(m.Success, "ACCT_ID='N*N.N*N' not found");
			return m.Groups[1].Value;
		}
		
		public string ExtractPassword(string page)
		{
			//Console.WriteLine("page: \r\n{0}", page);
			Regex r = new Regex("\\s*password \"sended\" to phone: (\\w*)\\n");
			Match m = r.Match(page);
			Assert.IsTrue(m.Success, "password not found");
			return m.Groups[1].Value;
		}
		
		public string ExtractPaRes(string page)
		{
			//Console.WriteLine("page: \r\n{0}", page);
			Regex r = new Regex("<input type=\"hidden\" name=\"PaRes\" value=\"(\\S*)\">");
			Match m = r.Match(page);
			Assert.IsTrue(m.Success, "PaRes not found");
			return m.Groups[1].Value;
		}
	}
}

