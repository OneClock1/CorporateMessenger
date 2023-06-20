using Chat.Domain.Abstractions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Chat.Domain.Entities
{
    public class User : IEntity<string>
    {
        public string Id { get; set; }

        public ICollection<ChatUsers> ChatUsers { get; set; }

    }
}
