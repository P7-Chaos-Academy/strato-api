using Microsoft.EntityFrameworkCore;
using stratoapi.Models;

namespace stratoapi.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<Test> Test { get; set; }
    public DbSet<User> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // Configure User entity
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Username);
            entity.Property(e => e.Email).IsRequired();
            entity.Property(e => e.PasswordHash).IsRequired();
            entity.Property(e => e.Role).IsRequired();
        });

        // Configure Post entity
        modelBuilder.Entity<Test>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Name).IsUnique();
        });
        
        // Seed admin user with password
        var initUser = new User
        {
            Id = 1,
            Username = "seedUser",
            PasswordHash = new byte[]
            {
                0x00, 0x1f, 0xa4, 0xb6, 0x86, 0x11, 0xfa, 0x0e,
                0x9c, 0xf9, 0x95, 0xc1, 0x45, 0x50, 0xf8, 0x1a,
                0xc6, 0x32, 0xe9, 0x26, 0xd9, 0x45, 0x21, 0x0d,
                0x60, 0x3f, 0x2b, 0xea, 0x4d, 0x53, 0x90, 0x0a,
                0xaf, 0x06, 0x27, 0x17, 0xf1, 0x7d, 0x7c, 0x25,
                0x6b, 0xe4, 0x30, 0x80, 0xf0, 0x3e, 0xc5, 0x50,
                0xa4, 0xb5, 0x64, 0x45, 0x87, 0x0a, 0xd4, 0x30,
                0x31, 0x04, 0xcd, 0x41, 0xe3, 0x69, 0xe6, 0xcc
            },                                                  // Extreme Code smell
            PasswordSalt = new byte[] {0},
            Role = AuthRole.SeedUser,
            CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
        };

        modelBuilder.Entity<User>().HasData(initUser);

        
    }
}

