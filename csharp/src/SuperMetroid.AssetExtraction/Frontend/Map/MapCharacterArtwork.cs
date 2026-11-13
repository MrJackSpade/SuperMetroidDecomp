using SuperMetroid.AssetExtraction.Hud;

namespace SuperMetroid.AssetExtraction.Frontend.Map;

/// <summary>
/// The characters the map screens, the pause interface and the HUD are pictured from (#1466): the import composes the
/// screen tile sheets, the HUD's images and the rooms' BG3 sheets from them. They are import data (#1636); the game never
/// reads them.
/// </summary>
/// <param name="Map">The map screens' characters.</param>
/// <param name="Hud">The HUD's characters.</param>
/// <param name="Pause">The pause interface's characters.</param>
public sealed record MapCharacterArtwork(MapTileCharacters Map, HudTileAtlas Hud, MapTileCharacters Pause);
