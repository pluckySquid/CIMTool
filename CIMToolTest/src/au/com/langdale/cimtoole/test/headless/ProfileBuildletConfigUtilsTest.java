package au.com.langdale.cimtoole.test.headless;

import java.io.File;
import java.io.FileOutputStream;
import java.io.InputStream;
import java.net.URL;
import java.nio.charset.StandardCharsets;

import org.apache.commons.io.IOUtils;
import org.eclipse.core.runtime.FileLocator;
import org.eclipse.core.runtime.Platform;
import org.eclipse.osgi.service.datalocation.Location;
import org.osgi.framework.Bundle;

import au.com.langdale.cimtoole.CIMToolPlugin;
import au.com.langdale.cimtoole.builder.ProfileBuildlets.TransformBuildlet;
import au.com.langdale.cimtoole.registries.ProfileBuildletConfigUtils;
import au.com.langdale.cimtoole.test.WorkspaceTest;

public class ProfileBuildletConfigUtilsTest extends WorkspaceTest {

	private static final String CONFIG_DIR = "builders";
	private static final String CONFIG_DEFAULTS_FILE = "builders.json";
	private static final String CONFIG_FILE = ".builders";
	private static final String JUNIT_TEST_XSL_FILE = "junit-test.xsl";
	private static final String JUNIT_TEST2_XSL_FILE = "junit-test2.xsl";
	private static final String CSHARP_EF_RDFS_XSL_FILE = "csharp-ef-rdfs.xsl";
	private static final String SCHEMA_JSON_DRAFT_07_XSL_FILE = "schema-json-draft-07.xsl";

	private File csharpEfRdfsXslFile;
	private File schemaJsonDraft07XslFile;

	private File dataAreaDir;
	private File dataAreaBuilderConfigFile;

	protected void setUp() throws Exception {
		super.setUp();
		Location configLocation = Platform.getConfigurationLocation();
		URL dataArea = configLocation.getDataArea(CIMToolPlugin.PLUGIN_ID);

		dataAreaDir = new File(dataArea.getPath());
		dataAreaDir = new File(dataAreaDir.getPath(), CONFIG_DIR);
		dataAreaBuilderConfigFile = new File(dataAreaDir, CONFIG_FILE);

		Bundle cimtooleBundle = Platform.getBundle(CIMToolPlugin.PLUGIN_ID);
		URL csharpEfRdfsXslURL = cimtooleBundle.getEntry(CONFIG_DIR + "/" + CSHARP_EF_RDFS_XSL_FILE);
		URL csharpEfRdfsXslFileURL = FileLocator.toFileURL(csharpEfRdfsXslURL);
		csharpEfRdfsXslFile = new File(csharpEfRdfsXslFileURL.toURI());

		URL schemaJsonDraft07XslURL = cimtooleBundle.getEntry(CONFIG_DIR + "/" + SCHEMA_JSON_DRAFT_07_XSL_FILE);
		URL schemaJsonDraft07XslFileURL = FileLocator.toFileURL(schemaJsonDraft07XslURL);
		schemaJsonDraft07XslFile = new File(schemaJsonDraft07XslFileURL.toURI());
	}

	@Override
	protected void tearDown() throws Exception {
		super.tearDown();
		if (Platform.isRunning()) {
			Location configLocation = Platform.getConfigurationLocation();
			URL dataArea = configLocation.getDataArea(CIMToolPlugin.PLUGIN_ID);

			File dataAreaDir = new File(dataArea.getPath());
			dataAreaDir = new File(dataAreaDir.getPath(), CONFIG_DIR);

			File dataAreaBuilderConfigFile = new File(dataAreaDir, CONFIG_FILE);
			if (dataAreaBuilderConfigFile.exists()) {
				dataAreaBuilderConfigFile.delete();
			}

			File dataAreaXslConfigFile = new File(dataAreaDir, JUNIT_TEST_XSL_FILE);
			if (dataAreaXslConfigFile.exists()) {
				dataAreaXslConfigFile.delete();
			}

			File dataAreaXslConfigFile2 = new File(dataAreaDir, JUNIT_TEST2_XSL_FILE);
			if (dataAreaXslConfigFile2.exists()) {
				dataAreaXslConfigFile2.delete();
			}
		}
	}

	public final void testGetCustomBuilders() throws Exception {

		ProfileBuildletConfigUtils.getTransformBuildlets();

		assertTrue("The " + CONFIG_FILE + " config file was not initialized", dataAreaBuilderConfigFile.exists());
		assertTrue(SCHEMA_JSON_DRAFT_07_XSL_FILE + " was not copied into the new config directory",
				schemaJsonDraft07XslFile.exists());
	}

	public final void testCSharpEfRdfsBuilderIsAvailable() throws Exception {

		TransformBuildlet buildlet = ProfileBuildletConfigUtils.getTransformBuildlet("csharp-ef-rdfs");
		TransformBuildlet buildletByExtension = ProfileBuildletConfigUtils
				.getTransformBuildletForExtension("csharp-ef-rdfs.cs");
		InputStream buildletInputStream = ProfileBuildletConfigUtils.getTransformBuildletInputStream("csharp-ef-rdfs");

		assertNotNull("csharp-ef-rdfs builder should be registered", buildlet);
		assertNotNull("csharp-ef-rdfs builder should be discoverable by extension", buildletByExtension);
		assertNotNull("csharp-ef-rdfs builder should provide an XSLT input stream", buildletInputStream);
		assertEquals("csharp-ef-rdfs.cs", buildlet.getFileExt());
		assertEquals("csharp-ef-rdfs", buildletByExtension.getStyle());
		try (InputStream is = buildletInputStream) {
			String xslt = IOUtils.toString(is, StandardCharsets.UTF_8);
			assertTrue("csharp-ef-rdfs builder should provide XSLT content", !xslt.isEmpty());
			assertTrue("csharp-ef-rdfs XSLT should be a stylesheet", xslt.contains("<xsl:stylesheet"));
			assertTrue("csharp-ef-rdfs XSLT should contain EF Core cascade-delete guidance",
					xslt.contains("DeleteBehavior.Cascade"));
		}
		assertTrue(CSHARP_EF_RDFS_XSL_FILE + " should exist in the shipped builders bundle",
				csharpEfRdfsXslFile.exists());
	}

