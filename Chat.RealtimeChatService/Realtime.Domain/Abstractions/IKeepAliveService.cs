using System;
using System.Collections.Generic;
using System.Text;

namespace Realtime.Domain.Abstractions
{
    public interface IKeepAliveService
    {
        void Connect(IWebSocketConnection webSocketConnection);
        void Disconnect(IWebSocketConnection webSocketConnection);
    }
}
