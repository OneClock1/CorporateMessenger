using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Realtime.Domain.Abstractions
{
    public interface IWebSocketConnectionService
    {
        public event EventHandler<IWebSocketConnection> Disconnect;
        public event EventHandler<IWebSocketConnection> Connect;

        Guid RegisterUserId(string userId, string sessionId);

        bool IsValidConnectionId(Guid connectionId);

        Task CreateWebSocketConnectionAsync(WebSocket webSocket, Guid connectionId);

        bool RemoveWebSocketConnection(Guid connectionId);

        Task SendToAllAsync(string message, CancellationToken cancellationToken);

        IWebSocketConnection GetWebSocketConnectionById(Guid connectionId);
    }
}
