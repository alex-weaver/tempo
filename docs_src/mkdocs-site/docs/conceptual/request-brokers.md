# Request Brokers

Calling a function in a continuous scope is a convenient way to allocate resources; the lifetime of
the containing scope naturally defines the duration for which the resource is allocated. This means that
there is no need to imperatively allocate and free the resource, that will happen automatically when the
scope is activated and deactivated. Using this idea of a function call in a continuous scope simplifies
reasoning about when resources will be in use.

We can take this model further: say the resource is an exclusive resource - ie. there can only be at most
one concurrent activation of the resource. In this case we might want to share access among concurrent
clients. We do this by placing a broker in between the client and the resource: clients apply requests
to the broker and the broker aggregates all of the concurrent requests into a single request to be placed
on the resource itself. The RequestBroker class provides one way to handle this aggregation. Other methods
are possible, but RequestBroker should cover the majority of cases.

Requests are reified as objects so that they can be stored by a RequestBroker. The resulting aggregate is
also an object for the same reason. The RequestBroker keeps a list of all concurrent requests. Each time this collection is modified,
RequestBroker calls a function to calculate what the new aggregated value should be. This function is provided
via a constructor argument.

A request broker exposes two methods for applying a request: ApplyRequest and ApplyConstRequest. The latter
is a simple convenience method to avoid boilerplate when constructing a constant request. Requests must be
applied in a continuous context. A client of the resource broker must provide a time-varying request value,
and in return the broker provides a time-varying result value. There may be many concurrent requests
applied to the broker; this provides an opportunity for the broker to apply a policy to decide how
best to service the requests collectively.

For example, a broker representing a video camera resource may expect a request object which contains
a desired resolution, and return a time-varying image value. If there are a number of concurrent requests
for the same camera, the implementer of the broker has a choice of a number of different possible policies.
A simple rule could be to pick the highest resolution among all the concurrent requests, and then provide
all clients with a feed of that resolution. In this case, the client must be prepared to receive a feed
that has a different resolution to what was requested. Alternatively, the broker could model exclusive
access semantics, where only one request is selected to be serviced, and the others receive null values
until the selected request is removed or changed, after which another request is selected.

In the VideoCapture sample, a request broker is used in the CaptureDevice class to aggregate requests.
In this case, the broker itself remains private to the class, instead exposing a continuous method to
apply requests, which keeps the API for CaptureDevice simple.



