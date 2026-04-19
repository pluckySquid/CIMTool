using Microsoft.EntityFrameworkCore;

public sealed class GeneratedOnlySampleProfileDbContext : DbContext
{
    public DbSet<SampleProfile.Organisation> Organisations => Set<SampleProfile.Organisation>();
    public DbSet<SampleProfile.IdentifiedObject> IdentifiedObjects => Set<SampleProfile.IdentifiedObject>();
    public DbSet<SampleProfile.ParentOrganization> ParentOrganizations => Set<SampleProfile.ParentOrganization>();
    public DbSet<SampleProfile.ElectronicAddress> ElectronicAddresses => Set<SampleProfile.ElectronicAddress>();
    public DbSet<SampleProfile.TelephoneNumber> TelephoneNumbers => Set<SampleProfile.TelephoneNumber>();
    public DbSet<SampleProfile.StreetAddress> StreetAddresses => Set<SampleProfile.StreetAddress>();

    public GeneratedOnlySampleProfileDbContext(DbContextOptions<GeneratedOnlySampleProfileDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        SampleProfile.ModelConfiguration.ConfigureModel(modelBuilder);
    }
}
