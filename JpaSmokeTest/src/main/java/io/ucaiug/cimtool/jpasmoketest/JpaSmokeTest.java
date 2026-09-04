package io.ucaiug.cimtool.jpasmoketest;

import io.ucaiug.cimtool.generated.SampleProfile;
import jakarta.persistence.CascadeType;
import jakarta.persistence.Column;
import jakarta.persistence.Entity;
import jakarta.persistence.GeneratedValue;
import jakarta.persistence.GenerationType;
import jakarta.persistence.Id;
import jakarta.persistence.Index;
import jakarta.persistence.Inheritance;
import jakarta.persistence.InheritanceType;
import jakarta.persistence.JoinColumn;
import jakarta.persistence.ManyToOne;
import jakarta.persistence.PrimaryKeyJoinColumn;
import jakarta.persistence.Table;
import jakarta.persistence.UniqueConstraint;
import org.hibernate.Session;
import org.hibernate.SessionFactory;
import org.hibernate.cfg.Configuration;

import java.lang.reflect.Field;
import java.nio.file.Files;
import java.nio.file.Path;
import java.nio.file.Paths;
import java.sql.Connection;
import java.sql.DriverManager;
import java.sql.ResultSet;
import java.sql.SQLException;
import java.sql.Statement;
import java.util.ArrayList;
import java.util.Arrays;
import java.util.List;
import java.util.Locale;
import java.util.Set;
import java.util.TreeSet;
import java.util.function.Consumer;

/**
 * Comprehensive JPA smoke/regression test for the CIMTool jpa-rdfs builder,
 * mirroring EfCoreSmokeTest/Program.cs where the concepts translate to JPA. Validates the
 * generated JPA entity model (Hibernate + in-memory H2) against the companion
 * sql-rdfs-ansi92 DDL.
 */
public final class JpaSmokeTest {

    /** Overridable via -Dcimtool.profiles.dir (the pom passes it) so the run does not depend on the cwd. */
    static final Path PROFILES_DIR = Paths.get(System.getProperty("cimtool.profiles.dir",
            Paths.get(System.getProperty("user.dir"))
                .resolve("../CSharpEFTestProject/CSharpEFTestProject/Profiles").toString())).normalize();
    static final Path GENERATED_JAVA_PATH = PROFILES_DIR.resolve("SampleProfile.jpa-rdfs.java");
    static final Path SQL_PATH = PROFILES_DIR.resolve("SampleProfile.rdfs-ansi92.sql");

    /**
     * Hibernate bootstraps through java.util.logging here, and the FK-guard sections
     * deliberately provoke constraint violations that it logs at ERROR. Left alone that
     * buries the section report under a few hundred lines. References are held so the
     * loggers are not garbage collected along with their configured levels.
     */
    private static final java.util.logging.Logger HIBERNATE_LOG =
            java.util.logging.Logger.getLogger("org.hibernate");
    private static final java.util.logging.Logger SQL_EXCEPTION_LOG =
            java.util.logging.Logger.getLogger("org.hibernate.engine.jdbc.spi.SqlExceptionHelper");
    static {
        HIBERNATE_LOG.setLevel(java.util.logging.Level.SEVERE);
        SQL_EXCEPTION_LOG.setLevel(java.util.logging.Level.OFF);
    }

    static final List<String> completedSections = new ArrayList<>();
    static final List<String> diagnostics = new ArrayList<>();
    static int dbCounter = 0;

    public static void main(String[] args) {
        for (Path fixture : List.of(GENERATED_JAVA_PATH, SQL_PATH)) {
            if (!java.nio.file.Files.isRegularFile(fixture)) {
                throw new IllegalStateException("Missing generated fixture: " + fixture
                    + System.lineSeparator() + "Generate it with CIMTool (see JpaSmokeTest/README.md), or pass "
                    + "-Dcimtool.profiles.dir=<dir> if the fixtures live elsewhere.");
            }
        }
        runSection("Generated Java Text Integrity", JpaSmokeTest::verifyGeneratedJavaTextIntegrity);
        runSection("Reflection Contract", JpaSmokeTest::verifyReflectionContract);
        runSection("JPA Metadata Contract", JpaSmokeTest::verifyJpaMetadataContract);
        runSection("SQL Schema Parity", JpaSmokeTest::verifySqlSchemaParityText);
        runSection("SQL DDL Cross-Check", JpaSmokeTest::verifySqlDdlCrossCheck);
        runSection("Lookup Equality Semantics", JpaSmokeTest::verifyLookupEqualitySemantics);
        runSection("Name Association Behavior", JpaSmokeTest::verifyNameAssociationBehavior);
        runSection("Parent Organisation Delete Guard", JpaSmokeTest::verifyParentOrganisationDeleteGuard);
        runSection("Compound Lifecycle", JpaSmokeTest::verifyCompoundLifecycle);
        runSection("Null Detach Baseline", JpaSmokeTest::verifyNullDetachBaseline);
        runSection("Inheritance Storage", JpaSmokeTest::verifyInheritanceStorage);
        runSection("Inheritance Delete Cleanup", JpaSmokeTest::verifyInheritanceDeleteCleanup);
        runSection("Compoundless Entity Lifecycle", JpaSmokeTest::verifyCompoundlessEntityLifecycle);
        runSection("Authoritative DDL Entity Round-Trip", JpaSmokeTest::verifyAuthoritativeDdlEntityRoundTrip);
        runSection("Authoritative DDL Owner Delete Cascade", JpaSmokeTest::verifyAuthoritativeDdlOwnerDeleteCascade);
        runSection("Authoritative DDL Replacement Guard", JpaSmokeTest::verifyAuthoritativeDdlReplacementGuard);

        System.out.println("Comprehensive JPA regression test passed (" + completedSections.size() + " sections).");
        for (String section : completedSections) {
            System.out.println("- " + section);
        }
        if (!diagnostics.isEmpty()) {
            System.out.println("Diagnostics:");
            for (String diagnostic : diagnostics) {
                System.out.println("- " + diagnostic);
            }
        }
    }

    // ================================================================
    // Section 1: Generated Java Text Integrity
    // ================================================================

    static void verifyGeneratedJavaTextIntegrity() throws Exception {
        String generated = Files.readString(GENERATED_JAVA_PATH);
        assertCondition(!generated.startsWith("<?xml"),
            "Generated Java must be rendered text, not the Indent XML document.");
        assertCondition(generated.contains("package io.ucaiug.cimtool.generated;"),
            "Expected the generated package declaration.");
        assertCondition(generated.contains("import jakarta.persistence.*;"),
            "Expected the jakarta.persistence import.");
        assertCondition(generated.contains("public static class Name"),
            "Expected generated Java to include the Name entity.");
        assertCondition(generated.contains("allClasses"),
            "Expected generated Java to include the allClasses bootstrap array.");
        assertCondition(!generated.contains("ShuntCompensator"),
            "Did not expect removed ShuntCompensator sample types in generated Java.");
    }

    // ================================================================
    // Section 2: Reflection Contract
    // ================================================================

