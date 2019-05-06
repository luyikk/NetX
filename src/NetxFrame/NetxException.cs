using System;
using System.Collections.Generic;
using System.Text;

namespace Netx
{
    /// <summary>
    /// 定制异常
    /// </summary>
    public class NetxException:Exception
    {
        /// <summary>
        /// 错误信息
        /// </summary>
        public string ErrorMsg { get; private set; }

        /// <summary>
        /// 错误类型
        /// </summary>
        public ErrorType ErrorType { get; private set; }

        /// <summary>
        /// 错误ID
        /// </summary>
        public int ErrorId { get; private set; }

        public NetxException(string msg, ErrorType errorType)
            :base(msg)
        {
            this.ErrorMsg = msg;
            this.ErrorType = errorType;
            this.ErrorId = (int)errorType;
        }

        public NetxException(string msg,int errorId)
        {
            this.ErrorMsg = msg;
            ErrorId = errorId;
            ErrorType = ErrorType.Other;
        }

        public override string Message => ErrorType+":"+ErrorMsg;



        public static string GetExceptionToString(Exception er)
        {
            if (er == null)
                return "";

            return er.ToString() +"\r\n\r\n"+ GetExceptionToString(er.InnerException);
        }

    }
}
