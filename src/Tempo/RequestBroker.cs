using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tempo
{
    /// <summary>
    /// RequestBroker keeps a list of all concurrent requests made via continuous calls to ApplyRequest or ApplyConstRequest,
    /// and each time the list changes, a single aggregate of the list is computed. This is useful for sharing access to an
    /// exclusive resource.
    /// </summary>
    /// <typeparam name="TRequest">The type of the request objects.</typeparam>
    /// <typeparam name="TAggregate">The type of the resulting aggregate object.</typeparam>
    public class RequestBroker<TRequest, TAggregate>
    {
        private ListCell<ICellRead<TRequest>> requests = new ListCell<ICellRead<TRequest>>();
        private ICellRead<TAggregate> results;

        /// <summary>
        /// Gets a read-only cell which always contains the current aggregate value.
        /// </summary>
        public ICellRead<TAggregate> Aggregate { get { return results; } }



        /// <summary>
        /// Constructs a new request broker. A default aggregate is required for the case when there are no requests.
        /// The second argument is the aggregator function. This is called every time the list of concurrent requests changes,
        /// and the return value becomes the new aggregate value.
        /// </summary>
        /// <param name="defaultAggregate">The default aggregate. The Aggregate property has this value when there are no requests.</param>
        /// <param name="aggregator">The aggregator function.</param>
        public RequestBroker(TAggregate defaultAggregate, Func<IEnumerable<TRequest>, TAggregate> aggregator)
        {
            if (aggregator == null) throw new ArgumentNullException("aggregator");

            results = requests
                .Flatten()
                .Aggregate(allRequests =>
                {
                    if (!allRequests.Any())
                    {
                        return CellBuilder.Const(defaultAggregate);
                    }

                    return CellBuilder.Const(aggregator(allRequests));
                })
            .Flatten()
            .DistinctUntilChanged();
        }

        /// <summary>
        /// Apply a request with a constant value. This must be called from a continuous scope.
        /// </summary>
        /// <param name="request">The value of the request.</param>
        public void ApplyConstRequest(TRequest request)
        {
            ApplyRequest(CellBuilder.Const(request));
        }


        /// <summary>
        /// Apply a variable request. This must be called from a continuous scope.
        /// </summary>
        /// <param name="request">A cell representing the variable request.</param>
        public void ApplyRequest(ICellRead<TRequest> request)
        {
            var callingScope = CurrentThread.CurrentContinuousScope();

            requests.Add(request);

            callingScope.lifetime.WhenDead(() =>
            {
                callingScope.ScheduleSequentialBlock(() =>
                {
                    requests.Remove(request);
                });
            });
        }
    }
}
