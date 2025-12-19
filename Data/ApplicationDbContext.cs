using Microsoft.EntityFrameworkCore;
using stratoapi.Models;
using System.Security.Cryptography;
using System.Security.Claims;
using System.Text;

namespace stratoapi.Data;

public class ApplicationDbContext : DbContext
{
    private readonly IHttpContextAccessor? _httpContextAccessor;

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, IHttpContextAccessor? httpContextAccessor = null) : base(options)
    {
        _httpContextAccessor = httpContextAccessor;
    }
    
    // Parameterless constructor for migrations
    public ApplicationDbContext()
    {
    }
    
    public DbSet<User> Users { get; set; }
    public DbSet<MetricType> MetricTypes { get; set; }
    public DbSet<Cluster> Clusters { get; set; }

    /// <summary>
    /// Gets the current authenticated user's ID from the HTTP context.
    /// </summary>
    /// <returns>The user ID if authenticated, otherwise null.</returns>
    private int? GetCurrentUserId()
    {
        var userIdClaim = _httpContextAccessor?.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
        {
            return userId;
        }
        return null;
    }

    /// <summary>
    /// Overrides SaveChangesAsync to automatically set audit fields (CreatedBy, UpdatedBy, CreatedAt, UpdatedAt)
    /// for all entities that inherit from BaseModel.
    /// </summary>
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        int? currentUserId = GetCurrentUserId();
        var entries = ChangeTracker.Entries<BaseModel>();

        foreach (var entry in entries)
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAt = DateTime.UtcNow;
                entry.Entity.CreatedBy = currentUserId;
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAt = DateTime.UtcNow;
                entry.Entity.UpdatedBy = currentUserId;
            }
        }

        return await base.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Overrides SaveChanges to automatically set audit fields (CreatedBy, UpdatedBy, CreatedAt, UpdatedAt)
    /// for all entities that inherit from BaseModel.
    /// </summary>
    public override int SaveChanges()
    {
        var currentUserId = GetCurrentUserId();
        var entries = ChangeTracker.Entries<BaseModel>();

        foreach (var entry in entries)
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAt = DateTime.UtcNow;
                entry.Entity.CreatedBy = currentUserId;
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAt = DateTime.UtcNow;
                entry.Entity.UpdatedBy = currentUserId;
            }
        }

        return base.SaveChanges();
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseNpgsql("Host=localhost;Database=stratoapi;Username=postgres;Password=postgres");
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // Configure User entity
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Username).IsUnique();
            entity.HasIndex(e => e.Email).IsUnique();
            entity.Property(e => e.Username).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(255);
            entity.Property(e => e.PasswordHash).IsRequired();
            entity.Property(e => e.PasswordSalt).IsRequired();
            entity.Property(e => e.Role).IsRequired();
            entity.Property(e => e.CreatedAt).IsRequired();
        });
        
        // Generate proper password hash and salt for seed user
        // Password: "p7-gr11-chaos-academy"
        using var hmac = new HMACSHA512();
        byte[] salt = hmac.Key;
        byte[] hash = hmac.ComputeHash(Encoding.UTF8.GetBytes("p7-gr11-chaos-academy"));
        
        var initUser = new User
        {
            Id = 1,
            Username = "seedUser",
            Email = "seed@example.com",
            PasswordHash = hash,
            PasswordSalt = salt,
            Role = AuthRole.SeedUser,
            CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
        };

        modelBuilder.Entity<User>().HasData(initUser);
        
        // Configure Metrics entity 
        modelBuilder.Entity<MetricType>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Description).IsRequired().HasMaxLength(500);
            entity.Property(e => e.PrometheusIdentifier).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Unit).IsRequired(false).HasMaxLength(50);
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.CreatedBy).IsRequired(false);
            entity.Property(e => e.UpdatedBy).IsRequired(false);
            entity.Property(e => e.UpdatedAt).IsRequired(false);
            entity.Property(e => e.IsDeleted).IsRequired();
            
            // Configure foreign key relationship for CreatedBy
            entity.HasOne<User>()
                .WithMany()
                .HasForeignKey(e => e.CreatedBy)
                .OnDelete(DeleteBehavior.SetNull)
                .IsRequired(false);
            
            // Configure foreign key relationship for UpdatedBy
            entity.HasOne<User>()
                .WithMany()
                .HasForeignKey(e => e.UpdatedBy)
                .OnDelete(DeleteBehavior.SetNull)
                .IsRequired(false);
        });
        
        // Configure Cluster entity
        modelBuilder.Entity<Cluster>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.ApiEndpoint).IsRequired().HasMaxLength(200);
            entity.Property(e => e.PrometheusEndpoint).IsRequired().HasMaxLength(200);
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.CreatedBy).IsRequired(false);
            entity.Property(e => e.UpdatedBy).IsRequired(false);
            entity.Property(e => e.UpdatedAt).IsRequired(false);    
            entity.Property(e => e.IsDeleted).IsRequired();
            
            // Configure foreign key relationship for UpdatedBy
            entity.HasOne<User>()
                .WithMany()
                .HasForeignKey(e => e.CreatedBy)
                .OnDelete(DeleteBehavior.SetNull)
                .IsRequired(false);
            entity.HasOne<User>()
                .WithMany()
                .HasForeignKey(e => e.UpdatedBy)
                .OnDelete(DeleteBehavior.SetNull)
                .IsRequired(false);
        });
    }
}
