using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Xml.Linq;
using NLog;
using System.Globalization;
using HtmlAgilityPack;

namespace RbsPayments
{
	public static class RbsSiteResponse
	{
		private static Logger Log = LogManager.GetCurrentClassLogger();
		
		public static void Login(string page, Action completed, Action<Exception> excepted)
		{
			Log.Trace("Login page:`{0}'", page);
			
			if(string.IsNullOrEmpty(page))
			{
				completed();
				return;
			}
			
			string text;
			
			try
			{
				HtmlDocument doc = new HtmlDocument();
				doc.LoadHtml(page);
				string xpath = "html/body/table/tr/td/table/tr/td/font/strong";
				HtmlNode node = doc.DocumentNode.SelectSingleNode(xpath);
				if(node == null)
					throw new FormatException("error text not found");
				text = node.InnerText;
				if(string.IsNullOrEmpty(text))
					throw new FormatException("error text not found");
			}
			catch (Exception err)
			{
				Log.Warn("can not parse page `{0}'", page);
				excepted(new FormatException("can not parse page", err));
				return;
			}
			
			excepted(new InvalidOperationException(text));
		}
		
		public static void PaymentList(string page, Action<IList<string>> completed, Action<Exception> excepted)
		{
			Log.Trace("PaymentList page:`{0}'", page);
			
			if(string.IsNullOrEmpty(page))
			{
				excepted(new InvalidOperationException("not response, login required"));
				return;
			}
			
			List<string> result = new List<string>();
			
			try
			{
				HtmlDocument doc = new HtmlDocument();
				doc.LoadHtml(page);
				string xpath = "html/body/table//tr/td/table/tr/td/table/tr/td/table//tr/td/a";
				HtmlNodeCollection nodes = doc.DocumentNode.SelectNodes(xpath);
				if(nodes == null)
					throw new FormatException("table not found");
				
				int N = nodes.Count - 1;
				for(int i = 1; i<N; i++)
				{
					string url = nodes[i].GetAttributeValue("href", null);
					if(url == null)
						continue;
					
					string mdorder = MdOrderInUrl(url);
					result.Add(mdorder);
				}
			}
			catch (Exception err)
			{
				Log.Warn("can not parse page `{0}'", page);
				excepted(new FormatException("can not parse page", err));
				return;
			}
			
			completed(result);
		}
		
		private static string _mdorderTag = "mdorder=";
		
		private static string MdOrderInUrl(string url)
		{
			int b = url.IndexOf(_mdorderTag);
			if(b == -1)
				return null;
			b += _mdorderTag.Length;
			int e = url.IndexOf("&", b);
			if(e == -1)
				return null;
			
			return url.Substring(b, e-b);
		}
	}
}
