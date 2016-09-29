using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace System.Diagnostics
{
	public class TracerTests
	{
		[Fact]
		public void when_getting_source_name_from_type_then_retrieves_full_name ()
		{
			var name = Tracer.NameFor<TracerTests>();

			Assert.Equal (typeof (TracerTests).FullName, name);
		}

		/// <summary>
		/// <see cref="System.Lazy{System.Tuple{String, int}}"/>.
		/// </summary>
		[Fact]
		public void when_getting_source_name_for_generics_type_then_retrieves_documentation_style_generic ()
		{
			var name = Tracer.NameFor<Lazy<Tuple<string, int>>>();

			Assert.Equal ("System.Lazy{System.Tuple{System.String,System.Int32}}", name);
		}
	}
}