    static void verifyReflectionContract() {
        // IdentifiedObject hierarchy: natural mRID key at the root only, JOINED inheritance
        assertNaturalKey(SampleProfile.IdentifiedObject.class, "mRID", "mRID");
        assertCondition(SampleProfile.IdentifiedObject.class.isAnnotationPresent(Inheritance.class)
                && SampleProfile.IdentifiedObject.class.getAnnotation(Inheritance.class).strategy() == InheritanceType.JOINED,
            "Expected @Inheritance(JOINED) on IdentifiedObject.");
        for (Class<?> sub : List.of(SampleProfile.Organisation.class, SampleProfile.ParentOrganization.class,
                SampleProfile.AssetInfo.class, SampleProfile.WireInfo.class, SampleProfile.OverheadWireInfo.class)) {
            assertNoDeclaredId(sub);
            assertCondition(sub.isAnnotationPresent(PrimaryKeyJoinColumn.class),
                "Expected @PrimaryKeyJoinColumn on subclass " + sub.getSimpleName());
        }
        // Enum lookup classes: natural 'name' key
        for (Class<?> lookup : List.of(SampleProfile.CrewStatusKind.class, SampleProfile.PhaseCode.class,
                SampleProfile.WireInsulationKind.class, SampleProfile.WireMaterialKind.class)) {
            assertNaturalKey(lookup, "name", "name");
        }
        // Compound value objects: provider-assigned surrogate UUID
        for (Class<?> compound : List.of(SampleProfile.ElectronicAddress.class, SampleProfile.TelephoneNumber.class,
                SampleProfile.StreetAddress.class, SampleProfile.Status.class,
                SampleProfile.StreetDetail.class, SampleProfile.TownDetail.class)) {
            assertSurrogateUuidKey(compound);
        }
        // Name is an independent entity with a surrogate key
        assertSurrogateUuidKey(SampleProfile.Name.class);

        assertEntityTable(SampleProfile.IdentifiedObject.class, "IdentifiedObject");
        assertEntityTable(SampleProfile.Name.class, "Name");
        assertEntityTable(SampleProfile.Organisation.class, "Organisation");
        assertEntityTable(SampleProfile.ParentOrganization.class, "ParentOrganization");
        assertEntityTable(SampleProfile.OverheadWireInfo.class, "OverheadWireInfo");

        // allClasses: complete and supertype-before-subtype
        List<Class<?>> all = Arrays.asList(SampleProfile.allClasses);
        for (Class<?> expected : List.of(SampleProfile.IdentifiedObject.class, SampleProfile.Name.class,
                SampleProfile.Organisation.class, SampleProfile.TelephoneNumber.class)) {
            assertCondition(all.contains(expected),
                "Expected allClasses to contain " + expected.getSimpleName());
        }
        for (Class<?> entity : all) {
            Class<?> superClass = entity.getSuperclass();
            if (superClass != null && all.contains(superClass)) {
                assertCondition(all.indexOf(superClass) < all.indexOf(entity),
                    "allClasses must list " + superClass.getSimpleName() + " before subclass " + entity.getSimpleName());
            }
        }

        // Column-name contract for the non-key columns the C# harness checks.
        assertColumnName(SampleProfile.Name.class, "name", "name");
        assertColumnName(SampleProfile.IdentifiedObject.class, "name", "name");
        assertCondition(joinColumn(SampleProfile.Name.class, "identifiedObject").name().equals("IdentifiedObject"),
            "Expected Name.identifiedObject to map to the IdentifiedObject join column.");

        // Cascade contract — the nearest JPA analogue of the C# DeleteBehavior checks
        // (Restrict on compounds, ClientNoAction on independent associations). The
        // mechanisms differ (EF guards the principal delete, JPA propagates it), so this
        // only pins which associations the builder treats as compounds.
        for (String field : List.of("electronicAddress", "phone1", "phone2", "postalAddress", "streetAddress")) {
            assertCascadeRemove(SampleProfile.Organisation.class, field, true);
        }
        for (String field : List.of("status", "streetDetail", "townDetail")) {
            assertCascadeRemove(SampleProfile.StreetAddress.class, field, true);
        }
        assertCascadeRemove(SampleProfile.Organisation.class, "parentOrganisation", false);
        assertCascadeRemove(SampleProfile.Name.class, "identifiedObject", false);

        // Compound FK columns: the SQL DDL declares them UNIQUE and generated C# carries
        // [Index(IsUnique = true)] on each. Record whether the JPA mapping declares uniqueness
        // anywhere it can (@JoinColumn.unique, a unique @Index, or a @UniqueConstraint).
        List<String> nonUniqueCompounds = new ArrayList<>();
        for (String field : List.of("electronicAddress", "phone1", "phone2", "postalAddress", "streetAddress")) {
            if (!mappedUnique(SampleProfile.Organisation.class, field)) {
                nonUniqueCompounds.add("Organisation." + field);
            }
        }
        for (String field : List.of("status", "streetDetail", "townDetail")) {
            if (!mappedUnique(SampleProfile.StreetAddress.class, field)) {
                nonUniqueCompounds.add("StreetAddress." + field);
            }
        }
        if (!nonUniqueCompounds.isEmpty()) {
            diagnostics.add("FINDING: compound reference mappings declare no uniqueness (no unique=true on "
                + "@JoinColumn, no unique @Index, no @UniqueConstraint) although the SQL DDL declares these "
                + "columns UNIQUE and generated C# emits [Index(IsUnique = true)]: " + nonUniqueCompounds);
        }

        // String key column lengths: the SQL DDL uses VARCHAR(100) and generated C# carries
        // [MaxLength(100)]. UUID-typed surrogate keys are a column-type question, not a length
        // one, and are reported from the schema in the JPA Metadata Contract section.
        List<String> defaultLengthKeys = new ArrayList<>();
        for (Class<?> type : List.of(SampleProfile.IdentifiedObject.class, SampleProfile.CrewStatusKind.class,
                SampleProfile.PhaseCode.class, SampleProfile.WireInsulationKind.class,
                SampleProfile.WireMaterialKind.class)) {
            Field id = declaredIdField(type);
            assertCondition(id != null && id.getType() == String.class && id.getAnnotation(Column.class) != null,
                "Expected " + type.getSimpleName() + " to declare a String @Id with @Column.");
            int length = id.getAnnotation(Column.class).length();
            if (length != 100) {
                defaultLengthKeys.add(type.getSimpleName() + "." + id.getName() + "=" + length);
            }
        }
        if (!defaultLengthKeys.isEmpty()) {
            diagnostics.add("FINDING: String key columns use the JPA default length rather than the DDL's "
                + "VARCHAR(100) (generated C# carries [MaxLength(100)]): " + defaultLengthKeys);
        }
    }

    // ================================================================
    // Section 3: JPA Metadata Contract
    // ================================================================

    static void verifyJpaMetadataContract() {
        withFreshDatabase(factory -> {
            try (Connection c = connect(currentDbUrl())) {
                for (String table : List.of("IdentifiedObject", "Name", "Organisation", "ParentOrganization",
                        "AssetInfo", "WireInfo", "OverheadWireInfo", "CrewStatusKind", "PhaseCode",
                        "TelephoneNumber", "ElectronicAddress", "StreetAddress", "Status")) {
                    resolveTable(c, table);
                }
                assertPk(c, "IdentifiedObject", "mRID");
                assertPk(c, "Organisation", "mRID");   // JOINED subclass PK = joined mRID
                assertPk(c, "Name", "id");
                assertPk(c, "CrewStatusKind", "name");
                assertPk(c, "PhaseCode", "name");
                Set<String> fks = foreignKeyEdges(c);
                assertCondition(fks.contains("NAME.IDENTIFIEDOBJECT->IDENTIFIEDOBJECT.MRID"),
                    "Expected FK Name.IdentifiedObject -> IdentifiedObject.mRID, got: " + fks);
                assertCondition(fks.contains("ORGANISATION.MRID->IDENTIFIEDOBJECT.MRID"),
                    "Expected JOINED-inheritance FK Organisation.mRID -> IdentifiedObject.mRID.");

                // The C# harness asserts a unique index on every Organisation compound FK column;
                // record what the Hibernate-generated schema actually carries, plus key column sizes.
                List<String> nonUnique = new ArrayList<>();
                for (String column : List.of("electronicAddress", "phone1", "phone2", "postalAddress", "streetAddress")) {
                    if (!hasUniqueIndex(c, "Organisation", column)) {
                        nonUnique.add("Organisation." + column);
                    }
                }
                if (!nonUnique.isEmpty()) {
                    diagnostics.add("FINDING: the JPA-generated schema has no UNIQUE index on compound FK columns "
                        + nonUnique + " (the SQL DDL declares them UNIQUE).");
                }
                int mridSize = columnSize(c, "IdentifiedObject", "mRID");
                if (mridSize != 100) {
                    diagnostics.add("FINDING: JPA-generated IdentifiedObject.mRID is VARCHAR(" + mridSize
                        + "); the SQL DDL declares VARCHAR(100).");
                }
                List<String> nonVarcharKeys = new ArrayList<>();
                for (String table : List.of("Name", "ElectronicAddress", "TelephoneNumber", "StreetAddress", "Status")) {
                    String type = columnType(c, table, "id");
                    if (!type.toUpperCase(Locale.ROOT).contains("CHAR")) {
                        nonVarcharKeys.add(table + ".id=" + type);
                    }
                }
                if (!nonVarcharKeys.isEmpty()) {
                    diagnostics.add("FINDING: JPA-generated surrogate keys are native UUID columns while the SQL DDL "
                        + "declares VARCHAR(100) (generated C# uses string Guids): " + nonVarcharKeys);
                }
            } catch (SQLException e) {
                throw new RuntimeException(e);
            }
        });
    }

    // ================================================================
    // Section 4: SQL Schema Parity (text assertions, mirrors C# harness)
    // ================================================================

