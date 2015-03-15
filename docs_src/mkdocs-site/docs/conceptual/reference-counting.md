Reference Counting
==================

Tempo allows the use of reference counting for tracking objects that require manual disposal.
Use of reference counting is entirely optional, and in many programs most, if not all, objects will be
reclaimed by the garbage collector. However, sometimes it is useful for objects to be manually
disposed, especially if they are large and have a high rate of turnover. In cases where reference
counting is most appropriate, Tempo provides pervasive support for automatically tracking the
reference count. If a value implementing IRefCounted is stored by a cell, the cell object
will retain a reference, and release it when the value is no longer stored in the cell. In turn,
the lifetime of cells is normally tracked by the context in which they are declared, but their
lifetime may be extended by storing them inside another cell with a longer lifetime. Most of
the time, an application using reference counted objects will not need to manually alter the
reference count at all - the lifetime of the object can usually be automatically tracked
by Tempo cells and contexts.

All cells implement IRefCounted, so that references can be tracked if cells are stored in
other cells (for example, a list cell containing MemoryCell objects). For this reason, data
structures that contain cells should themselves implement IRefCounted, and simply forward calls
to AddRef and Release on to each of the individual cells.

When implementing an object that requires manual disposal, Tempo provides a base class, RefCountedSafe,
which abstracts the details of counting references, and only requires implementing a Destroy() method.
The RefCountedSafe makes calls to AddRef and Release thread safe.

