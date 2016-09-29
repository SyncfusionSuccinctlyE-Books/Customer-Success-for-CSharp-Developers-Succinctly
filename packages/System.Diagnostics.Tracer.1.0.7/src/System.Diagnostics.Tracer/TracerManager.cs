using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;

namespace System.Diagnostics
{
	class TracerManager : ITracerManager, ITracerConfiguration
	{
		const string globalSourceName = "*";
		static readonly TraceSource globalSource = new TraceSource(globalSourceName);

		// If we can grab this from reflection from the TraceSource, we do it,
		// and it means we can also manipulate the configuration for built-in
		// sources instantiated natively by the BCLs throughout .NET :).
		static List<WeakReference> traceSourceCache = new List<WeakReference>();

		public string GlobalSourceName { get { return globalSourceName; } }

		static TracerManager ()
		{
			var cacheField = typeof(TraceSource).GetField("tracesources", BindingFlags.Static | BindingFlags.NonPublic);
			if (cacheField != null) {
				try {
					traceSourceCache = (List<WeakReference>)cacheField.GetValue (null);
				} catch {
					// Ignore, something went wrong, won't be able to configure system sources.
					globalSource.TraceEvent (TraceEventType.Warning, 0, Properties.Resources.FailedToRetrieveCache);
				}
			} else {
				globalSource.TraceEvent (TraceEventType.Warning, 0, Properties.Resources.FailedToRetrieveCache);
			}
		}

		public ITracer Get (string name)
		{
			return new AggregateTracer (name, CompositeFor (name)
				.Select (tracerName => new DiagnosticsTracer (
					 this.GetOrAdd (tracerName, sourceName => CreateSource (sourceName)))));
		}

		public TraceSource GetSource (string name)
		{
			return GetOrAdd (name, sourceName => CreateSource (sourceName));
		}

		public void AddListener (string sourceName, TraceListener listener)
		{
			GetOrAdd (sourceName, name => CreateSource (name)).Listeners.Add (listener);
		}

		public void RemoveListener (string sourceName, TraceListener listener)
		{
			GetOrAdd (sourceName, name => CreateSource (name)).Listeners.Remove (listener);
		}

		public void RemoveListener (string sourceName, string listenerName)
		{
			GetOrAdd (sourceName, name => CreateSource (name)).Listeners.Remove (listenerName);
		}

		public void SetTracingLevel (string sourceName, SourceLevels level)
		{
			GetOrAdd (sourceName, name => CreateSource (name)).Switch.Level = level;
		}

		TraceSource CreateSource (string name)
		{
			var source = new TraceSource(name);
			source.TraceInformation ("Initialized trace source {0} with initial level {1}", name, source.Switch.Level);
			return source;
		}

