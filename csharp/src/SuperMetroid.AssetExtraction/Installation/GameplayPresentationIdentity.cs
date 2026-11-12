using SuperMetroid.Core.Samus.Abilities.Artwork;

namespace SuperMetroid.AssetExtraction.Installation;

/// <summary>Selected X-ray art.</summary>
public static class GameplayPresentationIdentity
{
    /// <summary>Captures the named identity of the selected X-ray presentation.</summary>
    /// <param name="xray">Non-null selected reveal-operand and item/special-room overlay catalog.</param>
    /// <returns>A new ordinal-keyed dictionary mapping the X-ray installation domain name to its decoded-content identity string.</returns>
    /// <remarks>
    /// Reflects the supplied installed content without loading files or retaining catalogs; identities describe presentation
    /// content rather than engine revision, cartridge provenance, or active gameplay state. The gameplay base palette is a
    /// palette document only the import reads (#1544), and the standard OBJ characters are import data the installation
    /// never holds (#1636); the sheets composed from them carry their identity.
    /// </remarks>
    public static IReadOnlyDictionary<string, string> Create(InstalledXrayReveals xray) =>
        new Dictionary<string, string>(StringComparer.Ordinal)
        {
            [GameInstallationLayout.XrayRevealVisualDirectoryName] = xray.ContentIdentity,
        };
}
