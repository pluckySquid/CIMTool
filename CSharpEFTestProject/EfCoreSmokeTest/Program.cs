#if false
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

static void AssertCondition(bool condition, string message)
{
    if (!condition)
    {
        throw new InvalidOperationException(message);
    }
}

var connection = new SqliteConnection("Data Source=:memory:");
connection.Open();

var options = new DbContextOptionsBuilder<SampleProfileDbContext>()
    .UseSqlite(connection)
    .EnableSensitiveDataLogging()
    .Options;

using var context = new SampleProfileDbContext(options);

context.Database.EnsureDeleted();
context.Database.EnsureCreated();

var tableNames = context.Database
    .SqlQueryRaw<string>("SELECT name AS Value FROM sqlite_master WHERE type='table'")
    .ToList();

AssertCondition(tableNames.Contains("IdentifiedObject"), "Expected IdentifiedObject table to be created.");
AssertCondition(tableNames.Contains("Organisation"), "Expected Organisation table to be created.");
AssertCondition(tableNames.Contains("ParentOrganization"), "Expected ParentOrganization table to be created.");
AssertCondition(tableNames.Contains("ElectronicAddress"), "Expected ElectronicAddress table to be created.");
AssertCondition(tableNames.Contains("TelephoneNumber"), "Expected TelephoneNumber table to be created.");
AssertCondition(tableNames.Contains("StreetAddress"), "Expected StreetAddress table to be created.");

var lookupAssignedA = new SampleProfile.CrewStatusKind { Name = SampleProfile.CrewStatusKind.assigned };
var lookupAssignedB = new SampleProfile.CrewStatusKind { Name = SampleProfile.CrewStatusKind.assigned };
var lookupArrived = new SampleProfile.CrewStatusKind { Name = SampleProfile.CrewStatusKind.arrived };

AssertCondition(lookupAssignedA.Equals(lookupAssignedB),
    "Expected lookup entities with the same Name to compare equal.");
AssertCondition(lookupAssignedA.GetHashCode() == lookupAssignedB.GetHashCode(),
    "Expected equal lookup entities to produce the same hash code.");
AssertCondition(!lookupAssignedA.Equals(lookupArrived),
    "Expected lookup entities with different names to compare unequal.");

var organisation = new SampleProfile.Organisation
{
    MRId = "org-001",
    Name = "Todd Test Utility",
    ElectronicAddress = new SampleProfile.ElectronicAddress
    {
        Email1 = "ops@example.com",
        UserID = "ops-user"
    },
    Phone1 = new SampleProfile.TelephoneNumber
    {
        CountryCode = "1",
        AreaCode = "505",
        ItuPhone = "+1-505-555-0100"
    },
    StreetAddress = new SampleProfile.StreetAddress
    {
        StreetDetail = new SampleProfile.StreetDetail
        {
            Number = "123",
            Name = "Grid Way",
            Type = "Road"
        }
    }
};

AssertCondition(!string.IsNullOrWhiteSpace(organisation.ElectronicAddress?.Id),
    "Expected compound ElectronicAddress to get a generated surrogate Id in its constructor.");
AssertCondition(!string.IsNullOrWhiteSpace(organisation.Phone1?.Id),
    "Expected compound TelephoneNumber to get a generated surrogate Id in its constructor.");
AssertCondition(!string.IsNullOrWhiteSpace(organisation.StreetAddress?.Id),
    "Expected compound StreetAddress to get a generated surrogate Id in its constructor.");

context.Organisations.Add(organisation);
context.SaveChanges();

AssertCondition(context.Organisations.Count() == 1, "Expected one Organisation row after initial save.");
AssertCondition(context.IdentifiedObjects.Count() == 1,
    "Expected one IdentifiedObject row after saving the first Organisation.");
AssertCondition(context.ElectronicAddresses.Count() == 1, "Expected one ElectronicAddress row after initial save.");
AssertCondition(context.TelephoneNumbers.Count() == 1, "Expected one TelephoneNumber row after initial save.");
AssertCondition(context.StreetAddresses.Count() == 1, "Expected one StreetAddress row after initial save.");
AssertCondition(!string.IsNullOrWhiteSpace(organisation.ElectronicAddressId),
    "Expected ElectronicAddressId to be populated after save.");
AssertCondition(!string.IsNullOrWhiteSpace(organisation.Phone1Id),
    "Expected Phone1Id to be populated after save.");
AssertCondition(!string.IsNullOrWhiteSpace(organisation.StreetAddressId),
    "Expected StreetAddressId to be populated after save.");

var loaded = context.Organisations
    .Include(x => x.ElectronicAddress)
    .Include(x => x.Phone1)
    .Include(x => x.StreetAddress)
        .ThenInclude(x => x!.StreetDetail)
    .Single(x => x.MRId == "org-001");

AssertCondition(loaded.Name == "Todd Test Utility", "Expected Organisation.Name to round-trip through EF Core.");
AssertCondition(loaded.ElectronicAddress?.Email1 == "ops@example.com",
    "Expected ElectronicAddress.Email1 to round-trip through EF Core.");
AssertCondition(loaded.ElectronicAddressId == loaded.ElectronicAddress?.Id,
    "Expected ElectronicAddress FK to match the related entity Id.");
AssertCondition(loaded.Phone1?.ItuPhone == "+1-505-555-0100",
    "Expected TelephoneNumber.ItuPhone to round-trip through EF Core.");
AssertCondition(loaded.Phone1Id == loaded.Phone1?.Id,
    "Expected Phone1 FK to match the related entity Id.");
AssertCondition(loaded.StreetAddress?.StreetDetail?.Name == "Grid Way",
    "Expected StreetDetail.Name to round-trip through EF Core.");
AssertCondition(loaded.StreetAddressId == loaded.StreetAddress?.Id,
    "Expected StreetAddress FK to match the related entity Id.");

context.Remove(loaded);
context.SaveChanges();

AssertCondition(context.Organisations.Count() == 0, "Expected Organisation row to be deleted.");

var orphanElectronicAddresses = context.ElectronicAddresses.Count();
var orphanTelephoneNumbers = context.TelephoneNumbers.Count();
var orphanStreetAddresses = context.StreetAddresses.Count();

Console.WriteLine("Note: deleting Organisation does not currently cascade-delete referenced compound rows.");
Console.WriteLine($"Remaining ElectronicAddress rows: {orphanElectronicAddresses}");
Console.WriteLine($"Remaining TelephoneNumber rows: {orphanTelephoneNumbers}");
Console.WriteLine($"Remaining StreetAddress rows: {orphanStreetAddresses}");

AssertCondition(orphanElectronicAddresses == 1,
    "Expected one orphan ElectronicAddress row with the current generated mapping.");
AssertCondition(orphanTelephoneNumbers == 1,
    "Expected one orphan TelephoneNumber row with the current generated mapping.");
AssertCondition(orphanStreetAddresses == 1,
    "Expected one orphan StreetAddress row with the current generated mapping.");

using (var relationshipContext = new SampleProfileDbContext(options))
{
    var parent = new SampleProfile.ParentOrganization
    {
        MRId = "org-parent-001",
        Name = "Parent Utility"
    };

    var child = new SampleProfile.Organisation
    {
        MRId = "org-child-001",
        Name = "Child Utility",
        ParentOrganisation = parent
    };

    relationshipContext.Add(parent);
    relationshipContext.Add(child);
    relationshipContext.SaveChanges();
}

using (var verificationContext = new SampleProfileDbContext(options))
{
    AssertCondition(verificationContext.Organisations.Count() == 2,
        "Expected parent and child Organisation rows after saving the independent relationship scenario.");
    AssertCondition(verificationContext.ParentOrganizations.Count() == 1,
        "Expected one ParentOrganization row after saving the independent relationship scenario.");
    AssertCondition(verificationContext.IdentifiedObjects.Count() == 2,
        "Expected two IdentifiedObject rows after saving parent and child organisations.");

    var loadedChild = verificationContext.Organisations
        .Include(x => x.ParentOrganisation)
        .Single(x => x.MRId == "org-child-001");

    AssertCondition(loadedChild.ParentOrganisation?.MRId == "org-parent-001",
        "Expected ParentOrganisation to round-trip through EF Core.");
    AssertCondition(loadedChild.ParentOrganisationId == "org-parent-001",
        "Expected ParentOrganisation FK to be stored on the child Organisation row.");
}

Exception? deleteParentException = null;

using (var deleteContext = new SampleProfileDbContext(options))
{
    var parentToDelete = deleteContext.ParentOrganizations.Single(x => x.MRId == "org-parent-001");
    deleteContext.Remove(parentToDelete);

    try
    {
        deleteContext.SaveChanges();
    }
    catch (Exception ex)
    {
        deleteParentException = ex;
    }
}

AssertCondition(deleteParentException is not null,
    "Expected deleting a referenced ParentOrganization to fail with the current ClientNoAction mapping.");

using (var postFailureContext = new SampleProfileDbContext(options))
{
    AssertCondition(postFailureContext.ParentOrganizations.Count() == 1,
        "Expected ParentOrganization row to remain after failed delete.");
    AssertCondition(postFailureContext.Organisations.Count() == 2,
        "Expected child Organisation row to remain after failed parent delete.");
}

Console.WriteLine("EF Core smoke test passed.");
Console.WriteLine($"Organisation: {loaded.MRId} / {loaded.Name}");
Console.WriteLine($"Email: {loaded.ElectronicAddress?.Email1}");
Console.WriteLine($"Phone: {loaded.Phone1?.ItuPhone}");
Console.WriteLine($"Street: {loaded.StreetAddress?.StreetDetail?.Number} {loaded.StreetAddress?.StreetDetail?.Name}");
Console.WriteLine("Independent relationship delete test: ParentOrganization cannot be deleted while referenced.");
#endif
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Common;
using System.Reflection;
using System.Text.RegularExpressions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

var completedSections = new List<string>();
var diagnostics = new List<string>();

RunSection("Reflection Contract", VerifyReflectionContract);
RunSection("EF Metadata Contract", VerifyEfMetadataContract);
RunSection("SQL Schema Parity", VerifySqlSchemaParity);
RunSection("Generated CSharp Formatting", VerifyGeneratedCSharpFormatting);
RunSection("Generated CSharp Text Integrity", VerifyGeneratedCSharpTextIntegrity);
RunSection("Lookup Equality Semantics", VerifyLookupEqualitySemantics);
RunSection("Database Uniqueness Enforcement", VerifyDatabaseUniquenessEnforcement);
RunSection("Generated Identity Behavior", VerifyGeneratedIdentityBehavior);
RunSection("Required And Optional Behavior", VerifyRequiredAndOptionalBehavior);
RunSection("FK Navigation Synchronization", VerifyForeignKeyNavigationSynchronization);
RunSection("Compound Reference Replacement", VerifyCompoundReferenceReplacement);
RunSection("Dependent Delete Direction", VerifyDependentDeleteDirection);
RunSection("Principal Delete Cascade", VerifyPrincipalDeleteCascade);
RunSection("Async Compound Cleanup", VerifyAsyncCompoundCleanup);
RunSection("Direct FK Compound Replacement", VerifyDirectForeignKeyCompoundReplacement);
RunSection("Compound Slot Ownership", VerifyCompoundSlotOwnership);
RunSection("Cross Owner Compound Ownership", VerifyCrossOwnerCompoundOwnership);
RunSection("Compound Ownership Handoff", VerifyCompoundOwnershipHandoff);
RunSection("Compound Mutation Stress", VerifyCompoundMutationStress);
RunSection("Compound Failure Rollback Safety", VerifyCompoundFailureRollbackSafety);
RunSection("Compound Failure Recovery", VerifyCompoundFailureRecovery);
RunSection("Generated Mapping Baseline", VerifyGeneratedMappingBaseline);
RunSection("Compound Null Detach Cleanup", VerifyCompoundNullDetachCleanup);
RunSection("Generated Null Detach Baseline", VerifyGeneratedNullDetachBaseline);
RunSection("Independent Relationships", VerifyIndependentRelationships);
RunSection("Inheritance Storage", VerifyInheritanceStorage);
RunSection("Inheritance Delete Cleanup", VerifyInheritanceDeleteCleanup);
RunSection("Inheritance Query Materialization", VerifyInheritanceQueryMaterialization);
RunSection("Repeated Compound Replacement Cycles", VerifyRepeatedCompoundReplacementCycles);
RunSection("Repeated Address Replacement Cycles", VerifyRepeatedAddressReplacementCycles);

Console.WriteLine($"Comprehensive EF Core regression test passed ({completedSections.Count} sections).");
foreach (var section in completedSections)
{
    Console.WriteLine($"- {section}");
}

if (diagnostics.Count > 0)
{
    Console.WriteLine("Diagnostics:");
    foreach (var diagnostic in diagnostics)
    {
        Console.WriteLine($"- {diagnostic}");
    }
}

void RunSection(string name, Action action)
{
    try
    {
        action();
        completedSections.Add(name);
    }
    catch (Exception ex)
    {
        throw new InvalidOperationException($"Section failed: {name}. {ex.Message}", ex);
    }
}

void AssertCondition(bool condition, string message)
{
    if (!condition)
    {
        throw new InvalidOperationException(message);
    }
}

void WithFreshDatabase(Action<DbContextOptions<SampleProfileDbContext>> action)
{
    using var connection = new SqliteConnection("Data Source=:memory:");
    connection.Open();

    var options = new DbContextOptionsBuilder<SampleProfileDbContext>()
        .UseSqlite(connection)
        .EnableSensitiveDataLogging()
        .Options;

    using (var setupContext = new SampleProfileDbContext(options))
    {
        setupContext.Database.EnsureDeleted();
        setupContext.Database.EnsureCreated();
    }

    action(options);
}

void WithFreshGeneratedOnlyDatabase(Action<DbContextOptions<GeneratedOnlySampleProfileDbContext>> action)
{
    using var connection = new SqliteConnection("Data Source=:memory:");
    connection.Open();

    var options = new DbContextOptionsBuilder<GeneratedOnlySampleProfileDbContext>()
        .UseSqlite(connection)
        .EnableSensitiveDataLogging()
        .Options;

    using (var setupContext = new GeneratedOnlySampleProfileDbContext(options))
    {
        setupContext.Database.EnsureDeleted();
        setupContext.Database.EnsureCreated();
    }

    action(options);
}

void VerifyReflectionContract()
{
    AssertCondition(SampleProfile.allClasses.Length == 21, "Expected 21 generated model classes in SampleProfile.allClasses.");

    foreach (var type in SampleProfile.allClasses)
    {
        AssertCondition(type.GetCustomAttribute<TableAttribute>() is not null,
            $"Expected {type.Name} to have a [Table] attribute.");
    }

    AssertPropertyKey(typeof(SampleProfile.IdentifiedObject), nameof(SampleProfile.IdentifiedObject.MRId), "mRID", 100);
    AssertPropertyKey(typeof(SampleProfile.ElectronicAddress), nameof(SampleProfile.ElectronicAddress.Id), "id", 100);
    AssertPropertyKey(typeof(SampleProfile.StreetAddress), nameof(SampleProfile.StreetAddress.Id), "id", 100);
    AssertPropertyKey(typeof(SampleProfile.CrewStatusKind), nameof(SampleProfile.CrewStatusKind.Name), "name", 100);
    AssertPropertyKey(typeof(SampleProfile.ShuntCompensatorControl), nameof(SampleProfile.ShuntCompensatorControl.Id), "id", 100);

    AssertNoDeclaredKeyProperties(typeof(SampleProfile.AssetInfo));
    AssertNoDeclaredKeyProperties(typeof(SampleProfile.Organisation));
    AssertNoDeclaredKeyProperties(typeof(SampleProfile.ParentOrganization));
    AssertNoDeclaredKeyProperties(typeof(SampleProfile.ShuntCompensatorInfo));
    AssertNoDeclaredKeyProperties(typeof(SampleProfile.WireInfo));
    AssertNoDeclaredKeyProperties(typeof(SampleProfile.WireSpacingInfo));
    AssertNoDeclaredKeyProperties(typeof(SampleProfile.OverheadWireInfo));

    var organisationIndexes = typeof(SampleProfile.Organisation).GetCustomAttributes<IndexAttribute>().ToList();
    AssertCondition(organisationIndexes.Count == 5, "Expected Organisation to declare five unique compound indexes.");
    AssertCondition(HasSinglePropertyUniqueIndex(organisationIndexes, nameof(SampleProfile.Organisation.ElectronicAddressId)),
        "Expected a unique index on Organisation.ElectronicAddressId.");
    AssertCondition(HasSinglePropertyUniqueIndex(organisationIndexes, nameof(SampleProfile.Organisation.Phone1Id)),
        "Expected a unique index on Organisation.Phone1Id.");
    AssertCondition(HasSinglePropertyUniqueIndex(organisationIndexes, nameof(SampleProfile.Organisation.Phone2Id)),
        "Expected a unique index on Organisation.Phone2Id.");
    AssertCondition(HasSinglePropertyUniqueIndex(organisationIndexes, nameof(SampleProfile.Organisation.PostalAddressId)),
        "Expected a unique index on Organisation.PostalAddressId.");
    AssertCondition(HasSinglePropertyUniqueIndex(organisationIndexes, nameof(SampleProfile.Organisation.StreetAddressId)),
        "Expected a unique index on Organisation.StreetAddressId.");

    AssertCondition(!new SampleProfile.Organisation { MRId = "same-id" }
        .Equals(new SampleProfile.ParentOrganization { MRId = "same-id" }),
        "IdentifiedObject equality should reject different runtime types.");
    AssertCondition(new SampleProfile.ElectronicAddress { Id = "compound-1" }
        .Equals(new SampleProfile.ElectronicAddress { Id = "compound-1" }),
        "Compound types should compare equal when surrogate Id matches.");
    AssertCondition(new SampleProfile.ShuntCompensatorControl { Id = "control-1" }
        .Equals(new SampleProfile.ShuntCompensatorControl { Id = "control-1" }),
        "Surrogate-key root types should compare equal when Id matches.");
    AssertCondition(!string.IsNullOrWhiteSpace(new SampleProfile.TelephoneNumber().Id),
        "TelephoneNumber constructor should generate an Id.");
}

