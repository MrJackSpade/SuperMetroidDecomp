using SuperMetroid.Core;

namespace SuperMetroid.AssetExtraction.Frontend.Pause.Artwork;

/// <summary>
/// The installed key of each <see cref="PauseAtlas"/> (#2520), written out so renaming a member never changes a name
/// the installation, an installed document or a save spells it by.
/// </summary>
internal static class PauseAtlasKeys
{
    /// <summary>Each PauseAtlas member's installed key.</summary>
    internal static InstalledKeys<PauseAtlas> Installed { get; } = new(new Dictionary<PauseAtlas, string>
    {
        [PauseAtlas.Map] = "Map",
        [PauseAtlas.Interface] = "Interface",
    });
}