    static void verifySqlSchemaParityText() throws Exception {
        String sql = Files.readString(SQL_PATH);
        assertCondition(sql.contains("CREATE TABLE \"Name\""), "Expected SQL to declare the Name table.");
        assertCondition(sql.contains("\"IdentifiedObject\" VARCHAR(100)"),
            "Expected SQL to store the Name.IdentifiedObject foreign key.");
        assertCondition(sql.contains("ALTER TABLE \"Name\" ADD FOREIGN KEY ( \"IdentifiedObject\" ) REFERENCES \"IdentifiedObject\" ( \"mRID\" );"),
            "Expected SQL to point Name.IdentifiedObject to IdentifiedObject.mRID.");
        assertCondition(sql.contains("CREATE TABLE \"Organisation\""), "Expected SQL to declare the Organisation table.");
        assertCondition(sql.contains("\"electronicAddress\" VARCHAR(100) UNIQUE"),
            "Expected Organisation.electronicAddress UNIQUE in SQL.");
        assertCondition(sql.contains("\"phone1\" VARCHAR(100) UNIQUE"),
            "Expected Organisation.phone1 UNIQUE in SQL.");
        assertCondition(sql.contains("ALTER TABLE \"TelephoneNumber\" ADD CONSTRAINT fk_TelephoneNumber_Organisation_phone1 FOREIGN KEY ( \"id\" ) REFERENCES \"Organisation\" ( \"phone1\" ) ON DELETE CASCADE;"),
            "Expected SQL reverse cascade for Organisation.phone1 compound cleanup.");
        assertCondition(sql.contains("ALTER TABLE \"Status\" ADD CONSTRAINT fk_Status_StreetAddress_status FOREIGN KEY ( \"id\" ) REFERENCES \"StreetAddress\" ( \"status\" ) ON DELETE CASCADE;"),
            "Expected SQL reverse cascade for StreetAddress.status compound cleanup.");
        assertCondition(sql.contains("CREATE INDEX ix_Name_IdentifiedObject ON \"Name\" ( \"IdentifiedObject\" );"),
            "Expected SQL index on Name.IdentifiedObject.");
        diagnostics.add("PARITY GAP OBSERVED: SQL models compound ownership with reverse ON DELETE CASCADE constraints, "
            + "while generated JPA uses forward @ManyToOne(cascade=REMOVE) from the owner side.");
    }

    // ================================================================
    // Section 5: SQL DDL Cross-Check (executes the DDL into a second H2)
    // ================================================================

    static void verifySqlDdlCrossCheck() throws Exception {
        String ddlUrl = "jdbc:h2:mem:jpa_smoke_ddl;DB_CLOSE_DELAY=-1";
        List<String> ddlFailures = new ArrayList<>();
        try (Connection ddlConn = connect(ddlUrl); Statement st = ddlConn.createStatement()) {
            String sql = Files.readString(SQL_PATH);
            StringBuilder cleaned = new StringBuilder();
            for (String line : sql.split("\n")) {
                if (!line.strip().startsWith("--")) {
                    cleaned.append(line).append('\n');
                }
            }
            for (String stmt : cleaned.toString().split(";")) {
                if (stmt.isBlank()) continue;
                try {
                    st.execute(stmt);
                } catch (SQLException e) {
                    ddlFailures.add(firstLine(stmt) + " -> " + e.getMessage());
                }
            }
            if (!ddlFailures.isEmpty()) {
                diagnostics.add("DDL FINDING: " + ddlFailures.size() + " generated ANSI-92 statements rejected by H2:");
                for (String failure : ddlFailures) {
                    diagnostics.add("  " + failure);
                }
            }
            withFreshDatabase(factory -> {
                try (Connection jpaConn = connect(currentDbUrl())) {
                    Set<String> sqlTables = tableNames(ddlConn);
                    Set<String> jpaTables = tableNames(jpaConn);
                    assertCondition(!sqlTables.isEmpty(), "Executed DDL produced no tables in H2.");
                    diffSets("tables only in SQL DDL", sqlTables, jpaTables);
                    diffSets("tables only in JPA schema", jpaTables, sqlTables);
                    for (String table : sqlTables) {
                        if (!containsIgnoreCase(jpaTables, table)) continue;
                        List<String> sqlPk = primaryKeyColumns(ddlConn, table);
                        List<String> jpaPk = primaryKeyColumns(jpaConn, table);
                        assertCondition(equalsIgnoreCaseList(sqlPk, jpaPk),
                            "PK mismatch on " + table + ": SQL=" + sqlPk + " JPA=" + jpaPk);
                        diffSets("columns only in SQL " + table, columns(ddlConn, table), columns(jpaConn, table));
                        diffSets("columns only in JPA " + table, columns(jpaConn, table), columns(ddlConn, table));
                    }
                    Set<String> sqlFks = foreignKeyEdges(ddlConn);
                    Set<String> jpaFks = foreignKeyEdges(jpaConn);
                    diffSets("FK edges only in SQL DDL", sqlFks, jpaFks);
                    diffSets("FK edges only in JPA schema", jpaFks, sqlFks);
                    // Self-check of the metadata helpers against the executed DDL, which is known
                    // to declare Organisation.phone1 UNIQUE and IdentifiedObject.mRID VARCHAR(100).
                    assertCondition(hasUniqueIndex(ddlConn, "Organisation", "phone1"),
                        "Expected the executed DDL to carry a UNIQUE index on Organisation.phone1.");
                    assertCondition(columnSize(ddlConn, "IdentifiedObject", "mRID") == 100,
                        "Expected the executed DDL to declare IdentifiedObject.mRID as VARCHAR(100).");
                } catch (SQLException e) {
                    throw new RuntimeException(e);
                }
            });
        }
    }

    // ================================================================
    // Section 6: Lookup Equality Semantics
    // ================================================================

    static void verifyLookupEqualitySemantics() {
        SampleProfile.CrewStatusKind statusA = new SampleProfile.CrewStatusKind();
        statusA.setName("arrived");
        SampleProfile.CrewStatusKind statusB = new SampleProfile.CrewStatusKind();
        statusB.setName("arrived");
        assertCondition(statusA.equals(statusB), "Expected lookup types to compare by natural name key.");
        assertCondition(statusA.hashCode() == statusB.hashCode(), "Expected equal lookups to share a hashCode.");
        SampleProfile.CrewStatusKind statusC = new SampleProfile.CrewStatusKind();
        statusC.setName("dispatched");
        assertCondition(!statusA.equals(statusC), "Expected lookup types with different names to be unequal.");

        SampleProfile.Organisation org = new SampleProfile.Organisation();
        org.setMRID("same-id");
        SampleProfile.ParentOrganization parent = new SampleProfile.ParentOrganization();
        parent.setMRID("same-id");
        assertCondition(!org.equals(parent),
            "Expected IdentifiedObject equality to reject different runtime types.");

        // Intentional difference from generated C#: UUID ids are provider-assigned
        // on persist (@GeneratedValue), not in the constructor.
        SampleProfile.TelephoneNumber unsaved = new SampleProfile.TelephoneNumber();
        assertCondition(unsaved.getId() == null,
            "Expected compound id to be null before persist (provider-assigned UUID).");
        withFreshDatabase(factory -> {
            SampleProfile.TelephoneNumber phone = new SampleProfile.TelephoneNumber();
            phone.setItuPhone("+1-555-0100");
            SampleProfile.TelephoneNumber phone2 = new SampleProfile.TelephoneNumber();
            phone2.setItuPhone("+1-555-0101");
            SampleProfile.Organisation owner = new SampleProfile.Organisation();
            owner.setMRID("org-eq-001");
            owner.setPhone1(phone);
            owner.setPhone2(phone2);
            SampleProfile.Name name = new SampleProfile.Name();
            name.setName("Equality Name");
            name.setIdentifiedObject(owner);
            inTransaction(factory, s -> {
                s.persist(phone);
                s.persist(phone2);
                s.persist(owner);
                s.persist(name);
            });
            assertCondition(phone.getId() != null && phone2.getId() != null,
                "Expected compound ids to be assigned on persist.");
            assertCondition(!phone.getId().equals(phone2.getId()),
                "Expected each persisted TelephoneNumber to receive a distinct surrogate id.");
            try (Session s = factory.openSession()) {
                assertCondition(s.get(SampleProfile.TelephoneNumber.class, phone.getId()).equals(phone),
                    "Expected compound types to compare by surrogate id.");
                assertCondition(s.get(SampleProfile.Name.class, name.getId()).equals(name),
                    "Expected Name entities to compare by surrogate id.");
                assertCondition(!phone.equals(phone2),
                    "Expected compounds with different surrogate ids to be unequal.");
            }
            assertCondition(!new SampleProfile.TelephoneNumber().equals(new SampleProfile.TelephoneNumber()),
                "Expected transient compounds with null ids to be unequal.");

            SampleProfile.ElectronicAddress email1 = new SampleProfile.ElectronicAddress();
            SampleProfile.ElectronicAddress email2 = new SampleProfile.ElectronicAddress();
            SampleProfile.StreetAddress address1 = new SampleProfile.StreetAddress();
            SampleProfile.StreetAddress address2 = new SampleProfile.StreetAddress();
            inTransaction(factory, s -> {
                s.persist(email1);
                s.persist(email2);
                s.persist(address1);
                s.persist(address2);
            });
            assertCondition(email1.getId() != null && !email1.getId().equals(email2.getId()),
                "Expected each persisted ElectronicAddress to receive a distinct surrogate id.");
            assertCondition(address1.getId() != null && !address1.getId().equals(address2.getId()),
                "Expected each persisted StreetAddress to receive a distinct surrogate id.");
        });
        diagnostics.add("PARITY NOTE: JPA compound surrogate UUIDs are assigned on persist (@GeneratedValue), "
            + "while generated C# assigns Guid.NewGuid() in the constructor.");
    }

    // ================================================================
    // Section 7: Name Association Behavior
    // ================================================================

