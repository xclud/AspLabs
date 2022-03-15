// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Threading;
using System.Threading.Tasks;

namespace System.Web.Internal
{
    internal static class AsyncTaskHelper
    {
        internal static IAsyncResult BeginTask<T>(Func<T, Task> taskFunc, AsyncCallback callback, object state, T funcState)
        {
            var task = taskFunc(funcState);

            // We need to wrap the inner Task so that the IAsyncResult exposed by this method
            // has the state object that was provided as a parameter. We could be a bit smarter
            // about this to save an allocation if the state objects are equal, but that's a
            // micro-optimization.
            var resultToReturn = new TaskWrapperAsyncResult(task, state);

            // Task instances are always marked CompletedSynchronously = false, even if the
            // operation completed synchronously. We should detect this and modify the IAsyncResult
            // we pass back to our caller as appropriate. Only read the 'IsCompleted' property once
            // to avoid a race condition where the underlying Task completes during this method.
            bool actuallyCompletedSynchronously = task.IsCompleted;
            if (actuallyCompletedSynchronously)
            {
                resultToReturn.ForceCompletedSynchronously();
            }

            if (callback != null)
            {
                // ContinueWith() is a bit slow: it captures execution context and hops threads. We should
                // avoid calling it and just invoke the callback directly if the underlying Task is
                // already completed. Only use ContinueWith as a fallback. There's technically a race here
                // in that the Task may have completed between the check above and the call to
                // ContinueWith below, but ContinueWith will do the right thing in both cases.
                if (actuallyCompletedSynchronously)
                {
                    callback(resultToReturn);
                }
                else
                {
                    task.ContinueWith(_ => callback(resultToReturn));
                }
            }

            return resultToReturn;
        }

        // The parameter is named 'ar' since it matches the parameter name on the EndEventHandler delegate type,
        // and we expect that most consumers will end up invoking this method via an instance of that delegate.
        internal static void EndTask(IAsyncResult ar)
        {
            if (ar == null)
            {
                throw new ArgumentNullException(nameof(ar));
            }

            // Make sure the incoming parameter is actually the correct type.
            if (ar is not TaskWrapperAsyncResult taskWrapper)
            {
                throw new ArgumentException("Parameter not expected type");
            }

            // The End* method doesn't actually perform any actual work, but we do need to maintain two invariants:
            // 1. Make sure the underlying Task actually *is* complete.
            // 2. If the Task encountered an exception, observe it here.
            // (TaskAwaiter.GetResult() handles both of those, and it rethrows the original exception rather than an AggregateException.)
            taskWrapper.Task.GetAwaiter().GetResult();
        }

        internal sealed class TaskWrapperAsyncResult : IAsyncResult
        {
            private bool _forceCompletedSynchronously;

            internal TaskWrapperAsyncResult(Task task, object asyncState)
            {
                Task = task;
                AsyncState = asyncState;
            }

            public object AsyncState { get; }

            internal Task Task { get; }

            public WaitHandle AsyncWaitHandle => ((IAsyncResult)Task).AsyncWaitHandle;

            public bool CompletedSynchronously => _forceCompletedSynchronously || ((IAsyncResult)Task).CompletedSynchronously;

            public bool IsCompleted => ((IAsyncResult)Task).IsCompleted;

            internal void ForceCompletedSynchronously() => _forceCompletedSynchronously = true;
        }
    }
}
