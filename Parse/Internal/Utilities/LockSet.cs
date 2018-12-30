using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Parse.Common.Internal
{
    public class LockSet
    {
        static ConditionalWeakTable<object, IComparable> StableIdentifiers = new ConditionalWeakTable<object, IComparable> { };

        static long nextStableId { get; set; } = 0;

        IEnumerable<object> Mutexes { get; }

        public LockSet(IEnumerable<object> mutexes) => Mutexes = (from mutex in mutexes orderby GetStableId(mutex) select mutex).ToList();

        public void Enter()
        {
            foreach (object mutex in Mutexes)
                Monitor.Enter(mutex);
        }

        public void Exit()
        {
            foreach (object mutex in Mutexes)
                Monitor.Exit(mutex);
        }

        private static IComparable GetStableId(object mutex)
        {
            lock (StableIdentifiers)
                return StableIdentifiers.GetValue(mutex, k => nextStableId++);
        }
    }
}
