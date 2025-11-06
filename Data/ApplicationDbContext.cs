using Microsoft.AspNetCore.SignalR.Protocol;
using Microsoft.EntityFrameworkCore;
using stratoapi.Models;
using stratoapi.Services; //TODO: Ikke hardcode users

namespace stratoapi.Data;

public class ApplicationDbContext : DbContext
{
    private readonly AuthService _authService; //TODO: Ikke hardcode users
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
        _authService = new AuthService(new ConfigurationBuilder().Build(), this); //TODO: Ikke hardcode users
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
        
        AuthService.PasswordHashAndSalt hashAndSalt = _authService.HashPassword("clanker"); //TODO: Ikke hardcode users
        
        // Seed admin user with password
        var initUser = new User
        {
            Id = 1,
            Username = "seedUser",
            PasswordHash = hashAndSalt.PasswordHash,
            PasswordSalt = hashAndSalt.PasswordSalt,
            Role = AuthRole.SeedUser,
            CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
        };

        modelBuilder.Entity<User>().HasData(initUser);

        
    }
}

