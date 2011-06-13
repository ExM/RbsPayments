using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Xml.Linq;
using NLog;

namespace RbsPayments
{
	public static class RbsResponse
	{
		private static Logger Log = LogManager.GetCurrentClassLogger();
		
		public static void Merchant2Rbs(string text, Action<string, ResultInfo, RbsPaymentState> completed, Action<Exception> excepted)
		{
			Log.Trace("Merchant2Rbs response:`{0}'", text);
			
			string mdorder = null;
			RbsPaymentState state = RbsPaymentState.Declined;
			
			string answer = null;
			string stateText = null;
			
			try
			{
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
			}
			catch (Exception err)
			{
				excepted(new InvalidOperationException(text, err));
				return;
			}
			
			ResultInfo rInfo;
			
			try
			{
				XDocument doc = XDocument.Parse(answer);
				rInfo = ExtractResultInfo(doc.Root);
				state = ParseState(stateText);
				if(state != RbsPaymentState.Approved &&
					state != RbsPaymentState.Declined &&
					state != RbsPaymentState.Deposited)
					throw new InvalidOperationException(string.Format("unexpected payment state `{0}'", state));
			}
			catch (Exception err)
			{
				excepted(new FormatException("can not parse response", err));
				return;
			}

			completed(mdorder, rInfo, state);
		}
		
		public static void QueryOrders(string text, Action<ResultInfo, PaymentInfo, RbsPaymentState> completed, Action<Exception> excepted)
		{
			Log.Trace("QueryOrders response:`{0}'", text);
			ResultInfo rInfo = new ResultInfo();
			PaymentInfo pInfo = null;
			RbsPaymentState state = RbsPaymentState.Unknown;
			
			try
			{
				XDocument doc = XDocument.Parse(text);
				rInfo = ExtractResultInfo(doc.Root);
				if(rInfo.Success)
				{
					XElement psOrderEl = doc.Root.Element(_namePSOrder);
					if(psOrderEl == null)
						throw new FormatException("element `PSOrder' nof found");
				
					pInfo = ExtractPaymentInfo(psOrderEl, out state);
				}
				else if(!rInfo.MdOrderNotFound)
				{
					Log.Warn("unexpected code result {0}", rInfo);
					throw new FormatException(string.Format("unexpected code result {0}", rInfo));
				}
			}
			catch(SystemException err)
			{
				excepted(new FormatException("can not parse response", err));
				return;
			}
			
			completed(rInfo, pInfo, state);
		}
		
		private static ResultInfo ExtractResultInfo(XElement el)
		{
			try
			{
				//<?xml version="1.0" encoding="UTF-8">
				//	<PSApiResult primaryRC="0" secondaryRC="0"/>
				if(el.Name != _namePSApiResult)
					throw new FormatException(string.Format("unknown element name `{0}'", el.Name));
	
				XAttribute pAt = el.Attribute(_namePrimaryRC);
				if (pAt == null || string.IsNullOrEmpty(pAt.Value))
					throw new FormatException("primaryRC attribute not found");
				int pRC;
				if(!int.TryParse(pAt.Value, out pRC))
					throw new FormatException("primaryRC attribute not a number");

				XAttribute sAt = el.Attribute(_nameSecondaryRC);
				if (sAt == null || string.IsNullOrEmpty(sAt.Value))
					throw new FormatException("secondaryRC attribute not found");
				int sRC;
				if (!int.TryParse(sAt.Value, out sRC))
					throw new FormatException("secondaryRC attribute not a number");
				
				return new ResultInfo{PrimaryRC = pRC, SecondaryRC = sRC};
			}
			catch(SystemException err)
			{
				throw new FormatException("can not extract code result", err);
			}
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
		private static XName _namePSOrder = _ns + "PSOrder";
		private static XName _nameAmount = _ns + "amount";
		private static XName _nameMerchantNumber = _ns + "merchantNumber";
		private static XName _nameOrderNumber = _ns + "orderNumber";
		
		public static PaymentInfo ExtractPaymentInfo(XElement el, out RbsPaymentState state)
		{
			try
			{
				//<PSOrder amount="123456789" currency="810" merchantNumber="123456789" orderNumber="123456789" state="order_ordered">
				//  <PaymentCollection>
				//    <PSPayment approvalCode="207433" approveAmount="123456789" authCode="0"
				//      authTime="Thu Mar 14 12:10:24 GMT+03:00 2002" capCode="0"
				//      depositAmount="123456789" paymentNumber="123456789" 
				//      paymentType="BPC" payment_state="payment_deposited" 
				//      pan="412345..1234"/>
				//  </PaymentCollection>
				//</PSOrder>
				PaymentInfo pInfo = new PaymentInfo();
				
				pInfo.Amount = el.GetIntAttribute(_nameAmount);
				pInfo.MerchantNumber = el.GetStringAttribute(_nameMerchantNumber);
				pInfo.OrderNumber = el.GetStringAttribute(_nameOrderNumber);
				
				state = RbsPaymentState.Declined;
				return pInfo;
			}
			catch(SystemException err)
			{
				throw new FormatException("can not extract payment info", err);
			}
		}
					
		public static int GetIntAttribute(this XElement el, XName xn)
		{
			XAttribute at = el.Attribute(xn);
			if(at == null)
				throw new FormatException(
					string.Format("attribute `{0}' in element `{1}' not found", xn, el.Name));
			try
			{
				return int.Parse(at.Value);
			}
			catch(SystemException err)
			{
				throw new FormatException(
					string.Format("attribute `{0}' not contain number", xn), err);
			}
		}
		
		public static string GetStringAttribute(this XElement el, XName xn)
		{
			XAttribute at = el.Attribute(xn);
			if(at == null)
				throw new FormatException(
					string.Format("attribute `{0}' in element `{1}' not found", xn, el.Name));
			if(string.IsNullOrEmpty(at.Value))
				throw new FormatException(
					string.Format("attribute `{0}' not contain text", xn));
			
			return at.Value;
		}
	}
}
