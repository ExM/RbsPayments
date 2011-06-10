using System;
using NLog;
using NLog.Config;
using NLog.Targets;
using Configuration;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;

namespace RbsPayments.Test
{
	public static class Env
	{
		private static Logger Log = LogManager.GetCurrentClassLogger();

		public static IAppSettings AppSettings;

		static Env()
		{
			LoggingConfiguration config = new LoggingConfiguration();
			ConsoleTarget consoleTarget = new ConsoleTarget();
			config.AddTarget("console", consoleTarget);
			consoleTarget.Layout = "${date:format=HH\\:MM\\:ss.fff}, ${logger}, ${level}, Th:${threadid}, ${message}";
			LoggingRule rule1 = new LoggingRule("*", LogLevel.Trace, consoleTarget);
			config.LoggingRules.Add(rule1);
			LogManager.Configuration = config;

			AppSettings = new FileSettings("RbsPayments.Test.cfg.xml");

			ServicePointManager.ServerCertificateValidationCallback = ValidateServerCertificate;
		}

		public static bool ValidateServerCertificate(object sender,
			X509Certificate certificate, X509Chain chain,
			SslPolicyErrors sslPolicyErrors)
		{
			Log.Info("Validate server certificate for uri: {0}", ((HttpWebRequest)sender).RequestUri.ToString());
			Log.Info("Server certificate:\r\n{0}", certificate.ToString(true));
			return true;
		}
	}
}

