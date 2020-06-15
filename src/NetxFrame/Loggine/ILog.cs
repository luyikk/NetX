using Microsoft.Extensions.Logging;
using System;

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

        void Trace(Exception exception, string content, params object[] args);

        void TraceFormat(string format, params object[] args);

        void Debug(string msg);

        void Debug(object obj);

        void Debug(Exception exception);

        void Debug(string content, Exception exception);

        void Debug(Exception exception, string content, params object[] args);

        void DebugFormat(string format, params object[] args);

        void Info(string msg);

        void Info(object obj);

        void Info(Exception exception);

        void Info(string content, Exception exception);

        void Info(Exception exception, string content, params object[] args);

        void InfoFormat(string format, params object[] args);

        void Warn(string msg);

        void Warn(object obj);

        void Warn(Exception exception);

        void Warn(string content, Exception exception);

        void Warn(Exception exception, string content, params object[] args);

        void WarnFormat(string format, params object[] args);

        void Error(string msg);

        void Error(object obj);

        void Error(Exception exception);

        void Error(string content, Exception exception);

        void Error(Exception exception, string content, params object[] args);

        void ErrorFormat(string format, params object[] args);

        void Critical(string msg);

        void Critical(object obj);

        void Critical(Exception exception);

        void Critical(string content, Exception exception);

        void Critical(Exception exception, string content, params object[] args);

        void CriticalFormat(string format, params object[] args);
    }
}
