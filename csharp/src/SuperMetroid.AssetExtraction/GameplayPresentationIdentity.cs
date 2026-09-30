using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.AssetExtraction;

/// <summary>Selected shared gameplay colors, common sprite pixels, and X-ray art.</summary>
public static class GameplayPresentationIdentity
{
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
