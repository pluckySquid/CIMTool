# C# EF Testing Notes

## Status

I was able to set up the current C# EF builder workflow and generate C# output from the profile. I also created and ran a regression-style EF Core test project against the generated classes.

The comprehensive regression test currently passes overall, which means the generated code is at least usable enough to build, map, and exercise through EF Core with SQLite.

On April 23, 2026, after pulling Todd's updated `SampleProfile.owl` and replacing the broken empty RC8 `csharp-ef-rdfs.xsl` override with Todd's builder, I regenerated the sample and rebuilt the smoke test around the current smaller model. The current harness now passes 12 sections against the regenerated external project sample. The active sections are:

- Reflection contract
- EF metadata contract
- SQL schema parity
- Generated CSharp text integrity
- Lookup equality semantics
- Name association behavior
- Parent organisation delete guard
- Generated compound replacement cleanup
- Generated null detach cleanup
- Generated mapping baseline
- Inheritance storage
- Inheritance delete cleanup

The most important currently reproduced diagnostics from that April 23 run are:

- `DbContextBase` is generated without a `DbContextOptions` constructor, so the test subclass currently has to configure SQLite through `OnConfiguring`
- the generated `DbContextBase` cleanup removes top-level replaced/detached compounds such as `TelephoneNumber`, `ElectronicAddress`, and `StreetAddress`
- replacing or null-detaching `StreetAddress` still leaves nested `Status`, `StreetDetail`, and `TownDetail` rows behind
- the generated-only baseline still leaves compound graphs orphaned without the generated cleanup path
- `Name -> IdentifiedObject -> Organisation` behaves consistently with `ClientNoAction`: deleting `Name` is fine, deleting an `Organisation` still referenced by `Name` is blocked

On April 22, 2026, after adding the `Name` class to the sample profile and regenerating the C# output, I updated the EF Core regression harness to match the new generated shape. The important model change is that human-readable object names on `IdentifiedObject`-derived classes and on compound detail classes are now emitted as `NameValue`, because the profile also contains a concrete `Name` entity. After updating the harness, the regression suite passed all 31 sections. The new `Name Association Behavior` section confirms that `Name` rows round-trip correctly through `Name.IdentifiedObjectId -> IdentifiedObject.MRId`, and that deleting an `Organisation` referenced by a `Name` is currently blocked by the generated `ClientNoAction` foreign key.

On April 21, 2026, after regenerating `SampleProfile.csharp-ef-rdfs.cs` from the updated builder, I switched the smoke-test `SampleProfileDbContext` from the older hand-written cleanup prototype to the newly generated `SampleProfile.DbContextBase`. The regression suite still passed all 30 sections after that switch, which is the strongest confirmation so far that the XSL is now generating the intended cleanup path directly.

On April 17, 2026, I also applied a formatting-focused cleanup to the `csharp-ef-rdfs.xsl` template in the CIMTool source tree and synced the same XSL into the RC8 runtime builder folder. That cleanup was intentionally conservative: it reduced extra spacer emissions inside generated class bodies and removed one extra blank section break after the `allClasses` block, without changing the entity-model semantics.

Any blank-line counts or formatting diagnostics still refer to the last generated `.cs` artifact until the profile is regenerated through CIMTool using the updated builder.

After regenerating `SampleProfile.csharp-ef-rdfs.cs` on April 18, 2026 with the updated builder, the regression harness still passed all 18 sections. Formatting also improved measurably:

- whitespace-only blank lines dropped from 167 to 119
- consecutive blank-line runs dropped to 0
- blank lines immediately before closing braces dropped to 0

The remaining formatting issue is now mostly isolated to single whitespace-only spacer lines between top-level class declarations and between generated `Configure...` methods.

Later on April 18, 2026, I applied a second formatting pass to the XSL itself. This second pass removed additional leading spacer emissions from:

- nested class templates
- scalar/enumerated/navigation property templates
- the generated `allClasses` block
- the `ModelConfiguration` template and per-entity `Configure...` method template

That second pass has been synced to the RC8 runtime builder folder, but it has not yet been measured against a freshly regenerated `.cs` file. The current numeric diagnostics still reflect the previous regeneration checkpoint until the profile is generated again.

After regenerating again later on April 18, 2026 with that second-pass builder:

