# Cells

Tempo implements two types of cells: `MemoryCell<T>` and `ListCell<T>`. `MemoryCell<T>` stores a single
value of type T. `ListCell<T>` represents a list of values of type T. Both cells can notify observers
when their contents change. The `ListCell<T>` can also notify observers of exactly what has changed,
to minimize the amount of recomputation observers need to perform.


## MemoryCell

`MemoryCell<T>` implements two interfaces: `ICellRead<T>` and `ICellWrite<T>`. This separates the
authority to read from a cell from the authority to write to the cell. This means that since
the *Whilst* operator accepts an argument of type `ICellRead<bool>`, we can pass a memory cell but
be guaranteed that the value of the cell will not be modified.

The CellBuilder class provides some methods for constructing read-only cells.
CellBuilder.Const constructs a cell that only ever has a single value for the duration of its
lifetime. This can be useful where a method requires an `ICellRead<T>` argument, but the caller
knows the value cannot change. The Cellbuilder.Unit method constructs a constant cell which always
holds the unit value. This is useful for implementing functions that must return an `ICellRead<T>`
but the implementation does not need to return any information.

It can often be useful to generate a derived value by combining a number of values. The
CellBuilder.Merge methods can construct a read-only cell which is derived from the value
of two or more other cells.

`ICellRead<T>` also has some extension methods. MemoryCellExtentions.Select applies a transformation
to a MemoryCell value. Use Select instead of WithEach when the transformation does not need to
allocate any resources, and can be simply derived from its argument.

MemoryCellExtensions.Flatten flattens a nested `ICellRead<ICellRead<T>>` into a single `ICellRead<T>`
value which contains the concatenation of all of the innermost values.

## ListCell

As with MemoryCell, ListCell implements separate interfaces for reading and writing (`IListCellRead<T>`
and `IListCellWrite<T>`). The ListCell represents a time-varying list value. Any time the list is
modified, the cell notifies listeners of exactly what has changed.

There are some extension methods to ListCell for working with time-varying lists.

Select applies a transformation to each element of the list. Similar to Select for `ICellRead<T>`,
use this method if there is no need for a temporal scope in the transformation, and it is a simple
derivation of the list element.

FirstOrDefault returns a read-only memory cell which is always equal to the first element of the list.
If the list changes, the returned cell updates to reflect the change.

ElementAt combines a time-varying list with a time-varying integer index, the return value continuously
updates to reflect the value at the index.

Flatten turns a list of observable values into a plain list of values. If one of the observable values
in the source list changes, that is modelled in the output list as though that value was replaced by another.
For example, if the source list is [cell{1}, cell{2}, cell{3}] then the output list will initially be
[1, 2, 3]. If the second value of the source list changes to 4, so the source list is [cell{1}, cell{4}, cell{3}],
then the output list will be [1, 4, 3]


