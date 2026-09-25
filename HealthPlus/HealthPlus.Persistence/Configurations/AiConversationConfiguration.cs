using HealthPlus.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HealthPlus.Persistence.Configurations;

public class AiConversationConfiguration : IEntityTypeConfiguration<AiConversation>
{
    public void Configure(EntityTypeBuilder<AiConversation> builder)
    {
        builder.ToTable("AiConversations");
        builder.HasKey(c => c.Id);
        builder.HasIndex(c => c.UserId);
        builder.Property(c => c.Title).HasMaxLength(200);

        builder.HasOne(c => c.User)
            .WithMany()
            .HasForeignKey(c => c.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class AiMessageConfiguration : IEntityTypeConfiguration<AiMessage>
{
    public void Configure(EntityTypeBuilder<AiMessage> builder)
    {
        builder.ToTable("AiMessages");
        builder.HasKey(m => m.Id);
        builder.HasIndex(m => m.ConversationId);
        builder.Property(m => m.Role).IsRequired().HasMaxLength(20);
        builder.Property(m => m.Content).IsRequired().HasMaxLength(4000);

        builder.HasOne(m => m.Conversation)
            .WithMany(c => c.Messages)
            .HasForeignKey(m => m.ConversationId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
