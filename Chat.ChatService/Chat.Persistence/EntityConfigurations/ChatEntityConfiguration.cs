using Chat.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace Chat.Persistence.EntityConfigurations
{
    class ChatEntityConfiguration : IEntityTypeConfiguration<Chat.Domain.Entities.Chat>
    {
        public void Configure(EntityTypeBuilder<Chat.Domain.Entities.Chat> chatBuilder)
        {
            chatBuilder.HasKey(p => p.Id);

            chatBuilder.Property(p => p.Id).IsRequired();

            chatBuilder.Property(p => p.Name).IsRequired().HasMaxLength(30);
            
            chatBuilder.Property(p => p.Type).IsRequired();

        }
    }
}
