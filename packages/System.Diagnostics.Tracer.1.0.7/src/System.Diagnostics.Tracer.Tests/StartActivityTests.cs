using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using Moq;
using Xunit;

namespace System.Diagnostics
{
	public class StartActivityTests
	{
		[Fact]
		public void when_using_activity_then_starts_and_ends ()
		{
			var listener = new Mock<TraceListener>();
			Tracer.Configuration.AddListener ("Foo", listener.Object);
			Tracer.Configuration.SetTracingLevel ("Foo", SourceLevels.All);

			var originalId = Guid.NewGuid();
			Trace.CorrelationManager.ActivityId = originalId;

			var tracer = Tracer.Get("Foo");
			var activityId = Guid.Empty;

			using (var activity = tracer.StartActivity("Test")) {
				activityId = Trace.CorrelationManager.ActivityId;
			}

			Assert.NotEqual (Guid.Empty, activityId);

			// Activity equals:
			//	 Transfer -> Data(Start) -> [Event] -> Data(Stop) -> Transfer

			listener.Verify (x => x.TraceTransfer (
				It.IsAny<TraceEventCache> (),
				"Foo",
				It.IsAny<int> (),
				It.IsAny<string> (),
				activityId));

			listener.Verify (x => x.TraceEvent (
				It.IsAny<TraceEventCache> (),
				"Foo",
				TraceEventType.Start,
				It.IsAny<int> (),
				It.IsAny<string>(),
				It.IsAny<object[]> ()));

			listener.Verify (x => x.TraceEvent (
				It.IsAny<TraceEventCache> (),
				"Foo",
				TraceEventType.Stop,
				It.IsAny<int> (),
				It.IsAny<string> (),
				It.IsAny<object[]> ()));

			listener.Verify (x => x.TraceTransfer (
				It.IsAny<TraceEventCache> (),
				"Foo",
				It.IsAny<int> (),
				It.IsAny<string> (),
				originalId));
		}

		[Fact]
		public void when_no_initial_activity_then_does_not_transfer ()
		{
			var listener = new Mock<TraceListener>();
			Tracer.Configuration.AddListener ("Foo", listener.Object);
			Tracer.Configuration.SetTracingLevel ("Foo", SourceLevels.All);

			Trace.CorrelationManager.ActivityId = Guid.Empty;

			var tracer = Tracer.Get("Foo");
			var activityId = Guid.Empty;

			using (var activity = tracer.StartActivity ("Test")) {
				activityId = Trace.CorrelationManager.ActivityId;
			}

			Assert.NotEqual (Guid.Empty, activityId);

			// Activity without existing activity in progress equals:
			//	 Data(Start) -> [Event] -> Data(Stop)

			listener.Verify (x => x.TraceEvent (
				It.IsAny<TraceEventCache> (),
				"Foo",
				TraceEventType.Start,
				It.IsAny<int> (),
				It.IsAny<string> (),
				It.IsAny<object[]> ()));

			listener.Verify (x => x.TraceEvent (
				It.IsAny<TraceEventCache> (),
				"Foo",
				TraceEventType.Stop,
				It.IsAny<int> (),
				It.IsAny<string> (),
				It.IsAny<object[]> ()));

			listener.Verify (x => x.TraceTransfer (
				It.IsAny<TraceEventCache> (),
				"Foo",
				It.IsAny<int> (),
				It.IsAny<string> (),
				It.IsAny<Guid>()),
				Times.Never ());
		}

		[Fact]
		public void when_activity_has_exception_then_traces_data ()
		{
			var xml = Path.GetTempFileName();
			var listener = new XmlWriterTraceListener(xml);

			Tracer.Configuration.AddListener ("Foo", listener);
			Tracer.Configuration.SetTracingLevel ("Foo", SourceLevels.All);

			var tracer = Tracer.Get("Foo");

			using (var outer = tracer.StartActivity ("Outer")) {
				using (var inner = tracer.StartActivity("Inner")) {
					tracer.Info ("Hey & > 23!");
				}

				try {
					throw new ArgumentException ();
				} catch (Exception ex) {
					ex.Data.Add ("Version", "1.0");
					ex.Data.Add ("Branch", "master");
					ex.Data.Add ("Commit", "1ab314b");
					tracer.Error (ex, "Some error!");
				}
			}

			Tracer.Configuration.RemoveListener ("Foo", listener);
			listener.Flush ();
			listener.Dispose ();

			using (var reader = XmlReader.Create (xml, new XmlReaderSettings { ConformanceLevel = ConformanceLevel.Fragment })) {
				Assert.Equal (XmlNodeType.Element, reader.MoveToContent ());
				while (reader.Read()) {
					if (reader.NodeType == XmlNodeType.Element && reader.NamespaceURI == "http://schemas.microsoft.com/2004/10/E2ETraceEvent/TraceRecord") {
						Assert.True (reader.ReadToDescendant ("Commit", "http://schemas.microsoft.com/2004/10/E2ETraceEvent/TraceRecord"));
						Assert.Equal ("1ab314b", reader.ReadElementContentAsString ());
					}
				}
			}

			//Process.Start ("notepad", xml);

			//Process.Start (@"C:\Program Files (x86)\Microsoft SDKs\Windows\v10.0A\bin\NETFX 4.6 Tools\SvcTraceViewer.exe", xml);
			//Process.Start (@"C:\Program Files (x86)\Microsoft SDKs\Windows\v7.0A\Bin\SvcTraceViewer.exe", xml);
			//Process.Start (@"C:\Program Files (x86)\Microsoft SDKs\Windows\v8.0A\bin\NETFX 4.0 Tools\SvcTraceViewer.exe", xml);
			//Process.Start (@"C:\Program Files (x86)\Microsoft SDKs\Windows\v8.1A\bin\NETFX 4.5.1 Tools\SvcTraceViewer.exe", xml);
		}
	}
}
