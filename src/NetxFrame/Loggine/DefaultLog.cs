using Microsoft.Extensions.Logging;
using System;

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

        public void Trace(Exception exception, string content, params object[] args)
        {
            if (!this.IsTraceEnabled)
            {
                return;
            }

            this.Logger?.Log(LogLevel.Trace, exception, content, args);
        }

        public void TraceFormat(string format, params object[] args)
        {
            if (!this.IsTraceEnabled)
            {
                return;
            }

            this.Logger?.Log(LogLevel.Trace, format, args);
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

        public void Debug(Exception exception, string content, params object[] args)
        {
            if (!this.IsDebugEnabled)
            {
                return;
            }

            this.Logger?.Log(LogLevel.Debug, exception, content, args);
        }

        public void DebugFormat(string format, params object[] args)
        {
            if (!this.IsDebugEnabled)
            {
                return;
            }

            this.Logger?.Log(LogLevel.Debug, format, args);
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

        public void Info(Exception exception, string content, params object[] args)
        {
            if (!this.IsInfoEnabled)
            {
                return;
            }

            this.Logger?.Log(LogLevel.Information, exception, content, args);
        }

        public void InfoFormat(string format, params object[] args)
        {
            if (!this.IsInfoEnabled)
            {
                return;
            }

            this.Logger?.Log(LogLevel.Information, format, args);
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

        public void Warn(Exception exception, string content, params object[] args)
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

            this.Logger?.Log(LogLevel.Warning, format, args);
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

        public void Error(Exception exception, string content, params object[] args)
        {
            if (!this.IsErrorEnabled)
            {
                return;
            }

            this.Logger?.Log(LogLevel.Error, exception, content, args);
        }

        public void ErrorFormat(string format, params object[] args)
        {
            if (!this.IsErrorEnabled)
            {
                return;
            }

            this.Logger?.Log(LogLevel.Error, format, args);
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

        public void Critical(Exception exception, string content, params object[] args)
        {
            if (!this.IsCriticalEnabled)
            {
                return;
            }

            this.Logger?.Log(LogLevel.Critical, exception, content, args);
        }

        public void CriticalFormat(string format, params object[] args)
        {
            if (!this.IsCriticalEnabled)
            {
                return;
            }

            this.Logger?.Log(LogLevel.Critical, format, args);
        }


    }

}
