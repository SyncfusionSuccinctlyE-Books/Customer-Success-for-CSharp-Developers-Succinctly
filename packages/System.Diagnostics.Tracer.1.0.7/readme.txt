# System.Diagnostics.Tracer

An improved API on top of System.Diagnostics

Adds support for dynamic runtime configuration of trace sources, hierarchical trace sources and testability.

## Why

The `System.Diagnostics.TraceSource` is almost enough for trivial 
tracing. It's what .NET itself uses all over the place even. There 
are a few shortcomings though:

* Run-time configuration: can't change trace level and add/remove
  listeners unless the specific `TraceSource` instance used by the 
  code is exposed directly. 
* Inheriting configuration: can't easily turn on tracing for an 
  entire application module or namespace unless every class in it 
  is using the same trace source. So you have to think about tracing 
  scopes up-front, and can't fine-tune later.
* Testing is s harder because of both points above.
* Correlating activities is non-trivial and error prone (issuing 
  Start/Stop/Transfer calls appropriately)

## What

This project (the `Tracer` from now on) provides a solution to all three
issues, while still leveraging the built-in `TraceSource` class. This 
means you're not opting out of .NET core tracing for something totally 
new and unproven. Essentially, `Tracer` provides just a slightly improved 
API that ultimately simply extends the built-in `TraceSource`. 

This means that everything down to configuring sources via config files 
works as usual, yet you gain run-time configuration of even built-in WCF/WPF/other sources (you can try for example the "System.Windows.Markup" in a 
WPF app). In addition, integration with the 
[Service Trace Viewer](https://goo.gl/9PsPFI) is vastly improved for 
activity tracing as well as exception reporting. 


## How

### Install

    PM> Install-Package System.Diagnostics.Tracer

### Use
Just like for `TraceSource`, the recommended way to retrieve a 
tracer for your code is to do so via a static readonly field:

    public class MyClass {
        static readonly ITracer tracer = Tracer.Get<MyClass>();        

        public MyClass() {
            tracer.Info("Initialized my class!");
        }
    }

Built-in support for [using Service Trace Viewer](https://goo.gl/9PsPFI), including activity tracing:

    using (var activity = tracer.StartActivity("Uploading")) {
        tracer.Info("10% done");
        ...
    }

When exceptions are traced, any additional data in the `Exception.Data` 
dictionary is also exposed to the viewer. 


## Configure

One of the key benefits of leveraging the `Tracer` is that configuration 
can happen dynamically at run-time, via the `Tracer.Configuration`, such 
as setting trace levels and adding/removing trace listeners:

    // Add a manually configured trace listener to the entire 'Contoso' system
    Tracer.Configuration.AddListener ("Contoso", new XmlWriterTraceListener(xml));
    
    // Tune up logging for an entire sub-system
    Tracer.Configuration.SetTracingLevel ("Contoso.Payments", SourceLevels.All);
    
    // Get the underlying TraceSource if needed
    TraceSource source = Tracer.Configuration.GetSource("Contoso");
    
    // Turn on logging for all sources retrieved via Tracer
    Tracer.Configuration.SetTracingLevel ("*", SourceLevels.Warning);
    
The wildcard trace source allows system-wide configuration of sources retrieved 
via the `Tracer`, since all hierarchical sources inherit from it automatically.