    static void verifyNameAssociationBehavior() {
        withFreshDatabase(factory -> {
            inTransaction(factory, s -> {
                SampleProfile.Organisation org = new SampleProfile.Organisation();
                org.setMRID("org-name-001");
                org.setName("Acme Utility");
                SampleProfile.Name name = new SampleProfile.Name();
                name.setName("Acme Utility Legal Name");
                name.setIdentifiedObject(org);
                s.persist(org);
                s.persist(name);
            });
            try (Session s = factory.openSession()) {
                var cb = s.getCriteriaBuilder();
                var query = cb.createQuery(SampleProfile.Name.class);
                query.from(SampleProfile.Name.class).fetch("identifiedObject");
                SampleProfile.Name loaded = s.createQuery(query).getSingleResult();
                assertCondition("org-name-001".equals(loaded.getIdentifiedObject().getMRID()),
                    "Expected Name.IdentifiedObject to round-trip.");
                assertCondition("Acme Utility Legal Name".equals(loaded.getName()),
                    "Expected Name.name to round-trip.");
            }
            inTransaction(factory, s -> loadAll(s, SampleProfile.Name.class).forEach(s::remove));
            assertCondition(countRows(factory, SampleProfile.Name.class) == 0,
                "Expected Name deletion to remove only the Name row.");
            assertCondition(countRows(factory, SampleProfile.Organisation.class) == 1,
                "Expected Organisation to remain after deleting Name.");
            assertCondition(countRows(factory, SampleProfile.IdentifiedObject.class) == 1,
                "Expected IdentifiedObject base row to remain after deleting Name.");

            // FK guard: deleting an Organisation still referenced by a Name must fail
            inTransaction(factory, s -> {
                SampleProfile.Name blocking = new SampleProfile.Name();
                blocking.setName("Blocking Name");
                blocking.setIdentifiedObject(s.get(SampleProfile.Organisation.class, "org-name-001"));
                s.persist(blocking);
            });
            Exception failure = tryInTransaction(factory, s ->
                s.remove(s.get(SampleProfile.Organisation.class, "org-name-001")));
            assertCondition(failure != null,
                "Expected deleting an Organisation still referenced by Name to fail.");
            assertCondition(countRows(factory, SampleProfile.Organisation.class) == 1,
                "Expected Organisation row to remain after failed delete.");
            assertCondition(countRows(factory, SampleProfile.Name.class) == 1,
                "Expected Name row to remain after failed Organisation delete.");
        });
    }

    // ================================================================
    // Section 8: Parent Organisation Delete Guard
    // ================================================================

    static void verifyParentOrganisationDeleteGuard() {
        withFreshDatabase(factory -> {
            inTransaction(factory, s -> {
                SampleProfile.ParentOrganization parent = new SampleProfile.ParentOrganization();
                parent.setMRID("org-parent-001");
                parent.setName("Parent Utility");
                SampleProfile.Organisation child = new SampleProfile.Organisation();
                child.setMRID("org-child-001");
                child.setName("Child Utility");
                child.setParentOrganisation(parent);
                s.persist(parent);
                s.persist(child);
            });
            Exception failure = tryInTransaction(factory, s ->
                s.remove(s.get(SampleProfile.ParentOrganization.class, "org-parent-001")));
            assertCondition(failure != null,
                "Expected deleting ParentOrganization with child references to fail.");
            assertCondition(countRows(factory, SampleProfile.ParentOrganization.class) == 1,
                "Expected ParentOrganization row to remain after failed delete.");
            assertCondition(countRows(factory, SampleProfile.Organisation.class) == 2,
                "Expected child Organisation to remain after failed parent delete.");
        });
    }

    // ================================================================
    // Section 9: Compound Lifecycle (owner delete cascade + replacement baseline)
    // ================================================================

    static SampleProfile.StreetAddress createAddressGraph(String prefix) {
        SampleProfile.StreetAddress address = new SampleProfile.StreetAddress();
        address.setPoBox(prefix + "-po-box");
        SampleProfile.StreetDetail street = new SampleProfile.StreetDetail();
        street.setName(prefix + "-street");
        SampleProfile.TownDetail town = new SampleProfile.TownDetail();
        town.setName(prefix + "-town");
        address.setStreetDetail(street);
        address.setTownDetail(town);
        address.setStatus(new SampleProfile.Status());
        return address;
    }

    static void persistAddressGraph(Session s, SampleProfile.StreetAddress address) {
        s.persist(address.getStatus());
        s.persist(address.getStreetDetail());
        s.persist(address.getTownDetail());
        s.persist(address);
    }

    static void verifyCompoundLifecycle() {
        withFreshDatabase(factory -> {
            inTransaction(factory, s -> {
                SampleProfile.Organisation org = new SampleProfile.Organisation();
                org.setMRID("org-compound-001");
                org.setName("Compound Utility");
                SampleProfile.TelephoneNumber phone = new SampleProfile.TelephoneNumber();
                phone.setItuPhone("+1-555-0200");
                org.setPhone1(phone);
                SampleProfile.StreetAddress address = createAddressGraph("initial");
                org.setStreetAddress(address);
                s.persist(phone);
                persistAddressGraph(s, address);
                s.persist(org);
            });
            // Owner delete: forward cascade=REMOVE should delete the compound graph,
            // matching the intent of SQL's reverse ON DELETE CASCADE.
            inTransaction(factory, s -> s.remove(s.get(SampleProfile.Organisation.class, "org-compound-001")));
            long phones = countRows(factory, SampleProfile.TelephoneNumber.class);
            long addresses = countRows(factory, SampleProfile.StreetAddress.class);
            long statuses = countRows(factory, SampleProfile.Status.class);
            long streets = countRows(factory, SampleProfile.StreetDetail.class);
            long towns = countRows(factory, SampleProfile.TownDetail.class);
            // This is JPA's own cascade working, not a by-design SQL/JPA difference, so it is asserted
            // rather than merely reported. Nested compounds reach it through StreetAddress.
            assertCondition(phones + addresses + statuses + streets + towns == 0,
                "Expected deleting the owning Organisation to cascade-remove the whole compound graph "
                + "(TelephoneNumber=" + phones + ", StreetAddress=" + addresses + ", Status=" + statuses
                + ", StreetDetail=" + streets + ", TownDetail=" + towns + ").");

            // Replacement baseline: no generated cleanup layer exists in the JPA output
            // (the generated C# has a DbContextBase SaveChanges cleanup; JPA does not).
            SampleProfile.TelephoneNumber originalPhone = new SampleProfile.TelephoneNumber();
            SampleProfile.StreetAddress originalAddress = createAddressGraph("replace-initial");
            inTransaction(factory, s -> {
                SampleProfile.Organisation org = new SampleProfile.Organisation();
                org.setMRID("org-compound-002");
                org.setName("Replace Utility");
                originalPhone.setItuPhone("+1-555-0300");
                org.setPhone1(originalPhone);
                org.setStreetAddress(originalAddress);
                s.persist(originalPhone);
                persistAddressGraph(s, originalAddress);
                s.persist(org);
            });
            // Counts are taken here so the baseline does not depend on what the owner-delete
            // probe above left behind.
            long phonesBefore = countRows(factory, SampleProfile.TelephoneNumber.class);
            long addressesBefore = countRows(factory, SampleProfile.StreetAddress.class);
            long statusesBefore = countRows(factory, SampleProfile.Status.class);
            long streetsBefore = countRows(factory, SampleProfile.StreetDetail.class);
            long townsBefore = countRows(factory, SampleProfile.TownDetail.class);
            inTransaction(factory, s -> {
                SampleProfile.Organisation org = s.get(SampleProfile.Organisation.class, "org-compound-002");
                SampleProfile.TelephoneNumber replacement = new SampleProfile.TelephoneNumber();
                replacement.setItuPhone("+1-555-0399");
                SampleProfile.StreetAddress replacementAddress = createAddressGraph("replace-updated");
                s.persist(replacement);
                persistAddressGraph(s, replacementAddress);
                org.setPhone1(replacement);
                org.setStreetAddress(replacementAddress);
            });
            try (Session s = factory.openSession()) {
                SampleProfile.Organisation reloaded = s.get(SampleProfile.Organisation.class, "org-compound-002");
                assertCondition(reloaded.getPhone1() != null && "+1-555-0399".equals(reloaded.getPhone1().getItuPhone()),
                    "Expected replacing phone1 to persist the new TelephoneNumber.");
                assertCondition(!originalPhone.getId().equals(reloaded.getPhone1().getId()),
                    "Expected replacing phone1 to update the stored foreign key.");
                assertCondition(reloaded.getStreetAddress() != null
                        && !originalAddress.getId().equals(reloaded.getStreetAddress().getId()),
                    "Expected replacing streetAddress to update the stored foreign key.");
            }
            long phonesAfter = countRows(factory, SampleProfile.TelephoneNumber.class);
            long addressesAfter = countRows(factory, SampleProfile.StreetAddress.class);
            long statusesAfter = countRows(factory, SampleProfile.Status.class);
            long streetsAfter = countRows(factory, SampleProfile.StreetDetail.class);
            long townsAfter = countRows(factory, SampleProfile.TownDetail.class);
            assertCondition(phonesAfter == phonesBefore + 1 && addressesAfter == addressesBefore + 1
                    && statusesAfter == statusesBefore + 1 && streetsAfter == streetsBefore + 1
                    && townsAfter == townsBefore + 1,
                "Expected the generated-only baseline to leave the replaced compound graph orphaned "
                + "(TelephoneNumber " + phonesBefore + "->" + phonesAfter + ", StreetAddress " + addressesBefore + "->"
                + addressesAfter + ", Status " + statusesBefore + "->" + statusesAfter + ", StreetDetail "
                + streetsBefore + "->" + streetsAfter + ", TownDetail " + townsBefore + "->" + townsAfter + ").");
            diagnostics.add("BASELINE OBSERVED: replacing compound references leaves the old rows orphaned "
                + "(replaced TelephoneNumber and the nested StreetAddress/Status/StreetDetail/TownDetail graph all "
                + "remain) — no DbContextBase-style cleanup exists in generated JPA (the C# harness asserts the "
                + "same in its generated-only Mapping Baseline).");
        });
    }

