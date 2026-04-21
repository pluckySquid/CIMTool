public class SampleProfileDbContext : SampleProfile.DbContextBase
{
    public SampleProfileDbContext(DbContextOptions<SampleProfileDbContext> options)
        : base(options) { }
}