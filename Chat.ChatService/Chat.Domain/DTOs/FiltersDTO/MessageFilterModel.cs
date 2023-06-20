using Common.Domain.DTOs.FilterDTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace Chat.Domain.DTOs.FiltersDTO
{
   public class MessageFilterModel : BaseFilterModel
    {
        public long MessageId { get; set; }

        public long ChatId { get; set; }

        public long LastUpdatedTime { get; set; }

        public MessageFilterModel()
            : base()
        {
            LastUpdatedTime = 0;
            MessageId = 0;
        }

    }
}
