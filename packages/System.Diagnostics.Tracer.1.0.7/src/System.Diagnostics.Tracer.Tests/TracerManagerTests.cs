using Moq;
using Xunit;

namespace System.Diagnostics
{
    public class TracerManagerTests
    {
		[Fact]
		public void when_getting_source_then_can_retrieve_built_in_source ()
		{
			var manager = new TracerManager();
			var expected = PresentationTraceSources.MarkupSource;
			var actual = manager.GetSource("System.Windows.Markup");

			Assert.Same (expected, actual);
		}

		[Fact]
		public void when_tracing_aggregate_then_replaces_source_name_for_each_level ()
		{
			var globalListener = new Mock<TraceListener>();
			var fooListener = new Mock<TraceListener>();
			var barListener = new Mock<TraceListener>();

			var manager = new TracerManager();

			var tracer = manager.Get("Foo.Bar");

			manager.AddListener (Tracer.Configuration.GlobalSourceName, globalListener.Object);
			manager.AddListener ("Foo", fooListener.Object);
			manager.AddListener ("Foo.Bar", barListener.Object);

			manager.SetTracingLevel (Tracer.Configuration.GlobalSourceName, SourceLevels.All);
			manager.SetTracingLevel ("Foo", SourceLevels.All);
			manager.SetTracingLevel ("Foo.Bar", SourceLevels.All);

			tracer.Trace (TraceEventType.Information, "Hi");

			globalListener.Verify (x => x.TraceEvent (It.IsAny<TraceEventCache> (), "Foo.Bar", TraceEventType.Information, It.IsAny<int> (), "Hi", It.IsAny<object[]> ()));
			fooListener.Verify (x => x.TraceEvent (It.IsAny<TraceEventCache> (), "Foo.Bar", TraceEventType.Information, It.IsAny<int> (), "Hi", It.IsAny<object[]> ()));
			barListener.Verify (x => x.TraceEvent (It.IsAny<TraceEventCache> (), "Foo.Bar", TraceEventType.Information, It.IsAny<int> (), "Hi", It.IsAny<object[]> ()));
		}

		[Fact]
		public void when_removing_listener_then_stops_tracing_to_added_listener ()
		{
			var listener = new Mock<TraceListener>();
			var manager = new TracerManager();

			var tracer = manager.Get("Foo");

			manager.SetTracingLevel ("Foo", SourceLevels.Information);
			manager.AddListener ("Foo", listener.Object);

			tracer.Info ("Hi");
			manager.RemoveListener ("Foo", listener.Object);
			tracer.Info ("Hi");

			listener.Verify (x => x.TraceEvent (It.IsAny<TraceEventCache> (), "Foo", TraceEventType.Information, It.IsAny<int> (), "Hi", It.IsAny<object[]> ()), Times.Once());
		}

		[Fact]
		public void when_removing_listener_by_name_then_stops_tracing_to_added_listener ()
		{
			var listener = new Mock<TraceListener>();
			listener.Setup (x => x.Name).Returns ("FooListener");
			var manager = new TracerManager();

			var tracer = manager.Get("Foo");

			manager.SetTracingLevel ("Foo", SourceLevels.Information);
			manager.AddListener ("Foo", listener.Object);

			tracer.Info ("Hi");
			manager.RemoveListener ("Foo", "FooListener");
            tracer.Info ("Hi");

			listener.Verify (x => x.TraceEvent (It.IsAny<TraceEventCache> (), "Foo", TraceEventType.Information, It.IsAny<int> (), "Hi", It.IsAny<object[]> ()), Times.Once ());
		}

		[Fact]
		public void when_tracing_below_configured_level_then_does_not_trace_to_listener ()
		{
			var listener = new Mock<TraceListener>();
			var manager = new TracerManager();

			var tracer = manager.Get("Foo");

			manager.SetTracingLevel ("Foo", SourceLevels.Warning);
			manager.AddListener ("Foo", listener.Object);

			tracer.Info ("Hi");
			tracer.Verbose ("Bye");

			listener.Verify (x => x.TraceEvent (It.IsAny<TraceEventCache> (), "Foo", TraceEventType.Information, It.IsAny<int> (), "Hi", It.IsAny<object[]> ()), Times.Never ());
		}

		[Fact]
		public void when_tracing_event_then_succeeds ()
		{
			var listener = new Mock<TraceListener>();
			var manager = new TracerManager();

			var tracer = manager.Get("Foo");

			manager.SetTracingLevel ("Foo", SourceLevels.All);
			manager.AddListener ("Foo", listener.Object);

			tracer.Trace (TraceEventType.Suspend, "Suspend");

			listener.Verify (x => x.TraceEvent (It.IsAny<TraceEventCache> (), "Foo", TraceEventType.Suspend, It.IsAny<int> (), "Suspend", It.IsAny<object[]> ()), Times.Once());
		}

		[Fact]
		public void when_replacing_manager_then_can_provide_mock ()
		{
			var tracer = Mock.Of<ITracer>();
			var manager = Mock.Of<ITracerManager>(x => x.Get("Foo") == tracer);

			using (Tracer.ReplaceManager(manager)) {
				Assert.Same (tracer, Tracer.Get ("Foo"));
			}

			Assert.NotSame (tracer, Tracer.Get ("Foo"));
		}
	}
}
