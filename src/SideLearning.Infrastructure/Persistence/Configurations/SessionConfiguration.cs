using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SideLearning.Domain.Sessions;
using SideLearning.Domain.Users;

namespace SideLearning.Infrastructure.Persistence.Configurations;

public sealed class SessionConfiguration : IEntityTypeConfiguration<Session>
{
    public void Configure(EntityTypeBuilder<Session> builder)
    {
        builder.ToTable("sessions");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.UserId).IsRequired();
        builder.Property(x => x.Title).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Summary).HasMaxLength(2000).IsRequired();
        builder.Property(x => x.Goal).HasMaxLength(2000).IsRequired();
        builder.Property(x => x.EstimatedDurationInMinutes);
        builder.Property(x => x.CreatedAtUtc).IsRequired();
        builder.Property(x => x.UpdatedAtUtc).IsRequired();
        builder.Property(x => x.StartedAtUtc);
        builder.Property(x => x.CompletedAtUtc);

        builder.Property(x => x.Status)
            .HasConversion<int>()
            .IsRequired();

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.OwnsOne(x => x.Context, context =>
        {
            context.Property(x => x.Explanation)
                .HasColumnName("ContextExplanation")
                .HasMaxLength(4000)
                .IsRequired();

            context.Property(x => x.WhyItMatters)
                .HasColumnName("ContextWhyItMatters")
                .HasMaxLength(4000)
                .IsRequired();

            context.Property(x => x.YoutubeUrl)
                .HasColumnName("ContextYoutubeUrl")
                .HasMaxLength(2048);

            context.Property(x => x.AdditionalResources)
                .HasColumnName("ContextAdditionalResources")
                .HasMaxLength(4000);
        });
        builder.Navigation(x => x.Context).IsRequired();

        builder.OwnsOne(x => x.HandsOn, handsOn =>
        {
            handsOn.Property(x => x.Instructions)
                .HasColumnName("HandsOnInstructions")
                .HasMaxLength(8000)
                .IsRequired();

            handsOn.Property(x => x.ExpectedOutput)
                .HasColumnName("HandsOnExpectedOutput")
                .HasMaxLength(4000);
        });
        builder.Navigation(x => x.HandsOn).IsRequired();

        builder.OwnsOne(x => x.Reflection, reflection =>
        {
            reflection.Property(x => x.Solution)
                .HasColumnName("ReflectionSolution")
                .HasMaxLength(8000);

            reflection.Property(x => x.Reflection)
                .HasColumnName("ReflectionText")
                .HasMaxLength(8000);

            reflection.Property(x => x.Notes)
                .HasColumnName("ReflectionNotes")
                .HasMaxLength(8000);

            reflection.Property(x => x.DifficultyFeedback)
                .HasColumnName("ReflectionDifficultyFeedback")
                .HasConversion<int?>();
        });
        builder.Navigation(x => x.Reflection).IsRequired();

        builder.Metadata.FindNavigation(nameof(Session.Topics))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);

        builder.OwnsMany(x => x.Topics, topics =>
        {
            topics.ToTable("session_topics");
            topics.WithOwner().HasForeignKey("SessionId");
            topics.Property<int>("Id");
            topics.HasKey("Id");

            topics.Property(x => x.Value)
                .HasColumnName("Topic")
                .HasMaxLength(300)
                .IsRequired();
        });

        builder.Ignore(x => x.DomainEvents);
    }
}
