using System.ComponentModel;
using System.Linq;

namespace System.Diagnostics
{
    /// <summary>
    /// Provides access to tracer instances.
    /// </summary>
    public static class Tracer
    {
		static readonly TracerManager implementation = new TracerManager();
		// Separate variable for the actual manager used to get sources, so that
		// it can be replaced from ReplaceManager.
		static ITracerManager manager = implementation;

		static Tracer ()
		{
			manager = implementation;
		}

		/// <summary>
		/// Provides access to the tracer manager for manipulating the
		/// underlying trace sources and their configuration.
		/// </summary>
		public static ITracerConfiguration Configuration { get { return implementation; } }

        /// <summary>
        /// Gets a tracer instance with the full type name of <typeparamref name="T"/>.
        /// </summary>
        public static ITracer Get<T>()
        {
            return manager.Get(NameFor(typeof(T)));
        }

        /// <summary>
        /// Gets a tracer instance with the full type name of the <paramref name="type"/>.
        /// </summary>
        public static ITracer Get(Type type)
        {
            return manager.Get(NameFor(type));
        }

        /// <summary>
        /// Gets a tracer instance with the given name.
        /// </summary>
        public static ITracer Get(string name)
        {
            return manager.Get(name);
        }


        /// <summary>
        /// Gets the tracer name for the given type.
        /// </summary>
        public static string NameFor<T>()
        {
            return NameFor(typeof(T));
        }

        /// <summary>
        /// Gets the tracer name for the given type.
        /// </summary>
        public static string NameFor(Type type)
        {
            if (type.IsGenericType)
            {
                var genericName = type.GetGenericTypeDefinition().FullName;

                return genericName.Substring(0, genericName.IndexOf('`')) +
                    "{" +
                    string.Join(",", type.GetGenericArguments().Select(t => NameFor(t)).ToArray()) +
                    "}";
            }

            return type.FullName;
        }

		/// <summary>
		/// Replaces the existing trace manager with a new one, and returns
		/// a disposable object that can be used to restore the previous manager.
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		public static IDisposable ReplaceManager (ITracerManager manager)
		{
			return new TracerManagerReplacer (manager);
		}

		class TracerManagerReplacer : IDisposable
		{
			ITracerManager original;

			public TracerManagerReplacer (ITracerManager manager)
			{
				original = Tracer.manager;
				Tracer.manager = manager;
			}

			public void Dispose ()
			{
				Tracer.manager = original;
			}
		}
	}
}
