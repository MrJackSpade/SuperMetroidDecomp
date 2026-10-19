using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms.Backgrounds;

namespace SuperMetroid.Core.Samus.Abilities;

/// <summary>BG1 tile sources for X-ray reveal tests that build a BG1 tilemap by hand (#1351).</summary>
internal static class XrayBg1Fixtures
{
    /// <summary>The 64x32 BG1 tilemap written at its gameplay word in <paramref name="vram"/>.</summary>
    internal static IBgTileSource Tilemap(SnesVram vram) =>
        new VramBgTilemap(vram, GameplayVramLayout.Bg1TilemapWord, new Extent16<TilemapCells>(64, 32));
}
