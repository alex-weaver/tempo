# Conceptual Overview

Tempo provides a programming model based on two kinds of code:

**sequential blocks** behave like regular C# methods; a sequence of statements are executed
one after the other. Each statement is only executed after the previous one has completed.

**continuous blocks** contain a number of continuous statements. A continuous block is activated
at some point in time, remains continuously active, and is deactivated at a later point in time.
While each expression is active it is enforcing some relationship, rule or constraint. The lifetime
of this continuous block is referred to as a *temporal scope*.

In Tempo, the top level of the program is a continuous block, which is activated when the program starts,
and is deactivated when the program exits. This temporal scope may be subdivided into smaller segments of
the program's running time with operators like *Whilst*. The *Whilst* operator activates its inner temporal
scope as long as a given boolean expression is true. This can be used for activating application logic when
it is applicable, and deactivating it when it would either lead to incorrect behaviour or consume unnecessary
resources. For example, an application may show a video feed from a camera on one screen, but on other screens
it can be deactivated. This would be modelled in the following way (pseudocode)

```
whilst(currentScreen == Screens.CameraScreen)
{
    ShowCameraPreview(window.previewArea)
}
```

The body of the *whilst* block completely encapsulates the allocation and freeing of resources that comes
with interacting with hardware
such as cameras. Extra conditions can easily be added to the whilst condition (eg. to disable the camera if the
window is minimized), and resources will never be double-allocated or double-freed.

A continuous block cannot execute sequential statements, and vice-versa. For example, it would be meaningless
to continously execute a discrete operation such as x += 1; if it were allowed, how many times would x be
incremented? In order for the two types of block to interact, special operators are provided by the runtime.
A continuous block can set up a rule defining when a sequential block should be triggered with operators
like *When* or *Every*. The *When* operator triggers a sequential block whenever a boolean expression
changes from being false to being true. The *Every* operator repeatedly triggers a sequential block at
a particular interval.

A sequential block cannot directly activate a continuous block, because there would be no way to define the
continuous block's lifetime. Instead, a sequential block can affect which continuous blocks are active by
modifying variables which are used in *whilst* conditions.

In this way an application's execution follows an interplay between continuous and sequential code. Continous
blocks provide the high level structure and decomposition of a program, but are unable to perform any imperative
operations. Conversely, the sequential blocks can modify the state of the program, which may change which
continuous blocks are active, but a sequential block cannot directly allocate resources because the lifetime
of the resource would not be known. The one exception to this is the ContinuousObject class; It is a reference
counted object, which means it's lifetime is known. This known lifetime is used to define the activity of a
continuous block.

To illustrate how continuous and sequential code interacts, the following is a short pseudocode program
which displays a text label initially showing 0, and a button. With each press of the button, the number
in the text label is incremented by 1. The function ShowWindow is assumed to set up these UI widgets.
Note that the top-level of a program in Tempo is a continuous block.

```
main()
{
    var window = ShowWindow();
    var count = new MemoryCell<int>(0);

    bind(count, window.label.content);

    when(window.button.IsPressed)
    {
        count.Cur = count.Cur + 1;
    }
}
```

The bind operator updates the label whenever count changes. The when operator triggers whenever
the button transitions from being not pressed to being pressed, causing an increment of count,
which triggers the binding to update the label. The body of the when block is the only sequential
code, the remainder is continuous. Note that in this example, a number of possible sources of bugs
are not present. There is no possibility of the UI state becoming out of sync with the model state,
because there is a single source of truth (the count cell), and data dependencies are made explicit
(the bind operator).


## State management

In order for state to be used in continuous code, it must be possible for continuous code to observe
when the state has changed. For this reason, Tempo provides 'cells', which represent time-varying
data structures. The most basic of these is the class `MemoryCell<T>`, which stores a value of type T.

`MemoryCell<T>` implements a separate interfaces for reading and writing, these are `ICellRead<T>` and
`ICellWrite<T>`. This allows the authority to read and observe a cell to be separated from the authority
to modify the value. This separation also allows the construction of read-only cells.

Consider the following simplified example: an API for reading from temperature sensors
might have the following signature:

    double GetReading(int sensorId)

This would allow client code to imperatively read the value of a sensor at any time. If instead you
wanted to observe the value of the sensor every time it provided new data, you may use a .NET event,
or the Reactive Extensions to provide an event-driven interface. Tempo proposes an alternative: in this
case we can extend the GetReading API function into the time domain:

    ICellRead<double> GetReading(ICellRead<int> sensorId)

Where `ICellRead<T>` represents a time-varying value of type T. Now when we call GetReading we are returned
a time-varying double which could be directly observed, or may be composed with other time-varying values
first. The only thing missing from the above signature is some way for the GetReading method to know when
to *stop* observing the sensor (including freeing resources). In Tempo, the duration of the observation
is given by the *temporal scope* in which the method was called. Since the top level of a Tempo program
is a continuous block that is active for the duration of the program, calling GetReading at this top
level would observe the sensor for the lifetime of the program. Alternatively it may be called from
a scope that has a much shorter lifetime, such as the body of a *Whilst* operator, or *WithEach* operator.

`ICellRead<T>` is different to `IObservable<T>` because `ICellRead<T>` always has a *current*
value, and can notify observers when that current value changes. In other words, `ICellRead<T>` represents
time-varying state whereas `IObservable<T>` represents events (changes to state).

