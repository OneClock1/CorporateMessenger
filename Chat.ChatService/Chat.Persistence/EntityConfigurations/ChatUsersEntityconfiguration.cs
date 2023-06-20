using Chat.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace Chat.Persistence.EntityConfigurations
{
    class ChatUsersEntityconfiguration : IEntityTypeConfiguration<ChatUsers>
    {
        public void Configure(EntityTypeBuilder<ChatUsers> chatUsersBuilder)
        {
            chatUsersBuilder.HasKey(p => new { p.ChatId, p.UserId });

            chatUsersBuilder.HasOne(p => p.Chat)
                            .WithMany(p => p.ChatUsers)
                            .HasForeignKey(p => p.ChatId);

            chatUsersBuilder.HasOne(p => p.User)
                            .WithMany(p => p.ChatUsers)
                            .HasForeignKey(p => p.UserId);

        }
    }
}
