# Integrating With External Events

This section covers integrating events that come from external sources, such as .NET events
or Reactive Extensions.

## .NET Events

The Events class provides three overloads of Listen to subscribe to a .NET event. Event
subscriptions are considered a resource, and therefore must have a bounded lifetime.
The Listen methods can only be called from a continuous block, so that Tempo knows
when to unsubscribe from the events.

The first overload of Listen, with no generic type parameters, is for subscribing to events
with the delegate type EventHandler. Callers simply provide a target object, an event
name and a callback. The event is looked up using reflection. The callback will be run
as an imperative block, and passed the event arguments.

The second overload, with one generic type parameter, is for subscribing to events
with the generic delegate type `EventHandler<T>`. This allows the event arguments to
be strongly typed, and otherwise follows the same pattern as the above overload.

The final overload is provided for events which do not use either EventHandler or
`EventHandler<T>`. Many events in WPF define their own delegate type, so this overload
is provided to handle those events. It follows a slightly different pattern to the other
overloads; instead of using reflection to look up the event, an action is called for
each time a delegate needs to be added or removed from the event. For example, the following
code handles subscribing to the KeyDown event of a TextBox object called 'textBox':

```
Events.Listen<KeyEventHandler, KeyEventArgs>(
    h => textBox.KeyDown += h,
    h => textBox.KeyDown -= h,
    args =>
    {
    })
```

## Reactive Extensions - IObservable

Tempo distinguishes between two cases where you might want to integrate IObservables:
Either you need to trigger an imperative action whenever a new value is emitted by the
IObservable, or the observable represents a changing value, in which case it can be
represented with an ICellRead.

The Events.Subscribe method handles the former case, while the Events.SubscribeValue method handles
the latter. SubscribeValue requires an intial value, because the observable may not emit
a value immediately, if at all, while a readable cell is always required to have a value.







