using SuperMetroid.AssetExtraction.Rooms;

namespace SuperMetroid.AssetExtraction.Frontend.Intro.Artwork;

/// <summary>Editable subtitle-staging tile references for BG3 rows 24-27; glyph pixels, translated narration, and reveal timing are separate resources or runtime state.</summary>
public sealed record IntroFinalLineTilemapDocument
{
    /// <summary>Schema revision, which loading requires to equal <see cref="IntroFinalLineTilemapFormat.Version"/>.</summary>
    public required int Version { get; init; }
    /// <summary>Exactly 128 non-null cells in four rows of 32, including margins; character-sheet columns/rows are 0..31 and BG palette selectors are 0..7.</summary>
    public required RoomBackgroundTilemapCell[] Cells { get; init; }
}
