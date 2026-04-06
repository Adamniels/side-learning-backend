using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SideLearning.Domain.Users;
using SideLearning.Infrastructure.Identity;

namespace SideLearning.Infrastructure.Persistence.Configurations;

public sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("domain_users");
        builder.HasKey(x => x.Id);

        builder.OwnsOne(x => x.Email, email =>
        {
            email.Property(x => x.Value)
                .HasColumnName("Email")
                .HasMaxLength(256)
                .IsRequired();

            email.Property(x => x.NormalizedValue)
                .HasColumnName("NormalizedEmail")
                .HasMaxLength(256)
                .IsRequired();

            email.HasIndex(x => x.NormalizedValue)
                .IsUnique();
        });
        builder.Navigation(x => x.Email).IsRequired();

        builder.Property(x => x.DisplayName)
            .HasMaxLength(120)
            .IsRequired();

        builder.Ignore(x => x.DomainEvents);

        builder.HasOne<ApplicationUser>()
            .WithOne()
            .HasForeignKey<User>(x => x.Id)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
