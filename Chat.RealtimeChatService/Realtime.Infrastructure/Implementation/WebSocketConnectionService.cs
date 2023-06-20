using Common.Domain.Abstractions;
using Common.Domain.Enums;
using Common.Implementations.ExceptionImplementations.Exceptions;
using Common.Implementations.InnerHttpClient;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using NJsonSchema;
using Realtime.Domain.Abstractions;
using Realtime.Domain.DTOs;
using Realtime.Domain.Options;
using Realtime.Domain.Structures;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Realtime.Infrastructure.Implementation
{
    public class WebSocketConnectionService : IWebSocketConnectionService
    {
        private class Connection 
        {
            public Connection(IWebSocketConnection webSocketConnection, CancellationTokenSource cancellationTokenSource)
            {
                WebSocketConnection = webSocketConnection;
                CancellationTokenSource = cancellationTokenSource;
            }
            public IWebSocketConnection WebSocketConnection { get; }

            public CancellationTokenSource CancellationTokenSource { get; }
        } 
        public WebSocketConnectionService(INotificationService notificationService, IOptions<RealTimeOptions> options, ILogger<WebSocketConnectionService> logger, ChatHttpService chatHttpService)
        {
            _notificationService = notificationService;
            _realTimeOptions = options.Value;
            _logger = logger;
            _chatHttpService = chatHttpService;
        }

        private readonly INotificationService _notificationService;
        private readonly ILogger<WebSocketConnectionService> _logger;

        private readonly ChatHttpService _chatHttpService;

        private readonly RealTimeOptions _realTimeOptions;

        private readonly ConcurrentDictionary<Guid, UserAuthorizationInfo> pendingConnections = new ConcurrentDictionary<Guid, UserAuthorizationInfo>();
        private readonly ConcurrentDictionary<Guid, Connection> connections = new ConcurrentDictionary<Guid, Connection>();

        private readonly ConcurrentDictionary<long, IChatRoom> chats = new ConcurrentDictionary<long, IChatRoom>();

        public event EventHandler<IWebSocketConnection> Connect;
        public event EventHandler<IWebSocketConnection> Disconnect;

        public async Task CreateWebSocketConnectionAsync(WebSocket webSocket, Guid connectionId)
        {
            if (pendingConnections.TryRemove(connectionId, out UserAuthorizationInfo userAuthorizationInfo))
            {
                CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
                IWebSocketConnection webSocketConnection = new WebSocketConnection(webSocket, connectionId, _realTimeOptions.PayloadBufferSize, cancellationTokenSource.Token)
                {
                    UserId = userAuthorizationInfo.UserId,
                    SessionId = userAuthorizationInfo.SessionId
                };

                var connection = new Connection(webSocketConnection, cancellationTokenSource);

                if (connections.TryAdd(webSocketConnection.Id, connection))
                {
                    webSocketConnection.ReceiveText += ReciveText;
                    webSocketConnection.OnClose += WebSocketConnection_OnClose;
                    Connect?.Invoke(this, webSocketConnection);
                    try
                    {
                        await webSocketConnection.ReceiveMessagesUntilCloseAsync();
                    }
                    catch (WebSocketException ex)
                    {
                        _logger.LogError(ex,$"Error: {ex.ErrorCode} -  {ex.Message}. WebSocketConnection: {webSocketConnection.Id} had been close.", webSocketConnection);
                    }
                    catch (OperationCanceledException ex)
                    {
                        _logger.LogWarning(ex, $"Error: Check failed -  {ex.Message}. WebSocket: {webSocketConnection.Id} had been close.", webSocketConnection);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, $"Error: {ex.Message}. WebSocket: {webSocketConnection.Id} had been close.", webSocketConnection);
                    }
                }
                else
                {
                    await webSocketConnection.SendAsync($"WS {webSocketConnection.Id} NOT added, another copy of this connectionID detected", cancellationTokenSource.Token);
                    _logger.LogWarning($"WS {webSocketConnection.Id} NOT added, another copy of connectionID detected");
                }
            }
            else
            {
                _logger.LogError($"UserAuthorizationInfo {connectionId} did NOT remove ");
            }
        }

        private void WebSocketConnection_OnClose(object sender, IWebSocketConnection webSocketConnection)
        {
            if (connections.TryRemove(webSocketConnection.Id, out Connection connection))
            {
                Disconnect?.Invoke(this, webSocketConnection);
                _logger.LogInformation($"WebSocketConnection: \"{webSocketConnection.Id}\" was closed", connection);
            }
        }

        private async Task<bool> AddToChat(long chatId, IWebSocketConnection webSocketConnection)
        {
            if (!await _chatHttpService.IsExistChatAsync(chatId))
            {
                webSocketConnection.SendAsync($"WebSocketConnection: \"{webSocketConnection.Id}\" Chat {chatId} not found", CancellationToken.None);
                _logger.LogInformation($"WebSocketConnection: \"{webSocketConnection.Id}\" Chat {chatId} not found");
                return false;
            }
            if (!await _chatHttpService.IsAccessToChatAsync(chatId, webSocketConnection.UserId))
            {
                webSocketConnection.SendAsync($"WebSocketConnection: \"{webSocketConnection.Id}\" forbidden access to {chatId}", CancellationToken.None);
                _logger.LogInformation($"WebSocketConnection: \"{webSocketConnection.Id}\" forbidden access to {chatId}");
                return false;
            }

            if (chats.ContainsKey(chatId))
            {
                if (chats.TryGetValue(chatId, out IChatRoom chat))
                {
                    chat.Add(webSocketConnection);
                }
            }
            else
            {
                var chat = CreateChatRoom(chatId);
                chats.TryAdd(chatId, chat);
                chat.Add(webSocketConnection);
            }

            webSocketConnection.SendAsync($"WebSocketConnection: \"{webSocketConnection.Id}\" was added to {chatId}", CancellationToken.None);
            _logger.LogInformation($"WebSocketConnection: \"{webSocketConnection.Id}\" was added to {chatId}");

            return true;
        }

        private IChatRoom CreateChatRoom(long chatId) 
        {
            IChatRoom chatRoom = new ChatRoom(chatId);
            _notificationService.SubscribeToChat(chatId, chatRoom.SendMessage);

            _logger.LogInformation($"ChatRoom: \"{chatId}\" was created", chatRoom);
            return chatRoom;
        }

        private bool RemoveFromChat(long chatId, IWebSocketConnection webSocketConnection)
        {
            chats.TryGetValue(chatId, out IChatRoom chat);
            chat.Remove(webSocketConnection.Id);

            if(chat.MemberCount == 0)
            {
                chats.TryRemove(chatId, out IChatRoom removedChat);
                _notificationService.UnsubscribeFromChat(chatId);
                _logger.LogInformation($"ChatRoom: \"{chatId}\" was removed", removedChat);
            }
            webSocketConnection.SendAsync($"WebSocketConnection: \"{webSocketConnection.Id}\" was removed from {chatId}", CancellationToken.None);
            _logger.LogInformation($"WebSocketConnection: \"{webSocketConnection.Id}\" was removed from {chatId}", webSocketConnection);
            return true;
        }

        public IWebSocketConnection GetWebSocketConnectionById(Guid connectionId)
        {
            if (connections.TryGetValue(connectionId, out Connection connection))
            {
                return connection.WebSocketConnection;
            }
            else
            {
                _logger.LogDebug(new InvalidDataException(ErrorCode.NotFound) ,$"WebSocketConnection: \"{connectionId}\" had not found");
                throw new InvalidDataException(ErrorCode.NotFound);
            }

        }

        public bool IsValidConnectionId(Guid connectionId)
        {
            return pendingConnections.ContainsKey(connectionId);
        }

        public Guid RegisterUserId(string userId, string sessionId)
        {
            Guid connectionId = Guid.NewGuid();
            pendingConnections.TryAdd(connectionId, new UserAuthorizationInfo(userId, sessionId));
            _logger.LogDebug($"Connection Id: \"{connectionId}\" was registred");

            return connectionId;
        }

        public bool RemoveWebSocketConnection(Guid connectionId)
        {
            if(connections.TryRemove(connectionId, out Connection connection))
            {
                connection.WebSocketConnection.SendAsync($"WebSocketConnection: {connectionId} will have been close", CancellationToken.None);
                connection.CancellationTokenSource.Cancel();
                _logger.LogInformation($"WebSocketConnection: \"{connectionId}\" was closed", connection);

                return true;
            }
            else
            {
                _logger.LogError($"WebSocketConnection: \"{connectionId}\" wasn't closed", connection);
                return false;
            }

        }

        public async Task SendToAllAsync(string message, CancellationToken cancellationToken)
        {
            _logger.LogDebug($"Send to all connection. Mwssage:{message}");
            foreach (var connection in connections)
            {
                await connection.Value.WebSocketConnection.SendAsync(message, cancellationToken);
            }
        }

        private void ReciveText(object sender, string webSocketMessage)
        {
            var schema = JsonSchema.FromType<SubscribeSocketDTO>();
            var errors = schema.Validate(webSocketMessage).Count;
            if (errors == 0)
            {
                _logger.LogInformation($"WebSocketConnection: \"{(sender as WebSocketConnection).Id}\" recived the message\"{webSocketMessage}\"");
                ProcessActionMessage(sender as WebSocketConnection, DeserializeToObject<SubscribeSocketDTO>(webSocketMessage));
            }
        }

        private TObject DeserializeToObject<TObject>(string webSocketMessage)
        {
            TObject recivedMessage = JsonConvert.DeserializeObject<TObject>(webSocketMessage);
            return recivedMessage;
        }

        private void ProcessActionMessage(IWebSocketConnection webSocketConnection, SubscribeSocketDTO subscribeSocketDTO)
        {

            if (webSocketConnection == null)
            {
                _logger.LogError(new InvalidDataException(ErrorCode.InvalidConnectionId), $"WebSocketConnection is invalid", webSocketConnection);

            }

            if (subscribeSocketDTO == null)
            {
                _logger.LogError(new InvalidDataException(ErrorCode.Invalid), $"SubscribeSocketDTO is invalid", subscribeSocketDTO);
            }

            if (subscribeSocketDTO.SocketAction == Domain.Enums.TypeOfSocketAction.Join &&
                subscribeSocketDTO.TypeOfSubscribeObject == Domain.Enums.TypeOfSubscribeObject.Chat)
            {
                AddToChat(subscribeSocketDTO.ObjectId, webSocketConnection);
            }
            else 
            if(subscribeSocketDTO.SocketAction == Domain.Enums.TypeOfSocketAction.Leave &&
               subscribeSocketDTO.TypeOfSubscribeObject == Domain.Enums.TypeOfSubscribeObject.Chat) 
            {
                RemoveFromChat(subscribeSocketDTO.ObjectId, webSocketConnection);
            }
        }
    }
}
