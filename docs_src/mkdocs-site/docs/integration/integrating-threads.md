# Integrating With Threads

This section covers integrating threads not managed by Tempo. This includes background
worker threads, TPL tasks and callbacks from asynchronous methods (such as
HttpWebRequest.BeginGetResponse).

The general pattern for handling these cases is: When the thread work is complete,
schedule an imperative action on the Tempo thread that started the work. To be able
to schedule tasks on a Tempo thread, you need to keep a reference to the scope that
initiated the work. Calling CurrentThread.AnyCurrentScope() will return a TemporalScope
object representing the current scope, whether it is a continuous or sequential scope.
Later calling TemporalScope.ScheduleSequentialBlock will schedule a task on the original Tempo thread.

## WebRequest Example

As an example, say we want to create a method which will asynchronously start a
web request, and invoke a sequential block when the response is available in its entirety.
For this example, we will request the html for the home page of Microsoft's Contoso website,
represented as a single string value.

The full code looks like this:

```
using System.Net;

...

public static void WebRequestExample(Action<string> handler)
{
    var callingScope = CurrentThread.AnyCurrentScope();

    var request = WebRequest.Create("http://www.contoso.com/default.html");
    request.BeginGetResponse(result =>
        {
            var response = request.EndGetResponse(result);
            var stream = response.GetResponseStream();
            using (var reader = new System.IO.StreamReader(stream))
            {
                var content = reader.ReadToEnd();

                callingScope.ScheduleSequentialBlock(() => handler(content));
            }

            response.Close();
        }, null);
}
```

The single argument to this method will be invoked as a sequential block with the html
as it's argument. The only two lines which differ from ordinary use of WebRequest are
the initialization of *callingScope* and the call to ScheduleSequentialBlock. The initialization
of *callingScope* stores the scope in which the method is called, which includes the
current thread's scheduler. For this reason, the WebRequestExample method can only be
called from a valid Tempo scope. The body of BeginGetResponse runs asynchronously, so
in order to get back to a Tempo managed scope, it calls ScheduleSequentialBlock to invoke the
handler.

This same pattern applies to any asynchronous work which is not managed by Tempo. The
Flickr sample in the Examples directory shows how the above technique can be used in
practice.


