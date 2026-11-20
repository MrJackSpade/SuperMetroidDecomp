using SuperMetroid.AssetExtraction.Areas.Escape;
using SuperMetroid.AssetExtraction.Fx;
using SuperMetroid.AssetExtraction.Frontend.Map.Artwork;
using SuperMetroid.AssetExtraction.Frontend.Ending;
using SuperMetroid.AssetExtraction.Frontend.Intro;
using SuperMetroid.AssetExtraction.Frontend.Title;

namespace SuperMetroid.AssetExtraction.Frontend.Map;

/// <summary>
/// The map presentation's characters and the compositions read with them, from the component's import data (#1636): the
/// import composes the map, menu, pause, HUD, title, narration, ending and room pictures from them. The installation never
/// holds them, and the game reads none.
/// </summary>
/// <param name="Characters">The characters the map screens, the pause interface and the HUD are pictured from.</param>
/// <param name="Sprites">The map and menu sprite compositions, pictured into the map sprite sheet and the menu sheets.</param>
/// <param name="EscapeTimerTiles">The escape timer's characters, which the escape encounters' sheets are composed from (#1513).</param>
/// <param name="BlockAnimationAtlas">The block animations' characters, which room sheets and the Tourian statues' pictures are composed from (#1527).</param>
/// <param name="WorldCharacters">The world map's characters, which the world-map and menu picture sheets are composed from (#1583).</param>
/// <param name="IntroFont">The opening narration's font, which the narration pictures are composed from (#1583).</param>
/// <param name="EndingFont">The ending's font, which the credits, post-credits and explosion pictures are composed from (#1583).</param>
/// <param name="TitleGraphics">The title's characters and compositions, which the title and title floor sheets are composed from (#1583).</param>
internal sealed record ImportPresentationCharacters(
    MapCharacterArtwork Characters,
    MapSpriteCompositions Sprites,
    EscapeTimerCharacters EscapeTimerTiles,
    RoomFxBlockAnimationAtlas BlockAnimationAtlas,
    WorldMapCharacters WorldCharacters,
    IntroFontCharacters IntroFont,
    EndingFontCharacters EndingFont,
    TitleGraphicsCharacters TitleGraphics);
