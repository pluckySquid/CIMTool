#!/usr/bin/env bash
# Runs the repository's jpa-rdfs and sql-rdfs-ansi92 builders over SampleProfile.xml
# with Saxon, for inspecting builder output without the CIMTool GUI.
#
# This CANNOT regenerate the committed fixtures. The XSLTs emit CIMTool's
# Indent-namespace XML; rendering that to plain text happens inside CIMTool and
# has no standalone entry point in this repository. Output therefore goes to a
# scratch directory and the committed fixtures are never touched.
set -euo pipefail
cd "$(dirname "$0")"
PROFILES="../CSharpEFTestProject/CSharpEFTestProject/Profiles"
BUILDERS="../CIMUtil/builders"
XML="$PROFILES/SampleProfile.xml"
OUT="target/builder-output"

if [ ! -f "$XML" ]; then
  echo "ERROR: $XML not found. See README.md." >&2
  exit 1
fi
mkdir -p "$OUT" target/tools

SAXON="target/tools/saxon-he.jar"
RESOLVER="target/tools/xmlresolver.jar"
if [ ! -f "$SAXON" ]; then
  mvn -q org.apache.maven.plugins:maven-dependency-plugin:3.8.1:copy \
    -Dartifact=net.sf.saxon:Saxon-HE:12.5 \
    -DoutputDirectory=target/tools -Dmdep.stripVersion=true
  mv target/tools/Saxon-HE.jar "$SAXON"
fi
# Saxon-HE 12.x needs xmlresolver on the classpath; dependency:copy does not
# bring transitive dependencies along.
if [ ! -f "$RESOLVER" ]; then
  mvn -q org.apache.maven.plugins:maven-dependency-plugin:3.8.1:copy \
    -Dartifact=org.xmlresolver:xmlresolver:5.2.2 \
    -DoutputDirectory=target/tools -Dmdep.stripVersion=true
  mv target/tools/xmlresolver.jar "$RESOLVER" 2>/dev/null || true
fi

run_builder() { # $1=stylesheet $2=output-file
  java -cp "$SAXON:$RESOLVER" net.sf.saxon.Transform \
    -s:"$XML" -xsl:"$BUILDERS/$1" -o:"$OUT/$2" \
    "envelope=SampleProfile" "baseURI=http://www.ucaiug.org/profile#" "version=CIMTool"
  echo "wrote $OUT/$2"
}
run_builder jpa-rdfs.xsl        SampleProfile.jpa-rdfs.indent.xml
run_builder sql-rdfs-ansi92.xsl SampleProfile.rdfs-ansi92.indent.xml

echo
echo "These are Indent-namespace XML documents, not the rendered Java/SQL."
echo "Compare them against the committed fixtures if you are changing a builder;"
echo "regenerating the fixtures themselves still requires CIMTool."
