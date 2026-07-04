#!/usr/bin/env bash
# Regenerates SampleProfile.jpa-rdfs.java and SampleProfile.rdfs-ansi92.sql from
# SampleProfile.xml (CIMTool's serialized profile) using this repository's
# builders, without needing the CIMTool GUI. Requires: mvn, and a
# SampleProfile.xml generated once by CIMTool's "xml" builder (see README.md).
set -euo pipefail
cd "$(dirname "$0")"
PROFILES="../CSharpEFTestProject/CSharpEFTestProject/Profiles"
BUILDERS="../CIMUtil/builders"
XML="$PROFILES/SampleProfile.xml"
if [ ! -f "$XML" ]; then
  echo "ERROR: $XML not found."
  echo "Generate it once in CIMTool by enabling the 'xml' builder on Profiles/SampleProfile.owl. See README.md."
  exit 1
fi

SAXON="target/tools/saxon-he.jar"
if [ ! -f "$SAXON" ]; then
  mvn -q org.apache.maven.plugins:maven-dependency-plugin:3.8.1:copy \
    -Dartifact=net.sf.saxon:Saxon-HE:12.5 \
    -DoutputDirectory=target/tools -Dmdep.stripVersion=true
  mv target/tools/Saxon-HE.jar "$SAXON"
fi

run_builder() { # $1=stylesheet $2=output-file
  java -cp "$SAXON" net.sf.saxon.Transform \
    -s:"$XML" -xsl:"$BUILDERS/$1" -o:"$PROFILES/$2" \
    "envelope=SampleProfile" "baseURI=http://www.ucaiug.org/profile#" "version=CIMTool"
  echo "generated $PROFILES/$2"
}
run_builder jpa-rdfs.xsl        SampleProfile.jpa-rdfs.java
run_builder sql-rdfs-ansi92.xsl SampleProfile.rdfs-ansi92.sql

# The builders emit an Indent-namespace XML document that CIMTool renders to
# plain text. If the outputs above still start with '<?xml', the text renderer
# step is missing — compare against a CIMTool-GUI-generated file and extend this
# script before trusting the output.
for f in SampleProfile.jpa-rdfs.java SampleProfile.rdfs-ansi92.sql; do
  if head -c 5 "$PROFILES/$f" | grep -q '<?xml'; then
    echo "WARNING: $f is Indent XML, not rendered text. Do not use until this script renders it (see comment above)."
  fi
done
