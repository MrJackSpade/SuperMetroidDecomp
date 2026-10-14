using System.Security.Cryptography;
using System.Text;

namespace SuperMetroid.SourceAudit;

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
