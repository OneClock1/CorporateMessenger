using Realtime.Domain.DTOs;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Realtime.Domain.Abstractions
{
    public interface IChatRoom
    {
        long ChatId { get; }

        int MemberCount { get; }

        void Add(IWebSocketConnection webSocketConnection);

        void Remove(Guid webSocketConnectionId);

        void SendMessage(string message);
        Task SendMessageAsync(string message);
    }
}
