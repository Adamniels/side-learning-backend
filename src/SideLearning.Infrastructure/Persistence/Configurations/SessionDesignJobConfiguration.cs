using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SideLearning.Domain.SessionDesign;
using SideLearning.Domain.Users;

namespace SideLearning.Infrastructure.Persistence.Configurations;

public sealed class SessionDesignJobConfiguration : IEntityTypeConfiguration<SessionDesignJob>
{
    public void Configure(EntityTypeBuilder<SessionDesignJob> builder)
    {
        builder.ToTable("session_design_jobs");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.UserId).IsRequired();
        builder.Property(x => x.Status)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(x => x.CreatedAtUtc).IsRequired();
        builder.Property(x => x.StartedAtUtc);
        builder.Property(x => x.CompletedAtUtc);

        builder.Property(x => x.ResultJson);
        builder.Property(x => x.ErrorCode).HasMaxLength(200);
        builder.Property(x => x.ErrorMessage).HasMaxLength(4000);

        builder.Property(x => x.CreatedSessionId);

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Ignore(x => x.DomainEvents);
    }
}