	public final void testRemoveCustomBuilderConfigEntry() throws Exception {

		String builderKey = JUNIT_TEST_XSL_FILE.substring(0, JUNIT_TEST_XSL_FILE.indexOf("."));

		initializeJUnitBuildersConfiguration();

		Location configLocation = Platform.getConfigurationLocation();
		URL dataArea = configLocation.getDataArea(CIMToolPlugin.PLUGIN_ID);
		File dataAreaDir = new File(dataArea.getPath());
		dataAreaDir = new File(dataAreaDir.getPath(), CONFIG_DIR);

		File dataAreaBuilderConfigFile = new File(dataAreaDir, CONFIG_FILE);
		File dataAreaXslFile = new File(dataAreaDir, JUNIT_TEST_XSL_FILE);

		// Simply ensure that test initialization was successful...
		assertTrue(dataAreaBuilderConfigFile.exists());
		assertTrue(dataAreaXslFile.exists());

		// Now delete the configuration entry...
		ProfileBuildletConfigUtils.deleteTransformBuilderConfigEntry(new TransformBuildlet(builderKey, "ext"));

		assertTrue("Builder " + builderKey + " was not deleted", !ProfileBuildletConfigUtils.hasBuildlet(builderKey));
		assertTrue("XSLT file " + JUNIT_TEST_XSL_FILE + " was not deleted", !dataAreaXslFile.exists());
	}

	public final void testAddCustomBuilderConfigEntry() throws Exception {

		String builderKey = JUNIT_TEST2_XSL_FILE.substring(0, JUNIT_TEST2_XSL_FILE.indexOf("."));

		initializeJUnitBuildersConfiguration();

		Location configLocation = Platform.getConfigurationLocation();
		URL dataArea = configLocation.getDataArea(CIMToolPlugin.PLUGIN_ID);
		File dataAreaDir = new File(dataArea.getPath());
		dataAreaDir = new File(dataAreaDir.getPath(), CONFIG_DIR);

		File dataAreaBuilderConfigFile = new File(dataAreaDir, CONFIG_FILE);
		File dataAreaXslFile = new File(dataAreaDir, JUNIT_TEST_XSL_FILE);
		File dataAreaXslFile2 = new File(dataAreaDir, JUNIT_TEST2_XSL_FILE);

		// Simply ensure that test initialization was successful...
		assertTrue(dataAreaBuilderConfigFile.exists());
		assertTrue(dataAreaXslFile.exists());
		assertTrue(!dataAreaXslFile2.exists());

		// Now delete the configuration entry...
		TransformBuildlet buildlet = new TransformBuildlet(builderKey, "junit-ext2");

		File xslFile = new File(CONFIG_DIR + "/" + JUNIT_TEST2_XSL_FILE);
		ProfileBuildletConfigUtils.addTransformBuilderConfigEntry(buildlet, xslFile, null);

		assertTrue("Builder " + builderKey + " was not added", ProfileBuildletConfigUtils.hasBuildlet(builderKey));
		assertTrue("XSLT file " + JUNIT_TEST2_XSL_FILE + " was not added", dataAreaXslFile2.exists());
	}

	private void initializeJUnitBuildersConfiguration() {
		try {
			if (Platform.isRunning()) {
				Location configLocation = Platform.getConfigurationLocation();
				URL dataArea = configLocation.getDataArea(CIMToolPlugin.PLUGIN_ID);

				File dataAreaDir = new File(dataArea.getPath());
				dataAreaDir = new File(dataAreaDir.getPath(), CONFIG_DIR);

				File dataAreaBuilderConfigFile = new File(dataAreaDir, CONFIG_FILE);

				if (dataAreaBuilderConfigFile.exists()) {
					dataAreaBuilderConfigFile.delete();
				}

				if (!dataAreaDir.exists())
					dataAreaDir.mkdirs();

				if (dataAreaDir.exists() && !dataAreaBuilderConfigFile.exists()) {
					dataAreaBuilderConfigFile.createNewFile();

					InputStream is = null;

					try {
						is = Thread.currentThread().getContextClassLoader()
								.getResourceAsStream(CONFIG_DIR + "/" + CONFIG_DEFAULTS_FILE);
						IOUtils.copy(is, new FileOutputStream(dataAreaBuilderConfigFile));
					} finally {
						if (is != null) {
							try {
								is.close();
							} catch (Exception e) {
								// Do nothing
							}
						}
					}

					if (dataAreaBuilderConfigFile.exists()) {
						File dataAreaXslFile = new File(dataAreaDir, JUNIT_TEST_XSL_FILE);
						dataAreaXslFile.createNewFile();

						is = null;
						try {
							is = Thread.currentThread().getContextClassLoader()
									.getResourceAsStream(CONFIG_DIR + "/" + JUNIT_TEST_XSL_FILE);
							IOUtils.copy(is, new FileOutputStream(dataAreaXslFile));
						} finally {
							if (is != null) {
								try {
									is.close();
								} catch (Exception e) {
									// Do nothing
								}
							}
						}
					}
				}

			}
		} catch (Exception e) {
			// We log the exception, fail gracefully and proceed...
			e.printStackTrace();
			throw new RuntimeException(e);
		}
	}

}
