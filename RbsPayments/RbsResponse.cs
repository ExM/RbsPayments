using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Xml.Linq;
using NLog;

namespace RbsPayments
{
	public class RbsResponse
	{
		private static Logger Log = LogManager.GetCurrentClassLogger();
		
		public static void Merchant2Rbs(string text, Action<string, int, int, RbsPaymentState> completed, Action<Exception> excepted)
		{
			Log.Trace("Merchant2Rbs response:`{0}'", text);
			
			string mdorder = null;
			RbsPaymentState state = RbsPaymentState.Declined;
			int primaryRC = 0;
			int secondaryRC = 0;

			try
			{
				string answer = null;
				string stateText = null;

				foreach (string pair in text.Split('&'))
				{
					int d = pair.IndexOf('=');
					if (d == -1)
					{
						Log.Warn("char `=' missed in pair `{0}'", pair);
						continue;
					}
					else
					{
						string key = HttpUtility.UrlDecode(pair.Substring(0, d));
						string val = HttpUtility.UrlDecode(pair.Substring(d + 1, pair.Length - d - 1));
						if (key == "MDORDER")
							mdorder = val.Trim();
						else if (key == "ANSWER")
							answer = val;
						else if (key == "STATE")
							stateText = val.Trim();
					}
				}

				if (string.IsNullOrEmpty(mdorder))
					throw new FormatException("key `MDORDER' not found");
				if (string.IsNullOrEmpty(answer))
					throw new FormatException("key `ANSWER' not found");
				if (string.IsNullOrEmpty(stateText))
					throw new FormatException("key `STATE' not found");

				ExtractCodeResult(answer, out primaryRC, out secondaryRC);
				state = ParseState(stateText);
				if(state != RbsPaymentState.Approved &&
					state != RbsPaymentState.Declined &&
					state != RbsPaymentState.Deposited)
					throw new FormatException(string.Format("unexpected payment state `{0}'", state));
			}
			catch (Exception err)
			{
				excepted(new FormatException("can not parse response", err));
				return;
			}

			completed(mdorder, primaryRC, secondaryRC, state);
		}

		public static RbsPaymentState ParseState(string stateText)
		{
			switch (stateText)
			{
				case "payment_approved": return RbsPaymentState.Approved;
				case "payment_deposited": return RbsPaymentState.Deposited;
				case "payment_declined": return RbsPaymentState.Declined;
			}
			throw new FormatException(string.Format("unknown payment state `{0}'", stateText));
		}

		private static XNamespace _ns = XNamespace.None;
		private static XName _namePSApiResult = _ns + "PSApiResult";
		private static XName _namePrimaryRC = _ns + "primaryRC";
		private static XName _nameSecondaryRC = _ns + "secondaryRC";

		public static void ExtractCodeResult(string xmlText, out int primaryRC, out int secondaryRC)
		{
			try
			{
				//<?xml version="1.0" encoding="UTF-8">
				//	<PSApiResult primaryRC="0" secondaryRC="0"/>
				XDocument doc = XDocument.Parse(xmlText);
				if(doc.Root.Name != _namePSApiResult)
					throw new FormatException(string.Format("unknown root name `{0}'", doc.Root.Name));

				XAttribute pAt = doc.Root.Attribute(_namePrimaryRC);
				if (pAt == null || string.IsNullOrEmpty(pAt.Value))
					throw new FormatException("primaryRC attribute not found");
				if(!int.TryParse(pAt.Value, out primaryRC))
					throw new FormatException("primaryRC attribute not a number");

				XAttribute sAt = doc.Root.Attribute(_nameSecondaryRC);
				if (sAt == null || string.IsNullOrEmpty(sAt.Value))
					throw new FormatException("secondaryRC attribute not found");
				if (!int.TryParse(sAt.Value, out secondaryRC))
					throw new FormatException("secondaryRC attribute not a number");
			}
			catch(SystemException err)
			{
				throw new FormatException("can not extract code result", err);
			}
		}
	}
}
