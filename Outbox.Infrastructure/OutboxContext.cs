using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Outbox.Core;

namespace Outbox.Infrastructure;

public class OutboxContext : DbContext
{
    private string DbPath { get; }
    
    public OutboxContext()
    {
        const Environment.SpecialFolder folder = Environment.SpecialFolder.LocalApplicationData;
        var path = Environment.GetFolderPath(folder);
        DbPath = System.IO.Path.Join(path, "outbox.db");
    }
    
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite($"Data Source={DbPath}");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new MessageTypeConfiguration());
        modelBuilder.ApplyConfiguration(new PartitionTypeConfiguration());
    }
}

public class MessageTypeConfiguration : IEntityTypeConfiguration<Message>
{
    public void Configure(EntityTypeBuilder<Message> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.PartitionKey);
        builder.Property(x => x.Content).IsRequired();
        builder.Property(x => x.DeliveryStatus);
        builder.Property(x => x.RetryCount);
        builder.Property(x => x.ReceivedDateTime);
        builder.Property(x => x.PublishedDateTime);
    }
}

public class PartitionTypeConfiguration : IEntityTypeConfiguration<Partition>
{
    public void Configure(EntityTypeBuilder<Partition> builder)
    {
        builder.HasKey(x => x.Id);
        builder.HasIndex(x => x.PartitionKey).IsUnique();
    }
}