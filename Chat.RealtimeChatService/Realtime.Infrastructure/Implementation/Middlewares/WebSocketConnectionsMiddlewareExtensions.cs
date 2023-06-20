using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Realtime.Domain.Abstractions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Realtime.Infrastructure.Implementation.Middlewares
{
    public static class WebSocketConnectionsMiddlewareExtensions
    {
        public static IApplicationBuilder MapWebSocketConnections(this IApplicationBuilder app,
                                                                  PathString pathMatch)
        {
            var applicationBuilder = app.Map(pathMatch, branchedApp => branchedApp.UseMiddleware<WebSocketConnectionsMiddleware>());
            applicationBuilder.ApplicationServices.GetService(typeof(IKeepAliveService));
            return app;
        }
    }
}