- whitespace-only blank lines dropped to 0
- consecutive blank-line runs remained at 0
- blank lines immediately before closing braces remained at 0
- the formatting diagnostics section no longer reported any formatting issues

At this point, the generated C# formatting for the tested sample profile is in a much healthier state. The remaining important problems are semantic EF/DDL parity issues rather than layout issues.

After re-checking the regenerated file later on April 18, 2026, I found one more quality issue that is separate from layout: some generated XML documentation text now contains mojibake-style punctuation such as `Ã¢â‚¬â€` instead of normal dashes. This appears to come from the `csharp-ef-rdfs.xsl` source text itself rather than from the sample profile, because the same corrupted sequences are already present inside the XSL comments and emitted documentation literals.

## Test Coverage Completed

The current regression test covers these areas:

- Reflection contract
- EF metadata contract
- SQL schema parity
- Generated CSharp formatting
- Generated CSharp text integrity
- Lookup equality semantics
- Database uniqueness enforcement
- Generated identity behavior
- Required and optional behavior
- FK/navigation synchronization
- Compound reference replacement
- Dependent delete direction
- Principal delete cascade
- Async compound cleanup
- Direct FK compound replacement
- Compound slot ownership
- Cross owner compound ownership
- Compound ownership handoff
- Compound mutation stress
- Compound failure rollback safety
- Compound failure recovery
- Generated mapping baseline
- Compound null detach cleanup
- Generated null detach baseline
- Independent relationships
- Name association behavior
- Inheritance storage
- Inheritance delete cleanup
- Inheritance query materialization
- Repeated compound replacement cycles
- Repeated address replacement cycles

The test confirms that the generated model can be used with EF Core and that the major entity mapping patterns are being exercised.

## How Pass/Fail Is Determined

The EF Core regression harness is a console program that runs each verification area as a named section.

A run is treated as passed when:

- `dotnet run` exits successfully with exit code `0`
- the console output includes `Comprehensive EF Core regression test passed (...) sections.`
- every section is listed as completed
- no unhandled exception stops execution

A run is treated as failed when:

- `dotnet run` exits with a non-zero exit code
- the harness throws an exception such as `Section failed: ...`
- a required assertion is not satisfied inside any verification section

The `Diagnostics:` lines at the end are not test failures by themselves. They are currently used to record known or observed behaviours that still look important and may need confirmation or design decisions.

## Current Findings

### 1. Dependent delete behavior

Deleting `Organisation` does not currently cascade-delete referenced compound rows.

Observed remaining rows include:

- `ElectronicAddress`
- `TelephoneNumber`
- `StreetAddress`

This may be acceptable if the model intentionally treats those compound rows as independent principals, but it is something that should be confirmed.

### 2. Principal delete behavior in inheritance cases

Deleting a compound principal can remove the dependent `Organisation`, but the inherited `IdentifiedObject` base row may remain behind.

This suggests there may be an inheritance cleanup issue or a table-per-type delete behavior gap in the generated mapping.

Direct EF deletes of inherited entities themselves currently behave better in testing: deleting `ParentOrganization` and `OverheadWireInfo` through both derived and base-set queries cleaned up their inheritance rows correctly.

### 3. Name association delete behavior

After adding `Name` into the sample profile, the generated C# model now emits `Name` as a separate entity plus a `Name.IdentifiedObjectId` foreign key back to `IdentifiedObject.MRId`.

Observed behavior:

- deleting a `Name` row does not delete the referenced `Organisation`
- deleting an `Organisation` that is still referenced by a `Name` currently fails, because the generated relationship is configured with `DeleteBehavior.ClientNoAction`

This behavior looks internally consistent with the generated EF metadata, but it should be confirmed whether this is the intended design for profile-generated `Name` associations.

This suggests the leftover `IdentifiedObject` rows are more likely tied to the compound-principal cascade path than to normal EF delete handling of inheritance in general.

### 3. Generated C# formatting quality

The generated `.cs` file formatting is now in much better shape after the two XSL cleanup passes.

The latest regenerated file checks found:

- 0 whitespace-only blank lines
- 0 consecutive blank-line runs
- 0 blank lines immediately before closing braces

This specific formatting problem looks effectively fixed for the current sample profile.

### 4. Generated text encoding quality

Although the spacing/layout is now much cleaner, the generated XML documentation comments still contain corrupted punctuation sequences such as:

