using System;
using System.Collections.Generic;
using System.Text;

namespace Chat.Domain.Abstractions
{
    public interface IEntity<TKey>
    {
        public TKey Id { get; set; }
    }
}
