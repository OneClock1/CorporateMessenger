using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common.Domain.Enums;
using Common.Implementations.ExceptionImplementations.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Realtime.Domain.Abstractions;

namespace Realtime.API.Controllers
{
    [Authorize]
    [Route("api/realtime")]
    [ApiController]
    public class RealtimeController : ControllerBase
    {

        public RealtimeController(IWebSocketConnectionService webSocketConnectionService)
        {
            _webSocketConnectionService = webSocketConnectionService;
        }

        private readonly IWebSocketConnectionService _webSocketConnectionService;

        /// <summary>
        /// Get Connection Id
        /// </summary>
        /// <returns>Connection Id Guid</returns>
        [HttpGet]
        public Guid GetConnectionId()
        {
            var currentUsername = User.Claims.FirstOrDefault(prop => prop.Type == "username")?.Value;

            if (String.IsNullOrEmpty(currentUsername))
                throw new InvalidPermissionException(ErrorCode.InvalidPermision, "Invalid claim username");

            var currentSessionId = User.Claims.FirstOrDefault(prop => prop.Type == "session_id")?.Value;

            if (String.IsNullOrEmpty(currentSessionId))
                throw new InvalidPermissionException(ErrorCode.InvalidPermision, "Invalid claim currentSessionId");

            return _webSocketConnectionService.RegisterUserId(currentUsername, currentSessionId);
        }
    }
}