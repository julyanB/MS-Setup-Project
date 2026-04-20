using EmployeeManagementService.Infrastructure.Identity.UserData;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EmployeeManagementService.Infrastructure.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder
            .HasDiscriminator(u => u.IsExternal)
            .HasValue<User>(false)
            .HasValue<ExternalUser>(true);

        builder
            .Property(u => u.IsExternal)
            .HasDefaultValue(false);
    }
}
