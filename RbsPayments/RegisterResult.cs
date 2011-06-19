using System;

namespace RbsPayments
{
	public class RegisterResult
	{

		public readonly string MdOrder;
		public readonly ResultCode Code;
		public readonly RbsPaymentState State;
		public readonly string AcsUrl;
		public readonly string PaReq;
		public readonly bool Required3DSecure;
		
		public RegisterResult(ResultCode resCode)
		{
			MdOrder = null;
			Code = resCode;
			State = RbsPaymentState.Unknown;
			AcsUrl = null;
			PaReq = null;
			Required3DSecure = false;
		}
		
		public RegisterResult(string mdOrder, ResultCode resCode, RbsPaymentState state)
		{
			MdOrder = mdOrder;
			Code = resCode;
			State = state;
			AcsUrl = null;
			PaReq = null;
			Required3DSecure = false;
		}
		
		public RegisterResult(string mdOrder, string acsUrl, string paReq)
		{
			MdOrder = mdOrder;
			Code = new ResultCode(){PrimaryRC = 0, SecondaryRC = 0};
			State = RbsPaymentState.Unknown;
			AcsUrl = acsUrl;
			PaReq = paReq;
			Required3DSecure = true;
		}
	}
}