void VerifyLookupEqualitySemantics()
{
    var assignedA = new SampleProfile.CrewStatusKind { Name = SampleProfile.CrewStatusKind.assigned };
    var assignedB = new SampleProfile.CrewStatusKind { Name = SampleProfile.CrewStatusKind.assigned };
    var arrived = new SampleProfile.CrewStatusKind { Name = SampleProfile.CrewStatusKind.arrived };

    AssertCondition(assignedA.Equals(assignedB), "Lookup entities with the same Name should compare equal.");
    AssertCondition(assignedA.GetHashCode() == assignedB.GetHashCode(),
        "Equal lookup entities should have the same hash code.");
    AssertCondition(!assignedA.Equals(arrived), "Lookup entities with different names should compare unequal.");
}

void VerifyGeneratedCSharpFormatting()
{
    var source = LoadGeneratedCSharpText();
    var lines = File.ReadAllLines(GetGeneratedCSharpPath());

    AssertCondition(!source.Contains('\t'),
        "Expected generated C# to use spaces rather than tab characters.");

    var whitespaceOnlyLines = lines
        .Select((line, index) => (LineNumber: index + 1, Line: line))
        .Where(item => item.Line.Length > 0 && string.IsNullOrWhiteSpace(item.Line))
        .Select(item => item.LineNumber)
        .ToList();

    if (whitespaceOnlyLines.Count > 0)
    {
        diagnostics.Add($"FORMATTING OBSERVED: generated C# contains {whitespaceOnlyLines.Count} whitespace-only blank line(s); first examples at lines {string.Join(", ", whitespaceOnlyLines.Take(8))}.");
    }

    var doubleBlankStarts = new List<int>();
    for (var i = 0; i < lines.Length - 1; i++)
    {
        if (string.IsNullOrWhiteSpace(lines[i]) && string.IsNullOrWhiteSpace(lines[i + 1]))
        {
            doubleBlankStarts.Add(i + 1);
        }
    }

    if (doubleBlankStarts.Count > 0)
    {
        diagnostics.Add($"FORMATTING OBSERVED: generated C# contains consecutive blank-line runs starting at lines {string.Join(", ", doubleBlankStarts.Take(8))}.");
    }

    var blankBeforeCloseLines = new List<int>();
    for (var i = 1; i < lines.Length; i++)
    {
        if (lines[i].Trim() == "}" && string.IsNullOrWhiteSpace(lines[i - 1]))
        {
            blankBeforeCloseLines.Add(i + 1);
        }
    }

    if (blankBeforeCloseLines.Count > 0)
    {
        diagnostics.Add($"FORMATTING OBSERVED: generated C# contains blank lines immediately before closing braces at lines {string.Join(", ", blankBeforeCloseLines.Take(8))}.");
    }
}

void VerifyGeneratedCSharpTextIntegrity()
{
    var lines = File.ReadAllLines(GetGeneratedCSharpPath());

    var suspiciousLines = lines
        .Select((line, index) => (LineNumber: index + 1, Line: line))
        .Where(item =>
            item.Line.Contains('Ã') ||
            item.Line.Contains('Â') ||
            item.Line.Contains("â€", StringComparison.Ordinal) ||
            item.Line.Contains("â‚¬", StringComparison.Ordinal))
        .ToList();

    if (suspiciousLines.Count > 0)
    {
        diagnostics.Add(
            $"TEXT INTEGRITY OBSERVED: generated C# contains likely mojibake on {suspiciousLines.Count} line(s); first examples at lines {string.Join(", ", suspiciousLines.Select(item => item.LineNumber).Take(8))}.");
    }
}

void VerifySqlSchemaParity()
{
    var sql = LoadGeneratedSqlText();

    foreach (var type in SampleProfile.allClasses)
    {
        var tableName = type.GetCustomAttribute<TableAttribute>()?.Name
            ?? throw new InvalidOperationException($"Expected [Table] on {type.Name}.");
        AssertCondition(sql.Contains($"CREATE TABLE \"{tableName}\"", StringComparison.Ordinal),
            $"Expected generated SQL to declare CREATE TABLE for {tableName}.");
    }

    AssertSqlTableBlockContains(sql, "IdentifiedObject", "\"mRID\" VARCHAR(100) PRIMARY KEY");
    AssertSqlTableBlockContains(sql, "Organisation", "\"mRID\" VARCHAR(100) PRIMARY KEY");
    AssertSqlTableBlockContains(sql, "Organisation", "\"electronicAddress\" VARCHAR(100) UNIQUE");
    AssertSqlTableBlockContains(sql, "Organisation", "\"phone1\" VARCHAR(100) UNIQUE");
    AssertSqlTableBlockContains(sql, "Organisation", "\"streetAddress\" VARCHAR(100) UNIQUE");
    AssertSqlTableBlockContains(sql, "StreetAddress", "\"status\" VARCHAR(100) UNIQUE");
    AssertSqlTableBlockContains(sql, "StreetAddress", "\"streetDetail\" VARCHAR(100) UNIQUE");
    AssertSqlTableBlockContains(sql, "StreetAddress", "\"townDetail\" VARCHAR(100) UNIQUE");
    AssertSqlTableBlockContains(sql, "ShuntCompensatorControl", "\"id\" VARCHAR(100) PRIMARY KEY");

    AssertSqlForeignKey(sql, "AssetInfo", "mRID", "IdentifiedObject", "mRID");
    AssertSqlForeignKey(sql, "Organisation", "mRID", "IdentifiedObject", "mRID");
    AssertSqlForeignKey(sql, "ParentOrganization", "mRID", "Organisation", "mRID");
    AssertSqlForeignKey(sql, "WireInfo", "mRID", "AssetInfo", "mRID");
    AssertSqlForeignKey(sql, "OverheadWireInfo", "mRID", "WireInfo", "mRID");

    AssertSqlForeignKey(sql, "Organisation", "electronicAddress", "ElectronicAddress", "id");
    AssertSqlForeignKey(sql, "Organisation", "ParentOrganisation", "ParentOrganization", "mRID");
    AssertSqlForeignKey(sql, "Organisation", "phone1", "TelephoneNumber", "id");
    AssertSqlForeignKey(sql, "Organisation", "phone2", "TelephoneNumber", "id");
    AssertSqlForeignKey(sql, "Organisation", "postalAddress", "StreetAddress", "id");
    AssertSqlForeignKey(sql, "Organisation", "streetAddress", "StreetAddress", "id");
    AssertSqlForeignKey(sql, "StreetAddress", "status", "Status", "id");
    AssertSqlForeignKey(sql, "StreetAddress", "streetDetail", "StreetDetail", "id");
    AssertSqlForeignKey(sql, "StreetAddress", "townDetail", "TownDetail", "id");
    AssertSqlForeignKey(sql, "ElectronicAddress", "id", "Organisation", "electronicAddress", onDeleteCascade: true);
    AssertSqlForeignKey(sql, "TelephoneNumber", "id", "Organisation", "phone1", onDeleteCascade: true);
    AssertSqlForeignKey(sql, "TelephoneNumber", "id", "Organisation", "phone2", onDeleteCascade: true);
    AssertSqlForeignKey(sql, "StreetAddress", "id", "Organisation", "postalAddress", onDeleteCascade: true);
    AssertSqlForeignKey(sql, "StreetAddress", "id", "Organisation", "streetAddress", onDeleteCascade: true);

    WithFreshDatabase(options =>
    {
        using var context = new SampleProfileDbContext(options);

        foreach (var type in SampleProfile.allClasses)
        {
            var entityType = GetEntityType(context, type);
            var store = StoreObjectIdentifier.Table(entityType.GetTableName()!, entityType.GetSchema());
            var sqlColumns = GetSqlTableColumns(sql, entityType.GetTableName()!);

            foreach (var property in entityType.GetProperties())
            {
                var columnName = property.GetColumnName(store);
                if (columnName is null)
                {
                    continue;
                }

                AssertCondition(sqlColumns.TryGetValue(columnName, out var sqlDefinition),
                    $"Expected SQL table {entityType.GetTableName()} to define column {columnName} for EF property {type.Name}.{property.Name}.");
                sqlDefinition ??= string.Empty;

                if (property.IsPrimaryKey())
                {
                    if (!sqlDefinition.Contains("PRIMARY KEY", StringComparison.Ordinal))
                    {
                        var sqlUsesUniqueInstead = sqlDefinition.Contains("UNIQUE", StringComparison.Ordinal)
                            && entityType.GetProperties().Count() == 1;
                        AssertCondition(sqlUsesUniqueInstead,
                            $"Expected SQL column {entityType.GetTableName()}.{columnName} to be PRIMARY KEY.");

                        diagnostics.Add($"PARITY NOTE OBSERVED: SQL uses UNIQUE instead of PRIMARY KEY for {entityType.GetTableName()}.{columnName}, while EF treats it as the key.");
                    }
                }

                if (property.ClrType == typeof(string))
                {
                    var maxLength = property.GetMaxLength();
                    if (maxLength is int length)
                    {
                        AssertCondition(sqlDefinition.Contains($"VARCHAR({length})", StringComparison.Ordinal),
                            $"Expected SQL column {entityType.GetTableName()}.{columnName} to use VARCHAR({length}).");
                    }
                }
                else if (property.ClrType == typeof(bool) || property.ClrType == typeof(bool?))
                {
                    AssertCondition(sqlDefinition.Contains("INTEGER", StringComparison.Ordinal),
                        $"Expected SQL column {entityType.GetTableName()}.{columnName} to use INTEGER for boolean storage.");
                }
                else if (property.ClrType == typeof(double) || property.ClrType == typeof(double?))
                {
                    AssertCondition(sqlDefinition.Contains("DOUBLE PRECISION", StringComparison.Ordinal),
                        $"Expected SQL column {entityType.GetTableName()}.{columnName} to use DOUBLE PRECISION.");
                }
                else if (property.ClrType == typeof(DateTime) || property.ClrType == typeof(DateTime?))
                {
                    AssertCondition(sqlDefinition.Contains("TIMESTAMP", StringComparison.Ordinal),
                        $"Expected SQL column {entityType.GetTableName()}.{columnName} to use TIMESTAMP.");
                }

                if (!property.IsPrimaryKey())
                {
                    AssertCondition(property.IsNullable == !sqlDefinition.Contains("NOT NULL", StringComparison.Ordinal),
                        $"Expected nullability parity for {entityType.GetTableName()}.{columnName} between EF and SQL.");
                }
            }

            foreach (var index in entityType.GetIndexes().Where(candidate => candidate.IsUnique && candidate.Properties.Count == 1))
            {
                var columnName = index.Properties[0].GetColumnName(store);
                if (columnName is null)
                {
                    continue;
                }

                AssertCondition(sqlColumns.TryGetValue(columnName, out var sqlDefinition)
                    && sqlDefinition is not null
                    && sqlDefinition.Contains("UNIQUE", StringComparison.Ordinal),
                    $"Expected SQL column {entityType.GetTableName()}.{columnName} to carry UNIQUE for EF unique index parity.");
            }
        }

        AssertCondition(GetEntityType(context, typeof(SampleProfile.IdentifiedObject)).FindProperty(nameof(SampleProfile.IdentifiedObject.MRId))!.IsNullable == false,
            "Expected EF metadata to mark IdentifiedObject.MRId as required, matching SQL PRIMARY KEY.");
        AssertCondition(GetEntityType(context, typeof(SampleProfile.Organisation)).FindProperty(nameof(SampleProfile.Organisation.ParentOrganisationId))!.IsNullable,
            "Expected EF metadata to mark Organisation.ParentOrganisationId as nullable, matching SQL.");
        AssertCondition(GetEntityType(context, typeof(SampleProfile.Organisation)).FindProperty(nameof(SampleProfile.Organisation.ElectronicAddressId))!.IsNullable,
            "Expected EF metadata to mark Organisation.ElectronicAddressId as nullable, matching SQL.");

        AssertUniqueIndex(context, typeof(SampleProfile.Organisation), nameof(SampleProfile.Organisation.ElectronicAddressId));
        AssertUniqueIndex(context, typeof(SampleProfile.Organisation), nameof(SampleProfile.Organisation.Phone1Id));
        AssertUniqueIndex(context, typeof(SampleProfile.Organisation), nameof(SampleProfile.Organisation.Phone2Id));
        AssertUniqueIndex(context, typeof(SampleProfile.Organisation), nameof(SampleProfile.Organisation.PostalAddressId));
        AssertUniqueIndex(context, typeof(SampleProfile.Organisation), nameof(SampleProfile.Organisation.StreetAddressId));
        AssertUniqueIndex(context, typeof(SampleProfile.StreetAddress), nameof(SampleProfile.StreetAddress.StatusId));
        AssertUniqueIndex(context, typeof(SampleProfile.StreetAddress), nameof(SampleProfile.StreetAddress.StreetDetailId));
        AssertUniqueIndex(context, typeof(SampleProfile.StreetAddress), nameof(SampleProfile.StreetAddress.TownDetailId));

        AssertForeignKeyTarget(context, typeof(SampleProfile.Organisation), nameof(SampleProfile.Organisation.ElectronicAddressId),
            typeof(SampleProfile.ElectronicAddress), nameof(SampleProfile.ElectronicAddress.Id));
        AssertForeignKeyTarget(context, typeof(SampleProfile.Organisation), nameof(SampleProfile.Organisation.ParentOrganisationId),
            typeof(SampleProfile.ParentOrganization), nameof(SampleProfile.ParentOrganization.MRId));
        AssertForeignKeyTarget(context, typeof(SampleProfile.StreetAddress), nameof(SampleProfile.StreetAddress.StatusId),
            typeof(SampleProfile.Status), nameof(SampleProfile.Status.Id));

        var organisationForeignKeys = GetSqliteForeignKeys(context, "Organisation");
        var sqlHasReverseCompoundCascade = HasSqlForeignKey(sql, "ElectronicAddress", "id", "Organisation", "electronicAddress", onDeleteCascade: true);
        var efHasPrincipalSideCascade = organisationForeignKeys.Any(fk => fk.From == "electronicAddress" && fk.Table == "ElectronicAddress" && fk.OnDelete == "CASCADE");

        if (sqlHasReverseCompoundCascade && efHasPrincipalSideCascade)
        {
            diagnostics.Add("PARITY GAP OBSERVED: SQL models compound ownership with reverse ON DELETE CASCADE constraints, but the EF/SQLite model currently cascades in the opposite direction from Organisation FK columns.");
        }
    });
}

void VerifyDatabaseUniquenessEnforcement()
{
    WithFreshDatabase(options =>
    {
        using (var context = new SampleProfileDbContext(options))
        {
            context.Add(new SampleProfile.CrewStatusKind { Name = SampleProfile.CrewStatusKind.assigned });
            context.SaveChanges();
        }

        var duplicateLookupException = TrySave(options, context =>
        {
            context.Add(new SampleProfile.CrewStatusKind { Name = SampleProfile.CrewStatusKind.assigned });
        });

        AssertCondition(duplicateLookupException is DbUpdateException,
            "Expected duplicate lookup Name to be rejected by the database.");

        using (var context = new SampleProfileDbContext(options))
        {
            var sharedAddress = new SampleProfile.ElectronicAddress { Email1 = "shared@example.com" };
            context.Add(sharedAddress);
            context.Add(new SampleProfile.Organisation
            {
                MRId = "org-unique-001",
                Name = "Unique One",
                ElectronicAddress = sharedAddress
            });
            context.SaveChanges();
        }

        var duplicateRelationshipException = TrySave(options, context =>
        {
            var sharedAddress = context.ElectronicAddresses.Single();
            context.Add(new SampleProfile.Organisation
            {
                MRId = "org-unique-002",
                Name = "Unique Two",
                ElectronicAddress = sharedAddress
            });
        });

        AssertCondition(duplicateRelationshipException is DbUpdateException,
            "Expected unique Organisation.ElectronicAddressId constraint to reject sharing one compound principal.");

        using var verificationContext = new SampleProfileDbContext(options);
        AssertCondition(verificationContext.Set<SampleProfile.CrewStatusKind>().Count() == 1,
            "Expected one CrewStatusKind row after duplicate insert attempt.");
        AssertCondition(verificationContext.Organisations.Count() == 1,
            "Expected one Organisation row after duplicate unique-FK insert attempt.");
        AssertCondition(verificationContext.ElectronicAddresses.Count() == 1,
            "Expected one ElectronicAddress row after duplicate unique-FK insert attempt.");
    });
}