- `Ã¢â‚¬â€`
- `Ã¢â€ â€™`

This is not just a console-display issue in the generated file. The same mojibake sequences are already present inside `csharp-ef-rdfs.xsl`, so the builder is currently emitting corrupted text exactly as written in its own source.

Current status:

- detected in the regenerated `SampleProfile.csharp-ef-rdfs.cs`
- traced back to the XSL source file itself
- now covered by the regression harness under `Generated CSharp text integrity`

Planned fix direction:

- replace corrupted Unicode punctuation in the XSL with ASCII-safe punctuation where practical
- regenerate the sample profile
- rerun the regression harness and confirm the text-integrity diagnostic disappears

Progress made on April 18, 2026:

- patched the visible emitted documentation strings in the source `csharp-ef-rdfs.xsl`
- patched the same emitted documentation strings in the RC8 runtime builder copy
- no entity-model semantics were changed as part of this text cleanup

Still pending:

- regenerate `SampleProfile.csharp-ef-rdfs.cs` through CIMTool/RC8
- rerun the regression harness to confirm the mojibake count drops from the current 15 affected lines

Verified after regeneration later on April 18, 2026:

- the regenerated `SampleProfile.csharp-ef-rdfs.cs` no longer contains the previously observed mojibake markers in the emitted documentation text
- the regression harness still passes, now with 19 sections and no `Generated CSharp text integrity` diagnostic
- this means the visible emitted comment-text corruption for the tested sample is fixed

What still remains open after that regeneration:

- SQL vs EF compound cascade-direction parity
- compound orphan accumulation after replacement/delete paths
- lookup-table `UNIQUE` vs `PRIMARY KEY` parity questions

Prototype work completed later on April 18, 2026:

- added a profile-aware compound cleanup prototype in the EF Core smoke test project
- wired the test `SampleProfileDbContext` through that prototype using `SaveChanges` / `SaveChangesAsync` overrides
- reran the full regression harness successfully after the prototype change

What the prototype improved in the regression run:

- deleting `Organisation` now cleans up its orphaned compound graph rows in the smoke test context
- replacing `Organisation.Phone1` now cleans up the previous `TelephoneNumber` row in the smoke test context
- clearing optional compound navigations back to `null` now cleans up the previously referenced compound rows in the smoke test context
- repeated `Phone1`, `PostalAddress`, and `StreetAddress` replacement cycles no longer accumulate orphan compound rows in the smoke test context
- async `SaveChangesAsync` now exercises the same cleanup path in the smoke test context
- direct FK reassignment of a compound reference now exercises the same cleanup path in the smoke test context

Additional regression clarification added later on April 18, 2026:

- introduced a second, plain `GeneratedOnlySampleProfileDbContext` with no cleanup override
- added a dedicated `Generated mapping baseline` section so the regression suite now distinguishes:
  - native generated EF behavior
  - cleanup-enabled smoke test behavior

What the baseline section now proves explicitly:

- without generated cleanup, replacing compound references leaves orphaned compound rows behind
- without generated cleanup, deleting the owner `Organisation` leaves compound graphs behind
- without generated cleanup, deleting a compound principal still cascades into the owner and leaves the inherited `IdentifiedObject` base row behind
- without generated cleanup, clearing optional compound navigations back to `null` leaves orphan compound rows behind

Updated scope note after regeneration on April 21, 2026:

- the main smoke-test path now uses generated profile code from `csharp-ef-rdfs.xsl` via `SampleProfile.DbContextBase`
- the old hand-written helper has been superseded by generated `DbContextBase` cleanup
- `GeneratedOnlySampleProfileDbContext` is still intentionally kept as a plain `DbContext` baseline so the suite can compare raw `ModelConfiguration` behavior against the generated cleanup-enabled path
- the builder documentation has been updated to stop claiming that no `SaveChanges` override is required for owner-side compound cleanup
- the underlying EF metadata still points cascade in the principal-to-owner direction because the FK column remains on the owner row

What remains open after the generated cleanup switch:

