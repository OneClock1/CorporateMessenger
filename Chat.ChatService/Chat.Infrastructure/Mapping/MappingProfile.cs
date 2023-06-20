using AutoMapper;
using Chat.Domain.DTOs;
using Chat.Domain.Entities;
using Chat.Infrastructure.Extensions;
using Common.Domain.DTOs.ChatDTOs;
using Common.Domain.DTOs.MessageDTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace Chat.Infrastructure.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            AllowNullCollections = true;

            AllowNullDestinationValues = true;

            CreateMap<CreateChatDTO, Domain.Entities.Chat>();

            CreateMap<Domain.Entities.Chat, ViewChatDTO>();


            CreateMap<User, ViewUserDTO>();


            CreateMap<CreateMessageDTO, Message>();

            CreateMap<Message, ViewMessageDTO>()
                .ForMember(d => d.LastUpdatedTime, w => w.MapFrom(u => UtcToUnixConverter.Convert(u.LastUpdatedTime)));
        }
    }
}