void VerifyGeneratedIdentityBehavior()
{
    var generatedCompoundIds = Enumerable.Range(0, 12)
        .Select(_ => new SampleProfile.TelephoneNumber().Id)
        .Concat(Enumerable.Range(0, 12).Select(_ => new SampleProfile.StreetAddress().Id))
        .Concat(Enumerable.Range(0, 12).Select(_ => new SampleProfile.Status().Id))
        .ToList();

    AssertCondition(generatedCompoundIds.All(id => !string.IsNullOrWhiteSpace(id)),
        "Expected compound constructors to generate non-empty surrogate Id values.");
    AssertCondition(generatedCompoundIds.Distinct(StringComparer.Ordinal).Count() == generatedCompoundIds.Count,
        "Expected generated compound surrogate Id values to be unique across new instances.");

    AssertCondition(string.IsNullOrWhiteSpace(new SampleProfile.ShuntCompensatorControl().Id),
        "Surrogate-key root types should not silently auto-generate Id when the model expects the caller to provide one.");

    WithFreshDatabase(options =>
    {
        var missingSurrogateRootIdException = TrySave(options, context =>
        {
            context.Add(new SampleProfile.ShuntCompensatorControl
            {
                Id = null!,
                SensingPhaseCode = "ABC"
            });
        });

        AssertCondition(missingSurrogateRootIdException is InvalidOperationException or DbUpdateException,
            "Expected saving ShuntCompensatorControl without Id to fail.");
    });
}

void VerifyRequiredAndOptionalBehavior()
{
    WithFreshDatabase(options =>
    {
        var missingMridException = TrySave(options, context =>
        {
            context.Add(new SampleProfile.Organisation
            {
                MRId = null!,
                Name = "Missing MRId"
            });
        });

        AssertCondition(missingMridException is InvalidOperationException or DbUpdateException,
            "Expected saving Organisation without MRId to fail during EF tracking or database save.");

        using (var context = new SampleProfileDbContext(options))
        {
            context.Add(new SampleProfile.Organisation
            {
                MRId = "org-optional-001",
                Name = "Optional Utility",
                Phone2 = null,
                PostalAddress = null,
                StreetAddress = new SampleProfile.StreetAddress
                {
                    StreetDetail = new SampleProfile.StreetDetail
                    {
                        Number = "10",
                        Name = "Optional Way"
                    }
                }
            });
            context.SaveChanges();
        }

        using (var context = new SampleProfileDbContext(options))
        {
            var loaded = context.Organisations
                .Include(x => x.StreetAddress)
                .ThenInclude(x => x!.StreetDetail)
                .Single(x => x.MRId == "org-optional-001");

            AssertCondition(loaded.Phone2Id is null, "Expected optional Phone2Id to remain null.");
            AssertCondition(loaded.Phone2 is null, "Expected optional Phone2 navigation to remain null.");
            AssertCondition(loaded.PostalAddressId is null, "Expected optional PostalAddressId to remain null.");
            AssertCondition(loaded.PostalAddress is null, "Expected optional PostalAddress navigation to remain null.");
            AssertCondition(loaded.StreetAddressId is not null, "Expected StreetAddressId to be populated for assigned optional navigation.");
            AssertCondition(loaded.StreetAddress?.StreetDetail?.Name == "Optional Way",
                "Expected assigned StreetAddress graph to round-trip while unrelated optional navigations stay null.");
        }
    });
}

void VerifyForeignKeyNavigationSynchronization()
{
    WithFreshDatabase(options =>
    {
        using (var context = new SampleProfileDbContext(options))
        {
            context.Add(new SampleProfile.ParentOrganization
            {
                MRId = "parent-fk-001",
                Name = "Parent FK Utility"
            });
            context.Add(new SampleProfile.TelephoneNumber
            {
                Id = "phone-sync-001",
                ItuPhone = "+1-555-0101"
            });
            context.SaveChanges();
        }

        using (var context = new SampleProfileDbContext(options))
        {
            context.Add(new SampleProfile.Organisation
            {
                MRId = "org-sync-001",
                Name = "Sync Utility",
                ParentOrganisationId = "parent-fk-001",
                Phone1Id = "phone-sync-001"
            });
            context.SaveChanges();
        }

        using (var context = new SampleProfileDbContext(options))
        {
            var loaded = context.Organisations
                .Include(x => x.ParentOrganisation)
                .Include(x => x.Phone1)
                .Single(x => x.MRId == "org-sync-001");

            AssertCondition(loaded.ParentOrganisation?.MRId == "parent-fk-001",
                "Expected navigation to resolve when only ParentOrganisationId was assigned.");
            AssertCondition(loaded.Phone1?.Id == "phone-sync-001",
                "Expected navigation to resolve when only Phone1Id was assigned.");
        }

        using (var context = new SampleProfileDbContext(options))
        {
            var loaded = context.Organisations.Single(x => x.MRId == "org-sync-001");
            loaded.Phone2 = new SampleProfile.TelephoneNumber
            {
                ItuPhone = "+1-555-0102"
            };
            loaded.ParentOrganisation = null;
            loaded.ParentOrganisationId = null;
            context.SaveChanges();
        }

        using (var context = new SampleProfileDbContext(options))
        {
            var loaded = context.Organisations
                .Include(x => x.Phone1)
                .Include(x => x.Phone2)
                .Include(x => x.ParentOrganisation)
                .Single(x => x.MRId == "org-sync-001");

            AssertCondition(loaded.Phone1Id == "phone-sync-001",
                "Expected existing FK-assigned Phone1 relationship to remain unchanged after update.");
            AssertCondition(loaded.Phone2Id == loaded.Phone2?.Id,
                "Expected new navigation-assigned Phone2 relationship to populate its FK.");
            AssertCondition(loaded.Phone2?.ItuPhone == "+1-555-0102",
                "Expected newly assigned Phone2 navigation to round-trip.");
            AssertCondition(loaded.ParentOrganisationId is null,
                "Expected optional ParentOrganisationId to clear successfully.");
            AssertCondition(loaded.ParentOrganisation is null,
                "Expected optional ParentOrganisation navigation to clear successfully.");
        }
    });
}

void VerifyCompoundReferenceReplacement()
{
    WithFreshDatabase(options =>
    {
        string originalPhoneId;

        using (var context = new SampleProfileDbContext(options))
        {
            var organisation = new SampleProfile.Organisation
            {
                MRId = "org-replace-001",
                Name = "Replace Utility",
                Phone1 = new SampleProfile.TelephoneNumber
                {
                    ItuPhone = "+1-555-0200"
                }
            };

            context.Add(organisation);
            context.SaveChanges();
            originalPhoneId = organisation.Phone1Id!;
        }

        using (var context = new SampleProfileDbContext(options))
        {
            var loaded = context.Organisations
                .Include(x => x.Phone1)
                .Single(x => x.MRId == "org-replace-001");

            loaded.Phone1 = new SampleProfile.TelephoneNumber
            {
                ItuPhone = "+1-555-0299"
            };

            context.SaveChanges();
        }

        using (var context = new SampleProfileDbContext(options))
        {
            var loaded = context.Organisations
                .Include(x => x.Phone1)
                .Single(x => x.MRId == "org-replace-001");

            AssertCondition(loaded.Phone1Id is not null && loaded.Phone1Id != originalPhoneId,
                "Expected replacing Phone1 navigation to update the stored foreign key.");
            AssertCondition(loaded.Phone1?.ItuPhone == "+1-555-0299",
                "Expected replacement Phone1 navigation to round-trip.");
            AssertCondition(context.TelephoneNumbers.Count() == 1,
                "Expected replacing Phone1 navigation to clean up the previous TelephoneNumber row.");

            loaded.Phone1 = null;
            loaded.Phone1Id = null;
            context.SaveChanges();
        }

        using (var context = new SampleProfileDbContext(options))
        {
            var loaded = context.Organisations
                .Include(x => x.Phone1)
                .Single(x => x.MRId == "org-replace-001");

            AssertCondition(loaded.Phone1Id is null,
                "Expected clearing Phone1 navigation to clear the stored foreign key.");
            AssertCondition(loaded.Phone1 is null,
                "Expected clearing Phone1 navigation to remove the related reference on reload.");
            AssertCondition(context.TelephoneNumbers.Count() == 0,
                "Expected clearing Phone1 navigation to clean up the orphan TelephoneNumber row.");
        }
    });
}

void VerifyEfMetadataContract()
{
    WithFreshDatabase(options =>
    {
        using var context = new SampleProfileDbContext(options);
        var tableNames = context.Database.SqlQueryRaw<string>("SELECT name AS Value FROM sqlite_master WHERE type='table'").ToList();

        foreach (var type in SampleProfile.allClasses)
        {
            var entityType = GetEntityType(context, type);
            var tableAttribute = type.GetCustomAttribute<TableAttribute>();
            AssertCondition(tableAttribute is not null, $"Expected [Table] on {type.Name}.");
            AssertCondition(entityType.GetTableName() == tableAttribute!.Name,
                $"Expected EF metadata table for {type.Name} to be {tableAttribute.Name}.");
            AssertCondition(tableNames.Contains(tableAttribute.Name),
                $"Expected SQLite table {tableAttribute.Name} to exist.");
        }

        var keys = new (Type Type, string PropertyName)[]
        {
            (typeof(SampleProfile.CrewStatusKind), nameof(SampleProfile.CrewStatusKind.Name)),
            (typeof(SampleProfile.PhaseCode), nameof(SampleProfile.PhaseCode.Name)),
            (typeof(SampleProfile.ShuntImpedanceControlKind), nameof(SampleProfile.ShuntImpedanceControlKind.Name)),
            (typeof(SampleProfile.ShuntImpedanceLocalControlKind), nameof(SampleProfile.ShuntImpedanceLocalControlKind.Name)),
            (typeof(SampleProfile.WireInsulationKind), nameof(SampleProfile.WireInsulationKind.Name)),
            (typeof(SampleProfile.WireMaterialKind), nameof(SampleProfile.WireMaterialKind.Name)),
            (typeof(SampleProfile.ElectronicAddress), nameof(SampleProfile.ElectronicAddress.Id)),
            (typeof(SampleProfile.Status), nameof(SampleProfile.Status.Id)),
            (typeof(SampleProfile.StreetDetail), nameof(SampleProfile.StreetDetail.Id)),
            (typeof(SampleProfile.TelephoneNumber), nameof(SampleProfile.TelephoneNumber.Id)),
            (typeof(SampleProfile.TownDetail), nameof(SampleProfile.TownDetail.Id)),
            (typeof(SampleProfile.StreetAddress), nameof(SampleProfile.StreetAddress.Id)),
            (typeof(SampleProfile.IdentifiedObject), nameof(SampleProfile.IdentifiedObject.MRId)),
            (typeof(SampleProfile.ShuntCompensatorControl), nameof(SampleProfile.ShuntCompensatorControl.Id)),
            (typeof(SampleProfile.AssetInfo), nameof(SampleProfile.AssetInfo.MRId)),
            (typeof(SampleProfile.Organisation), nameof(SampleProfile.Organisation.MRId)),
            (typeof(SampleProfile.ParentOrganization), nameof(SampleProfile.ParentOrganization.MRId)),
            (typeof(SampleProfile.ShuntCompensatorInfo), nameof(SampleProfile.ShuntCompensatorInfo.MRId)),
            (typeof(SampleProfile.WireInfo), nameof(SampleProfile.WireInfo.MRId)),
            (typeof(SampleProfile.WireSpacingInfo), nameof(SampleProfile.WireSpacingInfo.MRId)),
            (typeof(SampleProfile.OverheadWireInfo), nameof(SampleProfile.OverheadWireInfo.MRId)),
        };

        foreach (var item in keys)
        {
            AssertPrimaryKey(context, item.Type, item.PropertyName);
        }

        AssertColumnName(context, typeof(SampleProfile.IdentifiedObject), nameof(SampleProfile.IdentifiedObject.MRId), "mRID");
        AssertColumnName(context, typeof(SampleProfile.Organisation), nameof(SampleProfile.Organisation.ElectronicAddressId), "electronicAddress");
        AssertColumnName(context, typeof(SampleProfile.Organisation), nameof(SampleProfile.Organisation.ParentOrganisationId), "ParentOrganisation");
        AssertColumnName(context, typeof(SampleProfile.ElectronicAddress), nameof(SampleProfile.ElectronicAddress.Email1), "email1");
        AssertColumnName(context, typeof(SampleProfile.StreetAddress), nameof(SampleProfile.StreetAddress.StreetDetailId), "streetDetail");
        AssertColumnName(context, typeof(SampleProfile.CrewStatusKind), nameof(SampleProfile.CrewStatusKind.Name), "name");

        var deleteBehaviors = new (Type Type, string PropertyName, DeleteBehavior Behavior)[]
        {
            (typeof(SampleProfile.StreetAddress), nameof(SampleProfile.StreetAddress.StatusId), DeleteBehavior.Cascade),
            (typeof(SampleProfile.StreetAddress), nameof(SampleProfile.StreetAddress.StreetDetailId), DeleteBehavior.Cascade),
            (typeof(SampleProfile.StreetAddress), nameof(SampleProfile.StreetAddress.TownDetailId), DeleteBehavior.Cascade),
            (typeof(SampleProfile.Organisation), nameof(SampleProfile.Organisation.ElectronicAddressId), DeleteBehavior.Cascade),
            (typeof(SampleProfile.Organisation), nameof(SampleProfile.Organisation.ParentOrganisationId), DeleteBehavior.ClientNoAction),
            (typeof(SampleProfile.Organisation), nameof(SampleProfile.Organisation.Phone1Id), DeleteBehavior.Cascade),
            (typeof(SampleProfile.Organisation), nameof(SampleProfile.Organisation.Phone2Id), DeleteBehavior.Cascade),
            (typeof(SampleProfile.Organisation), nameof(SampleProfile.Organisation.PostalAddressId), DeleteBehavior.Cascade),
            (typeof(SampleProfile.Organisation), nameof(SampleProfile.Organisation.StreetAddressId), DeleteBehavior.Cascade),
            (typeof(SampleProfile.ShuntCompensatorInfo), nameof(SampleProfile.ShuntCompensatorInfo.ShuntCompensatorControlId), DeleteBehavior.ClientNoAction),
        };

        foreach (var item in deleteBehaviors)
        {
            AssertDeleteBehavior(context, item.Type, item.PropertyName, item.Behavior);
        }

        AssertUniqueIndex(context, typeof(SampleProfile.Organisation), nameof(SampleProfile.Organisation.ElectronicAddressId));
        AssertUniqueIndex(context, typeof(SampleProfile.Organisation), nameof(SampleProfile.Organisation.Phone1Id));
        AssertUniqueIndex(context, typeof(SampleProfile.Organisation), nameof(SampleProfile.Organisation.Phone2Id));
        AssertUniqueIndex(context, typeof(SampleProfile.Organisation), nameof(SampleProfile.Organisation.PostalAddressId));
        AssertUniqueIndex(context, typeof(SampleProfile.Organisation), nameof(SampleProfile.Organisation.StreetAddressId));

        var organisationForeignKeys = GetSqliteForeignKeys(context, "Organisation");
        AssertCondition(organisationForeignKeys.Any(fk => fk.From == "electronicAddress" && fk.Table == "ElectronicAddress" && fk.OnDelete == "CASCADE"),
            "Expected SQLite ON DELETE CASCADE for Organisation.electronicAddress.");
        AssertCondition(organisationForeignKeys.Any(fk => fk.From == "ParentOrganisation" && fk.OnDelete != "CASCADE"),
            "Expected a non-cascading SQLite foreign key for Organisation.ParentOrganisation.");
        AssertCondition(organisationForeignKeys.Any(fk => fk.From == "phone1" && fk.Table == "TelephoneNumber" && fk.OnDelete == "CASCADE"),
            "Expected SQLite ON DELETE CASCADE for Organisation.phone1.");
        AssertCondition(organisationForeignKeys.Any(fk => fk.From == "phone2" && fk.Table == "TelephoneNumber" && fk.OnDelete == "CASCADE"),
            "Expected SQLite ON DELETE CASCADE for Organisation.phone2.");
        AssertCondition(organisationForeignKeys.Any(fk => fk.From == "postalAddress" && fk.Table == "StreetAddress" && fk.OnDelete == "CASCADE"),
            "Expected SQLite ON DELETE CASCADE for Organisation.postalAddress.");
        AssertCondition(organisationForeignKeys.Any(fk => fk.From == "streetAddress" && fk.Table == "StreetAddress" && fk.OnDelete == "CASCADE"),
            "Expected SQLite ON DELETE CASCADE for Organisation.streetAddress.");

        var streetAddressForeignKeys = GetSqliteForeignKeys(context, "StreetAddress");
        AssertCondition(streetAddressForeignKeys.Any(fk => fk.From == "status" && fk.Table == "Status" && fk.OnDelete == "CASCADE"),
            "Expected SQLite ON DELETE CASCADE for StreetAddress.status.");
        AssertCondition(streetAddressForeignKeys.Any(fk => fk.From == "streetDetail" && fk.Table == "StreetDetail" && fk.OnDelete == "CASCADE"),
            "Expected SQLite ON DELETE CASCADE for StreetAddress.streetDetail.");
        AssertCondition(streetAddressForeignKeys.Any(fk => fk.From == "townDetail" && fk.Table == "TownDetail" && fk.OnDelete == "CASCADE"),
            "Expected SQLite ON DELETE CASCADE for StreetAddress.townDetail.");
    });
}

