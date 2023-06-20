using System;
using System.Collections.Generic;
using System.Text;

namespace Chat.Domain.Entities
{
    public class ChatUsers
    {
        public long ChatId { get; set; }

        public Chat Chat { get; set; }

        public string UserId { get; set; }

        public User User { get; set; }
    }
}
