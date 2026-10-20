namespace SuperMetroid.Core.Fx.PaletteFx;

/// <summary>Development-tool members of <see cref="NintendoLogoFadePaletteFxColorDefinitions"/>; never linked by player hosts.</summary>
internal static class NintendoLogoFadePaletteFxColorDefinitionsTooling
{
    /// <summary>Shared two-entry list backing the boot-logo and copyright fade catalog.</summary>
    internal static readonly IReadOnlyList<NintendoLogoFadePaletteFxColorDefinition> ReadOnlyDefinitions = [new(), new()];
    /// <summary>The boot-logo and copyright entries in native definition order, calculated from their semantic entry contracts.</summary>
    [AccessedByReflection]
    public static IReadOnlyList<NintendoLogoFadePaletteFxColorDefinition> All => ReadOnlyDefinitions;
}
