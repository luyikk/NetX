using System;
using System.Globalization;
using Microsoft.Extensions.Logging;

namespace Netx.Loggine
{

    public sealed class DefaultLog : ILog
    {
        static readonly Func<object, Exception, string> MessageFormatter = Format;
        public ILogger Logger { get; }

        public DefaultLog(ILogger logger)
        {
            this.Logger = logger;
        }

        static string Format(object target, Exception exception)
           => exception == null ? target.ToString() : $"{target} {exception}";


        public bool IsTraceEnabled =>
            this.Logger?.IsEnabled(LogLevel.Trace) ?? false;

        public bool IsDebugEnabled =>
            this.Logger?.IsEnabled(LogLevel.Debug) ?? false;

        public bool IsInfoEnabled =>
            this.Logger?.IsEnabled(LogLevel.Information) ?? false;

        public bool IsWarnEnabled =>
            this.Logger?.IsEnabled(LogLevel.Warning) ?? false;

        public bool IsErrorEnabled =>
            this.Logger?.IsEnabled(LogLevel.Error) ?? false;

        public bool IsCriticalEnabled =>
            this.Logger?.IsEnabled(LogLevel.Critical) ?? false;

        public void Trace(string msg)
        {
            if (!this.IsTraceEnabled)
            {
                return;
            }

            this.Logger?.Log(LogLevel.Trace, msg);
        }

        public void Trace(object obj)
         => Trace(obj.ToString());

        public void Trace(Exception exception)
        {
            if (!this.IsTraceEnabled)
            {
                return;
            }

            this.Logger?.Log(LogLevel.Trace, exception, exception.Message);
        }

        public void Trace(string content, Exception exception)
        {
            if (!this.IsTraceEnabled)
            {
                return;
            }

            this.Logger?.Log(LogLevel.Trace, exception, content);
        }

        public void TraceFormat(string format, params object[] args)
        {
            if (!this.IsTraceEnabled)
            {
                return;
            }

            this.TraceFormat(CultureInfo.CurrentCulture, format, args);
        }

        public void TraceFormat(IFormatProvider formatProvider, string format, params object[] args)
        {
            if (!this.IsTraceEnabled
                || formatProvider == null
                || string.IsNullOrEmpty(format))
            {
                return;
            }

            string message = string.Format(formatProvider, format, args);
            this.Logger?.Log(LogLevel.Trace, message);
        }

        public void Debug(string msg)
        {
            if (!this.IsDebugEnabled)
            {
                return;
            }

            this.Logger?.Log(LogLevel.Debug, msg);
        }

        public void Debug(object obj)
            => Debug(obj.ToString());

        public void Debug(Exception exception)
        {
            if (!this.IsDebugEnabled)
            {
                return;
            }

            this.Logger?.Log(LogLevel.Debug, exception, exception.Message);
        }

        public void Debug(string content, Exception exception)
        {
            if (!this.IsDebugEnabled)
            {
                return;
            }

            this.Logger?.Log(LogLevel.Debug, exception, content);
        }

        public void DebugFormat(string format, params object[] args)
        {
            if (!this.IsDebugEnabled)
            {
                return;
            }

            this.DebugFormat(CultureInfo.CurrentCulture, format, args);
        }

        public void DebugFormat(IFormatProvider formatProvider, string format, params object[] args)
        {
            if (!this.IsDebugEnabled
                || formatProvider == null
                || string.IsNullOrEmpty(format))
            {
                return;
            }

            string message = string.Format(formatProvider, format, args);
            this.Logger?.Log(LogLevel.Debug, message);
        }

        public void Info(string msg)
        {
            if (!this.IsInfoEnabled)
            {
                return;
            }

            this.Logger?.Log(LogLevel.Information, msg);
        }

        public void Info(object obj)
            => Info(obj.ToString());

        public void Info(Exception exception)
        {
            if (!this.IsInfoEnabled)
            {
                return;
            }

            this.Logger?.Log(LogLevel.Information, exception, exception.Message);
        }

        public void Info(string content, Exception exception)
        {
            if (!this.IsInfoEnabled)
            {
                return;
            }

            this.Logger?.Log(LogLevel.Information, exception, content);
        }

