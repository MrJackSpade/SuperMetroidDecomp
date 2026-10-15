using Microsoft.CodeAnalysis;

namespace SuperMetroid.ReachabilityAudit.Removal;

/// <summary>
/// How relocated members are packaged in the target: the class holding moved static members, the
/// class holding moved instance members as <c>extension(Owner self)</c> blocks, their documentation,
/// and whether target paths keep the source project's directory name.
/// </summary>
/// <param name="MembersWord">Names the static-member class: <c>&lt;Owner&gt;</c> plus this word, placed before a definition-catalog suffix when <paramref name="BeforeCatalogSuffix"/>.</param>
/// <param name="ExtensionsWord">Names the instance-extension class: <c>&lt;Owner&gt;</c> plus this word.</param>
/// <param name="BeforeCatalogSuffix">Whether the members word goes before a catalog suffix so the class still reads as a definition catalog.</param>
/// <param name="KeepsProjectDirectory">Whether target paths keep the source project's directory name.</param>
/// <param name="MembersSummary">Summary of a new static-member class; <c>{0}</c> is the owner name.</param>
/// <param name="ExtensionsSummary">Summary of a new instance-extension class; <c>{0}</c> is the owner name.</param>
/// <param name="ExtensionBlockSummary">Summary of a new receiver block; <c>{0}</c> is the owner name.</param>
internal sealed record RelocationStyle(string MembersWord, string ExtensionsWord, bool BeforeCatalogSuffix, bool KeepsProjectDirectory,
    string MembersSummary, string ExtensionsSummary, string ExtensionBlockSummary)
{
    /// <summary>File-name suffixes the cartridge-encoding analyzer reads as definition catalogs.</summary>
    private static readonly string[] CatalogSuffixes =
        ["Addresses", "Catalog", "CodePointers", "Codes", "Constants", "Definitions", "Definition", "Flags", "InstructionLists",
         "Layout", "Masks", "Pointers", "RomData", "Tables", "Values", "Words"];

    /// <summary>Development-only project adapters for shipped code only tools reach.</summary>
    public static RelocationStyle Tooling { get; } = new("Tooling", "ToolingExtensions", true, true,
        "Development-tool members of <see cref=\"{0}\"/>; never linked by player hosts.",
        "Development-tool instance members of <see cref=\"{0}\"/>.",
        "Development-tool instance members of <see cref=\"{0}\"/>.");

    /// <summary>
    /// Test-support classes for shipped code only verification uses, following its convention:
    /// cartridge values in <c>&lt;Owner&gt;Constants</c>, instance access in <c>&lt;Owner&gt;Access</c>.
    /// </summary>
    public static RelocationStyle VerificationAccess { get; } = new("Constants", "Access", false, false,
        "Cartridge values and helpers of <see cref=\"{0}\"/> that only verification reads.",
        "Verification access to <see cref=\"{0}\"/> members production does not use.",
        "Exposes <see cref=\"{0}\"/> state that only verification reads.");

    /// <summary>The class holding an owner's moved static members and nested types.</summary>
    public string AdapterName(INamedTypeSymbol owner)
    {
        string flat = FlatName(owner);
        if (BeforeCatalogSuffix && CatalogSuffixes.FirstOrDefault(s => flat.EndsWith(s, StringComparison.Ordinal) && flat.Length > s.Length) is { } suffix)
            return flat[..^suffix.Length] + MembersWord + suffix;
        return flat + MembersWord;
    }

    /// <summary>The class holding an owner's moved instance members.</summary>
    public string ExtensionsName(INamedTypeSymbol owner) => FlatName(owner) + ExtensionsWord;

    /// <summary>Concatenates containing type names for a unique adapter identifier.</summary>
    private static string FlatName(INamedTypeSymbol type) =>
        type.ContainingType is { } outer ? FlatName(outer) + type.Name : type.Name;
}
