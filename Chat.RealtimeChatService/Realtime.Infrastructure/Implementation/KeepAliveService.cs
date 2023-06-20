using Common.Domain.Enums;
using Common.Implementations.ExceptionImplementations.Exceptions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Realtime.Domain.Abstractions;
using Realtime.Domain.Options;
using System;
using System.Collections.Concurrent;
using System.Threading;

namespace Realtime.Infrastructure.Implementation
{
    public class KeepAliveService : IKeepAliveService
    {
        private class Connection
        {
            public Connection(IWebSocketConnection webSocketConnection)
            {
                WebSocketConnection = webSocketConnection;
            }

            public IWebSocketConnection WebSocketConnection { get; }

            public bool IsConfirmed {get; set; } = false;

            public int Fails { get; private set; } = 0;

            public void IncreaseFail()
            {
                Fails++;
            }
            public void DecreaseFail()
            {
                if(Fails != 0)
                    Fails = 0;
            }

        }


        public KeepAliveService(IWebSocketConnectionService webSocketConnectionService, IOptions<RealTimeOptions> options, ILogger<KeepAliveService> logger)
        {
            _webSocketConnectionService = webSocketConnectionService;
            _logger = logger;
            _realTimeOptions = options.Value;
            _webSocketConnectionService.Connect += WebSocketConnectionConnect;
            _webSocketConnectionService.Disconnect += WebSocketConnectionDisconnect;
            _timer = new Timer(new TimerCallback(StartPinging), 0, _realTimeOptions.CheckInterval, _realTimeOptions.CheckInterval);
        }

        private readonly IWebSocketConnectionService _webSocketConnectionService;
        private readonly RealTimeOptions _realTimeOptions;
        private readonly ILogger<KeepAliveService> _logger;
        private readonly ConcurrentDictionary<Guid, Connection> _connections = new ConcurrentDictionary<Guid, Connection>();

        // keep timer alive 
        private readonly Timer _timer;

        public void Connect(IWebSocketConnection webSocketConnection)
        {
            WebSocketConnectionConnect(this, webSocketConnection);
        }

        public void Disconnect(IWebSocketConnection webSocketConnection)
        {
            WebSocketConnectionDisconnect(this, webSocketConnection);
        }

        private void StartPinging(object obj)
        {
            if (0 == _connections.Count) 
            {
                return;
            }

            _logger.LogInformation("Hearbeating...");
            foreach (var connection in _connections)
            {
                if (connection.Value.Fails >= _realTimeOptions.LimitOfFails)
                {
                    var webSocketConnection = connection.Value.WebSocketConnection;
                    webSocketConnection.CloseStatus = System.Net.WebSockets.WebSocketCloseStatus.EndpointUnavailable;

                    _connections.TryRemove(webSocketConnection.Id, out Connection removedConnection );

                    _webSocketConnectionService.RemoveWebSocketConnection(webSocketConnection.Id);
                }
                if (connection.Value.IsConfirmed == true)
                {
                    connection.Value.IsConfirmed = false;
                }
                else
                {
                    connection.Value.IncreaseFail();
                    _logger.LogWarning($"WebSocketConnection: {connection.Key} failed the hearbeating, attempt {connection.Value.Fails}", connection);
                }
            }
            _webSocketConnectionService.SendToAllAsync(_realTimeOptions.PingSendMessage, CancellationToken.None);
        }

        private void WebSocketConnectionConnect(object sender, IWebSocketConnection webSocketConnection)
        {
            if (_connections.TryAdd(webSocketConnection.Id, new Connection(webSocketConnection)))
            {
                webSocketConnection.ReceiveText += ReceiveText;
            }
            else
            {
                _logger.LogWarning(new InvalidDataException(ErrorCode.Invalid), $"The same copy was detected", webSocketConnection, sender);

            }
        }

        private void WebSocketConnectionDisconnect(object sender, IWebSocketConnection webSocketConnection)
        {
            if (_connections.TryRemove(webSocketConnection.Id, out Connection connection))
            {
                webSocketConnection.ReceiveText -= ReceiveText;
            }
            else
            {
                _logger.LogWarning(new InvalidDataException(ErrorCode.Invalid), $"Failed to delete Connection", webSocketConnection, sender);


            }
        }
        private void ReceiveText(object sender, string textMessage)
        {
            if (textMessage.Equals(_realTimeOptions.PongExpectMessage))
            {
                var webSocketConnection = sender as IWebSocketConnection;
                _connections.TryGetValue(webSocketConnection.Id, out Connection connection);
                connection.IsConfirmed = true;
                connection.DecreaseFail();
                _logger.LogInformation($"WebSocketConnection: {webSocketConnection.Id} Confirmed Hearbeating...", webSocketConnection);
            }
        }

    }
}
