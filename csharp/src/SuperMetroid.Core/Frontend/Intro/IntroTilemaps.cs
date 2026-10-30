using SuperMetroid.Core.Assets;
namespace SuperMetroid.Core.Frontend.Intro;

/// <summary>
/// The intro tilemap that changes as it runs (#1358): the narration BG3 tilemap the staging map is uploaded into. The
/// scene pages and the portrait are installed pictures (#1467).
/// </summary>
internal sealed class IntroTilemaps
{
    /// <summary>The narration tilemap's tiles.</summary>
    private readonly BackgroundTile[] narration = new BackgroundTile[IntroCinematicDefinitions.Tilemaps.TextTilemapWordCount];

    /// <summary>The narration tilemap.</summary>
    internal Span<BackgroundTile> Narration => narration;

    /// <summary>Loads the installed first narration card.</summary>
    internal void LoadNarration(ReadOnlySpan<BackgroundTile> tiles) =>
        tiles[..IntroCinematicDefinitions.Artwork.NarrationTilemapCells].CopyTo(narration);
}
