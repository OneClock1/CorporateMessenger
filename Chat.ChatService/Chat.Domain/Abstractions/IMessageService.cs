using Chat.Domain.DTOs;
using Chat.Domain.DTOs.FiltersDTO;
using Chat.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Chat.Domain.Abstractions
{
    public interface IMessageService
    {
        public Task<bool> SendMessage(Message message);

        public Task<Message[]> GetMessages(int skip, int take, Expression<Func<Message, bool>> expression = null); 
    }
}
