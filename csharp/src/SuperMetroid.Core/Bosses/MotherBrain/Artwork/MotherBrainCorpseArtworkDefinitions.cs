namespace SuperMetroid.Core.Bosses.MotherBrain.Artwork;

/// <summary>The rows of Mother Brain's row-by-row corpse decay.</summary>
/// <remarks>
/// The count stays in Core (#2471): her corpse's rot schedule drops each of its rows
/// (<see cref="MotherBrainCorpseRottingState.EntryCount"/>), as well as the import reading it.
/// </remarks>
public static class MotherBrainCorpseArtworkDefinitions
{
    /// <summary>The corpse's visible pixel rows; its rot schedule drops every one.</summary>
    public const int PixelRowCount = 48;
}
