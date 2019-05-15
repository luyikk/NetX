using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace ChatServer.Models
{
    public partial class UserDatabaseContext : DbContext
    {
        public UserDatabaseContext()
        {
        }

        public UserDatabaseContext(DbContextOptions<UserDatabaseContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Message> Message { get; set; }
        public virtual DbSet<Users> Users { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlite("Data Source=UserDatabase.db3");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasAnnotation("ProductVersion", "2.2.4-servicing-10062");

            modelBuilder.Entity<Message>(entity =>
            {
                entity.Property(e => e.MessageId).ValueGeneratedOnAdd();

                entity.Property(e => e.MessageContext).IsRequired();

                entity.HasOne(d => d.FromUser)
                    .WithMany(p => p.MessageFromUser)
                    .HasForeignKey(d => d.FromUserId);

                entity.HasOne(d => d.TargetUser)
                    .WithMany(p => p.MessageTargetUser)
                    .HasForeignKey(d => d.TargetUserId);
            });

            modelBuilder.Entity<Users>(entity =>
            {
                entity.HasKey(e => e.UserId);

                entity.Property(e => e.UserId).ValueGeneratedOnAdd();
            });
        }
    }
}
