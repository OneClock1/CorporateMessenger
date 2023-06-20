using Chat.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Chat.Domain.Abstractions
{
    public interface IChatService
    {

        public Task<Entities.Chat> CreateChat(Entities.Chat chat);

        public Task<bool> AddUserToChat(string userName, long chatId);

        public Task<bool> RemoveUserFromChat(string userName, long chatId);

        public Task<Entities.Chat> GetChat(long chatId);

        public Task<Entities.Chat[]> GetUsersChats(string userName);

        public Task<User[]> GetUsersFromChat(long chatId);

        //public Task<bool> SendMessage(Message message);

        public Task<bool> RemoveChat(long chatId);

    }
}
