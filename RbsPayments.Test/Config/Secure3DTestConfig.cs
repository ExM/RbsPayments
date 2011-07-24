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
		
		[XmlIgnore]
		public TimeSpan UserWait = TimeSpan.Zero;
		[XmlAttribute("UserWait")]
		public string xmlUserWait
		{
			get
			{
				return UserWait.ToString();
			}
			set
			{
				UserWait = TimeSpan.Parse(value);
			}
		}
	}
}
