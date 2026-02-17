using Microsoft.EntityFrameworkCore;
using RequirementService.Models;

namespace RequirementService.Data
{
    public class RequirementDbContext : DbContext
    {
        public RequirementDbContext(DbContextOptions<RequirementDbContext> options)
            : base(options) { }

        public DbSet<Requirement> Requirements { get; set; }
    }
}
