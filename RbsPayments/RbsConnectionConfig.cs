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

		[XmlAttribute]
		public string MerchantNumber { get; set; }

		[XmlAttribute]
		public string MerchantPassword { get; set; }

		[XmlAttribute]
		public string User { get; set; }
		
		[XmlAttribute]
		public string Password { get; set; }
	}
}
