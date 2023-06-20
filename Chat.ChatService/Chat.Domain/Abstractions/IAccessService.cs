using Chat.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Chat.Domain.Abstractions
{
    public interface IAccessService
    {
        /// <summary>
        /// Verify user's access to add user to chat
        /// </summary> 
        /// <param name="currentUsername">User's name what performs action</param>
        /// <param name="currentChat">Entity of Chat</param>
        /// <returns>Boolean value which present access permission;True - Allow, False - Deny</returns>
        public bool VerifyAccessToAddUser(string currentUsername, Domain.Entities.Chat currentChat);
        /// <summary>
        /// Verify user's access to add user to chat
        /// </summary> 
        /// <param name="currentUsername">User's name what performs action</param>
        /// <param name="chatId">Chat's identyfire</param>
        /// <returns>Boolean value which present access permission;True - Allow, False - Deny</returns>
        public Task<bool> VerifyAccessToAddUser(string currentUsername, long chatId);
        /// <summary>
        /// Verify user's access to remove user from chat
        /// </summary>
        /// <param name="currentUsername">User's name what performs action</param>
        /// <param name="targetUsername">The target user's name over which performs actions</param>
        /// <param name="currentChat">Entity of Chat</param>
        /// <returns>Boolean value which present access permission;True - Allow, False - Deny</returns>
        public bool VerifyAccessToRemoveUser(string currentUsername, string targetUsername, Domain.Entities.Chat currentChat);
        /// <summary>
        /// Verify user's access to remove user from chat
        /// </summary>
        /// <param name="currentUsername">User's name what performs action</param>
        /// <param name="targetUsername">The target user's name over which performs actions</param>
        /// <param name="chatId">Chat's identyfire</param>
        /// <returns>Boolean value which present access permission;True - Allow, False - Deny</returns>
        public Task<bool> VerifyAccessToRemoveUser(string currentUsername, string targetUsername, long chatId);
        /// <summary>
        /// Verify user's access to remove chat
        /// </summary>
        /// <param name="currentUsername">User's name what performs action</param>
        /// <param name="currentChat">Entity of Chat</param>
        /// <returns>Boolean value which present access permission;True - Allow, False - Deny</returns>
        public bool VerifyAccessToRemoveChat(string currentUsername, Domain.Entities.Chat currentChat);
        /// <summary>
        /// Verify user's access to remove chat
        /// </summary>
        /// <param name="currentUsername">User's name what performs action</param>
        /// <param name="chatId">Chat's identyfire</param>
        /// <returns>Boolean value which present access permission;True - Allow, False - Deny</returns>
        public Task<bool> VerifyAccessToRemoveChat(string currentUsername, long chatId);
        /// <summary>
        /// Verify user's access to messages in specify chat
        /// </summary>
        /// <param name="currentUsername">User's name what performs action</param>
        /// <param name="currentChat">Entity of Chat</param>
        /// <returns>Boolean value which present access permission;True - Allow, False - Deny</returns>
        public bool VerifyAccessToMessages(string currentUsername, Domain.Entities.Chat currentChat);
        /// <summary>
        /// Verify user's access to messages in specify chat
        /// </summary>
        /// <param name="currentUsername">User's name what performs action</param>
        /// <param name="chatId">Chat's identyfire</param>
        /// <returns>Boolean value which present access permission;True - Allow, False - Deny</returns>
        public Task<bool> VerifyAccessToMessages(string currentUsername, long chatId);
        /// <summary>
        /// Verify chat's owner 
        /// </summary>
        /// <param name="username">User's name</param>
        /// <param name="currentChat">Entity of Chat</param>
        /// <returns>True - User are owner, False - User aren't owner</returns>
        public bool IsChatOwner(string username, Domain.Entities.Chat currentChat);
        /// <summary>
        /// Verify chat's owner 
        /// </summary>
        /// <param name="username">User's name</param>
        /// <param name="chatId">Chat's identyfire</param>
        /// <returns>True - User are owner, False - User aren't owner</returns>
        public Task<bool> IsChatOwner(string username, long chatId);

    }
}
