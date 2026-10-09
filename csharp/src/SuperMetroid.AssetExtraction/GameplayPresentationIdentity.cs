using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.AssetExtraction;

/// <summary>Selected shared gameplay colors, common sprite pixels, and X-ray art.</summary>
public static class GameplayPresentationIdentity
{
    /// <summary>Captures named identities for the selected shared gameplay colors, common OBJ characters, and X-ray presentation.</summary>
    /// <param name="palettes">Non-null selected gameplay base-palette catalog, not runtime animated CGRAM state.</param>
    /// <param name="commonSprites">Non-null selected standard-object planar character atlas.</param>
    /// <param name="xray">Non-null selected reveal-operand and item/special-room overlay catalog.</param>
    /// <returns>A new ordinal-keyed dictionary mapping the three installation domain names to decoded-content identity strings.</returns>
    /// <remarks>Reflects already selected overrides without loading files or retaining catalogs; identities describe presentation content rather than engine revision, cartridge provenance, or active gameplay state.</remarks>
    public static IReadOnlyDictionary<string, string> Create(
        GameplayBasePaletteCatalog palettes, RoomCharacterAtlas commonSprites, XrayRevealVisualCatalog xray) =>
        new Dictionary<string, string>(StringComparer.Ordinal)
        {
            [GameInstallationLayout.GameplayBasePaletteDirectoryName] = palettes.ContentIdentity,
            [GameInstallationLayout.StandardObjectDirectoryName] = commonSprites.ContentIdentity,
            [GameInstallationLayout.XrayRevealVisualDirectoryName] = xray.ContentIdentity,
        };

    /// <summary>Loads the same selected domains for installed diagnostic-artifact import.</summary>
    public static IReadOnlyDictionary<string, string> Load(GameInstallation installation)
    {
        ArgumentNullException.ThrowIfNull(installation);
        return Create(installation.LoadGameplayBasePalettes(), installation.LoadStandardObjects(),
            installation.LoadXrayRevealVisuals());
    }
}
