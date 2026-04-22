package au.com.langdale.cimtoole.test.headless;

import java.io.IOException;
import java.io.InputStream;
import java.nio.charset.StandardCharsets;
import java.nio.file.Files;
import java.nio.file.Path;
import java.nio.file.StandardCopyOption;

import org.eclipse.core.resources.IFile;
import org.eclipse.core.resources.IMarker;
import org.eclipse.core.resources.IResource;
import org.eclipse.core.resources.IncrementalProjectBuilder;
import org.eclipse.core.runtime.CoreException;

import au.com.langdale.cimtoole.builder.ProfileBuildlets.TransformBuildlet;
import au.com.langdale.cimtoole.project.Task;
import au.com.langdale.cimtoole.registries.ProfileBuildletConfigUtils;
import au.com.langdale.cimtoole.test.ProjectTest;
import au.com.langdale.kena.OntModel;

public class CSharpEfRdfsTransformTest extends ProjectTest {

	private static final String SAMPLE_PROFILE = "SampleProfile.owl";
	private static final String EXPECTATIONS_FILE = "SampleProfile.csharp-ef-rdfs.expectations.txt";
	private static final String SAMPLE_SCHEMA = TransformTasks.PART9_SCHEMA;
	private static final String SAMPLE_SCHEMA_NS = "http://iec.ch/TC57/CIM100#";
	private static final String BUILDER_KEY = "csharp-ef-rdfs";
	private static final String OUTPUT_EXT = "csharp-ef-rdfs.cs";

	@Override
	protected void setUp() throws Exception {
		super.setUp();
		setupSchema();
		setupProfile();
	}

	@Override
	protected String getProfileForTesting() {
		return SAMPLE_PROFILE;
	}

	@Override
	protected String getSchemaForTesting() {
		return SAMPLE_SCHEMA;
	}

	@Override
	protected String getSchemaNSForTesting() {
		return SAMPLE_SCHEMA_NS;
	}

	@Override
	protected void setUpTestData() {
		super.setUpTestData();
		copyProfileFixture();
	}

	private void copyProfileFixture() {
		try (InputStream is = Thread.currentThread().getContextClassLoader()
				.getResourceAsStream("CIMToolTestFiles/" + SAMPLE_PROFILE)) {
			assertNotNull("Sample profile resource should be available on the test classpath", is);
			Path destination = Path.of(getSamplesFolder(), SAMPLE_PROFILE);
			Files.copy(is, destination, StandardCopyOption.REPLACE_EXISTING);
		} catch (IOException e) {
			throw new RuntimeException("Unable to copy SampleProfile.owl into the temporary test samples folder", e);
		}
	}

