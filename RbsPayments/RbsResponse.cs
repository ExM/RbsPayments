using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Xml.Linq;
using NLog;
using System.Globalization;

namespace RbsPayments
{
	public static class RbsResponse
	{
		private static Logger Log = LogManager.GetCurrentClassLogger();
		
		public static void Merchant2Rbs(string text, Action<RegisterResult> completed, Action<Exception> excepted)
		{
			Log.Trace("Merchant2Rbs response:`{0}'", text);
			
			string mdorder = null;
			string answer = null;
			string stateText = null;
			string acsUrl = null;
			string paReq = null;
			
			try
			{
				foreach (string pair in text.Split('&'))
				{
					int d = pair.IndexOf('=');
					if (d == -1)
						continue;

					string key = HttpUtility.UrlDecode(pair.Substring(0, d));
					string encVal = pair.Substring(d + 1, pair.Length - d - 1);
					string val = HttpUtility.UrlDecode(encVal);
					if (key == "MDORDER")
						mdorder = val.Trim();
					else if (key == "ANSWER")
						answer = val;
					else if (key == "STATE")
						stateText = val.Trim();
					else if (key == "ACSUrl")
						acsUrl = val.Trim();
					else if (key == "PaReq")
						paReq = encVal;
				}
			}
			catch (Exception err)
			{
				Log.Warn("unexpected keys in `{0}'", text);
				excepted(new InvalidOperationException("can not parse response", err));
				return;
			}
			
			if (!string.IsNullOrEmpty(answer) &&
				!string.IsNullOrEmpty(stateText) &&
				!string.IsNullOrEmpty(mdorder))
				No3DSequre(mdorder, answer, stateText, completed, excepted);
			else if(!string.IsNullOrEmpty(acsUrl) &&
				!string.IsNullOrEmpty(paReq) &&
				!string.IsNullOrEmpty(mdorder))
				completed(new RegisterResult(mdorder, acsUrl, paReq));
			else
				CheckResultCode(text, completed, excepted);
		}
		
		private static void CheckResultCode(string text, 
			Action<RegisterResult> completed, Action<Exception> excepted)
		{
			ResultCode rCode;
			
			try
			{
				XDocument doc = XDocument.Parse(text);
				rCode = ExtractResultInfo(doc.Root);
			}
			catch (Exception)
			{
				excepted(new InvalidOperationException(text));
				return;
			}
			
			if(rCode.Success)
				excepted(new InvalidOperationException("unexpected success result code"));
			else
				completed(new RegisterResult(rCode));
		}

		private static void No3DSequre(string mdorder, string answer, string stateText,
			Action<RegisterResult> completed, Action<Exception> excepted)
		{
			ResultCode rInfo;
			RbsPaymentState state;
			
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
			
			completed(new RegisterResult(mdorder, rInfo, state));
		}
		
		public static void QueryOrders(string text, Action<ResultCode, PaymentInfo, RbsPaymentState> completed, Action<Exception> excepted)
		{
			Log.Trace("QueryOrders response:`{0}'", text);
			ResultCode rInfo = new ResultCode();
			PaymentInfo pInfo = null;
			RbsPaymentState state = RbsPaymentState.Unknown;
			
			text = FixUnclosedTag(text);
			
			try
			{
				XDocument doc = XDocument.Parse(text);
				
				if(doc.Root.Name == XNamespace.None + "error")
					throw new InvalidOperationException(doc.Root.Value);
				
				rInfo = ExtractResultInfo(doc.Root);
				if(rInfo.Success)
				{
					XElement psOrderEl = doc.Root.GetElement("PSOrder");
					pInfo = ExtractPaymentInfo(psOrderEl, out state);
				}
				else if(!rInfo.MdOrderNotFound)
				{
					Log.Warn("unexpected code result {0}", rInfo);
					throw new FormatException(string.Format("unexpected code result {0}", rInfo));
				}
			}
			catch(InvalidOperationException err)
			{
				excepted(err);
				return;
			}
			catch(SystemException err)
			{
				excepted(new FormatException("can not parse response", err));
				return;
			}
			
			completed(rInfo, pInfo, state);
		}

		public static void DepositPayment(string text, Action<ResultCode> completed, Action<Exception> excepted)
		{
			Log.Trace("DepositPayment response:`{0}'", text);
			ResultCode rInfo;
			
			text = FixUnclosedTag(text);
			
			try
			{
				XDocument doc = XDocument.Parse(text);
				
				if(doc.Root.Name == XNamespace.None + "error")
					throw new InvalidOperationException(doc.Root.Value);
				
				rInfo = ExtractResultInfo(doc.Root);
			}
			catch(InvalidOperationException err)
			{
				excepted(err);
				return;
			}
			catch(SystemException err)
			{
				//HACK: в документации не отражено что возвращает сервер в случае ошибки
				excepted(new FormatException("can not parse response", err));
				return;
			}
			
			completed(rInfo);
		}
		
		public static void Refund(string text, Action<ResultCode> completed, Action<Exception> excepted)
		{
			Log.Trace("Refund response:`{0}'", text);
			ResultCode rInfo;
			
			text = FixUnclosedTag(text);
			
			try
			{
				XDocument doc = XDocument.Parse(text);
				
				if(doc.Root.Name == XNamespace.None + "error")
					throw new InvalidOperationException(doc.Root.Value);
				
				rInfo = ExtractResultInfo(doc.Root);
			}
			catch(InvalidOperationException err)
			{
				excepted(err);
				return;
			}
			catch(SystemException err)
			{
				//HACK: в документации не отражено что возвращает сервер в случае ошибки
				excepted(new FormatException("can not parse response", err));
				return;
			}
			
			completed(rInfo);
		}
		
