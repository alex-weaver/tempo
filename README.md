# Tempo

Tempo is a C# library for building reactive applications. Tempo provides mechanisms for
safely mixing
reactive code with imperative code. Rather than focusing on **events**, reactive code
in Tempo models **relationships** between elements of time-varying state. Imperative code
blocks are **isolated**; changes made by imperative code are not visible to the rest of
the program until the block has finished. In other words, from the perspective of
reactive observers, the imperative block appears to have executed instantly. This is great
for preventing glitches (temporarily incorrect values) that may cause unwanted side-effects.

By modelling reactive code around time-varying state, we can eliminate a great deal of
accidental state that would be introduced when using event based models.

Tempo also explicitly models the lifetime of code and data. Reactive code is modelled as
logically continuous; for example, a reactive expression might bind the contents of a UI
widget to always be equal to a corresponding value in the model. The lifetime of this
expression may be bound to the duration the view is visible, so when the view is shown,
the expression is activated, and when the view is hidden it is deactivated. For this
duration, the expression is **logically** continuously active.

Finally, the concept of **resource brokers** combines time-varying state and lifetime
management to provide a way of exposing access to resources without any accidental state.

## Usage

The Tempo solution contains three projects:

* Tempo: This is the core library. Using the core library only requires implementing
IScheduler for the platform it is used on, so that Tempo's main thread can cooperate with
the platform's UI thread.
* TempoTest: A set of unit tests for the core library.
* Tempo.Wpf: Provides integration for building WPF apps with Tempo.

The Examples directory contains some simple WPF projects demonstrating key features
of the library.

To get started building WPF applications with Tempo, see the main documentation page
(details below).

## Documentation

## Licensing

Please see the license.txt file.