    // ================================================================
    // Section 10: Null Detach Baseline
    // ================================================================

    /**
     * Detach scenario of the C# "Generated Null Detach Cleanup" section, asserted at the
     * generated-only baseline (the outcome the C# "Generated Mapping Baseline" pins for its
     * raw-mapping context). The C# cleanup section itself asserts the opposite — rows deleted —
     * because it runs with the generated DbContextBase cleanup layer, which generated JPA does
     * not have. Here clearing a compound reference updates the owner's FK but leaves the whole
     * compound graph orphaned.
     */
    static void verifyNullDetachBaseline() {
        withFreshDatabase(factory -> {
            inTransaction(factory, s -> {
                SampleProfile.Organisation org = new SampleProfile.Organisation();
                org.setMRID("org-null-001");
                org.setName("Null Detach Utility");
                SampleProfile.ElectronicAddress email = new SampleProfile.ElectronicAddress();
                email.setEmail1("ops@example.com");
                SampleProfile.TelephoneNumber phone = new SampleProfile.TelephoneNumber();
                phone.setItuPhone("+1-555-0300");
                SampleProfile.StreetAddress address = createAddressGraph("null-detach");
                org.setElectronicAddress(email);
                org.setPhone1(phone);
                org.setStreetAddress(address);
                s.persist(email);
                s.persist(phone);
                persistAddressGraph(s, address);
                s.persist(org);
            });
            inTransaction(factory, s -> {
                SampleProfile.Organisation org = s.get(SampleProfile.Organisation.class, "org-null-001");
                org.setElectronicAddress(null);
                org.setPhone1(null);
                org.setStreetAddress(null);
            });
            try (Session s = factory.openSession()) {
                SampleProfile.Organisation reloaded = s.get(SampleProfile.Organisation.class, "org-null-001");
                assertCondition(reloaded.getElectronicAddress() == null, "Expected electronicAddress to be cleared.");
                assertCondition(reloaded.getPhone1() == null, "Expected phone1 to be cleared.");
                assertCondition(reloaded.getStreetAddress() == null, "Expected streetAddress to be cleared.");
            }
            long emails = countRows(factory, SampleProfile.ElectronicAddress.class);
            long phones = countRows(factory, SampleProfile.TelephoneNumber.class);
            long addresses = countRows(factory, SampleProfile.StreetAddress.class);
            long statuses = countRows(factory, SampleProfile.Status.class);
            long streets = countRows(factory, SampleProfile.StreetDetail.class);
            long towns = countRows(factory, SampleProfile.TownDetail.class);
            assertCondition(emails == 1 && phones == 1 && addresses == 1 && statuses == 1 && streets == 1 && towns == 1,
                "Expected the generated-only baseline to leave every detached compound row orphaned (ElectronicAddress="
                + emails + ", TelephoneNumber=" + phones + ", StreetAddress=" + addresses + ", Status=" + statuses
                + ", StreetDetail=" + streets + ", TownDetail=" + towns + ").");
            diagnostics.add("BASELINE OBSERVED: clearing compound references leaves the detached compound graph orphaned "
                + "(one row each in ElectronicAddress/TelephoneNumber/StreetAddress/Status/StreetDetail/TownDetail) — "
                + "generated C# deletes them via DbContextBase cleanup.");
        });
    }

    // ================================================================
    // Section 11: Inheritance Storage
    // ================================================================

    static void verifyInheritanceStorage() {
        withFreshDatabase(factory -> {
            inTransaction(factory, s -> {
                SampleProfile.Organisation org = new SampleProfile.Organisation();
                org.setMRID("org-base-001");
                org.setName("Base Organisation");
                SampleProfile.ParentOrganization parent = new SampleProfile.ParentOrganization();
                parent.setMRID("org-parent-002");
                parent.setName("Derived Parent Organisation");
                SampleProfile.WireInfo wire = new SampleProfile.WireInfo();
                wire.setMRID("wire-base-001");
                wire.setName("Base Wire");
                SampleProfile.OverheadWireInfo overhead = new SampleProfile.OverheadWireInfo();
                overhead.setMRID("wire-overhead-001");
                overhead.setName("Derived Wire");
                s.persist(org);
                s.persist(parent);
                s.persist(wire);
                s.persist(overhead);
            });
            try (Session s = factory.openSession()) {
                List<SampleProfile.IdentifiedObject> all = loadAll(s, SampleProfile.IdentifiedObject.class);
                assertCondition(all.size() == 4,
                    "Expected four IdentifiedObject rows across the hierarchy, got " + all.size());
                for (Class<?> expected : List.of(SampleProfile.Organisation.class,
                        SampleProfile.ParentOrganization.class, SampleProfile.WireInfo.class,
                        SampleProfile.OverheadWireInfo.class)) {
                    assertCondition(all.stream().anyMatch(o -> o.getClass() == expected),
                        "Expected polymorphic base query to materialize " + expected.getSimpleName());
                }
            }
        });
    }

    // ================================================================
    // Section 12: Inheritance Delete Cleanup
    // ================================================================

    static void verifyInheritanceDeleteCleanup() {
        withFreshDatabase(factory -> {
            inTransaction(factory, s -> {
                SampleProfile.ParentOrganization parent = new SampleProfile.ParentOrganization();
                parent.setMRID("org-parent-delete-001");
                parent.setName("Delete Parent");
                s.persist(parent);
                SampleProfile.OverheadWireInfo overhead = new SampleProfile.OverheadWireInfo();
                overhead.setMRID("wire-overhead-delete-001");
                overhead.setName("Delete Wire");
                s.persist(overhead);
            });
            inTransaction(factory, s -> {
                s.remove(s.get(SampleProfile.ParentOrganization.class, "org-parent-delete-001"));
                s.remove(s.get(SampleProfile.OverheadWireInfo.class, "wire-overhead-delete-001"));
            });
            try (Connection c = connect(currentDbUrl()); Statement st = c.createStatement()) {
                // Three-level chain (IdentifiedObject -> Organisation -> ParentOrganization) and
                // four-level chain (IdentifiedObject -> AssetInfo -> WireInfo -> OverheadWireInfo).
                for (String table : List.of("IdentifiedObject", "Organisation", "ParentOrganization",
                        "AssetInfo", "WireInfo", "OverheadWireInfo")) {
                    try (ResultSet rs = st.executeQuery("select count(*) from \"" + resolveTable(c, table) + "\"")) {
                        rs.next();
                        assertCondition(rs.getInt(1) == 0,
                            "Expected JOINED deletes to clear the " + table + " row.");
                    }
                }
            } catch (SQLException e) {
                throw new RuntimeException(e);
            }
        });
    }

    // ================================================================
    // Section 13: Compoundless Entity Lifecycle
    // ================================================================

    static void verifyCompoundlessEntityLifecycle() {
        withFreshDatabase(factory -> {
            inTransaction(factory, s -> {
                SampleProfile.Organisation org = new SampleProfile.Organisation();
                org.setMRID("org-plain-001");
                org.setName("No Compounds Utility");
                s.persist(org);
            });
            inTransaction(factory, s ->
                s.get(SampleProfile.Organisation.class, "org-plain-001").setName("No Compounds Utility Updated"));
            try (Session s = factory.openSession()) {
                assertCondition("No Compounds Utility Updated".equals(
                        s.get(SampleProfile.Organisation.class, "org-plain-001").getName()),
                    "Expected update to round-trip.");
            }
            inTransaction(factory, s -> s.remove(s.get(SampleProfile.Organisation.class, "org-plain-001")));
            assertCondition(countRows(factory, SampleProfile.Organisation.class) == 0,
                "Expected compoundless Organisation to delete cleanly.");
            assertCondition(countRows(factory, SampleProfile.IdentifiedObject.class) == 0,
                "Expected base IdentifiedObject row to be removed too.");
        });
    }

    // ================================================================
    // Section 14: Authoritative DDL Entity Round-Trip
    // ================================================================

