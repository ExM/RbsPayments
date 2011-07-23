using System.Xml.Serialization;
using System;
using System.Globalization;

namespace RbsPayments
{
	public class AuthenticateConfig
	{
		[XmlAttribute]
		public string User { get; set; }
		
		[XmlAttribute]
		public string Pass { get; set; }
	}
}