        public void InfoFormat(string format, params object[] args)
        {
            if (!this.IsInfoEnabled)
            {
                return;
            }

            this.InfoFormat(CultureInfo.CurrentCulture, format, args);
        }

        public void InfoFormat(IFormatProvider formatProvider, string format, params object[] args)
        {
            if (!this.IsInfoEnabled
                || formatProvider == null
                || string.IsNullOrEmpty(format))
            {
                return;
            }

            string message = string.Format(formatProvider, format, args);
            this.Logger?.Log(LogLevel.Information, message);
        }

        public void Warn(string msg)
        {
            if (!this.IsWarnEnabled)
            {
                return;
            }

            this.Logger?.Log(LogLevel.Warning, msg);
        }

        public void Warn(object obj)
            => Warn(obj.ToString());

        public void Warn(Exception exception)
        {
            if (!this.IsWarnEnabled)
            {
                return;
            }

            this.Logger?.Log(LogLevel.Warning, exception, exception.Message);
        }

        public void Warn(string content, Exception exception)
        {
            if (!this.IsWarnEnabled)
            {
                return;
            }

            this.Logger?.Log(LogLevel.Warning, exception, content);
        }

        public void WarnFormat(string format, params object[] args)
        {
            if (!this.IsWarnEnabled)
            {
                return;
            }

            this.WarnFormat(CultureInfo.CurrentCulture, format, args);
        }

        public void WarnFormat(IFormatProvider formatProvider, string format, params object[] args)
        {
            if (!this.IsWarnEnabled
                || formatProvider == null
                || string.IsNullOrEmpty(format))
            {
                return;
            }

            string message = string.Format(formatProvider, format, args);
            this.Logger?.Log(LogLevel.Warning, message);
        }

        public void Error(string msg)
        {
            if (!this.IsErrorEnabled)
            {
                return;
            }

            this.Logger?.Log(LogLevel.Error, msg);
        }

        public void Error(object obj)
            => Error(obj.ToString());

        public void Error(Exception exception)
        {
            if (!this.IsErrorEnabled)
            {
                return;
            }

            this.Logger?.Log(LogLevel.Error, exception, exception.Message);
        }

        public void Error(string content, Exception exception)
        {
            if (!this.IsErrorEnabled)
            {
                return;
            }

            this.Logger?.Log(LogLevel.Error, exception, content);
        }

        public void ErrorFormat(string format, params object[] args)
        {
            if (!this.IsErrorEnabled)
            {
                return;
            }

            this.ErrorFormat(CultureInfo.CurrentCulture, format, args);
        }

        public void ErrorFormat(IFormatProvider formatProvider, string format, params object[] args)
        {
            if (!this.IsErrorEnabled
                || formatProvider == null
                || string.IsNullOrEmpty(format))
            {
                return;
            }

            string message = string.Format(formatProvider, format, args);
            this.Logger?.Log(LogLevel.Error, message);
        }

        public void Critical(string msg)
        {
            if (!this.IsCriticalEnabled)
            {
                return;
            }


            this.Logger?.Log(LogLevel.Critical, msg);
        }

        public void Critical(object obj)
            => Critical(obj.ToString());

        public void Critical(Exception exception)
        {
            if (!this.IsCriticalEnabled)
            {
                return;
            }

            this.Logger?.Log(LogLevel.Critical, exception, exception.Message);
        }

        public void Critical(string content, Exception exception)
        {
            if (!this.IsCriticalEnabled)
            {
                return;
            }

            this.Logger?.Log(LogLevel.Critical, exception, content);
        }

        public void CriticalFormat(string format, params object[] args)
        {
            if (!this.IsCriticalEnabled)
            {
                return;
            }

            this.CriticalFormat(CultureInfo.CurrentCulture, format, args);
        }

        public void CriticalFormat(IFormatProvider formatProvider, string format, params object[] args)
        {
            if (!this.IsCriticalEnabled
                || formatProvider == null
                || string.IsNullOrEmpty(format))
            {
                return;
            }

            string message = string.Format(formatProvider, format, args);
            this.Logger?.Log(LogLevel.Critical, message);
        }
    }

}
