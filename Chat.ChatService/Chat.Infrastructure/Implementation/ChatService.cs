using Chat.Domain.Entities;
using Chat.Domain.Abstractions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Common.Implementations.InnerHttpClient;
using Common.Implementations.ExceptionImplementations.Exceptions;
using Common.Domain.Enums;

namespace Chat.Infrastructure.Implementation
{
    public class ChatService : IChatService
    {
        private IUnitOfWork _uof;

        private IdentityHttpService _identityHttpService;

        public ChatService(IUnitOfWork UOF, IdentityHttpService identityHttpService)
        {
            _uof = UOF;
            _identityHttpService = identityHttpService;
        }

        /// <summary>
        /// Create chat
        /// </summary>
        /// <param name="chat">Instance of Chat entity</param>
        /// <returns>Chat entity</returns>
        public async Task<Domain.Entities.Chat> CreateChat(Domain.Entities.Chat chat)
        {
            if (await IfUserExistsAdd(chat.OwnerName))
            {
                var chatRepository = _uof.GetRepository<Domain.Entities.Chat, long>();

                chat.ChatUsers = new List<ChatUsers>() 
                {
                    new ChatUsers 
                    {
                        ChatId = chat.Id,
                        UserId = chat.OwnerName
                    }
                };

                var result = await chatRepository.CreateAsync(chat);

                if (result == null)
                    throw new BaseException(ErrorCode.UnknownError, "Can't create chat. Please alert administrators");


                await Commit();

                return result;
            }
            else
                throw new BaseException(ErrorCode.UnknownError, "Something went wrong. Please alert administrators");
        }

        /// <summary>
        /// Add user to chat
        /// </summary>
        /// <param name="userName">User's name</param>
        /// <param name="chatId">Chat's identifier</param>
        /// <returns>True: If user was added; 
        /// False: If user wasn't added</returns>
        public async Task<bool> AddUserToChat(string userName, long chatId)
        {
            if (await IfUserExistsAdd(userName))
            {
                var chatRepository = _uof.GetRepository<Domain.Entities.Chat, long>();
                var chat = await chatRepository.FindByKeyAsync(chatId, "ChatUsers");

                if (chat == null)
                    throw new NotFoundException(ErrorCode.NotFound, "Not found Chat");

                foreach (var item in chat.ChatUsers)
                    if (item.UserId == userName)
                        throw new InvalidDataException(ErrorCode.Conflict, "User exists in Chat");


                chat.ChatUsers.Add(new ChatUsers
                {
                    ChatId = chatId,
                    UserId = userName
                });

                await Commit();

                return true;
            }
            else
                throw new Exception();
        }

        /// <summary>
        /// Remove user from chat
        /// </summary>
        /// <param name="userName">User's name</param>
        /// <param name="chatId">Chat's identifier</param>
        /// <returns>True: If user was removed; 
        /// False: If user wasn't removed;</returns>
        public async Task<bool> RemoveUserFromChat(string userName, long chatId)
        {
            var chatRepository = _uof.GetRepository<Domain.Entities.Chat, long>();
            var chat = await chatRepository.FindByKeyAsync(chatId, "ChatUsers");

            if (chat == null)
                throw new NotFoundException(ErrorCode.NotFound, "Not found Chat");

            var chatUser = chat.ChatUsers.SingleOrDefault(p => p.ChatId == chatId && p.UserId == userName);

            if (chatUser == null)
                throw new NotFoundException(ErrorCode.NotFoundUser, "Not found User");

            var result = chat.ChatUsers.Remove(chatUser);

            await Commit();

            return result;
        }

        /// <summary>
        /// Get chat's informations
        /// </summary>
        /// <param name="chatId">Chat's identifier</param>
        /// <returns></returns>
        public async Task<Domain.Entities.Chat> GetChat(long chatId)
        {
            var chatRepository = _uof.GetRepository<Domain.Entities.Chat, long>();

            var chat = await chatRepository.FindByKeyAsync(chatId, "ChatUsers");

            if (chat == null)
                throw new NotFoundException(ErrorCode.NotFound, "Not found Chat");

            return chat;
        }

        /// <summary>
        /// Get user's chats
        /// </summary>
        /// <param name="userName">User's name</param>
        /// <returns>Array of Chats</returns>
        public async Task<Domain.Entities.Chat[]> GetUsersChats(string userName)
        {
            var userRepository = _uof.GetRepository<User, string>();

            var user = await userRepository.FindByKeyAsync(userName, "ChatUsers.Chat");

            if (user == null)
                throw new NotFoundException(ErrorCode.NotFoundUser, "Not found User");

            var chats = user.ChatUsers.Select(p => p.Chat).ToArray();

            return chats;
        }

        /// <summary>
        /// Get chat's users
        /// </summary>
        /// <param name="chatId">Chat's identifier</param>
        /// <returns>Array of Users</returns>
        public async Task<User[]> GetUsersFromChat(long chatId)
        {
            var chatRepository = _uof.GetRepository<Domain.Entities.Chat, long>();

            var chat = await chatRepository.FindByKeyAsync(chatId, "ChatUsers.User");

            if (chat == null)
                throw new NotFoundException(ErrorCode.NotFound, "Not found Chat");

            var users = chat.ChatUsers.Select(p => p.User).ToArray();

            return users;
        }


        /// <summary>
        /// Remove chat
        /// </summary>
        /// <param name="chatId">Chat's identifier</param>
        /// <returns>True: If chat was removed; 
        /// False: If chat wasn't removed</returns>
        public async Task<bool> RemoveChat(long chatId)
        {
            var chat = await _uof.GetRepository<Domain.Entities.Chat, long>().FindByKeyAsync(chatId);

            if (chat == null)
                throw new NotFoundException(ErrorCode.NotFound, "Not found Chat");

            var result = _uof.GetRepository<Domain.Entities.Chat, long>().Remove(chatId);

           await Commit();
            return result;
        }


        /// <summary>
        /// Commit all changes
        /// </summary>
        /// <returns></returns>
        private async Task Commit()
        {
            await _uof.SaveChangesAsync();
        }


        /// <summary>
        /// Will add a user if the user exists in identity and not exist in chat database.
        /// </summary>
        /// <param name="userName">User's name</param>
        /// <returns>bool: true - If user exist or added; false - if user not exist</returns>
        private async Task<bool> IfUserExistsAdd(string userName)
        {
            var user = await _uof.GetRepository<User, string>().FindByKeyAsync(userName);

            if (user == null)
            {
                var userExist = await _identityHttpService.IsUserExistsAsync(userName);

                if (userExist == true)
                {
                    var newUser = new User
                    {
                        Id = userName
                    };

                    await _uof.GetRepository<User, string>().CreateAsync(newUser);
                    await Commit();

                    return true;
                }
                else
                    throw new NotFoundException(ErrorCode.NotFoundUser, "Not found User");
            }

            return true;

        }

    }
}
