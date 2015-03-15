# Basic operators

Most of the methods for handling core functionality in Tempo are located in one of the following places:

## ICellRead extension methods

The `Whilst` and `WhilstTrue` methods provide a way for the activity of a continuous block to be divided into smaller chunks.
Both methods activate inner continuous block as long as an observed boolean cell holds a true value.

The `When` and `WhenTrue` methods trigger an inner sequential block when an observed boolean cell transitions
from false to true.

The `WithLatest` method activates an inner continuous block for each value of a cell. This is useful for activating a different
set of continuous expressions for different states (for example, for different screens of an application).

Both `Latest` and `Changes` trigger a sequential block to run each time an `ICellRead<T>` changes.
The difference is that sequential block in `Latest` observes only the most recent value of the cell,
whereas in `Changes` the sequential block is passed all the values that the cell has held since the block
was last invoked.

The `Bind` method updates the contents of another cell whenever this cell changes.

## IListCellRead extension methods

The `WithEach` method activates an inner continuous block for each element of a list (in a similar way to the
WithLatest method of ICellRead).

The `Bind` method updates the contents of another cell whenever this cell changes.

## Events static methods

The `Once` method executes a sequential block once each time the containing continuous scope is activated.
`WhenEnded` is this method's counterpart: It executes a sequential block once each time the containing socpe
is deactivated.

The `Listen`, `Subscribe` and `SubscribeValue` methods are provided for integrating with .NET events and
IObservable instances. You can find more detail on this in [Integrating Events](../integration/integrating-events.md).

The `AnyChange` method triggers a sequential block whenever any of a set of cells change.

## Timing static methods

The `After` method triggers a sequential block to run once after a certain time interval has passed after each time
the containing scope is activated.

The `Every` method repeatedly runs a sequential block at a given interval.