void VerifyDependentDeleteDirection()
{
    WithFreshDatabase(options =>
    {
        using var context = new SampleProfileDbContext(options);
        var organisation = new SampleProfile.Organisation
        {
            MRId = "org-dependent-001",
            Name = "Dependent Delete Utility",
            ElectronicAddress = new SampleProfile.ElectronicAddress { Email1 = "ops@example.com", UserID = "ops-user" },
            Phone1 = new SampleProfile.TelephoneNumber { ItuPhone = "+1-505-555-0100" },
            Phone2 = new SampleProfile.TelephoneNumber { ItuPhone = "+1-505-555-0101" },
            PostalAddress = new SampleProfile.StreetAddress
            {
                Status = new SampleProfile.Status { Value = "postal" },
                StreetDetail = new SampleProfile.StreetDetail { Number = "123", Name = "Postal Way", Type = "Road" },
                TownDetail = new SampleProfile.TownDetail { Name = "Postal Town" }
            },
            StreetAddress = new SampleProfile.StreetAddress
            {
                Status = new SampleProfile.Status { Value = "street" },
                StreetDetail = new SampleProfile.StreetDetail { Number = "456", Name = "Grid Way", Type = "Road" },
                TownDetail = new SampleProfile.TownDetail { Name = "Grid Town" }
            }
        };

        context.Organisations.Add(organisation);
        context.SaveChanges();

        var loaded = context.Organisations
            .Include(x => x.ElectronicAddress)
            .Include(x => x.Phone1)
            .Include(x => x.Phone2)
            .Include(x => x.PostalAddress).ThenInclude(x => x!.StreetDetail)
            .Include(x => x.PostalAddress).ThenInclude(x => x!.TownDetail)
            .Include(x => x.PostalAddress).ThenInclude(x => x!.Status)
            .Include(x => x.StreetAddress).ThenInclude(x => x!.StreetDetail)
            .Include(x => x.StreetAddress).ThenInclude(x => x!.TownDetail)
            .Include(x => x.StreetAddress).ThenInclude(x => x!.Status)
            .Single(x => x.MRId == "org-dependent-001");

        AssertCondition(loaded.ElectronicAddressId == loaded.ElectronicAddress?.Id, "ElectronicAddress FK should match related Id.");
        AssertCondition(loaded.Phone1Id == loaded.Phone1?.Id, "Phone1 FK should match related Id.");
        AssertCondition(loaded.Phone2Id == loaded.Phone2?.Id, "Phone2 FK should match related Id.");
        AssertCondition(loaded.PostalAddressId == loaded.PostalAddress?.Id, "PostalAddress FK should match related Id.");
        AssertCondition(loaded.StreetAddressId == loaded.StreetAddress?.Id, "StreetAddress FK should match related Id.");

        context.Remove(loaded);
        context.SaveChanges();

        AssertCondition(context.Organisations.Count() == 0, "Deleting Organisation should remove the dependent row.");
        AssertCondition(context.ElectronicAddresses.Count() == 0, "Deleting Organisation should clean up ElectronicAddress.");
        AssertCondition(context.TelephoneNumbers.Count() == 0, "Deleting Organisation should clean up TelephoneNumber rows.");
        AssertCondition(context.StreetAddresses.Count() == 0, "Deleting Organisation should clean up StreetAddress rows.");
        AssertCondition(context.Set<SampleProfile.Status>().Count() == 0, "Deleting Organisation should clean up Status rows.");
        AssertCondition(context.Set<SampleProfile.StreetDetail>().Count() == 0, "Deleting Organisation should clean up StreetDetail rows.");
        AssertCondition(context.Set<SampleProfile.TownDetail>().Count() == 0, "Deleting Organisation should clean up TownDetail rows.");
    });
}

void VerifyPrincipalDeleteCascade()
{
    VerifyOrganisationCompoundPrincipalDelete(
        "org-electronic",
        organisation => organisation.ElectronicAddress = new SampleProfile.ElectronicAddress { Email1 = "cascade-electronic@example.com" },
        context => context.ElectronicAddresses.Single(),
        "Deleting ElectronicAddress principal should cascade-delete Organisation.");

    VerifyOrganisationCompoundPrincipalDelete(
        "org-phone1",
        organisation => organisation.Phone1 = new SampleProfile.TelephoneNumber { ItuPhone = "+1-505-555-0110" },
        context => context.TelephoneNumbers.Single(),
        "Deleting Phone1 principal should cascade-delete Organisation.");

    VerifyOrganisationCompoundPrincipalDelete(
        "org-phone2",
        organisation => organisation.Phone2 = new SampleProfile.TelephoneNumber { ItuPhone = "+1-505-555-0111" },
        context => context.TelephoneNumbers.Single(),
        "Deleting Phone2 principal should cascade-delete Organisation.");

    VerifyOrganisationCompoundPrincipalDelete(
        "org-postal",
        organisation => organisation.PostalAddress = new SampleProfile.StreetAddress { StreetDetail = new SampleProfile.StreetDetail { Name = "Postal Cascade" } },
        context => context.StreetAddresses.Single(),
        "Deleting PostalAddress principal should cascade-delete Organisation.");

    VerifyOrganisationCompoundPrincipalDelete(
        "org-street",
        organisation => organisation.StreetAddress = new SampleProfile.StreetAddress { StreetDetail = new SampleProfile.StreetDetail { Name = "Street Cascade" } },
        context => context.StreetAddresses.Single(),
        "Deleting StreetAddress principal should cascade-delete Organisation.");

    VerifyStreetAddressCompoundPrincipalDelete(
        address => address.Status = new SampleProfile.Status { Value = "cascade-status" },
        context => context.Set<SampleProfile.Status>().Single(),
        "Deleting Status principal should cascade-delete StreetAddress.");

    VerifyStreetAddressCompoundPrincipalDelete(
        address => address.StreetDetail = new SampleProfile.StreetDetail { Name = "Cascade Detail" },
        context => context.Set<SampleProfile.StreetDetail>().Single(),
        "Deleting StreetDetail principal should cascade-delete StreetAddress.");

    VerifyStreetAddressCompoundPrincipalDelete(
        address => address.TownDetail = new SampleProfile.TownDetail { Name = "Cascade Town" },
        context => context.Set<SampleProfile.TownDetail>().Single(),
        "Deleting TownDetail principal should cascade-delete StreetAddress.");
}

void VerifyAsyncCompoundCleanup()
{
    WithFreshDatabase(options =>
    {
        using (var context = new SampleProfileDbContext(options))
        {
            context.Add(new SampleProfile.Organisation
            {
                MRId = "org-async-001",
                Name = "Async Utility",
                Phone1 = new SampleProfile.TelephoneNumber
                {
                    ItuPhone = "+1-555-0400"
                },
                StreetAddress = CreateAddressGraph("async-initial")
            });
            context.SaveChanges();
        }

        using (var context = new SampleProfileDbContext(options))
        {
            var loaded = context.Organisations
                .Include(x => x.Phone1)
                .Include(x => x.StreetAddress).ThenInclude(x => x!.StreetDetail)
                .Single(x => x.MRId == "org-async-001");

            loaded.Phone1 = new SampleProfile.TelephoneNumber
            {
                ItuPhone = "+1-555-0499"
            };
            loaded.StreetAddress = CreateAddressGraph("async-updated");

            context.SaveChangesAsync().GetAwaiter().GetResult();
        }

        using (var context = new SampleProfileDbContext(options))
        {
            var loaded = context.Organisations
                .Include(x => x.Phone1)
                .Include(x => x.StreetAddress).ThenInclude(x => x!.StreetDetail)
                .Single(x => x.MRId == "org-async-001");

            AssertCondition(loaded.Phone1?.ItuPhone == "+1-555-0499",
                "Expected async SaveChanges to persist the replacement Phone1 graph.");
            AssertCondition(loaded.StreetAddress?.StreetDetail?.Name == "async-updated",
                "Expected async SaveChanges to persist the replacement StreetAddress graph.");
            AssertCondition(context.TelephoneNumbers.Count() == 1,
                "Expected async SaveChanges cleanup to remove the replaced TelephoneNumber row.");
            AssertCondition(context.StreetAddresses.Count() == 1,
                "Expected async SaveChanges cleanup to remove the replaced StreetAddress row.");
            AssertCondition(context.Set<SampleProfile.Status>().Count() == 1,
                "Expected async SaveChanges cleanup to remove replaced nested Status rows.");
            AssertCondition(context.Set<SampleProfile.StreetDetail>().Count() == 1,
                "Expected async SaveChanges cleanup to remove replaced nested StreetDetail rows.");
            AssertCondition(context.Set<SampleProfile.TownDetail>().Count() == 1,
                "Expected async SaveChanges cleanup to remove replaced nested TownDetail rows.");
        }
    });
}

void VerifyDirectForeignKeyCompoundReplacement()
{
    WithFreshDatabase(options =>
    {
        string originalPhoneId;
        string replacementPhoneId;

        using (var context = new SampleProfileDbContext(options))
        {
            var originalPhone = new SampleProfile.TelephoneNumber
            {
                Id = "phone-direct-001",
                ItuPhone = "+1-555-0501"
            };
            var replacementPhone = new SampleProfile.TelephoneNumber
            {
                Id = "phone-direct-002",
                ItuPhone = "+1-555-0502"
            };

            context.Add(originalPhone);
            context.Add(replacementPhone);
            context.Add(new SampleProfile.Organisation
            {
                MRId = "org-direct-fk-001",
                Name = "Direct FK Utility",
                Phone1Id = originalPhone.Id
            });
            context.SaveChanges();

            originalPhoneId = originalPhone.Id;
            replacementPhoneId = replacementPhone.Id;
        }

        using (var context = new SampleProfileDbContext(options))
        {
            var loaded = context.Organisations.Single(x => x.MRId == "org-direct-fk-001");
            loaded.Phone1Id = replacementPhoneId;
            loaded.Phone1 = null;
            context.SaveChanges();
        }

        using (var context = new SampleProfileDbContext(options))
        {
            var loaded = context.Organisations
                .Include(x => x.Phone1)
                .Single(x => x.MRId == "org-direct-fk-001");

            AssertCondition(loaded.Phone1Id == replacementPhoneId,
                "Expected direct FK reassignment to persist the replacement Phone1Id.");
            AssertCondition(loaded.Phone1?.ItuPhone == "+1-555-0502",
                "Expected direct FK reassignment to resolve the replacement Phone1 navigation.");
            AssertCondition(!context.TelephoneNumbers.Any(x => x.Id == originalPhoneId),
                "Expected direct FK reassignment cleanup to remove the orphaned TelephoneNumber row.");
            AssertCondition(context.TelephoneNumbers.Count() == 1,
                "Expected direct FK reassignment to leave exactly one active TelephoneNumber row.");
        }
    });
}

void VerifyCompoundSlotOwnership()
{
    WithFreshDatabase(options =>
    {
        using (var context = new SampleProfileDbContext(options))
        {
            var sharedPhone = new SampleProfile.TelephoneNumber
            {
                Id = "phone-slot-shared",
                ItuPhone = "+1-555-0600"
            };

            context.Add(sharedPhone);
            context.Add(new SampleProfile.Organisation
            {
                MRId = "org-slot-001",
                Name = "Slot Ownership Utility",
                Phone1Id = sharedPhone.Id,
                Phone2Id = sharedPhone.Id
            });
            context.SaveChanges();
        }

        using (var context = new SampleProfileDbContext(options))
        {
            var loaded = context.Organisations.Single(x => x.MRId == "org-slot-001");
            if (loaded.Phone1Id == loaded.Phone2Id && loaded.Phone1Id is not null)
            {
                diagnostics.Add(
                    "OWNERSHIP GAP OBSERVED: the same TelephoneNumber row can be assigned to both Organisation.Phone1 and Organisation.Phone2, so one-compound-per-slot ownership is not fully enforced.");
            }
        }
    });

    WithFreshDatabase(options =>
    {
        using (var context = new SampleProfileDbContext(options))
        {
            var sharedAddress = CreateAddressGraph("slot-shared");
            sharedAddress.Id = "address-slot-shared";

            context.Add(sharedAddress);
            context.Add(new SampleProfile.Organisation
            {
                MRId = "org-slot-002",
                Name = "Address Slot Ownership Utility",
                PostalAddressId = sharedAddress.Id,
                StreetAddressId = sharedAddress.Id
            });
            context.SaveChanges();
        }

        using (var context = new SampleProfileDbContext(options))
        {
            var loaded = context.Organisations.Single(x => x.MRId == "org-slot-002");
            if (loaded.PostalAddressId == loaded.StreetAddressId && loaded.PostalAddressId is not null)
            {
                diagnostics.Add(
                    "OWNERSHIP GAP OBSERVED: the same StreetAddress row can be assigned to both Organisation.PostalAddress and Organisation.StreetAddress, so one-compound-per-slot ownership is not fully enforced.");
            }
        }
    });
}

void VerifyCrossOwnerCompoundOwnership()
{
    WithFreshDatabase(options =>
    {
        using (var context = new SampleProfileDbContext(options))
        {
            var sharedPhone1 = new SampleProfile.TelephoneNumber
            {
                Id = "phone-cross-owner-phone1",
                ItuPhone = "+1-555-0801"
            };

            context.Add(sharedPhone1);
            context.Add(new SampleProfile.Organisation
            {
                MRId = "org-cross-owner-phone1-a",
                Name = "Cross Owner Phone1 A",
                Phone1Id = sharedPhone1.Id
            });
            context.SaveChanges();
        }

        var duplicatePhone1Exception = TrySave(options, context =>
        {
            var sharedPhone1 = context.TelephoneNumbers.Single(x => x.Id == "phone-cross-owner-phone1");
            context.Add(new SampleProfile.Organisation
            {
                MRId = "org-cross-owner-phone1-b",
                Name = "Cross Owner Phone1 B",
                Phone1 = sharedPhone1
            });
        });

        AssertCondition(duplicatePhone1Exception is DbUpdateException,
            "Expected unique Phone1Id constraint to reject sharing one TelephoneNumber across owners in the same slot.");
    });

    WithFreshDatabase(options =>
    {
        using (var context = new SampleProfileDbContext(options))
        {
            var sharedPostalAddress = CreateAddressGraph("cross-owner-postal");
            sharedPostalAddress.Id = "address-cross-owner-postal";

            context.Add(sharedPostalAddress);
            context.Add(new SampleProfile.Organisation
            {
                MRId = "org-cross-owner-postal-a",
                Name = "Cross Owner Postal A",
                PostalAddressId = sharedPostalAddress.Id
            });
            context.SaveChanges();
        }

        var duplicatePostalException = TrySave(options, context =>
        {
            var sharedPostalAddress = context.StreetAddresses.Single(x => x.Id == "address-cross-owner-postal");
            context.Add(new SampleProfile.Organisation
            {
                MRId = "org-cross-owner-postal-b",
                Name = "Cross Owner Postal B",
                PostalAddress = sharedPostalAddress
            });
        });

        AssertCondition(duplicatePostalException is DbUpdateException,
            "Expected unique PostalAddressId constraint to reject sharing one StreetAddress across owners in the same slot.");
    });

    WithFreshDatabase(options =>
    {
        using (var context = new SampleProfileDbContext(options))
        {
            var sharedPhone = new SampleProfile.TelephoneNumber
            {
                Id = "phone-cross-owner-mixed-slot",
                ItuPhone = "+1-555-0802"
            };

            context.Add(sharedPhone);
            context.Add(new SampleProfile.Organisation
            {
                MRId = "org-cross-owner-mixed-slot-a",
                Name = "Cross Owner Mixed Slot A",
                Phone1Id = sharedPhone.Id
            });
            context.SaveChanges();
        }

        using (var context = new SampleProfileDbContext(options))
        {
            var sharedPhone = context.TelephoneNumbers.Single(x => x.Id == "phone-cross-owner-mixed-slot");
            context.Add(new SampleProfile.Organisation
            {
                MRId = "org-cross-owner-mixed-slot-b",
                Name = "Cross Owner Mixed Slot B",
                Phone2 = sharedPhone
            });
            context.SaveChanges();
        }

        using (var context = new SampleProfileDbContext(options))
        {
            var organisations = context.Organisations
                .Where(x => x.MRId == "org-cross-owner-mixed-slot-a" || x.MRId == "org-cross-owner-mixed-slot-b")
                .OrderBy(x => x.MRId)
                .ToList();

            AssertCondition(organisations.Count == 2,
                "Expected both cross-owner mixed-slot organisations to be stored.");

            if (organisations[0].Phone1Id == organisations[1].Phone2Id &&
                organisations[0].Phone1Id is not null)
            {
                diagnostics.Add(
                    "OWNERSHIP GAP OBSERVED: the same TelephoneNumber row can be shared across different organisations when one uses Phone1 and the other uses Phone2.");
            }
        }
    });

    WithFreshDatabase(options =>
    {
        using (var context = new SampleProfileDbContext(options))
        {
            var sharedAddress = CreateAddressGraph("cross-owner-mixed-address");
            sharedAddress.Id = "address-cross-owner-mixed-slot";

            context.Add(sharedAddress);
            context.Add(new SampleProfile.Organisation
            {
                MRId = "org-cross-owner-address-a",
                Name = "Cross Owner Address A",
                PostalAddressId = sharedAddress.Id
            });
            context.SaveChanges();
        }

        using (var context = new SampleProfileDbContext(options))
        {
            var sharedAddress = context.StreetAddresses.Single(x => x.Id == "address-cross-owner-mixed-slot");
            context.Add(new SampleProfile.Organisation
            {
                MRId = "org-cross-owner-address-b",
                Name = "Cross Owner Address B",
                StreetAddress = sharedAddress
            });
            context.SaveChanges();
        }

        using (var context = new SampleProfileDbContext(options))
        {
            var organisations = context.Organisations
                .Where(x => x.MRId == "org-cross-owner-address-a" || x.MRId == "org-cross-owner-address-b")
                .OrderBy(x => x.MRId)
                .ToList();

            AssertCondition(organisations.Count == 2,
                "Expected both cross-owner mixed-slot address organisations to be stored.");

            if (organisations[0].PostalAddressId == organisations[1].StreetAddressId &&
                organisations[0].PostalAddressId is not null)
            {
                diagnostics.Add(
                    "OWNERSHIP GAP OBSERVED: the same StreetAddress row can be shared across different organisations when one uses PostalAddress and the other uses StreetAddress.");
            }
        }
    });
}