    /**
     * Runs the generated entities against the schema created by the authoritative
     * sql-rdfs-ansi92 DDL with hbm2ddl=none — the deployment mode the generated
     * file's own header prescribes. Counterpart of the C# harness sections that
     * exercise the generated cleanup story: in the JPA/SQL pairing that story
     * lives in the DDL, so it has to be tested on the DDL-created schema.
     */
    static void verifyAuthoritativeDdlEntityRoundTrip() throws Exception {
        String url = "jdbc:h2:mem:jpa_smoke_authddl_roundtrip;DB_CLOSE_DELAY=-1";
        String phoneId = "11111111-1111-1111-1111-111111111111";
        try (Connection c = connect(url)) {
            applyAuthoritativeDdl(c);
            try (Statement st = c.createStatement()) {
                st.execute("INSERT INTO \"IdentifiedObject\" (\"mRID\", \"name\") VALUES ('org-authddl-001', 'DDL Org')");
                st.execute("INSERT INTO \"Organisation\" (\"mRID\") VALUES ('org-authddl-001')");
                SQLException compoundFirst = expectSqlFailure(st,
                    "INSERT INTO \"TelephoneNumber\" (\"id\", \"ituPhone\") VALUES ('" + phoneId + "', '+1-555-0100')");
                SQLException ownerFirst = expectSqlFailure(st,
                    "UPDATE \"Organisation\" SET \"phone1\" = '" + phoneId + "' WHERE \"mRID\" = 'org-authddl-001'");
                if (compoundFirst != null && ownerFirst != null) {
                    diagnostics.add("DDL FINDING: a compound row cannot be inserted in either order - the forward "
                        + "owner FK requires the compound row to exist first, the reverse cascade FK requires the "
                        + "owner column to already point at it. Deferring the checks does not rescue this: a compound "
                        + "type reachable from two owner columns (phone1/phone2, postalAddress/streetAddress) carries "
                        + "one reverse FK per slot, so no single row can satisfy all of them at once - the end state "
                        + "itself is unsatisfiable, not merely the statement order. Confirmed on PostgreSQL 14.10, "
                        + "where SET CONSTRAINTS ALL DEFERRED has no effect because the DDL declares no constraint "
                        + "DEFERRABLE.");
                } else {
                    diagnostics.add("DDL NOTE: the authoritative schema now admits a compound insert order "
                        + "(compound-first was " + describeOutcome(compoundFirst) + ", owner-first was "
                        + describeOutcome(ownerFirst) + ") - the paired forward/reverse FK deadlock appears resolved.");
                }
            }
            seedWithIntegrityOff(c,
                "INSERT INTO \"TelephoneNumber\" (\"id\", \"ituPhone\") VALUES ('" + phoneId + "', '+1-555-0100')",
                "UPDATE \"Organisation\" SET \"phone1\" = '" + phoneId + "' WHERE \"mRID\" = 'org-authddl-001'");
        }
        withDatabase(url, "none", factory -> inTransaction(factory, s -> {
            SampleProfile.Organisation loaded = s.get(SampleProfile.Organisation.class, "org-authddl-001");
            assertCondition(loaded != null, "Expected Organisation to load from the authoritative schema.");
            assertCondition("DDL Org".equals(loaded.getName()),
                "Expected the JOINED inheritance name to load from the IdentifiedObject table.");
            assertCondition(loaded.getPhone1() != null && "+1-555-0100".equals(loaded.getPhone1().getItuPhone()),
                "Expected the phone1 compound to load through the VARCHAR(100) surrogate key.");
        }));
    }

    // ================================================================
    // Section 15: Authoritative DDL Owner Delete Cascade
    // ================================================================

    /**
     * The C# harness proves compound cleanup via the generated DbContextBase
     * helpers; the SQL DDL's equivalent mechanism is the reverse-reference
     * ON DELETE CASCADE chain. Deleting the owning Organisation must clear
     * every compound row, including the compounds nested under StreetAddress.
     */
    static void verifyAuthoritativeDdlOwnerDeleteCascade() throws Exception {
        String url = "jdbc:h2:mem:jpa_smoke_authddl_cascade;DB_CLOSE_DELAY=-1";
        try (Connection c = connect(url)) {
            applyAuthoritativeDdl(c);
            seedWithIntegrityOff(c,
                "INSERT INTO \"IdentifiedObject\" (\"mRID\", \"name\") VALUES ('org-cascade-001', 'Cascade Org')",
                "INSERT INTO \"Organisation\" (\"mRID\", \"electronicAddress\", \"phone1\", \"phone2\", \"postalAddress\", \"streetAddress\") VALUES "
                    + "('org-cascade-001', 'e0000000-0000-0000-0000-000000000001', 'a0000000-0000-0000-0000-000000000001', "
                    + "'a0000000-0000-0000-0000-000000000002', 'b0000000-0000-0000-0000-000000000001', 'b0000000-0000-0000-0000-000000000002')",
                "INSERT INTO \"ElectronicAddress\" (\"id\", \"email1\") VALUES ('e0000000-0000-0000-0000-000000000001', 'ops@example.com')",
                "INSERT INTO \"TelephoneNumber\" (\"id\", \"ituPhone\") VALUES ('a0000000-0000-0000-0000-000000000001', '+1-555-0100')",
                "INSERT INTO \"TelephoneNumber\" (\"id\", \"ituPhone\") VALUES ('a0000000-0000-0000-0000-000000000002', '+1-555-0101')",
                "INSERT INTO \"StreetAddress\" (\"id\", \"postalCode\", \"status\", \"streetDetail\", \"townDetail\") VALUES "
                    + "('b0000000-0000-0000-0000-000000000001', '53703', 'c0000000-0000-0000-0000-000000000001', "
                    + "'d0000000-0000-0000-0000-000000000001', 'f0000000-0000-0000-0000-000000000001')",
                "INSERT INTO \"StreetAddress\" (\"id\", \"postalCode\", \"status\", \"streetDetail\", \"townDetail\") VALUES "
                    + "('b0000000-0000-0000-0000-000000000002', '53704', 'c0000000-0000-0000-0000-000000000002', "
                    + "'d0000000-0000-0000-0000-000000000002', 'f0000000-0000-0000-0000-000000000002')",
                "INSERT INTO \"Status\" (\"id\", \"value\") VALUES ('c0000000-0000-0000-0000-000000000001', 'verified')",
                "INSERT INTO \"Status\" (\"id\", \"value\") VALUES ('c0000000-0000-0000-0000-000000000002', 'verified')",
                "INSERT INTO \"StreetDetail\" (\"id\", \"name\") VALUES ('d0000000-0000-0000-0000-000000000001', 'Main St')",
                "INSERT INTO \"StreetDetail\" (\"id\", \"name\") VALUES ('d0000000-0000-0000-0000-000000000002', 'Second St')",
                "INSERT INTO \"TownDetail\" (\"id\", \"name\") VALUES ('f0000000-0000-0000-0000-000000000001', 'Madison')",
                "INSERT INTO \"TownDetail\" (\"id\", \"name\") VALUES ('f0000000-0000-0000-0000-000000000002', 'Madison')");
            try (Statement st = c.createStatement()) {
                SQLException ownerDelete = expectSqlFailure(st,
                    "DELETE FROM \"Organisation\" WHERE \"mRID\" = 'org-cascade-001'");
                if (ownerDelete == null) {
                    st.execute("DELETE FROM \"IdentifiedObject\" WHERE \"mRID\" = 'org-cascade-001'");
                    for (String table : List.of("ElectronicAddress", "TelephoneNumber", "StreetAddress",
                            "Status", "StreetDetail", "TownDetail")) {
                        assertCondition(countTable(st, table) == 0,
                            "Expected deleting the owning Organisation to cascade-clear " + table
                            + " (the SQL counterpart of the C# DbContextBase cleanup).");
                    }
                    diagnostics.add("DDL NOTE: owner delete cascaded the full compound cleanup chain end-to-end.");
                } else {
                    // The reverse cascade tries to remove compound rows while the owner row -
                    // whose forward FK still references them - has not been removed yet, so the
                    // documented cleanup mechanism cannot execute at all. The delete must then
                    // be a no-op: verify statement-level atomicity left the graph intact.
                    assertCondition(countTable(st, "Organisation") == 1,
                        "Expected the rejected owner delete to leave the Organisation row in place.");
                    assertCondition(countTable(st, "TelephoneNumber") == 2
                            && countTable(st, "StreetAddress") == 2
                            && countTable(st, "Status") == 2,
                        "Expected the rejected owner delete to leave all compound rows in place.");
                    diagnostics.add("DDL FINDING: the reverse ON DELETE CASCADE compound cleanup cannot execute "
                        + "under immediate FK checking - the forward owner FK still references each compound row "
                        + "mid-cascade, so the owner delete is rejected outright (" + firstLine(ownerDelete.getMessage())
                        + "). Deferred constraints would not fix this either - see the Section 14 DDL FINDING.");
                }
            }
        }
    }

