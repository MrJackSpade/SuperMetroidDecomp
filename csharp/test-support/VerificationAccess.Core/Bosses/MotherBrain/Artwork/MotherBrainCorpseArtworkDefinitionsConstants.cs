namespace SuperMetroid.Core.Bosses.MotherBrain.Artwork;

/// <summary>Cartridge values and helpers of <see cref="MotherBrainCorpseArtworkDefinitions"/> that only verification reads.</summary>
internal static class MotherBrainCorpseArtworkDefinitionsConstants
{
    /// <summary>$A9:E262 has eight row offsets, including two beyond the visible six rows.</summary>
    public static int TileRowOffset(int row) => (uint)row < 8
        ? row * MotherBrainCorpseArtworkDefinitions.RowBytes : throw new ArgumentOutOfRangeException(nameof(row));
}