	public final void testCSharpEfRdfsGeneration() throws CoreException, IOException {
		TransformBuildlet buildlet = ProfileBuildletConfigUtils.getTransformBuildlet(BUILDER_KEY);
		assertNotNull("csharp-ef-rdfs buildlet should be registered", buildlet);

		OntModel model = Task.getProfileModel(profile);
		buildlet.setFlagged(model, true);
		workspace.run(Task.saveProfile(profile, model), monitor);
		workspace.build(IncrementalProjectBuilder.INCREMENTAL_BUILD, monitor);

		IFile generated = getRelated(OUTPUT_EXT);
		assertTrue("generated C# output should exist", generated.exists());
		assertTrue("generated C# output should not have error markers",
				generated.findMaxProblemSeverity(null, true, IResource.DEPTH_ZERO) < IMarker.SEVERITY_ERROR);

		String output = Files.readString(generated.getLocation().toFile().toPath(), StandardCharsets.UTF_8);
		assertTrue("generated C# should include data annotations support",
				output.contains("using System.ComponentModel.DataAnnotations;"));
		assertTrue("generated C# should include EF Core support",
				output.contains("using Microsoft.EntityFrameworkCore;"));
		assertTrue("generated C# should include table mapping annotations", output.contains("[Table(\"IdentifiedObject\")]"));
		assertTrue("generated C# should emit the IdentifiedObject class", output.contains("public class IdentifiedObject"));
		assertTrue("generated C# should mark IdentifiedObject.mRID as the key",
				output.contains("[Key]\r\n        [Column(\"mRID\")]")
						|| output.contains("[Key]\n        [Column(\"mRID\")]"));
		assertTrue("generated C# should emit the MRId property",
				output.contains("public string MRId { get; set; } = null!;"));
		assertTrue("generated C# should emit Organisation as an IdentifiedObject subclass",
				output.contains("public class Organisation : IdentifiedObject"));
		assertTrue("generated C# should emit a surrogate-key compound ElectronicAddress class",
				output.contains("public class ElectronicAddress"));
		assertTrue("generated C# should mark compound ids explicitly",
				output.contains("[Key]\r\n        [Column(\"id\")]")
						|| output.contains("[Key]\n        [Column(\"id\")]"));
		assertTrue("generated C# should assign surrogate ids in compound constructors",
				output.contains("public ElectronicAddress() { Id = System.Guid.NewGuid().ToString(); }"));
		assertTrue("generated C# should emit the Organisation ElectronicAddress foreign-key property",
				output.contains("public string? ElectronicAddressId { get; set; }"));
		assertTrue("generated C# should configure the Organisation ElectronicAddress relationship",
				output.contains("HasForeignKey(x => x.ElectronicAddressId)"));
		assertTrue("generated C# should emit runtime-type-aware equality for root entities",
				output.contains("if (GetType() != other.GetType()) return false;"));
		assertTrue("generated C# should emit hash code generation for IdentifiedObject equality",
				output.contains("HashCode.Combine(GetType(), MRId)"));
		assertTrue("generated C# should emit lookup classes keyed by name",
				output.contains("public class CrewStatusKind"));
		assertTrue("generated C# should map lookup names explicitly",
				output.contains("[Column(\"name\")]"));
		assertTrue("generated C# should emit ParentOrganization as a derived Organisation type",
				output.contains("public class ParentOrganization : Organisation"));
		assertTrue("generated C# should include the SampleProfile wrapper class", output.contains("public class SampleProfile"));
		assertTrue("generated C# should include EF model configuration",
				output.contains("ModelConfiguration.ConfigureModel(modelBuilder)"));
		assertTrue("generated C# should configure Organisation in Fluent API",
				output.contains("ConfigureOrganisation(modelBuilder)"));
		assertTrue("generated C# should configure compound cascade delete behavior",
				output.contains("DeleteBehavior.Cascade"));
		assertTrue("generated C# should configure independent entity references as ClientNoAction",
				output.contains("HasForeignKey(x => x.ParentOrganisationId).OnDelete(DeleteBehavior.ClientNoAction)"));
		assertContainsAll(output, "generated C# should include all generated table mappings",
				new String[] { "[Table(\"CrewStatusKind\")]", "[Table(\"PhaseCode\")]", "[Table(\"ShuntImpedanceControlKind\")]",
						"[Table(\"ShuntImpedanceLocalControlKind\")]", "[Table(\"WireInsulationKind\")]",
						"[Table(\"WireMaterialKind\")]", "[Table(\"ElectronicAddress\")]", "[Table(\"Status\")]",
						"[Table(\"StreetDetail\")]", "[Table(\"TelephoneNumber\")]", "[Table(\"TownDetail\")]",
						"[Table(\"StreetAddress\")]", "[Table(\"IdentifiedObject\")]", "[Table(\"ShuntCompensatorControl\")]",
						"[Table(\"AssetInfo\")]", "[Table(\"Organisation\")]", "[Table(\"ParentOrganization\")]",
						"[Table(\"ShuntCompensatorInfo\")]", "[Table(\"WireInfo\")]", "[Table(\"WireSpacingInfo\")]",
						"[Table(\"OverheadWireInfo\")]" });
		assertContainsAll(output, "generated C# should list all generated model types in allClasses",
				new String[] { "typeof(CrewStatusKind)", "typeof(PhaseCode)", "typeof(ShuntImpedanceControlKind)",
						"typeof(ShuntImpedanceLocalControlKind)", "typeof(WireInsulationKind)", "typeof(WireMaterialKind)",
						"typeof(ElectronicAddress)", "typeof(Status)", "typeof(StreetDetail)", "typeof(TelephoneNumber)",
						"typeof(TownDetail)", "typeof(StreetAddress)", "typeof(IdentifiedObject)",
						"typeof(ShuntCompensatorControl)", "typeof(AssetInfo)", "typeof(Organisation)",
						"typeof(ParentOrganization)", "typeof(ShuntCompensatorInfo)", "typeof(WireInfo)",
						"typeof(WireSpacingInfo)", "typeof(OverheadWireInfo)" });
		assertContainsAll(output, "generated C# should configure all expected relationship delete behaviors",
				new String[] {
						"e.HasOne(x => x.Status).WithMany().HasForeignKey(x => x.StatusId).OnDelete(DeleteBehavior.Cascade);",
						"e.HasOne(x => x.StreetDetail).WithMany().HasForeignKey(x => x.StreetDetailId).OnDelete(DeleteBehavior.Cascade);",
						"e.HasOne(x => x.TownDetail).WithMany().HasForeignKey(x => x.TownDetailId).OnDelete(DeleteBehavior.Cascade);",
						"e.HasOne(x => x.ElectronicAddress).WithMany().HasForeignKey(x => x.ElectronicAddressId).OnDelete(DeleteBehavior.Cascade);",
						"e.HasOne(x => x.ParentOrganisation).WithMany().HasForeignKey(x => x.ParentOrganisationId).OnDelete(DeleteBehavior.ClientNoAction);",
						"e.HasOne(x => x.Phone1).WithMany().HasForeignKey(x => x.Phone1Id).OnDelete(DeleteBehavior.Cascade);",
						"e.HasOne(x => x.Phone2).WithMany().HasForeignKey(x => x.Phone2Id).OnDelete(DeleteBehavior.Cascade);",
						"e.HasOne(x => x.PostalAddress).WithMany().HasForeignKey(x => x.PostalAddressId).OnDelete(DeleteBehavior.Cascade);",
						"e.HasOne(x => x.StreetAddress).WithMany().HasForeignKey(x => x.StreetAddressId).OnDelete(DeleteBehavior.Cascade);",
						"e.HasOne(x => x.ShuntCompensatorControl).WithMany().HasForeignKey(x => x.ShuntCompensatorControlId).OnDelete(DeleteBehavior.ClientNoAction);" });
		assertMatchesRegressionFixture(output);
	}

	private void assertContainsAll(String output, String message, String[] expectedFragments) {
		for (String fragment : expectedFragments) {
			assertTrue(message + ": missing fragment: " + fragment, output.contains(fragment));
		}
	}

	private void assertMatchesRegressionFixture(String output) throws IOException {
		String fixture = loadFixtureText(EXPECTATIONS_FILE);
		String normalisedOutput = normalizeLineEndings(output);
		String[] fragments = fixture.split("\\R====\\R");

		for (int i = 0; i < fragments.length; i++) {
			String fragment = normalizeLineEndings(fragments[i]).trim();
			if (!fragment.isEmpty()) {
				assertTrue("generated C# should contain regression fixture fragment #" + (i + 1),
						normalisedOutput.contains(fragment));
			}
		}
	}

	private String loadFixtureText(String fileName) throws IOException {
		try (InputStream is = Thread.currentThread().getContextClassLoader()
				.getResourceAsStream("CIMToolTestFiles/" + fileName)) {
			assertNotNull("Fixture resource should be available on the test classpath: " + fileName, is);
			return new String(is.readAllBytes(), StandardCharsets.UTF_8);
		}
	}

	private String normalizeLineEndings(String text) {
		return text.replace("\r\n", "\n");
	}
}
