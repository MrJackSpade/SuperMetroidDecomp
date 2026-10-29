using SuperMetroid.AssetExtraction.Hud;
using SuperMetroid.Core;
using SuperMetroid.Core.Frontend.Map.Artwork;

namespace SuperMetroid.AssetExtraction.Frontend.Map;

/// <summary>
/// The installed characters the map screens, the pause interface and the HUD are pictured from (#1466): the import
/// composes the screen tile sheets, the HUD's images and the rooms' BG3 sheets from them. The game never reads them.
/// </summary>
/// <param name="Map">The map screens' characters.</param>
/// <param name="Hud">The HUD's characters.</param>
/// <param name="Pause">The pause interface's characters.</param>
public sealed record MapCharacterArtwork(MapTileCharacters Map, HudTileAtlas Hud, MapTileCharacters Pause)
{
    /// <summary>The characters <paramref name="maps"/> was read with.</summary>
    /// <exception cref="InvalidDataException">The presentation was not read from installed artwork.</exception>
    public static MapCharacterArtwork Of(AreaMapPresentation maps) => (Ensure.NotNull(maps) as InstalledAreaMapPresentation)?.Characters
        ?? throw new InvalidDataException("The map presentation was not read from installed artwork.");
}
