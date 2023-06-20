using Common.Domain.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Realtime.Domain.Abstractions;
using Realtime.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;

namespace Realtime.Infrastructure.Implementation.Middlewares
{
    internal class WebSocketConnectionsMiddleware
    {
        private readonly IWebSocketConnectionService _connectionsService;
        private readonly ILogger<WebSocketConnectionsMiddleware> _logger;
        private readonly RequestDelegate _next;


        public WebSocketConnectionsMiddleware(RequestDelegate next, IWebSocketConnectionService connectionsService, ILogger<WebSocketConnectionsMiddleware> logger)
        {
            _connectionsService = connectionsService;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            if (context.WebSockets.IsWebSocketRequest)
            {

                if (Guid.TryParse(context.Request.Query["connectionId"], out Guid connectionId))
                {
                    if (_connectionsService.IsValidConnectionId(connectionId))
                    {
                        WebSocket webSocket = await context.WebSockets.AcceptWebSocketAsync();
                        await _connectionsService.CreateWebSocketConnectionAsync(webSocket, connectionId);
                    }
                    else
                    {
                        _logger.LogWarning($"ErrorCode: \"{ErrorCode.InvalidConnectionId}\" - Invalid Connection Id: \"{connectionId}\"");
                    }
                }
                else
                {
                    _logger.LogWarning($"ErrorCode: \"{ErrorCode.ConnectionIdNotFound}\" - Not found Connection Id: \"{connectionId}\"");

                }
            }
        }
    }
}
