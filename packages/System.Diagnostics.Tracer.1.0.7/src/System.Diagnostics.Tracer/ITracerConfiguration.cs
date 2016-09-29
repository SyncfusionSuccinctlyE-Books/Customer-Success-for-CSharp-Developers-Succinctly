namespace System.Diagnostics
{
    /// <summary>
    /// Provides access for manipulating the configuration of the
	/// underlying <see cref="TraceSource"/>s.
    /// </summary>
    public interface ITracerConfiguration
    {
		/// <summary>
		/// Source name that can be used to setup global tracing and listeners,
		/// since all source names inherit from it automatically.
		/// </summary>
		string GlobalSourceName { get; }

		/// <summary>
		/// Adds a listener to the source with the given <paramref name="sourceName"/>.
		/// </summary>
		void AddListener (string sourceName, TraceListener listener);

		/// <summary>
		/// Removes a listener from the source with the given <paramref name="sourceName"/>.
		/// </summary>
		void RemoveListener (string sourceName, TraceListener listener);

		/// <summary>
		/// Removes a listener from the source with the given <paramref name="sourceName"/>.
		/// </summary>
		void RemoveListener (string sourceName, string listenerName);

		/// <summary>
		/// Sets the tracing level for the source with the given <paramref name="sourceName"/>
		/// </summary>
		void SetTracingLevel (string sourceName, SourceLevels level);

		/// <summary>
		/// Gets the underlying trace source of the given name for low level configuration.
		/// </summary>
		TraceSource GetSource (string sourceName);
	}
}