    // ================================================================
    // Section 16: Authoritative DDL Replacement Guard
    // ================================================================

    /**
     * Generated C# supports compound replacement/detach with DbContextBase
     * cleanup; the authoritative schema instead freezes compound ownership:
     * repointing, detaching, and deleting a referenced compound are all
     * rejected under immediate FK checking. This section pins down those
     * semantics and confirms in-place attribute updates still work.
     */
    static void verifyAuthoritativeDdlReplacementGuard() throws Exception {
        String url = "jdbc:h2:mem:jpa_smoke_authddl_guard;DB_CLOSE_DELAY=-1";
        String oldPhone = "11111111-1111-1111-1111-111111111111";
        String newPhone = "22222222-2222-2222-2222-222222222222";
        try (Connection c = connect(url)) {
            applyAuthoritativeDdl(c);
            seedWithIntegrityOff(c,
                "INSERT INTO \"IdentifiedObject\" (\"mRID\", \"name\") VALUES ('org-guard-001', 'Guard Org')",
                "INSERT INTO \"Organisation\" (\"mRID\", \"phone1\") VALUES ('org-guard-001', '" + oldPhone + "')",
                "INSERT INTO \"TelephoneNumber\" (\"id\", \"ituPhone\") VALUES ('" + oldPhone + "', '+1-555-0100')",
                // A valid replacement row, already present: repointing at it is the case a consumer
                // would actually hit, and is a stronger probe than repointing at a missing id.
                "INSERT INTO \"TelephoneNumber\" (\"id\", \"ituPhone\") VALUES ('" + newPhone + "', '+1-555-0200')");
            try (Statement st = c.createStatement()) {
                assertCondition(expectSqlFailure(st,
                        "UPDATE \"Organisation\" SET \"phone1\" = '" + newPhone + "' WHERE \"mRID\" = 'org-guard-001'") != null,
                    "Expected repointing phone1 at an existing replacement compound row to be rejected "
                    + "(the reverse cascade FK orphans the old row).");
                assertCondition(expectSqlFailure(st,
                        "DELETE FROM \"TelephoneNumber\" WHERE \"id\" = '" + oldPhone + "'") != null,
                    "Expected the forward FK to reject deleting a still-referenced compound row.");
                assertCondition(expectSqlFailure(st,
                        "UPDATE \"Organisation\" SET \"phone1\" = NULL WHERE \"mRID\" = 'org-guard-001'") != null,
                    "Expected the reverse cascade FK to reject detaching phone1 while its compound row remains.");
                st.execute("UPDATE \"TelephoneNumber\" SET \"ituPhone\" = '+1-555-0199' WHERE \"id\" = '" + oldPhone + "'");
                try (ResultSet rs = st.executeQuery(
                        "SELECT \"ituPhone\" FROM \"TelephoneNumber\" WHERE \"id\" = '" + oldPhone + "'")) {
                    rs.next();
                    assertCondition("+1-555-0199".equals(rs.getString(1)),
                        "Expected in-place compound attribute updates to remain possible.");
                }
            }
            diagnostics.add("PARITY GAP OBSERVED: the authoritative schema freezes compound ownership under "
                + "immediate FK checking (repointing at an existing replacement, detaching, and deleting a "
                + "referenced compound are all rejected, and the owner delete cascade is itself blocked - see the "
                + "DDL FINDING above - leaving in-place attribute updates as the only mutation), while generated C# "
                + "supports replacement/detach via DbContextBase cleanup and generated JPA relies on forward "
                + "cascade only. Note the probe runs on a state seeded with referential integrity disabled, because "
                + "no conforming application could create it (Section 14).");
        }
    }

    // ================================================================
    // Infrastructure
    // ================================================================

    interface Section {
        void run() throws Exception;
    }

    static void runSection(String name, Section section) {
        try {
            section.run();
        } catch (Exception e) {
            throw new IllegalStateException("Section '" + name + "' failed: " + e.getMessage(), e);
        }
        completedSections.add(name);
    }

    static String currentDbUrl() {
        return "jdbc:h2:mem:jpa_smoke_" + dbCounter + ";DB_CLOSE_DELAY=-1";
    }

    /** Fresh in-memory H2 database with the schema created by Hibernate from allClasses. */
    static void withFreshDatabase(Consumer<SessionFactory> action) {
        dbCounter++;
        withDatabase(currentDbUrl(), "create", action);
    }

    /** SessionFactory over an existing H2 URL with the given hbm2ddl mode. */
    static void withDatabase(String url, String hbm2ddl, Consumer<SessionFactory> action) {
        Configuration cfg = new Configuration();
        for (Class<?> entity : SampleProfile.allClasses) {
            cfg.addAnnotatedClass(entity);
        }
        cfg.setProperty("hibernate.connection.url", url);
        cfg.setProperty("hibernate.connection.driver_class", "org.h2.Driver");
        cfg.setProperty("hibernate.connection.username", "sa");
        cfg.setProperty("hibernate.connection.password", "");
        cfg.setProperty("hibernate.hbm2ddl.auto", hbm2ddl);
        cfg.setProperty("hibernate.show_sql", "false");
        // The generated entities use raw CIM attribute names as column names (e.g.
        // Status.value); the authoritative sql-rdfs-ansi92 DDL quotes every identifier,
        // so Hibernate must quote them too or H2 rejects reserved words like VALUE.
        cfg.setProperty("hibernate.globally_quoted_identifiers", "true");
        // ...but generated @Column(columnDefinition=...) values (e.g. INTEGER DEFAULT 0)
        // must not be quoted along with them.
        cfg.setProperty("hibernate.globally_quoted_identifiers_skip_column_definitions", "true");
        try (SessionFactory factory = cfg.buildSessionFactory()) {
            action.accept(factory);
        }
    }

    /** Executes the authoritative sql-rdfs-ansi92 DDL, failing the section on any rejected statement. */
    static void applyAuthoritativeDdl(Connection c) throws Exception {
        String sql = Files.readString(SQL_PATH);
        StringBuilder cleaned = new StringBuilder();
        for (String line : sql.split("\n")) {
            if (!line.strip().startsWith("--")) {
                cleaned.append(line).append('\n');
            }
        }
        try (Statement st = c.createStatement()) {
            for (String stmt : cleaned.toString().split(";")) {
                if (stmt.isBlank()) continue;
                st.execute(stmt);
            }
        }
    }

    /**
     * Seeds rows with H2 referential integrity disabled. Required because the
     * DDL's circular forward/reverse compound FKs admit no valid insert order
     * (see the Section 14 probe); re-enabling does not re-validate existing rows.
     */
    static void seedWithIntegrityOff(Connection c, String... statements) throws SQLException {
        try (Statement st = c.createStatement()) {
            st.execute("SET REFERENTIAL_INTEGRITY FALSE");
            for (String statement : statements) {
                st.execute(statement);
            }
            st.execute("SET REFERENTIAL_INTEGRITY TRUE");
        }
    }

    static int countTable(Statement st, String table) throws SQLException {
        try (ResultSet rs = st.executeQuery("SELECT COUNT(*) FROM \"" + table + "\"")) {
            rs.next();
            return rs.getInt(1);
        }
    }

    static SQLException expectSqlFailure(Statement st, String sql) {
        try {
            st.execute(sql);
            return null;
        } catch (SQLException e) {
            return e;
        }
    }

    static String describeOutcome(SQLException e) {
        return e == null ? "accepted" : "rejected";
    }

    static void inTransaction(SessionFactory factory, Consumer<Session> work) {
        try (Session session = factory.openSession()) {
            session.beginTransaction();
            work.accept(session);
            session.getTransaction().commit();
        }
    }

    /** Runs work in a transaction, returning the failure if the commit fails (FK guards). */
    static Exception tryInTransaction(SessionFactory factory, Consumer<Session> work) {
        try (Session session = factory.openSession()) {
            try {
                session.beginTransaction();
                work.accept(session);
                session.getTransaction().commit();
                return null;
            } catch (Exception e) {
                if (session.getTransaction().isActive()) {
                    session.getTransaction().rollback();
                }
                return e;
            }
        }
    }

    static long countRows(SessionFactory factory, Class<?> entity) {
        try (Session session = factory.openSession()) {
            var cb = session.getCriteriaBuilder();
            var query = cb.createQuery(Long.class);
            query.select(cb.count(query.from(entity)));
            return session.createQuery(query).getSingleResult();
        }
    }

    /** Loads all rows of an entity via the Criteria API (nested classes are not
     *  resolvable by simple name in HQL strings). */
    static <T> List<T> loadAll(Session session, Class<T> entity) {
        var cb = session.getCriteriaBuilder();
        var query = cb.createQuery(entity);
        query.from(entity);
        return session.createQuery(query).getResultList();
    }

    static void assertCondition(boolean condition, String message) {
        if (!condition) {
            throw new IllegalStateException(message);
        }
    }

    // ---- reflection assertions ----

