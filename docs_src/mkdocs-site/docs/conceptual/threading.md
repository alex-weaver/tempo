# Threading

Scheduling work to be performed on another thread is straightforward in Tempo. The *Thread* operator
must be used from a continuous block, and it is passed an inner continuous block which is executed
on a new thread. A short pseudocode example:

```
main()
{
   ...
   thread
   {
        every(TimeSpan.FromSeconds(60))
        {
            PerformSlowOperation();
        }
   }
}
```

In this example, some slow work is performed in a background thread every 60 seconds, but does
not interrupt the main thread. It is common to need a thread nested within a whilst operator,
so Tempo provides the *WhilstThread* operator which combines these into one statement.

## Synchronization

By default, Tempo assumes that all work is thread local. This reflects the library's approach
to scaling: Use isolation where possible, and synchronize where necessary. In making isolation
the default, Tempo can schedule and execute thread local work efficiently. Eventually some
communication between threads will be necessary, and only at this point is synchronization used.

In Tempo, threads communicate via shared cells. This means there are two places where synchronization
must be considered: when a shared cell is observed by a continuous block, and when the contents of a shared
cell is updated by an imperative block. The class `Shared<T>` is provided to wrap cells that are to be
shared across threads.

In order to safely update a shared cell, it must first be locked. Tempo provides the *Lock* operator
so that a thread can ensure exclusive access to all shared cells involved in an update. If any of the
cells are currently locked elsewhere, the body of the lock operator will be put in a queue until the
cells become available. This means that in Tempo, lock contention does not block a thread - instead
only the block that requires the lock will wait and the thread will continue to schedule other work
until it is possible to lock all the required cells. The lock operator should not be used from within
another lock operator. Instead, the outermost lock operator should lock all the required data. Following
this rule means that deadlock cannot occur, although it is still possible for a long-running operation
to starve other threads of access to locked cells.

