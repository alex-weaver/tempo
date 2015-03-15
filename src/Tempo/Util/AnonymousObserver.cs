using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tempo.Util
{
    /// <summary>
    /// Represents an implementation of IObserver, given an Action for each method.
    /// </summary>
    /// <typeparam name="T">The type of items in the observable sequence.</typeparam>
    public class AnonymousObserver<T> : IObserver<T>
    {
        private Action onCompleted;
        private Action<Exception> onError;
        private Action<T> onNext;


        /// <summary>
        /// Constructs a new observer.
        /// </summary>
        /// <param name="onCompleted">The handler for calls to OnCompleted.</param>
        /// <param name="onError">The handler for calls to OnError.</param>
        /// <param name="onNext">The handler for calls to OnNext.</param>
        public AnonymousObserver(Action onCompleted, Action<Exception> onError, Action<T> onNext)
        {
            this.onCompleted = onCompleted;
            this.onError = onError;
            this.onNext = onNext;
        }

        public void OnCompleted()
        {
            if (onCompleted != null)
            {
                onCompleted();
            }
        }

        public void OnError(Exception error)
        {
            if (onError != null)
            {
                onError(error);
            }
        }

        public void OnNext(T value)
        {
            if (onNext != null)
            {
                onNext(value);
            }
        }
    }
}
