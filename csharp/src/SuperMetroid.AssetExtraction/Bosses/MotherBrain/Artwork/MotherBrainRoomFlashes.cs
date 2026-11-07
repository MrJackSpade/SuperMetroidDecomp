namespace SuperMetroid.AssetExtraction.Bosses.MotherBrain.Artwork;

/// <summary>The strength of each image of Mother Brain's room flash, which the import pre-colours her room's flash shades at (#1527).</summary>
internal static class MotherBrainRoomFlashes
{
    /// <summary>The strength, in thirds, of flash image <paramref name="frame"/>, zero through thirteen.</summary>
    internal static int FlashStrength(int frame) =>
        (uint)frame < (uint)MotherBrainRoomFlashPaintDefinitions.FlashStrengths.Length
            ? MotherBrainRoomFlashPaintDefinitions.FlashStrengths[frame]
            : throw new ArgumentOutOfRangeException(nameof(frame));
}
