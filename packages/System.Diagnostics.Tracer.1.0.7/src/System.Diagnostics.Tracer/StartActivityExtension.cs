using System;
using System.ComponentModel;
using System.Globalization;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;

namespace System.Diagnostics
{
    /// <summary>
    /// Extensions to <see cref="ITracer"/> for activity tracing.
    /// </summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
    public static class StartActivityExtension
    {
        /// <summary>
        /// Starts a new activity scope.
        /// </summary>
        public static IDisposable StartActivity(this ITracer tracer, string format, params object[] args)
        {
            return new TraceActivity(tracer, format, args);
        }

        /// <summary>
        /// Starts a new activity scope.
        /// </summary>
        public static IDisposable StartActivity(this ITracer tracer, string displayName)
        {
            return new TraceActivity(tracer, displayName);
        }

        /// <devdoc>
        /// In order for activity tracing to happen, the trace source needs to
        /// have <see cref="SourceLevels.ActivityTracing"/> enabled.
        /// </devdoc>
        class TraceActivity : IDisposable
        {
            string displayName;
            bool disposed;
            ITracer tracer;
            Guid oldId;
            Guid newId;

            public TraceActivity(ITracer tracer, string displayName)
                : this(tracer, displayName, null)
            {
            }

            public TraceActivity(ITracer tracer, string displayName, params object[] args)
            {
                this.tracer = tracer;
                this.displayName = displayName;
                if (args != null && args.Length > 0)
                    this.displayName = string.Format(displayName, args, CultureInfo.CurrentCulture);

                newId = Guid.NewGuid();
                oldId = Trace.CorrelationManager.ActivityId;

				if (oldId != Guid.Empty)
					tracer.Trace(TraceEventType.Transfer, newId);

                Trace.CorrelationManager.ActivityId = newId;
                tracer.Trace(TraceEventType.Start, this.displayName);
            }

            public void Dispose()
            {
                if (!disposed)
                {
					tracer.Trace (TraceEventType.Stop, displayName);
					if (oldId != Guid.Empty)
						tracer.Trace (TraceEventType.Transfer, oldId);

					Trace.CorrelationManager.ActivityId = oldId;
                }

                disposed = true;
            }
        }
    }
}
