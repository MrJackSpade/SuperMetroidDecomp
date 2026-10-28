namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>The strength of each image of Mother Brain's room flash performance.</summary>
internal static class MotherBrainRoomFlashes
{
    /// <summary>The strength, in thirds, of flash image <paramref name="frame"/>, zero through thirteen.</summary>
    internal static int FlashStrength(int frame) =>
        (uint)frame < (uint)MotherBrainRoomPaletteProgramDefinitions.FlashStrengths.Length
            ? MotherBrainRoomPaletteProgramDefinitions.FlashStrengths[frame]
            : throw new ArgumentOutOfRangeException(nameof(frame));
}
