using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Parse.Common.Internal
{
    public class TaskQueue
    {
        Task Tail { get; set; }

        Task GetTaskToAwait(CancellationToken cancellationToken = default)
        {
            lock (Mutex)
            {
                Task toAwait = Tail ?? Task.FromResult(true);
                return toAwait.ContinueWith(task => { }, cancellationToken);
            }
        }

        public T Enqueue<T>(Func<Task, T> taskStart, CancellationToken cancellationToken = default) where T : Task
        {
            Task oldTail;
            T task;
            lock (Mutex)
                Tail = Task.WhenAll(oldTail = Tail ?? Task.FromResult(true), task = taskStart(GetTaskToAwait(cancellationToken)));

            return task;
        }

        public object Mutex { get; } = new object { };
    }
}
