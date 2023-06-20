using Chat.Domain.Enums;
using Chat.Domain.Abstractions;
using System.Threading.Tasks;
using System.Linq;
using Common.Implementations.ExceptionImplementations.Exceptions;
using Common.Domain.Enums;

namespace Chat.Infrastructure.Implementation
{
    /// <summary>
    /// Verify User's Access to specific chat
    /// </summary>
    public class AccessService : IAccessService
    {

        public AccessService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        private readonly IUnitOfWork _unitOfWork;

        public bool VerifyAccessToAddUser(string currentUsername, Domain.Entities.Chat currentChat)
        {
            switch (currentChat.Type)
            {
                case TypeOfChat.OneToOne:
                    if (IsChatOwner(currentUsername, currentChat))
                    {
                        if (currentChat.ChatUsers.Count <= 1)
                            return true;
                        else
                            throw new InvalidDataException(ErrorCode.Invalid, "Can't add more users to this chat");
                    }
                    else
                        return false;

                case TypeOfChat.PrivateGroup:
                    if (IsChatOwner(currentUsername, currentChat))
                    {
                        return true;
                    }
                    else
                        return false;

                case TypeOfChat.PublicGroup:
                    return true;

                default:
                    throw new InvalidDataException(ErrorCode.Invalid, "Invalid Type of chat");
            }
        }

        public async Task<bool> VerifyAccessToAddUser(string currentUsername, long chatId)
        {

            var chat = await _unitOfWork.GetRepository<Domain.Entities.Chat, long>().FindByKeyAsync(chatId, "ChatUsers");
            return VerifyAccessToAddUser(currentUsername, chat);
        }

        public bool VerifyAccessToRemoveUser(string currentUsername, string targetUsername, Domain.Entities.Chat currentChat)
        {
            switch (currentChat.Type)
            {
                case TypeOfChat.OneToOne:
                    if (IsChatOwner(currentUsername, currentChat) || currentUsername == targetUsername)
                    {
                        if (currentChat.ChatUsers.Count == 2)
                            return true;
                        else
                            throw new InvalidDataException(ErrorCode.Invalid, "Can't remove last user from this chat");
                    }
                    else
                        return false;

                case TypeOfChat.PrivateGroup:
                    if (IsChatOwner(currentUsername, currentChat) || currentUsername == targetUsername)
                    {
                        return true;
                    }
                    else
                        return false;

                case TypeOfChat.PublicGroup:
                    if (IsChatOwner(currentUsername, currentChat) || currentUsername == targetUsername)
                    {
                        return true;
                    }
                    else
                        return false;
                default:
                    throw new InvalidDataException(ErrorCode.Invalid, "Invalid Type of chat");
            }
        }

        public async Task<bool> VerifyAccessToRemoveUser(string currentUsername, string targetUsername, long chatId)
        {
            var chat = await _unitOfWork.GetRepository<Domain.Entities.Chat, long>().FindByKeyAsync(chatId, "ChatUsers");
            return VerifyAccessToRemoveUser(currentUsername, targetUsername, chat);
        }

        public bool VerifyAccessToRemoveChat(string currentUsername, Domain.Entities.Chat currentChat)
        {
            if (IsChatOwner(currentUsername, currentChat))
            {
                return true;
            }
            else
                return false;
        }

        public async Task<bool> VerifyAccessToRemoveChat(string currentUsername, long chatId)
        {
            var chat = await _unitOfWork.GetRepository<Domain.Entities.Chat, long>().FindByKeyAsync(chatId);

            return VerifyAccessToRemoveChat(currentUsername, chat);
        }

        public bool VerifyAccessToMessages(string currentUsername, Domain.Entities.Chat currentChat)
        {
            if (currentChat.ChatUsers.FirstOrDefault(u => u.UserId == currentUsername) != null)
                return true;
            return false;
        }

        public async Task<bool> VerifyAccessToMessages(string currentUsername, long chatId)
        {
            var chat = await _unitOfWork.GetRepository<Domain.Entities.Chat, long>().FindByKeyAsync(chatId, "ChatUsers");
            return VerifyAccessToMessages(currentUsername, chat);
        }

        public bool IsChatOwner(string username, Domain.Entities.Chat currentChat)
        {
            return currentChat.OwnerName == username;
        }

        public async Task<bool> IsChatOwner(string username, long chatId)
        {
            var chat = await _unitOfWork.GetRepository<Domain.Entities.Chat, long>().FindByKeyAsync(chatId);
            return IsChatOwner(username, chat);
        }
    }
}
