using Chat.Domain.Abstractions;
using Chat.Domain.Enums;
using Common.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Chat.Domain.Entities
{
    public class Chat : IEntity<long>
    {
        public long Id { get; set; }

        public string Name { get; set; }

        public string OwnerName { get; set; }

        public TypeOfChat Type { get; set; }

        public ICollection<ChatUsers> ChatUsers { get; set; }

        public ICollection<Message> Messages { get; set; }
    }
}
