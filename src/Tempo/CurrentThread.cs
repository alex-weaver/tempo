using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Tempo.Scheduling;
using TwistedOak.Util;

namespace Tempo
{
    public static class CurrentThread
    {
        private static ThreadLocal<TaskQueue> taskQueue = new ThreadLocal<TaskQueue>();
        private static ThreadLocal<TemporalScope?> currentScope = new ThreadLocal<TemporalScope?>();
        private static ThreadLocal<bool> scopeIsContinuous = new ThreadLocal<bool>();


        public static TaskQueue Init(IScheduler scheduler, Lifetime topLevelLifetime, Action topLevelConstructor)
        {
            if(taskQueue.Value != null)
            {
                throw new InvalidOperationException("The top-level scope has already been initialized on this thread.");
            }

            taskQueue.Value = new TaskQueue(scheduler);
            currentScope.Value = new TemporalScope(topLevelLifetime, taskQueue.Value);
            scopeIsContinuous.Value = true;
            topLevelConstructor();
            currentScope.Value = null;

            topLevelLifetime.WhenDead(Shutdown);

            return taskQueue.Value;
        }

        private static void Shutdown()
        {
            taskQueue.Value = null;
            currentScope.Value = null;
        }

        public static void ResetThread()
        {
            Shutdown();
        }

        

        public static void ConstructScope(TemporalScope parentScope, Lifetime innerLifetime, Action constructor)
        {
            ConstructScope<bool>(parentScope, innerLifetime, () =>
                {
                    constructor();
                    return false;
                });
        }

        public static TResult ConstructScope<TResult>(TemporalScope parentScope, Lifetime innerLifetime, Func<TResult> constructor)
        {
            var innerScope = new TemporalScope(innerLifetime, parentScope.taskQueue);
            
            var callingScope = currentScope.Value;
            var callingIsContinuous = scopeIsContinuous.Value;

            currentScope.Value = innerScope;
            scopeIsContinuous.Value = true;
            var result = constructor();

            currentScope.Value = callingScope;
            scopeIsContinuous.Value = callingIsContinuous;

            return result;
        }

        public static void RunSequentialBlock(Action handler)
        {
            if (!scopeIsContinuous.Value)
            {
                handler();
            }
            else
            {
                var callingScope = currentScope.Value;
                var callingIsContinuous = scopeIsContinuous.Value;

                var innerLifetimeSrc = new LifetimeSource();
                currentScope.Value = new TemporalScope(innerLifetimeSrc.Lifetime, taskQueue.Value);
                scopeIsContinuous.Value = false;

                handler();
                innerLifetimeSrc.EndLifetime();

                scopeIsContinuous.Value = callingIsContinuous;
                currentScope.Value = callingScope;
            }
        }



        public static TemporalScope AnyCurrentScope()
        {
            if (currentScope.Value.HasValue)
            {
                return currentScope.Value.Value;
            }
            else
            {
                throw new InvalidOperationException("There are no active sequential or continuous blocks");
            }
        }

        public static TemporalScope CurrentContinuousScope()
        {
            if (taskQueue.Value == null)
            {
                throw new InvalidOperationException("The top-level scope has not been initialized on this thread. Call ThreadScope.Init before calling CurrentScope");
            }

            if (!scopeIsContinuous.Value)
            {
                throw new InvalidOperationException("There is no currently active continuous scope. CurrentScope cannot be called from a sequential block.");
            }

            return currentScope.Value.Value;
        }

        public static TemporalScope CurrentSequentialScope()
        {
            if (taskQueue.Value == null)
            {
                throw new InvalidOperationException("The top-level scope has not been initialized on this thread. Call ThreadScope.Init before calling CurrentScope");
            }

            if (scopeIsContinuous.Value)
            {
                throw new InvalidOperationException("The caller expected to be run in a sequential block - instead it has been called from a continuous block constructor");
            }

            return currentScope.Value.Value;
        }
    }
}
