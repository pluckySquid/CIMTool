using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

internal static class SampleProfileCompoundCleanup
{
    public static int SaveChangesWithCleanup(SampleProfileDbContext context, Func<int> baseSaveChanges)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(baseSaveChanges);

        context.ChangeTracker.DetectChanges();

        var candidates = CollectCleanupCandidates(context);
        var rows = baseSaveChanges();

        if (candidates.Count == 0)
        {
            return rows;
        }

        if (!MarkOrphanedCompoundsForDeletion(context, candidates))
        {
            return rows;
        }

        rows += baseSaveChanges();
        return rows;
    }

    public static async Task<int> SaveChangesWithCleanupAsync(
        SampleProfileDbContext context,
        Func<CancellationToken, Task<int>> baseSaveChangesAsync,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(baseSaveChangesAsync);

        context.ChangeTracker.DetectChanges();

        var candidates = CollectCleanupCandidates(context);
        var rows = await baseSaveChangesAsync(cancellationToken);

        if (candidates.Count == 0)
        {
            return rows;
        }

        if (!MarkOrphanedCompoundsForDeletion(context, candidates))
        {
            return rows;
        }

        rows += await baseSaveChangesAsync(cancellationToken);
        return rows;
    }

    private static List<CompoundCleanupCandidate> CollectCleanupCandidates(SampleProfileDbContext context)
    {
        var candidates = new List<CompoundCleanupCandidate>();

        foreach (var entry in context.ChangeTracker.Entries<SampleProfile.Organisation>())
        {
            CollectChangedCompound(entry, nameof(SampleProfile.Organisation.ElectronicAddressId), typeof(SampleProfile.ElectronicAddress), candidates);
            CollectChangedCompound(entry, nameof(SampleProfile.Organisation.Phone1Id), typeof(SampleProfile.TelephoneNumber), candidates);
            CollectChangedCompound(entry, nameof(SampleProfile.Organisation.Phone2Id), typeof(SampleProfile.TelephoneNumber), candidates);
            CollectChangedCompound(entry, nameof(SampleProfile.Organisation.PostalAddressId), typeof(SampleProfile.StreetAddress), candidates);
            CollectChangedCompound(entry, nameof(SampleProfile.Organisation.StreetAddressId), typeof(SampleProfile.StreetAddress), candidates);
        }

        foreach (var entry in context.ChangeTracker.Entries<SampleProfile.StreetAddress>())
        {
            CollectChangedCompound(entry, nameof(SampleProfile.StreetAddress.StatusId), typeof(SampleProfile.Status), candidates);
            CollectChangedCompound(entry, nameof(SampleProfile.StreetAddress.StreetDetailId), typeof(SampleProfile.StreetDetail), candidates);
            CollectChangedCompound(entry, nameof(SampleProfile.StreetAddress.TownDetailId), typeof(SampleProfile.TownDetail), candidates);
        }

        return candidates;
    }

    private static void CollectChangedCompound<TEntity>(
        EntityEntry<TEntity> entry,
        string propertyName,
        Type compoundType,
        List<CompoundCleanupCandidate> candidates)
        where TEntity : class
    {
        if (entry.State is not EntityState.Modified and not EntityState.Deleted)
        {
            return;
        }

        var originalId = entry.OriginalValues[propertyName] as string;
        var currentId = entry.State == EntityState.Deleted
            ? null
            : entry.CurrentValues[propertyName] as string;

        if (!string.IsNullOrWhiteSpace(originalId) &&
            !string.Equals(originalId, currentId, StringComparison.Ordinal))
        {
            candidates.Add(new CompoundCleanupCandidate(compoundType, originalId));
        }
    }

    private static bool MarkOrphanedCompoundsForDeletion(
        SampleProfileDbContext context,
        IReadOnlyCollection<CompoundCleanupCandidate> candidates)
    {
        var plannedDeletes = new HashSet<CompoundCleanupCandidate>();
        var visiting = new HashSet<CompoundCleanupCandidate>();

        foreach (var candidate in candidates)
        {
            PlanCompoundDeletion(context, candidate, plannedDeletes, visiting);
        }

        var deletedAny = false;

        foreach (var candidate in plannedDeletes)
        {
            deletedAny |= MarkCompoundForDeletion(context, candidate);
        }

        return deletedAny;
    }

    private static void PlanCompoundDeletion(
        SampleProfileDbContext context,
        CompoundCleanupCandidate candidate,
        HashSet<CompoundCleanupCandidate> plannedDeletes,
        HashSet<CompoundCleanupCandidate> visiting)
    {
        if (string.IsNullOrWhiteSpace(candidate.Id) ||
            plannedDeletes.Contains(candidate) ||
            !visiting.Add(candidate))
        {
            return;
        }

        if (IsStillReferenced(context, candidate, plannedDeletes))
        {
            visiting.Remove(candidate);
            return;
        }

        plannedDeletes.Add(candidate);

        foreach (var child in GetNestedCompoundCandidates(context, candidate))
        {
            PlanCompoundDeletion(context, child, plannedDeletes, visiting);
        }

        visiting.Remove(candidate);
    }

    private static bool IsStillReferenced(
        SampleProfileDbContext context,
        CompoundCleanupCandidate candidate,
        HashSet<CompoundCleanupCandidate> plannedDeletes)
    {
        if (candidate.CompoundType == typeof(SampleProfile.ElectronicAddress))
        {
            return context.Organisations.Any(x => x.ElectronicAddressId == candidate.Id);
        }

        if (candidate.CompoundType == typeof(SampleProfile.TelephoneNumber))
        {
            return context.Organisations.Any(x => x.Phone1Id == candidate.Id || x.Phone2Id == candidate.Id);
        }

        if (candidate.CompoundType == typeof(SampleProfile.StreetAddress))
        {
            return context.Organisations.Any(x => x.PostalAddressId == candidate.Id || x.StreetAddressId == candidate.Id);
        }

        if (candidate.CompoundType == typeof(SampleProfile.Status))
        {
            return context.Set<SampleProfile.StreetAddress>()
                .Where(x => x.StatusId == candidate.Id)
                .Select(x => x.Id)
                .AsEnumerable()
                .Any(id => !plannedDeletes.Contains(new CompoundCleanupCandidate(typeof(SampleProfile.StreetAddress), id)));
        }

        if (candidate.CompoundType == typeof(SampleProfile.StreetDetail))
        {
            return context.Set<SampleProfile.StreetAddress>()
                .Where(x => x.StreetDetailId == candidate.Id)
                .Select(x => x.Id)
                .AsEnumerable()
                .Any(id => !plannedDeletes.Contains(new CompoundCleanupCandidate(typeof(SampleProfile.StreetAddress), id)));
        }

        if (candidate.CompoundType == typeof(SampleProfile.TownDetail))
        {
            return context.Set<SampleProfile.StreetAddress>()
                .Where(x => x.TownDetailId == candidate.Id)
                .Select(x => x.Id)
                .AsEnumerable()
                .Any(id => !plannedDeletes.Contains(new CompoundCleanupCandidate(typeof(SampleProfile.StreetAddress), id)));
        }

        return false;
    }

    private static IEnumerable<CompoundCleanupCandidate> GetNestedCompoundCandidates(
        SampleProfileDbContext context,
        CompoundCleanupCandidate candidate)
    {
        if (candidate.CompoundType != typeof(SampleProfile.StreetAddress))
        {
            return Array.Empty<CompoundCleanupCandidate>();
        }

        var address = context.Set<SampleProfile.StreetAddress>()
            .SingleOrDefault(x => x.Id == candidate.Id);

        if (address is null)
        {
            return Array.Empty<CompoundCleanupCandidate>();
        }

        var children = new List<CompoundCleanupCandidate>();

        AddIfPresent(children, typeof(SampleProfile.Status), address.StatusId);
        AddIfPresent(children, typeof(SampleProfile.StreetDetail), address.StreetDetailId);
        AddIfPresent(children, typeof(SampleProfile.TownDetail), address.TownDetailId);

        return children;
    }

    private static void AddIfPresent(List<CompoundCleanupCandidate> children, Type type, string? id)
    {
        if (!string.IsNullOrWhiteSpace(id))
        {
            children.Add(new CompoundCleanupCandidate(type, id));
        }
    }

    private static bool MarkCompoundForDeletion(SampleProfileDbContext context, CompoundCleanupCandidate candidate)
    {
        if (candidate.CompoundType == typeof(SampleProfile.ElectronicAddress))
        {
            return DeleteIfPresent(context.Set<SampleProfile.ElectronicAddress>(), x => x.Id == candidate.Id, context);
        }

        if (candidate.CompoundType == typeof(SampleProfile.TelephoneNumber))
        {
            return DeleteIfPresent(context.Set<SampleProfile.TelephoneNumber>(), x => x.Id == candidate.Id, context);
        }

        if (candidate.CompoundType == typeof(SampleProfile.StreetAddress))
        {
            return DeleteIfPresent(context.Set<SampleProfile.StreetAddress>(), x => x.Id == candidate.Id, context);
        }

        if (candidate.CompoundType == typeof(SampleProfile.Status))
        {
            return DeleteIfPresent(context.Set<SampleProfile.Status>(), x => x.Id == candidate.Id, context);
        }

        if (candidate.CompoundType == typeof(SampleProfile.StreetDetail))
        {
            return DeleteIfPresent(context.Set<SampleProfile.StreetDetail>(), x => x.Id == candidate.Id, context);
        }

        if (candidate.CompoundType == typeof(SampleProfile.TownDetail))
        {
            return DeleteIfPresent(context.Set<SampleProfile.TownDetail>(), x => x.Id == candidate.Id, context);
        }

        return false;
    }

    private static bool DeleteIfPresent<TEntity>(
        DbSet<TEntity> set,
        Func<TEntity, bool> predicate,
        DbContext context)
        where TEntity : class
    {
        var entity = set.Local.FirstOrDefault(predicate) ?? set.AsEnumerable().FirstOrDefault(predicate);
        if (entity is null)
        {
            return false;
        }

        context.Remove(entity);
        return true;
    }

    private readonly record struct CompoundCleanupCandidate(Type CompoundType, string Id);
}
