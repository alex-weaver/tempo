# Integrating With WPF

This section covers integrating with WPF user interfaces. This covers handling
the application lifecycle, binding to dependency properties and listening to events.

## The WPF Scheduler

Tempo.WPF provides an implementation of IScheduler which uses a WPF dispatcher to
perform the work. Normally, applications do not need to use WpfScheduler directly,
since the TempoApp.Init method will construct one for you.

## Application Lifecycle

The helper method TempoApp.Init is provided to construct the top-level Tempo scope
for the application. This method observes the Startup and Exit events of the Application
object, so that it knows when to create and destroy the top-level scope. This means
that TempoApp.Init must be called *before* the application is started, otherwise the Startup
event will not be handled. The constructor is usually the best place to do this.

## Binding to Dependency Properties

Dependency properties can be bound to value cells, in either direction. The PropertyAdapter
class provides two methods, Read and Write, to handle this interaction boundary. The Read method
returns a read-only cell which observes the dependency property, while Write returns a write-only
cell that forwards any modifications to the dependency property. These cells can then be bound
to other cells managed by Tempo.

## Listening to Events

In addition to dependency properties, WPF controls expose regular .NET events too. Observing
.NET events is covered in [Integrating With External Events](./integrating-events.md).

## Binding to Selector Controls

While `IListCellRead<T>.Bind` can be used to bind directly to IList objects, and hence could bind to the
Selector.Items property, this does not correctly preserve the selection state of the
Selector control. Instead, Tempo.Wpf provides the SelectorBinding.Bind method which
includes additional logic to ensure the selection remains consistent when the source
list is modified.


