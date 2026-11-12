using SuperMetroid.AssetExtraction.Hud;
using SuperMetroid.Core;
using SuperMetroid.Core.Frontend.Map.Artwork;

namespace SuperMetroid.AssetExtraction.Frontend.Map;

/// <summary>
/// The characters the map screens, the pause interface and the HUD are pictured from (#1466): the import composes the
/// screen tile sheets, the HUD's images and the rooms' BG3 sheets from them. They are import data (#1636); the game never
/// reads them.
/// </summary>
/// <param name="Map">The map screens' characters.</param>
/// <param name="Hud">The HUD's characters.</param>
/// <param name="Pause">The pause interface's characters.</param>
public sealed record MapCharacterArtwork(MapTileCharacters Map, HudTileAtlas Hud, MapTileCharacters Pause)
{
    /// <summary>The characters the import's map presentation <paramref name="maps"/> was read with (#1636).</summary>
    /// <exception cref="InvalidOperationException">The presentation was not read for the import.</exception>
    public static MapCharacterArtwork Of(AreaMapPresentation maps) => ImportAreaMapPresentation.Imported(maps, "map characters").Characters;
}