void VerifyCompoundOwnershipHandoff()
{
    WithFreshDatabase(options =>
    {
        using (var context = new SampleProfileDbContext(options))
        {
            var sharedPhone = new SampleProfile.TelephoneNumber
            {
                Id = "phone-handoff-001",
                ItuPhone = "+1-555-0901"
            };

            context.Add(sharedPhone);
            context.Add(new SampleProfile.Organisation
            {
                MRId = "org-handoff-phone-a",
                Name = "Phone Handoff A",
                Phone1Id = sharedPhone.Id
            });
            context.Add(new SampleProfile.Organisation
            {
                MRId = "org-handoff-phone-b",
                Name = "Phone Handoff B"
            });
            context.SaveChanges();
        }

        using (var context = new SampleProfileDbContext(options))
        {
            var organisations = context.Organisations
                .Where(x => x.MRId == "org-handoff-phone-a" || x.MRId == "org-handoff-phone-b")
                .OrderBy(x => x.MRId)
                .ToList();

            var source = organisations[0];
            var target = organisations[1];
            var sharedPhoneId = source.Phone1Id;

            AssertCondition(!string.IsNullOrWhiteSpace(sharedPhoneId),
                "Expected source organisation to start with a Phone1 reference for handoff testing.");

            source.Phone1Id = null;
            source.Phone1 = null;
            target.Phone1Id = sharedPhoneId;
            target.Phone1 = null;

            context.SaveChanges();
        }

        using (var context = new SampleProfileDbContext(options))
        {
            var organisations = context.Organisations
                .Include(x => x.Phone1)
                .Where(x => x.MRId == "org-handoff-phone-a" || x.MRId == "org-handoff-phone-b")
                .OrderBy(x => x.MRId)
                .ToList();

            var source = organisations[0];
            var target = organisations[1];

            AssertCondition(source.Phone1Id is null,
                "Expected source Phone1 FK to clear after same-slot handoff.");
            AssertCondition(source.Phone1 is null,
                "Expected source Phone1 navigation to clear after same-slot handoff.");
            AssertCondition(target.Phone1Id == "phone-handoff-001",
                "Expected target Phone1 FK to receive the handed-off phone.");
            AssertCondition(target.Phone1?.ItuPhone == "+1-555-0901",
                "Expected handed-off phone to remain available after same-slot handoff.");
            AssertCondition(context.TelephoneNumbers.Count() == 1,
                "Expected same-slot handoff to keep exactly one TelephoneNumber row.");
        }
    });

    WithFreshDatabase(options =>
    {
        using (var context = new SampleProfileDbContext(options))
        {
            var sharedAddress = CreateAddressGraph("handoff-address");
            sharedAddress.Id = "address-handoff-001";

            context.Add(sharedAddress);
            context.Add(new SampleProfile.Organisation
            {
                MRId = "org-handoff-address-a",
                Name = "Address Handoff A",
                PostalAddressId = sharedAddress.Id
            });
            context.Add(new SampleProfile.Organisation
            {
                MRId = "org-handoff-address-b",
                Name = "Address Handoff B"
            });
            context.SaveChanges();
        }

        using (var context = new SampleProfileDbContext(options))
        {
            var organisations = context.Organisations
                .Where(x => x.MRId == "org-handoff-address-a" || x.MRId == "org-handoff-address-b")
                .OrderBy(x => x.MRId)
                .ToList();

            var source = organisations[0];
            var target = organisations[1];
            var sharedAddressId = source.PostalAddressId;

            AssertCondition(!string.IsNullOrWhiteSpace(sharedAddressId),
                "Expected source organisation to start with a PostalAddress reference for handoff testing.");

            source.PostalAddressId = null;
            source.PostalAddress = null;
            target.PostalAddressId = sharedAddressId;
            target.PostalAddress = null;

            context.SaveChanges();
        }

        using (var context = new SampleProfileDbContext(options))
        {
            var organisations = context.Organisations
                .Include(x => x.PostalAddress).ThenInclude(x => x!.Status)
                .Include(x => x.PostalAddress).ThenInclude(x => x!.StreetDetail)
                .Include(x => x.PostalAddress).ThenInclude(x => x!.TownDetail)
                .Where(x => x.MRId == "org-handoff-address-a" || x.MRId == "org-handoff-address-b")
                .OrderBy(x => x.MRId)
                .ToList();

            var source = organisations[0];
            var target = organisations[1];

            AssertCondition(source.PostalAddressId is null,
                "Expected source PostalAddress FK to clear after same-slot handoff.");
            AssertCondition(source.PostalAddress is null,
                "Expected source PostalAddress navigation to clear after same-slot handoff.");
            AssertCondition(target.PostalAddressId == "address-handoff-001",
                "Expected target PostalAddress FK to receive the handed-off address.");
            AssertCondition(target.PostalAddress?.StreetDetail?.Name == "handoff-address",
                "Expected handed-off address graph to remain intact after same-slot handoff.");
            AssertCondition(context.StreetAddresses.Count() == 1,
                "Expected same-slot address handoff to keep exactly one StreetAddress row.");
            AssertCondition(context.Set<SampleProfile.Status>().Count() == 1,
                "Expected same-slot address handoff to keep exactly one Status row.");
            AssertCondition(context.Set<SampleProfile.StreetDetail>().Count() == 1,
                "Expected same-slot address handoff to keep exactly one StreetDetail row.");
            AssertCondition(context.Set<SampleProfile.TownDetail>().Count() == 1,
                "Expected same-slot address handoff to keep exactly one TownDetail row.");
        }
    });
}

void VerifyCompoundMutationStress()
{
    WithFreshDatabase(options =>
    {
        using (var context = new SampleProfileDbContext(options))
        {
            var electronicAddress = new SampleProfile.ElectronicAddress
            {
                Id = "email-stress-001",
                Email1 = "stress@example.com"
            };
            var phone1 = new SampleProfile.TelephoneNumber
            {
                Id = "phone-stress-001",
                ItuPhone = "+1-555-1001"
            };
            var phone2 = new SampleProfile.TelephoneNumber
            {
                Id = "phone-stress-002",
                ItuPhone = "+1-555-1002"
            };
            var postalAddress = CreateAddressGraph("stress-postal-old");
            postalAddress.Id = "address-stress-postal-001";
            var streetAddress = CreateAddressGraph("stress-street-old");
            streetAddress.Id = "address-stress-street-001";

            context.Add(electronicAddress);
            context.Add(phone1);
            context.Add(phone2);
            context.Add(postalAddress);
            context.Add(streetAddress);
            context.Add(new SampleProfile.Organisation
            {
                MRId = "org-mutation-stress-a",
                Name = "Mutation Stress A",
                ElectronicAddressId = electronicAddress.Id,
                Phone1Id = phone1.Id,
                Phone2Id = phone2.Id,
                PostalAddressId = postalAddress.Id,
                StreetAddressId = streetAddress.Id
            });
            context.Add(new SampleProfile.Organisation
            {
                MRId = "org-mutation-stress-b",
                Name = "Mutation Stress B"
            });
            context.SaveChanges();
        }

        string oldStreetStatusId;
        string oldStreetDetailId;
        string oldStreetTownId;

        using (var context = new SampleProfileDbContext(options))
        {
            var source = context.Organisations
                .Include(x => x.ElectronicAddress)
                .Include(x => x.Phone1)
                .Include(x => x.Phone2)
                .Include(x => x.PostalAddress).ThenInclude(x => x!.Status)
                .Include(x => x.PostalAddress).ThenInclude(x => x!.StreetDetail)
                .Include(x => x.PostalAddress).ThenInclude(x => x!.TownDetail)
                .Include(x => x.StreetAddress).ThenInclude(x => x!.Status)
                .Include(x => x.StreetAddress).ThenInclude(x => x!.StreetDetail)
                .Include(x => x.StreetAddress).ThenInclude(x => x!.TownDetail)
                .Single(x => x.MRId == "org-mutation-stress-a");

            var target = context.Organisations.Single(x => x.MRId == "org-mutation-stress-b");

            oldStreetStatusId = source.StreetAddress!.StatusId!;
            oldStreetDetailId = source.StreetAddress.StreetDetailId!;
            oldStreetTownId = source.StreetAddress.TownDetailId!;

            source.ElectronicAddress = null;
            source.Phone1 = null;
            source.Phone1Id = null;
            source.Phone2 = new SampleProfile.TelephoneNumber
            {
                ItuPhone = "+1-555-1003"
            };
            source.PostalAddress = null;
            source.PostalAddressId = null;
            source.StreetAddress = CreateAddressGraph("stress-street-new");

            target.Phone1Id = "phone-stress-002";
            target.Phone1 = null;
            target.Phone2Id = "phone-stress-001";
            target.Phone2 = null;
            target.StreetAddressId = "address-stress-postal-001";
            target.StreetAddress = null;

            context.SaveChanges();
        }

        using (var context = new SampleProfileDbContext(options))
        {
            var organisations = context.Organisations
                .Include(x => x.Phone1)
                .Include(x => x.Phone2)
                .Include(x => x.StreetAddress).ThenInclude(x => x!.Status)
                .Include(x => x.StreetAddress).ThenInclude(x => x!.StreetDetail)
                .Include(x => x.StreetAddress).ThenInclude(x => x!.TownDetail)
                .Where(x => x.MRId == "org-mutation-stress-a" || x.MRId == "org-mutation-stress-b")
                .OrderBy(x => x.MRId)
                .ToList();

            var source = organisations[0];
            var target = organisations[1];

            AssertCondition(source.ElectronicAddressId is null,
                "Expected source ElectronicAddressId to clear in the compound mutation stress scenario.");
            AssertCondition(source.Phone1Id is null,
                "Expected source Phone1Id to clear in the compound mutation stress scenario.");
            AssertCondition(source.Phone2Id is not null && source.Phone2Id != "phone-stress-002",
                "Expected source Phone2 to be replaced with a newly generated compound row in the stress scenario.");
            AssertCondition(source.Phone2?.ItuPhone == "+1-555-1003",
                "Expected source replacement Phone2 to round-trip in the stress scenario.");
            AssertCondition(source.PostalAddressId is null,
                "Expected source PostalAddressId to clear in the compound mutation stress scenario.");
            AssertCondition(source.StreetAddressId is not null && source.StreetAddressId != "address-stress-street-001",
                "Expected source StreetAddress to be replaced with a newly generated graph in the stress scenario.");
            AssertCondition(source.StreetAddress?.StreetDetail?.Name == "stress-street-new",
                "Expected replacement StreetAddress graph to round-trip in the stress scenario.");

            AssertCondition(target.Phone1Id == "phone-stress-002",
                "Expected target Phone1 to receive the transferred original Phone2 row.");
            AssertCondition(target.Phone1?.ItuPhone == "+1-555-1002",
                "Expected target Phone1 to resolve the transferred original Phone2 row.");
            AssertCondition(target.Phone2Id == "phone-stress-001",
                "Expected target Phone2 to receive the transferred original Phone1 row.");
            AssertCondition(target.Phone2?.ItuPhone == "+1-555-1001",
                "Expected target Phone2 to resolve the transferred original Phone1 row.");
            AssertCondition(target.StreetAddressId == "address-stress-postal-001",
                "Expected target StreetAddress to receive the transferred original PostalAddress graph.");
            AssertCondition(target.StreetAddress?.StreetDetail?.Name == "stress-postal-old",
                "Expected transferred original PostalAddress graph to remain intact on the target.");

            AssertCondition(!context.ElectronicAddresses.Any(x => x.Id == "email-stress-001"),
                "Expected orphaned ElectronicAddress row to be removed in the compound mutation stress scenario.");
            AssertCondition(context.TelephoneNumbers.Count() == 3,
                "Expected compound mutation stress scenario to keep the two transferred phones plus one new replacement phone.");
            AssertCondition(context.TelephoneNumbers.Any(x => x.Id == "phone-stress-001"),
                "Expected transferred original Phone1 row to remain.");
            AssertCondition(context.TelephoneNumbers.Any(x => x.Id == "phone-stress-002"),
                "Expected transferred original Phone2 row to remain.");
            AssertCondition(context.TelephoneNumbers.Any(x => x.ItuPhone == "+1-555-1003"),
                "Expected replacement Phone2 row to remain.");

            AssertCondition(context.StreetAddresses.Count() == 2,
                "Expected compound mutation stress scenario to keep the transferred postal address and the replacement street address.");
            AssertCondition(context.StreetAddresses.Any(x => x.Id == "address-stress-postal-001"),
                "Expected transferred original PostalAddress graph to remain.");
            AssertCondition(!context.StreetAddresses.Any(x => x.Id == "address-stress-street-001"),
                "Expected replaced original StreetAddress graph to be removed.");
            AssertCondition(!context.Set<SampleProfile.Status>().Any(x => x.Id == oldStreetStatusId),
                "Expected nested Status row for the replaced original StreetAddress graph to be removed.");
            AssertCondition(!context.Set<SampleProfile.StreetDetail>().Any(x => x.Id == oldStreetDetailId),
                "Expected nested StreetDetail row for the replaced original StreetAddress graph to be removed.");
            AssertCondition(!context.Set<SampleProfile.TownDetail>().Any(x => x.Id == oldStreetTownId),
                "Expected nested TownDetail row for the replaced original StreetAddress graph to be removed.");
            AssertCondition(context.Set<SampleProfile.Status>().Count() == 2,
                "Expected compound mutation stress scenario to keep nested rows for the transferred postal graph and the replacement street graph.");
            AssertCondition(context.Set<SampleProfile.StreetDetail>().Count() == 2,
                "Expected compound mutation stress scenario to keep nested street-detail rows for the transferred postal graph and the replacement street graph.");
            AssertCondition(context.Set<SampleProfile.TownDetail>().Count() == 2,
                "Expected compound mutation stress scenario to keep nested town-detail rows for the transferred postal graph and the replacement street graph.");
        }
    });
}

