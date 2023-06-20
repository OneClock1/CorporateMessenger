using Chat.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace Chat.Persistence.EntityConfigurations
{
    class MessageEntityConfiguration : IEntityTypeConfiguration<Message>
    {
        public void Configure(EntityTypeBuilder<Message> messageBuilder)
        {
            messageBuilder.HasKey(p => p.Id);

            messageBuilder.HasIndex(p => p.Sender);

            messageBuilder.Property(p => p.Sender).IsRequired();

            messageBuilder.Property(p => p.LastUpdatedTime)
                          .HasDefaultValueSql("current_timestamp(6) ON UPDATE current_timestamp(6)")
                          .ValueGeneratedOnAddOrUpdate();

            messageBuilder.HasOne(P => P.Chat)
                          .WithMany(p => p.Messages)
                          .HasForeignKey(p => p.ChatId)
                          .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
