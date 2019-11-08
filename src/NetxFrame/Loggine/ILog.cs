using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace Netx.Loggine
{

    public interface ILog
    {
        ILogger Logger { get; }

        bool IsTraceEnabled { get; }

        bool IsDebugEnabled { get; }

        bool IsInfoEnabled { get; }

        bool IsWarnEnabled { get; }

        bool IsErrorEnabled { get; }

        bool IsCriticalEnabled { get; }

        void Trace(string msg);

        void Trace(object obj);

        void Trace(Exception exception);

        void Trace(string content, Exception exception);

        void TraceFormat(IFormatProvider formatProvider, string format, params object[] args);

        void TraceFormat(string format, params object[] args);

        void Debug(string msg);

        void Debug(object obj);

        void Debug(Exception exception);

        void Debug(string content, Exception exception);

        void DebugFormat(IFormatProvider formatProvider, string format, params object[] args);

        void DebugFormat(string format, params object[] args);

        void Info(string msg);

        void Info(object obj);

        void Info(Exception exception);

        void Info(string content, Exception exception);

        void InfoFormat(IFormatProvider formatProvider, string format, params object[] args);

        void InfoFormat(string format, params object[] args);

        void Warn(string msg);

        void Warn(object obj);

        void Warn(Exception exception);

        void Warn(string content, Exception exception);

        void WarnFormat(IFormatProvider formatProvider, string format, params object[] args);

        void WarnFormat(string format, params object[] args);

        void Error(string msg);

        void Error(object obj);

        void Error(Exception exception);

        void Error(string content, Exception exception);

        void ErrorFormat(IFormatProvider formatProvider, string format, params object[] args);

        void ErrorFormat(string format, params object[] args);

        void Critical(string msg);

        void Critical(object obj);

        void Critical(Exception exception);

        void Critical(string content, Exception exception);

        void CriticalFormat(IFormatProvider formatProvider, string format, params object[] args);

        void CriticalFormat(string format, params object[] args);
    }
}
