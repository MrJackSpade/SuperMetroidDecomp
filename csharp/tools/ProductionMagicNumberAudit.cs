using SuperMetroid.Core;
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
    /// <summary>Repository-relative reviewed fingerprint set used to distinguish existing debt from new findings.</summary>
    private const string BaselineRelativePath = "csharp/magic-number-baseline.txt";

    /// <summary>Functional source trees checked by the offline audit.</summary>
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
        "PlmHeaderId.cs", // Exclusive native PLM header enum; its members are definitions.
        "RoomPlmInstruction.cs", // Exclusive native bank-$84 PLM instruction enum; its members are definitions.
        "SamusPoseId.cs", // Exclusive native pose enum; its members are definitions.
        "Tables.cs",
        "Values.cs",
        "Words.cs",
    ];

    /// <summary>Finds C# hexadecimal literals after comments and quoted text are stripped.</summary>
    [GeneratedRegex(@"0[xX][0-9a-fA-F_]+(?:[uUlL]{0,2})", RegexOptions.CultureInvariant)]
    private static partial Regex HexLiteralRegex();

    /// <summary>Recognizes a raw hexadecimal switch discriminator before small-value exemptions apply.</summary>
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
        Ensure.NotNull(source);
        var findings = new List<MagicNumberFinding>();
        AuditLines(relativePath.Replace('\\', '/'), source.Split('\n'), findings);
        return findings;
    }

    /// <summary>Reads a production file under its repository-relative identity for stable finding fingerprints.</summary>
    private static void AuditFile(
        string repositoryRoot,
        string path,
        List<MagicNumberFinding> findings)
    {
        string relativePath = Path.GetRelativePath(repositoryRoot, path).Replace('\\', '/');
        AuditLines(relativePath, File.ReadLines(path), findings);
    }

    /// <summary>Walks parent directories until the core source tree identifies the repository root.</summary>
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

    /// <summary>Skips reviewed definition owners, strips non-code text, and records classified hexadecimal uses.</summary>
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
                MagicNumberCategory? category = Classify(code, value);
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

    /// <summary>Assigns a domain to a raw value from its surrounding operation and numeric range.</summary>
    private static MagicNumberCategory? Classify(string code, ulong value)
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

    /// <summary>Checks whether a source expression contains any classifier cue using ordinal matching.</summary>
    private static bool ContainsAny(string value, params string[] fragments) =>
        fragments.Any(fragment => value.Contains(fragment, StringComparison.Ordinal));

    /// <summary>Accepts an inline exception only when it names the finding category and supplies a reason.</summary>
    private static bool HasReviewedInlineExemption(
        string line,
        MagicNumberCategory category)
    {
        // An exemption must name its category and include a reason after " - ". This keeps
        // the allowlist searchable and prevents a bare suppression token from accumulating.
        string marker = $"magic-number-audit: allow({category}) - ";
        return line.Contains(marker, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>Parses a C# hex token after removing digit separators and unsigned or long suffixes.</summary>
    private static ulong ParseHexLiteral(string literal)
    {
        string digits = literal[2..].TrimEnd('u', 'U', 'l', 'L').Replace("_", string.Empty);
        return ulong.Parse(digits, NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture);
    }

    /// <summary>Decodes the reviewed 64-bit fingerprint prefixes from the compact baseline payload.</summary>
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

    /// <summary>Retains only code on one line while carrying block-comment state across lines.</summary>
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

/// <summary>Domains whose raw hexadecimal values require a named owner or reviewed exemption.</summary>
internal enum MagicNumberCategory
{
    /// <summary>A cartridge bus address that belongs in an address or range catalog.</summary>
    RomAddress,
    /// <summary>A native callback or pre-instruction pointer embedded in functional code.</summary>
    CallbackPointer,
    /// <summary>A switch-arm value selecting a domain state without a named member.</summary>
    DomainDiscriminator,
    /// <summary>A room collision type that should use the collision domain.</summary>
    CollisionType,
    /// <summary>A block BTS behavior byte that should use a named room behavior value.</summary>
    BlockBehavior,
    /// <summary>A Samus pose or movement dispatch value outside its typed catalog.</summary>
    PoseOrMovement,
    /// <summary>A projectile family or kind selected by a raw value.</summary>
    ProjectileFamily,
    /// <summary>A sound or music command identifier used without its audio catalog.</summary>
    AudioId,
    /// <summary>A controller bit mask embedded in an input expression.</summary>
    ControllerBits,
    /// <summary>A bit-packed PPU or level word manipulated without a named wrapper.</summary>
    PackedPpuWord,
    /// <summary>An SRAM offset used outside its saved-memory layout owner.</summary>
    SramOffset,
}

/// <summary>One classified source occurrence with a stable signature independent of line movement.</summary>
internal sealed record MagicNumberFinding(
    string RelativePath,
    int LineNumber,
    string Literal,
    MagicNumberCategory Category,
    string Source,
    string Signature)
{
    /// <summary>First 64 SHA-256 bits used for membership in the compact reviewed baseline.</summary>
    public string BaselineFingerprint => Signature[..16];

    /// <summary>Hashes the source identity, category, literal, and expression to identify a reviewed occurrence.</summary>
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

    /// <summary>Actionable source location and expected named owner for this raw value.</summary>
    public string Diagnostic =>
        $"{RelativePath}:{LineNumber}: raw {Literal} [{Category}]. " +
        $"Expected {ExpectedContainer(Category)}. {Remediation(Category)} Source: {Source}";

    /// <summary>Names the catalog or type expected to own values in a classified domain.</summary>
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
        _ => throw new ArgumentOutOfRangeException(nameof(category), category, "Undefined magic-number category."),
    };

    /// <summary>Explains the named-owner and reviewed intrinsic-data paths for one category.</summary>
    private static string Remediation(MagicNumberCategory category) =>
        $"Move the value to the {ExpectedContainer(category)} and reference that symbol; " +
        $"if it is intrinsic algorithm data, add a reviewed same-line allow({category}) reason.";
}

/// <summary>Current audit occurrences, reviewed baseline size, and additions requiring remediation.</summary>
internal sealed record MagicNumberAuditResult(
    int CurrentFindingCount,
    int BaselineCount,
    int RetiredBaselineEntries,
    IReadOnlyList<MagicNumberFinding> CurrentFindings,
    IReadOnlyList<MagicNumberFinding> NewFindings)
{
    /// <summary>Whether the production scan introduced no findings outside the reviewed baseline.</summary>
    public bool Passed => NewFindings.Count == 0;
}
