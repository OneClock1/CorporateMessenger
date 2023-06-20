using Realtime.Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Realtime.Domain.DTOs
{
    public class SubscribeSocketDTO
    {
        [Required]
        public TypeOfSocketAction SocketAction { get; set; }

        [Required]
        public TypeOfSubscribeObject TypeOfSubscribeObject { get; set; }

        [Required]
        public long ObjectId { get; set; }
    }
}