void VerifyCompoundFailureRollbackSafety()
{
    WithFreshDatabase(options =>
    {
        using (var context = new SampleProfileDbContext(options))
        {
            context.Add(new SampleProfile.Organisation
            {
                MRId = "org-failure-phone-a",
                Name = "Failure Phone A",
                Phone1 = new SampleProfile.TelephoneNumber
                {
                    Id = "phone-failure-a",
                    ItuPhone = "+1-555-1101"
                }
            });
            context.Add(new SampleProfile.Organisation
            {
                MRId = "org-failure-phone-b",
                Name = "Failure Phone B",
                Phone1 = new SampleProfile.TelephoneNumber
                {
                    Id = "phone-failure-b",
                    ItuPhone = "+1-555-1102"
                }
            });
            context.SaveChanges();
        }

        Exception? failure = null;

        using (var context = new SampleProfileDbContext(options))
        {
            var organisations = context.Organisations
                .Include(x => x.Phone1)
                .Where(x => x.MRId == "org-failure-phone-a" || x.MRId == "org-failure-phone-b")
                .OrderBy(x => x.MRId)
                .ToList();

            var first = organisations[0];
            var second = organisations[1];

            second.Phone1 = new SampleProfile.TelephoneNumber
            {
                ItuPhone = "+1-555-1199"
            };
            second.Phone1Id = first.Phone1Id;

            try
            {
                context.SaveChanges();
            }
            catch (Exception ex)
            {
                failure = ex;
            }
        }

        AssertCondition(failure is DbUpdateException,
            "Expected duplicate same-slot phone assignment to fail with DbUpdateException in rollback-safety scenario.");

        using (var context = new SampleProfileDbContext(options))
        {
            var organisations = context.Organisations
                .Include(x => x.Phone1)
                .Where(x => x.MRId == "org-failure-phone-a" || x.MRId == "org-failure-phone-b")
                .OrderBy(x => x.MRId)
                .ToList();

            var first = organisations[0];
            var second = organisations[1];

            AssertCondition(first.Phone1Id == "phone-failure-a",
                "Expected first organisation Phone1 FK to remain unchanged after failed save.");
            AssertCondition(first.Phone1?.ItuPhone == "+1-555-1101",
                "Expected first organisation Phone1 row to remain unchanged after failed save.");
            AssertCondition(second.Phone1Id == "phone-failure-b",
                "Expected second organisation Phone1 FK to remain unchanged after failed save.");
            AssertCondition(second.Phone1?.ItuPhone == "+1-555-1102",
                "Expected second organisation original Phone1 row to remain after failed save.");
            AssertCondition(context.TelephoneNumbers.Count() == 2,
                "Expected failed phone save not to delete existing compound rows or persist the replacement row.");
            AssertCondition(!context.TelephoneNumbers.Any(x => x.ItuPhone == "+1-555-1199"),
                "Expected failed phone save not to persist the replacement TelephoneNumber row.");
        }
    });

    WithFreshDatabase(options =>
    {
        using (var context = new SampleProfileDbContext(options))
        {
            var addressA = CreateAddressGraph("failure-address-a");
            addressA.Id = "address-failure-a";
            var addressB = CreateAddressGraph("failure-address-b");
            addressB.Id = "address-failure-b";

            context.Add(new SampleProfile.Organisation
            {
                MRId = "org-failure-address-a",
                Name = "Failure Address A",
                StreetAddress = addressA
            });
            context.Add(new SampleProfile.Organisation
            {
                MRId = "org-failure-address-b",
                Name = "Failure Address B",
                StreetAddress = addressB
            });
            context.SaveChanges();
        }

        Exception? failure = null;

        using (var context = new SampleProfileDbContext(options))
        {
            var organisations = context.Organisations
                .Include(x => x.StreetAddress).ThenInclude(x => x!.Status)
                .Include(x => x.StreetAddress).ThenInclude(x => x!.StreetDetail)
                .Include(x => x.StreetAddress).ThenInclude(x => x!.TownDetail)
                .Where(x => x.MRId == "org-failure-address-a" || x.MRId == "org-failure-address-b")
                .OrderBy(x => x.MRId)
                .ToList();

            var first = organisations[0];
            var second = organisations[1];

            second.StreetAddress = CreateAddressGraph("failure-address-new");
            second.StreetAddressId = first.StreetAddressId;

            try
            {
                context.SaveChanges();
            }
            catch (Exception ex)
            {
                failure = ex;
            }
        }

        AssertCondition(failure is DbUpdateException,
            "Expected duplicate same-slot street-address assignment to fail with DbUpdateException in rollback-safety scenario.");

        using (var context = new SampleProfileDbContext(options))
        {
            var organisations = context.Organisations
                .Include(x => x.StreetAddress).ThenInclude(x => x!.StreetDetail)
                .Where(x => x.MRId == "org-failure-address-a" || x.MRId == "org-failure-address-b")
                .OrderBy(x => x.MRId)
                .ToList();

            var first = organisations[0];
            var second = organisations[1];

            AssertCondition(first.StreetAddressId == "address-failure-a",
                "Expected first organisation StreetAddress FK to remain unchanged after failed save.");
            AssertCondition(first.StreetAddress?.StreetDetail?.Name == "failure-address-a",
                "Expected first organisation StreetAddress graph to remain unchanged after failed save.");
            AssertCondition(second.StreetAddressId == "address-failure-b",
                "Expected second organisation StreetAddress FK to remain unchanged after failed save.");
            AssertCondition(second.StreetAddress?.StreetDetail?.Name == "failure-address-b",
                "Expected second organisation original StreetAddress graph to remain after failed save.");
            AssertCondition(context.StreetAddresses.Count() == 2,
                "Expected failed address save not to delete existing address graphs or persist the replacement graph.");
            AssertCondition(context.Set<SampleProfile.Status>().Count() == 2,
                "Expected failed address save not to delete nested Status rows.");
            AssertCondition(context.Set<SampleProfile.StreetDetail>().Count() == 2,
                "Expected failed address save not to delete nested StreetDetail rows.");
            AssertCondition(context.Set<SampleProfile.TownDetail>().Count() == 2,
                "Expected failed address save not to delete nested TownDetail rows.");
            AssertCondition(!context.Set<SampleProfile.StreetDetail>().Any(x => x.Name == "failure-address-new"),
                "Expected failed address save not to persist the replacement StreetAddress graph.");
        }
    });
}

void VerifyCompoundFailureRecovery()
{
    WithFreshDatabase(options =>
    {
        using (var context = new SampleProfileDbContext(options))
        {
            context.Add(new SampleProfile.Organisation
            {
                MRId = "org-recovery-phone-a",
                Name = "Recovery Phone A",
                Phone1 = new SampleProfile.TelephoneNumber
                {
                    Id = "phone-recovery-a",
                    ItuPhone = "+1-555-1201"
                }
            });
            context.Add(new SampleProfile.Organisation
            {
                MRId = "org-recovery-phone-b",
                Name = "Recovery Phone B",
                Phone1 = new SampleProfile.TelephoneNumber
                {
                    Id = "phone-recovery-b",
                    ItuPhone = "+1-555-1202"
                }
            });
            context.SaveChanges();
        }

        using (var context = new SampleProfileDbContext(options))
        {
            var organisations = context.Organisations
                .Include(x => x.Phone1)
                .Where(x => x.MRId == "org-recovery-phone-a" || x.MRId == "org-recovery-phone-b")
                .OrderBy(x => x.MRId)
                .ToList();

            var first = organisations[0];
            var second = organisations[1];

            second.Phone1Id = first.Phone1Id;
            second.Phone1 = null;

            Exception? failure = null;

            try
            {
                context.SaveChanges();
            }
            catch (Exception ex)
            {
                failure = ex;
            }

            AssertCondition(failure is DbUpdateException,
                "Expected same-context duplicate Phone1 assignment to fail before recovery.");

            second.Phone1 = new SampleProfile.TelephoneNumber
            {
                ItuPhone = "+1-555-1299"
            };
            second.Phone1Id = null;

            context.SaveChanges();
        }

        using (var context = new SampleProfileDbContext(options))
        {
            var organisations = context.Organisations
                .Include(x => x.Phone1)
                .Where(x => x.MRId == "org-recovery-phone-a" || x.MRId == "org-recovery-phone-b")
                .OrderBy(x => x.MRId)
                .ToList();

            var first = organisations[0];
            var second = organisations[1];

            AssertCondition(first.Phone1Id == "phone-recovery-a",
                "Expected first organisation Phone1 to remain unchanged after recovery save.");
            AssertCondition(first.Phone1?.ItuPhone == "+1-555-1201",
                "Expected first organisation Phone1 row to remain unchanged after recovery save.");
            AssertCondition(second.Phone1Id is not null && second.Phone1Id != "phone-recovery-b",
                "Expected second organisation Phone1 to be replaced with a new valid compound row after recovery save.");
            AssertCondition(second.Phone1?.ItuPhone == "+1-555-1299",
                "Expected second organisation replacement Phone1 to round-trip after recovery save.");
            AssertCondition(context.TelephoneNumbers.Count() == 2,
                "Expected recovery save to keep only the surviving original phone and the new replacement phone.");
            AssertCondition(!context.TelephoneNumbers.Any(x => x.Id == "phone-recovery-b"),
                "Expected recovery save to clean up the original second Phone1 compound after successful replacement.");
        }
    });

    WithFreshDatabase(options =>
    {
        using (var context = new SampleProfileDbContext(options))
        {
            var addressA = CreateAddressGraph("recovery-address-a");
            addressA.Id = "address-recovery-a";
            var addressB = CreateAddressGraph("recovery-address-b");
            addressB.Id = "address-recovery-b";

            context.Add(new SampleProfile.Organisation
            {
                MRId = "org-recovery-address-a",
                Name = "Recovery Address A",
                StreetAddress = addressA
            });
            context.Add(new SampleProfile.Organisation
            {
                MRId = "org-recovery-address-b",
                Name = "Recovery Address B",
                StreetAddress = addressB
            });
            context.SaveChanges();
        }

        string oldStatusId;
        string oldStreetDetailId;
        string oldTownDetailId;

        using (var context = new SampleProfileDbContext(options))
        {
            var organisations = context.Organisations
                .Include(x => x.StreetAddress).ThenInclude(x => x!.Status)
                .Include(x => x.StreetAddress).ThenInclude(x => x!.StreetDetail)
                .Include(x => x.StreetAddress).ThenInclude(x => x!.TownDetail)
                .Where(x => x.MRId == "org-recovery-address-a" || x.MRId == "org-recovery-address-b")
                .OrderBy(x => x.MRId)
                .ToList();

            var first = organisations[0];
            var second = organisations[1];

            oldStatusId = second.StreetAddress!.StatusId!;
            oldStreetDetailId = second.StreetAddress.StreetDetailId!;
            oldTownDetailId = second.StreetAddress.TownDetailId!;

            second.StreetAddressId = first.StreetAddressId;
            second.StreetAddress = null;

            Exception? failure = null;

            try
            {
                context.SaveChanges();
            }
            catch (Exception ex)
            {
                failure = ex;
            }

            AssertCondition(failure is DbUpdateException,
                "Expected same-context duplicate StreetAddress assignment to fail before recovery.");

            second.StreetAddress = CreateAddressGraph("recovery-address-new");
            second.StreetAddressId = null;

            context.SaveChanges();
        }

        using (var context = new SampleProfileDbContext(options))
        {
            var organisations = context.Organisations
                .Include(x => x.StreetAddress).ThenInclude(x => x!.Status)
                .Include(x => x.StreetAddress).ThenInclude(x => x!.StreetDetail)
                .Include(x => x.StreetAddress).ThenInclude(x => x!.TownDetail)
                .Where(x => x.MRId == "org-recovery-address-a" || x.MRId == "org-recovery-address-b")
                .OrderBy(x => x.MRId)
                .ToList();

            var first = organisations[0];
            var second = organisations[1];

            AssertCondition(first.StreetAddressId == "address-recovery-a",
                "Expected first organisation StreetAddress to remain unchanged after recovery save.");
            AssertCondition(first.StreetAddress?.StreetDetail?.Name == "recovery-address-a",
                "Expected first organisation StreetAddress graph to remain unchanged after recovery save.");
            AssertCondition(second.StreetAddressId is not null && second.StreetAddressId != "address-recovery-b",
                "Expected second organisation StreetAddress to be replaced with a new valid graph after recovery save.");
            AssertCondition(second.StreetAddress?.StreetDetail?.Name == "recovery-address-new",
                "Expected second organisation replacement StreetAddress graph to round-trip after recovery save.");
            AssertCondition(context.StreetAddresses.Count() == 2,
                "Expected recovery save to keep only the surviving original address graph and the new replacement graph.");
            AssertCondition(!context.StreetAddresses.Any(x => x.Id == "address-recovery-b"),
                "Expected recovery save to clean up the original second StreetAddress graph after successful replacement.");
            AssertCondition(!context.Set<SampleProfile.Status>().Any(x => x.Id == oldStatusId),
                "Expected recovery save to clean up the original second StreetAddress Status row.");
            AssertCondition(!context.Set<SampleProfile.StreetDetail>().Any(x => x.Id == oldStreetDetailId),
                "Expected recovery save to clean up the original second StreetAddress StreetDetail row.");
            AssertCondition(!context.Set<SampleProfile.TownDetail>().Any(x => x.Id == oldTownDetailId),
                "Expected recovery save to clean up the original second StreetAddress TownDetail row.");
            AssertCondition(context.Set<SampleProfile.Status>().Count() == 2,
                "Expected recovery save to leave exactly two active Status rows.");
            AssertCondition(context.Set<SampleProfile.StreetDetail>().Count() == 2,
                "Expected recovery save to leave exactly two active StreetDetail rows.");
            AssertCondition(context.Set<SampleProfile.TownDetail>().Count() == 2,
                "Expected recovery save to leave exactly two active TownDetail rows.");
        }
    });
}

void VerifyGeneratedMappingBaseline()
{
    WithFreshGeneratedOnlyDatabase(options =>
    {
        string originalPhoneId;
        string replacementPhoneId;
        string originalStreetAddressId;

        using (var context = new GeneratedOnlySampleProfileDbContext(options))
        {
            var organisation = new SampleProfile.Organisation
            {
                MRId = "org-generated-baseline-001",
                Name = "Generated Baseline Utility",
                Phone1 = new SampleProfile.TelephoneNumber
                {
                    ItuPhone = "+1-555-0700"
                },
                StreetAddress = CreateAddressGraph("generated-baseline-initial")
            };

            context.Add(organisation);
            context.SaveChanges();

            originalPhoneId = organisation.Phone1Id!;
            originalStreetAddressId = organisation.StreetAddressId!;
        }

        using (var context = new GeneratedOnlySampleProfileDbContext(options))
        {
            var loaded = context.Organisations
                .Include(x => x.Phone1)
                .Include(x => x.StreetAddress)
                .ThenInclude(x => x!.StreetDetail)
                .Single(x => x.MRId == "org-generated-baseline-001");

            loaded.Phone1 = new SampleProfile.TelephoneNumber
            {
                ItuPhone = "+1-555-0799"
            };
            loaded.StreetAddress = CreateAddressGraph("generated-baseline-updated");

            context.SaveChanges();
            replacementPhoneId = loaded.Phone1Id!;
        }

        using (var context = new GeneratedOnlySampleProfileDbContext(options))
        {
            AssertCondition(context.Organisations.Count() == 1,
                "Expected generated-only baseline to keep the owner Organisation row.");
            AssertCondition(context.TelephoneNumbers.Count() == 2,
                "Expected generated-only baseline to leave the replaced TelephoneNumber row behind.");
            AssertCondition(context.StreetAddresses.Count() == 2,
                "Expected generated-only baseline to leave the replaced StreetAddress row behind.");
            AssertCondition(context.Set<SampleProfile.Status>().Count() == 2,
                "Expected generated-only baseline to leave replaced nested Status rows behind.");
            AssertCondition(context.Set<SampleProfile.StreetDetail>().Count() == 2,
                "Expected generated-only baseline to leave replaced nested StreetDetail rows behind.");
            AssertCondition(context.Set<SampleProfile.TownDetail>().Count() == 2,
                "Expected generated-only baseline to leave replaced nested TownDetail rows behind.");
            diagnostics.Add(
                "BASELINE OBSERVED: without the SaveChanges cleanup helper, generated EF mapping leaves replaced compound graphs orphaned.");

            var loaded = context.Organisations
                .Include(x => x.Phone1)
                .Single(x => x.MRId == "org-generated-baseline-001");
            AssertCondition(loaded.Phone1Id == replacementPhoneId && loaded.Phone1?.ItuPhone == "+1-555-0799",
                "Expected generated-only baseline to still point at the latest active Phone1 row.");
        }

        using (var context = new GeneratedOnlySampleProfileDbContext(options))
        {
            var loaded = context.Organisations.Single(x => x.MRId == "org-generated-baseline-001");
            context.Remove(loaded);
            context.SaveChanges();
        }

        using (var context = new GeneratedOnlySampleProfileDbContext(options))
        {
            AssertCondition(context.Organisations.Count() == 0,
                "Expected generated-only baseline delete to remove the owner Organisation row.");
            AssertCondition(context.TelephoneNumbers.Count() == 2,
                "Expected generated-only baseline delete to leave compound TelephoneNumber rows behind.");
            AssertCondition(context.StreetAddresses.Count() == 2,
                "Expected generated-only baseline delete to leave compound StreetAddress rows behind.");
            AssertCondition(context.Set<SampleProfile.Status>().Count() == 2,
                "Expected generated-only baseline delete to leave nested Status rows behind.");
            AssertCondition(context.Set<SampleProfile.StreetDetail>().Count() == 2,
                "Expected generated-only baseline delete to leave nested StreetDetail rows behind.");
            AssertCondition(context.Set<SampleProfile.TownDetail>().Count() == 2,
                "Expected generated-only baseline delete to leave nested TownDetail rows behind.");
            diagnostics.Add(
                "BASELINE OBSERVED: without the SaveChanges cleanup helper, deleting the owner Organisation leaves compound graphs behind.");
        }

        using (var context = new GeneratedOnlySampleProfileDbContext(options))
        {
            context.Add(new SampleProfile.Organisation
            {
                MRId = "org-generated-baseline-002",
                Name = "Generated Baseline Cascade Utility",
                Phone1 = new SampleProfile.TelephoneNumber
                {
                    ItuPhone = "+1-555-0710"
                }
            });
            context.SaveChanges();
        }

        using (var context = new GeneratedOnlySampleProfileDbContext(options))
        {
            var principal = context.TelephoneNumbers.Single(x => x.ItuPhone == "+1-555-0710");
            context.Remove(principal);
            context.SaveChanges();
        }

        using (var context = new GeneratedOnlySampleProfileDbContext(options))
        {
            AssertCondition(context.Organisations.Count() == 0,
                "Expected generated-only baseline principal delete to cascade-remove the Organisation row.");
            AssertCondition(context.IdentifiedObjects.Count() == 1,
                "Expected generated-only baseline principal delete to leave the IdentifiedObject base row behind.");
            diagnostics.Add(
                "BASELINE OBSERVED: without the SaveChanges cleanup helper, deleting a compound principal still cascades into the owner and leaves the IdentifiedObject base row behind.");
        }

        _ = originalPhoneId;
        _ = originalStreetAddressId;
    });
}

