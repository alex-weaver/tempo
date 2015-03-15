using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TwistedOak.Util;

namespace Tempo.Util
{
    public class MessageRelay<T>
    {
        private HashSet<Action<T>> handlers = new HashSet<Action<T>>();
        private bool copyOnWrite = false;


        public MessageRelay()
        {

        }

        public void AddHandler(Lifetime lifetime, Action<T> handler)
        {
            if(handlers.Contains(handler))
            {
                throw new InvalidOperationException("handler has already been added to MessageRelay");
            }

            CopyIfNeeded();
            handlers.Add(handler);

            lifetime.WhenDead(() =>
                {
                    CopyIfNeeded();
                    handlers.Remove(handler);
                });
        }

        private void CopyIfNeeded()
        {
            if(copyOnWrite)
            {
                handlers = new HashSet<Action<T>>(handlers);
            }
        }

        public void Broadcast(T value)
        {
            copyOnWrite = true;
            foreach(var handler in handlers)
            {
                handler(value);
            }
            copyOnWrite = false;
        }
    }
}
