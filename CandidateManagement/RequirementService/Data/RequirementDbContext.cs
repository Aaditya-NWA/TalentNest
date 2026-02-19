using Microsoft.EntityFrameworkCore;
using RequirementService.Models;
using System.Diagnostics.CodeAnalysis;

namespace RequirementService.Data
{
    [ExcludeFromCodeCoverage]
    public class RequirementDbContext : DbContext
    {
        public RequirementDbContext(DbContextOptions<RequirementDbContext> options)
            : base(options) { }

        public DbSet<Requirement> Requirements { get; set; }
    }

}
