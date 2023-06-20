using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Chat.Domain.Abstractions;
using Chat.Domain.DTOs;
using Common.Domain.DTOs;
using Common.Domain.DTOs.ChatDTOs;
using Common.Domain.Enums;
using Common.Implementations.ExceptionImplementations.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Chat.API.Controllers
{
    [Authorize]
    [Route("api/chats")]
    [ApiController]
    public class ChatController : ControllerBase
    {
        public ChatController(IChatService chatService, IMapper mapper, IAccessService accessService)
        {
            _chatService = chatService;

            _accessService = accessService;

            _mapper = mapper;
        }

        private readonly IAccessService _accessService;

        private readonly IMapper _mapper;

        private readonly IChatService _chatService;

        /// <summary>
        /// Verify access to chat
        /// </summary>
        /// <param name="chatId">Chat's identifier</param>
        /// <param name="username">User's nick name</param>
        /// <returns></returns>
        [HttpGet("{chatId}/users/{username}/access")]
        public async Task<ActionResult<bool>> Access([FromRoute] long chatId, [FromRoute] string username)
        {
            if (String.IsNullOrEmpty(username))
                throw new InvalidPermissionException(ErrorCode.InvalidPermision, "Invalid claim username");

            var result = await _accessService.VerifyAccessToMessages(username, chatId);

            return result == true ? Ok(result) : StatusCode(403, result);
        }

        /// <summary>
        /// Verify access to chat
        /// </summary>
        /// <param name="chatId">Chat's identifier</param>
        /// <returns></returns>
        [HttpGet("{chatId}/exist")]
        public async Task<ActionResult<bool>> IsExist([FromRoute] long chatId)
        {
            var result = await _chatService.GetChat(chatId);

            return Ok(true);
        }

        /// <summary>
        /// Get chat's informations
        /// </summary>
        /// <param name="chatId">Chat's identifier</param>
        /// <returns>Return 200 status code with ViewChatDTO</returns>
        [HttpGet("{chatId}")]
        public async Task<ActionResult<ViewChatDTO>> GetChat([FromRoute]long chatId)
        {

            if (chatId == 0)
                throw new InvalidDataException(ErrorCode.Invalid, "Invalid chatId");

            var chat = await _chatService.GetChat(chatId);

            var mappedChat = _mapper.Map<ViewChatDTO>(chat);

            return Ok(mappedChat);
        }

        /// <summary>
        /// Get all User's chats
        /// </summary>
        /// <returns>ContentDTO include ViewChatDTO</returns>
        [HttpGet]
        public async Task<ActionResult<ContentDTO<ViewChatDTO>>> GetUserChats()
        {
            var username = User.Claims.FirstOrDefault(prop => prop.Type == "username")?.Value;

            if (String.IsNullOrEmpty(username))
                throw new InvalidPermissionException(ErrorCode.InvalidPermision, "Invalid claim username");

            var chats = await _chatService.GetUsersChats(username);

            ICollection<ViewChatDTO> viewChatDTOs = new List<ViewChatDTO>();

            foreach (var item in chats)
            {
                viewChatDTOs.Add(_mapper.Map<ViewChatDTO>(item));
            }

            return Ok(new ContentDTO<ViewChatDTO>
            {
                Content = viewChatDTOs.ToArray(),
                TotalCount = viewChatDTOs.Count
            });
        }


        /// <summary>
        /// Get all users from specific Chat
        /// </summary>
        /// <param name="chatId">Chat's identifier</param>
        /// <returns></returns>
        [HttpGet("{chatId}/users")]
        public async Task<ActionResult<ContentDTO<ViewUserDTO>>> GetUsersFromChat([FromRoute]long chatId)
        {

            var users = await _chatService.GetUsersFromChat(chatId);

            ICollection<ViewUserDTO> viewUserDTOs = new List<ViewUserDTO>();

            foreach (var item in users)
            {
                viewUserDTOs.Add(_mapper.Map<ViewUserDTO>(item));
            }

            return Ok(new ContentDTO<ViewUserDTO> 
            { 
                Content = viewUserDTOs.ToArray(), 
                TotalCount = viewUserDTOs.Count 
            });
        }

        /// <summary>
        /// Create Chat
        /// </summary>
        /// <param name="createChatDTO"> Chat's DTO for creating Chat</param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ActionResult<ViewChatDTO>> CreateChat([FromBody]CreateChatDTO createChatDTO)
        {
            var username = User.Claims.FirstOrDefault(prop => prop.Type == "username")?.Value;

            if (String.IsNullOrEmpty(username))
                throw new InvalidPermissionException(ErrorCode.InvalidPermision, "Invalid claim username");

            var mappedChat = _mapper.Map<Domain.Entities.Chat>(createChatDTO);

            mappedChat.OwnerName = username;

            var chat = await _chatService.CreateChat(mappedChat);

            return Ok(_mapper.Map<ViewChatDTO>(chat));
        }


        /// <summary>
        /// Add user to specific chat
        /// </summary>
        /// <param name="targetUsername">The username you want to add</param>
        /// <param name="chatId">Chat's identifier</param>
        /// <returns>True: seccess; Flase: Fail</returns>
        [HttpPost("{chatId}/users")]
        public async Task<ActionResult<bool>> AddUserToChat([FromRoute] long chatId, [FromQuery]string targetUsername)
        {
            var currentUsername = User.Claims.FirstOrDefault(prop => prop.Type == "username")?.Value;

            if (String.IsNullOrEmpty(currentUsername))
                throw new InvalidPermissionException(ErrorCode.InvalidPermision, "Invalid claim username");

            if (String.IsNullOrEmpty(targetUsername) || chatId == 0)
                throw new InvalidDataException(ErrorCode.Invalid, "Invalid UserName or chatId");

            if(await _accessService.VerifyAccessToAddUser(currentUsername, chatId))
                return Ok(await _chatService.AddUserToChat(targetUsername, chatId));
            else
                throw new InvalidPermissionException(ErrorCode.InvalidPermision, "Don't have permission to add user");
        }

        /// <summary>
        /// Remove user from specific chat
        /// </summary>
        /// <param name="targetUsername">The username you want to remove</param>
        /// <param name="chatId">Chat's identifier</param>
        /// <returns>True: seccess; Flase: Fail</returns>
        [HttpDelete("{chatId}/users")]
        public async Task<ActionResult<bool>> RemoveUserFromChat([FromRoute] long chatId, [FromQuery] string targetUsername)
        {
            var currentUsername = User.Claims.FirstOrDefault(prop => prop.Type == "username")?.Value;

            if (String.IsNullOrEmpty(currentUsername))
                throw new InvalidPermissionException(ErrorCode.InvalidPermision, "Invalid claim username");

            if (String.IsNullOrEmpty(targetUsername) || chatId <= 0)
                throw new InvalidDataException(ErrorCode.Invalid, "Invalid UserName or chatId");

            if (await _accessService.VerifyAccessToRemoveUser(currentUsername, targetUsername, chatId))
                return Ok(await _chatService.RemoveUserFromChat(targetUsername, chatId));
            else
                throw new InvalidPermissionException(ErrorCode.InvalidPermision, "Don't have permission to remove user");
        }

        /// <summary>
        /// Remove chat
        /// </summary>
        /// <param name="chatId">Chat's identifier</param>
        /// <returns>True: seccess; Flase: Fail</returns>
        [HttpDelete("{chatId}")]
        public async Task<ActionResult<bool>> RemoveChat([FromRoute]long chatId)
        {
            var currentUsername = User.Claims.FirstOrDefault(prop => prop.Type == "username")?.Value;

            if (String.IsNullOrEmpty(currentUsername))
                throw new InvalidPermissionException(ErrorCode.InvalidPermision, "Invalid claim username");

            if (chatId == 0)
                throw new InvalidDataException(ErrorCode.Invalid, "Invalid chatId");

            if (await _accessService.VerifyAccessToRemoveChat(currentUsername, chatId))
                return Ok(await _chatService.RemoveChat(chatId));
            else
                throw new InvalidPermissionException(ErrorCode.InvalidPermision, "Don't have permission to remove user");

            throw new BaseException(ErrorCode.UnknownError, "Somthing went wrong");
        }
    }
}