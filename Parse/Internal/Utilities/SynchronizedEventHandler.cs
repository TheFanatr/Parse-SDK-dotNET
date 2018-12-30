using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Parse.Common.Internal
{
    public class SynchronizedEventHandler<T>
    {
        LinkedList<(Delegate, TaskFactory)> delegates = new LinkedList<(Delegate, TaskFactory)> { };

        public void Add(Delegate del)
        {
            lock (delegates)
            {
                TaskFactory factory = SynchronizationContext.Current != null ? new TaskFactory(CancellationToken.None, TaskCreationOptions.None, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.FromCurrentSynchronizationContext())     : Task.Factory;

                foreach (Delegate d in del.GetInvocationList())
                    delegates.AddLast((d, factory));
            }
        }

        public void Remove(Delegate del)
        {
            lock (delegates)
            {
                if (delegates.Count == 0)
                    return;

                foreach (Delegate d in del.GetInvocationList())
                {
                    LinkedListNode<(Delegate, TaskFactory)> node = delegates.First;
                    while (node != null)
                    {
                        if (node.Value.Item1 == d)
                        {
                            delegates.Remove(node);
                            break;
                        }
                        node = node.Next;
                    }
                }
            }
        }

        public Task Invoke(object sender, T args)
        {
            IEnumerable<(Delegate, TaskFactory)> toInvoke;
            Task<int>[] toContinue = new[] { Task.FromResult(0) };

            lock (delegates)
                toInvoke = delegates.ToList();

            return Task.WhenAll(toInvoke.Select(p => p.Item2.ContinueWhenAll(toContinue, _ => p.Item1.DynamicInvoke(sender, args))).ToList());
        }
    }
}
