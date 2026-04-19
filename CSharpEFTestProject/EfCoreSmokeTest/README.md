# EF Core Regression Test

This console app exercises the generated
`SampleProfile.csharp-ef-rdfs.cs` model with a real relational EF Core
provider.

What it does:

- reuses the generated `SampleProfile.csharp-ef-rdfs.cs` file directly
- builds an in-memory SQLite database
- applies the generated `SampleProfile.ModelConfiguration`
- verifies reflection-level contracts such as `[Key]`, `[Column]`, `[MaxLength]`, and index placement
- verifies EF Core model metadata such as table names, primary keys, relational column names, indexes, and delete behaviors
- inserts and reloads a deep `Organisation` graph with compound children
- verifies all generated relationship directions present in the sample profile
- verifies independent references such as `ParentOrganisation` and `ShuntCompensatorControl` block principal deletion
- verifies inheritance storage across base and derived tables
- verifies helper-assisted cleanup when optional compound navigations are cleared back to `null`
- distinguishes native generated mapping behavior from helper-assisted behavior for null-detach orphan cleanup
- checks which compound-sharing cases are blocked by per-slot unique indexes and which still slip through across different slots
- verifies that helper cleanup does not over-delete compounds during same-save ownership handoff between organisations
- stress-tests one `SaveChanges` that mixes detach, replacement, and reassignment across multiple compounds and owners
- verifies rollback safety when a failing `SaveChanges` should leave both existing compounds and nested rows untouched
- verifies same-context recovery after a failed save, so a repaired retry still cleans up only the intended orphaned compounds
- prints known-issue diagnostics for surprising current behaviors

## Current Limitation

With the current generated mapping, deleting an `Organisation` does **not**
cascade-delete the referenced compound rows (`ElectronicAddress`,
`TelephoneNumber`, `StreetAddress`). The regression runner reports this
explicitly as a known-issue diagnostic so the behavior is visible instead of
being silently mistaken for a working ownership delete.

The runner also confirms that `ParentOrganisation` behaves differently:
because that relationship is generated with `DeleteBehavior.ClientNoAction`,
deleting a referenced `ParentOrganization` should fail while a child
`Organisation` still points at it.

It also shows the important directionality detail for compound references:
the generated foreign key is on the `Organisation` or `StreetAddress`
dependent row, so deleting the compound principal can cascade-delete the
dependent row, while deleting the dependent row does not delete the principal.

The current regression run also reproduces another issue on the
`Organisation` inheritance chain: when a compound principal delete removes an
`Organisation`, the inherited `IdentifiedObject` base row is still left
behind.

## Files

- `EfCoreSmokeTest.csproj`
- `Program.cs`
- `SampleProfileDbContext.cs`

The generated model file is referenced from:

- `..\CSharpEFTestProject\Profiles\SampleProfile.csharp-ef-rdfs.cs`

## Run

Once the .NET SDK is installed and `dotnet` is available on `PATH`:

```powershell
cd D:\Claude\CIM\CSharpEFTestProject\EfCoreSmokeTest
dotnet restore
dotnet run
```

Expected output is a short success message plus the round-tripped
section summary and any known-issue diagnostics.