void VerifyCompoundNullDetachCleanup()
{
    WithFreshDatabase(options =>
    {
        using (var context = new SampleProfileDbContext(options))
        {
            context.Add(new SampleProfile.Organisation
            {
                MRId = "org-null-detach-001",
                Name = "Null Detach Utility",
                ElectronicAddress = new SampleProfile.ElectronicAddress
                {
                    Email1 = "detach@example.com"
                },
                Phone2 = new SampleProfile.TelephoneNumber
                {
                    ItuPhone = "+1-555-0710"
                },
                PostalAddress = CreateAddressGraph("postal-null-detach"),
                StreetAddress = CreateAddressGraph("street-null-detach")
            });
            context.SaveChanges();
        }

        using (var context = new SampleProfileDbContext(options))
        {
            var loaded = context.Organisations
                .Include(x => x.ElectronicAddress)
                .Include(x => x.Phone2)
                .Include(x => x.PostalAddress).ThenInclude(x => x!.Status)
                .Include(x => x.PostalAddress).ThenInclude(x => x!.StreetDetail)
                .Include(x => x.PostalAddress).ThenInclude(x => x!.TownDetail)
                .Include(x => x.StreetAddress).ThenInclude(x => x!.Status)
                .Include(x => x.StreetAddress).ThenInclude(x => x!.StreetDetail)
                .Include(x => x.StreetAddress).ThenInclude(x => x!.TownDetail)
                .Single(x => x.MRId == "org-null-detach-001");

            loaded.ElectronicAddress = null;
            loaded.Phone2 = null;
            loaded.PostalAddress = null;
            loaded.StreetAddress = null;

            context.SaveChanges();
        }

        using (var context = new SampleProfileDbContext(options))
        {
            var loaded = context.Organisations.Single(x => x.MRId == "org-null-detach-001");

            AssertCondition(loaded.ElectronicAddressId is null,
                "Expected ElectronicAddressId to clear when the navigation is detached.");
            AssertCondition(loaded.Phone2Id is null,
                "Expected Phone2Id to clear when the navigation is detached.");
            AssertCondition(loaded.PostalAddressId is null,
                "Expected PostalAddressId to clear when the navigation is detached.");
            AssertCondition(loaded.StreetAddressId is null,
                "Expected StreetAddressId to clear when the navigation is detached.");

            AssertCondition(context.ElectronicAddresses.Count() == 0,
                "Expected detached ElectronicAddress rows to be cleaned up by the helper context.");
            AssertCondition(context.TelephoneNumbers.Count() == 0,
                "Expected detached TelephoneNumber rows to be cleaned up by the helper context.");
            AssertCondition(context.StreetAddresses.Count() == 0,
                "Expected detached StreetAddress rows to be cleaned up by the helper context.");
            AssertCondition(context.Set<SampleProfile.Status>().Count() == 0,
                "Expected nested detached Status rows to be cleaned up by the helper context.");
            AssertCondition(context.Set<SampleProfile.StreetDetail>().Count() == 0,
                "Expected nested detached StreetDetail rows to be cleaned up by the helper context.");
            AssertCondition(context.Set<SampleProfile.TownDetail>().Count() == 0,
                "Expected nested detached TownDetail rows to be cleaned up by the helper context.");
        }
    });
}

void VerifyGeneratedNullDetachBaseline()
{
    WithFreshGeneratedOnlyDatabase(options =>
    {
        using (var context = new GeneratedOnlySampleProfileDbContext(options))
        {
            context.Add(new SampleProfile.Organisation
            {
                MRId = "org-generated-null-detach-001",
                Name = "Generated Null Detach Utility",
                ElectronicAddress = new SampleProfile.ElectronicAddress
                {
                    Email1 = "baseline-detach@example.com"
                },
                Phone2 = new SampleProfile.TelephoneNumber
                {
                    ItuPhone = "+1-555-0720"
                },
                PostalAddress = CreateAddressGraph("postal-generated-null-detach"),
                StreetAddress = CreateAddressGraph("street-generated-null-detach")
            });
            context.SaveChanges();
        }

        using (var context = new GeneratedOnlySampleProfileDbContext(options))
        {
            var loaded = context.Organisations
                .Include(x => x.ElectronicAddress)
                .Include(x => x.Phone2)
                .Include(x => x.PostalAddress).ThenInclude(x => x!.Status)
                .Include(x => x.PostalAddress).ThenInclude(x => x!.StreetDetail)
                .Include(x => x.PostalAddress).ThenInclude(x => x!.TownDetail)
                .Include(x => x.StreetAddress).ThenInclude(x => x!.Status)
                .Include(x => x.StreetAddress).ThenInclude(x => x!.StreetDetail)
                .Include(x => x.StreetAddress).ThenInclude(x => x!.TownDetail)
                .Single(x => x.MRId == "org-generated-null-detach-001");

            loaded.ElectronicAddress = null;
            loaded.Phone2 = null;
            loaded.PostalAddress = null;
            loaded.StreetAddress = null;

            context.SaveChanges();
        }

        using (var context = new GeneratedOnlySampleProfileDbContext(options))
        {
            var loaded = context.Organisations.Single(x => x.MRId == "org-generated-null-detach-001");

            AssertCondition(loaded.ElectronicAddressId is null,
                "Expected generated-only ElectronicAddressId to clear when the navigation is detached.");
            AssertCondition(loaded.Phone2Id is null,
                "Expected generated-only Phone2Id to clear when the navigation is detached.");
            AssertCondition(loaded.PostalAddressId is null,
                "Expected generated-only PostalAddressId to clear when the navigation is detached.");
            AssertCondition(loaded.StreetAddressId is null,
                "Expected generated-only StreetAddressId to clear when the navigation is detached.");

            if (context.ElectronicAddresses.Count() != 0)
            {
                diagnostics.Add("KNOWN ISSUE REPRODUCED: generated-only null detaching Organisation.ElectronicAddress leaves orphan ElectronicAddress rows behind.");
            }

            if (context.TelephoneNumbers.Count() != 0)
            {
                diagnostics.Add("KNOWN ISSUE REPRODUCED: generated-only null detaching Organisation.Phone2 leaves orphan TelephoneNumber rows behind.");
            }

            if (context.StreetAddresses.Count() != 0 ||
                context.Set<SampleProfile.Status>().Count() != 0 ||
                context.Set<SampleProfile.StreetDetail>().Count() != 0 ||
                context.Set<SampleProfile.TownDetail>().Count() != 0)
            {
                diagnostics.Add("KNOWN ISSUE REPRODUCED: generated-only null detaching Organisation address graphs leaves orphan StreetAddress/Status/StreetDetail/TownDetail rows behind.");
            }
        }
    });
}

void VerifyIndependentRelationships()
{
    WithFreshDatabase(options =>
    {
        using (var context = new SampleProfileDbContext(options))
        {
            context.Add(new SampleProfile.ParentOrganization { MRId = "org-parent-001", Name = "Parent Utility" });
            context.Add(new SampleProfile.Organisation
            {
                MRId = "org-child-001",
                Name = "Child Utility",
                ParentOrganisation = context.ParentOrganizations.Local.Single(x => x.MRId == "org-parent-001")
            });
            context.Add(new SampleProfile.ShuntCompensatorControl { Id = "control-001", SensingPhaseCode = "ABC" });
            context.Add(new SampleProfile.ShuntCompensatorInfo
            {
                MRId = "info-001",
                Name = "Shunt Info",
                ShuntCompensatorControl = context.Set<SampleProfile.ShuntCompensatorControl>().Local.Single(x => x.Id == "control-001")
            });
            context.SaveChanges();
        }

        using (var context = new SampleProfileDbContext(options))
        {
            var loadedChild = context.Organisations.Include(x => x.ParentOrganisation).Single(x => x.MRId == "org-child-001");
            AssertCondition(loadedChild.ParentOrganisation?.MRId == "org-parent-001",
                "ParentOrganisation should round-trip through EF Core.");

            var loadedInfo = context.Set<SampleProfile.ShuntCompensatorInfo>()
                .Include(x => x.ShuntCompensatorControl)
                .Single(x => x.MRId == "info-001");
            AssertCondition(loadedInfo.ShuntCompensatorControl?.Id == "control-001",
                "ShuntCompensatorControl should round-trip through EF Core.");
        }

        var deleteParentException = TryDelete(options, context => context.ParentOrganizations.Single(x => x.MRId == "org-parent-001"));
        var deleteControlException = TryDelete(options, context => context.Set<SampleProfile.ShuntCompensatorControl>().Single(x => x.Id == "control-001"));

        AssertCondition(deleteParentException is DbUpdateException,
            "Deleting a referenced ParentOrganization should fail with DbUpdateException.");
        AssertCondition(deleteControlException is DbUpdateException,
            "Deleting a referenced ShuntCompensatorControl should fail with DbUpdateException.");

        using var verificationContext = new SampleProfileDbContext(options);
        AssertCondition(verificationContext.ParentOrganizations.Count() == 1, "ParentOrganization should remain after failed delete.");
        AssertCondition(verificationContext.Organisations.Count() == 2, "Child Organisation should remain after failed parent delete.");
        AssertCondition(verificationContext.Set<SampleProfile.ShuntCompensatorControl>().Count() == 1,
            "ShuntCompensatorControl should remain after failed delete.");
        AssertCondition(verificationContext.Set<SampleProfile.ShuntCompensatorInfo>().Count() == 1,
            "ShuntCompensatorInfo should remain after failed control delete.");
    });
}

void VerifyInheritanceStorage()
{
    WithFreshDatabase(options =>
    {
        using var context = new SampleProfileDbContext(options);
        context.Add(new SampleProfile.ParentOrganization { MRId = "inherit-parent-001", Name = "Parent Utility" });
        context.Add(new SampleProfile.OverheadWireInfo { MRId = "wire-001", Name = "Overhead Wire" });
        context.SaveChanges();

        AssertCondition(GetTableRowCount(context, "IdentifiedObject") == 2,
            "Expected two rows in IdentifiedObject for the inserted inheritance graph.");
        AssertCondition(GetTableRowCount(context, "Organisation") == 1,
            "Expected one row in Organisation for ParentOrganization.");
        AssertCondition(GetTableRowCount(context, "ParentOrganization") == 1,
            "Expected one row in ParentOrganization.");
        AssertCondition(GetTableRowCount(context, "AssetInfo") == 1,
            "Expected one row in AssetInfo for OverheadWireInfo.");
        AssertCondition(GetTableRowCount(context, "WireInfo") == 1,
            "Expected one row in WireInfo for OverheadWireInfo.");
        AssertCondition(GetTableRowCount(context, "OverheadWireInfo") == 1,
            "Expected one row in OverheadWireInfo.");
    });
}

void VerifyInheritanceDeleteCleanup()
{
    VerifyInheritanceDeleteScenario(
        "inherit-delete-parent-derived",
        context => context.Add(new SampleProfile.ParentOrganization
        {
            MRId = "inherit-delete-parent-derived",
            Name = "Derived Parent"
        }),
        context => context.ParentOrganizations.Single(x => x.MRId == "inherit-delete-parent-derived"),
        new (string Table, int ExpectedCount)[]
        {
            ("ParentOrganization", 0),
            ("Organisation", 0),
            ("IdentifiedObject", 0)
        });

    VerifyInheritanceDeleteScenario(
        "inherit-delete-parent-base",
        context => context.Add(new SampleProfile.ParentOrganization
        {
            MRId = "inherit-delete-parent-base",
            Name = "Base Loaded Parent"
        }),
        context => context.Set<SampleProfile.IdentifiedObject>().Single(x => x.MRId == "inherit-delete-parent-base"),
        new (string Table, int ExpectedCount)[]
        {
            ("ParentOrganization", 0),
            ("Organisation", 0),
            ("IdentifiedObject", 0)
        });

    VerifyInheritanceDeleteScenario(
        "inherit-delete-wire-derived",
        context => context.Add(new SampleProfile.OverheadWireInfo
        {
            MRId = "inherit-delete-wire-derived",
            Name = "Derived Wire",
            Material = "aluminum"
        }),
        context => context.Set<SampleProfile.OverheadWireInfo>().Single(x => x.MRId == "inherit-delete-wire-derived"),
        new (string Table, int ExpectedCount)[]
        {
            ("OverheadWireInfo", 0),
            ("WireInfo", 0),
            ("AssetInfo", 0),
            ("IdentifiedObject", 0)
        });

    VerifyInheritanceDeleteScenario(
        "inherit-delete-wire-base",
        context => context.Add(new SampleProfile.OverheadWireInfo
        {
            MRId = "inherit-delete-wire-base",
            Name = "Base Loaded Wire",
            Material = "copper"
        }),
        context => context.Set<SampleProfile.AssetInfo>().Single(x => x.MRId == "inherit-delete-wire-base"),
        new (string Table, int ExpectedCount)[]
        {
            ("OverheadWireInfo", 0),
            ("WireInfo", 0),
            ("AssetInfo", 0),
            ("IdentifiedObject", 0)
        });
}

void VerifyInheritanceQueryMaterialization()
{
    WithFreshDatabase(options =>
    {
        using (var context = new SampleProfileDbContext(options))
        {
            context.Add(new SampleProfile.Organisation
            {
                MRId = "org-base-001",
                Name = "Base Organisation"
            });
            context.Add(new SampleProfile.ParentOrganization
            {
                MRId = "org-parent-010",
                Name = "Derived Parent Organisation"
            });
            context.Add(new SampleProfile.WireSpacingInfo
            {
                MRId = "asset-spacing-001",
                Name = "Wire Spacing Asset"
            });
            context.Add(new SampleProfile.OverheadWireInfo
            {
                MRId = "asset-overhead-001",
                Name = "Overhead Wire Asset",
                Material = "aluminum",
                Gmr = 0.42
            });
            context.SaveChanges();
        }

        using (var context = new SampleProfileDbContext(options))
        {
            var identifiedObjects = context.Set<SampleProfile.IdentifiedObject>()
                .OrderBy(x => x.MRId)
                .ToList();

            AssertCondition(identifiedObjects.Count == 4,
                "Expected four rows when querying the IdentifiedObject base set.");
            AssertCondition(identifiedObjects.Any(x => x.GetType() == typeof(SampleProfile.Organisation)),
                "Expected base-set query to materialize Organisation rows.");
            AssertCondition(identifiedObjects.Any(x => x.GetType() == typeof(SampleProfile.ParentOrganization)),
                "Expected base-set query to materialize ParentOrganization rows.");
            AssertCondition(identifiedObjects.Any(x => x.GetType() == typeof(SampleProfile.WireSpacingInfo)),
                "Expected base-set query to materialize WireSpacingInfo rows.");
            AssertCondition(identifiedObjects.Any(x => x.GetType() == typeof(SampleProfile.OverheadWireInfo)),
                "Expected base-set query to materialize OverheadWireInfo rows.");

            var assetInfos = context.Set<SampleProfile.AssetInfo>()
                .OrderBy(x => x.MRId)
                .ToList();

            AssertCondition(assetInfos.Count == 2,
                "Expected two rows when querying the AssetInfo base set.");
            AssertCondition(assetInfos.Any(x => x is SampleProfile.WireSpacingInfo),
                "Expected AssetInfo base-set query to include WireSpacingInfo.");
            AssertCondition(assetInfos.Any(x => x is SampleProfile.OverheadWireInfo),
                "Expected AssetInfo base-set query to include OverheadWireInfo.");

            var overheadFromWireInfo = context.Set<SampleProfile.WireInfo>()
                .Single(x => x.MRId == "asset-overhead-001");

            AssertCondition(overheadFromWireInfo is SampleProfile.OverheadWireInfo,
                "Expected WireInfo query to materialize OverheadWireInfo for derived rows.");

            ((SampleProfile.OverheadWireInfo)overheadFromWireInfo).Material = "copper";
            context.SaveChanges();
        }

        using (var context = new SampleProfileDbContext(options))
        {
            var reloaded = context.Set<SampleProfile.WireInfo>()
                .Single(x => x.MRId == "asset-overhead-001");

            AssertCondition(reloaded is SampleProfile.OverheadWireInfo,
                "Expected reloaded WireInfo row to remain materialized as OverheadWireInfo.");
            AssertCondition(reloaded.Material == "copper",
                "Expected updates made through a base-set materialized derived instance to persist.");
        }
    });
}

