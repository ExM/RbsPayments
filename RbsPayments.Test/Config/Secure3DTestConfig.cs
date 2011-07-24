using System.Xml.Serialization;
using System;
using System.Globalization;

namespace RbsPayments
{
	[XmlRoot("Secure3DTest")]
	public class Secure3DTestConfig
	{
		[XmlAttribute]
		public string BackUrl { get; set; }
		
		[XmlAttribute]
		public string Browser { get; set; }
	}
}