		/// <summary>
		/// Gets the list of trace source names that are used to inherit trace source logging for the given <paramref name="name"/>.
		/// </summary>
		static IEnumerable<string> CompositeFor (string name)
		{
			if (name != globalSourceName)
				yield return globalSourceName;

			var indexOfGeneric = name.IndexOf('<');
			var indexOfLastDot = name.LastIndexOf('.');

			if (indexOfGeneric == -1 && indexOfLastDot == -1) {
				yield return name;
				yield break;
			}

			var parts = default(string[]);

			if (indexOfGeneric == -1)
				parts = name
					.Substring (0, name.LastIndexOf ('.'))
					.Split (new[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
			else
				parts = name
					.Substring (0, indexOfGeneric)
					.Split (new[] { '.' }, StringSplitOptions.RemoveEmptyEntries);

			for (int i = 1; i <= parts.Length; i++) {
				yield return string.Join (".", parts, 0, i);
			}

			yield return name;
		}

		TraceSource GetOrAdd (string sourceName, Func<string, TraceSource> factory)
		{
			if (sourceName == globalSourceName)
				return globalSource;

			var cachedSource = traceSourceCache
				.Where(weak => weak.IsAlive)
				.Select(weak => (TraceSource)weak.Target)
				.FirstOrDefault(source => source != null && source.Name == sourceName);

			return cachedSource ?? factory (sourceName);
		}

		/// <summary>
		/// Logs to multiple tracers simulateously. Used for the
		/// source "inheritance"
		/// </summary>
		class AggregateTracer : ITracer
		{
			private List<DiagnosticsTracer> tracers;
			private string name;

			public AggregateTracer (string name, IEnumerable<DiagnosticsTracer> tracers)
			{
				this.name = name;
				this.tracers = tracers.ToList ();
			}

			/// <summary>
			/// Traces the specified message with the given <see cref="TraceEventType"/>.
			/// </summary>
			public void Trace (TraceEventType type, object message)
			{
				tracers.ForEach (tracer => tracer.Trace (name, type, message));
			}

			/// <summary>
			/// Traces the specified formatted message with the given <see cref="TraceEventType"/>.
			/// </summary>
			public void Trace (TraceEventType type, string format, params object[] args)
			{
				tracers.ForEach (tracer => tracer.Trace (name, type, format, args));
			}

			/// <summary>
			/// Traces an exception with the specified message and <see cref="TraceEventType"/>.
			/// </summary>
			public void Trace (TraceEventType type, Exception exception, object message)
			{
				tracers.ForEach (tracer => tracer.Trace (name, type, exception, message));
			}

			/// <summary>
			/// Traces an exception with the specified formatted message and <see cref="TraceEventType"/>.
			/// </summary>
			public void Trace (TraceEventType type, Exception exception, string format, params object[] args)
			{
				tracers.ForEach (tracer => tracer.Trace (name, type, exception, format, args));
			}

			public override string ToString ()
			{
				return "Aggregate sources for " + this.name;
			}
		}

		/// <summary>
		/// Implements the <see cref="ITracer"/> interface on top of
		/// <see cref="TraceSource"/>.
		/// </summary>
		class DiagnosticsTracer
		{
			TraceSource source;

			public DiagnosticsTracer (TraceSource source)
			{
				this.source = source;
			}

			public void Trace (string sourceName, TraceEventType type, object message)
			{
				lock (source) {
					using (new SourceNameReplacer (source, sourceName)) {
						// Transfers with a Guid payload should instead trace a transfer
						// with that as the related Guid.
						var guid = message as Guid?;
						if (guid != null && type == TraceEventType.Transfer)
							source.TraceTransfer (0, "Transfer", guid.Value);
						else
							source.TraceEvent (type, 0, message.ToString ());
					}
				}
			}

			public void Trace (string sourceName, TraceEventType type, string format, params object[] args)
			{
				lock (source) {
					using (new SourceNameReplacer (source, sourceName)) {
						source.TraceEvent (type, 0, format, args);
					}
				}
			}

			public void Trace (string sourceName, TraceEventType type, Exception exception, object message)
			{
				Trace (sourceName, type, exception, message.ToString ());
			}

			public void Trace (string sourceName, TraceEventType type, Exception exception, string format, params object[] args)
			{
				lock (source) {
					using (new SourceNameReplacer (source, sourceName)) {
						var message = format;
						if (args != null && args.Length > 0)
							message = string.Format (CultureInfo.CurrentCulture, format, args);

						if (TraceExceptionXml (exception, message)) {
							// This means we've traced the exception as XML to at least one XML writer,
							// So we should skip the next event from them.
							var xmlListeners = source.Listeners.OfType<XmlWriterTraceListener>().ToArray();
							foreach (var listener in xmlListeners) {
								source.Listeners.Remove (listener);
							}

							try {
								source.TraceEvent (type, 0, string.Format (format, args) + Environment.NewLine + exception);
							} finally {
								source.Listeners.AddRange (xmlListeners);
							}
						}
					}
				}
			}

			const string TraceXmlNs = "http://schemas.microsoft.com/2004/10/E2ETraceEvent/TraceRecord";

			bool TraceExceptionXml (Exception exception, string format, params object[] args)
			{
				if (source.Listeners.OfType<XmlWriterTraceListener> ().Any ()) {
					var message = format;
					if (args != null && args.Length > 0)
						message = string.Format (CultureInfo.CurrentCulture, format, args);

					var xdoc = new XDocument();
					var writer = xdoc.CreateWriter();

					writer.WriteStartElement ("", "TraceRecord", TraceXmlNs);
					writer.WriteAttributeString ("Severity", "Error");
					//writer.WriteElementString ("TraceIdentifier", msdnTraceCode);
					writer.WriteElementString ("Description", TraceXmlNs, message);
					writer.WriteElementString ("AppDomain", TraceXmlNs, AppDomain.CurrentDomain.FriendlyName);
					writer.WriteElementString ("Source", TraceXmlNs, source.Name);
					AddExceptionXml (writer, exception);
					writer.WriteEndElement ();

					writer.Flush ();
					writer.Close ();

					source.TraceData (TraceEventType.Error, 0, xdoc.CreateNavigator ());
					return true;
				}

				return false;
			}

			static void AddExceptionXml (XmlWriter xml, Exception exception)
			{
				xml.WriteElementString ("ExceptionType", TraceXmlNs, exception.GetType ().AssemblyQualifiedName);
				xml.WriteElementString ("Message", TraceXmlNs, exception.Message);
				xml.WriteElementString ("StackTrace", TraceXmlNs, exception.StackTrace);
				xml.WriteElementString ("ExceptionString", TraceXmlNs, exception.ToString ());

				if ((exception.Data != null) && (exception.Data.Count > 0)) {
					xml.WriteStartElement ("DataItems", TraceXmlNs);
					foreach (var key in exception.Data.Keys) {
						var data = exception.Data[key];
						xml.WriteElementString (key.ToString (), data != null ? data.ToString () : "");
					}
					xml.WriteEndElement ();
				}

				if (exception.InnerException != null) {
					xml.WriteStartElement ("InnerException", TraceXmlNs);
					AddExceptionXml (xml, exception.InnerException);
					xml.WriteEndElement ();
				}
			}
		}

		/// <summary>
		/// The TraceSource instance name matches the name of each of the "segments"
		/// we built the aggregate source from. This means that when we trace, we issue
		/// multiple trace statements, one for each. If a listener is added to (say) "*"
		/// source name, all traces done through it will appear as coming from the source
		/// "*", rather than (say) "Foo.Bar" which might be the actual source class.
		/// This diminishes the usefulness of hierarchical loggers significantly, since
		/// it now means that you need to add listeners too all trace sources you're
		/// interested in receiving messages from, and all its "children" potentially,
		/// some of them which might not have been created even yet. This is not feasible.
		/// Instead, since we issue the trace call to each trace source (which is what
		/// enables the configurability of all those sources in the app.config file),
		/// we need to fix the source name right before tracing, so that a configured
		/// listener at "*" still receives as the source name the original (aggregate) one,
		/// and not "*". This requires some private reflection, and a lock to guarantee
		/// proper logging, but this decreases its performance.
		/// </summary>
		class SourceNameReplacer : IDisposable
		{
			// Private reflection needed here in order to make the inherited source names still
			// log as if the original source name was the one logging, so as not to lose the
			// originating class name.
			static readonly FieldInfo sourceNameField = typeof(TraceSource).GetField("sourceName", BindingFlags.Instance | BindingFlags.NonPublic);

			TraceSource source;
			string originalName;

			static Action<TraceSource, string> replacer;

			static SourceNameReplacer ()
			{
				if (sourceNameField == null)
					replacer = (source, name) => { };
				else
					replacer = (source, name) => sourceNameField.SetValue (source, name);
			}

			public SourceNameReplacer (TraceSource source, string sourceName)
			{
				this.source = source;
				this.originalName = source.Name;
				// Transient change of the source name while the trace call
				// is issued, if possible.
				replacer (source, sourceName);
			}

			public void Dispose ()
			{
				replacer (source, originalName);
			}
		}
	}
}
