// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel;
using System.Threading.Tasks;
using System.Web.Internal;

namespace System.Web
{
    public abstract class HttpTaskAsyncHandler : IHttpAsyncHandler
    {
        protected HttpTaskAsyncHandler()
        {
        }

        public virtual bool IsReusable => false;

        public abstract Task ProcessRequestAsync(HttpContext context);

        public IAsyncResult BeginProcessRequest(HttpContext context, AsyncCallback callback, object extraData)
            => AsyncTaskHelper.BeginTask(static state => state.Item1.ProcessRequestAsync(state.context), callback, extraData, (this, context));

        public void EndProcessRequest(IAsyncResult result)
            => AsyncTaskHelper.EndTask(result);

        [EditorBrowsable(EditorBrowsableState.Never)]
        public virtual void ProcessRequest(HttpContext context)
        {
            throw new NotSupportedException($"IHttpHandler {GetType()} cannot be executed synchronously");
        }
    }
}
