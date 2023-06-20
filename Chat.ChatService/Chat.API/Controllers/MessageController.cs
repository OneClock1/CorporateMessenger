using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using AutoMapper;
using Chat.Domain.Abstractions;
using Chat.Domain.DTOs.FiltersDTO;
using Chat.Domain.Entities;
using Common.Domain.Abstractions;
using Common.Domain.DTOs;
using Common.Domain.DTOs.MessageDTOs;
using Common.Domain.Enums;
using Common.Implementations.ExceptionImplementations.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace Chat.API.Controllers
{
    /// <summary>
    /// Message controller
    /// </summary>
    [Route("api/messages")]
    [ApiController]
    public class MessageController : ControllerBase
    {
        public MessageController(IMapper mapper, IMessageService messageService, INotificationService notificationService, IAccessService accessService)
        {
            _messageService = messageService;
            _mapper = mapper;
            _notificationService = notificationService;
            _accessService = accessService;
        }

        private readonly INotificationService _notificationService;


        private readonly IMessageService _messageService;

        private readonly IMapper _mapper;

        private readonly IAccessService _accessService;


        /// <summary>
        /// Send message to chat
        /// </summary>
        /// <param name="createMessageDTO">DTO for message</param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ActionResult<string>> Send(CreateMessageDTO createMessageDTO)
        {
            var mappedMessage = _mapper.Map<Message>(createMessageDTO);

            var username = User.Claims.FirstOrDefault(prop => prop.Type == "username")?.Value;

            if (String.IsNullOrEmpty(username))
                throw new InvalidPermissionException(ErrorCode.InvalidPermision, "Invalid claim username");
            
            if (await _accessService.VerifyAccessToMessages(username, mappedMessage.ChatId))
            {
                mappedMessage.Sender = username;
                await _messageService.SendMessage(mappedMessage);

                var mappedViewMessage = _mapper.Map<ViewMessageDTO>(mappedMessage);
                _notificationService.Publish(mappedViewMessage, mappedMessage.ChatId);
                return Ok($"Message was send: \"{mappedMessage.TextMessage}\" to \"{mappedMessage.ChatId}\"");
            }
            else
                throw new InvalidPermissionException(ErrorCode.InvalidPermision, "Don't have permission to send message");
        }

        /// <summary>
        /// Get messages 
        /// </summary>
        /// <param name="messageFilter"> Filter model</param>
        /// <returns></returns>
        [HttpGet]
        public async Task<ActionResult<ContentDTO<ViewMessageDTO>>> GetMessages([FromQuery]MessageFilterModel messageFilter)
        {
            var username = User.Claims.FirstOrDefault(prop => prop.Type == "username")?.Value;

            if (String.IsNullOrEmpty(username))
                throw new InvalidPermissionException(ErrorCode.InvalidPermision, "Invalid claim username");

            if (await _accessService.VerifyAccessToMessages(username, messageFilter.ChatId))
            {
                Expression<Func<Message, bool>> expression = m => m.ChatId == messageFilter.ChatId
                                                                  && m.LastUpdatedTime.Second >= messageFilter.LastUpdatedTime
                                                                  && m.Id > messageFilter.MessageId;

                var skipCount = (messageFilter.Page - 1) * messageFilter.Limit;

                var messages = await _messageService.GetMessages(skipCount, messageFilter.Limit, expression);


                ICollection<ViewMessageDTO> ViewMessageDTOs = new List<ViewMessageDTO>();

                foreach (var item in messages)
                {
                    ViewMessageDTOs.Add(_mapper.Map<ViewMessageDTO>(item));
                }
                return Ok(new ContentDTO<ViewMessageDTO>
                {
                    Content = ViewMessageDTOs.ToArray(),
                    TotalCount = ViewMessageDTOs.Count
                });
            }
            else
                throw new InvalidPermissionException(ErrorCode.InvalidPermision, "Don't have permission to get messages");
        }
    }
}