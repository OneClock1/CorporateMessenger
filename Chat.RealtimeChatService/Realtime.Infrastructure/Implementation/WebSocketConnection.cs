using Common.Domain;
using Common.Domain.DTOs.MessageDTOs;
using Newtonsoft.Json;
using Realtime.Domain;
using Realtime.Domain.Abstractions;
using Realtime.Domain.DTOs;
using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Realtime.Infrastructure.Implementation
{
    public class WebSocketConnection : IWebSocketConnection
    {
        public WebSocketConnection(WebSocket webSocket, Guid ConnectionId, int receivePayloadBufferSize, CancellationToken cancellationToken)
        {
            _webSocket = webSocket ?? throw new ArgumentNullException(nameof(webSocket));
            Id = ConnectionId;
            _receivePayloadBufferSize = receivePayloadBufferSize;
            _cancellationToken = cancellationToken;
        }

        public Guid Id { get; set; }

        public string UserId { get; set; }

        public string SessionId { get; set; }

        public WebSocketCloseStatus? CloseStatus { get; set; } = null;

        public string CloseStatusDescription { get; private set; } = null;

        public event EventHandler<string> ReceiveText;

        public event EventHandler<IWebSocketConnection> OnClose;

        private readonly WebSocket _webSocket;

        private readonly int _receivePayloadBufferSize;

        private readonly CancellationToken _cancellationToken;



        public async Task SendAsync(byte[] message, CancellationToken cancellationToken)
        {
            await _webSocket.SendAsync(message, WebSocketMessageType.Text, true, cancellationToken);

        }

        public async Task SendAsync(string message, CancellationToken cancellationToken)
        {
            await _webSocket.SendAsync(
                Encoding.UTF8.GetBytes(message), WebSocketMessageType.Text, true, cancellationToken);
        }

        public void Send(object sender, EventArgs<ViewMessageDTO> args)
        {
            var message = JsonConvert.SerializeObject(args.Value);
            Task.Run(() => _webSocket.SendAsync(Encoding.UTF8.GetBytes(message), WebSocketMessageType.Text, true, _cancellationToken));
        }

        public async Task ReceiveMessagesUntilCloseAsync()
        {
            byte[] receivePayloadBuffer = new byte[_receivePayloadBufferSize];
            WebSocketReceiveResult webSocketReceiveResult;

            while (true)
            {
                webSocketReceiveResult = await _webSocket.ReceiveAsync(new ArraySegment<byte>(receivePayloadBuffer), _cancellationToken);

                if (webSocketReceiveResult.MessageType == WebSocketMessageType.Close)
                {
                    OnClose?.Invoke(this, this);
                    break;
                }
                   
                string webSocketMessage = Encoding.UTF8.GetString(receivePayloadBuffer).TrimEnd((char)0);

                Array.Clear(receivePayloadBuffer, 0, webSocketMessage.Length);
                OnReceiveText(webSocketMessage);
            }
            CloseStatus = webSocketReceiveResult.CloseStatus.Value;
            CloseStatusDescription = webSocketReceiveResult.CloseStatusDescription;
            _webSocket.Dispose();
        }

        private void OnReceiveText(string webSocketMessage)
        {
            ReceiveText?.Invoke(this, webSocketMessage);
        }
    }
}
