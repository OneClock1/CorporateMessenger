using Chat.Domain.Abstractions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Chat.Domain.Entities
{
    public class Message : IEntity<long>
    {
        public long Id { get; set; }

        public long ChatId { get; set; }

        public Chat Chat { get; set; }

        public string Sender { get; set; }

        public string TextMessage { get; set; }

        public DateTime LastUpdatedTime { get; set; }
    }
}
