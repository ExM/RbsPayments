using System.Xml.Serialization;
using System;
using System.Globalization;

namespace RbsPayments
{
	[XmlRoot("RbsConnection")]
	public class RbsConnectionConfig
	{
		[XmlAttribute]
		public string Uri { get; set; }
		
		[XmlElement("Merchant")]
		public AuthenticateConfig Merchant {get; set;}
		
		[XmlElement("Site")]
		public AuthenticateConfig Site {get; set;}
		
		[XmlElement("Refund")]
		public AuthenticateConfig Refund {get; set;}
	}
}