    static void assertEntityTable(Class<?> type, String tableName) {
        assertCondition(type.isAnnotationPresent(Entity.class), type.getSimpleName() + " must be @Entity.");
        Table table = type.getAnnotation(Table.class);
        assertCondition(table != null && table.name().equals(tableName),
            "Expected @Table(name=\"" + tableName + "\") on " + type.getSimpleName());
    }

    static Field declaredIdField(Class<?> type) {
        return Arrays.stream(type.getDeclaredFields())
            .filter(f -> f.isAnnotationPresent(Id.class))
            .findFirst().orElse(null);
    }

    static void assertNaturalKey(Class<?> type, String fieldName, String columnName) {
        Field id = declaredIdField(type);
        assertCondition(id != null && id.getName().equals(fieldName),
            "Expected " + type.getSimpleName() + " @Id on field '" + fieldName + "'.");
        Column column = id.getAnnotation(Column.class);
        assertCondition(column != null && column.name().equals(columnName),
            "Expected " + type.getSimpleName() + "." + fieldName + " to map to column '" + columnName + "'.");
    }

    static void assertSurrogateUuidKey(Class<?> type) {
        Field id = declaredIdField(type);
        assertCondition(id != null && id.getName().equals("id"),
            "Expected " + type.getSimpleName() + " to declare surrogate @Id 'id'.");
        Column column = id.getAnnotation(Column.class);
        assertCondition(column != null && column.name().equals("id"),
            "Expected " + type.getSimpleName() + ".id to map to column 'id'.");
        assertCondition(id.isAnnotationPresent(GeneratedValue.class)
                && id.getAnnotation(GeneratedValue.class).strategy() == GenerationType.UUID,
            "Expected " + type.getSimpleName() + ".id to be @GeneratedValue(strategy=UUID).");
    }

    static Field declaredField(Class<?> type, String name) {
        try {
            return type.getDeclaredField(name);
        } catch (NoSuchFieldException e) {
            throw new IllegalStateException("Expected " + type.getSimpleName() + " to declare field '" + name + "'.");
        }
    }

    static JoinColumn joinColumn(Class<?> type, String field) {
        JoinColumn column = declaredField(type, field).getAnnotation(JoinColumn.class);
        assertCondition(column != null, "Expected @JoinColumn on " + type.getSimpleName() + "." + field);
        return column;
    }

    static void assertColumnName(Class<?> type, String field, String columnName) {
        Column column = declaredField(type, field).getAnnotation(Column.class);
        assertCondition(column != null && column.name().equals(columnName),
            "Expected " + type.getSimpleName() + "." + field + " to map to column '" + columnName + "'.");
    }

    /** True if the mapping declares the association column unique anywhere JPA allows it. */
    static boolean mappedUnique(Class<?> type, String field) {
        JoinColumn join = joinColumn(type, field);
        if (join.unique()) {
            return true;
        }
        Table table = type.getAnnotation(Table.class);
        if (table == null) {
            return false;
        }
        for (Index index : table.indexes()) {
            if (index.unique() && index.columnList().trim().replaceAll("(?i)\\s+(asc|desc)$", "").equalsIgnoreCase(join.name())) {
                return true;
            }
        }
        for (UniqueConstraint constraint : table.uniqueConstraints()) {
            if (constraint.columnNames().length == 1 && constraint.columnNames()[0].equalsIgnoreCase(join.name())) {
                return true;
            }
        }
        return false;
    }

    static void assertCascadeRemove(Class<?> type, String field, boolean expectRemove) {
        ManyToOne association = declaredField(type, field).getAnnotation(ManyToOne.class);
        assertCondition(association != null, "Expected @ManyToOne on " + type.getSimpleName() + "." + field);
        boolean cascadesRemove = Arrays.asList(association.cascade()).contains(CascadeType.REMOVE);
        assertCondition(cascadesRemove == expectRemove,
            "Expected " + type.getSimpleName() + "." + field + (expectRemove ? " to" : " not to")
            + " cascade REMOVE, found cascade=" + Arrays.toString(association.cascade()));
    }

    static void assertNoDeclaredId(Class<?> type) {
        Field id = declaredIdField(type);
        assertCondition(id == null,
            "Did not expect " + type.getSimpleName() + " to declare its own @Id (it inherits mRID).");
    }

    // ---- JDBC metadata helpers ----

    static Connection connect(String url) throws SQLException {
        return DriverManager.getConnection(url, "sa", "");
    }

    static Set<String> tableNames(Connection c) throws SQLException {
        Set<String> names = new TreeSet<>(String.CASE_INSENSITIVE_ORDER);
        try (ResultSet rs = c.getMetaData().getTables(null, "PUBLIC", null, new String[]{"TABLE", "BASE TABLE"})) {
            while (rs.next()) {
                names.add(rs.getString("TABLE_NAME"));
            }
        }
        return names;
    }

    static String resolveTable(Connection c, String table) throws SQLException {
        for (String t : tableNames(c)) {
            if (t.equalsIgnoreCase(table)) return t;
        }
        throw new IllegalStateException("Table not found: " + table);
    }

    static List<String> primaryKeyColumns(Connection c, String table) throws SQLException {
        List<String> cols = new ArrayList<>();
        try (ResultSet rs = c.getMetaData().getPrimaryKeys(null, "PUBLIC", resolveTable(c, table))) {
            while (rs.next()) {
                cols.add(rs.getString("COLUMN_NAME"));
            }
        }
        return cols;
    }

    static Set<String> columns(Connection c, String table) throws SQLException {
        Set<String> cols = new TreeSet<>(String.CASE_INSENSITIVE_ORDER);
        try (ResultSet rs = c.getMetaData().getColumns(null, "PUBLIC", resolveTable(c, table), null)) {
            while (rs.next()) {
                cols.add(rs.getString("COLUMN_NAME"));
            }
        }
        return cols;
    }

    static Set<String> foreignKeyEdges(Connection c) throws SQLException {
        Set<String> edges = new TreeSet<>();
        for (String t : tableNames(c)) {
            try (ResultSet rs = c.getMetaData().getImportedKeys(null, "PUBLIC", t)) {
                while (rs.next()) {
                    edges.add((rs.getString("FKTABLE_NAME") + "." + rs.getString("FKCOLUMN_NAME")
                            + "->" + rs.getString("PKTABLE_NAME") + "." + rs.getString("PKCOLUMN_NAME"))
                        .toUpperCase(Locale.ROOT));
                }
            }
        }
        return edges;
    }

    static boolean hasUniqueIndex(Connection c, String table, String column) throws SQLException {
        try (ResultSet rs = c.getMetaData().getIndexInfo(null, "PUBLIC", resolveTable(c, table), false, false)) {
            while (rs.next()) {
                String indexed = rs.getString("COLUMN_NAME");
                if (indexed != null && indexed.equalsIgnoreCase(column) && !rs.getBoolean("NON_UNIQUE")) {
                    return true;
                }
            }
        }
        return false;
    }

    static String columnType(Connection c, String table, String column) throws SQLException {
        try (ResultSet rs = c.getMetaData().getColumns(null, "PUBLIC", resolveTable(c, table), null)) {
            while (rs.next()) {
                if (rs.getString("COLUMN_NAME").equalsIgnoreCase(column)) {
                    return rs.getString("TYPE_NAME");
                }
            }
        }
        throw new IllegalStateException("Column not found: " + table + "." + column);
    }

    static int columnSize(Connection c, String table, String column) throws SQLException {
        try (ResultSet rs = c.getMetaData().getColumns(null, "PUBLIC", resolveTable(c, table), null)) {
            while (rs.next()) {
                if (rs.getString("COLUMN_NAME").equalsIgnoreCase(column)) {
                    return rs.getInt("COLUMN_SIZE");
                }
            }
        }
        throw new IllegalStateException("Column not found: " + table + "." + column);
    }

    static void assertPk(Connection c, String table, String expectedColumn) {
        try {
            List<String> pk = primaryKeyColumns(c, table);
            assertCondition(pk.size() == 1 && pk.get(0).equalsIgnoreCase(expectedColumn),
                "Expected PK of " + table + " to be " + expectedColumn + ", got " + pk);
        } catch (SQLException e) {
            throw new RuntimeException(e);
        }
    }

    static boolean containsIgnoreCase(Set<String> set, String value) {
        return set.stream().anyMatch(s -> s.equalsIgnoreCase(value));
    }

    static boolean equalsIgnoreCaseList(List<String> a, List<String> b) {
        if (a.size() != b.size()) return false;
        for (int i = 0; i < a.size(); i++) {
            if (!a.get(i).equalsIgnoreCase(b.get(i))) return false;
        }
        return true;
    }

    static void diffSets(String label, Set<String> left, Set<String> right) {
        List<String> only = left.stream().filter(v -> !containsIgnoreCase(right, v)).toList();
        if (!only.isEmpty()) {
            diagnostics.add("SCHEMA DIFF (" + label + "): " + only);
        }
    }

    static String firstLine(String s) {
        return s.strip().lines().findFirst().orElse("");
    }

    private JpaSmokeTest() {
    }
}
