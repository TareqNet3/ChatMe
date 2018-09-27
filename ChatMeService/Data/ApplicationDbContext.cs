using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ChatMeService.Models;
using ChatMeService.Models.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ChatMeService.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // ConversationAdmin M2M
            builder.Entity<ConversationUser>()
                   .HasKey(ca => new { ca.ConversationID, ca.UserID });
            builder.Entity<ConversationUser>()
                   .HasOne(ca => ca.Conversation)
                   .WithMany(b => b.ConversationUsers)
                   .HasForeignKey(u => u.UserID);
            builder.Entity<ConversationUser>()
                   .HasOne(ca => ca.User)
                   .WithMany(c => c.ConversationUsers)
                   .HasForeignKey(cp => cp.ConversationID);

            // ConversationAdmin M2M
            builder.Entity<ConversationAdmin>()
                   .HasKey(ca => new { ca.ConversationID, ca.AdminID });
            builder.Entity<ConversationAdmin>()
                   .HasOne(ca => ca.Conversation)
                   .WithMany(b => b.ConversationAdmins)
                   .HasForeignKey(u => u.AdminID);
            builder.Entity<ConversationAdmin>()
                   .HasOne(ca => ca.Admin)
                   .WithMany(c => c.ConversationAdmins)
                   .HasForeignKey(cp => cp.ConversationID);
        }

        public DbSet<Conversation> Conversations { get; set; }
        public DbSet<ConversationUser> ConversationUsers { get; set; }
        public DbSet<ConversationAdmin> ConversationAdmins { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<UserMessage> UserMessages { get; set; }
    }
}