- direct deletion of a compound principal still reproduces the old bad behavior in EF metadata terms
- the generated `DbContextBase` cleanup still needs continued scrutiny on more profile shapes beyond the current sample
- lookup-table `UNIQUE` vs `PRIMARY KEY` parity questions still need a design decision
- ownership is still not fully enforced per compound slot: the same compound row can currently be assigned to multiple owner columns on the same `Organisation`
- ownership is also not fully enforced across owners when the same compound row is reused through different slots (for example `Phone1` on one owner and `Phone2` on another)
- generated cleanup still needs continued scrutiny on handoff/move scenarios, because ownership can change without the compound becoming orphaned
- generated cleanup also needs stress verification when a single unit of work mixes detach, replacement, and reassignment across multiple compounds at once
- generated cleanup should also be verified under failing saves so candidate collection does not accidentally translate into partial deletions when the database rejects the mutation
- generated cleanup should also be verified for repaired retries in the same tracked context after a failed save

Testing scope update:

- Java/Eclipse-side regression experiments were removed from scope
- current testing scope is intentionally limited to generated C# and `.NET` / EF Core execution
- the authoritative runnable test asset in this workspace is the local EF Core regression harness
- no Java or Eclipse headless execution is required for the current testing plan

### 5. Compound replacement update behavior

Replacing `Organisation.Phone1` with a new `TelephoneNumber` correctly updates the foreign key and the new related row round-trips as expected.

However, the previous `TelephoneNumber` row remains behind after replacement.

This may be consistent with the current ownership/delete direction in the generated mapping, but it is worth confirming whether that is intentional.

After repeated replacement cycles, orphan buildup appears to be systematic rather than incidental. In the latest run, repeated `Organisation.Phone1` replacements accumulated multiple orphan `TelephoneNumber` rows.

The same pattern also appears for nested address graphs. Repeated replacement of `PostalAddress` and `StreetAddress` accumulated orphan `StreetAddress`, `Status`, `StreetDetail`, and `TownDetail` rows, even though the latest foreign keys and latest nested values still round-tripped correctly.

### 6. SQL and EF parity gap for compound delete direction

The generated SQL schema models compound ownership using reverse foreign keys with `ON DELETE CASCADE`, so deleting the owning parent row is intended to remove the compound row automatically.

The generated EF/SQLite model currently behaves differently: the principal-side foreign key on `Organisation` is configured with cascade delete, so deleting the compound principal removes `Organisation` instead, while deleting `Organisation` leaves the compound row behind.

This is an important parity gap between the generated SQL builder and the generated C# EF builder.

### 7. SQL and EF parity note for lookup keys

The generated C# EF model treats lookup/enumeration tables such as `CrewStatusKind`, `PhaseCode`, and related `name` columns as primary keys.

The generated SQL schema currently uses `UNIQUE` on those `name` columns rather than declaring them as `PRIMARY KEY`.

This may still be functionally acceptable for some use cases, but it is another important difference between the SQL and C# EF builders.

## Current Pass Status

The latest local run passed with 30 sections:

- Reflection contract
- EF metadata contract
- SQL schema parity
- Generated CSharp formatting
- Generated CSharp text integrity
- Lookup equality semantics
- Database uniqueness enforcement
- Generated identity behavior
- Required and optional behavior
- FK/navigation synchronization
- Compound reference replacement
- Dependent delete direction
- Principal delete cascade
- Async compound cleanup
- Direct FK compound replacement
- Compound slot ownership
- Cross owner compound ownership
- Compound ownership handoff
- Compound mutation stress
- Compound failure rollback safety
- Compound failure recovery
- Generated mapping baseline
- Compound null detach cleanup
- Generated null detach baseline
- Independent relationships
- Inheritance storage
- Inheritance delete cleanup
- Inheritance query materialization
- Repeated compound replacement cycles
- Repeated address replacement cycles

The current mainline runtime path for those passing sections is now:

- `SampleProfileDbContext : SampleProfile.DbContextBase` for the generated cleanup-enabled behavior
- `GeneratedOnlySampleProfileDbContext : DbContext` for the raw mapping baseline comparisons

The SQL parity section now checks both selected contract points and broader table-column parity, including:

- mapped table presence
- primary key shape
- string length/type shape for mapped scalar columns
- boolean and timestamp SQL type shape
- nullable vs non-nullable intent
- single-column unique index parity
- selected foreign-key target parity

The latest run also added three new generated-only baseline diagnostics:

- generated-only null detaching `Organisation.ElectronicAddress` leaves orphan `ElectronicAddress` rows behind
- generated-only null detaching `Organisation.Phone2` leaves orphan `TelephoneNumber` rows behind
- generated-only null detaching `Organisation` address graphs leaves orphan `StreetAddress` / `Status` / `StreetDetail` / `TownDetail` rows behind