		public static void DepositReversal(string text, Action<ResultCode> completed, Action<Exception> excepted)
		{
			Log.Trace("DepositReversal response:`{0}'", text);
			ResultCode rInfo;
			
			text = FixUnclosedTag(text);
			XDocument doc;
			try
			{
				doc = XDocument.Parse(text);
			}
			catch(SystemException err)
			{
				excepted(new InvalidOperationException(text, err));
				return;
			}
			
			try
			{
				rInfo = ExtractResultInfo(doc.Root);
			}
			catch(SystemException err)
			{
				excepted(new FormatException("can not parse response", err));
				return;
			}
			
			completed(rInfo);
		}
		
		private static string FixUnclosedTag(string text)
		{
			//HACK: в тексте ошибки может быть включен не закрытый тег <p>
			return text.Replace("<p>", "\n");
		}
		
		private static ResultCode ExtractResultInfo(XElement el)
		{
			try
			{
				//<?xml version="1.0" encoding="UTF-8">
				//	<PSApiResult primaryRC="0" secondaryRC="0"/>
				if(el.Name != XNamespace.None + "PSApiResult")
					throw new FormatException(string.Format("unknown element name `{0}'", el.Name));
	
				int pRC = el.GetIntAttribute("primaryRC");
				int sRC = el.GetIntAttribute("secondaryRC");
				
				return new ResultCode{PrimaryRC = pRC, SecondaryRC = sRC};
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
				case "payment_void": return RbsPaymentState.Void;
				case "payment_pending": return RbsPaymentState.Pending;
				case "refunded": return RbsPaymentState.Refunded;
				case "refund failed": return RbsPaymentState.RefundFailed;
			}
			throw new FormatException(string.Format("unknown payment state `{0}'", stateText));
		}
		
		public static PaymentInfo ExtractPaymentInfo(XElement el, out RbsPaymentState state)
		{
			try
			{
				//HACK: это пример ответа из документации
				//<PSOrder amount="123456789" currency="810" merchantNumber="123456789"
				//    orderNumber="123456789" state="order_ordered">
				//  <PaymentCollection>
				//    <PSPayment approvalCode="207433" approveAmount="123456789" authCode="0"
				//      authTime="Thu Mar 14 12:10:24 GMT+03:00 2002" capCode="0"
				//      depositAmount="123456789" paymentNumber="123456789" 
				//      paymentType="BPC" payment_state="payment_deposited" 
				//      pan="412345..1234"/>
				//  </PaymentCollection>
				//</PSOrder>
				
				//HACK: это пример ответа тестового сервера
				//<PSOrder PAN="411111**1112" amount="1000" currency="810" expiry="201110" 
				//    merchantNumber="118600604" orderNumber="5687340" state="order_ordered">
				//  <PaymentCollection>
				//    <PSPayment approvalCode="123456" approveAmount="1000" authCode="0" 
				//      authTime="Fri Jun 10 17:16:17 MSD 2011" depositAmount="0" paymentNumber="1" 
				//      paymentType="BPC" payment_state="payment_approved"/>
				//  </PaymentCollection>
				//</PSOrder>
				
				PaymentInfo pInfo = new PaymentInfo();
				
				pInfo.Amount = el.GetIntAttribute("amount");
				pInfo.MerchantNumber = el.GetStringAttribute("merchantNumber");
				pInfo.OrderNumber = el.GetStringAttribute("orderNumber");
				
				XElement psPayEl = el
					.GetElement("PaymentCollection")
					.GetElement("PSPayment");
				
				pInfo.ApprovalCode = psPayEl.GetStringAttribute("approvalCode");
				pInfo.ApproveAmount = psPayEl.GetIntAttribute("approveAmount");
				pInfo.AuthCode = psPayEl.GetIntAttribute("authCode");
				pInfo.AuthTime = ParseTime(psPayEl.GetStringAttribute("authTime"));
				pInfo.DepositAmount = psPayEl.GetIntAttribute("depositAmount");
				pInfo.Pan = el.GetStringAttribute("PAN");

				state = ParseState(psPayEl.GetStringAttribute("payment_state"));
				return pInfo;
			}
			catch(SystemException err)
			{
				throw new FormatException("can not extract payment info", err);
			}
		}
		
		public static DateTime ParseTime(string text)
		{
			//Fri Jun 10 17:16:17 MSD 2011
			return DateTime.ParseExact(text, "ddd' 'MMM' 'dd' 'HH':'mm':'ss' MSD 'yyyy", CultureInfo.InvariantCulture);
		}
		
		public static XElement GetElement(this XElement el, string name)
		{
			XElement result = el.Element(XNamespace.None + name);
			if(result == null)
				throw new FormatException(
					string.Format("element `{0}' not contain element `{1}' ", el.Name, name));
			
			return result;
		}
		
		public static int GetIntAttribute(this XElement el, string name)
		{
			XAttribute at = el.Attribute(XNamespace.None + name);
			if(at == null)
				throw new FormatException(
					string.Format("element `{0}' not contain attribute `{1}'", el.Name, name));
			try
			{
				return int.Parse(at.Value);
			}
			catch(SystemException err)
			{
				throw new FormatException(
					string.Format("attribute `{0}' not contain number", name), err);
			}
		}
		
		public static string GetStringAttribute(this XElement el, string name)
		{
			XAttribute at = el.Attribute(XNamespace.None + name);
			if(at == null)
				throw new FormatException(
					string.Format("element `{0}' not contain attribute `{1}'", el.Name, name));
			if(string.IsNullOrEmpty(at.Value))
				throw new FormatException(
					string.Format("attribute `{0}' not contain text", name));
			
			return at.Value;
		}
	}
}
