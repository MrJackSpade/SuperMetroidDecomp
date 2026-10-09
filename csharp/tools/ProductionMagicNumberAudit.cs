using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace SuperMetroid.SourceAudit;

/// <summary>
/// Rejects raw hexadecimal domain values in production C# while permitting named data
/// catalogs and a reviewed, exact baseline. The scanner deliberately has no Roslyn or
/// project dependency, so both Verification and DebugRunner can execute it offline.
/// </summary>
internal static partial class ProductionMagicNumberAudit
{
    /// <summary>Repository-relative location of the reviewed fingerprint set loaded by the audit.</summary>
    private const string BaselineRelativePath = "csharp/magic-number-baseline.txt";

    /// <summary>Production source trees whose handwritten functional code is checked for raw hexadecimal values.</summary>
    private static readonly string[] ProductionSourceRoots =
    [
        "csharp/src/SuperMetroid.Core",
        "csharp/src/SuperMetroid.Desktop",
        "csharp/src/SuperMetroid.DebugRunner/Assets",
    ];

    /// <summary>
    /// These suffixes identify files whose purpose is to own reviewed data definitions.
    /// A raw value is permitted there; moving it into one of these named containers is the
    /// expected remediation for a functional use site reported by this audit.
    /// </summary>
    private static readonly string[] ReviewedDefinitionSuffixes =
    [
        "Addresses.cs",
        "Catalog.cs",
        "CodePointers.cs",
        "Codes.cs",
        "Constants.cs",
        "Definitions.cs",
        "Flags.cs",
        "InstructionLists.cs",
        "Layout.cs",
        "Masks.cs",
        "Pointers.cs",
        "RomData.cs",
        "SamusPoseId.cs", // Exclusive native pose enum; its members are definitions.
        "Tables.cs",
        "Values.cs",
        "Words.cs",
    ];

    [GeneratedRegex(@"0[xX][0-9a-fA-F_]+(?:[uUlL]{0,2})", RegexOptions.CultureInvariant)]
    private static partial Regex HexLiteralRegex();

    [GeneratedRegex(@"\bcase\s+0[xX]", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex RawCaseRegex();

    /// <summary>Runs the repository audit and returns every non-baselined finding.</summary>
    public static MagicNumberAuditResult Run(string repositoryRoot)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(repositoryRoot);
        string root = Path.GetFullPath(repositoryRoot);
        string baselinePath = Path.Combine(root, BaselineRelativePath.Replace('/', Path.DirectorySeparatorChar));
        HashSet<string> baseline = ReadBaseline(baselinePath);
        var current = new List<MagicNumberFinding>();

        foreach (string relativeRoot in ProductionSourceRoots)
        {
            string sourceRoot = Path.Combine(root, relativeRoot.Replace('/', Path.DirectorySeparatorChar));
            if (!Directory.Exists(sourceRoot))
                throw new DirectoryNotFoundException($"Magic-number audit source root is missing: {sourceRoot}");

            foreach (string path in Directory.EnumerateFiles(sourceRoot, "*.cs", SearchOption.AllDirectories))
            {
                if (path.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase) ||
                    path.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }
                AuditFile(root, path, current);
            }
        }

        MagicNumberFinding[] newFindings = current
            .Where(finding => !baseline.Contains(finding.BaselineFingerprint))
            .OrderBy(finding => finding.RelativePath, StringComparer.Ordinal)
            .ThenBy(finding => finding.LineNumber)
            .ThenBy(finding => finding.Literal, StringComparer.Ordinal)
            .ToArray();
        int retiredBaselineEntries = baseline.Count(signature =>
            current.All(finding => finding.BaselineFingerprint != signature));
        return new MagicNumberAuditResult(
            current.Count,
            baseline.Count,
            retiredBaselineEntries,
            current,
            newFindings);
    }

    /// <summary>
    /// Serializes reviewed findings as sorted 64-bit SHA-256 prefixes. The compact binary
    /// set retains per-finding membership (and therefore identifies additions) without a
    /// thousand-line textual debt file. A prefix collision is computationally negligible.
    /// </summary>
    public static string CreateBaselinePayload(IEnumerable<MagicNumberFinding> findings)
    {
        string[] fingerprints = findings
            .Select(finding => finding.BaselineFingerprint)
            .Distinct(StringComparer.Ordinal)
            .OrderBy(value => value, StringComparer.Ordinal)
            .ToArray();
        var bytes = new byte[fingerprints.Length * 8];
        for (int index = 0; index < fingerprints.Length; index++)
            Convert.FromHexString(fingerprints[index]).CopyTo(bytes, index * 8);
        return $"count={fingerprints.Length}{Environment.NewLine}" +
            $"fingerprints={Convert.ToBase64String(bytes)}";
    }

