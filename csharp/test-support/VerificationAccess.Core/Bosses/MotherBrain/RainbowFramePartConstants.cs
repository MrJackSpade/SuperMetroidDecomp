namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>Cartridge values and helpers of <see cref="RainbowFramePart"/> that only verification reads.</summary>
internal static class RainbowFramePartConstants
{
    /// <summary>The part of <paramref name="parts"/> of a frame's 256 parts, 0 through 255.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The part is a whole frame or more, or below zero.</exception>
    public static RainbowFramePart Of(int parts) => new(parts);
}
