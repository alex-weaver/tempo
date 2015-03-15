using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TwistedOak.Util;

namespace Tempo
{
    /// <summary>
    /// The interface that all cell types must implement.
    /// </summary>
    public interface ICell
    {
        /// <summary>
        /// Subscribe to notifications that the cell has changed. The listener is unsubscribed when the lifetime ends.
        /// </summary>
        /// <param name="lifetime">The lifetime of the subscription.</param>
        /// <param name="handler">The action to invoke when a change occurs.</param>
        void ListenForChanges(Lifetime lifetime, Action handler);
    }
}
