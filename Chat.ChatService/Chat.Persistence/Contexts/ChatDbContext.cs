using Chat.Domain.Entities;
using Chat.Persistence.EntityConfigurations;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Chat.Persistence.Contexts
{
    public class ChatDbContext : DbContext
    {

        public ChatDbContext(DbContextOptions<ChatDbContext> options)
            : base(options)
        {
        }

        public DbSet<Chat.Domain.Entities.Chat> Chats { get; set; }

        public DbSet<User> Users { get; set; }

        public DbSet<ChatUsers> ChatUsers { get; set; }

        public DbSet<Message> Messages { get; set; }


        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.ApplyConfiguration(new ChatEntityConfiguration());
            builder.ApplyConfiguration(new ChatUsersEntityconfiguration());
            builder.ApplyConfiguration(new UserEntityConfiguration());
            builder.ApplyConfiguration(new MessageEntityConfiguration());

        }
    }
}
