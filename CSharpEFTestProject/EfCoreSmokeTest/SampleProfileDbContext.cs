using Microsoft.EntityFrameworkCore;

public sealed class SampleProfileDbContext : DbContext
{
    public DbSet<SampleProfile.Organisation> Organisations => Set<SampleProfile.Organisation>();
    public DbSet<SampleProfile.IdentifiedObject> IdentifiedObjects => Set<SampleProfile.IdentifiedObject>();
    public DbSet<SampleProfile.ParentOrganization> ParentOrganizations => Set<SampleProfile.ParentOrganization>();
    public DbSet<SampleProfile.ElectronicAddress> ElectronicAddresses => Set<SampleProfile.ElectronicAddress>();
    public DbSet<SampleProfile.TelephoneNumber> TelephoneNumbers => Set<SampleProfile.TelephoneNumber>();
    public DbSet<SampleProfile.StreetAddress> StreetAddresses => Set<SampleProfile.StreetAddress>();

    public SampleProfileDbContext(DbContextOptions<SampleProfileDbContext> options)
        : base(options)
    {
    }

    public override int SaveChanges()
        => SampleProfileCompoundCleanup.SaveChangesWithCleanup(this, () => base.SaveChanges());

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
        => SampleProfileCompoundCleanup.SaveChangesWithCleanup(this, () => base.SaveChanges(acceptAllChangesOnSuccess));

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => SampleProfileCompoundCleanup.SaveChangesWithCleanupAsync(
            this,
            ct => base.SaveChangesAsync(ct),
            cancellationToken);

    public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
        => SampleProfileCompoundCleanup.SaveChangesWithCleanupAsync(
            this,
            ct => base.SaveChangesAsync(acceptAllChangesOnSuccess, ct),
            cancellationToken);

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        SampleProfile.ModelConfiguration.ConfigureModel(modelBuilder);
    }
}
