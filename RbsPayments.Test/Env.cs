using System;
using NLog;
using NLog.Config;
using NLog.Targets;
using Configuration;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;
using NUnit.Framework;

namespace RbsPayments.Test
{
	public class Env
	{
		private static Logger Log = LogManager.GetCurrentClassLogger();
		
		public static Random _rnd = new Random();
		private static IAppSettings _settings;
		
		public RbsConnectionConfig NoConn;
		public RbsConnectionConfig Sandbox;
		public Secure3DTestConfig Secure3DTest;
		public RbsApi ApiConn;
		public RbsSite SiteConn;

		static Env()
		{
			LoggingConfiguration config = new LoggingConfiguration();
			ConsoleTarget consoleTarget = new ConsoleTarget();
			config.AddTarget("console", consoleTarget);
			consoleTarget.Layout = "${date:format=HH\\:MM\\:ss.fff}, ${logger}, ${level}, Th:${threadid}, ${message}";
			LoggingRule rule1 = new LoggingRule("*", LogLevel.Trace, consoleTarget);
			config.LoggingRules.Add(rule1);
			LogManager.Configuration = config;

			_settings = new FileSettings("RbsPayments.Test.cfg.xml");

			ServicePointManager.ServerCertificateValidationCallback = ValidateServerCertificate;
		}
		
		public void SandboxConfigure()
		{
			Sandbox = _settings.Load<RbsConnectionConfig>(EmptyResult.Throw, "Sandbox");
			SyncConnector conn = new SyncConnector(new Uri(Sandbox.Uri), TimeSpan.FromSeconds(30));
			ApiConn = new RbsApi(conn, Sandbox.Merchant, Sandbox.Refund);
		}
		
		public void BrowserConfigure()
		{
			Secure3DTest = _settings.Load<Secure3DTestConfig>(EmptyResult.Throw);
		}
		
		public void SanboxSiteConfigure()
		{
			Sandbox = _settings.Load<RbsConnectionConfig>(EmptyResult.Throw, "Sandbox");
			SyncConnector conn = new SyncConnector(new Uri(Sandbox.Uri), TimeSpan.FromSeconds(30));
			SiteConn = new RbsSite(conn);
		}

		public void NoConnConfigure()
		{
			NoConn = _settings.Load<RbsConnectionConfig>(EmptyResult.Throw, "NoConn");
			SyncConnector conn = new SyncConnector(new Uri(NoConn.Uri), TimeSpan.FromSeconds(30));
			ApiConn = new RbsApi(conn, NoConn.Merchant, NoConn.Refund);
		}
		
		public static bool ValidateServerCertificate(object sender,
			X509Certificate certificate, X509Chain chain,
			SslPolicyErrors sslPolicyErrors)
		{
			Log.Info("Validate server certificate for uri: {0}", ((HttpWebRequest)sender).RequestUri.ToString());
			Log.Info("Server certificate:\r\n{0}", certificate.ToString(true));
			return true;
		}
		
		public static string CreateOrderNumber()
		{
			//HACK: тут можно сделать проверку на уникальность номера, если изменится поведение сервера
			return _rnd.Next(999999999).ToString();
		}
		
		public RegisterResult Block_3DSec(string orderNumber, decimal amount)
		{
			RegisterResult result = null;

			ApiConn.Block(orderNumber, amount, TestCard.Good3DSec,
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
	}
}

