using Microsoft.EntityFrameworkCore;
using Minimal.Api.Authentication.Database.Entities;
using Minimal.Api.Shared.Settings;
namespace Minimal.Api.Authentication.Database;
public class UsersDbContext : DbContext
{
    private readonly JwtParameters _jwtParameters;
    public DbSet<User> Users { get; set; }
    public DbSet<UserRefreshToken> UsersRefreshTokens { get; set; }
    public DbSet<UserLoginAttempt> UsersLoginAttempts { get; set; }
    public UsersDbContext(DbContextOptions<UsersDbContext> options, JwtParameters jwtParameters) : base(options) =>
        _jwtParameters = jwtParameters;
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Email)
                  .IsRequired()
                  .HasMaxLength(100);

            entity.Property(e => e.PasswordHash)
                  .IsRequired();

            entity.Property(e => e.Active)
                  .HasDefaultValue(true);

            entity.HasOne(e => e.RefreshToken)
                  .WithOne()
                  .HasForeignKey<UserRefreshToken>(rt => rt.UserId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(i => i.Email);
        });

        modelBuilder.Entity<UserRefreshToken>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Token)
                  .IsRequired()
                  .HasMaxLength(_jwtParameters.RefreshTokenLength);

            entity.HasOne(rt => rt.User)
                  .WithOne(u => u.RefreshToken)
                  .HasForeignKey<UserRefreshToken>(rt => rt.UserId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(i => i.UserId);
        });

        modelBuilder.Entity<UserLoginAttempt>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Username)
                  .IsRequired()
                  .HasMaxLength(100);

            entity.Property(e => e.IpAddress)
                  .IsRequired()
                  .HasMaxLength(39);

            entity.Property(e => e.Modified)
                  .IsRequired()
                  .HasDefaultValue(DateTime.Now);

            entity.Property(e => e.AttemptsCount)
                  .IsRequired();

            entity.HasIndex(i => i.Username).IncludeProperties(p => p.IpAddress);
            entity.HasIndex(i => i.IpAddress);
        });
    }
}