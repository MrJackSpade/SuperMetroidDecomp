using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.Intro.Artwork;
namespace SuperMetroid.Core.Frontend.Intro;

/// <summary>
/// The intro tilemaps that change as it runs (#1358): the Samus portrait BG2 tilemap, whose eye rectangle the object
/// system redraws, and the narration BG3 tilemap the staging map is uploaded into. The capture places them.
/// </summary>
internal sealed class IntroTilemaps
{
    /// <summary>The four scene pages' tiles, page after page.</summary>
    private readonly BackgroundTile[] scenePages = new BackgroundTile[IntroCinematicArtworkFormat.BackgroundPageCount * ScreenGeometry.TilemapScreenCellCount];

    /// <summary>The portrait tilemap's tiles.</summary>
    private readonly BackgroundTile[] portrait = new BackgroundTile[ScreenGeometry.TilemapScreenCellCount];

    /// <summary>The narration tilemap's tiles.</summary>
    private readonly BackgroundTile[] narration = new BackgroundTile[IntroCinematicDefinitions.Tilemaps.TextTilemapWordCount];

    /// <summary>The portrait tilemap.</summary>
    internal Span<BackgroundTile> Portrait => portrait;

    /// <summary>The narration tilemap.</summary>
    internal Span<BackgroundTile> Narration => narration;

    /// <summary>The tilemap of one scene page.</summary>
    internal ReadOnlySpan<BackgroundTile> ScenePage(IntroScenePage page) =>
        scenePages.AsSpan(PageSlot(page) * ScreenGeometry.TilemapScreenCellCount, ScreenGeometry.TilemapScreenCellCount);

    /// <summary>The installed scene pages in the order they are loaded.</summary>
    private static readonly IntroScenePage[] InstalledPageOrder = Enum.GetValues<IntroScenePage>();

    /// <summary>The position of <paramref name="page"/> among the installed scene pages.</summary>
    private static int PageSlot(IntroScenePage page)
    {
        int slot = Array.IndexOf(InstalledPageOrder, page);
        return slot >= 0 ? slot : throw new ArgumentOutOfRangeException(nameof(page), page, "Undefined intro scene page.");
    }

    /// <summary>Loads the installed scene pages.</summary>
    internal void LoadScenePages(ReadOnlySpan<BackgroundTile> tiles) => tiles[..scenePages.Length].CopyTo(scenePages);

    /// <summary>Loads the installed portrait image.</summary>
    internal void LoadPortrait(ReadOnlySpan<BackgroundTile> tiles) =>
        tiles[..IntroCinematicDefinitions.Artwork.SamusHeadTilemapCells].CopyTo(portrait);

    /// <summary>Loads the installed first narration card.</summary>
    internal void LoadNarration(ReadOnlySpan<BackgroundTile> tiles) =>
        tiles[..IntroCinematicDefinitions.Artwork.NarrationTilemapCells].CopyTo(narration);
}
