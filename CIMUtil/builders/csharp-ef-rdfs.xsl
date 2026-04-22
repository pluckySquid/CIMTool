<?xml version="1.0" encoding="UTF-8"?>
<!--
  Copyright 2026 UCAIug

  Licensed under the Apache License, Version 2.0 (the "License");
  you may not use this file except in compliance with the License.
  You may obtain a copy of the License at

  https://www.apache.org/licenses/LICENSE-2.0

  Unless required by applicable law or agreed to in writing, software
  distributed under the License is distributed on an "AS IS" BASIS,
  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.

  See the License for the specific language governing permissions and
  limitations under the License.
-->
<xsl:stylesheet version="3.0"
    xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
    xmlns:xs="http://www.w3.org/2001/XMLSchema"
    xmlns:a="http://langdale.com.au/2005/Message#"
    xmlns:sawsdl="http://www.w3.org/ns/sawsdl"
    xmlns:map="http://www.w3.org/2005/xpath-functions/map"
    xmlns:fn="http://www.w3.org/2005/xpath-functions"
    xmlns:cimtool="http://cimtool.ucaiug.io/functions"
    xmlns="http://langdale.com.au/2009/Indent">

    <!--
       Revised CIMTOOL C# Class Generator with Properties
       This XSLT generates class declarations (names) along with:
         - A default (parameterless) constructor
         - A ToString() override
         - Auto-implemented properties for attributes and associations
       Licensed under the Apache License, Version 2.0.
    -->

    <xsl:output indent="yes" method="xml" encoding="UTF-8" />

    <!-- Parameters -->
    <xsl:param name="version"/>
    <xsl:param name="baseURI"/>
    <xsl:param name="envelope">Profile</xsl:param>
    <xsl:param name="package">io.ucaiug.cimtool.generated</xsl:param>
    <xsl:param name="mridType">string</xsl:param>

    <!--
    Ã¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢Â
    BEGIN: TYPE MAPPING FUNCTIONS
    Ã¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢Â
    -->

    <!--
        Returns a C# [MaxLength(n)] annotation string for string-like XSD types,
        or an empty sequence for types that carry no MaxLength constraint in EF Core.

        These type lengths should strictly correlate to the length values as
        generated in the SQL DDL script by the sql.xsl builder.

        Parameters:
            $xstype  Ã¢â‚¬â€ The XSD simple type name taken from @xstype on the profile
                       attribute node (e.g. 'string', 'integer', 'dateTime').
            $name    Ã¢â‚¬â€ The attribute or column name taken from @name on the profile
                       attribute node. Used to distinguish the 'mRID' and surrogate
                       compound 'id' columns, which are capped at 100 rather than 255.

        Returns:
            xs:string?  Ã¢â‚¬â€ One of:
                '[MaxLength(100)]'   for mRID and compound surrogate id columns
                '[MaxLength(255)]'   for string, normalizedString, and token types
                '[MaxLength(2048)]'  for anyURI (per practical browser compatibility ceiling)
                ()                   for all non-string types (numeric, boolean, binary,
                                     date/time) which carry no MaxLength annotation in C#

        Notes:
            - The empty sequence return allows the call site to use exists() as a
              clean gate, or simply pass the result to xsl:value-of which produces
              no output for an empty sequence.
            - BLOB types (base64Binary, hexBinary) intentionally return () Ã¢â‚¬â€ EF Core
              maps these to byte[] which carries no MaxLength constraint by default.
            - The boolean type maps to INTEGER 0/1 in SQL and to bool in C#, neither
              of which uses MaxLength.
    -->
    <xsl:function name="cimtool:maxLength" as="xs:string?">
        <xsl:param name="xstype" as="xs:string"/>
        <xsl:param name="name"   as="xs:string"/>

        <xsl:choose>
            <!-- mRID and compound surrogate id are always capped at 100 -->
            <xsl:when test="($xstype = 'string') and ($name = 'mRID' or $name = 'id')">
                <xsl:sequence select="'[MaxLength(100)]'"/>
            </xsl:when>

            <!-- Standard string-like types -->
            <xsl:when test="$xstype = 'string' or $xstype = 'normalizedString' or $xstype = 'token'">
                <xsl:sequence select="'[MaxLength(255)]'"/>
            </xsl:when>

            <!-- URLs Ã¢â‚¬â€ 2048 per practical browser/IE compatibility ceiling -->
            <xsl:when test="$xstype = 'anyURI'">
                <xsl:sequence select="'[MaxLength(2048)]'"/>
            </xsl:when>

            <!-- Non-string types carry no MaxLength annotation:        -->
            <!-- short Ã¢â€ â€™ SMALLINT                                        -->
            <!-- int/integer Ã¢â€ â€™ INTEGER                                   -->
            <!-- long Ã¢â€ â€™ BIGINT                                           -->
            <!-- decimal/float/double Ã¢â€ â€™ DOUBLE PRECISION                 -->
            <!-- base64Binary/hexBinary Ã¢â€ â€™ BLOB                           -->
            <!-- date Ã¢â€ â€™ DATE                                             -->
            <!-- time Ã¢â€ â€™ TIME                                             -->
            <!-- dateTime Ã¢â€ â€™ TIMESTAMP                                    -->
            <!-- boolean Ã¢â€ â€™ INTEGER 0/1                                   -->
            <xsl:otherwise>
                <xsl:sequence select="()"/>
            </xsl:otherwise>
        </xsl:choose>
    </xsl:function>

    <!--
        Returns the C# primitive or framework type name corresponding to a given
        XSD simple type, for use in generating EF Core entity property declarations.

        Parameters:
            $xstype  Ã¢â‚¬â€ The XSD simple type name taken from @xstype on the profile
                       attribute node (e.g. 'string', 'integer', 'dateTime').

        Returns:
            xs:string  Ã¢â‚¬â€ The C# type name. One of:
                           'string'    for string, normalizedString, token, anyURI
                           'short'     for short
                           'int'       for integer, int
                           'long'      for long
                           'double'    for decimal, float, double
                           'byte[]'    for base64Binary, hexBinary
                           'DateOnly'  for date
                           'TimeOnly'  for time
                           'DateTime'  for dateTime
                           'bool'      for boolean
                           'string'    for all unrecognised types (safe fallback)

        Notes:
            - decimal and float are both mapped to 'double' rather than 'decimal'
              to align with the DOUBLE PRECISION mapping used in the parallel SQL
              DDL generator. If exact decimal arithmetic is required for a specific
              profile, the caller should override accordingly.
            - DateOnly and TimeOnly require .NET 6 or later. If earlier .NET
              versions must be supported, these should be mapped to DateTime.
            - byte[] carries no MaxLength annotation Ã¢â‚¬â€ cimtool:maxLength() correctly
              returns () for base64Binary and hexBinary types.
            - anyURI is mapped to string as there is no native C# Uri property
              type that EF Core maps cleanly across all five target RDBMS backends.
            - Unrecognised types fall back to string, matching the behaviour of
              the parallel SQL DDL generator which falls back to VARCHAR(255).
    -->
    <xsl:function name="cimtool:csType" as="xs:string">
        <xsl:param name="xstype" as="xs:string"/>

        <xsl:choose>
            <!-- String-like types -->
            <xsl:when test="$xstype = 'string' or $xstype = 'normalizedString' or $xstype = 'token' or $xstype = 'anyURI'">
                <xsl:sequence select="'string'"/>
            </xsl:when>

            <!-- Integer types -->
            <xsl:when test="$xstype = 'short'">
                <xsl:sequence select="'short'"/>
            </xsl:when>
            <xsl:when test="$xstype = 'integer' or $xstype = 'int'">
                <xsl:sequence select="'int'"/>
            </xsl:when>
            <xsl:when test="$xstype = 'long'">
                <xsl:sequence select="'long'"/>
            </xsl:when>

            <!-- Floating point types Ã¢â‚¬â€ all map to double to align with
                 DOUBLE PRECISION in the parallel SQL DDL generator -->
            <xsl:when test="$xstype = 'decimal' or $xstype = 'float' or $xstype = 'double'">
                <xsl:sequence select="'double'"/>
            </xsl:when>

            <!-- Binary types -->
            <xsl:when test="$xstype = 'base64Binary' or $xstype = 'hexBinary'">
                <xsl:sequence select="'byte[]'"/>
            </xsl:when>

            <!-- Date and time types Ã¢â‚¬â€ require .NET 6+ -->
            <xsl:when test="$xstype = 'date'">
                <xsl:sequence select="'DateOnly'"/>
            </xsl:when>
            <xsl:when test="$xstype = 'time'">
                <xsl:sequence select="'TimeOnly'"/>
            </xsl:when>
            <xsl:when test="$xstype = 'dateTime'">
                <xsl:sequence select="'DateTime'"/>
            </xsl:when>

            <!-- Boolean Ã¢â‚¬â€ maps to INTEGER 0/1 in SQL, bool in C# -->
            <xsl:when test="$xstype = 'boolean'">
                <xsl:sequence select="'bool'"/>
            </xsl:when>

            <!-- Safe fallback Ã¢â‚¬â€ mirrors VARCHAR(255) fallback in SQL DDL generator -->
            <xsl:otherwise>
                <xsl:sequence select="'string'"/>
            </xsl:otherwise>
        </xsl:choose>
    </xsl:function>

    <!--
        Returns a capitalized version of the given name string, with any
        hyphens replaced by underscores prior to capitalisation.

        Parameters:
            $name  Ã¢â‚¬â€ The name string to capitalize, typically taken from @name
                     on a profile element node.

        Returns:
            xs:string  Ã¢â‚¬â€ The input string with hyphens replaced by underscores
                         and the first character converted to uppercase.

        Notes:
            - The lowercase and uppercase alphabet variables ($lc, $uc) are
              intentionally scoped inside this function as they are only used
              here. The translate() approach is used in preference to the
              XPath 3.0 upper-case() function to keep capitalisation behaviour
              explicit and locale-independent.
            - Only the first character is capitalized Ã¢â‚¬â€ the remainder of the
              string is preserved exactly as supplied.
            - This function is naturally immune to C# keyword conflicts because
              it always produces a result beginning with an uppercase letter,
              and every C# reserved and contextual keyword begins with a
              lowercase letter. cimtool:safeIdentifier() is therefore not needed
              for any call site that uses this function.
    -->
    <xsl:function name="cimtool:capitalize" as="xs:string">
        <xsl:param name="name" as="xs:string"/>

        <xsl:variable name="lc">abcdefghijklmnopqrstuvwxyz</xsl:variable>
        <xsl:variable name="uc">ABCDEFGHIJKLMNOPQRSTUVWXYZ</xsl:variable>
        <xsl:variable name="clean" select="translate($name, '-', '_')"/>

        <!-- Special case: 'mRID' must capitalize to 'MRId' (lowercase 'd'), not
             the generic 'MRID' that the first-char-uppercase rule would produce.
             This matches the IEC CIM convention where the identifier is 'mRID'
             (master Resource IDentifier) with a lowercase terminal 'd'. -->
        <xsl:choose>
            <xsl:when test="$clean = 'mRID'">
                <xsl:sequence select="'MRId'"/>
            </xsl:when>
            <xsl:otherwise>
                <xsl:sequence select="concat(
                    translate(substring($clean, 1, 1), $lc, $uc),
                    substring($clean, 2)
                )"/>
            </xsl:otherwise>
        </xsl:choose>
    </xsl:function>
    
	<!--
	    Returns a safe C# identifier from the given name string by:
	      1. Replacing hyphens with underscores (hyphens are invalid in C# identifiers)
	      2. Prefixing with '_' if the result begins with a digit (C# identifiers
	         cannot start with a digit, e.g. a hypothetical enum value '3phase')
	      3. Prefixing with '@' if the result matches a C# reserved or contextual
	         keyword (e.g. 'base', 'default', 'in', 'value', 'get', 'set')
	
	    This function is intentionally case-preserving Ã¢â‚¬â€ it does NOT capitalize the
	    first character. Use cimtool:capitalize() for PascalCase property names.
	    Use this function directly for enum literal identifiers where case must be
	    preserved to avoid collisions between values that differ only in case
	    (e.g. UnitSymbol 'H' henry vs 'h' hour).
	
	    Parameters:
	        $name  Ã¢â‚¬â€ The raw name string, typically taken from @name on a profile
	                 element node (e.g. an a:EnumeratedValue).
	
	    Returns:
	        xs:string  Ã¢â‚¬â€ A valid C# identifier. Examples:
	                       'H'          Ã¢â€ â€™ 'H'
	                       'h'          Ã¢â€ â€™ 'h'
	                       'some-value' Ã¢â€ â€™ 'some_value'
	                       '3phase'     Ã¢â€ â€™ '_3phase'
	                       'base'       Ã¢â€ â€™ '@base'
	                       'default'    Ã¢â€ â€™ '@default'
	                       'value'      Ã¢â€ â€™ '@value'
	
	    Notes:
	        - Covers all C# reserved keywords (C# specification section 6.4.4) and
	          all contextual keywords (section 6.4.4.1) since contextual keywords
	          can still conflict in certain syntactic positions.
	        - Digit-leading values are prefixed with '_' rather than '@' since '@'
	          is only valid for keyword escaping in C# Ã¢â‚¬â€ '@3phase' is not legal.
	        - cimtool:capitalize() is naturally safe against both issues since it
	          always produces a PascalCase result beginning with an uppercase letter,
	          and all C# keywords are lowercase. This function is therefore only
	          needed for case-preserving enum literal generation.
	-->
	<xsl:function name="cimtool:safeIdentifier" as="xs:string">
	    <xsl:param name="name" as="xs:string"/>
	
	    <xsl:variable name="clean" select="translate($name, '-', '_')"/>
	
	    <!-- C# reserved keywords Ã¢â‚¬â€ C# specification section 6.4.4 -->
	    <xsl:variable name="reserved" as="xs:string+" select="(
	        'abstract', 'as',       'base',      'bool',      'break',
	        'byte',     'case',     'catch',     'char',      'checked',
	        'class',    'const',    'continue',  'decimal',   'default',
	        'delegate', 'do',       'double',    'else',      'enum',
	        'event',    'explicit', 'extern',    'false',     'finally',
	        'fixed',    'float',    'for',       'foreach',   'goto',
	        'if',       'implicit', 'in',        'int',       'interface',
	        'internal', 'is',       'lock',      'long',      'namespace',
	        'new',      'null',     'object',    'operator',  'out',
	        'override', 'params',   'private',   'protected', 'public',
	        'readonly', 'ref',      'return',    'sbyte',     'sealed',
	        'short',    'sizeof',   'stackalloc','static',    'string',
	        'struct',   'switch',   'this',      'throw',     'true',
	        'try',      'typeof',   'uint',      'ulong',     'unchecked',
	        'unsafe',   'ushort',   'using',     'virtual',   'void',
	        'volatile', 'while'
	    )"/>
	
	    <!-- C# contextual keywords Ã¢â‚¬â€ section 6.4.4.1 -->
	    <xsl:variable name="contextual" as="xs:string+" select="(
	        'add',       'alias',     'ascending', 'async',     'await',
	        'by',        'descending','dynamic',   'equals',    'from',
	        'get',       'global',    'group',     'into',      'join',
	        'let',       'nameof',    'on',        'orderby',   'partial',
	        'remove',    'select',    'set',       'unmanaged', 'value',
	        'var',       'when',      'where',     'with',      'yield'
	    )"/>
	
	    <xsl:sequence select="
	        if (matches($clean, '^[0-9]'))
	        then concat('_', $clean)
	        else if ($clean = $reserved or $clean = $contextual)
	        then concat('@', $clean)
	        else $clean
	    "/>
	</xsl:function>

    <!--
        Returns a safe C# property name for a given profile attribute, guarding
        against two distinct collision classes that cimtool:capitalize() alone
        cannot detect:

        Collision class 1 Ã¢â‚¬â€ property name matches a sibling class name.
            Example: a:Enumerated @name='curveStyle' on class Curve capitalizes
            to 'CurveStyle', which is also a sibling EnumeratedType class name.
            In a nested-class context, the unqualified name resolves to the type,
            making 'public string CurveStyle' ambiguous or a compile error
            depending on usage. Similarly 'sVCControlMode' Ã¢â€ â€™ 'SVCControlMode'
            collides with the SVCControlMode EnumeratedType class.

        Collision class 2 Ã¢â‚¬â€ property name matches its own C# type name.
            Example: a:Simple @name='dateTime' @xstype='dateTime' capitalizes to
            'DateTime' and maps to C# type 'DateTime', producing:
                public DateTime? DateTime { get; set; }
            Within the class body, 'DateTime' now resolves to the property rather
            than the System.DateTime type, breaking any unqualified type references.

        In both cases the fix is to append 'Value' to the property name. This is
        the conventional CIM disambiguation suffix (cf. 'value1', 'value2' in
        BasicIntervalSchedule) and does not affect the [Column("...")] annotation,
        which preserves the original database column name unchanged.

        Parameters:
            $name    Ã¢â‚¬â€ The raw attribute name from @name on the profile element.
            $xstype  Ã¢â‚¬â€ The XSD type string from @xstype. Pass the empty string ''
                       for a:Enumerated properties which carry no @xstype.
            $context Ã¢â‚¬â€ Any element node within the current document, used to reach
                       the document root for the catalog class-name lookup.

        Returns:
            xs:string Ã¢â‚¬â€ A collision-free PascalCase C# property name.

        Notes:
            - cimtool:capitalize() is called first, so the mRID Ã¢â€ â€™ MRId special
              case is already applied before collision checking.
            - Navigation property names produced by a:Instance / a:Reference /
              a:Compound are intentionally NOT routed through this function.
              'public BusNameMarker? BusNameMarker' (property same name as its
              type) is legal C# and is the EF Core idiomatic pattern.
            - Collision detection checks the full catalog, not just the enclosing
              class, because all generated classes share the same outer namespace.
    -->
    <xsl:function name="cimtool:safePropertyName" as="xs:string">
        <xsl:param name="name"    as="xs:string"/>
        <xsl:param name="xstype"  as="xs:string"/>
        <xsl:param name="context" as="element()"/>

        <xsl:variable name="base" select="cimtool:capitalize($name)"/>

        <!-- Collision 1: base name matches any catalog class name -->
        <xsl:variable name="classCollision" as="xs:boolean" select="
            exists(root($context)//(a:EnumeratedType|a:CompoundType|a:ComplexType|a:Root)
                   [@name = $base])
        "/>

        <!-- Collision 2: base name matches the C# type of this attribute -->
        <xsl:variable name="typeCollision" as="xs:boolean" select="
            $xstype != '' and $base = cimtool:csType($xstype)
        "/>

        <xsl:sequence select="
            if ($classCollision or $typeCollision) then concat($base, 'Value') else $base
        "/>
    </xsl:function>

    <!--
        Returns a simple pluralized DbSet property name for a generated entity type.

        The generated DbContextBase uses these names so consumers get a predictable
        property surface (e.g. Organisations, TelephoneNumbers, StreetAddresses).
        This is intentionally heuristic rather than dictionary-driven; the goal is
        stable generated names, not linguistic perfection.
    -->
    <xsl:function name="cimtool:pluralize" as="xs:string">
        <xsl:param name="name" as="xs:string"/>
        <xsl:sequence select="
            if (matches($name, '(s|ss|sh|ch|x|z)$')) then concat($name, 'es')
            else if (matches($name, '[^AEIOUaeiou]y$')) then concat(substring($name, 1, string-length($name) - 1), 'ies')
            else concat($name, 's')
        "/>
    </xsl:function>

    <!--
        Returns true if the given CIM profile element, or any ancestor reached by
        walking the a:SuperType chain, directly declares an attribute named 'mRID'.
        This is the definitive programmatic test for membership in the
        IdentifiedObject hierarchy within the profile catalog XML.

        BACKGROUND:
        
        In the CIM, IdentifiedObject is the universal root class for all persistent,
        independently-addressable objects. It carries the 'mRID' (master resource
        identifier) attribute. In a CIMTool profile, an entity in this hierarchy 
        will either:
        
          a) declare 'mRID' directly as an a:Simple child (if it IS IdentifiedObject
             or a profile includes mRID explicitly), or
          b) inherit it transitively through the a:SuperType chain.

        A small but important set of CIM classes do NOT inherit from IdentifiedObject.
        These are typically collection-member or point-data classes, for example:
          - CurveData               (owned by Curve)
          - RegularTimePoint        (owned by RegularIntervalSchedule)
          - NonlinearShuntCompensatorPoint  (owned by NonlinearShuntCompensator)
          - TapChangerTablePoint    (base class for PhaseTapChangerTablePoint,
                                     RatioTapChangerTablePoint)

        These classes have no natural single-column primary key derivable from the
        CIM model. The generators handle them with a surrogate 'id' PK (identical
        to the CompoundType pattern) plus a heuristic UNIQUE constraint across all
        required non-surrogate columns as a guard against semantically corrupt
        duplicate rows. See the a:ComplexType|a:Root template for the full
        implementation.

        This function is structurally similar to cimtool:inherits-from but checks
        for the presence of an mRID attribute rather than a specific @baseClass URI.
        A dedicated function is used rather than extending cimtool:inherits-from so
        that each function remains single-purpose and its intent is unambiguous at
        every call site.

        Parameters:
            $element  Ã¢â‚¬â€ The a:ComplexType or a:Root element to examine.

        Returns:
            xs:boolean  Ã¢â‚¬â€ true if mRID is present at this class or any ancestor.
    -->
    <xsl:function name="cimtool:has-mrid-ancestor" as="xs:boolean">
        <xsl:param name="element" as="element()"/>
        <xsl:sequence select="
            if ($element/a:Simple[@name='mRID']) then
                true()
            else if ($element/a:SuperType) then
                let $superBaseClass := string($element/a:SuperType[1]/@baseClass),
                    $parent := (
                        root($element)//a:Root     [@baseClass = $superBaseClass] |
                        root($element)//a:ComplexType[@baseClass = $superBaseClass]
                    )[1]
                return
                    if ($parent) then cimtool:has-mrid-ancestor($parent)
                    else false()
            else
                false()
        "/>
    </xsl:function>

    <!--
    Ã¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢Â   
    BEGIN: TOPOLOGICAL SORT FUNCTIONS
    
    These six functions form a self-contained unit with no dependencies on any. 
    They operate purely on the CIMTool profile catalog XML structure (a:Catalog, 
    a:CompoundType, a:Root, a:ComplexType, a:SuperType, a:Instance, a:Reference, 
    a:Compound) and the XPath 3.1 map: namespace.

    Note: a:Compound children (compound-type references such as electronicAddress,
    phone1, status, streetDetail) are handled by cimtool:get-dependencies via its
    non-Instance/Reference branch - any child element with @baseClass that is not
    a:SuperType, a:Instance, a:Reference, a:InverseInstance, or a:InverseReference
    is treated as a direct dependency. This correctly ensures that e.g. StreetAddress
    is sorted after Status, StreetDetail, and TownDetail.

    Call sequence:
        cimtool:topological-sort
            Ã¢â€â€Ã¢â€â‚¬Ã¢â€â‚¬ cimtool:topological-sort-helper
                    Ã¢â€â€Ã¢â€â‚¬Ã¢â€â‚¬ cimtool:build-dependencies-map
                            Ã¢â€â€Ã¢â€â‚¬Ã¢â€â‚¬ cimtool:get-dependencies
                                    Ã¢â€Å“Ã¢â€â‚¬Ã¢â€â‚¬ cimtool:get-union-dependencies
                                    Ã¢â€â€š       Ã¢â€â€Ã¢â€â‚¬Ã¢â€â‚¬ cimtool:inherits-from
                                    Ã¢â€â€Ã¢â€â‚¬Ã¢â€â‚¬ cimtool:inherits-from
        cimtool:create-exclusions-map   (used to seed the exclusion map passed
                                        into build-dependencies-map)
    Ã¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢Â
    -->

    <!--
        Function: cimtool:inherits-from
        Purpose: Checks if an element inherits from a given baseClass by
                 recursively walking the a:SuperType chain.
        Parameters:
            $element         Ã¢â‚¬â€ The element to check (a:Root or a:ComplexType)
            $targetBaseClass Ã¢â‚¬â€ The @baseClass URI to search for in the hierarchy
        Returns:
            xs:boolean Ã¢â‚¬â€ true if $element is or inherits from $targetBaseClass
    -->
    <xsl:function name="cimtool:inherits-from" as="xs:boolean">
        <xsl:param name="element"         as="element()"/>
        <xsl:param name="targetBaseClass" as="xs:string"/>

        <xsl:variable name="result" select="
            if ($element/@baseClass = $targetBaseClass) then
                true()
            else if ($element/a:SuperType) then
                let $superBaseClass := $element/a:SuperType/@baseClass,
                    $parent := (
                        root($element)//a:Root[@baseClass = $superBaseClass] |
                        root($element)//a:ComplexType[@baseClass = $superBaseClass]
                    )[1]
                return
                    if ($parent)
                    then cimtool:inherits-from($parent, $targetBaseClass)
                    else false()
            else
                false()
        "/>

        <xsl:sequence select="$result"/>
    </xsl:function>

    <!--
        Function: cimtool:get-union-dependencies
        Purpose: Returns the @baseClass values of all concrete a:Root elements
                 that inherit from a given abstract a:ComplexType. Used to
                 resolve abstract class references to their concrete subtypes
                 when building the dependency graph.
        Parameters:
            $abstract-element Ã¢â‚¬â€ The a:ComplexType element (abstract class)
        Returns:
            xs:string* Ã¢â‚¬â€ Distinct sequence of @baseClass values for all
                         concrete subclasses
    -->
    <xsl:function name="cimtool:get-union-dependencies" as="xs:string*">
        <xsl:param name="abstract-element" as="element()"/>

        <xsl:variable name="abstract-baseClass" select="string($abstract-element/@baseClass)"/>

        <xsl:variable name="result" select="
            distinct-values(
                for $root in root($abstract-element)//a:Root
                return
                    if (cimtool:inherits-from($root, $abstract-baseClass))
                    then string($root/@baseClass)
                    else ()
            )
        "/>

        <xsl:sequence select="$result"/>
    </xsl:function>

    <!--
        Function: cimtool:get-dependencies
        Purpose: Returns all distinct @baseClass dependency values for a single
                 element, including dependencies inherited through its a:SuperType
                 chain AND all FK associations (a:Instance, a:Reference). For
                 a:Instance/a:Reference pointing at abstract classes (a:ComplexType),
                 resolves to concrete subclass baseClasses via
                 cimtool:get-union-dependencies. Excludes types present in the
                 $exclusion-map (i.e. already-processed categories).

                 USAGE IN THIS BUILDER Ã¢â‚¬â€ COMPOUND TYPES ONLY:
                 This function is used only for Tier 2 (CompoundType) ordering,
                 where full FK dep tracking is appropriate. CompoundType association
                 networks are shallow and do not create cycles, so the full dep graph
                 produces correct ordering for compound class declarations.

                 For Tier 3 (ComplexType + Root entity class ordering), only
                 inheritance order is needed. C# allows forward class references
                 within a single compilation unit, so FK associations do NOT impose
                 any ordering constraint on class declarations. Using full FK deps
                 for entity classes creates cycles in profiles with dense association
                 graphs (e.g. CGMES CoreEquipment), causing Kahn's algorithm to stall
                 and fall back to document order for 40+ classes.
                 cimtool:get-inheritance-deps is used for Tier 3 instead.
        Parameters:
            $element       Ã¢â‚¬â€ The element to examine (a:CompoundType, a:Root, etc.)
            $exclusion-map Ã¢â‚¬â€ map(xs:string, xs:boolean) of baseClass values to exclude
        Returns:
            xs:string* Ã¢â‚¬â€ Distinct filtered sequence of dependency @baseClass values
    -->
    <xsl:function name="cimtool:get-dependencies" as="xs:string*">
        <xsl:param name="element"       as="element()"/>
        <xsl:param name="exclusion-map" as="map(xs:string, xs:boolean)"/>

        <xsl:variable name="result" select="
            distinct-values((
                (: Non-association children with @baseClass Ã¢â‚¬â€ e.g. Compound references :)
                $element/*[@baseClass]
                    [not(self::a:SuperType)]
                    [not(self::a:Instance)]
                    [not(self::a:Reference)]
                    [not(self::a:InverseInstance)]
                    [not(self::a:InverseReference)]
                    /string(@baseClass)[not(map:contains($exclusion-map, .))],

                (: Instance/Reference children Ã¢â‚¬â€ resolve abstract classes to union members.
                   The [1] predicate on $referenced-element guards against profiles where
                   the same @baseClass URI appears on more than one catalog element (e.g. a
                   class defined both as a Root and as a ComplexType in cross-profile
                   scenarios). cimtool:get-union-dependencies requires a single element
                   argument; without [1] it would throw XPTY0004 on such profiles. :)
                for $assoc in $element/(a:Instance|a:Reference)[@baseClass]
                return
                    let $assoc-baseClass      := string($assoc/@baseClass),
                        $referenced-element   := (root($element)/*/node()[@baseClass = $assoc-baseClass])[1]
                    return
                        if ($referenced-element/self::a:ComplexType) then
                            (
                                (: Union members Ã¢â‚¬â€ concrete Root subclasses of this abstract class :)
                                cimtool:get-union-dependencies($referenced-element)
                                    [not(map:contains($exclusion-map, .))],
                                (: The ComplexType itself Ã¢â‚¬â€ needed when it has no Root subclasses in
                                   this profile (e.g. NameType, NameTypeAuthority in EndDeviceControls).
                                   Without this, edges like NameÃ¢â€ â€™NameType are invisible to the sort
                                   and ordering becomes arbitrary. When union members do exist,
                                   distinct-values() collapses any redundancy. :)
                                if (not(map:contains($exclusion-map, $assoc-baseClass)))
                                then $assoc-baseClass
                                else ()
                            )
                        else if ($referenced-element/self::a:Root) then
                            if (not(map:contains($exclusion-map, $assoc-baseClass)))
                            then $assoc-baseClass
                            else ()
                        else
                            if (not(map:contains($exclusion-map, $assoc-baseClass)))
                            then $assoc-baseClass
                            else (),

                (: Include parent class itself and its dependencies via SuperType.
                   The parent's own @baseClass must be emitted as a direct dependency
                   so the topological sort places the parent before this element even
                   when the parent has no associations of its own (e.g. WorkLocation
                   before ServiceLocation). Without this, only the parent's transitive
                   dependencies flow through Ã¢â‚¬â€ the parentÃ¢â€ â€™child edge is missing. :)
                if ($element/a:SuperType) then
                    let $supertype-baseClass := string($element/a:SuperType/@baseClass),
                        $parent := (root($element)/*/node()[@baseClass = $supertype-baseClass])[1]
                    return (
                        if (not(map:contains($exclusion-map, $supertype-baseClass)))
                        then $supertype-baseClass
                        else (),
                        if ($parent)
                        then cimtool:get-dependencies($parent, $exclusion-map)
                        else ()
                    )
                else
                    ()
            ))
        "/>

        <xsl:sequence select="$result"/>
    </xsl:function>

    <!--
        Function: cimtool:get-inheritance-deps
        Purpose: Returns the @baseClass dependency value needed for topological
                 sorting of EF Core entity classes. Unlike cimtool:get-dependencies
                 (which also tracks FK associations), this function tracks ONLY the
                 direct SuperType parent dependency.

                 For C# EF Core entity class ordering, only inheritance order is
                 needed. C# allows forward class references within a single
                 compilation unit Ã¢â‚¬â€ EF Core resolves all relationships by type name
                 and reflection, not by declaration order. FK associations do NOT
                 impose any ordering constraint on class declarations.

                 Using the full cimtool:get-dependencies for entity class ordering
                 creates cycles in profiles with dense association graphs (e.g. CGMES
                 CoreEquipment where OperationalLimitSet Ã¢â€ â€™ Equipment Ã¢â€ â€™ Terminal Ã¢â€ â€™
                 ACDCConverter Ã¢â€ â€™ ... loops back through the association network).
                 These cycles cause Kahn's algorithm to stall and fall back to
                 document order for 40+ classes, defeating the purpose of the sort.

                 By tracking only the direct SuperType parent, this function
                 guarantees a cycle-free dependency graph and produces correct
                 inheritance-ordered output for all CIM profiles.
        Parameters:
            $element       Ã¢â‚¬â€ The element to examine (a:Root or a:ComplexType)
            $exclusion-map Ã¢â‚¬â€ map(xs:string, xs:boolean) of baseClass values to exclude
                             (already-processed tiers such as EnumeratedType and
                             CompoundType)
        Returns:
            xs:string* Ã¢â‚¬â€ Zero or one @baseClass string (the direct parent, if any
                         and if not excluded)
    -->
    <xsl:function name="cimtool:get-inheritance-deps" as="xs:string*">
        <xsl:param name="element"       as="element()"/>
        <xsl:param name="exclusion-map" as="map(xs:string, xs:boolean)"/>

        <xsl:sequence select="
            if ($element/a:SuperType) then
                let $supertype-baseClass := string($element/a:SuperType[1]/@baseClass)
                return
                    if (not(map:contains($exclusion-map, $supertype-baseClass)))
                    then $supertype-baseClass
                    else ()
            else
                ()
        "/>
    </xsl:function>

    <!--
        Function: cimtool:create-exclusions-map
        Purpose: Builds a map(xs:string, xs:boolean) keyed on @baseClass values
                 for a set of already-processed elements. Passed into
                 cimtool:build-dependencies-map to exclude those types from the
                 dependency graph of the next processing tier.
        Parameters:
            $elements Ã¢â‚¬â€ Elements whose @baseClass values should be excluded
                        (e.g. //a:EnumeratedType after enumerations are processed)
        Returns:
            map(xs:string, xs:boolean) Ã¢â‚¬â€ Key: @baseClass value, Value: true()
    -->
    <xsl:function name="cimtool:create-exclusions-map" as="map(xs:string, xs:boolean)">
        <xsl:param name="elements" as="element()*"/>

        <xsl:variable name="result" select="
            map:merge(
                for $element in $elements[@baseClass]
                return map:entry(string($element/@baseClass), true())
            )
        "/>

        <xsl:sequence select="$result"/>
    </xsl:function>

    <!--
        Function: cimtool:build-dependencies-map
        Purpose: Builds a map(xs:string, xs:string*) representing the full
                 dependency graph for a set of elements. Each entry maps an
                 element's @baseClass to the sequence of @baseClass values it
                 depends on (filtered by $exclusion-map).
        Parameters:
            $elements      Ã¢â‚¬â€ Elements to include in the dependency graph
            $exclusion-map Ã¢â‚¬â€ map(xs:string, xs:boolean) of already-processed
                             types to exclude from all dependency lists
        Returns:
            map(xs:string, xs:string*) Ã¢â‚¬â€ Key: @baseClass, Value: dependency sequence
    -->
    <xsl:function name="cimtool:build-dependencies-map" as="map(xs:string, xs:string*)">
        <xsl:param name="elements"      as="element()*"/>
        <xsl:param name="exclusion-map" as="map(xs:string, xs:boolean)"/>

        <xsl:variable name="result" select="
            map:merge(
                for $type in $elements[@baseClass]
                return map:entry(
                    string($type/@baseClass),
                    cimtool:get-dependencies($type, $exclusion-map)
                )
            )
        "/>

        <xsl:sequence select="$result"/>
    </xsl:function>

    <!--
        Function: cimtool:build-inheritance-map
        Purpose: Variant of cimtool:build-dependencies-map that uses
                 cimtool:get-inheritance-deps instead of cimtool:get-dependencies.
                 Used for Tier 3 (ComplexType + Root) sorting where only inheritance
                 order is required Ã¢â‚¬â€ see cimtool:get-inheritance-deps for rationale.
        Parameters:
            $elements      Ã¢â‚¬â€ Elements to include in the dependency graph
            $exclusion-map Ã¢â‚¬â€ map(xs:string, xs:boolean) of already-processed types
        Returns:
            map(xs:string, xs:string*) Ã¢â‚¬â€ Key: @baseClass, Value: dependency sequence
    -->
    <xsl:function name="cimtool:build-inheritance-map" as="map(xs:string, xs:string*)">
        <xsl:param name="elements"      as="element()*"/>
        <xsl:param name="exclusion-map" as="map(xs:string, xs:boolean)"/>

        <xsl:variable name="result" select="
            map:merge(
                for $type in $elements[@baseClass]
                return map:entry(
                    string($type/@baseClass),
                    cimtool:get-inheritance-deps($type, $exclusion-map)
                )
            )
        "/>

        <xsl:sequence select="$result"/>
    </xsl:function>

    <!--
        Function: cimtool:topological-sort
        Purpose: Entry point for topological sort. Sorts a sequence of elements
                 into dependency order so that each element appears only after
                 all elements it depends on. Falls back to document order for
                 any elements involved in circular dependencies.
        Parameters:
            $elements  Ã¢â‚¬â€ Elements to sort (e.g. //a:CompoundType or //a:Root)
            $deps-map  Ã¢â‚¬â€ Dependency map from cimtool:build-dependencies-map
        Returns:
            element()* Ã¢â‚¬â€ Elements in topological (dependency-safe) order
    -->
    <xsl:function name="cimtool:topological-sort" as="element()*">
        <xsl:param name="elements"  as="element()*"/>
        <xsl:param name="deps-map"  as="map(xs:string, xs:string*)"/>

        <xsl:sequence select="cimtool:topological-sort-helper($elements, $deps-map, ())"/>
    </xsl:function>

    <!--
        Function: cimtool:topological-sort-helper
        Purpose: Recursive Kahn's-algorithm implementation. On each pass, finds
                 all elements whose dependencies are fully satisfied by the
                 already-sorted set, adds them to the sorted sequence, and
                 recurses on the remainder. Terminates when all elements are
                 sorted or a circular dependency is detected (fallback to
                 document order for the remaining unsortable elements).
        Parameters:
            $remaining Ã¢â‚¬â€ Elements not yet placed in the sorted output
            $deps-map  Ã¢â‚¬â€ Dependency map
            $sorted    Ã¢â‚¬â€ Accumulated sorted elements (initially empty)
        Returns:
            element()* Ã¢â‚¬â€ Fully sorted sequence
    -->
    <xsl:function name="cimtool:topological-sort-helper" as="element()*">
        <xsl:param name="remaining" as="element()*"/>
        <xsl:param name="deps-map"  as="map(xs:string, xs:string*)"/>
        <xsl:param name="sorted"    as="element()*"/>

        <xsl:choose>
            <xsl:when test="fn:empty($remaining)">
                <!-- Base case: all elements sorted -->
                <xsl:sequence select="$sorted"/>
            </xsl:when>
            <xsl:otherwise>
                <xsl:variable name="sorted-baseClasses" select="
                    for $elem in $sorted return string($elem/@baseClass)
                "/>

                <!-- Elements whose every dependency is already in the sorted set -->
                <xsl:variable name="ready-elements" select="
                    for $elem in $remaining
                    return
                        let $elem-baseClass := fn:string($elem/@baseClass),
                            $elem-deps      := $deps-map($elem-baseClass)
                        return
                            if (every $dep in $elem-deps satisfies $dep = $sorted-baseClasses)
                            then $elem
                            else ()
                "/>

                <xsl:choose>
                    <xsl:when test="fn:exists($ready-elements)">
                        <xsl:sequence select="cimtool:topological-sort-helper(
                            $remaining except $ready-elements,
                            $deps-map,
                            ($sorted, $ready-elements)
                        )"/>
                    </xsl:when>
                    <xsl:otherwise>
                        <!-- Circular dependency detected Ã¢â‚¬â€ append remaining in document order -->
                        <xsl:sequence select="$sorted"/>
                        <xsl:sequence select="$remaining"/>
                    </xsl:otherwise>
                </xsl:choose>
            </xsl:otherwise>
        </xsl:choose>
    </xsl:function>

    <!--
    Ã¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢Â
    END: TOPOLOGICAL SORT FUNCTIONS
    Ã¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢Â
    -->

    <!-- Top-level Catalog template -->
    <xsl:template match="a:Catalog">

        <!-- Ã¢â€â‚¬Ã¢â€â‚¬ Topological sort Ã¢â‚¬â€ computed once, reused by both class output   -->
        <!-- Ã¢â€â‚¬Ã¢â€â‚¬ and allClasses array to ensure both are in the same safe order. -->

        <!-- Tier 1: EnumeratedType Ã¢â‚¬â€ no dependencies, processed first -->

        <!-- Tier 2: CompoundType Ã¢â‚¬â€ exclude already-processed EnumeratedTypes -->
        <xsl:variable name="compound-exclusions"
            select="cimtool:create-exclusions-map(//a:EnumeratedType)"/>
        <xsl:variable name="compound-deps-map"
            select="cimtool:build-dependencies-map(//a:CompoundType, $compound-exclusions)"/>
        <xsl:variable name="sorted-compounds"
            select="cimtool:topological-sort(//a:CompoundType, $compound-deps-map)"/>

        <!-- Tier 3: ComplexType + Root Ã¢â‚¬â€ exclude EnumeratedTypes and CompoundTypes.    -->
        <!-- Uses cimtool:get-inheritance-deps rather than cimtool:get-dependencies   -->
        <!-- because only inheritance order is needed for C# class declarations.      -->
        <!-- See cimtool:get-inheritance-deps documentation for full rationale.       -->
        <xsl:variable name="class-exclusions"
            select="cimtool:create-exclusions-map(//a:EnumeratedType|//a:CompoundType)"/>
        <xsl:variable name="class-deps-map"
            select="cimtool:build-inheritance-map(//a:ComplexType|//a:Root, $class-exclusions)"/>
        <xsl:variable name="sorted-classes"
            select="cimtool:topological-sort(//a:ComplexType|//a:Root, $class-deps-map)"/>

        <document>
			<list begin="// ============================================================" indent="// " end="// ============================================================">
				<item>Annotated C# for <xsl:value-of select="$envelope"/></item>
				<item>Generated by CIMTool https://cimtool.ucaiug.io [cimtool.ucaiug.io]</item>
				<item></item>
				<item>DO NOT EDIT - this file is fully regenerated by CIMTool on</item>
				<item>every build.  Hand-written customisations belong in partial</item>
				<item>class files alongside this one.</item>
            </list>
            <item></item>
            <item>using System;</item>
            <item>using System.ComponentModel.DataAnnotations;</item>
            <item>using System.ComponentModel.DataAnnotations.Schema;</item>
            <item>using System.Collections.Generic;</item>
            <item>using System.Linq;</item>
            <item>using System.Threading;</item>
            <item>using System.Threading.Tasks;</item>
            <item>using Microsoft.EntityFrameworkCore;</item>
            <item></item>
            <list begin="" indent="/// " end="">
				<list begin="&lt;summary&gt;" indent="" end="&lt;/summary&gt;">
					<item>Entity Framework Core entity classes generated from the</item>
					<item>&lt;b&gt;CoreEquipment&lt;/b&gt; CIMTool profile.</item>
				</list>
				<list begin="&lt;remarks&gt;" indent="" end="&lt;/remarks&gt;">
					<list begin="&lt;para&gt;" indent="" end="&lt;/para&gt;">
						<item>&lt;b&gt;No &lt;c&gt;ICollection&amp;lt;T&amp;gt;&lt;/c&gt; Properties - Intentional Design&lt;/b&gt;&lt;br/&gt;</item>
						<item>Collection-valued inverse navigation properties</item>
						<item>(&lt;c&gt;ICollection&amp;lt;T&amp;gt;&lt;/c&gt;) are intentionally not generated.</item>
						<item>Single-valued foreign-key relationships are fully mapped; inverse</item>
						<item>one-to-many collections are omitted by design, not by omission.</item>
						<item>EF Core does not require them to configure or resolve a relationship.</item>
						<item>They are excluded because:</item>
						<list begin="&lt;list type=&quot;bullet&quot;&gt;" indent="   " end=" &lt;/list&gt;">
							<list begin="&lt;item&gt;" indent="  " end="&lt;/item&gt;">
								<item>CIM associations are unbounded by definition. Materialising an</item>
								<item>unbounded &lt;c&gt;ICollection&amp;lt;T&amp;gt;&lt;/c&gt; into memory with no pagination</item>
								<item>is a runtime hazard for large profiles such as CGMES CoreEquipment.</item>
							</list>
							<list begin="&lt;item&gt;" indent="  " end="&lt;/item&gt;">
								<item>They introduce circular-reference risk during JSON serialisation</item>
								<item>with &lt;c&gt;System.Text.Json&lt;/c&gt; or &lt;c&gt;Newtonsoft.Json&lt;/c&gt; unless</item>
								<item>reference handling is explicitly configured.</item>
							</list>
							<list begin="&lt;item&gt;" indent="  " end="&lt;/item&gt;">
								<item>Inverse associations are profile-dependent and not present in all</item>
								<item>profiles, making generated &lt;c&gt;ICollection&amp;lt;T&amp;gt;&lt;/c&gt; properties</item>
								<item>inconsistent across profiles.</item>
							</list>
							<list begin="&lt;item&gt;" indent="  " end="&lt;/item&gt;">
								<item>Bidirectional EF Core mappings require both sides to be kept</item>
								<item>in sync on every add/remove, adding fragility to consumer code.</item>
							</list>
							<list begin="&lt;item&gt;" indent="  " end="&lt;/item&gt;">
								<item>Explicit LINQ queries support pagination, filtering, and sorting</item>
								<item>that a generated collection property never can.</item>
							</list>
						</list>
						<item>To navigate from a parent entity to its children, query through</item>
						<item>the child side using LINQ:</item>
						<list begin="&lt;code&gt;" indent="" end="&lt;/code&gt;">
							<item>// All children for a given parent (via shadow FK):</item>
							<item>var units = context.Set&lt;PowerElectronicsUnit&gt;()</item>
							<item>    .Where(u => u.PowerElectronicsConnectionId == connection.MRId)</item>
							<item>    .ToList();</item>
							<item></item>
							<item>// With pagination for large result sets:</item>
							<item>var equipment = context.Set&lt;Equipment&gt;()</item>
							<item>    .Where(e => e.EquipmentContainerId == substation.MRId)</item>
							<item>    .Skip(0).Take(100).ToList();</item>
						</list>
						<item>Consumers who need inverse navigation for specific patterns can add</item>
						<item>&lt;c&gt;ICollection&amp;lt;T&amp;gt;&lt;/c&gt; properties non-destructively via</item>
						<item>&lt;c&gt;partial class&lt;/c&gt; declarations without modifying this generated file.</item>
					</list>	
					<list begin="&lt;para&gt;" indent="" end="&lt;/para&gt;">
						<item>&lt;b&gt;Compound Type References&lt;/b&gt;&lt;br/&gt;</item>
						<item>CIM Compound types are treated as value objects - each compound row has</item>
						<item>exactly one owner and is never shared across parent columns or rows.</item>
						<item>Each compound reference is represented as a shadow FK string property</item>
						<item>(e.g. &lt;c&gt;ElectronicAddressId&lt;/c&gt;) paired with a nullable navigation</item>
						<item>property (e.g. &lt;c&gt;ElectronicAddress?&lt;/c&gt;).</item>
						<item>The companion SQL DDL emits &lt;c&gt;ON DELETE CASCADE&lt;/c&gt; for every compound FK,</item>
						<item>and the generated &lt;c&gt;ModelConfiguration&lt;/c&gt; applies</item>
						<item>&lt;c&gt;DeleteBehavior.Cascade&lt;/c&gt; for compound relationships so EF Core treats</item>
						<item>compound references as owned-like one-to-one links in its relationship metadata.</item>
						<item>Because the FK column lives on the owner row, owner-side orphan cleanup during</item>
						<item>replacement or parent deletion must still be handled explicitly. The generated</item>
						<item>&lt;c&gt;DbContextBase&lt;/c&gt; in this file includes profile-specific cleanup hooks that</item>
						<item>collect and remove now-unreferenced compound rows after owner changes are saved.</item>
						<item>Each compound entity constructor assigns a new &lt;see cref="System.Guid"/&gt;</item>
						<item>to its &lt;c&gt;Id&lt;/c&gt; property on instantiation.</item>
					</list>
					<list begin="&lt;para&gt;" indent="" end="&lt;/para&gt;">
						<item>&lt;b&gt;DbContext Integration&lt;/b&gt;&lt;br/&gt;</item>
						<item>This file includes a generated &lt;c&gt;ModelConfiguration&lt;/c&gt; nested static</item>
						<item>class and a generated abstract &lt;c&gt;DbContextBase&lt;/c&gt; that provides</item>
						<item>DbSet properties, EF model configuration wiring, and generated compound</item>
						<item>cleanup support for owner-side orphan handling.</item>
						<item>Create the following thin DbContext subclass once in your project - it</item>
						<item>will not be overwritten by CIMTool:</item>
						<list begin="&lt;code&gt;" indent="" end="&lt;/code&gt;">
							<item>public class <xsl:value-of select="$envelope"/>DbContext : <xsl:value-of select="$envelope"/>.DbContextBase</item>
							<list begin="{{" indent="    " end="}}">
								<item>public <xsl:value-of select="$envelope"/>DbContext(DbContextOptions options) : base(options) { }</item>
							</list>
						</list>
					</list>	
				</list>	
            </list>
            <item>public class <xsl:value-of select="$envelope"/></item>
            <list begin="{{" indent="    " end="}}">
                <!-- Classes emitted in topological order -->
                <xsl:apply-templates select="a:EnumeratedType"/>
                <xsl:apply-templates select="$sorted-compounds"/>
                <xsl:apply-templates select="$sorted-classes"/>
                <!-- allClasses array reuses the same sorted sequences -->
                <xsl:call-template name="config">
                    <xsl:with-param name="sorted-compounds" select="$sorted-compounds"/>
                    <xsl:with-param name="sorted-classes"   select="$sorted-classes"/>
                </xsl:call-template>
                <!-- ModelConfiguration static class Ã¢â‚¬â€ complete Fluent API configuration -->
                <xsl:call-template name="dbcontext">
                    <xsl:with-param name="sorted-compounds" select="$sorted-compounds"/>
                    <xsl:with-param name="sorted-classes"   select="$sorted-classes"/>
                </xsl:call-template>
                <xsl:call-template name="dbcontext-base">
                    <xsl:with-param name="sorted-compounds" select="$sorted-compounds"/>
                    <xsl:with-param name="sorted-classes"   select="$sorted-classes"/>
                </xsl:call-template>
            </list>
        </document>
    </xsl:template>

    <!--
        Class template for ComplexType and Root.

        ROUTING LOGIC
        Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬
        This template handles two structurally distinct cases, detected by
        cimtool:has-mrid-ancestor():

        Case 1 Ã¢â‚¬â€ IdentifiedObject hierarchy (mRID ancestor found)
          The standard Table-Per-Type (TPT) path. The class inherits mRID as its
          natural primary key from IdentifiedObject (directly or transitively). EF
          Core maps the class to its own table, inheriting the [Key] from the root
          entity. A superclass reference is emitted as a C# base class.

        Case 2 Ã¢â‚¬â€ Non-IdentifiedObject class (no mRID ancestor)
          A small subset of CIM classes do not inherit from IdentifiedObject. These
          are typically collection-member or point-data classes (e.g. CurveData,
          RegularTimePoint, NonlinearShuntCompensatorPoint, TapChangerTablePoint).
          They have no natural single-column PK derivable from the CIM model.

          Treatment mirrors the CompoundType surrogate pattern with two sub-cases:

          2a Ã¢â‚¬â€ Has a superclass (e.g. PhaseTapChangerTablePoint, RatioTapChangerTablePoint)
            The entity participates in a non-IdentifiedObject TPT hierarchy. It
            inherits the surrogate 'id' key from its parent and MUST NOT redeclare
            it. The class declaration emits C# inheritance (': SuperType') without
            any [Key] block.

          2b Ã¢â‚¬â€ No superclass (e.g. TapChangerTablePoint, CurveData, RegularTimePoint,
            NonlinearShuntCompensatorPoint)
            A surrogate VARCHAR(100) 'id' column is emitted as the [Key].
            A heuristic [Index(..., IsUnique=true)] is emitted across all required
            (minOccurs=1) non-surrogate scalar properties and single-valued FK
            shadow properties. This is the best uniqueness guard achievable without
            class-specific knowledge, since the profile XML does not encode which
            subset of columns forms the natural composite key Ã¢â‚¬â€ that information
            exists only in the IEC specification prose, not the machine-readable
            profile.
            - The heuristic may include more columns than strictly necessary for the
              natural key (e.g. value columns alongside the true key columns), making
              the constraint wider than ideal but never incorrect. A constraint that
              is too wide prevents valid duplicates from being inserted; a constraint
              that is missing allows silent semantic corruption. The wider constraint
              is the safer failure mode.
            - NOTE: The parallel SQL DDL builder (sql.xsl) must apply the same
              detection and emit "id" VARCHAR(100) PRIMARY KEY plus an equivalent
              UNIQUE constraint for these classes. Both builders share the same
              cimtool:has-mrid-ancestor logic to ensure consistent output.
    -->
    <xsl:template match="a:ComplexType|a:Root">
        <xsl:variable name="super" select="a:SuperType[1]"/>
        <xsl:call-template name="annotate"/>
        <item>[Table(&quot;<xsl:value-of select="@name"/>&quot;)]</item>
        <!-- Emit [Index] unique constraint for each a:Compound FK column.
             a:Instance references are NOT unique Ã¢â‚¬â€ a:Compound references are
             always 1:1 by definition and must be enforced as such in EF Core.
             [Index] is a class-level attribute from Microsoft.EntityFrameworkCore
             and has no data annotation equivalent in System.ComponentModel.DataAnnotations. -->
        <xsl:for-each select="a:Compound">
            <item>[Index(nameof(<xsl:value-of select="cimtool:capitalize(@name)"/>Id), IsUnique = true)]</item>
        </xsl:for-each>

        <xsl:choose>

            <!-- Ã¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢Â Case 1: IdentifiedObject hierarchy Ã¢â‚¬â€ standard TPT path Ã¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢Â -->
            <xsl:when test="cimtool:has-mrid-ancestor(.)">
                <xsl:choose>
                    <xsl:when test="$super">
                        <item>public class <xsl:value-of select="@name"/> : <xsl:value-of select="$super/@name"/></item>
                    </xsl:when>
                    <xsl:otherwise>
                        <item>public class <xsl:value-of select="@name"/></item>
                    </xsl:otherwise>
                </xsl:choose>
                <list begin="{{" indent="    " delim="" end="}}">
                    <xsl:if test="$super">
                        <item>// Inherits from <xsl:value-of select="$super/@name"/> - configure further in EF Fluent API if needed</item>
                    </xsl:if>
                    <item>public <xsl:value-of select="@name"/>() { }</item>
                    <item>public override string ToString() { return this.GetType().Name; }</item>
                    <xsl:if test="not($super)">
                        <list begin="" indent="/// " end="">
                            <list begin="&lt;summary&gt;" indent="" end="&lt;/summary&gt;">
                                <item>Determines whether this instance and a specified object represent the same</item>
                                <item><xsl:value-of select="@name"/>, compared by runtime type and &lt;c&gt;MRId&lt;/c&gt;.</item>
                            </list>
                            <list begin="&lt;param name=&quot;obj&quot;&gt;" indent="" end="&lt;/param&gt;">
                                <item>The object to compare with this instance.</item>
                            </list>
                            <list begin="&lt;returns&gt;" indent="" end="&lt;/returns&gt;">
                                <item>&lt;c&gt;true&lt;/c&gt; if &lt;paramref name=&quot;obj&quot;/&gt; is the same concrete type as this instance</item>
                                <item>and both have an equal, non-null &lt;c&gt;MRId&lt;/c&gt;; otherwise &lt;c&gt;false&lt;/c&gt;.</item>
                            </list>
                            <list begin="&lt;remarks&gt;" indent="" end="&lt;/remarks&gt;">
                                <item>The runtime-type guard ensures two different subclasses with coincidentally</item>
                                <item>identical UUIDs are never considered equal. Returning &lt;c&gt;false&lt;/c&gt; when</item>
                                <item>&lt;c&gt;MRId&lt;/c&gt; is &lt;c&gt;null&lt;/c&gt; is consistent with the EF Core convention that a</item>
                                <item>transient (not-yet-persisted) entity is not equal to any other entity.</item>
                                <item>All subclasses inherit this implementation and must not override it.</item>
                            </list>
                        </list>
                        <item>public override bool Equals(object? obj)</item>
                        <list begin="{{" indent="    " delim="" end="}}">
                            <item>if (obj is not <xsl:value-of select="@name"/> other) return false;</item>
                            <item>if (ReferenceEquals(this, other)) return true;</item>
                            <item>if (GetType() != other.GetType()) return false;</item>
                            <item>return MRId != null &amp;&amp; MRId == other.MRId;</item>
                        </list>
                        <list begin="" indent="/// " end="">
                            <list begin="&lt;summary&gt;" indent="" end="&lt;/summary&gt;">
                                <item>Returns a hash code based on runtime type and &lt;c&gt;MRId&lt;/c&gt;.</item>
                            </list>
                            <list begin="&lt;returns&gt;" indent="" end="&lt;/returns&gt;">
                                <item>A hash code combining the runtime type and &lt;c&gt;MRId&lt;/c&gt;, consistent with</item>
                                <item>the &lt;see cref=&quot;Equals&quot;/&gt; override. All subclasses inherit this implementation.</item>
                            </list>
                        </list>
                        <item>public override int GetHashCode() => HashCode.Combine(GetType(), MRId);</item>
                    </xsl:if>
                    <xsl:apply-templates/>
                </list>
            </xsl:when>

            <!-- Ã¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢Â Case 2: Non-IdentifiedObject Ã¢â‚¬â€ surrogate id + heuristic UNIQUE Ã¢â€¢ÂÃ¢â€¢ÂÃ¢â€¢Â
                 Sub-case 2a: Has a superclass (e.g. PhaseTapChangerTablePoint, RatioTapChangerTablePoint)
                   The entity participates in a non-IdentifiedObject TPT hierarchy. It
                   inherits the surrogate 'id' key from its parent and must NOT redeclare
                   it Ã¢â‚¬â€ doing so would create a duplicate [Key] in the EF model. The class
                   declaration emits C# inheritance exactly as Case 1 does, but without the
                   surrogate key block. The SQL schema enforces this via the inheritance FK
                   constraint (e.g. "PhaseTapChangerTablePoint"."id" REFERENCES
                   "TapChangerTablePoint"."id"), which the SQL builder emits correctly.

                 Sub-case 2b: No superclass (e.g. TapChangerTablePoint, CurveData,
                   RegularTimePoint, NonlinearShuntCompensatorPoint)
                   The entity is the root of its own hierarchy (or a standalone class) with
                   no natural single-column PK. A surrogate 'id' key is declared here. -->
            <xsl:otherwise>
                <!-- Build the heuristic UNIQUE index across all required non-surrogate
                     properties. Covers required scalar attributes and required single-valued
                     FK shadow properties. Collection associations are excluded since the FK
                     lives on the child table side, not here. Applies to both sub-cases. -->
                <xsl:variable name="required-props" as="xs:string*">
                    <xsl:for-each select="(a:Simple|a:Domain)[@minOccurs='1']">
                        <xsl:sequence select="cimtool:capitalize(@name)"/>
                    </xsl:for-each>
                    <xsl:for-each select="(a:Instance|a:Reference)[@minOccurs='1'][not(@maxOccurs) or @maxOccurs='1']">
                        <xsl:sequence select="concat(cimtool:capitalize(@name), 'Id')"/>
                    </xsl:for-each>
                </xsl:variable>
                <xsl:if test="count($required-props) gt 0">
                    <item>[Index(<xsl:value-of
                        select="string-join(for $p in $required-props return concat('nameof(', $p, ')'), ', ')"/>, IsUnique = true)]</item>
                </xsl:if>
                <xsl:choose>

                    <!-- Sub-case 2a: Has superclass Ã¢â‚¬â€ inherits surrogate key from parent -->
                    <xsl:when test="$super">
                        <item>public class <xsl:value-of select="@name"/> : <xsl:value-of select="$super/@name"/></item>
                        <list begin="{{" indent="    " delim="" end="}}">
                            <item>// Inherits surrogate 'id' key from <xsl:value-of select="$super/@name"/> Ã¢â‚¬â€ do not redeclare [Key] here.</item>
                            <item>// Neither this class nor its parent inherits from IdentifiedObject.</item>
                            <item>// EF Core maps this as TPT using the shared surrogate 'id' PK.</item>
                            <item>public <xsl:value-of select="@name"/>() { }</item>
                            <item>public override string ToString() { return this.GetType().Name; }</item>
                            <xsl:apply-templates/>
                        </list>
                    </xsl:when>

                    <!-- Sub-case 2b: No superclass Ã¢â‚¬â€ declares its own surrogate key -->
                    <xsl:otherwise>
                        <item>public class <xsl:value-of select="@name"/></item>
                        <list begin="{{" indent="    " delim="" end="}}">
                            <item>public <xsl:value-of select="@name"/>() { }</item>
                            <item>public override string ToString() { return this.GetType().Name; }</item>
                            <list begin="" indent="/// " end="">
                                <list begin="&lt;summary&gt;" indent="" end="&lt;/summary&gt;">
                                    <item>Determines whether this instance and a specified object represent the same</item>
                                    <item>&lt;c&gt;<xsl:value-of select="@name"/>&lt;/c&gt;, compared by runtime type and surrogate &lt;c&gt;Id&lt;/c&gt;.</item>
                                </list>
                                <list begin="&lt;param name=&quot;obj&quot;&gt;" indent="" end="&lt;/param&gt;">
                                    <item>The object to compare with this instance.</item>
                                </list>
                                <list begin="&lt;returns&gt;" indent="" end="&lt;/returns&gt;">
                                    <item>&lt;c&gt;true&lt;/c&gt; if &lt;paramref name=&quot;obj&quot;/&gt; is the same concrete type as this instance</item>
                                    <item>and both have an equal, non-null surrogate &lt;c&gt;Id&lt;/c&gt;; otherwise &lt;c&gt;false&lt;/c&gt;.</item>
                                </list>
                                <list begin="&lt;remarks&gt;" indent="" end="&lt;/remarks&gt;">
                                    <item>This class uses a surrogate &lt;c&gt;Id&lt;/c&gt; rather than a natural &lt;c&gt;MRId&lt;/c&gt;</item>
                                    <item>because it does not inherit from IdentifiedObject. The runtime-type guard</item>
                                    <item>ensures subclasses with the same surrogate &lt;c&gt;Id&lt;/c&gt; are not considered equal.</item>
                                    <item>Returning &lt;c&gt;false&lt;/c&gt; when &lt;c&gt;Id&lt;/c&gt; is &lt;c&gt;null&lt;/c&gt; is consistent with the</item>
                                    <item>convention that a transient entity is not equal to any other entity.</item>
                                    <item>Subclasses inherit this implementation and must not override it.</item>
                                </list>
                            </list>
                            <item>public override bool Equals(object? obj)</item>
                            <list begin="{{" indent="    " delim="" end="}}">
                                <item>if (obj is not <xsl:value-of select="@name"/> other) return false;</item>
                                <item>if (ReferenceEquals(this, other)) return true;</item>
                                <item>if (GetType() != other.GetType()) return false;</item>
                                <item>return Id != null &amp;&amp; Id == other.Id;</item>
                            </list>
                            <list begin="" indent="/// " end="">
                                <list begin="&lt;summary&gt;" indent="" end="&lt;/summary&gt;">
                                    <item>Returns a hash code based on runtime type and surrogate &lt;c&gt;Id&lt;/c&gt;.</item>
                                </list>
                                <list begin="&lt;returns&gt;" indent="" end="&lt;/returns&gt;">
                                    <item>A hash code combining the runtime type and &lt;c&gt;Id&lt;/c&gt;, consistent with</item>
                                    <item>the &lt;see cref=&quot;Equals&quot;/&gt; override. Subclasses inherit this implementation.</item>
                                </list>
                            </list>
                            <item>public override int GetHashCode() => HashCode.Combine(GetType(), Id);</item>
                            <list begin="" indent="/// " end="">
                                <list begin="&lt;summary&gt;" indent="" end="&lt;/summary&gt;">
                                    <item>Surrogate primary key - this class does not inherit from IdentifiedObject</item>
                                    <item>and has no natural single-column primary key. This surrogate 'id' is a</item>
                                    <item>persistence artefact. The [Index] above enforces a heuristic uniqueness</item>
                                    <item>constraint across all required non-surrogate columns as a guard against</item>
                                    <item>semantically corrupt duplicate rows. See the template documentation in</item>
                                    <item>the XSLT source for a full explanation of the design tradeoffs.</item>
                                </list>
                            </list>
                            <item>[Key]</item>
                            <item>[Column("id")]</item>
                            <item>[MaxLength(100)]</item>
                            <item>public string Id { get; set; } = null!;</item>
                            <xsl:apply-templates/>
                        </list>
                    </xsl:otherwise>

                </xsl:choose>
            </xsl:otherwise>

        </xsl:choose>
    </xsl:template>

    <!-- Class template for CompoundType -->
    <xsl:template match="a:CompoundType">
        <xsl:call-template name="annotate"/>
        <item>[Table(&quot;<xsl:value-of select="@name"/>&quot;)]</item>
        <!-- Emit [Index] unique constraint for each nested a:Compound FK column.
             CompoundTypes can themselves reference other CompoundTypes (e.g.
             StreetAddress references Status, StreetDetail, TownDetail), and those
             nested compound references are equally 1:1 and require unique indexes. -->
        <xsl:for-each select="a:Compound">
            <item>[Index(nameof(<xsl:value-of select="cimtool:capitalize(@name)"/>Id), IsUnique = true)]</item>
        </xsl:for-each>
        <item>public class <xsl:value-of select="@name"/></item>
        <list begin="{{" indent="    " delim="" end="}}">
            <list begin="" indent="/// " end="">
                <list begin="&lt;summary&gt;" indent="" end="&lt;/summary&gt;">
                    <item>Initialises a new &lt;c&gt;<xsl:value-of select="@name"/>&lt;/c&gt; and assigns a new UUID to &lt;c&gt;Id&lt;/c&gt;,</item>
                    <item>satisfying the compound value object ownership invariant: each compound row</item>
                    <item>has exactly one owner and must carry a unique surrogate id that is never</item>
                    <item>shared across parent columns or parent rows. See the companion</item>
                    <item>sql-rdfs-ansi92 DDL script for full details.</item>
                </list>
            </list>
            <item>public <xsl:value-of select="@name"/>() { Id = System.Guid.NewGuid().ToString(); }</item>
            <item>public override string ToString() { return this.GetType().Name; }</item>
            <list begin="" indent="/// " end="">
                <list begin="&lt;summary&gt;" indent="" end="&lt;/summary&gt;">
                    <item>Determines whether this instance and a specified object represent the same</item>
                    <item>&lt;c&gt;<xsl:value-of select="@name"/>&lt;/c&gt; compound value object, compared by surrogate &lt;c&gt;Id&lt;/c&gt;.</item>
                </list>
                <list begin="&lt;param name=&quot;obj&quot;&gt;" indent="" end="&lt;/param&gt;">
                    <item>The object to compare with this instance.</item>
                </list>
                <list begin="&lt;returns&gt;" indent="" end="&lt;/returns&gt;">
                    <item>&lt;c&gt;true&lt;/c&gt; if &lt;paramref name=&quot;obj&quot;/&gt; is a &lt;c&gt;<xsl:value-of select="@name"/>&lt;/c&gt; with the same</item>
                    <item>&lt;c&gt;Id&lt;/c&gt;; otherwise &lt;c&gt;false&lt;/c&gt;.</item>
                </list>
                <list begin="&lt;remarks&gt;" indent="" end="&lt;/remarks&gt;">
                    <item>Compound types are value objects - each row has exactly one owner and its</item>
                    <item>&lt;c&gt;Id&lt;/c&gt; is a UUID assigned on construction, so &lt;c&gt;Id&lt;/c&gt; is never</item>
                    <item>&lt;c&gt;null&lt;/c&gt; and no null guard is required.</item>
                </list>
            </list>
            <item>public override bool Equals(object? obj)</item>
            <list begin="{{" indent="    " delim="" end="}}">
                <item>if (obj is not <xsl:value-of select="@name"/> other) return false;</item>
                <item>if (ReferenceEquals(this, other)) return true;</item>
                <item>return Id == other.Id;</item>
            </list>
            <list begin="" indent="/// " end="">
                <list begin="&lt;summary&gt;" indent="" end="&lt;/summary&gt;">
                    <item>Returns a hash code based on the surrogate &lt;c&gt;Id&lt;/c&gt; of this instance.</item>
                </list>
                <list begin="&lt;returns&gt;" indent="" end="&lt;/returns&gt;">
                    <item>The hash code of &lt;c&gt;Id&lt;/c&gt;, consistent with the &lt;see cref=&quot;Equals&quot;/&gt; override.</item>
                </list>
            </list>
            <item>public override int GetHashCode() => Id.GetHashCode();</item>
            <item>[Key]</item>
            <item>[Column("id")]</item>
            <item>[MaxLength(100)]</item>
            <item>public string Id { get; set; } = null!;</item>
            <xsl:apply-templates/>
        </list>
    </xsl:template>

    <!-- Class template for EnumeratedType -->
    <xsl:template match="a:EnumeratedType">
        <xsl:call-template name="annotate"/>
        <item>[Table(&quot;<xsl:value-of select="@name"/>&quot;)]</item>
        <item>public class <xsl:value-of select="@name"/></item>
        <list begin="{{" indent="    " delim="" end="}}">
            <item>public <xsl:value-of select="@name"/>() { }</item>
            <item>public override string ToString() { return this.GetType().Name; }</item>
            <list begin="" indent="/// " end="">
                <list begin="&lt;summary&gt;" indent="" end="&lt;/summary&gt;">
                    <item>Determines whether this instance and a specified object represent the same</item>
                    <item>&lt;c&gt;<xsl:value-of select="@name"/>&lt;/c&gt; enumeration literal, compared by &lt;c&gt;Name&lt;/c&gt;.</item>
                </list>
                <list begin="&lt;param name=&quot;obj&quot;&gt;" indent="" end="&lt;/param&gt;">
                    <item>The object to compare with this instance.</item>
                </list>
                <list begin="&lt;returns&gt;" indent="" end="&lt;/returns&gt;">
                    <item>&lt;c&gt;true&lt;/c&gt; if &lt;paramref name=&quot;obj&quot;/&gt; is a &lt;c&gt;<xsl:value-of select="@name"/>&lt;/c&gt; with the same</item>
                    <item>non-null &lt;c&gt;Name&lt;/c&gt;; otherwise &lt;c&gt;false&lt;/c&gt;.</item>
                </list>
                <list begin="&lt;remarks&gt;" indent="" end="&lt;/remarks&gt;">
                    <item>Enumeration types are lookup tables keyed on &lt;c&gt;Name&lt;/c&gt;. Returning</item>
                    <item>&lt;c&gt;false&lt;/c&gt; when &lt;c&gt;Name&lt;/c&gt; is &lt;c&gt;null&lt;/c&gt; is consistent with the</item>
                    <item>convention that a transient entity is not equal to any other entity.</item>
                </list>
            </list>
            <item>public override bool Equals(object? obj)</item>
            <list begin="{{" indent="    " delim="" end="}}">
                <item>if (obj is not <xsl:value-of select="@name"/> other) return false;</item>
                <item>if (ReferenceEquals(this, other)) return true;</item>
                <item>return Name != null &amp;&amp; Name == other.Name;</item>
            </list>
            <list begin="" indent="/// " end="">
                <list begin="&lt;summary&gt;" indent="" end="&lt;/summary&gt;">
                    <item>Returns a hash code based on the &lt;c&gt;Name&lt;/c&gt; of this enumeration literal.</item>
                </list>
                <list begin="&lt;returns&gt;" indent="" end="&lt;/returns&gt;">
                    <item>The hash code of &lt;c&gt;Name&lt;/c&gt;, or zero if &lt;c&gt;Name&lt;/c&gt; is &lt;c&gt;null&lt;/c&gt;,</item>
                    <item>consistent with the &lt;see cref=&quot;Equals&quot;/&gt; override.</item>
                </list>
            </list>
            <item>public override int GetHashCode() => Name?.GetHashCode() ?? 0;</item>
            <item>[Key]</item>
            <item>[Column("name")]</item>
            <item>[MaxLength(100)]</item>
            <item>public string Name { get; set; } = null!;</item>
            <xsl:apply-templates select="a:EnumeratedValue"/>
        </list>
    </xsl:template>

    <!-- In config mode: output each class reference using typeof(...) -->
    <xsl:template match="a:ComplexType|a:Root|a:EnumeratedType|a:CompoundType" mode="config">
        <item>typeof(<xsl:value-of select="@name"/>)</item>
    </xsl:template>

    <!-- Property template for a:Simple and a:Domain (simple attributes) -->
    <xsl:template match="a:Simple|a:Domain">
        <xsl:call-template name="annotate"/>
        <xsl:choose>
            <xsl:when test="@name = 'mRID'">
                <item>[Key]</item>
            </xsl:when>
            <xsl:otherwise></xsl:otherwise>
        </xsl:choose>
        <item>[Column(&quot;<xsl:value-of select="@name"/>&quot;)]</item>
        <xsl:variable name="maxLength" select="cimtool:maxLength(@xstype, @name)"/>
        <xsl:if test="fn:exists($maxLength)">
            <item><xsl:value-of select="$maxLength"/></item>
        </xsl:if>
        <xsl:choose>
            <!-- Non-nullable string PK Ã¢â‚¬â€ MRId and compound surrogate id -->
            <xsl:when test="cimtool:csType(@xstype) = 'string' and (@name = 'mRID' or @name = 'id')">
                <item>public string <xsl:value-of select="cimtool:safePropertyName(@name, @xstype, .)"/> { get; set; } = null!;</item>
            </xsl:when>
            <!-- Required string Ã¢â‚¬â€ minOccurs = 1 -->
            <xsl:when test="cimtool:csType(@xstype) = 'string' and @minOccurs = '1'">
                <item>public string <xsl:value-of select="cimtool:safePropertyName(@name, @xstype, .)"/> { get; set; } = null!;</item>
            </xsl:when>
            <!-- Optional string Ã¢â‚¬â€ minOccurs = 0 or absent -->
            <xsl:when test="cimtool:csType(@xstype) = 'string'">
                <item>public string? <xsl:value-of select="cimtool:safePropertyName(@name, @xstype, .)"/> { get; set; }</item>
            </xsl:when>
            <!-- Required value type Ã¢â‚¬â€ minOccurs = 1 (bool, int, DateTime, etc.) -->
            <xsl:when test="@minOccurs = '1'">
                <item>public <xsl:value-of select="cimtool:csType(@xstype)"/><xsl:text> </xsl:text><xsl:value-of select="cimtool:safePropertyName(@name, @xstype, .)"/> { get; set; }</item>
            </xsl:when>
            <!-- Optional value type Ã¢â‚¬â€ minOccurs = 0 or absent -->
            <xsl:otherwise>
                <item>public <xsl:value-of select="cimtool:csType(@xstype)"/>?<xsl:text> </xsl:text><xsl:value-of select="cimtool:safePropertyName(@name, @xstype, .)"/> { get; set; }</item>
            </xsl:otherwise>
        </xsl:choose>
    </xsl:template>

    <!-- Property template for a:Instance, a:Reference, and a:Compound (navigation properties).
         a:Compound covers compound-type references (e.g. electronicAddress, phone1, status,
         streetDetail) which use the same shadow FK + navigation property pattern as a:Instance.
         The distinction between compound and non-compound types is handled at the SQL DDL level;
         at the C# EF layer both produce identical property declarations.

         Shadow FK nullability:
           @minOccurs='1'  Ã¢â€ â€™  non-nullable string (= null!) Ã¢â‚¬â€ EF Core treats the relationship
                              as required, consistent with the NOT NULL column in the SQL DDL.
           @minOccurs='0'  Ã¢â€ â€™  nullable string? Ã¢â‚¬â€ EF Core treats the relationship as optional,
                              consistent with the nullable column in the SQL DDL.
         In EF Core 6+, a non-nullable FK property automatically implies a required relationship
         without needing an explicit .IsRequired() call in the Fluent API. -->
    <xsl:template match="a:Instance|a:Reference|a:Compound">
        <xsl:call-template name="annotate"/>
        <xsl:choose>
            <!-- Single navigation property when maxOccurs is missing or equals '1' -->
            <xsl:when test="not(@maxOccurs) or @maxOccurs = '1'">
                <!-- Shadow FK property Ã¢â‚¬â€ nullability driven by minOccurs -->
                <item>[Column(&quot;<xsl:value-of select="@name"/>&quot;)]</item>
                <item>[MaxLength(100)]</item>
                <xsl:choose>
                    <xsl:when test="@minOccurs = '1'">
                        <item>public string <xsl:value-of select="cimtool:capitalize(@name)"/>Id { get; set; } = null!;</item>
                    </xsl:when>
                    <xsl:otherwise>
                        <item>public string? <xsl:value-of select="cimtool:capitalize(@name)"/>Id { get; set; }</item>
                    </xsl:otherwise>
                </xsl:choose>
                <!-- Navigation property Ã¢â‚¬â€ [ForeignKey] points at the shadow property above, not the column -->
                <item>[ForeignKey(nameof(<xsl:value-of select="cimtool:capitalize(@name)"/>Id))]</item>
                <item>public virtual <xsl:value-of select="@type"/>?<xsl:text> </xsl:text><xsl:value-of select="cimtool:capitalize(@name)"/> { get; set; }</item>
            </xsl:when>
            <!-- Unbounded collection (maxOccurs > '1' or 'unbounded'): intentionally suppressed.
                 ICollection<T> navigation properties are never generated Ã¢â‚¬â€ see the assembly-level
                 <remarks> block at the top of this file for the full rationale. The FK column
                 lives on the child table; the relationship must be configured from the child side
                 via Fluent API. A self-documenting comment block is emitted in the generated
                 source in place of the suppressed property so that implementers know the
                 association exists and are shown the correct EF Core query pattern. -->
            <xsl:otherwise>
                <xsl:variable name="childType"      select="string(@type)"/>
                <xsl:variable name="assocName"      select="cimtool:capitalize(@name)"/>
                <xsl:variable name="parentType"     select="string(parent::*/@name)"/>
                <xsl:variable name="inversePropName"
                    select="cimtool:capitalize(tokenize(@inverseBaseProperty, '[\.#]')[last()])"/>
                <xsl:variable name="parentPK"
                    select="if (cimtool:has-mrid-ancestor(parent::*)) then 'MRId' else 'Id'"/>
                <item></item>
				<list begin="" indent="// " end="">
					<item>Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬</item>
					<item>Suppressed Collection Navigation: <xsl:value-of select="$parentType"/> Ã¢â€ â€™ <xsl:value-of select="$childType"/> [<xsl:value-of select="@minOccurs"/>..*]</item>
					<item>The profile declares a [<xsl:value-of select="@minOccurs"/>..*] association on this class to <xsl:value-of select="$childType"/>.</item>
					<item>A collection navigation property has been intentionally suppressed:</item>
					<item></item>
					<list begin="" indent="  " end="">
						<item>// NOT generated</item>
						<item>public virtual ICollection&lt;<xsl:value-of select="$childType"/>&gt; <xsl:value-of select="$assocName"/> { get; set; }</item>
					</list>
					<item></item>
					<item>See the assembly-level &lt;remarks&gt; block at the top of this file for the full rationale.</item>
					<item>In summary: unbounded collections risk loading entire result sets into memory with no</item>
					<item>pagination, are not consistently present across profiles, and introduce serialisation</item>
					<item>and change-tracking fragility. Instead, use an explicit LINQ query such as:</item>
					<item></item>
					<item>Basic traversal:</item>
					<list begin="" indent="  " end="">
						<item>IQueryable&lt;<xsl:value-of select="$childType"/>&gt; results = ctx.Set&lt;<xsl:value-of select="$childType"/>&gt;()</item>
						<list begin="" indent="  " end="">
							<item>.Where(x => x.<xsl:value-of select="$inversePropName"/>Id == this.<xsl:value-of select="$parentPK"/>);</item>
						</list>
					</list>
					<item></item>
					<item>Paginated traversal:</item>
					<list begin="" indent="  " end="">
						<item>var page = await ctx.Set&lt;<xsl:value-of select="$childType"/>&gt;()</item>
						<list begin="" indent="  " end="">
							<item>.Where(x => x.<xsl:value-of select="$inversePropName"/>Id == this.<xsl:value-of select="$parentPK"/>)</item>
							<item>.OrderBy(x => x.MRId)</item>
							<item>.OrderBy(x => x.MRId)</item>
							<item>.Skip(offset).Take(pageSize)</item>
							<item>.ToListAsync();</item>
						</list>
					</list>
					<item>Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬</item>
				</list>
				<item></item>
            </xsl:otherwise>
        </xsl:choose>
    </xsl:template>

    <!-- Property template for a:Enumerated (enumerated property as a FK string reference) -->
    <xsl:template match="a:Enumerated">
        <xsl:call-template name="annotate"/>
        <item>[Column(&quot;<xsl:value-of select="@name"/>&quot;)]</item>
        <item>[MaxLength(100)]</item>
        <xsl:choose>
            <!-- Required enumerated reference Ã¢â‚¬â€ minOccurs = 1 -->
            <xsl:when test="@minOccurs = '1'">
                <item>public string <xsl:value-of select="cimtool:safePropertyName(@name, '', .)"/> { get; set; } = null!;</item>
            </xsl:when>
            <!-- Optional enumerated reference Ã¢â‚¬â€ minOccurs = 0 or absent -->
            <xsl:otherwise>
                <item>public string? <xsl:value-of select="cimtool:safePropertyName(@name, '', .)"/> { get; set; }</item>
            </xsl:otherwise>
        </xsl:choose>
    </xsl:template>
    
	<!-- 
		Template for enumerated values inside an EnumeratedType.
		cimtool:safeIdentifier() is used rather than cimtool:capitalize() to preserve
		case, preventing duplicate const identifiers for enum values that differ
		only in case (e.g. UnitSymbol 'H' henry vs 'h' hour). 
	-->
	<xsl:template match="a:EnumeratedValue">
    	<!-- 
    		We very intentionally do NOT call capitalize as some enumerations (e.g. UnitSymbol) can have h and H as two
    		distinct literals. This introduces compilation issues in C# so we honor the case sensitivity in that scenario.  
    	-->
	    <item>public const string <xsl:value-of select="cimtool:safeIdentifier(@name)"/> = &quot;<xsl:value-of select="@name"/>&quot;;</item>
	</xsl:template>

    <!--
        Configuration block listing all classes in topological order.
        Receives pre-computed sorted sequences from the a:Catalog template
        to avoid rebuilding the dependency maps a second time.
    -->
    <xsl:template name="config">
        <xsl:param name="sorted-compounds" as="element()*"/>
        <xsl:param name="sorted-classes"   as="element()*"/>
        <item>public static readonly System.Type[] allClasses = new System.Type[]</item>
        <list begin="{{" indent="    " delim="," end="}};">
            <!-- Tier 1: EnumeratedType Ã¢â‚¬â€ lookup tables, no dependencies -->
            <xsl:apply-templates select="a:EnumeratedType" mode="config"/>
            <!-- Tier 2: CompoundType Ã¢â‚¬â€ in topological dependency order -->
            <xsl:apply-templates select="$sorted-compounds" mode="config"/>
            <!-- Tier 3: ComplexType + Root Ã¢â‚¬â€ superclasses before subclasses -->
            <xsl:apply-templates select="$sorted-classes" mode="config"/>
        </list>
    </xsl:template>

    <!--
        Generates the ModelConfiguration nested static class containing the complete
        EF Core Fluent API configuration for all entities in the profile.
        Receives pre-computed sorted sequences from the a:Catalog template to avoid
        rebuilding the dependency maps a third time.

        Structure of the generated class:
          - One public static ConfigureModel() entry point that calls a private
            static method per entity in topological order.
          - One private static Configure<EntityName>() method per entity containing:
              ToTable() for explicit TPT mapping.
              HasOne().WithMany().HasForeignKey().OnDelete(...) for every
              a:Compound, a:Instance, and a:Reference child relationship, with
              DeleteBehavior differentiated by relationship kind (see below).
          - Entities with no FK relationships use a concise arrow expression body.
          - Entities with FK relationships use a block body with a local variable.

        CASCADE VS CLIENTNOACTION Ã¢â‚¬â€ DESIGN RATIONALE
        Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬
        Two distinct DeleteBehavior values are emitted depending on relationship kind:

          a:Compound:  DeleteBehavior.Cascade
            Compound types (e.g. ElectronicAddress, Status, TelephoneNumber) are CIM
            value objects Ã¢â‚¬â€ each compound row has exactly one owner and its lifecycle
            is fully subordinate to the parent. The companion SQL DDL emits ON DELETE
            CASCADE for every compound FK, so DeleteBehavior.Cascade is required here
            to keep EF Core's in-memory change tracker consistent with the database.
            Without it, deleting a tracked parent entity while a compound child is
            also tracked causes EF Core to throw a referential integrity exception
            before the delete reaches the DB, because ClientNoAction leaves the
            orphaned child in the change tracker with a dangling FK reference.

          a:Instance | a:Reference:  DeleteBehavior.ClientNoAction
            Associations between independent IdentifiedObject entities carry no DB-level
            cascade. Deleting one end of such a relationship (e.g. a Substation) must
            not silently delete the other (e.g. a SubGeographicalRegion). EF Core's
            ClientNoAction leaves cascade responsibility entirely to the application,
            which is correct and safe for these semantically independent associations.
    -->
    <xsl:template name="dbcontext">
        <xsl:param name="sorted-compounds" as="element()*"/>
        <xsl:param name="sorted-classes"   as="element()*"/>
        <list begin="" indent="/// " end="">
            <list begin="&lt;summary&gt;" indent="" end="&lt;/summary&gt;">
                <item>Generated EF Core Fluent API configuration for the <xsl:value-of select="$envelope"/> profile.</item>
                <item>Apply this from your hand-written DbContext.OnModelCreating override - see the</item>
                <item>file header comment for the recommended usage pattern.</item>
                <item>This class is fully regenerated by CIMTool - do not edit manually.</item>
            </list>
            <list begin="&lt;remarks&gt;" indent="" end="&lt;/remarks&gt;">
                <list begin="&lt;para&gt;" indent="" end="&lt;/para&gt;">
                    <item>&lt;b&gt;Cascade vs ClientNoAction Delete Behavior&lt;/b&gt;&lt;br/&gt;</item>
                    <item>Two distinct &lt;c&gt;DeleteBehavior&lt;/c&gt; values are configured depending on</item>
                    <item>relationship kind:</item>
                    <list begin="&lt;list type=&quot;bullet&quot;&gt;" indent="   " end=" &lt;/list&gt;">
                        <list begin="&lt;item&gt;" indent="  " end="&lt;/item&gt;">
                            <item>&lt;c&gt;DeleteBehavior.Cascade&lt;/c&gt; - CIM Compound type references</item>
                            <item>(e.g. &lt;c&gt;ElectronicAddress&lt;/c&gt;, &lt;c&gt;Status&lt;/c&gt;, &lt;c&gt;TelephoneNumber&lt;/c&gt;).</item>
                            <item>Compound rows are value objects with exactly one owner. The companion</item>
                            <item>SQL DDL emits &lt;c&gt;ON DELETE CASCADE&lt;/c&gt; for these FKs.</item>
                            <item>&lt;c&gt;DeleteBehavior.Cascade&lt;/c&gt; keeps EF Core's relationship metadata and</item>
                            <item>tracked graph behavior aligned. Owner-side orphan cleanup is handled</item>
                            <item>by the generated &lt;c&gt;DbContextBase&lt;/c&gt; when consumers derive from it.</item>
                        </list>
                        <list begin="&lt;item&gt;" indent="  " end="&lt;/item&gt;">
                            <item>&lt;c&gt;DeleteBehavior.ClientNoAction&lt;/c&gt; - associations between independent</item>
                            <item>IdentifiedObject entities. These carry no DB-level cascade and deleting</item>
                            <item>one end must not silently delete the other. Cascade responsibility is</item>
                            <item>left entirely to the application.</item>
                        </list>
                    </list>
                </list>
            </list>
        </list>
        <item>public static class ModelConfiguration</item>
        <list begin="{{" indent="    " delim="" end="}}">
            <!-- Entry point Ã¢â‚¬â€ calls one private method per entity in topological order -->
            <item>public static void ConfigureModel(ModelBuilder modelBuilder)</item>
            <list begin="{{" indent="    " delim="" end="}}">
                <!-- Tier 1: EnumeratedType -->
                <xsl:for-each select="a:EnumeratedType">
                    <item>Configure<xsl:value-of select="@name"/>(modelBuilder);</item>
                </xsl:for-each>
                <!-- Tier 2: CompoundType in topological dependency order -->
                <xsl:for-each select="$sorted-compounds">
                    <item>Configure<xsl:value-of select="@name"/>(modelBuilder);</item>
                </xsl:for-each>
                <!-- Tier 3: ComplexType + Root Ã¢â‚¬â€ superclasses before subclasses -->
                <xsl:for-each select="$sorted-classes">
                    <item>Configure<xsl:value-of select="@name"/>(modelBuilder);</item>
                </xsl:for-each>
            </list>
            <!-- Private configuration methods Ã¢â‚¬â€ one per entity -->
            <xsl:apply-templates select="a:EnumeratedType" mode="dbcontext"/>
            <xsl:apply-templates select="$sorted-compounds" mode="dbcontext"/>
            <xsl:apply-templates select="$sorted-classes" mode="dbcontext"/>
        </list>
    </xsl:template>

    <!--
        Generates a private static configuration method for a single entity.
        Entities with no FK relationships use a concise arrow expression body.
        Entities with a:Compound, a:Instance, or a:Reference children use a
        block body with a local variable to chain the relationship configuration.

        DeleteBehavior is differentiated by relationship kind:
          a:Compound   Ã¢â€ â€™  DeleteBehavior.Cascade
            Compound type rows are value objects owned exclusively by their parent.
            The companion SQL DDL emits ON DELETE CASCADE for these FKs. Cascade
            must be mirrored here so that EF Core's change tracker handles deletion
            of a tracked parent consistently Ã¢â‚¬â€ ClientNoAction would cause EF Core
            to throw a referential integrity exception when a tracked compound child
            is orphaned in memory, before the delete can reach the DB and trigger
            the SQL-level cascade.
          a:Instance | a:Reference  Ã¢â€ â€™  DeleteBehavior.ClientNoAction
            Independent IdentifiedObject associations carry no DB-level cascade.
            Deleting one end must not silently remove the other; application code
            is responsible for managing the lifecycle of associated entities.
    -->
    <xsl:template match="a:EnumeratedType|a:CompoundType|a:ComplexType|a:Root" mode="dbcontext">
        <xsl:variable name="fks" select="a:Compound|a:Instance|a:Reference"/>
        <xsl:choose>
            <!-- No FK relationships Ã¢â‚¬â€ concise arrow expression body -->
            <xsl:when test="not($fks)">
                <item>private static void Configure<xsl:value-of select="@name"/>(ModelBuilder modelBuilder)</item>
                <item>    => modelBuilder.Entity&lt;<xsl:value-of select="@name"/>&gt;().ToTable(&quot;<xsl:value-of select="@name"/>&quot;);</item>
            </xsl:when>
            <!-- Has FK relationships Ã¢â‚¬â€ block body with local variable -->
            <xsl:otherwise>
                <item>private static void Configure<xsl:value-of select="@name"/>(ModelBuilder modelBuilder)</item>
                <list begin="{{" indent="    " delim="" end="}}">
                    <item>var e = modelBuilder.Entity&lt;<xsl:value-of select="@name"/>&gt;();</item>
                    <item>e.ToTable(&quot;<xsl:value-of select="@name"/>&quot;);</item>
                    <xsl:for-each select="$fks">
                        <xsl:choose>
                            <!-- Compound: value object Ã¢â‚¬â€ Cascade mirrors ON DELETE CASCADE in companion DDL.
                                 See the ModelConfiguration class <remarks> for the full design rationale. -->
                            <xsl:when test="self::a:Compound">
                                <item>e.HasOne(x => x.<xsl:value-of select="cimtool:capitalize(@name)"/>).WithMany().HasForeignKey(x => x.<xsl:value-of select="cimtool:capitalize(@name)"/>Id).OnDelete(DeleteBehavior.Cascade);</item>
                            </xsl:when>
                            <!-- Unbounded Reference [0..*]: FK lives on the child table.
                                 No HasOne/HasMany is emitted on this (parent) side Ã¢â‚¬â€ doing so
                                 would reference a shadow FK property that does not exist on this
                                 entity. The relationship must be configured from the child side.
                                 A comment is emitted here to make that expectation explicit and
                                 to provide a copy-pasteable Fluent API starting point. -->
                            <xsl:when test="self::a:Reference and (@maxOccurs='unbounded' or number(@maxOccurs) > 1)">
                                <xsl:variable name="childType" select="string(@type)"/>
                                <xsl:variable name="parentType" select="string(parent::*/@name)"/>
                                <xsl:variable name="inversePropName" select="cimtool:capitalize(tokenize(@inverseBaseProperty, '[\.#]')[last()])"/>
                                <item></item>
								<list begin="" indent="// " end="">
									<item>Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬</item>
									<item>Suppressed Fluent API:  <xsl:value-of select="$parentType"/> Ã¢â€ â€™ <xsl:value-of select="$childType"/>  [<xsl:value-of select="@minOccurs"/>..*]</item>
									<item>The FK column for this [<xsl:value-of select="@minOccurs"/>..*] association lives on <xsl:value-of select="$childType"/>, not here.</item>
									<item>HasOne/HasMany is intentionally not configured on this parent side because</item>
									<item>no shadow FK property exists on <xsl:value-of select="$parentType"/> (see the suppressed collection</item>
									<item>navigation comment in the entity class above). Instead, configure from the child side:</item>
									<item></item>
									<list begin="" indent="  " end="">
										<item>modelBuilder.Entity&lt;<xsl:value-of select="$childType"/>&gt;()</item>
										<list begin="" indent="  " end="">
											<item>.HasOne(x => x.<xsl:value-of select="$inversePropName"/>)</item>
											<item>.WithMany()</item>
											<item>.HasForeignKey(x => x.<xsl:value-of select="$inversePropName"/>Id)</item>
											<item>.OnDelete(DeleteBehavior.ClientNoAction);</item>
										</list>
									</list>
									<item>Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬</item>
								</list>
                            </xsl:when>
                            <!-- Instance/Reference: independent entity Ã¢â‚¬â€ no cascade -->
                            <xsl:otherwise>
                                <item>e.HasOne(x => x.<xsl:value-of select="cimtool:capitalize(@name)"/>).WithMany().HasForeignKey(x => x.<xsl:value-of select="cimtool:capitalize(@name)"/>Id).OnDelete(DeleteBehavior.ClientNoAction);</item>
                            </xsl:otherwise>
                        </xsl:choose>
                    </xsl:for-each>
                </list>
            </xsl:otherwise>
        </xsl:choose>
    </xsl:template>

    <!--
        Generates an abstract DbContextBase nested inside the generated profile class.

        The goal is to move profile-specific EF boilerplate into generated code:
          - DbSet properties for each generated entity type
          - OnModelCreating() wiring into ModelConfiguration
          - SaveChanges / SaveChangesAsync cleanup hooks
          - orphan collection and nested compound cleanup logic derived directly
            from a:Compound relationships in the profile

        Consumers keep only a thin hand-written subclass that provides constructor
        wiring and any application-specific customisation.
    -->
    <xsl:template name="dbcontext-base">
        <xsl:param name="sorted-compounds" as="element()*"/>
        <xsl:param name="sorted-classes"   as="element()*"/>
        <xsl:variable name="entities-with-compounds" select="($sorted-compounds, $sorted-classes)[a:Compound]"/>

        <list begin="" indent="/// " end="">
            <list begin="&lt;summary&gt;" indent="" end="&lt;/summary&gt;">
                <item>Generated EF Core DbContext base for the <xsl:value-of select="$envelope"/> profile.</item>
                <item>This base class provides generated DbSet properties, model wiring,</item>
                <item>and compound orphan cleanup hooks derived from the profile itself.</item>
            </list>
            <list begin="&lt;remarks&gt;" indent="" end="&lt;/remarks&gt;">
                <list begin="&lt;para&gt;" indent="" end="&lt;/para&gt;">
                    <item>Create a thin hand-written subclass in your application:</item>
                </list>
                <list begin="&lt;code&gt;" indent="" end="&lt;/code&gt;">
                    <item>public class <xsl:value-of select="$envelope"/>DbContext : <xsl:value-of select="$envelope"/>.DbContextBase</item>
                    <list begin="{{" indent="    " end="}}">
                        <item>public <xsl:value-of select="$envelope"/>DbContext(DbContextOptions options) : base(options) { }</item>
                    </list>
                </list>
            </list>
        </list>
        <item>public abstract class DbContextBase : DbContext</item>
        <list begin="{{" indent="    " delim="" end="}}">
            <item>protected DbContextBase(DbContextOptions options) : base(options) { }</item>

            <xsl:for-each select="a:EnumeratedType">
                <item>public DbSet&lt;<xsl:value-of select="$envelope"/>.<xsl:value-of select="@name"/>&gt; <xsl:value-of select="cimtool:pluralize(@name)"/> =&gt; Set&lt;<xsl:value-of select="$envelope"/>.<xsl:value-of select="@name"/>&gt;();</item>
            </xsl:for-each>
            <xsl:for-each select="$sorted-compounds">
                <item>public DbSet&lt;<xsl:value-of select="$envelope"/>.<xsl:value-of select="@name"/>&gt; <xsl:value-of select="cimtool:pluralize(@name)"/> =&gt; Set&lt;<xsl:value-of select="$envelope"/>.<xsl:value-of select="@name"/>&gt;();</item>
            </xsl:for-each>
            <xsl:for-each select="$sorted-classes">
                <item>public DbSet&lt;<xsl:value-of select="$envelope"/>.<xsl:value-of select="@name"/>&gt; <xsl:value-of select="cimtool:pluralize(@name)"/> =&gt; Set&lt;<xsl:value-of select="$envelope"/>.<xsl:value-of select="@name"/>&gt;();</item>
            </xsl:for-each>

            <item>protected override void OnModelCreating(ModelBuilder modelBuilder)</item>
            <item>    =&gt; <xsl:value-of select="$envelope"/>.ModelConfiguration.ConfigureModel(modelBuilder);</item>

            <item>public override int SaveChanges()</item>
            <item>    =&gt; SaveChangesWithCleanup(() =&gt; base.SaveChanges());</item>

            <item>public override int SaveChanges(bool acceptAllChangesOnSuccess)</item>
            <item>    =&gt; SaveChangesWithCleanup(() =&gt; base.SaveChanges(acceptAllChangesOnSuccess));</item>

            <item>public override Task&lt;int&gt; SaveChangesAsync(CancellationToken cancellationToken = default)</item>
            <item>    =&gt; SaveChangesWithCleanupAsync(ct =&gt; base.SaveChangesAsync(ct), cancellationToken);</item>

            <item>public override Task&lt;int&gt; SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)</item>
            <item>    =&gt; SaveChangesWithCleanupAsync(ct =&gt; base.SaveChangesAsync(acceptAllChangesOnSuccess, ct), cancellationToken);</item>

            <item>private int SaveChangesWithCleanup(Func&lt;int&gt; baseSaveChanges)</item>
            <list begin="{{" indent="    " delim="" end="}}">
                <item>ArgumentNullException.ThrowIfNull(baseSaveChanges);</item>
                <item>ChangeTracker.DetectChanges();</item>
                <item>var candidates = CollectCleanupCandidates();</item>
                <item>var rows = baseSaveChanges();</item>
                <item>if (candidates.Count == 0)</item>
                <list begin="{{" indent="    " delim="" end="}}">
                    <item>return rows;</item>
                </list>
                <item>if (!MarkOrphanedCompoundsForDeletion(candidates))</item>
                <list begin="{{" indent="    " delim="" end="}}">
                    <item>return rows;</item>
                </list>
                <item>rows += baseSaveChanges();</item>
                <item>return rows;</item>
            </list>

            <item>private async Task&lt;int&gt; SaveChangesWithCleanupAsync(Func&lt;CancellationToken, Task&lt;int&gt;&gt; baseSaveChangesAsync, CancellationToken cancellationToken)</item>
            <list begin="{{" indent="    " delim="" end="}}">
                <item>ArgumentNullException.ThrowIfNull(baseSaveChangesAsync);</item>
                <item>ChangeTracker.DetectChanges();</item>
                <item>var candidates = CollectCleanupCandidates();</item>
                <item>var rows = await baseSaveChangesAsync(cancellationToken);</item>
                <item>if (candidates.Count == 0)</item>
                <list begin="{{" indent="    " delim="" end="}}">
                    <item>return rows;</item>
                </list>
                <item>if (!MarkOrphanedCompoundsForDeletion(candidates))</item>
                <list begin="{{" indent="    " delim="" end="}}">
                    <item>return rows;</item>
                </list>
                <item>rows += await baseSaveChangesAsync(cancellationToken);</item>
                <item>return rows;</item>
            </list>

            <item>private List&lt;CompoundCleanupCandidate&gt; CollectCleanupCandidates()</item>
            <list begin="{{" indent="    " delim="" end="}}">
                <item>var candidates = new List&lt;CompoundCleanupCandidate&gt;();</item>
                <xsl:for-each select="$entities-with-compounds">
                    <item>foreach (var entry in ChangeTracker.Entries&lt;<xsl:value-of select="$envelope"/>.<xsl:value-of select="@name"/>&gt;())</item>
                    <list begin="{{" indent="    " delim="" end="}}">
                        <item>if (entry.State is not EntityState.Modified and not EntityState.Deleted)</item>
                        <list begin="{{" indent="    " delim="" end="}}">
                            <item>continue;</item>
                        </list>
                        <xsl:for-each select="a:Compound">
                            <item>CollectChangedCompound(entry, nameof(<xsl:value-of select="$envelope"/>.<xsl:value-of select="parent::*/@name"/>.<xsl:value-of select="cimtool:capitalize(@name)"/>Id), typeof(<xsl:value-of select="$envelope"/>.<xsl:value-of select="@type"/>), candidates);</item>
                        </xsl:for-each>
                    </list>
                </xsl:for-each>
                <item>return candidates;</item>
            </list>

            <item>private static void CollectChangedCompound&lt;TEntity&gt;(Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry&lt;TEntity&gt; entry, string propertyName, Type compoundType, List&lt;CompoundCleanupCandidate&gt; candidates)</item>
            <item>    where TEntity : class</item>
            <list begin="{{" indent="    " delim="" end="}}">
                <item>var originalId = entry.OriginalValues[propertyName] as string;</item>
                <item>var currentId = entry.State == EntityState.Deleted ? null : entry.CurrentValues[propertyName] as string;</item>
                <item>if (!string.IsNullOrWhiteSpace(originalId) &amp;&amp; !string.Equals(originalId, currentId, StringComparison.Ordinal))</item>
                <list begin="{{" indent="    " delim="" end="}}">
                    <item>candidates.Add(new CompoundCleanupCandidate(compoundType, originalId));</item>
                </list>
            </list>

            <item>private bool MarkOrphanedCompoundsForDeletion(IReadOnlyCollection&lt;CompoundCleanupCandidate&gt; candidates)</item>
            <list begin="{{" indent="    " delim="" end="}}">
                <item>var plannedDeletes = new HashSet&lt;CompoundCleanupCandidate&gt;();</item>
                <item>var visiting = new HashSet&lt;CompoundCleanupCandidate&gt;();</item>
                <item>foreach (var candidate in candidates)</item>
                <list begin="{{" indent="    " delim="" end="}}">
                    <item>PlanCompoundDeletion(candidate, plannedDeletes, visiting);</item>
                </list>
                <item>var deletedAny = false;</item>
                <item>foreach (var candidate in plannedDeletes)</item>
                <list begin="{{" indent="    " delim="" end="}}">
                    <item>deletedAny |= MarkCompoundForDeletion(candidate);</item>
                </list>
                <item>return deletedAny;</item>
            </list>

            <item>private void PlanCompoundDeletion(CompoundCleanupCandidate candidate, HashSet&lt;CompoundCleanupCandidate&gt; plannedDeletes, HashSet&lt;CompoundCleanupCandidate&gt; visiting)</item>
            <list begin="{{" indent="    " delim="" end="}}">
                <item>if (string.IsNullOrWhiteSpace(candidate.Id) || plannedDeletes.Contains(candidate) || !visiting.Add(candidate))</item>
                <list begin="{{" indent="    " delim="" end="}}">
                    <item>return;</item>
                </list>
                <item>if (IsStillReferenced(candidate, plannedDeletes))</item>
                <list begin="{{" indent="    " delim="" end="}}">
                    <item>visiting.Remove(candidate);</item>
                    <item>return;</item>
                </list>
                <item>plannedDeletes.Add(candidate);</item>
                <item>foreach (var child in GetNestedCompoundCandidates(candidate))</item>
                <list begin="{{" indent="    " delim="" end="}}">
                    <item>PlanCompoundDeletion(child, plannedDeletes, visiting);</item>
                </list>
                <item>visiting.Remove(candidate);</item>
            </list>

            <item>private bool IsStillReferenced(CompoundCleanupCandidate candidate, HashSet&lt;CompoundCleanupCandidate&gt; plannedDeletes)</item>
            <list begin="{{" indent="    " delim="" end="}}">
                <xsl:for-each select="$sorted-compounds">
                    <xsl:variable name="compound-name" select="string(@name)"/>
                    <xsl:variable name="root-complex-parents" select="$sorted-classes[a:Compound[@type = $compound-name]]"/>
                    <xsl:variable name="compound-parents" select="$sorted-compounds[a:Compound[@type = $compound-name]]"/>
                    <item>if (candidate.CompoundType == typeof(<xsl:value-of select="$envelope"/>.<xsl:value-of select="@name"/>))</item>
                    <list begin="{{" indent="    " delim="" end="}}">
                        <item>var isReferenced = false;</item>
                        <xsl:for-each select="$root-complex-parents">
                            <xsl:variable name="predicate" as="xs:string"
                                select="string-join(
                                    for $child in a:Compound[@type = $compound-name]
                                    return concat('x.', cimtool:capitalize(string($child/@name)), 'Id == candidate.Id'),
                                    ' || '
                                )"/>
                            <item>isReferenced = isReferenced || Set&lt;<xsl:value-of select="$envelope"/>.<xsl:value-of select="@name"/>&gt;().Any(x =&gt; <xsl:value-of select="$predicate"/>);</item>
                        </xsl:for-each>
                        <xsl:for-each select="$compound-parents">
                            <xsl:variable name="predicate" as="xs:string"
                                select="string-join(
                                    for $child in a:Compound[@type = $compound-name]
                                    return concat('x.', cimtool:capitalize(string($child/@name)), 'Id == candidate.Id'),
                                    ' || '
                                )"/>
                            <item>isReferenced = isReferenced || Set&lt;<xsl:value-of select="$envelope"/>.<xsl:value-of select="@name"/>&gt;()</item>
                            <list begin="" indent="    " delim="" end="">
                                <item>.Where(x =&gt; <xsl:value-of select="$predicate"/>)</item>
                                <item>.Select(x =&gt; x.Id)</item>
                                <item>.AsEnumerable()</item>
                                <item>.Any(id =&gt; !plannedDeletes.Contains(new CompoundCleanupCandidate(typeof(<xsl:value-of select="$envelope"/>.<xsl:value-of select="@name"/>), id)));</item>
                            </list>
                        </xsl:for-each>
                        <item>return isReferenced;</item>
                    </list>
                </xsl:for-each>
                <item>return false;</item>
            </list>

            <item>private IEnumerable&lt;CompoundCleanupCandidate&gt; GetNestedCompoundCandidates(CompoundCleanupCandidate candidate)</item>
            <list begin="{{" indent="    " delim="" end="}}">
                <xsl:for-each select="$sorted-compounds[a:Compound]">
                    <item>if (candidate.CompoundType == typeof(<xsl:value-of select="$envelope"/>.<xsl:value-of select="@name"/>))</item>
                    <list begin="{{" indent="    " delim="" end="}}">
                        <item>var entity = Set&lt;<xsl:value-of select="$envelope"/>.<xsl:value-of select="@name"/>&gt;().SingleOrDefault(x =&gt; x.Id == candidate.Id);</item>
                        <item>if (entity is null)</item>
                        <list begin="{{" indent="    " delim="" end="}}">
                            <item>return Array.Empty&lt;CompoundCleanupCandidate&gt;();</item>
                        </list>
                        <item>var children = new List&lt;CompoundCleanupCandidate&gt;();</item>
                        <xsl:for-each select="a:Compound">
                            <item>AddIfPresent(children, typeof(<xsl:value-of select="$envelope"/>.<xsl:value-of select="@type"/>), entity.<xsl:value-of select="cimtool:capitalize(@name)"/>Id);</item>
                        </xsl:for-each>
                        <item>return children;</item>
                    </list>
                </xsl:for-each>
                <item>return Array.Empty&lt;CompoundCleanupCandidate&gt;();</item>
            </list>

            <item>private static void AddIfPresent(List&lt;CompoundCleanupCandidate&gt; children, Type type, string? id)</item>
            <list begin="{{" indent="    " delim="" end="}}">
                <item>if (!string.IsNullOrWhiteSpace(id))</item>
                <list begin="{{" indent="    " delim="" end="}}">
                    <item>children.Add(new CompoundCleanupCandidate(type, id));</item>
                </list>
            </list>

            <item>private bool MarkCompoundForDeletion(CompoundCleanupCandidate candidate)</item>
            <list begin="{{" indent="    " delim="" end="}}">
                <xsl:for-each select="$sorted-compounds">
                    <item>if (candidate.CompoundType == typeof(<xsl:value-of select="$envelope"/>.<xsl:value-of select="@name"/>))</item>
                    <list begin="{{" indent="    " delim="" end="}}">
                        <item>return DeleteIfPresent(Set&lt;<xsl:value-of select="$envelope"/>.<xsl:value-of select="@name"/>&gt;(), x =&gt; x.Id == candidate.Id);</item>
                    </list>
                </xsl:for-each>
                <item>return false;</item>
            </list>

            <item>private bool DeleteIfPresent&lt;TEntity&gt;(DbSet&lt;TEntity&gt; set, Func&lt;TEntity, bool&gt; predicate)</item>
            <item>    where TEntity : class</item>
            <list begin="{{" indent="    " delim="" end="}}">
                <item>var entity = set.Local.FirstOrDefault(predicate) ?? set.AsEnumerable().FirstOrDefault(predicate);</item>
                <item>if (entity is null)</item>
                <list begin="{{" indent="    " delim="" end="}}">
                    <item>return false;</item>
                </list>
                <item>Remove(entity);</item>
                <item>return true;</item>
            </list>

            <item>private readonly record struct CompoundCleanupCandidate(Type CompoundType, string Id);</item>
        </list>
    </xsl:template>

    <!--
        Generates a C# XML doc comment block from any a:Comment or a:Note
        children on the current element. Suppressed entirely when no such
        children exist, avoiding empty doc comment blocks on elements like
        ParentOrganization that carry no description in the profile.

        Pattern mirrors the file-level header:
          - Outer <list begin="" indent="/// " end=""> applies the /// prefix
            to every line without emitting spurious blank /// lines at the
            begin and end positions.
          - a:Comment content is wrapped in <summary>...</summary>.
          - a:Note content, when present, is wrapped in <remarks>...</remarks>.
          - Each XML doc tag is its own inner list so the open and close tags
            each land on their own /// -prefixed line.
          - Text content uses <wrap width="70"> to honour the column limit.
    -->
    <xsl:template name="annotate">
        <xsl:if test="a:Comment or a:Note">
            <list begin="" indent="/// " end="">
                <xsl:if test="a:Comment">
                    <list begin="&lt;summary&gt;" indent="" end="&lt;/summary&gt;">
                        <xsl:apply-templates select="a:Comment" mode="annotate"/>
                    </list>
                </xsl:if>
                <xsl:if test="a:Note">
                    <list begin="&lt;remarks&gt;" indent="" end="&lt;/remarks&gt;">
                        <xsl:apply-templates select="a:Note" mode="annotate"/>
                    </list>
                </xsl:if>
            </list>
        </xsl:if>
    </xsl:template>

    <xsl:template match="a:Comment|a:Note" mode="annotate">
        <wrap width="70">
            <xsl:value-of select="."/>
        </wrap>
    </xsl:template>

    <!--
        Suppresses inverse navigation properties entirely.

        InverseInstance and InverseReference represent the one side of a one-to-many
        association (e.g. "all Equipment belonging to this EquipmentContainer"). These
        are omitted by design, not by accident. The rationale is documented in full in
        the generated file header <remarks> block, but summarized here for maintainers:

          1. CIM associations are unbounded. Materializing an ICollection<T> with no
             pagination is a runtime hazard for large profiles such as CGMES CoreEquipment.
          2. Bidirectional mappings require both sides to be kept in sync on every
             add/remove, adding fragility and bug surface to consumer code.
          3. They introduce circular-reference risk during JSON serialization.
          4. Inverse associations are profile-dependent Ã¢â‚¬â€ not every profile includes them,
             so generated ICollection<T> properties would be inconsistent across profiles.
          5. Explicit LINQ queries (query through the child side via the shadow FK property)
             support pagination, filtering, and sorting that a collection property cannot.

        The correct pattern for consumers is to query through the child side:

            var units = context.Set<PowerElectronicsUnit>()
                .Where(u => u.PowerElectronicsConnectionId == connection.MRId)
                .ToList();

        This template must be explicit (rather than relying on the default text() suppressor)
        so that the intent is self-documenting for future maintainers of this builder.
    -->
    <xsl:template match="a:InverseInstance|a:InverseReference"/>

    <!-- Suppress text nodes -->
    <xsl:template match="text()"/>
    <xsl:template match="node()" mode="config"/>
    <xsl:template match="node()" mode="annotate"/>
    <xsl:template match="node()" mode="dbcontext"/>

</xsl:stylesheet>