void VerifyRepeatedCompoundReplacementCycles()
{
    WithFreshDatabase(options =>
    {
        using (var context = new SampleProfileDbContext(options))
        {
            context.Add(new SampleProfile.Organisation
            {
                MRId = "org-replace-cycles-001",
                Name = "Replacement Cycle Utility",
                Phone1 = new SampleProfile.TelephoneNumber
                {
                    ItuPhone = "+1-555-0300"
                }
            });
            context.SaveChanges();
        }

        string? latestPhoneId = null;

        for (var cycle = 1; cycle <= 4; cycle++)
        {
            using var context = new SampleProfileDbContext(options);
            var loaded = context.Organisations
                .Include(x => x.Phone1)
                .Single(x => x.MRId == "org-replace-cycles-001");

            loaded.Phone1 = new SampleProfile.TelephoneNumber
            {
                ItuPhone = $"+1-555-03{cycle:D2}"
            };

            context.SaveChanges();
            latestPhoneId = loaded.Phone1Id;
        }

        using (var context = new SampleProfileDbContext(options))
        {
            var loaded = context.Organisations
                .Include(x => x.Phone1)
                .Single(x => x.MRId == "org-replace-cycles-001");

            AssertCondition(loaded.Phone1Id == latestPhoneId,
                "Expected repeated Phone1 replacement cycles to keep the latest FK value.");
            AssertCondition(loaded.Phone1?.ItuPhone == "+1-555-0304",
                "Expected the latest Phone1 replacement value to round-trip after multiple cycles.");
            AssertCondition(context.TelephoneNumbers.Count() == 1,
                "Expected repeated Phone1 replacement cycles to keep only the active TelephoneNumber row.");
        }
    });
}

void VerifyRepeatedAddressReplacementCycles()
{
    WithFreshDatabase(options =>
    {
        using (var context = new SampleProfileDbContext(options))
        {
            context.Add(new SampleProfile.Organisation
            {
                MRId = "org-address-cycles-001",
                Name = "Address Replacement Utility",
                PostalAddress = CreateAddressGraph("postal-initial"),
                StreetAddress = CreateAddressGraph("street-initial")
            });
            context.SaveChanges();
        }

        string? latestPostalId = null;
        string? latestStreetId = null;

        for (var cycle = 1; cycle <= 3; cycle++)
        {
            using var context = new SampleProfileDbContext(options);
            var loaded = context.Organisations
                .Include(x => x.PostalAddress)
                .ThenInclude(x => x!.StreetDetail)
                .Include(x => x.StreetAddress)
                .ThenInclude(x => x!.StreetDetail)
                .Single(x => x.MRId == "org-address-cycles-001");

            loaded.PostalAddress = CreateAddressGraph($"postal-cycle-{cycle}");
            loaded.StreetAddress = CreateAddressGraph($"street-cycle-{cycle}");

            context.SaveChanges();
            latestPostalId = loaded.PostalAddressId;
            latestStreetId = loaded.StreetAddressId;
        }

        using (var context = new SampleProfileDbContext(options))
        {
            var loaded = context.Organisations
                .Include(x => x.PostalAddress).ThenInclude(x => x!.Status)
                .Include(x => x.PostalAddress).ThenInclude(x => x!.StreetDetail)
                .Include(x => x.PostalAddress).ThenInclude(x => x!.TownDetail)
                .Include(x => x.StreetAddress).ThenInclude(x => x!.Status)
                .Include(x => x.StreetAddress).ThenInclude(x => x!.StreetDetail)
                .Include(x => x.StreetAddress).ThenInclude(x => x!.TownDetail)
                .Single(x => x.MRId == "org-address-cycles-001");

            AssertCondition(loaded.PostalAddressId == latestPostalId,
                "Expected repeated PostalAddress replacement cycles to keep the latest FK value.");
            AssertCondition(loaded.StreetAddressId == latestStreetId,
                "Expected repeated StreetAddress replacement cycles to keep the latest FK value.");
            AssertCondition(loaded.PostalAddress?.StreetDetail?.Name == "postal-cycle-3",
                "Expected latest PostalAddress replacement graph to round-trip after multiple cycles.");
            AssertCondition(loaded.StreetAddress?.StreetDetail?.Name == "street-cycle-3",
                "Expected latest StreetAddress replacement graph to round-trip after multiple cycles.");
            AssertCondition(context.StreetAddresses.Count() == 2,
                "Expected repeated Organisation address replacements to keep only the active StreetAddress graphs.");
            AssertCondition(context.Set<SampleProfile.Status>().Count() == 2,
                "Expected repeated Organisation address replacements to keep only the active Status rows.");
            AssertCondition(context.Set<SampleProfile.StreetDetail>().Count() == 2,
                "Expected repeated Organisation address replacements to keep only the active StreetDetail rows.");
            AssertCondition(context.Set<SampleProfile.TownDetail>().Count() == 2,
                "Expected repeated Organisation address replacements to keep only the active TownDetail rows.");
        }
    });
}

SampleProfile.StreetAddress CreateAddressGraph(string label)
{
    return new SampleProfile.StreetAddress
    {
        Status = new SampleProfile.Status
        {
            Value = $"{label}-status"
        },
        StreetDetail = new SampleProfile.StreetDetail
        {
            Number = "100",
            Name = label,
            Type = "Road"
        },
        TownDetail = new SampleProfile.TownDetail
        {
            Name = $"{label}-town"
        }
    };
}

void VerifyOrganisationCompoundPrincipalDelete(string organisationId, Action<SampleProfile.Organisation> attachPrincipal,
    Func<SampleProfileDbContext, object> principalSelector, string assertionMessage)
{
    WithFreshDatabase(options =>
    {
        using (var context = new SampleProfileDbContext(options))
        {
            var organisation = new SampleProfile.Organisation { MRId = organisationId, Name = organisationId };
            attachPrincipal(organisation);
            context.Organisations.Add(organisation);
            context.SaveChanges();
        }

        using var verificationContext = new SampleProfileDbContext(options);
        verificationContext.Remove(principalSelector(verificationContext));
        verificationContext.SaveChanges();

        AssertCondition(verificationContext.Organisations.Count() == 0, assertionMessage);
        if (verificationContext.IdentifiedObjects.Count() != 0)
        {
            diagnostics.Add($"KNOWN ISSUE REPRODUCED: deleting the compound principal for {organisationId} removes Organisation but leaves the IdentifiedObject base row behind.");
        }
    });
}

void VerifyInheritanceDeleteScenario(
    string identifier,
    Action<SampleProfileDbContext> arrange,
    Func<SampleProfileDbContext, object> loadForDelete,
    IEnumerable<(string Table, int ExpectedCount)> expectedCounts)
{
    WithFreshDatabase(options =>
    {
        using (var context = new SampleProfileDbContext(options))
        {
            arrange(context);
            context.SaveChanges();
        }

        using (var context = new SampleProfileDbContext(options))
        {
            context.Remove(loadForDelete(context));
            context.SaveChanges();

            foreach (var (table, expectedCount) in expectedCounts)
            {
                AssertCondition(GetTableRowCount(context, table) == expectedCount,
                    $"Expected table {table} to have {expectedCount} rows after deleting inheritance graph {identifier}.");
            }
        }
    });
}

void VerifyStreetAddressCompoundPrincipalDelete(Action<SampleProfile.StreetAddress> attachPrincipal,
    Func<SampleProfileDbContext, object> principalSelector, string assertionMessage)
{
    WithFreshDatabase(options =>
    {
        using (var context = new SampleProfileDbContext(options))
        {
            var address = new SampleProfile.StreetAddress();
            attachPrincipal(address);
            context.Add(address);
            context.SaveChanges();
        }

        using var verificationContext = new SampleProfileDbContext(options);
        verificationContext.Remove(principalSelector(verificationContext));
        verificationContext.SaveChanges();
        AssertCondition(verificationContext.StreetAddresses.Count() == 0, assertionMessage);
    });
}

Exception? TryDelete(DbContextOptions<SampleProfileDbContext> options, Func<SampleProfileDbContext, object> targetSelector)
{
    using var context = new SampleProfileDbContext(options);
    context.Remove(targetSelector(context));

    try
    {
        context.SaveChanges();
        return null;
    }
    catch (Exception ex)
    {
        return ex;
    }
}

Exception? TrySave(DbContextOptions<SampleProfileDbContext> options, Action<SampleProfileDbContext> arrange)
{
    using var context = new SampleProfileDbContext(options);

    try
    {
        arrange(context);
        context.SaveChanges();
        return null;
    }
    catch (Exception ex)
    {
        return ex;
    }
}

void AssertPropertyKey(Type type, string propertyName, string columnName, int maxLength)
{
    var property = GetDeclaredProperty(type, propertyName);
    AssertCondition(property.GetCustomAttribute<KeyAttribute>() is not null,
        $"Expected {type.Name}.{propertyName} to have [Key].");
    AssertCondition(property.GetCustomAttribute<ColumnAttribute>()?.Name == columnName,
        $"Expected {type.Name}.{propertyName} to map to column {columnName}.");
    AssertCondition(property.GetCustomAttribute<MaxLengthAttribute>()?.Length == maxLength,
        $"Expected {type.Name}.{propertyName} to have MaxLength({maxLength}).");
}

PropertyInfo GetDeclaredProperty(Type type, string propertyName)
{
    return type.GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
        ?? throw new InvalidOperationException($"Expected property {type.Name}.{propertyName}.");
}

void AssertNoDeclaredKeyProperties(Type type)
{
    var declaredKeys = type
        .GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
        .Where(property => property.GetCustomAttribute<KeyAttribute>() is not null)
        .Select(property => property.Name)
        .ToList();

    AssertCondition(declaredKeys.Count == 0,
        $"{type.Name} should not declare its own [Key] property, but found: {string.Join(", ", declaredKeys)}.");
}

bool HasSinglePropertyUniqueIndex(IEnumerable<IndexAttribute> indexes, string propertyName)
{
    return indexes.Any(index => index.IsUnique && index.PropertyNames.Count == 1 && index.PropertyNames[0] == propertyName);
}

void AssertPrimaryKey(SampleProfileDbContext context, Type type, string propertyName)
{
    var key = GetEntityType(context, type).FindPrimaryKey()
        ?? throw new InvalidOperationException($"Expected a primary key for {type.Name}.");
    AssertCondition(key.Properties.Count == 1 && key.Properties[0].Name == propertyName,
        $"Expected primary key for {type.Name} to be {propertyName}.");
}

void AssertColumnName(SampleProfileDbContext context, Type type, string propertyName, string expectedColumnName)
{
    var entityType = GetEntityType(context, type);
    var property = entityType.FindProperty(propertyName)
        ?? throw new InvalidOperationException($"Expected property {type.Name}.{propertyName} in EF metadata.");
    var store = StoreObjectIdentifier.Table(entityType.GetTableName()!, entityType.GetSchema());
    var actual = property.GetColumnName(store);
    AssertCondition(actual == expectedColumnName,
        $"Expected EF column for {type.Name}.{propertyName} to be {expectedColumnName}, but was {actual}.");
}

void AssertDeleteBehavior(SampleProfileDbContext context, Type type, string propertyName, DeleteBehavior expectedBehavior)
{
    var foreignKey = GetEntityType(context, type).GetForeignKeys()
        .Single(key => key.Properties.Count == 1 && key.Properties[0].Name == propertyName);
    AssertCondition(foreignKey.DeleteBehavior == expectedBehavior,
        $"Expected DeleteBehavior.{expectedBehavior} for {type.Name}.{propertyName}, but found DeleteBehavior.{foreignKey.DeleteBehavior}.");
}

void AssertUniqueIndex(SampleProfileDbContext context, Type type, string propertyName)
{
    var index = GetEntityType(context, type).GetIndexes()
        .Single(candidate => candidate.Properties.Count == 1 && candidate.Properties[0].Name == propertyName);
    AssertCondition(index.IsUnique, $"Expected EF metadata index on {type.Name}.{propertyName} to be unique.");
}

IEntityType GetEntityType(SampleProfileDbContext context, Type type)
{
    return context.Model.FindEntityType(type)
        ?? throw new InvalidOperationException($"Expected EF metadata for {type.Name}.");
}

List<SqliteForeignKeyRow> GetSqliteForeignKeys(SampleProfileDbContext context, string tableName)
{
    var rows = new List<SqliteForeignKeyRow>();
    using var command = context.Database.GetDbConnection().CreateCommand();
    command.CommandText = $"PRAGMA foreign_key_list('{tableName}')";
    using var reader = command.ExecuteReader();

    while (reader.Read())
    {
        rows.Add(new SqliteForeignKeyRow(
            reader.GetString(reader.GetOrdinal("table")),
            reader.GetString(reader.GetOrdinal("from")),
            reader.GetString(reader.GetOrdinal("to")),
            reader.GetString(reader.GetOrdinal("on_delete"))));
    }

    return rows;
}

int GetTableRowCount(SampleProfileDbContext context, string tableName)
{
    using var command = context.Database.GetDbConnection().CreateCommand();
    command.CommandText = $"SELECT COUNT(*) FROM \"{tableName}\"";
    return Convert.ToInt32(command.ExecuteScalar());
}

string LoadGeneratedSqlText()
{
    var projectDirectory = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", ".."));
    var sqlPath = Path.GetFullPath(Path.Combine(projectDirectory, "..", "CSharpEFTestProject", "Profiles", "SampleProfile.rdfs-ansi92.sql"));
    AssertCondition(File.Exists(sqlPath), $"Expected generated SQL file to exist at {sqlPath}.");
    return File.ReadAllText(sqlPath);
}

string GetGeneratedCSharpPath()
{
    var projectDirectory = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", ".."));
    return Path.GetFullPath(Path.Combine(projectDirectory, "..", "CSharpEFTestProject", "Profiles", "SampleProfile.csharp-ef-rdfs.cs"));
}

string LoadGeneratedCSharpText()
{
    var csharpPath = GetGeneratedCSharpPath();
    AssertCondition(File.Exists(csharpPath), $"Expected generated C# file to exist at {csharpPath}.");
    return File.ReadAllText(csharpPath);
}

string GetSqlCreateTableBlock(string sql, string tableName)
{
    var match = Regex.Match(
        sql,
        $@"CREATE TABLE ""{Regex.Escape(tableName)}""\s*\((.*?)\);",
        RegexOptions.Singleline | RegexOptions.CultureInvariant);

    AssertCondition(match.Success, $"Expected CREATE TABLE block for {tableName} in generated SQL.");
    return match.Groups[1].Value;
}

Dictionary<string, string> GetSqlTableColumns(string sql, string tableName)
{
    var block = GetSqlCreateTableBlock(sql, tableName);
    var lines = block.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
    var columns = new Dictionary<string, string>(StringComparer.Ordinal);

    foreach (var rawLine in lines)
    {
        var line = rawLine.Trim().TrimEnd(',');
        if (!line.StartsWith('"'))
        {
            continue;
        }

        var match = Regex.Match(line, "^\"([^\"]+)\"\\s+(.+)$", RegexOptions.CultureInvariant);
        if (!match.Success)
        {
            continue;
        }

        columns[match.Groups[1].Value] = match.Groups[2].Value;
    }

    return columns;
}

void AssertSqlTableBlockContains(string sql, string tableName, string expectedFragment)
{
    var block = GetSqlCreateTableBlock(sql, tableName);
    AssertCondition(block.Contains(expectedFragment, StringComparison.Ordinal),
        $"Expected SQL CREATE TABLE block for {tableName} to contain: {expectedFragment}");
}

bool HasSqlForeignKey(string sql, string fromTable, string fromColumn, string toTable, string toColumn, bool onDeleteCascade = false)
{
    var pattern =
        $@"ALTER TABLE ""{Regex.Escape(fromTable)}"" ADD(?: CONSTRAINT [^\r\n]+)? FOREIGN KEY \( ""{Regex.Escape(fromColumn)}"" \) REFERENCES ""{Regex.Escape(toTable)}"" \( ""{Regex.Escape(toColumn)}"" \)"
        + (onDeleteCascade ? @" ON DELETE CASCADE" : "")
        + @"\s*;";

    return Regex.IsMatch(sql, pattern, RegexOptions.CultureInvariant);
}

void AssertSqlForeignKey(string sql, string fromTable, string fromColumn, string toTable, string toColumn, bool onDeleteCascade = false)
{
    AssertCondition(HasSqlForeignKey(sql, fromTable, fromColumn, toTable, toColumn, onDeleteCascade),
        $"Expected generated SQL foreign key {fromTable}.{fromColumn} -> {toTable}.{toColumn}"
        + (onDeleteCascade ? " with ON DELETE CASCADE." : "."));
}

void AssertForeignKeyTarget(SampleProfileDbContext context, Type type, string propertyName, Type principalType, string principalPropertyName)
{
    var foreignKey = GetEntityType(context, type).GetForeignKeys()
        .Single(key => key.Properties.Count == 1 && key.Properties[0].Name == propertyName);
    AssertCondition(foreignKey.PrincipalEntityType.ClrType == principalType,
        $"Expected principal type for {type.Name}.{propertyName} to be {principalType.Name}.");
    AssertCondition(foreignKey.PrincipalKey.Properties.Count == 1 && foreignKey.PrincipalKey.Properties[0].Name == principalPropertyName,
        $"Expected principal key for {type.Name}.{propertyName} to be {principalType.Name}.{principalPropertyName}.");
}

record SqliteForeignKeyRow(string Table, string From, string To, string OnDelete);
