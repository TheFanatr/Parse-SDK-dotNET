using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;

namespace Parse.Common.Internal
{
    public static class InternalExtensions
    {
        public static Task<T> Safe<T>(this Task<T> task) => task ?? Task.FromResult<T>(default(T));

        public static Task Safe(this Task task) => task ?? Task.FromResult<object>(null);

        public delegate void PartialAccessor<T>(ref T arg);

        public static TValue GetOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> self, TKey key, TValue defaultValue) => self.TryGetValue(key, out TValue value) ? value : defaultValue;

        public static bool CollectionsEqual<T>(this IEnumerable<T> a, IEnumerable<T> b) => Equals(a, b) || a != null && b != null && a.SequenceEqual(b);

        public static Task<TResult> OnSuccess<TIn, TResult>(this Task<TIn> task, Func<Task<TIn>, TResult> continuation) => ((Task) task).OnSuccess(t => continuation((Task<TIn>) t));

        public static Task OnSuccess<TIn>(this Task<TIn> task, Action<Task<TIn>> continuation) => task.OnSuccess((Func<Task<TIn>, object>) (t =>
        {
            continuation(t);
            return null;
        }));

        public static Task<TResult> OnSuccess<TResult>(this Task task, Func<Task, TResult> continuation) => task.ContinueWith(t =>
        {
            if (t.IsFaulted)
            {
                AggregateException ex = t.Exception.Flatten();

                if (ex.InnerExceptions.Count == 1)
                    ExceptionDispatchInfo.Capture(ex.InnerExceptions[0]).Throw();
                else
                    ExceptionDispatchInfo.Capture(ex).Throw();

                return Task.FromResult(default(TResult));
            }
            else
                return t.IsCanceled ? Task.FromCanceled<TResult>(CancellationToken.None) : Task.FromResult(continuation(t));
        }).Unwrap();

        public static Task OnSuccess(this Task task, Action<Task> continuation) => task.OnSuccess((Func<Task, object>) (t =>
        {
            continuation(t);
            return null;
        }));

        public static Task WhileAsync(Func<Task<bool>> predicate, Func<Task> body)
        {
            return Iterate();

            Task Iterate() => predicate.Invoke().OnSuccess(t => !t.Result ? Task.FromResult(0) : body().OnSuccess(_ => Iterate()).Unwrap()).Unwrap();
        }
    }
}
