using domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data
{
    public class SystemDbContext : DbContext
    {
        public SystemDbContext(DbContextOptions<SystemDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users => Set<User>();
        public DbSet<Role> Roles => Set<Role>();
        public DbSet<DatabaseInstance> DatabaseInstances => Set<DatabaseInstance>();
        public DbSet<UserInstance> UserInstances => Set<UserInstance>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Role
            modelBuilder.Entity<Role>(entity =>
            {
                entity.ToTable("Roles");

                entity.HasKey(r => r.Id);

                entity.Property(r => r.Name)
                      .IsRequired()
                      .HasMaxLength(50);

                entity.HasIndex(r => r.Name)
                      .IsUnique();
            });

            // User
            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("Users");

                entity.HasKey(u => u.Id);

                entity.Property(u => u.Username)
                      .IsRequired()
                      .HasMaxLength(100);

                entity.Property(u => u.Email)
                      .IsRequired()
                      .HasMaxLength(150);

                entity.Property(u => u.PasswordHash)
                      .IsRequired();

                entity.HasIndex(u => u.Username)
                      .IsUnique();

                entity.HasIndex(u => u.Email)
                      .IsUnique();

                entity.HasOne(u => u.Role)
                      .WithMany(r => r.Users)
                      .HasForeignKey(u => u.RoleId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(u => u.UserInstance)
                      .WithOne(ui => ui.User)
                      .HasForeignKey<UserInstance>(ui => ui.UserId);
            });

            // DatabaseInstance
            modelBuilder.Entity<DatabaseInstance>(entity =>
            {
                entity.ToTable("DatabaseInstances");

                entity.HasKey(di => di.Id);

                entity.Property(di => di.Name)
                      .IsRequired()
                      .HasMaxLength(100);

                entity.Property(di => di.Description)
                      .HasMaxLength(250);

                entity.Property(di => di.ConnectionString)
                      .HasMaxLength(500);
            });

            // UserInstance
            modelBuilder.Entity<UserInstance>(entity =>
            {
                entity.ToTable("UserInstances");

                entity.HasKey(ui => ui.Id);

                entity.HasOne(ui => ui.DatabaseInstance)
                      .WithMany(di => di.UserInstances)
                      .HasForeignKey(ui => ui.DatabaseInstanceId)
                      .OnDelete(DeleteBehavior.Restrict);

                // 1 estudiante = 1 instancia
                entity.HasIndex(ui => ui.UserId)
                      .IsUnique();
            });
        }
    }
}
