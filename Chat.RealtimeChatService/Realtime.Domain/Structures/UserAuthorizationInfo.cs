using System;
using System.Collections.Generic;
using System.Text;

namespace Realtime.Domain.Structures
{
    public struct UserAuthorizationInfo
    {
        public string UserId;
        public string SessionId;
        public UserAuthorizationInfo(string userId, string sessionId)
        {
            UserId = userId;
            SessionId = sessionId;
        }
    }
}
