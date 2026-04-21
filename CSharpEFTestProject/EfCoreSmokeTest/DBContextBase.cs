// Generated — do not edit. Part of SampleProfile.cs
public abstract class SampleProfileDbContextBase : DbContext
{
    // Generated DbSet properties
    public DbSet<SampleProfile.Organisation> Organisations 
        => Set<SampleProfile.Organisation>();
    // ... other DbSets ...

    protected override void OnModelCreating(ModelBuilder modelBuilder)
        => SampleProfile.ModelConfiguration.ConfigureModel(modelBuilder);

    public override int SaveChanges()
    {
        var orphans = CollectCompoundOrphans();
        var rows = base.SaveChanges();
        if (orphans.Count == 0) return rows;
        DeleteOrphanedCompounds(orphans);
        return rows + base.SaveChanges();
    }

    // ... SaveChangesAsync override, same pattern ...

    private List<(Type Type, string Id)> CollectCompoundOrphans()
    {
        ChangeTracker.DetectChanges();
        var orphans = new List<(Type, string)>();

        foreach (var entry in ChangeTracker.Entries<SampleProfile.Organisation>())
        {
            if (entry.State is not EntityState.Modified 
                           and not EntityState.Deleted) continue;

            // Generated per a:Compound child of Organisation
            CollectOrphan(entry, nameof(SampleProfile.Organisation.ElectronicAddressId), 
                          typeof(SampleProfile.ElectronicAddress), orphans);
            CollectOrphan(entry, nameof(SampleProfile.Organisation.Phone1Id),           
                          typeof(SampleProfile.TelephoneNumber), orphans);
            CollectOrphan(entry, nameof(SampleProfile.Organisation.Phone2Id),           
                          typeof(SampleProfile.TelephoneNumber), orphans);
            CollectOrphan(entry, nameof(SampleProfile.Organisation.PostalAddressId),    
                          typeof(SampleProfile.StreetAddress), orphans);
            CollectOrphan(entry, nameof(SampleProfile.Organisation.StreetAddressId),    
                          typeof(SampleProfile.StreetAddress), orphans);
        }

        // Generated per a:Compound child of StreetAddress (nested compound hierarchy)
        foreach (var entry in ChangeTracker.Entries<SampleProfile.StreetAddress>())
        {
            if (entry.State is not EntityState.Modified 
                           and not EntityState.Deleted) continue;

            CollectOrphan(entry, nameof(SampleProfile.StreetAddress.StatusId),      
                          typeof(SampleProfile.Status), orphans);
            CollectOrphan(entry, nameof(SampleProfile.StreetAddress.StreetDetailId),
                          typeof(SampleProfile.StreetDetail), orphans);
            CollectOrphan(entry, nameof(SampleProfile.StreetAddress.TownDetailId),  
                          typeof(SampleProfile.TownDetail), orphans);
        }

        return orphans;
    }

    private void DeleteOrphanedCompounds(List<(Type Type, string Id)> orphans)
    {
        foreach (var (type, id) in orphans)
        {
            // Generated per compound type
            if (type == typeof(SampleProfile.ElectronicAddress))
                DeleteCompound(Set<SampleProfile.ElectronicAddress>(), 
                               x => x.Id == id);
            else if (type == typeof(SampleProfile.TelephoneNumber))
                DeleteCompound(Set<SampleProfile.TelephoneNumber>(),   
                               x => x.Id == id);
            else if (type == typeof(SampleProfile.StreetAddress))
                DeleteCompound(Set<SampleProfile.StreetAddress>(),     
                               x => x.Id == id);
            else if (type == typeof(SampleProfile.Status))
                DeleteCompound(Set<SampleProfile.Status>(),            
                               x => x.Id == id);
            else if (type == typeof(SampleProfile.StreetDetail))
                DeleteCompound(Set<SampleProfile.StreetDetail>(),      
                               x => x.Id == id);
            else if (type == typeof(SampleProfile.TownDetail))
                DeleteCompound(Set<SampleProfile.TownDetail>(),        
                               x => x.Id == id);
        }
    }

    // Shared helper — not generated, part of the base infrastructure
    private static void CollectOrphan<TEntity>(
        EntityEntry<TEntity> entry, string propertyName, 
        Type compoundType, List<(Type, string)> orphans)
        where TEntity : class
    {
        var originalId = entry.OriginalValues[propertyName] as string;
        var currentId  = entry.State == EntityState.Deleted ? null
                       : entry.CurrentValues[propertyName] as string;
        if (!string.IsNullOrWhiteSpace(originalId) && originalId != currentId)
            orphans.Add((compoundType, originalId));
    }

    private void DeleteCompound<TEntity>(
        DbSet<TEntity> set, Expression<Func<TEntity, bool>> predicate)
        where TEntity : class
    {
        var entity = set.Local.FirstOrDefault(predicate.Compile()) 
                  ?? set.AsEnumerable().FirstOrDefault(predicate.Compile());
        if (entity is not null) Remove(entity);
    }
}