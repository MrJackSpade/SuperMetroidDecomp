using SuperMetroid.Core.Samus.Abilities.Artwork;
using SuperMetroid.AssetExtraction.Rooms.Artwork;

namespace SuperMetroid.AssetExtraction.Installation;

/// <summary>Selected common sprite pixels and X-ray art.</summary>
public static class GameplayPresentationIdentity
{
    /// <summary>Captures named identities for the selected common OBJ characters and X-ray presentation.</summary>
    /// <param name="commonSprites">Non-null selected standard-object planar character atlas.</param>
    /// <param name="xray">Non-null selected reveal-operand and item/special-room overlay catalog.</param>
    /// <returns>A new ordinal-keyed dictionary mapping the two installation domain names to decoded-content identity strings.</returns>
    /// <remarks>
    /// Reflects the supplied installed content without loading files or retaining catalogs; identities describe presentation
    /// content rather than engine revision, cartridge provenance, or active gameplay state. The gameplay base palette is a
    /// palette document only the import reads (#1544); the sheets it colours carry its identity.
    /// </remarks>
    public static IReadOnlyDictionary<string, string> Create(RoomCharacterAtlas commonSprites, InstalledXrayReveals xray) =>
        new Dictionary<string, string>(StringComparer.Ordinal)
        {
            [GameInstallationLayout.StandardObjectDirectoryName] = commonSprites.ContentIdentity,
            [GameInstallationLayout.XrayRevealVisualDirectoryName] = xray.ContentIdentity,
        };
}
