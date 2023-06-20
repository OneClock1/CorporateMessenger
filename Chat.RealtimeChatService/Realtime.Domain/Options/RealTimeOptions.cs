using System;
using System.Collections.Generic;
using System.Text;

namespace Realtime.Domain.Options
{
    public class RealTimeOptions
    {
        public string PingSendMessage { get; set; }

        public string PongExpectMessage { get; set; }

        public int CheckInterval { get; set; }

        public int LimitOfFails { get; set; }

        public int PayloadBufferSize { get; set; }

        public string WebSocketPath { get; set; }
    }
}
