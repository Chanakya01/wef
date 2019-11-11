namespace IngramWorkFlow.MsSql
{
    using System;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;

 
    public partial class SQLContext : DbContext
    {
        private readonly IConfiguration _config;

        public SQLContext(IConfiguration config)
        {
            _config = config;
        }

        public virtual DbSet<Ticket> Tickets { get; set; }
        public virtual DbSet<TicketTransitionHistory> TicketTransitionHistories { get; set; }
        public virtual DbSet<User> Users { get; set; }
        public virtual DbSet<Role> Roles { get; set; }
        public virtual DbSet<StructDivision> StructDivisions { get; set; }
        public virtual DbSet<Head> VHeads { get; set; }
        public virtual DbSet<WorkflowInbox> WorkflowInboxes { get; set; }
        public virtual DbSet<UserRole> UserRoles { get; set; }
        public virtual DbSet<WorkflowScheme> WorkflowSchemes { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer(_config.GetConnectionString("DefaultConnection"));
            }

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>()
                .HasMany(e => e.Tickets)
                .WithOne(e => e.Author)
                .HasForeignKey(e => e.AuthorId);

            modelBuilder.Entity<User>()
                .HasMany(e => e.Tickets1)
                .WithOne(e => e.Manager)
                .HasForeignKey(e => e.ManagerId);

            modelBuilder.Entity<User>()
                .HasMany(e => e.UserRoles)
                .WithOne(r => r.User)
                .HasForeignKey(r => r.UserId);

            modelBuilder.Entity<Role>()
                        .HasMany(e => e.UserRoles)
                        .WithOne(r => r.Role)
                        .HasForeignKey(r => r.RoleId);

            modelBuilder.Entity<StructDivision>()
                .HasMany(e => e.StructDivision1)
                .WithOne(e => e.StructDivision2)
                .HasForeignKey(e => e.ParentId)
            ;

            modelBuilder.Entity<UserRole>()
                .HasKey(x => new { x.RoleId, x.UserId })
            ;

            modelBuilder.Entity<UserRole>()
             .HasOne(x => x.Role).WithMany(x => x.UserRoles)
         ;

        }
    }
}
