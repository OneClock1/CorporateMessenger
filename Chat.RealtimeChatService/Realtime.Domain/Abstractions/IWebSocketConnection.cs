using Common.Domain;
using Common.Domain.DTOs.MessageDTOs;
using Realtime.Domain.DTOs;
using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Realtime.Domain.Abstractions
{
    public interface IWebSocketConnection
    {
        Guid Id { get; set; }
        string UserId { get; set; }
        string SessionId { get; set; }
        WebSocketCloseStatus? CloseStatus { get; set; }
        string CloseStatusDescription { get; }

        event EventHandler<string> ReceiveText;

        event EventHandler<IWebSocketConnection> OnClose;

        Task SendAsync(byte[] message, CancellationToken cancellationToken);
        Task SendAsync(string message, CancellationToken cancellationToken);
        void Send(object sender, EventArgs<ViewMessageDTO> args);
        Task ReceiveMessagesUntilCloseAsync();
    }
}
