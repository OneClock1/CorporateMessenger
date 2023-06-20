using Chat.Domain.Entities;
using Chat.Domain.Abstractions;
using System;
using System.Threading.Tasks;
using System.Linq;
using AutoMapper;
using System.Linq.Expressions;
using Common.Implementations.ExceptionImplementations.Exceptions;
using Common.Domain.Enums;

namespace Chat.Infrastructure.Implementation
{
    public class MessageService : IMessageService
    {

        public MessageService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _uof = unitOfWork;
            _mapper = mapper;
        }

        private readonly IMapper _mapper;

        private readonly IUnitOfWork _uof;

        public async Task<bool> SendMessage(Message message)
        {

            if (message == null)
                throw new InvalidDataException(ErrorCode.Invalid, "Invalid Message");
            var result = await _uof.GetRepository<Message, long>()
                                   .CreateAsync(message) != null ? true : false;
            await _uof.SaveChangesAsync();

            return result;

        }

        public async Task<Message[]> GetMessages(int skip, int take, Expression<Func<Message, bool>> expression = null)
        {
            var repository = _uof.GetRepository<Message, long>();

            return await repository.GetAsync(expression, p => p.Skip(skip)
                                                               .Take(take)
                                                               .OrderBy(t => t.Id));


        }
    }
}
