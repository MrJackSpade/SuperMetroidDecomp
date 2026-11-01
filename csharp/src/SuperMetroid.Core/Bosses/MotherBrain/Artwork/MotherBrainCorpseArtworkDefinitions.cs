namespace SuperMetroid.Core.Bosses.MotherBrain.Artwork;

/// <summary>The rows of Mother Brain's row-by-row corpse decay.</summary>
public static class MotherBrainCorpseArtworkDefinitions
{
    /// <summary>The corpse copy extracts six tile rows of the right-hand corpse frame.</summary>
    public const int RowCount = 6;
    /// <summary>The corpse's visible pixel rows, eight to each tile row; its rot schedule drops every one.</summary>
    public const int PixelRowCount = RowCount * 8;
}
