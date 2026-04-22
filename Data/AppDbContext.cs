using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using PokeBuilder.Server.Models;

namespace PokeBuilder.Server.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }
    public DbSet<Team> Teams { get; set; }

    // ── Game data ─────────────────────────────────────────────────────────────
    public DbSet<Game> Games { get; set; }
    public DbSet<PokemonEntry> Pokemon { get; set; }
    public DbSet<GameDexEntry> GameDex { get; set; }
    public DbSet<GamePokemonInfo> GamePokemonInfo { get; set; }
    public DbSet<Learnset> Learnsets { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Email).IsUnique();
            entity.Property(e => e.Email).IsRequired().HasMaxLength(255);
            entity.HasIndex(e => e.Username).IsUnique();
            entity.Property(e => e.Username).IsRequired().HasMaxLength(20);
            entity.Property(e => e.PasswordHash).IsRequired();
            entity.Property(e => e.AccessFailedCount).HasDefaultValue(0);
        });

        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.TokenHash);
            entity.Property(e => e.TokenHash).IsRequired().HasMaxLength(64);
            entity.Property(e => e.UserAgent).HasMaxLength(512);
            entity.Property(e => e.CreatedIp).HasMaxLength(45);

            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Team>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.GameKey).IsRequired().HasMaxLength(50);

            // Store PokemonIds as a JSONB column in PostgreSQL
            entity.Property(e => e.PokemonIds)
                .HasColumnType("jsonb")
                .IsRequired()
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                    v => JsonSerializer.Deserialize<int?[]>(v, (JsonSerializerOptions?)null) ?? new int?[6]
                )
                .Metadata.SetValueComparer(
                    new Microsoft.EntityFrameworkCore.ChangeTracking.ValueComparer<int?[]>(
                        (a, b) => (a == null && b == null) || (a != null && b != null && a.SequenceEqual(b)),
                        v => v.Aggregate(0, (acc, x) => HashCode.Combine(acc, x.GetHashCode())),
                        v => v.ToArray()
                    )
                );

            entity.HasOne<User>()
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ── Game ──────────────────────────────────────────────────────────────
        modelBuilder.Entity<Game>(entity =>
        {
            entity.HasKey(e => e.Key);
            entity.Property(e => e.Key).HasMaxLength(50);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
        });

        // ── PokemonEntry ──────────────────────────────────────────────────────
        modelBuilder.Entity<PokemonEntry>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);

            var stringArrayComparer = new Microsoft.EntityFrameworkCore.ChangeTracking.ValueComparer<string[]>(
                (a, b) => (a == null && b == null) || (a != null && b != null && a.SequenceEqual(b)),
                v => v.Aggregate(0, (acc, x) => HashCode.Combine(acc, x.GetHashCode())),
                v => v.ToArray()
            );

            entity.Property(e => e.Types)
                .HasColumnType("jsonb")
                .IsRequired()
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                    v => JsonSerializer.Deserialize<string[]>(v, (JsonSerializerOptions?)null) ?? Array.Empty<string>()
                )
                .Metadata.SetValueComparer(stringArrayComparer);
        });

        // ── GameDexEntry ──────────────────────────────────────────────────────
        modelBuilder.Entity<GameDexEntry>(entity =>
        {
            entity.HasKey(e => new { e.GameKey, e.PokemonId });
            entity.HasOne(e => e.Game).WithMany(g => g.DexEntries).HasForeignKey(e => e.GameKey);
            entity.HasOne(e => e.Pokemon).WithMany(p => p.DexEntries).HasForeignKey(e => e.PokemonId);
        });

        // ── GamePokemonInfo ───────────────────────────────────────────────────
        modelBuilder.Entity<GamePokemonInfo>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.GameKey, e.PokemonId }).IsUnique();
            entity.HasOne(e => e.Game).WithMany().HasForeignKey(e => e.GameKey);
            entity.HasOne(e => e.Pokemon).WithMany(p => p.GameDetails).HasForeignKey(e => e.PokemonId);

            var stringArrayComparer = new Microsoft.EntityFrameworkCore.ChangeTracking.ValueComparer<string[]>(
                (a, b) => (a == null && b == null) || (a != null && b != null && a.SequenceEqual(b)),
                v => v.Aggregate(0, (acc, x) => HashCode.Combine(acc, x.GetHashCode())),
                v => v.ToArray()
            );

            entity.Property(e => e.ObtainMethods)
                .HasColumnType("jsonb").IsRequired()
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                    v => JsonSerializer.Deserialize<string[]>(v, (JsonSerializerOptions?)null) ?? Array.Empty<string>()
                )
                .Metadata.SetValueComparer(stringArrayComparer);

            entity.Property(e => e.Locations)
                .HasColumnType("jsonb").IsRequired()
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                    v => JsonSerializer.Deserialize<string[]>(v, (JsonSerializerOptions?)null) ?? Array.Empty<string>()
                )
                .Metadata.SetValueComparer(stringArrayComparer);
        });

        // ── Learnset ──────────────────────────────────────────────────────────
        modelBuilder.Entity<Learnset>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.MoveName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.LearnMethod).IsRequired().HasMaxLength(20);
            entity.HasOne(e => e.Game).WithMany().HasForeignKey(e => e.GameKey);
            entity.HasOne(e => e.Pokemon).WithMany(p => p.Learnsets).HasForeignKey(e => e.PokemonId);
            entity.HasIndex(e => new { e.GameKey, e.PokemonId, e.LearnMethod });
        });
    }
}
