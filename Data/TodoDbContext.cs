using Microsoft.EntityFrameworkCore;
using TodoApi.Models;

namespace TodoApi.Data;

/// <summary>
/// EF Core database context for Todo data.
/// </summary>
public class TodoDbContext(DbContextOptions<TodoDbContext> options) : DbContext(options)
{
    public DbSet<Todo> Todos => Set<Todo>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var todo = modelBuilder.Entity<Todo>();
        todo.ToTable("Todos");

        todo.Property(t => t.Name)
            .IsRequired()
            .HasMaxLength(200);

        todo.Property(t => t.IsComplete)
            .HasDefaultValue(false);

        // Default to UTC timestamp for sqlite.
        todo.Property(t => t.CreatedAtUtc)
            .HasDefaultValueSql("CURRENT_TIMESTAMP");
    }
}
