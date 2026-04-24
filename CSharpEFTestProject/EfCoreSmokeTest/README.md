# EF Core Regression Test

This console app exercises the generated
`SampleProfile.csharp-ef-rdfs.cs` model with a real relational EF Core
provider.

What it does:

- reuses the generated `SampleProfile.csharp-ef-rdfs.cs` file directly
- builds an in-memory SQLite database
- exercises the generated `SampleProfile.DbContextBase` through a thin hand-written subclass
  that configures SQLite in `OnConfiguring`
- verifies reflection-level contracts such as `[Key]`, `[Column]`, `[MaxLength]`, and index placement
- verifies EF Core model metadata such as table names, primary keys, relational column names, indexes, and delete behaviors
- verifies `Name -> IdentifiedObject -> Organisation` behavior against the
  current sample profile
- verifies that deleting an `Organisation` referenced by `Name` or
  `ParentOrganisation` is blocked by `ClientNoAction`
- verifies generated compound cleanup when `Phone1`, `ElectronicAddress`, and
  `StreetAddress` references are replaced or cleared back to `null`
- distinguishes generated `DbContextBase` cleanup behavior from a plain
  `ModelConfiguration`-only baseline that leaves orphaned compounds behind
- verifies inheritance storage and delete cleanup across
  `IdentifiedObject -> Organisation -> ParentOrganization` and
  `IdentifiedObject -> AssetInfo -> WireInfo -> OverheadWireInfo`
- prints known-issue diagnostics for surprising current behaviors

## Current Limitation

The generated `DbContextBase` currently does **not** expose a
`DbContextOptions` constructor, so the smoke test configures SQLite through a
small `OnConfiguring` override in the hand-written subclass.

The runner also confirms that `ParentOrganisation` behaves differently:
because that relationship is generated with `DeleteBehavior.ClientNoAction`,
deleting a referenced `ParentOrganization` should fail while a child
`Organisation` still points at it.

The generated cleanup does remove replaced or detached top-level compound rows
such as `TelephoneNumber`, `ElectronicAddress`, and `StreetAddress`, but the
current run still reproduces a nested cleanup gap: replacing or null-detaching
`StreetAddress` leaves old `Status`, `StreetDetail`, and `TownDetail` rows
behind.

The baseline `GeneratedOnlySampleProfileDbContext` is kept on purpose so the
test can continue proving the difference between generated `DbContextBase`
cleanup and plain `ModelConfiguration` behavior.

## Files

- `EfCoreSmokeTest.csproj`
- `Program.cs`
- `SampleProfileDbContext.cs`
- `GeneratedOnlySampleProfileDbContext.cs`

The generated model file is referenced from:

- `..\CSharpEFTestProject\Profiles\SampleProfile.csharp-ef-rdfs.cs`

## Run

Once the .NET SDK is installed and `dotnet` is available on `PATH`:

```powershell
cd D:\Claude\CIM\CSharpEFTestProject\EfCoreSmokeTest
dotnet restore
dotnet run
```

Expected output is a short success message listing the completed sections plus
any known-issue diagnostics.
