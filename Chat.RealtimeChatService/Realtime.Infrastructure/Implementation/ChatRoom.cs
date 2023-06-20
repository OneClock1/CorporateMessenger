using Common.Domain.Abstractions;
using Newtonsoft.Json;
using Realtime.Domain;
using Realtime.Domain.Abstractions;
using Realtime.Domain.DTOs;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Realtime.Infrastructure.Implementation
{
    public class ChatRoom : IChatRoom
    {
        public long ChatId { get; }

        private readonly ConcurrentDictionary<Guid, IWebSocketConnection> _connections = new ConcurrentDictionary<Guid, IWebSocketConnection>();

        public int MemberCount
        {
            get
            {
                return _connections.Count;
            }
        }


        public ChatRoom(long chatId)
        {
            ChatId = chatId;
        }

        public void Add(IWebSocketConnection webSocketConnection)
        {
            if (_connections.TryAdd(webSocketConnection.Id, webSocketConnection))
            {
                webSocketConnection.OnClose += WebSocketConnection_OnClose;
            }
            else
            {
                webSocketConnection.SendAsync($"WebSocketConnection: {webSocketConnection.Id} alredy have connection to Chat:{ChatId}", CancellationToken.None);
            }

        }

        private void WebSocketConnection_OnClose(object sender, IWebSocketConnection webSocketConnection)
        {
            Remove(webSocketConnection.Id);
        }

        public void Remove(Guid webSocketConnectionId)
        {
            _connections.TryRemove(webSocketConnectionId, out IWebSocketConnection removedWebSocketConnection);
        }

        public void SendMessage(string message)
        {
            Task.Run(SendMessageAsync(message).RunSynchronously);
        }

        public async Task SendMessageAsync(string message)
        {
            foreach (var connection in _connections)
            {
                await connection.Value.SendAsync(Encoding.UTF8.GetBytes(message), CancellationToken.None);
            }
        }
    }
}