    /// <summary>Finds the repository root from a command's current or output directory.</summary>
    public static string FindRepositoryRoot()
    {
        string? fromCurrentDirectory = FindRepositoryRootFrom(Directory.GetCurrentDirectory());
        if (fromCurrentDirectory is not null)
            return fromCurrentDirectory;

        string? fromExecutable = FindRepositoryRootFrom(AppContext.BaseDirectory);
        return fromExecutable ?? throw new DirectoryNotFoundException(
            "Could not locate a repository containing csharp/src/SuperMetroid.Core.");
    }

    /// <summary>
    /// Audits one constructed source fragment. Verification uses this entry point to prove
    /// each domain classifier and the reviewed-definition exemption without touching disk.
    /// </summary>
    public static IReadOnlyList<MagicNumberFinding> AuditText(
        string relativePath,
        string source)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(relativePath);
        ArgumentNullException.ThrowIfNull(source);
        var findings = new List<MagicNumberFinding>();
        AuditLines(relativePath.Replace('\\', '/'), source.Split('\n'), findings);
        return findings;
    }

    /// <summary>Reads one source file and adds its non-exempt hexadecimal findings to the audit collection.</summary>
    /// <param name="repositoryRoot">Absolute repository root used to create a stable relative path.</param>
    /// <param name="path">Absolute source-file path to inspect.</param>
    /// <param name="findings">Destination collection receiving findings from the file.</param>
    private static void AuditFile(
        string repositoryRoot,
        string path,
        List<MagicNumberFinding> findings)
    {
        string relativePath = Path.GetRelativePath(repositoryRoot, path).Replace('\\', '/');
        AuditLines(relativePath, File.ReadLines(path), findings);
    }

    /// <summary>Walks upward from a directory until it finds the repository's core project tree.</summary>
    /// <param name="startingPath">Directory or executable path from which to begin the search.</param>
    /// <returns>The first matching repository root, or <see langword="null"/> when none is found.</returns>
    private static string? FindRepositoryRootFrom(string startingPath)
    {
        for (DirectoryInfo? directory = new(Path.GetFullPath(startingPath));
             directory is not null;
             directory = directory.Parent)
        {
            if (Directory.Exists(Path.Combine(
                    directory.FullName,
                    "csharp",
                    "src",
                    "SuperMetroid.Core")))
            {
                return directory.FullName;
            }
        }
        return null;
    }

    /// <summary>Classifies hexadecimal literals in source lines and records those outside approved definitions.</summary>
    /// <param name="relativePath">Repository-relative, slash-normalized path used in finding identity.</param>
    /// <param name="lines">Source lines to scan in order.</param>
    /// <param name="findings">Destination collection receiving classified findings.</param>
    private static void AuditLines(
        string relativePath,
        IEnumerable<string> lines,
        List<MagicNumberFinding> findings)
    {
        string fileName = Path.GetFileName(relativePath);
        // Generated partial catalogs have the same definition ownership as their
        // handwritten companion. Generated functional classes remain audited.
        if (fileName.EndsWith(".Generated.cs", StringComparison.OrdinalIgnoreCase))
            fileName = fileName[..^".Generated.cs".Length] + ".cs";
        if (ReviewedDefinitionSuffixes.Any(
                suffix => fileName.EndsWith(suffix, StringComparison.OrdinalIgnoreCase)))
        {
            return;
        }

        bool insideBlockComment = false;
        int lineNumber = 0;
        foreach (string originalLine in lines)
        {
            lineNumber++;
            string code = RemoveCommentsAndQuotedText(originalLine, ref insideBlockComment);
            if (string.IsNullOrWhiteSpace(code))
                continue;

            foreach (Match match in HexLiteralRegex().Matches(code))
            {
                string literal = match.Value;
                ulong value = ParseHexLiteral(literal);
                MagicNumberCategory? category = Classify(code, literal, value);
                if (category is null || HasReviewedInlineExemption(originalLine, category.Value))
                    continue;

                findings.Add(MagicNumberFinding.Create(
                    relativePath,
                    lineNumber,
                    literal,
                    category.Value,
                    originalLine.Trim()));
            }
        }
    }

    /// <summary>Assigns a domain category to a hexadecimal literal from its containing source expression.</summary>
    /// <param name="code">Comment-free and string-free source text from the literal's line.</param>
    /// <param name="literal">Matched hexadecimal token, including any numeric suffix.</param>
    /// <param name="value">Parsed unsigned value used for category range checks.</param>
    /// <returns>The detected domain, or <see langword="null"/> when the literal is outside this audit's scope.</returns>
    private static MagicNumberCategory? Classify(string code, string literal, ulong value)
    {
        string lower = code.ToLowerInvariant();

        // A hexadecimal switch arm in functional code is a domain discriminator until it
        // is represented by an enum or named catalog. This intentionally runs before the
        // ordinary zero/one exemption.
        if (RawCaseRegex().IsMatch(code))
            return MagicNumberCategory.DomainDiscriminator;

        if (value is >= 0x808000 and <= 0xffffff)
            return MagicNumberCategory.RomAddress;

        if (ContainsAny(lower, "collisiontype", "collision type") && value <= 0xffff)
            return MagicNumberCategory.CollisionType;

        if (ContainsAny(lower, ".bts", "behavior", "roomblockbehavior") && value <= 0xff)
            return MagicNumberCategory.BlockBehavior;

        if (ContainsAny(lower, "pose", "movementtype", "movement type", "movementhandler") &&
            value <= 0xffff)
        {
            return MagicNumberCategory.PoseOrMovement;
        }

        if (ContainsAny(lower, "projectiletype", "projectile type", "projectilefamily", "projectilekind") &&
            value <= 0xffff)
        {
            return MagicNumberCategory.ProjectileFamily;
        }

        if (ContainsAny(lower, "queuesound", "soundeffect", "queuemusic", "musictrack", "music track") &&
            value <= 0xffff)
        {
            return MagicNumberCategory.AudioId;
        }

        if (ContainsAny(lower, "controller", "input") &&
            ContainsAny(code, "&", "|", "^") && value <= 0xffff)
        {
            return MagicNumberCategory.ControllerBits;
        }

        if (ContainsAny(lower,
                "tilemap", "palettebits", "graphicsindex", "levelword", "oam", "obsel", "bgsc", "attribute") &&
            ContainsAny(code, "&", "|", "^", "<<", ">>") && value <= 0xffff)
        {
            return MagicNumberCategory.PackedPpuWord;
        }

        if (ContainsAny(lower, "sram", "saveram", "saveoffset", "slot offset", "slotoffset"))
            return MagicNumberCategory.SramOffset;

        if (ContainsAny(lower,
                "instructionpointer", "instruction pointer", "preinstruction", "callbackpointer",
                "callback pointer", "setupcode", "maincode") && value is >= 0x8000 and <= 0xffff)
        {
            return MagicNumberCategory.CallbackPointer;
        }

        return null;
    }

    /// <summary>Reports whether source text contains any classifier fragment using ordinal matching.</summary>
    /// <param name="value">Normalized source text being classified.</param>
    /// <param name="fragments">Domain terms whose presence indicates a candidate category.</param>
    /// <returns><see langword="true"/> when at least one fragment occurs in the text.</returns>
    private static bool ContainsAny(string value, params string[] fragments) =>
        fragments.Any(fragment => value.Contains(fragment, StringComparison.Ordinal));

    /// <summary>Recognizes an explicitly categorized same-line exemption with a non-empty reason marker.</summary>
    /// <param name="line">Original source line, including comments where the exemption is written.</param>
    /// <param name="category">Finding category that the exemption must name.</param>
    /// <returns><see langword="true"/> when the line contains the required category marker.</returns>
    private static bool HasReviewedInlineExemption(
        string line,
        MagicNumberCategory category)
    {
        // An exemption must name its category and include a reason after " - ". This keeps
        // the allowlist searchable and prevents a bare suppression token from accumulating.
        string marker = $"magic-number-audit: allow({category}) - ";
        return line.Contains(marker, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>Converts a matched hexadecimal token to its unsigned numeric value.</summary>
    /// <param name="literal">Token containing a hexadecimal prefix, digits, separators, and optional suffix.</param>
    /// <returns>The parsed base-sixteen value.</returns>
    private static ulong ParseHexLiteral(string literal)
    {
        string digits = literal[2..].TrimEnd('u', 'U', 'l', 'L').Replace("_", string.Empty);
        return ulong.Parse(digits, NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture);
    }

    /// <summary>Loads the compact fingerprint payload from the reviewed baseline file.</summary>
    /// <param name="path">Filesystem path to the baseline text file.</param>
    /// <returns>Case-normalized fingerprints, or an empty set when the payload line is absent.</returns>
    private static HashSet<string> ReadBaseline(string path)
    {
        if (!File.Exists(path))
            throw new FileNotFoundException("The reviewed magic-number baseline is missing.", path);

        string? encoded = File.ReadLines(path)
            .Select(line => line.Trim())
            .FirstOrDefault(line => line.StartsWith("fingerprints=", StringComparison.Ordinal));
        if (encoded is null)
            return [];

        byte[] bytes = Convert.FromBase64String(encoded["fingerprints=".Length..]);
        if (bytes.Length % 8 != 0)
            throw new InvalidDataException("Magic-number baseline fingerprint data is truncated.");
        var fingerprints = new HashSet<string>(StringComparer.Ordinal);
        for (int offset = 0; offset < bytes.Length; offset += 8)
            fingerprints.Add(Convert.ToHexString(bytes, offset, 8).ToLowerInvariant());
        return fingerprints;
    }

    /// <summary>Removes comments and quoted literals while carrying block-comment state to the next line.</summary>
    /// <param name="line">Single source line to reduce to executable code text.</param>
    /// <param name="insideBlockComment">Whether scanning enters or exits this line within a block comment.</param>
    /// <returns>Source text with comments and quoted contents replaced or removed.</returns>
    private static string RemoveCommentsAndQuotedText(string line, ref bool insideBlockComment)
    {
        var result = new StringBuilder(line.Length);
        for (int index = 0; index < line.Length; index++)
        {
            if (insideBlockComment)
            {
                int end = line.IndexOf("*/", index, StringComparison.Ordinal);
                if (end < 0)
                    return result.ToString();
                insideBlockComment = false;
                index = end + 1;
                continue;
            }

            if (index + 1 < line.Length && line[index] == '/' && line[index + 1] == '*')
            {
                insideBlockComment = true;
                index++;
                continue;
            }
            if (index + 1 < line.Length && line[index] == '/' && line[index + 1] == '/')
                break;

            if (line[index] is '\'' or '"')
            {
                char quote = line[index];
                result.Append(' ');
                for (index++; index < line.Length; index++)
                {
                    if (line[index] == '\\')
                    {
                        index++;
                        continue;
                    }
                    if (line[index] == quote)
                        break;
                }
                continue;
            }

            result.Append(line[index]);
        }
        return result.ToString();
    }
}

/// <summary>Domain that explains why a raw hexadecimal literal must be owned by a named definition.</summary>
internal enum MagicNumberCategory
{
    /// <summary>Mapped cartridge address or address range used as functional data.</summary>
    RomAddress,
    /// <summary>Native callback, instruction, or setup pointer used to select behavior.</summary>
    CallbackPointer,
    /// <summary>Raw switch discriminator that identifies a mutually exclusive domain value.</summary>
    DomainDiscriminator,
    /// <summary>Collision response code that belongs to the room collision domain.</summary>
    CollisionType,
    /// <summary>Block behavior or BTS value interpreted by room logic.</summary>
    BlockBehavior,
    /// <summary>Samus pose, movement type, or movement handler identity.</summary>
    PoseOrMovement,
    /// <summary>Projectile family or kind selector used by combat logic.</summary>
    ProjectileFamily,
    /// <summary>Sound effect or music identifier passed to audio dispatch.</summary>
    AudioId,
    /// <summary>Controller input bits combined or tested by input logic.</summary>
    ControllerBits,
    /// <summary>Packed PPU, tilemap, palette, or graphics word whose bit layout has domain meaning.</summary>
    PackedPpuWord,
    /// <summary>Battery-backed SRAM offset or slot-layout location.</summary>
    SramOffset,
}

/// <summary>A source occurrence of a hexadecimal value classified by the production audit.</summary>
/// <param name="RelativePath">Slash-normalized repository-relative path used to identify the source file.</param>
/// <param name="LineNumber">One-based source line where the literal appears.</param>
/// <param name="Literal">Original hexadecimal token, including its written casing and suffix.</param>
/// <param name="Category">Domain classification that determines the named container expected for the value.</param>
/// <param name="Source">Trimmed source line retained for diagnostics and fingerprint calculation.</param>
/// <param name="Signature">Full lowercase SHA-256 digest of the finding's stable identity input.</param>
internal sealed record MagicNumberFinding(
    string RelativePath,
    int LineNumber,
    string Literal,
    MagicNumberCategory Category,
    string Source,
    string Signature)
{
    /// <summary>Short stable baseline key derived from the leading digest bytes.</summary>
    public string BaselineFingerprint => Signature[..16];

    /// <summary>Creates a finding and computes its stable digest from path, category, token, and source line.</summary>
    /// <param name="relativePath">Slash-normalized repository-relative source path.</param>
    /// <param name="lineNumber">One-based line containing the finding.</param>
    /// <param name="literal">Matched hexadecimal token as written.</param>
    /// <param name="category">Domain classification assigned by the scanner.</param>
    /// <param name="source">Trimmed source line included in the identity digest and diagnostic.</param>
    /// <returns>A finding whose signature can be compared with the reviewed baseline.</returns>
    public static MagicNumberFinding Create(
        string relativePath,
        int lineNumber,
        string literal,
        MagicNumberCategory category,
        string source)
    {
        string fingerprintSource = $"{relativePath}|{category}|{literal.ToLowerInvariant()}|{source}";
        string signature = Convert.ToHexString(
            SHA256.HashData(Encoding.UTF8.GetBytes(fingerprintSource))).ToLowerInvariant();
        return new MagicNumberFinding(
            relativePath,
            lineNumber,
            literal,
            category,
            source,
            signature);
    }

    /// <summary>Human-readable finding text that names the expected definition owner and remediation.</summary>
    public string Diagnostic =>
        $"{RelativePath}:{LineNumber}: raw {Literal} [{Category}]. " +
        $"Expected {ExpectedContainer(Category)}. {Remediation(Category)} Source: {Source}";

    /// <summary>Names the typed catalog or domain owner appropriate for a finding category.</summary>
    /// <param name="category">Domain requiring a named definition.</param>
    /// <returns>Suggested owner name included in the audit diagnostic.</returns>
    private static string ExpectedContainer(MagicNumberCategory category) => category switch
    {
        MagicNumberCategory.RomAddress => "a named ROM address/range catalog",
        MagicNumberCategory.CallbackPointer => "a named callback/code-pointer catalog",
        MagicNumberCategory.DomainDiscriminator => "a domain enum or named value",
        MagicNumberCategory.CollisionType => "RoomCollisionType",
        MagicNumberCategory.BlockBehavior => "RoomBlockBehavior/RoomBlockBehaviorValues",
        MagicNumberCategory.PoseOrMovement => "a Samus pose or movement enum/catalog",
        MagicNumberCategory.ProjectileFamily => "a typed projectile family/kind",
        MagicNumberCategory.AudioId => "SoundEffectId or a named music command",
        MagicNumberCategory.ControllerBits => "SnesButtons/typed controller input",
        MagicNumberCategory.PackedPpuWord => "a packed SNES word wrapper",
        MagicNumberCategory.SramOffset => "SaveRamLayout or another SRAM layout container",
        _ => "a named domain definition",
    };

    /// <summary>Builds the migration guidance associated with a classified raw value.</summary>
    /// <param name="category">Finding domain used to select the appropriate remediation.</param>
    /// <returns>Text directing the author to a named owner or a reasoned inline exemption.</returns>
    private static string Remediation(MagicNumberCategory category) =>
        $"Move the value to the {ExpectedContainer(category)} and reference that symbol; " +
        $"if it is intrinsic algorithm data, add a reviewed same-line allow({category}) reason.";
}

/// <summary>Counts and finding sets produced by one production magic-number audit run.</summary>
/// <param name="CurrentFindingCount">Total classified findings discovered in scanned production sources.</param>
/// <param name="BaselineCount">Number of reviewed fingerprints loaded from the baseline.</param>
/// <param name="RetiredBaselineEntries">Baseline fingerprints no longer represented by a current finding.</param>
/// <param name="CurrentFindings">All current findings, including entries already covered by the baseline.</param>
/// <param name="NewFindings">Current findings whose fingerprints are absent from the reviewed baseline.</param>
internal sealed record MagicNumberAuditResult(
    int CurrentFindingCount,
    int BaselineCount,
    int RetiredBaselineEntries,
    IReadOnlyList<MagicNumberFinding> CurrentFindings,
    IReadOnlyList<MagicNumberFinding> NewFindings)
{
    /// <summary>Whether every current finding is already represented in the reviewed baseline.</summary>
    public bool Passed => NewFindings.Count == 0;
}
