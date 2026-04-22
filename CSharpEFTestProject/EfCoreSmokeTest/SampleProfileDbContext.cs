using Microsoft.EntityFrameworkCore;

public sealed class SampleProfileDbContext : SampleProfile.DbContextBase
{
    public SampleProfileDbContext(DbContextOptions<SampleProfileDbContext> options)
        : base(options)
    {
    }
}
