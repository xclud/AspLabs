// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace System.Web.Adapters;

internal class HttpHandlerMiddleware<T> : IMiddleware
    where T : IHttpHandler
{
    private T? _reuseable;

    public async Task InvokeAsync(HttpContextCore context, RequestDelegate next)
    {
        var handler = GetHandler(context.RequestServices);

        if (await RunHandlerAsync(handler, context))
        {
            await next(context);
        }
    }

    private T GetHandler(IServiceProvider services)
    {
        if (_reuseable is not null)
        {
            return _reuseable;
        }

        var handler = ActivatorUtilities.CreateInstance<T>(services);

        if (handler.IsReusable)
        {
            _reuseable = handler;
        }

        return handler;
    }

    /// <summary>
    /// Runs the handler based on the implementation that it has.
    /// </summary>
    /// <returns>Whether the middleware should call the next or continue.</returns>
    private async ValueTask<bool> RunHandlerAsync(T handler, HttpContext context)
    {
        if (handler is HttpTaskAsyncHandler taskHandler)
        {
            await taskHandler.ProcessRequestAsync(context);
        }
        else if (handler is IHttpAsyncHandler asyncHandler)
        {
            await Task.Factory.FromAsync(asyncHandler.BeginProcessRequest(context, null, null), asyncHandler.EndProcessRequest);
        }
        else
        {
            handler.ProcessRequest(context);
        }

        return !context.Response.IsEnded;
    }
}
