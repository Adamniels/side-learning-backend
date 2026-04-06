using Microsoft.AspNetCore.Identity;

namespace SideLearning.Infrastructure.Identity;

public sealed class ApplicationUser : IdentityUser<Guid>
{
    public string? DisplayName { get; set; }
}