The newest ownership-focused runtime checks also distinguish:

- same-slot cross-owner reuse is blocked as expected by the per-column unique indexes
- mixed-slot cross-owner reuse still slips through, which means uniqueness is enforced per FK column rather than per compound row across all owner slots
- same-save same-slot handoff between organisations keeps the shared compound row alive when ownership is transferred rather than deleted
- one mixed same-save stress update can still preserve transferred compounds while deleting only the truly orphaned compound rows and nested address rows
- failed same-save duplicate-slot mutations leave the previously committed compound rows and nested address rows untouched, which is the expected rollback-safe behavior for the generated cleanup path
- after a failed same-context duplicate-slot mutation, repairing the tracked entities and retrying still produces the intended cleanup result for the replaced compound rows

## Likely Cause of Formatting Issues

The current `csharp-ef-rdfs.xsl` appears to emit many empty `<item></item>` nodes inside CIMTool's custom Indent XML structure.

Those empty items are likely being rendered as whitespace-bearing blank lines in the generated C# output.

Examples of likely sources:

- blank spacer items before class bodies
- blank spacer items between methods and property blocks
- blank spacer items before navigation-property sections
- trailing blank spacer items before closing braces

## Suggested XSL Cleanup

The main formatting cleanup should be done in the XSL rather than manually in generated output.

Suggested changes:

1. Remove unnecessary empty `<item></item>` nodes where they are only being used as spacers.
2. Keep blank lines only between major logical sections:
   - class header and first member
   - constructor / `ToString()`
   - key block
   - property groups
3. Avoid emitting blank lines immediately before `}` in small classes.
4. Consider adding `xsl:strip-space elements="*"` if whitespace from the XSL source is contributing to formatting noise.
5. Make section spacing consistent across:
   - root classes
   - inherited classes
   - enum-like lookup classes
   - navigation-property blocks

Partially implemented on April 17, 2026:

- removed several extra spacer emissions between constructor, equality members, hash-code members, and key blocks in the core class templates
- removed one redundant blank break after the generated `allClasses` array
- synced the updated `csharp-ef-rdfs.xsl` into the RC8 runtime builder location so regeneration can use the new template immediately

Still to verify after regeneration:

- whether property-group spacing is now visually consistent in the emitted `.cs`
- how much the whitespace-only blank-line count drops in the next generated file

Observed after regeneration on April 18, 2026:

- property-group spacing inside many classes is better than before
- the largest remaining visual issue is top-level spacer lines such as the line between `}` of one class and the XML doc block for the next class
- the `allClasses`/configuration area no longer shows the earlier double-blank-run symptom
- after the second-pass regeneration checkpoint, those residual spacer-line symptoms no longer reproduced in the measured sample output

Expected after the next regeneration:

- fewer single whitespace-only spacer lines between nested classes
- fewer single whitespace-only spacer lines between `Configure...` methods
- less vertical noise between FK shadow properties and navigation properties

Follow-up found after the latest regeneration:

- layout is now clean for the tested sample
- the remaining generated-output quality issue is comment/doc-text encoding rather than blank-line formatting

## Questions to Confirm

These are the main points that should be confirmed:

- Is the current delete behavior expected for compounds and inheritance?
- Is the generated formatting roughly what is intended right now?
- Am I definitely using the correct `csharp-ef-rdfs.xsl` and builder configuration?
- Should the builder aim for cleaner generated C# formatting now, or is correctness the higher priority for this phase?

## Next Steps

Planned next work:

- verify whether the current generated output is using the intended builder file
- replace mojibake/corrupted punctuation in emitted XSL documentation text
- expand `.NET`-side coverage with more generated-profile and EF-behavior assertions only

## Still Worth Testing

The highest-value remaining tests are:

- testing with a second sample profile so the harness is not tied too closely to one schema shape
- deciding whether the lookup-table `UNIQUE` vs `PRIMARY KEY` difference is intentional
- deciding whether the SQL-vs-EF compound cascade direction difference is intentional or should be aligned
- deciding whether orphan accumulation after compound replacement is acceptable or should be cleaned up automatically
- adding more `.NET` assertions around generated metadata and database behavior without introducing Java/Eclipse test infrastructure
